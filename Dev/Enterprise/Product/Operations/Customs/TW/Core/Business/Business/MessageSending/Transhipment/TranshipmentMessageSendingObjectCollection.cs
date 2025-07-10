using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class TranshipmentMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<TranshipmentMessageSendingObject>
	{
		public TranshipmentMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
		protected override bool AllowNewCore => false;
	}
}
