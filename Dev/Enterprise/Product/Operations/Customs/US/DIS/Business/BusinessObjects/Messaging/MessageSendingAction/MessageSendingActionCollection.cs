using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.DIS.Business
{
	public class MessageSendingActionCollection : MessageSendingActionCollectionBase<MessageSendingAction, DISDocument>
	{
		public MessageSendingActionCollection(DISHostWrapper hostWrapper)
			: base(hostWrapper)
		{
		}

		protected override IMessageSendingActionBase CreateElement(IDISDocumentBase disDocument) => new MessageSendingAction(disDocument as DISDocument);

		protected override DISMessageManagerBase GetMessageManager(IDISDocumentBase disDocument) => new MessageManager(disDocument as DISDocument);
	}
}
