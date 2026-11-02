using LabFusion.Network.Serialization;

namespace LabRP.Network;

public class BalanceUpdateData : INetSerializable
{
	public ulong PlatformID;

	public long NewBalance;

	public long Delta;

	public BalanceReason Reason;

	public string Detail = "";

	public int? GetSize()
	{
		return 25 + SizeExtensions.GetSize(Detail);
	}

	public void Serialize(INetSerializer serializer)
	{
		serializer.SerializeValue(ref PlatformID);
		serializer.SerializeValue(ref NewBalance);
		serializer.SerializeValue(ref Delta);
		serializer.SerializeValue<BalanceReason>(ref Reason, (Precision)3);
		serializer.SerializeValue(ref Detail);
	}
}
