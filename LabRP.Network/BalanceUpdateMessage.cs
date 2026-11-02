using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabRP.Economy;

namespace LabRP.Network;

public class BalanceUpdateMessage : ModuleMessageHandler
{
	protected override void OnHandleMessage(ReceivedMessage received)
	{
		if (((ReceivedMessage)(ref received)).Sender.HasValue && ((ReceivedMessage)(ref received)).Sender.Value == 0)
		{
			ClientEconomy.ApplyUpdate(((ReceivedMessage)(ref received)).ReadData<BalanceUpdateData>());
		}
	}
}
