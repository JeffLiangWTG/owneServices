using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	[SupportedServiceProvider(ServiceProvider.BT)]
	class BTEnvelopContext : EnvelopContext
	{
		public override void Populate(IBaseMessage message, PartyResolver partyResolver)
		{
			var promotedValue = new BTPromotedValue();
			InternalMessage = promotedValue.Find(message, "InternalMessage");
			ClientAWB = ConextHelper.BuildAWBFromMessage(InternalMessage);
            SenderPIMA = promotedValue.Find(message, "AirlinePIMA");
            SenderID = partyResolver.ResolveParty(SenderPIMA);
		    RecipientPIMA = promotedValue.Find(message, "RecipientPIMA");
            RecipientID = partyResolver.ResolveRecipient(RecipientPIMA, ClientAWB);
			ExtractMessageTypeAndVersion();
            InternalMessage = RemoveExtraHeaderFromInternalMessage(InternalMessage);
        }
	}
}
