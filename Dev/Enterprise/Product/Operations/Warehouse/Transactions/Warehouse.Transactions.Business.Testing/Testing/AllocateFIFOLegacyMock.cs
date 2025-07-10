using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	/// <summary>
	/// This class has been created to allow current tests that use Allocate Pick Algorithms for set up to continue to function, if AllocationEngine is enabled, without the need to implement a AllocationEngine mock for every test.
	/// </summary>
	public class AllocateFIFOLegacyMock : IAllocationEngineManager
	{
		public AllocationResult Allocate(WhsPick pick, INotifications notifications, IPickStrategy pickStrategy, IEnumerable<WhsPickOrderedInventory> orderedInventories)
		{
			Argument.NotNull(pick, nameof(pick));
			Argument.NotNull(orderedInventories, nameof(orderedInventories));

			var invAllocated = false;

			foreach (var oi in orderedInventories)
			{
				invAllocated |= AllocateOrderedInventoryWithLegacy(oi);
			}

			return invAllocated ? AllocationResult.AllocatedStock : AllocationResult.NoStockAllocated;
		}

		public static bool AllocateOrderedInventoryWithLegacy(WhsPickOrderedInventory orderedInventory)
		{
			Argument.NotNull(orderedInventory, nameof(orderedInventory));

			var quantityShort = orderedInventory.QuantityShort;
			var availableInventories = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().ToList();

			var allocatedQty = AllocateQty(orderedInventory, quantityShort, availableInventories);
			quantityShort -= allocatedQty;
			availableInventories.RemoveAll(sa => sa.QuantityUnPicked == 0);

			return allocatedQty > 0;
		}

		static ZDecimal AllocateQty(WhsPickOrderedInventory orderedInventory, ZDecimal orderedInventoryQtyShort, List<WhsPickAvailableInventory> availableInventoryList)
		{
			var allocatedQty = 0m;
			if (orderedInventory != null && orderedInventory.SupplierPart != null)
			{
				allocatedQty = AllocateQtyCore(orderedInventory, orderedInventoryQtyShort, availableInventoryList);
			}

			return allocatedQty;
		}

		static ZDecimal AllocateQtyCore(WhsPickOrderedInventory orderedInventory, ZDecimal orderedInventoryQtyShort, List<WhsPickAvailableInventory> availableInventoryList)
		{
			var totalAllocatedQty = 0m;
			var quantityShort = orderedInventoryQtyShort;

			var areaOverride = orderedInventory.Pick.WP_WA_DynamicPickAreaOverride;
			var availableInvs = areaOverride.IsValid
					? availableInventoryList.GroupBy(inv => inv.LocationPK).Where(g => orderedInventory.Factory.Load<WhsLocation>(g.Key).WLV_WA_PickingArea == areaOverride).SelectMany(k => k).ToList() // ??
					: availableInventoryList;

			foreach (var inventory in availableInvs)
			{
				var allocatedQtyFromInventoryLine = AllocateInventoryLine(orderedInventory, quantityShort, inventory, availableInvs);
				quantityShort -= allocatedQtyFromInventoryLine;
				totalAllocatedQty += allocatedQtyFromInventoryLine;

				if (quantityShort <= 0m)
				{
					break;
				}
			}

			return totalAllocatedQty;
		}

		static ZDecimal AllocateInventoryLine(WhsPickOrderedInventory orderedInventory, ZDecimal orderedInventoryQtyShort, WhsPickAvailableInventory availableInventory, IEnumerable<WhsPickAvailableInventory> availableInventoryList)
		{
			var allocatedQty = 0m;
			if (IsInventoryPickable(availableInventory))
			{
				if (orderedInventory.Pick.CurrentPickStrategy.GetAutoAllocateQuantity(orderedInventoryQtyShort, availableInventory) > 0m)
				{
					allocatedQty = AllocateInventoryLineCore(orderedInventory, orderedInventoryQtyShort, availableInventory);
				}
			}

			return allocatedQty;
		}

		static bool IsInventoryPickable(WhsPickAvailableInventory availableInventory)
			=> availableInventory.IsInventoryPickable
			&& !availableInventory.IsExpired
			&& availableInventory.IsAcceptableMinimumShelfLife;

		static ZDecimal AllocateInventoryLineCore(WhsPickOrderedInventory orderedInventory, ZDecimal orderedInventoryQtyShort, WhsPickAvailableInventory availableInventory)
		{
			var allocatedQty = GetMaxAllocateQty(orderedInventory.Pick.CurrentPickStrategy, orderedInventoryQtyShort, availableInventory);

			if (allocatedQty > 0)
			{
				// Make tests with differing docketline SOH quantities deterministic.
				availableInventory.Inventory.Sort(WhsInventoryViewSchema.Constants.WI_TotalUnits);

				// Make attribute neutral tests deterministic.
				// These are not client visible, so it doesn't make sense to do it in production.
				var sortBySerialNumber =
					(
						WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value &&
						!availableInventory.Inventory[0].WI_SerialNumber.IsEmpty // We are sorting serial numbers in the Serial Number grid and allocating based on serial number order.
					) ||
					WhsPickAvailableInventoryCollection.IsAttributeNeutralUsed(orderedInventory.Factory, orderedInventory.SupplierPartPK, orderedInventory.ClientPK, orderedInventory);

				if (sortBySerialNumber)
				{
					availableInventory.Inventory.Sort(WhsInventoryViewSchema.Constants.WI_SerialNumber);
				}
				else if (availableInventory.HasSerialNumber)
				{
					// Just to make tests deterministic
					availableInventory.Inventory.Sort(new SerialNumberCollectionFirstItemComparer());
				}
				availableInventory.PickLineQuantity += allocatedQty;
			}

			return allocatedQty;
		}

		static ZDecimal GetMaxAllocateQty(PickStrategy pickStrategy, ZDecimal orderedInventoryQtyShort, WhsPickAvailableInventory availableInventory)
			=> Math.Min(orderedInventoryQtyShort, pickStrategy.GetAutoAllocateQuantity(orderedInventoryQtyShort, availableInventory));
	}
}
