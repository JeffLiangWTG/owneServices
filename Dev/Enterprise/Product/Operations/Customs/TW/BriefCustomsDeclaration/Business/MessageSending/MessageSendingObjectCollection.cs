using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class MessageSendingObjectCollection : NonPersistentBusinessObjectCollection<MessageSendingObject>
	{
		public MessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore => false;
	}
}
