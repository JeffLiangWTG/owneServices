using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class CresaMessageInstructions : IMessageInstructions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Document name")]
		public string DocumentName => "Goods Received (CRESA)";
		public string TranslatedDocumentName => string.Empty;
		public string DataContext => Documents.DataContext.FRPortsGoodsReceivedCRESA;
		public string Recipient => (NoResString)"Terminal"; // Constant String
		public string EHubClientID => "FORWARDING_PORT_MESSAGE";
		public string DirectXTClientID => string.Empty;
		public string XmlNamespace => "CRESA/1";

		public bool AllowSendMessage => true;
		public bool AllowSendMessageAmendment => true;
		public bool AllowSendMessageWithdrawal => true;
		public bool AllowResetToOriginal => true;

		public bool OrderLogsByLocalTime => true;
		public bool RequireMessageAmendmentReason => false;

		public ICodeDescriptionPairList AmendmentOptions => null;
		public ICodeDescriptionPairList WidthdrawalOptions => null;
	}
}
