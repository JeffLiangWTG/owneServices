using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Module;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsOrderShoppingCart : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TrackingWhsOrderShoppingCart(InventoryFilterBusinessObject filter, TrackingInventorySummaryCollection summaries)
		{
			this.filter = filter;
			RegisterEditableChildObject(this.filter);
			this.summaries = summaries;
			RegisterEditableChildObject(this.summaries);
		}

		InventoryFilterBusinessObject filter;
		TrackingInventorySummaryCollection summaries;

		public void Clear()
		{
			if (filter != null)
			{
				UnRegisterEditableChildObject(filter);
				filter = null;
			}
			if (summaries != null)
			{
				UnRegisterEditableChildObject(summaries);
				summaries = null;
			}
		}
	}
}
