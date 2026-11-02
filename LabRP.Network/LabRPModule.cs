using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Il2CppSLZ.Marrow;
using LabFusion.Player;
using LabFusion.SDK.Modules;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using LabRP.Economy;
using LabRP.UI;

namespace LabRP.Network;

public class LabRPModule : Module
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static ServerEvent _003C0_003E__OnStartedServer;

		public static ServerEvent _003C1_003E__OnJoinedServer;

		public static ServerEvent _003C2_003E__OnDisconnected;

		public static PlayerUpdate _003C3_003E__OnPlayerJoined;

		public static PlayerUpdate _003C4_003E__OnPlayerLeft;

		public static Action<PlayerID, string, string> _003C5_003E__OnMetadataChanged;

		public static Action<long, long, BalanceReason, string> _003C6_003E__OnLocalBalanceChanged;
	}

	private Action<RigManager> _onRigCreated;

	private Action<long, long, BalanceReason, string> _onLocalChanged;

	private Action<PlayerID, string, string> _onMetadataChanged;

	public override string Name => "LabRP";

	public override string Author => "windudes98";

	public override Version Version => new Version(1, 1, 2);

	public override ConsoleColor Color => ConsoleColor.Green;

	protected override void OnModuleRegistered()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		ModuleMessageManager.LoadHandlers(Assembly.GetExecutingAssembly());
		object obj = _003C_003EO._003C0_003E__OnStartedServer;
		if (obj == null)
		{
			ServerEvent val = OnStartedServer;
			_003C_003EO._003C0_003E__OnStartedServer = val;
			obj = (object)val;
		}
		MultiplayerHooking.OnStartedServer += (ServerEvent)obj;
		object obj2 = _003C_003EO._003C1_003E__OnJoinedServer;
		if (obj2 == null)
		{
			ServerEvent val2 = OnJoinedServer;
			_003C_003EO._003C1_003E__OnJoinedServer = val2;
			obj2 = (object)val2;
		}
		MultiplayerHooking.OnJoinedServer += (ServerEvent)obj2;
		object obj3 = _003C_003EO._003C2_003E__OnDisconnected;
		if (obj3 == null)
		{
			ServerEvent val3 = OnDisconnected;
			_003C_003EO._003C2_003E__OnDisconnected = val3;
			obj3 = (object)val3;
		}
		MultiplayerHooking.OnDisconnected += (ServerEvent)obj3;
		object obj4 = _003C_003EO._003C3_003E__OnPlayerJoined;
		if (obj4 == null)
		{
			PlayerUpdate val4 = OnPlayerJoined;
			_003C_003EO._003C3_003E__OnPlayerJoined = val4;
			obj4 = (object)val4;
		}
		MultiplayerHooking.OnPlayerJoined += (PlayerUpdate)obj4;
		object obj5 = _003C_003EO._003C4_003E__OnPlayerLeft;
		if (obj5 == null)
		{
			PlayerUpdate val5 = OnPlayerLeft;
			_003C_003EO._003C4_003E__OnPlayerLeft = val5;
			obj5 = (object)val5;
		}
		MultiplayerHooking.OnPlayerLeft += (PlayerUpdate)obj5;
		_onMetadataChanged = OnMetadataChanged;
		PlayerID.OnMetadataChangedEvent += _onMetadataChanged;
		_onRigCreated = delegate
		{
			WristHud.OnLocalRigCreated();
		};
		LocalPlayer.OnLocalRigCreated = (Action<RigManager>)Delegate.Combine(LocalPlayer.OnLocalRigCreated, _onRigCreated);
		_onLocalChanged = OnLocalBalanceChanged;
		ClientEconomy.OnLocalChanged += _onLocalChanged;
		LabRPMenu.Setup();
		((Module)this).LoggerInstance.Log("LabRP module registered: economy online.", ConsoleColor.White);
	}

	protected override void OnModuleUnregistered()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		object obj = _003C_003EO._003C0_003E__OnStartedServer;
		if (obj == null)
		{
			ServerEvent val = OnStartedServer;
			_003C_003EO._003C0_003E__OnStartedServer = val;
			obj = (object)val;
		}
		MultiplayerHooking.OnStartedServer -= (ServerEvent)obj;
		object obj2 = _003C_003EO._003C1_003E__OnJoinedServer;
		if (obj2 == null)
		{
			ServerEvent val2 = OnJoinedServer;
			_003C_003EO._003C1_003E__OnJoinedServer = val2;
			obj2 = (object)val2;
		}
		MultiplayerHooking.OnJoinedServer -= (ServerEvent)obj2;
		object obj3 = _003C_003EO._003C2_003E__OnDisconnected;
		if (obj3 == null)
		{
			ServerEvent val3 = OnDisconnected;
			_003C_003EO._003C2_003E__OnDisconnected = val3;
			obj3 = (object)val3;
		}
		MultiplayerHooking.OnDisconnected -= (ServerEvent)obj3;
		object obj4 = _003C_003EO._003C3_003E__OnPlayerJoined;
		if (obj4 == null)
		{
			PlayerUpdate val4 = OnPlayerJoined;
			_003C_003EO._003C3_003E__OnPlayerJoined = val4;
			obj4 = (object)val4;
		}
		MultiplayerHooking.OnPlayerJoined -= (PlayerUpdate)obj4;
		object obj5 = _003C_003EO._003C4_003E__OnPlayerLeft;
		if (obj5 == null)
		{
			PlayerUpdate val5 = OnPlayerLeft;
			_003C_003EO._003C4_003E__OnPlayerLeft = val5;
			obj5 = (object)val5;
		}
		MultiplayerHooking.OnPlayerLeft -= (PlayerUpdate)obj5;
		PlayerID.OnMetadataChangedEvent -= _onMetadataChanged;
		LocalPlayer.OnLocalRigCreated = (Action<RigManager>)Delegate.Remove(LocalPlayer.OnLocalRigCreated, _onRigCreated);
		ClientEconomy.OnLocalChanged -= _onLocalChanged;
	}

	private static void OnStartedServer()
	{
		ServerEconomy.OnServerStarted();
		WristHud.OnJoinedLobby();
	}

	private static void OnJoinedServer()
	{
		WristHud.OnJoinedLobby();
	}

	private static void OnDisconnected()
	{
		ServerEconomy.OnDisconnected();
		ClientEconomy.Clear();
		WristHud.OnLeftLobby();
	}

	private static void OnPlayerJoined(PlayerID playerID)
	{
		ServerEconomy.OnPlayerJoined(playerID);
		LabRPMenu.OnRosterChanged();
	}

	private static void OnPlayerLeft(PlayerID playerID)
	{
		ServerEconomy.OnPlayerLeft(playerID);
		LabRPMenu.OnRosterChanged();
	}

	private static void OnMetadataChanged(PlayerID playerID, string key, string value)
	{
		if (key == "Username")
		{
			ServerEconomy.OnUsernameChanged(playerID);
		}
	}

	private static void OnLocalBalanceChanged(long newBalance, long delta, BalanceReason reason, string detail)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		WristHud.OnLocalBalanceChanged(newBalance, delta, reason, detail);
		detail = NameUtil.Clean(detail);
		switch (reason)
		{
		case BalanceReason.PaymentReceived:
			if (delta != 0L)
			{
				Notifier.Send(new Notification
				{
					Title = NotificationText.op_Implicit("LabRP"),
					Message = NotificationText.op_Implicit(detail + " paid you $" + FormatAbs(delta) + "!"),
					Type = (NotificationType)3,
					ShowPopup = true,
					PopupLength = 3f
				});
			}
			break;
		case BalanceReason.Denied:
			Notifier.Send(new Notification
			{
				Title = NotificationText.op_Implicit("LabRP"),
				Message = NotificationText.op_Implicit("Payment failed: " + detail),
				Type = (NotificationType)1,
				ShowPopup = true,
				PopupLength = 3f
			});
			break;
		case BalanceReason.Admin:
			if (delta != 0L)
			{
				Notifier.Send(new Notification
				{
					Title = NotificationText.op_Implicit("LabRP"),
					Message = NotificationText.op_Implicit((delta >= 0) ? ("The host gave you $" + FormatAbs(delta) + ".") : ("The host took $" + FormatAbs(delta) + " from you.")),
					Type = (NotificationType)0,
					ShowPopup = true,
					PopupLength = 3f
				});
			}
			break;
		}
	}

	private static string FormatAbs(long value)
	{
		if (value == long.MinValue)
		{
			value = long.MaxValue;
		}
		return Math.Abs(value).ToString("N0");
	}
}
