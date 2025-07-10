using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ImportedOrderCollection : NonPersistentBusinessObjectCollection<ImportedOrder>
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
