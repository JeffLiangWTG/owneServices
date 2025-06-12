using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
    [SupportedServiceProvider(ServiceProvider.Cargonaut)]
    class CargonautEnvelopContext : EnvelopContext
	{
		public override void Populate(IBaseMessage message, PartyResolver partyResolver)
		{
			var promotedValue = new CargonautPromotedValue();
			InternalMessage = promotedValue.Find(message, "InternalMessage");
			ClientAWB = ExtractAWBFromMessage(InternalMessage);
			RecipientPIMA = promotedValue.Find(message, "ClientPIMA");
            RecipientID = partyResolver.ResolveRecipient(RecipientPIMA, ClientAWB);
            MessageType = promotedValue.Find(message, "MessageType");
            MessageVersion = promotedValue.Find(message, "MessageVersion");
            InternalMessage = RemoveExtraHeaderFromInternalMessage(InternalMessage);
        }

		private static string ExtractAWBFromMessage(string internalMessage)
		{
			if (string.IsNullOrEmpty(internalMessage))
				return internalMessage;
			var rex = System.Text.RegularExpressions.Regex.Match(internalMessage, @"(\d{3}-\d{8})");

			if (!rex.Success)
				return null;
			return rex.Groups[0].Value.Replace("-", string.Empty);
		}
	}
}
