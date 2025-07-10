using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PickStrategy : IPickStrategy
	{
		public PickStrategy(WhsPick pick)
		{
			Pick = Argument.NotNull(pick, nameof(pick));
		}

		protected WhsPick Pick { get; }

		#region CanInventoryBeAllocated

		public bool CanInventoryBeAllocated(WhsPickOrderedInventory orderedInventory, WhsInventoryView inventory)
		{
			return CanInventoryBeAllocatedCore(orderedInventory, inventory);
		}

		protected virtual bool CanInventoryBeAllocatedCore(WhsPickOrderedInventory orderedInventory, WhsInventoryView inventory)
		{
			var canBeAllocated = true;
			if (Pick.IsDisassemblyDynamicWorkOrderPick)
			{
				canBeAllocated = inventory.InDocketLine.BOMComponentLinks.Any();
			}
			return canBeAllocated;
		}

		#endregion

		#region GetAutoAllocateQuantity

		public ZDecimal GetAutoAllocateQuantity(ZDecimal orderedInventoryQtyShort, WhsPickAvailableInventory availableInventory)
		{
			return GetAutoAllocateQuantityCore(orderedInventoryQtyShort, availableInventory);
		}

		protected virtual ZDecimal GetAutoAllocateQuantityCore(ZDecimal orderedInventoryQtyShort, WhsPickAvailableInventory availableInventory)
		{
			return Math.Min(orderedInventoryQtyShort, GetQuantityUnPicked(availableInventory));
		}

		#endregion

		#region GetQuantityUnPicked

		public ZDecimal GetQuantityUnPicked(WhsPickAvailableInventory availableInventory)
		{
			var quantityUnPicked = ZDecimal.Zero;

			if (quantityUnPickedCache != null)
			{
				if (!quantityUnPickedCache.TryGetValue(availableInventory.PK, out quantityUnPicked))
				{
					quantityUnPickedCache[availableInventory.PK] = quantityUnPicked = availableInventory.QuantityUnPicked;
				}
			}
			else
			{
				quantityUnPicked = availableInventory.QuantityUnPicked;
			}

			return quantityUnPicked;
		}

		internal IDisposable CacheQuantityUnPicked()
		{
			SemaphoreManager manager = null;
			return new DisposableAction(
				() =>
				{
					manager = new SemaphoreManager(CacheQuantityUnPickedSemaphore);
					if (quantityUnPickedCache == null)
					{
						quantityUnPickedCache = new Dictionary<ZGuid, ZDecimal>();
					}
				},
				() =>
				{
					manager?.Dispose();

					if (!CacheQuantityUnPickedSemaphore.IsSuspended)
					{
						quantityUnPickedCache = null;
					}
				});
		}

		Dictionary<ZGuid, ZDecimal> quantityUnPickedCache;

		Semaphore CacheQuantityUnPickedSemaphore => cacheQuantityUnPickedSemaphore ?? (cacheQuantityUnPickedSemaphore = new Semaphore());
		Semaphore cacheQuantityUnPickedSemaphore;

		#endregion

		#region CanAllocateInQuantitiesDifferentToAutoAllocateQuantity

		public bool CanAllocateInQuantitiesDifferentToAutoAllocateQuantity(WhsPickAvailableInventory availableInventory)
		{
			return CanAllocateInQuantitiesDifferentToAutoAllocateQuantityCore(availableInventory);
		}

		protected virtual bool CanAllocateInQuantitiesDifferentToAutoAllocateQuantityCore(WhsPickAvailableInventory availableInventory) => true;

		#endregion

		#region OnInventoryPicked

		public void OnInventoryPicked(WhsPickOrderedInventory orderedInventory, WhsPickAvailableInventory availableInventory, ZDecimal qtyDelta)
		{
			OnInventoryPickedCore(orderedInventory, availableInventory, qtyDelta);
		}

		protected virtual void OnInventoryPickedCore(WhsPickOrderedInventory orderedInventory, WhsPickAvailableInventory availableInventory, ZDecimal qtyDelta)
		{
		}

		#endregion
	}
}
