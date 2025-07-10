using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transit.Document
{
	public sealed class CIN750MessageInstructions : IMessageInstructions
	{
		public CIN750MessageInstructions(CIN750NotificationDocData docData)
		{
			DocumentName = docData.DocumentName;
			DataContext = docData.DataContext;
			XmlNamespace = docData.XmlNamespace;
		}

		public string DocumentName { get; set; }

		public string TranslatedDocumentName => string.Empty;

		public string DataContext { get; set; }

		public string Recipient => (NoResString)"Terminal";

		public string EHubClientID => "CIN_WAREHOUSE";
		public string DirectXTClientID => string.Empty;

		public string XmlNamespace { get; set; }

		public bool AllowSendMessage => true;

		public bool AllowSendMessageAmendment => false;

		public bool AllowSendMessageWithdrawal => true;

		public bool AllowResetToOriginal => true;

		public bool OrderLogsByLocalTime => true;

		public bool RequireMessageAmendmentReason => false;

		public ICodeDescriptionPairList AmendmentOptions => null;

		public ICodeDescriptionPairList WidthdrawalOptions => null;
	}
}
