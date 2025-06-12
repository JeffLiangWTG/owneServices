using Microsoft.BizTalk.Message.Interop;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Products.AirMessaging.Schemas.Properies;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	[SupportedServiceProvider(ServiceProvider.Descartes)]
	class DescartesEnvelopContext : EnvelopContext
	{
		public override void Populate(IBaseMessage message, PartyResolver partyResolver)
		{
			var promotedValue = new DescartesPromotedValue();
			InternalMessage = promotedValue.Find(message, "InternalMessage");
			ExtractMessageTypeAndVersion();
			ClientAWB = ConextHelper.BuildAWBFromMessage(InternalMessage);
		    SenderPIMA = message.Context.ReadPropertyString<SenderPIMA>();
            SenderID = partyResolver.ResolveParty(SenderPIMA);
		    RecipientPIMA = message.Context.ReadPropertyString<RecipientPIMA>();
			RecipientID = (!string.IsNullOrEmpty(RecipientPIMA)) ? partyResolver.ResolveRecipient(RecipientPIMA, ClientAWB) : partyResolver.ResolveClientAWB(ClientAWB);
			InternalMessage = RemoveExtraHeaderFromInternalMessage(InternalMessage);
        }
	}
}
