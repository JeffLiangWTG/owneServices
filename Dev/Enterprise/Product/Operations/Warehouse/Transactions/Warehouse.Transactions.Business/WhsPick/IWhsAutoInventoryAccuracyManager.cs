using System.Collections.Generic;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsAutoInventoryAccuracyManager
	{
		void CreateAutoInventoryAccuracyManagementTasks(IReadOnlyCollection<WhsPickAvailableInventory> availableInventories, WhsWarehouse warehouse);
		void ClearAutoCreatedTasks(bool requireRollback);
	}
}
