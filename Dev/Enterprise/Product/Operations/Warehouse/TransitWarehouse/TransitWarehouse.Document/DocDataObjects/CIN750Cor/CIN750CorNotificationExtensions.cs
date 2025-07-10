using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750CorNotificationExtensions : CIN750NotificationExtensions<CIN750CorNotification>
	{
		public CIN750CorNotificationExtensions(IDocument document, IMessageInstructions messageInstructions) : base(document, messageInstructions)
		{
		}

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications) => ContinueWithSendingMessageCore(CIN750NotificationValidation.GetCorNotificationValidationMessage(docDataObject), notifications);

		protected override string GetDataStoreName() => TransitDocDataConstants.ReceiveConsignmentDataStoreNames.CIN750CorFromRCN;
	}
}
