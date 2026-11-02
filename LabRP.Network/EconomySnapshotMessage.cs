using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabRP.Economy;

namespace LabRP.Network;

public class EconomySnapshotMessage : ModuleMessageHandler
{
	protected override void OnHandleMessage(ReceivedMessage received)
	{
		if (((ReceivedMessage)(ref received)).Sender.HasValue && ((ReceivedMessage)(ref received)).Sender.Value == 0)
		{
			ClientEconomy.ApplySnapshot(((ReceivedMessage)(ref received)).ReadData<EconomySnapshotData>());
		}
	}
}
