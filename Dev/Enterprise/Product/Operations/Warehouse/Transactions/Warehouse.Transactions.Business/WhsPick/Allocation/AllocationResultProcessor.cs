using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Warehouse.Transactions.Business
{
	class AllocationResultProcessor : IAllocationResultProcessor
	{
		public AllocationProcessedResult ProcessResults(WhsPick pick, IEnumerable<AllocationResultFact> results)
		{
			Argument.NotNull(pick, nameof(pick));
			Argument.NotNull(results, nameof(results));

			var invAllocated = false;

			var logMap = new Dictionary<(IWhsPickAvailableInventoryInternals AvailInv, Guid Id, string RuleName, DateTime? ExpiryFilter), decimal>();

			using (pick.SuspendUpdatingAllocationLog())
			{
				foreach (var resultsByOrderedInv in results
					.GroupBy(r => r.OrderedInventoryPK)
					.Select(g => new { OrderedInventory = GetOrderedInventoryForPK(pick, g), Results = g })
					.OrderBy(r => r.OrderedInventory.SerialNumber.IsEmpty))
				{
					var orderedInv = resultsByOrderedInv.OrderedInventory;
					var resultsGroupedByAvailableInventory = resultsByOrderedInv.Results.GroupBy(r => r.AvailableInventoryPK).ToArray();

					using (SuspendValidationOnOwners(orderedInv))
					using (var splitAndAvailableInventoryDeferrers = new DisposableList(resultsGroupedByAvailableInventory.Length))
					using (((IBusinessObjectCollection)orderedInv.PickLines).SuspendListChanged())
					{
						var firstInventory = true;
						foreach (var resultGroupedByAvailableInventory in resultsGroupedByAvailableInventory)
						{
							var availableInv = (WhsPickAvailableInventory)orderedInv.AvailableInventories.FindByPK(resultGroupedByAvailableInventory.Key)
								?? throw new ArgumentException("Could not find available inventory!");

							splitAndAvailableInventoryDeferrers.Add(availableInv.DeferSplittingByPackTypeAndPickLineQuantityValidation(validateOrderedInventory: firstInventory));
							invAllocated |= AllocateAvailableInventory(availableInv, resultGroupedByAvailableInventory, logMap);

							firstInventory = false;
						}
					}
				}
			}

			foreach (var logKvp in logMap)
			{
				var availableInventory = logKvp.Key.AvailInv;
				var ruleName = logKvp.Key.RuleName;
				var expiryFilter = logKvp.Key.ExpiryFilter;
				var quantity = logKvp.Value;

				var logProcessString =
					expiryFilter.HasValue
						? Res.GetString("25e1fef3-dfa8-40f8-afe5-8633e158cf3e", "Rule: {0} up to Expiry Date: {1}", ruleName, expiryFilter.Value.ToString("d"))
						: Res.GetString("cfca0618-b055-47dd-8dc9-88883bc380c1", "Rule: {0}", ruleName);

				availableInventory.UpdateAllocationLog(logProcessString, quantity);
			}

			return new AllocationProcessedResult(null, invAllocated);

			static WhsPickOrderedInventory GetOrderedInventoryForPK(WhsPick pick, IGrouping<Guid, AllocationResultFact> g)
				=> (WhsPickOrderedInventory)pick.OrderedInventories.FindByPK(g.Key) ?? throw new ArgumentException("Could not find ordered inventory!");
		}

		static bool AllocateAvailableInventory(
			WhsPickAvailableInventory availableInv,
			IGrouping<Guid, AllocationResultFact> resultGroupedByAvailableInventory,
			Dictionary<(IWhsPickAvailableInventoryInternals AvailInv, Guid Id, string RuleName, DateTime? ExpiryFilter), decimal> logMap)
		{
			var availableInvInternals = (IWhsPickAvailableInventoryInternals)availableInv;
			var invAllocated = false;

			foreach (var result in resultGroupedByAvailableInventory)
			{
				var pickLineQuantity = availableInv.PickLineQuantity + result.Quantity;
				var unableToAllocateQuantity = availableInvInternals.SetPickLineQuantity(pickLineQuantity, result.OrderLinePK);
				if (unableToAllocateQuantity != 0)
				{
					throw new ArgumentException($"Attempted to allocate {result.Quantity}, but failed to allocate {unableToAllocateQuantity}.");
				}

				var logMapKey = (availableInvInternals, result.ActivationID, result.RuleName, result.ExpiryDateFilter);
				logMap.TryGetValue(logMapKey, out var logQuantity);
				logQuantity += result.Quantity;
				logMap[logMapKey] = logQuantity;

				invAllocated = true;
			}

			return invAllocated;
		}

		static IDisposable SuspendValidationOnOwners(WhsPickOrderedInventory orderedInventory)
		{
			var owners = (IBusiness)orderedInventory.Owners;
			return new DisposableAction(owners.SuspendValidation, owners.ResumeValidation);
		}
	}
}
