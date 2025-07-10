using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsInventoryLookups : WhsInventoryViewLookups
	{
		public TrackingWhsInventoryLookups(TrackingWhsInventory parent)
			: base(parent)
		{
		}

		#region InventoryStatuses

		public InventoryStatus InventoryStatuses => Factory.GetCachedValue(
			"WhsInventoryViewLookups|WhsInventoryHeldCodeCollection",
			() =>
			{
				var statuses = new InventoryStatus();
				statuses.Sort();

				return statuses;
			});

		#endregion
	}
}
