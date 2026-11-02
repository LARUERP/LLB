using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;

namespace LabRP.Economy;

public static class EconomySave
{
	private static string FolderPath => Path.Combine(MelonEnvironment.UserDataDirectory, "LabRP");

	private static string FilePath => Path.Combine(FolderPath, "economy.json");

	private static string BackupPath => Path.Combine(FolderPath, "economy.backup.json");

	private static string TempPath => Path.Combine(FolderPath, "economy.tmp.json");

	private static string DamagedPath => Path.Combine(FolderPath, "economy.damaged.json");

	public static Dictionary<ulong, PlayerRecord> Load()
	{
		Dictionary<ulong, PlayerRecord> dictionary = TryLoadFile(FilePath, "save");
		bool flag = dictionary == null && File.Exists(FilePath);
		if (flag)
		{
			PreserveDamagedMain();
		}
		bool flag2 = false;
		if (dictionary == null)
		{
			dictionary = TryLoadFile(BackupPath, "backup");
			flag2 = dictionary != null;
		}
		if (dictionary == null)
		{
			if (flag)
			{
				MelonLogger.Error("[LabRP] Both save files were unreadable. The broken save was kept as economy.damaged.json; starting a fresh economy.");
			}
			return new Dictionary<ulong, PlayerRecord>();
		}
		Sanitize(dictionary);
		if (flag2)
		{
			Save(dictionary);
			MelonLogger.Msg("[LabRP] Recovered the economy from the backup file. The unreadable save was kept as economy.damaged.json.");
		}
		MelonLogger.Msg($"[LabRP] Loaded {dictionary.Count} saved player record(s).");
		return dictionary;
	}

	private static Dictionary<ulong, PlayerRecord> TryLoadFile(string path, string label)
	{
		try
		{
			if (!File.Exists(path))
			{
				return null;
			}
			return JsonConvert.DeserializeObject<Dictionary<ulong, PlayerRecord>>(File.ReadAllText(path));
		}
		catch (Exception ex)
		{
			MelonLogger.Error("[LabRP] Failed to read the " + label + " file: " + ex.Message);
			return null;
		}
	}

	private static void PreserveDamagedMain()
	{
		try
		{
			if (File.Exists(FilePath))
			{
				File.Copy(FilePath, DamagedPath, overwrite: true);
			}
		}
		catch
		{
		}
	}

	private static void Sanitize(Dictionary<ulong, PlayerRecord> records)
	{
		foreach (ulong item in records.Keys.ToList())
		{
			if (records[item] == null)
			{
				records[item] = new PlayerRecord
				{
					Name = "Unknown",
					Balance = LabRPConfig.ClampBalance(LabRPConfig.StartingBalance.Value),
					FirstSeenUtc = DateTime.UtcNow.ToString("o"),
					LastSeenUtc = DateTime.UtcNow.ToString("o")
				};
			}
		}
		foreach (PlayerRecord value in records.Values)
		{
			value.Balance = LabRPConfig.ClampBalance(value.Balance);
			value.Name = (string.IsNullOrWhiteSpace(value.Name) ? "Unknown" : NameUtil.Clean(value.Name));
			PlayerRecord playerRecord = value;
			if (playerRecord.LastSeenUtc == null)
			{
				playerRecord.LastSeenUtc = "";
			}
			playerRecord = value;
			if (playerRecord.FirstSeenUtc == null)
			{
				playerRecord.FirstSeenUtc = "";
			}
		}
	}

	public static bool Save(Dictionary<ulong, PlayerRecord> records)
	{
		try
		{
			Directory.CreateDirectory(FolderPath);
			string contents = JsonConvert.SerializeObject((object)records, (Formatting)1);
			File.WriteAllText(TempPath, contents);
			if (File.Exists(FilePath))
			{
				File.Replace(TempPath, FilePath, BackupPath);
			}
			else
			{
				File.Move(TempPath, FilePath);
			}
			return true;
		}
		catch (Exception ex)
		{
			MelonLogger.Error("[LabRP] Failed to save economy data: " + ex.Message);
			return false;
		}
	}
}
