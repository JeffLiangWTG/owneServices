using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750ConsNotificationExtensions : CIN750NotificationExtensions<CIN750ConsNotification>
	{
		public CIN750ConsNotificationExtensions(IDocument document, IMessageInstructions messageInstructions) : base(document, messageInstructions)
		{
		}

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications) => ContinueWithSendingMessageCore(CIN750NotificationValidation.GetConsNotificationValidationMessage(docDataObject), notifications);

		protected override string GetDataStoreName() => TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750ConsFromDCN;
	}
}
