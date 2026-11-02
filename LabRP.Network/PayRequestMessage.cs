using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabRP.Economy;

namespace LabRP.Network;

public class PayRequestMessage : ModuleMessageHandler
{
	protected override void OnHandleMessage(ReceivedMessage received)
	{
		if (((ReceivedMessage)(ref received)).IsServerHandled && NetworkInfo.IsHost && ((ReceivedMessage)(ref received)).Sender.HasValue)
		{
			PayRequestData payRequestData = ((ReceivedMessage)(ref received)).ReadData<PayRequestData>();
			ServerEconomy.TryTransfer(((ReceivedMessage)(ref received)).Sender.Value, payRequestData.TargetPlatformID, payRequestData.Amount);
		}
	}
}
