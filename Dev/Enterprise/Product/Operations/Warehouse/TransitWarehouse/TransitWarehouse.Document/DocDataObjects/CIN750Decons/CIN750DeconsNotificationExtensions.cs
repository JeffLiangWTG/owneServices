using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750DeconsNotificationExtensions : CIN750NotificationExtensions<CIN750DeconsNotification>
	{
		public CIN750DeconsNotificationExtensions(IDocument document, IMessageInstructions messageInstructions) : base(document, messageInstructions)
		{
		}

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications) => ContinueWithSendingMessageCore(CIN750NotificationValidation.GetDeconsNotificationValidationMessage(docDataObject), notifications);

		protected override string GetDataStoreName() => TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750DeconsFromDCN;
	}
}
