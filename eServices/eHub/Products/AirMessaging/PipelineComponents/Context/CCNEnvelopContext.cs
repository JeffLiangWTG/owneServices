using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	[SupportedServiceProvider(ServiceProvider.CCN)]
	class CCNEnvelopContext : EnvelopContext
	{
		public override void Populate(IBaseMessage message, PartyResolver partyResolver)
		{
			var promotedValue = new CCNPromotedValue();
			InternalMessage = promotedValue.Find(message, "InternalMessage");
			ExtractMessageTypeAndVersion();
			ClientAWB = ConextHelper.BuildAWBFromMessage(InternalMessage);
		    SenderPIMA = promotedValue.Find(message, "SenderPIMA");
            SenderID = partyResolver.ResolveAirline(SenderPIMA);
		    RecipientPIMA = promotedValue.Find(message, "RecipientPIMA");
            RecipientID = partyResolver.ResolveRecipient(RecipientPIMA, ClientAWB);
            InternalMessage = RemoveExtraHeaderFromInternalMessage(InternalMessage);
        }
	}
}
