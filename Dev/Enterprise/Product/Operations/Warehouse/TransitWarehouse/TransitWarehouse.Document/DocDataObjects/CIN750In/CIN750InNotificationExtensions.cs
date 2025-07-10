using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750InNotificationExtensions : CIN750NotificationExtensions<CIN750InNotification>
	{
		public CIN750InNotificationExtensions(IDocument document, IMessageInstructions messageInstructions) : base(document, messageInstructions)
		{
		}

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications) => ContinueWithSendingMessageCore(CIN750NotificationValidation.GetInNotificationValidationMessage(docDataObject), notifications);

		protected override string GetDataStoreName() => TransitDocDataConstants.ReceiveConsignmentDataStoreNames.CIN750InFromRCN;
	}
}
