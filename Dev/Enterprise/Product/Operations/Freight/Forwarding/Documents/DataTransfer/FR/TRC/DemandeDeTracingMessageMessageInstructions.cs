using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	public sealed class DemandeDeTracingMessageMessageInstructions : IMessageInstructions
	{
		public DemandeDeTracingMessageMessageInstructions(DemandeDeTracingDirection direction)
		{
			DocumentName = direction == DemandeDeTracingDirection.Import
				? (NoResString)"Tracing Request (TRC) - Import" // programmatic constant
				: (NoResString)"Tracing Request (TRC) - Export"; // programmatic constant
		}

		public string DocumentName { get; }
		public string TranslatedDocumentName => string.Empty;
		public string DataContext => Documents.DataContext.FRPortsTrackingRequestTRC;
		public string Recipient => (NoResString)"Terminal";  // Constant String
		public string EHubClientID => "FORWARDING_PORT_MESSAGE";
		public string DirectXTClientID => string.Empty;
		public string XmlNamespace => "TRC/1";

		public bool AllowSendMessage => true;
		public bool AllowSendMessageAmendment => false;
		public bool AllowSendMessageWithdrawal => false;
		public bool AllowResetToOriginal => false;

		public bool OrderLogsByLocalTime => true;
		public bool RequireMessageAmendmentReason => false;

		public ICodeDescriptionPairList AmendmentOptions => null;
		public ICodeDescriptionPairList WidthdrawalOptions => null;
	}
}
