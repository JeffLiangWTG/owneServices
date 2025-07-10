using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsAutoInventoryAccuracyManagementTaskCreator
	{
		void BeforeAutoTasksCreation(IReadOnlyCollection<WhsPickAvailableInventory> availableInventories, BusinessObjectFactory factory);
		void CreateInventoryAccuracyManagementTasksForAutoTouchCount(IEnumerable<IGrouping<WhsLocation, WhsPickAvailableInventory>> groupedAvailableInventoryLines, WhsWarehouse warehouse);
		void CreateAutoInventoryAccuracyManagementTasksForAutoZeroConfirmation(IEnumerable<IGrouping<WhsLocation, WhsPickAvailableInventory>> groupedAvailableInventoryLines, WhsWarehouse warehouse);
		void ClearAutoCreatedTasks(bool requireRollback);
	}
}
