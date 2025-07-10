using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;

namespace Enterprise.Warehouse.Transactions.Business
{
	class AvailableInventoryFactManager : IAvailableInventoryFactManager
	{
		public void Register(string key, IAvailableInventoryFact inventory)
		{
			Argument.NotNull(key, nameof(key));
			Argument.NotNull(inventory, nameof(inventory));

			if (!AvailableInventoryMap.TryGetValue(key, out var inventoryList))
			{
				AvailableInventoryMap[key] = inventoryList = new List<IAvailableInventoryFact>();
			}

			inventoryList.Add(inventory);
		}

		public bool HasKeyRegistered(string key) => AvailableInventoryMap.ContainsKey(key);

		public IEnumerable<IAvailableInventoryFact> Update(string key, Guid pkToExclude, decimal qtyToReduce)
		{
			IEnumerable<IAvailableInventoryFact> updatedInventory = null;

			if (!UpdatingRelatedInventorySemaphore.IsSuspended)
			{
				Argument.NotNull(key, nameof(key));

				if (!AvailableInventoryMap.TryGetValue(key, out var inventoryList))
				{
					throw new ArgumentException("Unregistered key!");
				}

				updatedInventory = inventoryList.ToArray();
				using (new SemaphoreManager(UpdatingRelatedInventorySemaphore))
				{
					foreach (var inventory in updatedInventory.Where(i => i.PK != pkToExclude))
					{
						inventory.DecreaseQuantity(qtyToReduce);
					}
				}
			}

			return updatedInventory ?? Enumerable.Empty<IAvailableInventoryFact>();
		}

		Dictionary<string, List<IAvailableInventoryFact>> AvailableInventoryMap
		{
			get { return availableInventoryMap ?? (availableInventoryMap = new Dictionary<string, List<IAvailableInventoryFact>>(StringComparer.OrdinalIgnoreCase)); }
		}

		Dictionary<string, List<IAvailableInventoryFact>> availableInventoryMap;

		Semaphore UpdatingRelatedInventorySemaphore
		{
			get { return updatingRelatedInventorySemaphore ?? (updatingRelatedInventorySemaphore = new Semaphore()); }
		}

		Semaphore updatingRelatedInventorySemaphore;
	}
}
