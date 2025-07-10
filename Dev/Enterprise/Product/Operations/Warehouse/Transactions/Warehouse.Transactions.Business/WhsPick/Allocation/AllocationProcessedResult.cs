using CargoWise.ComponentModel;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class AllocationProcessedResult
	{
		public AllocationProcessedResult(INotification notification, bool inventoryAllocated)
		{
			Notification = notification;
			InventoryAllocated = inventoryAllocated;
		}

		public INotification Notification { get; }
		public bool InventoryAllocated { get; }
	}
}
