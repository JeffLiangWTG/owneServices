using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveProcessQueueCollection : NonPersistentBusinessObjectCollection<ActiveProcessQueue>
	{
		public ActiveProcessQueueCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Should not be creating new ActiveProcessQueue. This class is only used for binding.");
		}
	}
}
