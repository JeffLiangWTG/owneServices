using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	[SupportedServiceProvider(ServiceProvider.GLSHK)]
    [SupportedServiceProvider(ServiceProvider.CargoStart)]
	class GLSHKEnvelopContext : EnvelopContext
	{
		public override void Populate(IBaseMessage message, PartyResolver partyResolver)
		{
			var promotedValue = new GLSHKPromotedValue();
			InternalMessage = promotedValue.Find(message, "InternalMessage");
			ClientAWB = ConextHelper.BuildAWBFromMessage(InternalMessage);
		    SenderPIMA = promotedValue.Find(message, "SenderPIMA");
            SenderID = partyResolver.ResolveParty(SenderPIMA);
		    RecipientPIMA = promotedValue.Find(message, "RecipientPIMA");
            RecipientID = partyResolver.ResolveRecipient(RecipientPIMA, ClientAWB);
			MessageType = promotedValue.Find(message, "MessageType");
			MessageVersion = promotedValue.Find(message, "MessageVersion");
			MessageType = ConextHelper.ConvertMessageTypeToStandard(MessageType);
			Reference = promotedValue.Find(message, "Reference");
            InternalMessage = RemoveExtraHeaderFromInternalMessage(InternalMessage);
        }
	}
}
