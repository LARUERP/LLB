using System;
using System.Collections.Generic;
using BoneLib.BoneMenu;
using LabFusion.Network;
using LabFusion.Player;
using LabRP.Economy;
using LabRP.Network;
using MelonLoader;
using UnityEngine;

namespace LabRP.UI;

public static class LabRPMenu
{
	private static Page _rootPage;

	private static FunctionElement _balanceLabel;

	private static Page _payPage;

	private static Page _payDetailPage;

	private static Page _balancesPage;

	private static Page _adminPage;

	private static Page _adminDetailPage;

	private static Page _hudPage;

	private static ulong _payTarget;

	private static int _payAmount = 100;

	private static ulong _adminTarget;

	private static int _adminSetAmount = 500;

	private static bool _labelDirty;

	private static bool _pageDirty;

	private static float _rebuildCooldown;

	private const float RebuildIntervalSeconds = 0.3f;

	private static readonly Color Green = new Color(0.45f, 1f, 0.6f, 1f);

	private static readonly Color Gold = new Color(1f, 0.85f, 0.4f, 1f);

	private static readonly Color Cyan = new Color(0.5f, 0.9f, 1f, 1f);

	private static readonly Color Red = new Color(1f, 0.45f, 0.45f, 1f);

	public static void Setup()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		Menu.Initialize();
		_rootPage = Page.Root.CreatePage("LabRP", Green, 0, true);
		_balanceLabel = _rootPage.CreateFunction("My Balance: $0", Color.white, (Action)delegate
		{
		});
		_payPage = _rootPage.CreatePage("Pay Players", Cyan, 0, true);
		_payDetailPage = _payPage.CreatePage("Payment", Cyan, 0, false);
		_balancesPage = _rootPage.CreatePage("Balances", Green, 0, true);
		_adminPage = _rootPage.CreatePage("Host Admin", Gold, 0, true);
		_adminDetailPage = _adminPage.CreatePage("Manage Player", Gold, 0, false);
		_hudPage = _rootPage.CreatePage("Wrist HUD", Color.white, 0, true);
		BuildHudPage();
		Menu.OnPageOpened += OnPageOpened;
		ClientEconomy.OnCacheChanged += OnCacheChanged;
		UpdateBalanceLabel();
	}

	private static void OnPageOpened(Page page)
	{
		if (page == _payPage)
		{
			BuildPayPage();
		}
		else if (page == _balancesPage)
		{
			BuildBalancesPage();
		}
		else if (page == _adminPage)
		{
			BuildAdminPage();
		}
	}

	private static void OnCacheChanged()
	{
		_labelDirty = true;
		_pageDirty = true;
	}

	public static void OnRosterChanged()
	{
		_pageDirty = true;
	}

	public static void Tick()
	{
		if (_rebuildCooldown > 0f)
		{
			_rebuildCooldown -= Time.unscaledDeltaTime;
		}
		else if (_labelDirty || _pageDirty)
		{
			_rebuildCooldown = 0.3f;
			if (_labelDirty)
			{
				_labelDirty = false;
				UpdateBalanceLabel();
			}
			if (_pageDirty)
			{
				_pageDirty = false;
				RefreshCurrentPage();
			}
		}
	}

	private static void RefreshCurrentPage()
	{
		Page currentPage = Menu.CurrentPage;
		if (currentPage == _payPage)
		{
			BuildPayPage();
		}
		else if (currentPage == _adminPage)
		{
			BuildAdminPage();
		}
		else if (currentPage == _balancesPage)
		{
			BuildBalancesPage();
		}
		else if (currentPage == _payDetailPage)
		{
			if (!IsOnline(_payTarget))
			{
				Menu.OpenPage(_payPage);
			}
			else
			{
				BuildPayDetailPage();
			}
		}
		else if (currentPage == _adminDetailPage)
		{
			if (!IsOnline(_adminTarget))
			{
				Menu.OpenPage(_adminPage);
			}
			else
			{
				BuildAdminDetailPage();
			}
		}
	}

	private static bool IsOnline(ulong platformID)
	{
		PlayerID playerID = PlayerIDManager.GetPlayerID(platformID);
		if (playerID != null)
		{
			return playerID.IsValid;
		}
		return false;
	}

	private static void UpdateBalanceLabel()
	{
		if (_balanceLabel != null)
		{
			((Element)_balanceLabel).ElementName = (NetworkInfo.HasServer ? $"My Balance: ${ClientEconomy.LocalBalance:N0}" : "My Balance: (join a Fusion lobby)");
		}
	}

	private static void BuildPayPage()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		_payPage.RemoveAll();
		if (!NetworkInfo.HasServer)
		{
			_payPage.CreateFunction("Join a Fusion lobby first!", Color.gray, (Action)delegate
			{
			});
			return;
		}
		bool flag = false;
		foreach (PlayerID otherPlayer in GetOtherPlayers())
		{
			flag = true;
			ulong platformID = otherPlayer.PlatformID;
			string displayName = ClientEconomy.GetDisplayName(platformID);
			_payPage.CreateFunction("Pay " + displayName, Cyan, (Action)delegate
			{
				_payTarget = platformID;
				BuildPayDetailPage();
				Menu.OpenPage(_payDetailPage);
			});
		}
		if (!flag)
		{
			_payPage.CreateFunction("Nobody else is here yet...", Color.gray, (Action)delegate
			{
			});
		}
	}

	private static void BuildPayDetailPage()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		_payDetailPage.RemoveAll();
		_payAmount = Math.Clamp(_payAmount, 1, 1000000);
		string displayName = ClientEconomy.GetDisplayName(_payTarget);
		_payDetailPage.CreateFunction("Paying: " + displayName, Color.white, (Action)delegate
		{
		});
		_payDetailPage.CreateFunction($"Your money: ${ClientEconomy.LocalBalance:N0}", Color.gray, (Action)delegate
		{
		});
		_payDetailPage.CreateInt("Amount", Cyan, _payAmount, 50, 1, 1000000, (Action<int>)delegate(int value)
		{
			_payAmount = value;
		});
		_payDetailPage.CreateFunction("+ $100", Color.white, (Action)delegate
		{
			_payAmount += 100;
			BuildPayDetailPage();
		});
		_payDetailPage.CreateFunction("+ $1,000", Color.white, (Action)delegate
		{
			_payAmount += 1000;
			BuildPayDetailPage();
		});
		_payDetailPage.CreateFunction("Reset Amount", Color.gray, (Action)delegate
		{
			_payAmount = 100;
			BuildPayDetailPage();
		});
		_payDetailPage.CreateFunction("CONFIRM PAYMENT", Green, (Action)delegate
		{
			SendPayRequest(_payTarget, _payAmount);
			Menu.OpenPage(_payPage);
		});
	}

	private static void SendPayRequest(ulong targetPlatformID, long amount)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkInfo.HasServer && amount > 0)
		{
			MessageRelay.RelayModule<PayRequestMessage, PayRequestData>(new PayRequestData
			{
				TargetPlatformID = targetPlatformID,
				Amount = amount
			}, new MessageRoute((RelayType)1, (NetworkChannel)0));
		}
	}

	private static void BuildBalancesPage()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		_balancesPage.RemoveAll();
		if (!NetworkInfo.HasServer)
		{
			_balancesPage.CreateFunction("Join a Fusion lobby first!", Color.gray, (Action)delegate
			{
			});
			return;
		}
		List<KeyValuePair<ulong, ClientEconomy.CachedPlayer>> leaderboard = ClientEconomy.GetLeaderboard();
		if (leaderboard.Count == 0)
		{
			_balancesPage.CreateFunction("No balances synced yet...", Color.gray, (Action)delegate
			{
			});
			return;
		}
		ulong localPlatformID = PlayerIDManager.LocalPlatformID;
		int num = 1;
		foreach (KeyValuePair<ulong, ClientEconomy.CachedPlayer> item in leaderboard)
		{
			string displayName = ClientEconomy.GetDisplayName(item.Key);
			bool num2 = item.Key == localPlatformID;
			Color val = (num2 ? Green : Color.white);
			string value = (num2 ? " (you)" : "");
			_balancesPage.CreateFunction($"#{num}  {displayName}{value}: ${item.Value.Balance:N0}", val, (Action)delegate
			{
			});
			num++;
		}
	}

	private static void BuildAdminPage()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		_adminPage.RemoveAll();
		if (!NetworkInfo.HasServer)
		{
			_adminPage.CreateFunction("Join a Fusion lobby first!", Color.gray, (Action)delegate
			{
			});
			return;
		}
		if (!NetworkInfo.IsHost)
		{
			_adminPage.CreateFunction("Host only. You are not the host.", Color.gray, (Action)delegate
			{
			});
			return;
		}
		_adminPage.CreateFunction("Salary Settings", Gold, (Action)delegate
		{
		});
		_adminPage.CreateInt("Salary Amount", Color.white, (int)Math.Clamp(LabRPConfig.SalaryAmount.Value, 0L, 1000000L), 50, 0, 1000000, (Action<int>)delegate(int value)
		{
			LabRPConfig.SalaryAmount.Value = value;
			LabRPConfig.Save();
		});
		_adminPage.CreateFloat("Salary Interval (min)", Color.white, Mathf.Clamp(LabRPConfig.SalaryIntervalMinutes.Value, 0.5f, 120f), 0.5f, 0.5f, 120f, (Action<float>)delegate(float value)
		{
			LabRPConfig.SalaryIntervalMinutes.Value = value;
			LabRPConfig.Save();
		});
		_adminPage.CreateFunction("Pay Salary To Everyone NOW", Green, (Action)ServerEconomy.PayEveryoneSalary);
		_adminPage.CreateFunction("Players", Gold, (Action)delegate
		{
		});
		foreach (PlayerID playerID in PlayerIDManager.PlayerIDs)
		{
			if (playerID != null && playerID.IsValid)
			{
				ulong platformID = playerID.PlatformID;
				string text = ClientEconomy.GetDisplayName(platformID);
				if (playerID.IsMe)
				{
					text += " (you)";
				}
				_adminPage.CreateFunction("Manage " + text, Color.white, (Action)delegate
				{
					_adminTarget = platformID;
					BuildAdminDetailPage();
					Menu.OpenPage(_adminDetailPage);
				});
			}
		}
		_adminPage.CreateFunction("RESET ALL BALANCES", Red, (Action)delegate
		{
			Menu.DisplayDialog("Reset Economy", "Reset every player to the starting balance and wipe all saved records?", (Texture2D)null, (Action)ServerEconomy.ResetAllBalances, (Action)null);
		});
	}

	private static void BuildAdminDetailPage()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		_adminDetailPage.RemoveAll();
		string displayName = ClientEconomy.GetDisplayName(_adminTarget);
		long balance = ServerEconomy.GetBalance(_adminTarget);
		_adminDetailPage.CreateFunction($"{displayName}: ${balance:N0}", Color.white, (Action)delegate
		{
		});
		_adminDetailPage.CreateFunction("Give $100", Green, (Action)delegate
		{
			AdminChange(100L, give: true);
		});
		_adminDetailPage.CreateFunction("Give $1,000", Green, (Action)delegate
		{
			AdminChange(1000L, give: true);
		});
		_adminDetailPage.CreateFunction("Give $10,000", Green, (Action)delegate
		{
			AdminChange(10000L, give: true);
		});
		_adminDetailPage.CreateFunction("Take $100", Red, (Action)delegate
		{
			AdminChange(100L, give: false);
		});
		_adminDetailPage.CreateFunction("Take $1,000", Red, (Action)delegate
		{
			AdminChange(1000L, give: false);
		});
		_adminDetailPage.CreateInt("Set Amount", Color.white, _adminSetAmount, 100, 0, 1000000, (Action<int>)delegate(int value)
		{
			_adminSetAmount = value;
		});
		_adminDetailPage.CreateFunction("Apply 'Set Amount'", Gold, (Action)delegate
		{
			ServerEconomy.AdminSet(_adminTarget, _adminSetAmount);
			BuildAdminDetailPage();
		});
	}

	private static void AdminChange(long amount, bool give)
	{
		if (give)
		{
			ServerEconomy.AdminGive(_adminTarget, amount);
		}
		else
		{
			ServerEconomy.AdminTake(_adminTarget, amount);
		}
		BuildAdminDetailPage();
	}

	private static void BuildHudPage()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		_hudPage.RemoveAll();
		_hudPage.CreateBool("HUD Enabled", Color.white, LabRPConfig.HudEnabled.Value, (Action<bool>)delegate(bool value)
		{
			LabRPConfig.HudEnabled.Value = value;
			LabRPConfig.Save();
		});
		_hudPage.CreateBool("Always Face You", Color.white, LabRPConfig.HudBillboard.Value, (Action<bool>)delegate(bool value)
		{
			LabRPConfig.HudBillboard.Value = value;
			LabRPConfig.Save();
			WristHud.ApplyTransformPrefs();
		});
		_hudPage.CreateFloat("Scale", Color.white, LabRPConfig.HudScale.Value, 0.25f, 0.1f, 6f, (Action<float>)delegate(float value)
		{
			LabRPConfig.HudScale.Value = value;
			WristHud.ApplyTransformPrefs();
		});
		_hudPage.CreateFunction("Position (1cm steps)", Color.gray, (Action)delegate
		{
		});
		CreateNudge("Up", 0f, 0.01f, 0f);
		CreateNudge("Down", 0f, -0.01f, 0f);
		CreateNudge("Left", -0.01f, 0f, 0f);
		CreateNudge("Right", 0.01f, 0f, 0f);
		CreateNudge("Toward Elbow", 0f, 0f, -0.01f);
		CreateNudge("Toward Fingers", 0f, 0f, 0.01f);
		_hudPage.CreateFloat("Yaw (fixed mode)", Color.white, LabRPConfig.HudYaw.Value, 15f, -180f, 180f, (Action<float>)delegate(float value)
		{
			LabRPConfig.HudYaw.Value = value;
			WristHud.ApplyTransformPrefs();
		});
		_hudPage.CreateFunction("Save HUD Settings", Green, (Action)LabRPConfig.Save);
		_hudPage.CreateFunction("Reset HUD Defaults", Red, (Action)delegate
		{
			LabRPConfig.HudEnabled.Value = true;
			LabRPConfig.HudBillboard.Value = true;
			LabRPConfig.HudScale.Value = 1f;
			LabRPConfig.HudOffsetX.Value = 0f;
			LabRPConfig.HudOffsetY.Value = 0.055f;
			LabRPConfig.HudOffsetZ.Value = -0.085f;
			LabRPConfig.HudYaw.Value = 0f;
			LabRPConfig.Save();
			WristHud.ApplyTransformPrefs();
			BuildHudPage();
		});
	}

	private static void CreateNudge(string label, float dx, float dy, float dz)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		_hudPage.CreateFunction("Nudge " + label, Color.white, (Action)delegate
		{
			MelonPreferences_Entry<float> hudOffsetX = LabRPConfig.HudOffsetX;
			hudOffsetX.Value += dx;
			MelonPreferences_Entry<float> hudOffsetY = LabRPConfig.HudOffsetY;
			hudOffsetY.Value += dy;
			MelonPreferences_Entry<float> hudOffsetZ = LabRPConfig.HudOffsetZ;
			hudOffsetZ.Value += dz;
			WristHud.ApplyTransformPrefs();
		});
	}

	private static IEnumerable<PlayerID> GetOtherPlayers()
	{
		foreach (PlayerID playerID in PlayerIDManager.PlayerIDs)
		{
			if (playerID != null && playerID.IsValid && !playerID.IsMe)
			{
				yield return playerID;
			}
		}
	}
}
