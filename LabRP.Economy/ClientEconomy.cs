using System;
using System.Collections.Generic;
using System.Linq;
using LabFusion.Player;
using LabFusion.SDK.Metadata;
using LabRP.Network;
using MelonLoader;

namespace LabRP.Economy;

public static class ClientEconomy
{
	public class CachedPlayer
	{
		public string Name = "";

		public long Balance;
	}

	private static readonly Dictionary<ulong, CachedPlayer> _cache = new Dictionary<ulong, CachedPlayer>();

	public static long LocalBalance
	{
		get
		{
			ulong localPlatformID = PlayerIDManager.LocalPlatformID;
			if (!_cache.TryGetValue(localPlatformID, out var value))
			{
				return 0L;
			}
			return value.Balance;
		}
	}

	public static event Action OnCacheChanged;

	public static event Action<long, long, BalanceReason, string> OnLocalChanged;

	public static void ApplySnapshot(EconomySnapshotData data)
	{
		_cache.Clear();
		foreach (SnapshotEntry entry in data.Entries)
		{
			_cache[entry.PlatformID] = new CachedPlayer
			{
				Name = NameUtil.Clean(entry.Name),
				Balance = entry.Balance
			};
		}
		SafeInvokeCacheChanged();
		SafeInvokeLocalChanged(LocalBalance, 0L, BalanceReason.Initial, "");
	}

	public static void ApplyUpdate(BalanceUpdateData data)
	{
		bool flag = data.PlatformID == PlayerIDManager.LocalPlatformID;
		if (data.Reason == BalanceReason.Denied)
		{
			if (flag)
			{
				SafeInvokeLocalChanged(LocalBalance, 0L, BalanceReason.Denied, data.Detail ?? "");
			}
			return;
		}
		if (!_cache.TryGetValue(data.PlatformID, out var value))
		{
			value = new CachedPlayer();
			_cache[data.PlatformID] = value;
		}
		value.Balance = data.NewBalance;
		if (string.IsNullOrEmpty(value.Name))
		{
			value.Name = NameUtil.Clean(GetLiveName(data.PlatformID));
		}
		SafeInvokeCacheChanged();
		if (flag)
		{
			SafeInvokeLocalChanged(data.NewBalance, data.Delta, data.Reason, data.Detail ?? "");
		}
	}

	public static void Clear()
	{
		_cache.Clear();
		SafeInvokeCacheChanged();
	}

	public static string GetDisplayName(ulong platformID)
	{
		string text = NameUtil.Clean(GetLiveName(platformID));
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		if (_cache.TryGetValue(platformID, out var value) && !string.IsNullOrWhiteSpace(value.Name))
		{
			return value.Name;
		}
		return "Player";
	}

	public static long GetBalance(ulong platformID)
	{
		if (!_cache.TryGetValue(platformID, out var value))
		{
			return 0L;
		}
		return value.Balance;
	}

	public static List<KeyValuePair<ulong, CachedPlayer>> GetLeaderboard()
	{
		return _cache.OrderByDescending((KeyValuePair<ulong, CachedPlayer> pair) => pair.Value.Balance).ToList();
	}

	private static string GetLiveName(ulong platformID)
	{
		PlayerID playerID = PlayerIDManager.GetPlayerID(platformID);
		if (playerID == null)
		{
			return null;
		}
		PlayerMetadata metadata = playerID.Metadata;
		if (metadata == null)
		{
			return null;
		}
		MetadataVariable username = metadata.Username;
		if (username == null)
		{
			return null;
		}
		return username.GetValue();
	}

	private static void SafeInvokeCacheChanged()
	{
		try
		{
			ClientEconomy.OnCacheChanged?.Invoke();
		}
		catch (Exception value)
		{
			MelonLogger.Error($"[LabRP] Error in OnCacheChanged listener: {value}");
		}
	}

	private static void SafeInvokeLocalChanged(long balance, long delta, BalanceReason reason, string detail)
	{
		try
		{
			ClientEconomy.OnLocalChanged?.Invoke(balance, delta, reason, detail);
		}
		catch (Exception value)
		{
			MelonLogger.Error($"[LabRP] Error in OnLocalChanged listener: {value}");
		}
	}
}
