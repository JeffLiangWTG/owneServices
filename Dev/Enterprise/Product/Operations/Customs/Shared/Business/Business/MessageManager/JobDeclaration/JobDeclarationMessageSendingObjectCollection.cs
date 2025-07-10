using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationMessageSendingObjectCollection<T> : NonPersistentBusinessObjectCollection<T> where T : JobDeclarationMessageSendingObject
	{
		public JobDeclarationMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore => false;
	}
}
