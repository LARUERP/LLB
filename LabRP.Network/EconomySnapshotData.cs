using System.Collections.Generic;
using LabFusion.Network.Serialization;

namespace LabRP.Network;

public class EconomySnapshotData : INetSerializable
{
	public List<SnapshotEntry> Entries = new List<SnapshotEntry>();

	public const int MaxEntries = 512;

	public int? GetSize()
	{
		int num = 4;
		foreach (SnapshotEntry entry in Entries)
		{
			num += 8 + SizeExtensions.GetSize(entry.Name ?? "") + 8;
		}
		return num;
	}

	public void Serialize(INetSerializer serializer)
	{
		int num = Entries?.Count ?? 0;
		serializer.SerializeValue(ref num);
		if (serializer.IsReader)
		{
			if (num < 0 || num > 512)
			{
				Entries = new List<SnapshotEntry>();
				return;
			}
			Entries = new List<SnapshotEntry>(num);
			for (int i = 0; i < num; i++)
			{
				ulong platformID = 0uL;
				string text = "";
				long balance = 0L;
				serializer.SerializeValue(ref platformID);
				serializer.SerializeValue(ref text);
				serializer.SerializeValue(ref balance);
				Entries.Add(new SnapshotEntry
				{
					PlatformID = platformID,
					Name = (text ?? ""),
					Balance = balance
				});
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				SnapshotEntry snapshotEntry = Entries[j];
				ulong platformID2 = snapshotEntry.PlatformID;
				string text2 = snapshotEntry.Name ?? "";
				long balance2 = snapshotEntry.Balance;
				serializer.SerializeValue(ref platformID2);
				serializer.SerializeValue(ref text2);
				serializer.SerializeValue(ref balance2);
			}
		}
	}
}
