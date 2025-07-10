using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects
{
	sealed class CargoReceiptAdviceMessageInstructions : IMessageInstructions
	{
		public CargoReceiptAdviceMessageInstructions()
		{
			DocumentName = (NoResString)"Cargo Receipt Advice"; // programmatic constant
		}

		public string DocumentName { get; }
		public string TranslatedDocumentName => string.Empty;
		public string DataContext => Documents.DataContext.CargoReceiptAdvice;
		public string Recipient => (NoResString)"Booking Party";     // Constant String
		public string EHubClientID => "CARGOWISE_CRA";
		public string DirectXTClientID => string.Empty;
		public string XmlNamespace => "CargoReceiptAdvice/1";

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
