using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FZConcurrenceMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<FZConcurrenceMessageSendingObject>
	{
		public FZConcurrenceMessageSendingObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
