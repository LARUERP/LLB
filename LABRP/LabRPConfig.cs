using MelonLoader;
using MelonLoader.Preferences;

namespace LabRP;

public static class LabRPConfig
{
	public const long MaxBalance = 999999999999L;

	public static MelonPreferences_Category Category { get; private set; }

	public static MelonPreferences_Entry<long> StartingBalance { get; private set; }

	public static MelonPreferences_Entry<long> SalaryAmount { get; private set; }

	public static MelonPreferences_Entry<float> SalaryIntervalMinutes { get; private set; }

	public static MelonPreferences_Entry<bool> HudEnabled { get; private set; }

	public static MelonPreferences_Entry<bool> HudBillboard { get; private set; }

	public static MelonPreferences_Entry<float> HudScale { get; private set; }

	public static MelonPreferences_Entry<float> HudOffsetX { get; private set; }

	public static MelonPreferences_Entry<float> HudOffsetY { get; private set; }

	public static MelonPreferences_Entry<float> HudOffsetZ { get; private set; }

	public static MelonPreferences_Entry<float> HudYaw { get; private set; }

	public static void Initialize()
	{
		Category = MelonPreferences.CreateCategory("LabRP");
		StartingBalance = Category.CreateEntry<long>("StartingBalance", 500L, "Starting Balance", "Money a player starts with the first time they join your server.", false, false, (ValueValidator)null, (string)null);
		SalaryAmount = Category.CreateEntry<long>("SalaryAmount", 200L, "Salary Amount", "Money paid to every connected player each paycheck.", false, false, (ValueValidator)null, (string)null);
		SalaryIntervalMinutes = Category.CreateEntry<float>("SalaryIntervalMinutes", 5f, "Salary Interval (Minutes)", "How often paychecks are handed out.", false, false, (ValueValidator)null, (string)null);
		HudEnabled = Category.CreateEntry<bool>("HudEnabled", true, "Wrist HUD Enabled", (string)null, false, false, (ValueValidator)null, (string)null);
		HudBillboard = Category.CreateEntry<bool>("HudBillboard", true, "Wrist HUD Faces You", "When true the money counter always turns to face your head.", false, false, (ValueValidator)null, (string)null);
		HudScale = Category.CreateEntry<float>("HudScale", 1f, "Wrist HUD Scale", (string)null, false, false, (ValueValidator)null, (string)null);
		HudOffsetX = Category.CreateEntry<float>("HudOffsetX", 0f, "Wrist HUD Offset X", (string)null, false, false, (ValueValidator)null, (string)null);
		HudOffsetY = Category.CreateEntry<float>("HudOffsetY", 0.055f, "Wrist HUD Offset Y", (string)null, false, false, (ValueValidator)null, (string)null);
		HudOffsetZ = Category.CreateEntry<float>("HudOffsetZ", -0.085f, "Wrist HUD Offset Z", (string)null, false, false, (ValueValidator)null, (string)null);
		HudYaw = Category.CreateEntry<float>("HudYaw", 0f, "Wrist HUD Yaw (Fixed Mode)", "Extra rotation applied when the HUD is not in billboard mode.", false, false, (ValueValidator)null, (string)null);
		SanitizeFloats();
	}

	private static void SanitizeFloats()
	{
		if ((0u | (FixNonFinite(SalaryIntervalMinutes, 5f) ? 1u : 0u) | (FixNonFinite(HudScale, 1f) ? 1u : 0u) | (FixNonFinite(HudOffsetX, 0f) ? 1u : 0u) | (FixNonFinite(HudOffsetY, 0.055f) ? 1u : 0u) | (FixNonFinite(HudOffsetZ, -0.085f) ? 1u : 0u) | (FixNonFinite(HudYaw, 0f) ? 1u : 0u)) != 0)
		{
			Save();
		}
	}

	private static bool FixNonFinite(MelonPreferences_Entry<float> entry, float fallback)
	{
		if (float.IsNaN(entry.Value) || float.IsInfinity(entry.Value))
		{
			entry.Value = fallback;
			return true;
		}
		return false;
	}

	public static void Save()
	{
		Category.SaveToFile(false);
	}

	public static long ClampBalance(long value)
	{
		if (value < 0)
		{
			return 0L;
		}
		if (value > 999999999999L)
		{
			return 999999999999L;
		}
		return value;
	}
}
