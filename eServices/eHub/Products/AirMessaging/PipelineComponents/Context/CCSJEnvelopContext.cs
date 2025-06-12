using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	[SupportedServiceProvider(ServiceProvider.CCSJ)]
	class CCSJEnvelopContext : EnvelopContext
	{
		public override void Populate(IBaseMessage message, PartyResolver partyResolver)
		{
			var promotedValue = new CCSJPromotedValue();
			InternalMessage = promotedValue.Find(message, "InternalMessage").TrimEnd() + "\r\n";
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
