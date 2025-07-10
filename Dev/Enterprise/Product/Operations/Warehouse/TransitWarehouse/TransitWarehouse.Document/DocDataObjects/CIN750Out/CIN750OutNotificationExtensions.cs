using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750OutNotificationExtensions : CIN750NotificationExtensions<CIN750OutNotification>
	{
		public CIN750OutNotificationExtensions(IDocument document, IMessageInstructions messageInstructions) : base(document, messageInstructions)
		{
		}

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications) => ContinueWithSendingMessageCore(CIN750NotificationValidation.GetOutNotificationValidationMessage(docDataObject), notifications);

		protected override string GetDataStoreName() => TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750OutFromDCN;
	}
}
