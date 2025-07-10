using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business
{
	public class AllocationAdjustmentCollection : NonPersistentBusinessObjectCollection<AllocationAdjustment>
	{
		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("AddNew not supported");
		}
	}
}
