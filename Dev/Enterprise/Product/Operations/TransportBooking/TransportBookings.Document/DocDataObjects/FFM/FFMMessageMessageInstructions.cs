using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Core;
using TransportBookingsDocumentDataContext = Enterprise.TransportBookings.Document.DataContext;

namespace Enterprise.TransportBookings.Document
{
	public sealed class FFMMessageMessageInstructions : IMessageInstructions
	{
		public FFMMessageMessageInstructions()
		{
			DocumentName = (NoResString)"FFM send request";
		}

		public string DocumentName { get; }
		public string TranslatedDocumentName => string.Empty;
		public string DataContext => TransportBookingsDocumentDataContext.FFMMessageRequest;
		public string Recipient => (NoResString)"Terminal";
		public string EHubClientID => string.Empty;
		public string DirectXTClientID => "MILANS_SMARTCITY";
		public string XmlNamespace => "FFM/8";

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
