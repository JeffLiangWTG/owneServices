using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IAllocationEngineManager
	{
		AllocationResult Allocate(WhsPick pick, INotifications notifications, IPickStrategy pickStrategy, IEnumerable<WhsPickOrderedInventory> orderedInventories);
	}
}
