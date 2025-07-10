using System.Collections.Generic;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IAllocationResultProcessor
	{
		AllocationProcessedResult ProcessResults(WhsPick pick, IEnumerable<AllocationResultFact> results);
	}
}
