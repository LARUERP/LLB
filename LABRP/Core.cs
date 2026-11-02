using System;
using System.Runtime.CompilerServices;
using LabFusion.SDK.Modules;
using LabRP.Economy;
using LabRP.Network;
using LabRP.UI;
using MelonLoader;

namespace LabRP;

public class Core : MelonMod
{
	private static bool _fusionActive;

	public static Core Instance { get; private set; }

	public override void OnInitializeMelon()
	{
		Instance = this;
		LabRPConfig.Initialize();
		bool flag = MelonBase.FindMelon("LabFusion", "Lakatrazz") != null;
		bool flag2 = MelonBase.FindMelon("BoneLib", "The BONELAB Modding Community") != null;
		if (!flag || !flag2)
		{
			((MelonBase)this).LoggerInstance.Error($"LabRP requires Fusion (installed: {flag}) and BoneLib (installed: {flag2})! " + "Install the missing mod(s) and restart the game.");
			return;
		}
		try
		{
			LoadFusionSide();
			_fusionActive = true;
			((MelonBase)this).LoggerInstance.Msg("LabRP initialized. Join or host a Fusion server to use the economy.");
		}
		catch (Exception value)
		{
			((MelonBase)this).LoggerInstance.Error($"LabRP failed to initialize. Are Fusion and BoneLib up to date? {value}");
		}
	}

	public override void OnUpdate()
	{
		if (_fusionActive)
		{
			FusionUpdate();
		}
	}

	public override void OnApplicationQuit()
	{
		if (_fusionActive)
		{
			FusionQuit();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void LoadFusionSide()
	{
		ModuleManager.RegisterModule<LabRPModule>();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void FusionUpdate()
	{
		ServerEconomy.OnUpdate();
		WristHud.OnUpdate();
		LabRPMenu.Tick();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void FusionQuit()
	{
		ServerEconomy.SaveIfHosting();
	}
}
