using LabFusion.Network.Serialization;

namespace LabRP.Network;

public class PayRequestData : INetSerializable
{
	public ulong TargetPlatformID;

	public long Amount;

	public int? GetSize()
	{
		return 16;
	}

	public void Serialize(INetSerializer serializer)
	{
		serializer.SerializeValue(ref TargetPlatformID);
		serializer.SerializeValue(ref Amount);
	}
}
