using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.ProductionRules.Integration;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;
using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Warehouse.Transactions.Business
{
	class AllocationEngineManager : IAllocationEngineManager
	{
		public AllocationEngineManager(
			IUserHaltableProductionRulesEngineService rulesEngine,
			IAllocationFactLoader factLoader,
			IAllocationResultProcessor resultProcessor)
		{
			RulesEngine = Argument.NotNull(rulesEngine, nameof(rulesEngine));
			FactLoader = Argument.NotNull(factLoader, nameof(factLoader));
			ResultProcessor = Argument.NotNull(resultProcessor, nameof(resultProcessor));
		}

		IUserHaltableProductionRulesEngineService RulesEngine { get; }
		IAllocationFactLoader FactLoader { get; }
		IAllocationResultProcessor ResultProcessor { get; }

		public AllocationResult Allocate(
			WhsPick pick,
			INotifications notifications,
			IPickStrategy pickStrategy,
			IEnumerable<WhsPickOrderedInventory> orderedInventories)
		{
			Argument.NotNull(pick, nameof(pick));
			Argument.NotNull(notifications, nameof(notifications));
			Argument.NotNull(pickStrategy, nameof(pickStrategy));
			Argument.NotNull(orderedInventories, nameof(orderedInventories));

			var result = AllocationResult.ErrorOrWarning;

			RulesEngine.RunRulesEngine(
				pick.Factory,
				notifications,
				RulesContextType.ProductWarehouseAllocation,
				ProductionRuleSetFilter.WithWarehouse(pick.WP_WW_Whs.ToGuid()),
				() =>
				{
					var inputFacts = FactLoader.GetAllocationFacts(orderedInventories, pickStrategy).ToArray();

					if (inputFacts.Length == 0)
					{
						result = AllocationResult.NoStockAllocated;
					}

					return inputFacts;
				},
				r =>
				{
					var processedResult = ResultProcessor.ProcessResults(pick, r.Facts.OfType<AllocationResultFact>().ToArray());
					result = processedResult.InventoryAllocated ? AllocationResult.AllocatedStock : AllocationResult.NoStockAllocated;

					return processedResult.Notification;
				});

			return result;
		}
	}
}
