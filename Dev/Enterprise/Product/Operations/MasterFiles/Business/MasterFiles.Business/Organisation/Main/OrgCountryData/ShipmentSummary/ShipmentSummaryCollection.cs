using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ShipmentSummaryCollection : NonPersistentBusinessObjectCollection<ShipmentSummary>
	{
		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
