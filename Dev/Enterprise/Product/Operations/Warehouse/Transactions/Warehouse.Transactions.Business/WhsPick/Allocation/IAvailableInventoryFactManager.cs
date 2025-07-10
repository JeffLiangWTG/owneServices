using System;
using System.Collections.Generic;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IAvailableInventoryFactManager
	{
		void Register(string key, IAvailableInventoryFact inventory);
		bool HasKeyRegistered(string key);
		IEnumerable<IAvailableInventoryFact> Update(string key, Guid pkToExclude, decimal qtyToReduce);
	}
}
