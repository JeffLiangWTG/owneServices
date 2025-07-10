using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class MessageSendingObjectCollection : NonPersistentBusinessObjectCollection<MessageSendingObject>
	{
		public MessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
		protected override bool AllowNewCore => false;
	}
}
