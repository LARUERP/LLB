using System;
using System.Collections.Generic;
using System.Threading;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Metadata;
using LabRP.Network;
using MelonLoader;
using UnityEngine;

namespace LabRP.Economy;

public static class ServerEconomy
{
	private struct RequestWindow
	{
		public float Start;

		public int Count;

		public bool Warned;
	}

	private static Dictionary<ulong, PlayerRecord> _records = new Dictionary<ulong, PlayerRecord>();

	private static bool _hosting = false;

	private static bool _dirty = false;

	private static float _salaryTimer = 0f;

	private static float _autosaveTimer = 0f;

	private const float AutosaveIntervalSeconds = 60f;

	private const int MaxRequestsPerWindow = 8;

	private const float RequestWindowSeconds = 5f;

	private static readonly Dictionary<byte, RequestWindow> _requestWindows = new Dictionary<byte, RequestWindow>();

	public static bool IsHosting => _hosting;

	public static void OnServerStarted()
	{
		if (_dirty && _records.Count > 0 && EconomySave.Save(_records))
		{
			_dirty = false;
		}
		_records = EconomySave.Load();
		_hosting = true;
		_dirty = false;
		_salaryTimer = 0f;
		_autosaveTimer = 0f;
		_requestWindows.Clear();
		PlayerID localID = PlayerIDManager.LocalID;
		if (localID != null)
		{
			EnsurePlayer(localID);
		}
		ClientEconomy.ApplySnapshot(BuildSnapshot());
		MelonLogger.Msg("[LabRP] Economy started. Balances will be saved on this machine.");
	}

	public static void OnPlayerJoined(PlayerID playerID)
	{
		if (_hosting && playerID != null)
		{
			EnsurePlayer(playerID);
			BroadcastSnapshot();
		}
	}

	public static void OnPlayerLeft(PlayerID playerID)
	{
		if (_hosting && playerID != null)
		{
			TouchRecord(playerID);
			_requestWindows.Remove(playerID.SmallID);
			Save();
			BroadcastSnapshot();
		}
	}

	public static void OnUsernameChanged(PlayerID playerID)
	{
		if (_hosting && playerID != null)
		{
			TouchRecord(playerID);
		}
	}

	public static void OnDisconnected()
	{
		if (_hosting)
		{
			SaveWithRetries();
			MelonLogger.Msg("[LabRP] Economy saved and stopped.");
		}
		_hosting = false;
		_salaryTimer = 0f;
		_requestWindows.Clear();
	}

	public static void OnUpdate()
	{
		if (!_hosting || !NetworkInfo.HasServer)
		{
			return;
		}
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		float num = LabRPConfig.SalaryIntervalMinutes.Value;
		if (float.IsNaN(num) || float.IsInfinity(num))
		{
			num = 5f;
		}
		float num2 = Mathf.Max(0.25f, num) * 60f;
		_salaryTimer = Mathf.Min(_salaryTimer + unscaledDeltaTime, num2);
		if (_salaryTimer >= num2)
		{
			_salaryTimer = 0f;
			PayEveryoneSalary();
		}
		_autosaveTimer += unscaledDeltaTime;
		if (_autosaveTimer >= 60f)
		{
			_autosaveTimer = 0f;
			if (_dirty)
			{
				StampOnlinePlayers();
				Save();
			}
		}
	}

	public static void SaveIfHosting()
	{
		if (_hosting)
		{
			StampOnlinePlayers();
			SaveWithRetries();
		}
	}

	public static void PayEveryoneSalary()
	{
		if (!_hosting)
		{
			return;
		}
		long num = Math.Clamp(LabRPConfig.SalaryAmount.Value, 0L, 999999999999L);
		if (num <= 0)
		{
			return;
		}
		foreach (PlayerID playerID in PlayerIDManager.PlayerIDs)
		{
			if (playerID != null && playerID.IsValid)
			{
				ChangeBalance(playerID.PlatformID, num, BalanceReason.Paycheck, "Paycheck");
			}
		}
	}

	public static void TryTransfer(byte senderSmallID, ulong targetPlatformID, long amount)
	{
		if (!_hosting)
		{
			return;
		}
		PlayerID playerID = PlayerIDManager.GetPlayerID(senderSmallID);
		if (playerID == null || !CheckRequestBudget(senderSmallID))
		{
			return;
		}
		PlayerID playerID2 = PlayerIDManager.GetPlayerID(targetPlatformID);
		if (amount <= 0)
		{
			Deny(playerID, "Invalid amount.");
			return;
		}
		if (playerID2 == null || !playerID2.IsValid)
		{
			Deny(playerID, "That player is no longer in the lobby.");
			return;
		}
		if (playerID2.PlatformID == playerID.PlatformID)
		{
			Deny(playerID, "You can't pay yourself.");
			return;
		}
		PlayerRecord playerRecord = EnsurePlayer(playerID);
		PlayerRecord playerRecord2 = EnsurePlayer(playerID2);
		if (playerRecord.Balance < amount)
		{
			Deny(playerID, "Not enough money.");
			return;
		}
		long num = 999999999999L - playerRecord2.Balance;
		if (amount > num)
		{
			Deny(playerID, "They can't hold that much money.");
			return;
		}
		string text = ResolveName(playerID);
		string text2 = ResolveName(playerID2);
		ChangeBalance(playerID.PlatformID, -amount, BalanceReason.PaymentSent, text2);
		ChangeBalance(playerID2.PlatformID, amount, BalanceReason.PaymentReceived, text);
		MelonLogger.Msg($"[LabRP] {text} paid {text2} ${amount:N0}.");
	}

	public static void AdminGive(ulong platformID, long amount)
	{
		if (_hosting && amount > 0 && IsOnline(platformID))
		{
			ChangeBalance(platformID, amount, BalanceReason.Admin, "Host grant");
		}
	}

	public static void AdminTake(ulong platformID, long amount)
	{
		if (_hosting && amount > 0 && IsOnline(platformID))
		{
			ChangeBalance(platformID, -amount, BalanceReason.Admin, "Host deduction");
		}
	}

	public static void AdminSet(ulong platformID, long value)
	{
		if (_hosting && IsOnline(platformID))
		{
			PlayerRecord orCreateRecord = GetOrCreateRecord(platformID);
			long num = LabRPConfig.ClampBalance(value);
			long delta = num - orCreateRecord.Balance;
			orCreateRecord.Balance = num;
			MarkDirty();
			BroadcastUpdate(platformID, orCreateRecord.Balance, delta, BalanceReason.Admin, "Host set");
		}
	}

	public static void ResetAllBalances()
	{
		if (!_hosting)
		{
			return;
		}
		_records.Clear();
		foreach (PlayerID playerID in PlayerIDManager.PlayerIDs)
		{
			if (playerID != null && playerID.IsValid)
			{
				EnsurePlayer(playerID);
			}
		}
		MarkDirty();
		Save();
		BroadcastSnapshot();
		ClientEconomy.ApplySnapshot(BuildSnapshot());
		MelonLogger.Msg("[LabRP] All balances were reset by the host.");
	}

	public static long GetBalance(ulong platformID)
	{
		if (!_records.TryGetValue(platformID, out var value))
		{
			return 0L;
		}
		return value.Balance;
	}

	public static bool IsOnline(ulong platformID)
	{
		PlayerID playerID = PlayerIDManager.GetPlayerID(platformID);
		if (playerID != null)
		{
			return playerID.IsValid;
		}
		return false;
	}

	private static bool CheckRequestBudget(byte senderSmallID)
	{
		float unscaledTime = Time.unscaledTime;
		if (!_requestWindows.TryGetValue(senderSmallID, out var value) || unscaledTime - value.Start > 5f)
		{
			value = new RequestWindow
			{
				Start = unscaledTime,
				Count = 0,
				Warned = false
			};
		}
		value.Count++;
		bool num = value.Count <= 8;
		if (!num && !value.Warned)
		{
			value.Warned = true;
			MelonLogger.Warning($"[LabRP] Dropping flood of pay requests from player {senderSmallID}.");
		}
		_requestWindows[senderSmallID] = value;
		return num;
	}

	private static void ChangeBalance(ulong platformID, long amount, BalanceReason reason, string detail)
	{
		PlayerRecord orCreateRecord = GetOrCreateRecord(platformID);
		long balance = orCreateRecord.Balance;
		orCreateRecord.Balance = LabRPConfig.ClampBalance(SaturatingAdd(balance, amount));
		long delta = orCreateRecord.Balance - balance;
		MarkDirty();
		BroadcastUpdate(platformID, orCreateRecord.Balance, delta, reason, detail);
	}

	private static long SaturatingAdd(long a, long b)
	{
		long num = a + b;
		if (b > 0 && num < a)
		{
			return long.MaxValue;
		}
		if (b < 0 && num > a)
		{
			return long.MinValue;
		}
		return num;
	}

	private static void Deny(PlayerID payer, string message)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		MessageRelay.RelayModule<BalanceUpdateMessage, BalanceUpdateData>(new BalanceUpdateData
		{
			PlatformID = payer.PlatformID,
			NewBalance = GetBalance(payer.PlatformID),
			Delta = 0L,
			Reason = BalanceReason.Denied,
			Detail = message
		}, new MessageRoute(payer.SmallID, (NetworkChannel)0));
	}

	private static void BroadcastUpdate(ulong platformID, long newBalance, long delta, BalanceReason reason, string detail)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		MessageRelay.RelayModule<BalanceUpdateMessage, BalanceUpdateData>(new BalanceUpdateData
		{
			PlatformID = platformID,
			NewBalance = newBalance,
			Delta = delta,
			Reason = reason,
			Detail = (detail ?? "")
		}, new MessageRoute((RelayType)2, (NetworkChannel)0));
	}

	private static void BroadcastSnapshot()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		MessageRelay.RelayModule<EconomySnapshotMessage, EconomySnapshotData>(BuildSnapshot(), new MessageRoute((RelayType)2, (NetworkChannel)0));
	}

	private static EconomySnapshotData BuildSnapshot()
	{
		EconomySnapshotData economySnapshotData = new EconomySnapshotData();
		foreach (PlayerID playerID in PlayerIDManager.PlayerIDs)
		{
			if (playerID != null && playerID.IsValid)
			{
				PlayerRecord orCreateRecord = GetOrCreateRecord(playerID.PlatformID);
				economySnapshotData.Entries.Add(new SnapshotEntry
				{
					PlatformID = playerID.PlatformID,
					Name = ResolveName(playerID),
					Balance = orCreateRecord.Balance
				});
			}
		}
		return economySnapshotData;
	}

	private static PlayerRecord EnsurePlayer(PlayerID playerID)
	{
		PlayerRecord orCreateRecord = GetOrCreateRecord(playerID.PlatformID);
		TouchRecord(playerID);
		return orCreateRecord;
	}

	private static PlayerRecord GetOrCreateRecord(ulong platformID)
	{
		if (!_records.TryGetValue(platformID, out var value))
		{
			value = new PlayerRecord
			{
				Name = "Unknown",
				Balance = LabRPConfig.ClampBalance(LabRPConfig.StartingBalance.Value),
				FirstSeenUtc = DateTime.UtcNow.ToString("o"),
				LastSeenUtc = DateTime.UtcNow.ToString("o")
			};
			_records[platformID] = value;
			MarkDirty();
		}
		return value;
	}

	private static void TouchRecord(PlayerID playerID)
	{
		if (playerID != null && _records.TryGetValue(playerID.PlatformID, out var value))
		{
			PlayerMetadata metadata = playerID.Metadata;
			object name;
			if (metadata == null)
			{
				name = null;
			}
			else
			{
				MetadataVariable username = metadata.Username;
				name = ((username != null) ? username.GetValue() : null);
			}
			string text = NameUtil.Clean((string)name);
			if (!string.IsNullOrWhiteSpace(text) && text != value.Name)
			{
				value.Name = text;
				MarkDirty();
			}
			value.LastSeenUtc = DateTime.UtcNow.ToString("o");
		}
	}

	private static void StampOnlinePlayers()
	{
		foreach (PlayerID playerID in PlayerIDManager.PlayerIDs)
		{
			if (playerID != null && playerID.IsValid)
			{
				TouchRecord(playerID);
			}
		}
	}

	public static string ResolveName(PlayerID playerID)
	{
		if (playerID == null)
		{
			return "Unknown";
		}
		PlayerMetadata metadata = playerID.Metadata;
		object name;
		if (metadata == null)
		{
			name = null;
		}
		else
		{
			MetadataVariable username = metadata.Username;
			name = ((username != null) ? username.GetValue() : null);
		}
		string text = NameUtil.Clean((string)name);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		if (!_records.TryGetValue(playerID.PlatformID, out var value) || string.IsNullOrWhiteSpace(value.Name) || !(value.Name != "Unknown"))
		{
			return $"Player {playerID.SmallID}";
		}
		return value.Name;
	}

	private static void MarkDirty()
	{
		_dirty = true;
	}

	private static void Save()
	{
		if (EconomySave.Save(_records))
		{
			_dirty = false;
		}
	}

	private static void SaveWithRetries()
	{
		for (int i = 0; i < 3; i++)
		{
			if (EconomySave.Save(_records))
			{
				_dirty = false;
				return;
			}
			Thread.Sleep(150);
		}
		MelonLogger.Error("[LabRP] Could not save the economy after multiple attempts. Changes since the last successful save may be lost.");
	}
}
