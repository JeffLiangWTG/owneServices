using System.Collections.Generic;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IAllocationFactLoader
	{
		IEnumerable<IEnumerable<IInputFact>> GetAllocationFacts(IEnumerable<WhsPickOrderedInventory> orderedInventories, IPickStrategy pickStrategy);
	}
}
