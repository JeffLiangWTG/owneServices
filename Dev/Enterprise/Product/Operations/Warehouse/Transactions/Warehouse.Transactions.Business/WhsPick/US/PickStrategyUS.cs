using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PickStrategyUS : PickStrategy
	{
		public PickStrategyUS(WhsPick pick)
			: base(pick)
		{
		}

		#region CanInventoryBeAllocatedCore

		protected override bool CanInventoryBeAllocatedCore(WhsPickOrderedInventory orderedInventory, WhsInventoryView inventory)
		{
			return base.CanInventoryBeAllocatedCore(orderedInventory, inventory) && CanAllocateFullPackageGroupID(orderedInventory, inventory);
		}

		#region CanAllocateFullPackageGroupID

		bool CanAllocateFullPackageGroupID(WhsPickOrderedInventory orderedInventory, WhsInventoryView inventory)
		{
			bool result = false;

			var packageGroupID = orderedInventory.PackageGroupId;
			if ((packageGroupID.IsEmpty || packageGroupID.EqualsIgnoringCase(inventory.PackageGroupId)) &&
				AreAllPackageOfOrderedInventoryMatched(orderedInventory, inventory) &&
				IsInventoryPerPackageQtyPickable(orderedInventory.QuantityOrdered, inventory))
			{
				result = true;
			}

			return result;
		}

		#region AreAllPackageOfOrderedInventoryMatched

		bool AreAllPackageOfOrderedInventoryMatched(WhsPickOrderedInventory orderedInventory, WhsInventoryView inventory)
		{
			var matched = true;
			if (!inventory.PackageGroupId.IsEmpty)
			{
				// look throught all the inventories with same PackageGroupId, see if all products in the same PackageGroupId are ordered together
				var firstOwner = orderedInventory.Owners.Count > 0 ? orderedInventory.Owners[0] : null;
				if (firstOwner != null)
				{
					var packageGroupId = inventory.PackageGroupId;
					var pick = orderedInventory.Pick;
					var warehouse = pick.Warehouse;
					var groupContent = USBondedHelper.GetDistinctPackageGroupIDInventories(pick.Factory, warehouse, packageGroupId);

					foreach (var packedItem in groupContent.Keys)
					{
						bool isOrdered = false;
						foreach (WhsPickOrderedInventory oi in pick.OrderedInventories)
						{
							if ((oi.SupplierPart.PK == packedItem.ProductPK) &&
								(oi.PartAttrib1.IsEmpty || oi.PartAttrib1.EqualsIgnoringCase(packedItem.PartAttrib1)) &&
								(oi.PartAttrib2.IsEmpty || oi.PartAttrib2.EqualsIgnoringCase(packedItem.PartAttrib2)) &&
								(oi.PartAttrib3.IsEmpty || oi.PartAttrib3.EqualsIgnoringCase(packedItem.PartAttrib3)) &&
								(oi.SerialNumber.IsEmpty || oi.SerialNumber.EqualsIgnoringCase(packedItem.SerialNumber)) &&
								(oi.ExpiryDate.IsEmpty || oi.ExpiryDate == packedItem.ExpiryDate) &&
								(oi.PackingDate.IsEmpty || oi.PackingDate == packedItem.PackingDate) &&
								(oi.PackageGroupId.IsEmpty || oi.PackageGroupId.EqualsIgnoringCase(packageGroupId)) &&
								oi.QuantityOrdered >= packedItem.PerPackageQty)
							{
								isOrdered = true;
								break;
							}
						}

						matched = matched && isOrdered;

						if (!matched)
						{
							break;
						}
					}
				}
			}

			return matched;
		}

		#endregion

		#region IsInventoryPerPackageQtyPickable

		bool IsInventoryPerPackageQtyPickable(decimal quantityOrdered, WhsInventoryView inventory)
		{
			return inventory.PerPackageQty.IsEmpty || (quantityOrdered >= inventory.PerPackageQty);
		}

		#endregion

		#endregion

		#endregion

		#region GetAutoAllocateQuantityCore

		protected override ZDecimal GetAutoAllocateQuantityCore(ZDecimal orderedInventoryQtyShort, WhsPickAvailableInventory availableInventory)
		{
			var packageGroupID = availableInventory.PackageGroupId;
			var perPackageQty = availableInventory.PerPackageQty;

			return !packageGroupID.IsEmpty || perPackageQty != 0m
				? GetAutoAllocateQtyToBePicked(availableInventory, packageGroupID)
				: base.GetAutoAllocateQuantityCore(orderedInventoryQtyShort, availableInventory);
		}

		ZDecimal GetAutoAllocateQtyToBePicked(WhsPickAvailableInventory availableInventory, ZString packageGroupID)
		{
			var result = 0m;
			var group = GetMatchingPackageGroup(availableInventory, packageGroupID);
			if (group != null)
			{
				var productWithAttributes = ProductWithAttributes.GetProductWithAttributes(availableInventory);
				var packedItem = group.PackedItems.FirstOrDefault(i => i.Key.Equals(productWithAttributes));
				if (packedItem != null && group.PackagesToPick > packedItem.PackagesPicked)
				{
					var locationKey = GetLocationHashKey(availableInventory);
					var availableQty = availableInventory.QuantityUnPicked;
					var quantityThatCanBePicked = GetQuantityThatCanBePicked(group, packedItem, locationKey);

					result = Math.Min(availableQty, quantityThatCanBePicked);
				}
			}

			return result;
		}

		#region GetMatchingPackageGroup

		PackageGroup GetMatchingPackageGroup(WhsPickAvailableInventory availableInventory, ZString packageGroupID)
		{
			var groupKey = (!packageGroupID.IsEmpty) ? packageGroupID : GetAttributesHashKey(availableInventory);
			AvailablePackagesAllocations.TryGetValue(groupKey, out var group);
			return group;
		}

		#endregion

		#region GetAttributesHashKey

		ZString GetAttributesHashKey(WhsPickAvailableInventory availableInventory)
		{
			var perPackageQty = GetPerPackageQty(availableInventory);
			return availableInventory.Client.PK.ToString() +
				availableInventory.SupplierPart.PK.ToString() +
				AttributeComparer.GetHashCodeForConsolidation(availableInventory) +
				perPackageQty;
		}

		static ZDecimal GetPerPackageQty(WhsPickAvailableInventory availableInventory)
		{
			return (availableInventory.PerPackageQty != 0)
				? availableInventory.PerPackageQty
				: Math.Pow(0.1, availableInventory.SupplierPart.OP_CountDecimalPlaces); // to treat "normal" inventory as packages so that any quantity can be picked;
		}

		#endregion

		#region GetLocationHashKey

		ZString GetLocationHashKey(WhsPickAvailableInventory availableInventory)
		{
			return availableInventory.Location?.WLV_LocationString ?? ZString.Empty + availableInventory.PalletID + availableInventory.InventoryStatus;
		}

		#endregion

		#region GetQuantityThatCanBePicked

		ZDecimal GetQuantityThatCanBePicked(PackageGroup group, PackedItem packedItem, ZString locationKey)
		{
			var maxPackagesPickedPerLocation = new Dictionary<ZString, ZInt>();
			foreach (var item in group.PackedItems)
			{
				foreach (var location in item.PickLocations)
				{
					if (location.Key != locationKey)
					{
						maxPackagesPickedPerLocation.TryGetValue(location.Key, out var prevMaxPackagesPickedPerLocation);
						maxPackagesPickedPerLocation[location.Key] = Math.Max(prevMaxPackagesPickedPerLocation, location.Value);
					}
				}
			}

			packedItem.PickLocations.TryGetValue(locationKey, out var packagesAlreadyPicked);
			return (group.PackagesToPick - maxPackagesPickedPerLocation.Sum(l => l.Value) - packagesAlreadyPicked) * packedItem.PerPackageQty;
		}

		#endregion

		#region AvailablePackagesAllocations

		Dictionary<ZString, PackageGroup> AvailablePackagesAllocations
		{
			get
			{
				if (availablePackagesAllocations == null)
				{
					availablePackagesAllocations = new Dictionary<ZString, PackageGroup>();
					var orderedItems = GetOrderedItems(availablePackagesAllocations);
					AddAvailableItemsToAllocations(availablePackagesAllocations);
					RecordAlreadyPickedInventory();
					DetermineBestAllocations(availablePackagesAllocations, orderedItems);
				}
				return availablePackagesAllocations;
			}
		}

		Dictionary<ZString, PackageGroup> availablePackagesAllocations;

		#region GetOrderedItems

		Dictionary<WhsPickOrderedInventory, OrderedItem> GetOrderedItems(Dictionary<ZString, PackageGroup> packagesForAllocation)
		{
			var result = new Dictionary<WhsPickOrderedInventory, OrderedItem>();
			foreach (WhsPickOrderedInventory orderedInventory in Pick.OrderedInventories)
			{
				if (!orderedInventory.PackageGroupId.IsEmpty)
				{
					AddPackageGroupForAllocations(packagesForAllocation, orderedInventory);
				}
				else
				{
					var orderedItem = new OrderedItem(orderedInventory);
					orderedItem.QtyOrdered = orderedInventory.QuantityShort;
					result.Add(orderedInventory, orderedItem);
				}
			}

			RemoveSomePackagesForAllocationToPickFullPackages(packagesForAllocation, result);
			return result;
		}

		#region AddPackageGroupForAllocations

		void AddPackageGroupForAllocations(Dictionary<ZString, PackageGroup> packagesForAllocation, WhsPickOrderedInventory orderedInventory)
		{
			var packageGroup = GetOrCreatePackageGroup(packagesForAllocation, orderedInventory.PackageGroupId);
			var orderedProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(orderedInventory);
			var matchingInventories = packageGroup.PackedItems.Where(i => ProductWithAttributes.IsMatchingWithEmptyCheck(orderedProductWithAttributes, i.Key));
			// This part will not handle some complicated cases, but hopefully those casses will never occur... Good luck to whoever is going to implement that..
			var perPackageQty = matchingInventories.Sum(i => i.PerPackageQty);
			if (perPackageQty != 0) // should never be 0 when Package Group ID is not empty.
			{
				var packagesOrdered = (ZInt)(orderedInventory.QuantityOrdered / perPackageQty); // if this division has a left overs, then congratulations it is one of those complicated cases.
																								// record what was ordered
				foreach (var matchingInventory in matchingInventories)
				{
					AddPickOrderedInventory(matchingInventory, orderedInventory, packagesOrdered);
				}
			}
		}

		PackageGroup GetOrCreatePackageGroup(Dictionary<ZString, PackageGroup> packagesForAllocation, ZString packageGroupID)
		{
			if (!packagesForAllocation.TryGetValue(packageGroupID, out var packageGroup))
			{
				packageGroup = new PackageGroup();
				var packageGroupIDContent = USBondedHelper.GetDistinctPackageGroupIDInventories(Pick.Factory, Pick.Warehouse, packageGroupID);
				foreach (var packageContentItem in packageGroupIDContent)
				{
					packageGroup.PackedItems.Add(new PackedItem(packageContentItem.Key, packageContentItem.Value));
				}
				packagesForAllocation.Add(packageGroupID, packageGroup);
			}
			return packageGroup;
		}

		static void AddPickOrderedInventory(PackedItem packedItem, WhsPickOrderedInventory orderedInventory, ZInt packagesOrdered)
		{
			if (!packedItem.PickOrderedInventories.TryGetValue(orderedInventory, out var orderedInventoryCache))
			{
				orderedInventoryCache = new OrderedInventoryCache();
				packedItem.PickOrderedInventories.Add(orderedInventory, orderedInventoryCache);
			}

			orderedInventoryCache.CurrentValue += packagesOrdered;
			orderedInventoryCache.BestValueCached += packagesOrdered;
		}

		#endregion

		#region RemoveSomePackagesForAllocationToPickFullPackages

		static void RemoveSomePackagesForAllocationToPickFullPackages(Dictionary<ZString, PackageGroup> packagesForAllocation, Dictionary<WhsPickOrderedInventory, OrderedItem> orderedProductsWithoutPackageGroupID)
		{
			// event out packagesForAllocation to pick full packages
			foreach (var packageGroup in packagesForAllocation.Values)
			{
				foreach (var packedItem in packageGroup.PackedItems)
				{
					var packsReserved = packageGroup.PackagesOrdered - packedItem.PacksOrdered;
					if (packsReserved > 0)
					{
						foreach (var orderedItem in orderedProductsWithoutPackageGroupID.ToArray())
						{
							if (orderedItem.Key.Client.PK == packedItem.Key.ClientPK &&
								orderedItem.Key.SupplierPart.PK == packedItem.Key.ProductPK &&
								(orderedItem.Key.PartAttrib1.IsEmpty || orderedItem.Key.PartAttrib1.EqualsIgnoringCase(packedItem.Key.PartAttrib1)) &&
								(orderedItem.Key.PartAttrib2.IsEmpty || orderedItem.Key.PartAttrib2.EqualsIgnoringCase(packedItem.Key.PartAttrib2)) &&
								(orderedItem.Key.PartAttrib3.IsEmpty || orderedItem.Key.PartAttrib3.EqualsIgnoringCase(packedItem.Key.PartAttrib3)) &&
								// It's not possible to setup a scenario where this check matters because Serial makes the Packed Item unique,
								// so you can't have more than one Package when using serials. This check is for consistency only (there is no test for this).
								(orderedItem.Key.SerialNumber.IsEmpty || orderedItem.Key.SerialNumber.EqualsIgnoringCase(packedItem.Key.SerialNumber)) &&
								(orderedItem.Key.ExpiryDate.IsEmpty || orderedItem.Key.ExpiryDate == packedItem.Key.ExpiryDate) &&
								(orderedItem.Key.PackingDate.IsEmpty || orderedItem.Key.PackingDate == packedItem.Key.PackingDate) &&
								(orderedItem.Key.BondedEntryKey.IsEmpty || orderedItem.Key.BondedEntryKey == packedItem.Key.BondedEntryKey))
							{
								var itemsToTake = (ZInt)Math.Min(packsReserved, orderedItem.Value.QtyOrdered / packedItem.PerPackageQty);
								orderedProductsWithoutPackageGroupID[orderedItem.Key].QtyOrdered -= itemsToTake * packedItem.PerPackageQty;
								if (orderedProductsWithoutPackageGroupID[orderedItem.Key].QtyOrdered <= 0)
								{
									orderedProductsWithoutPackageGroupID.Remove(orderedItem.Key);
								}
								AddPickOrderedInventory(packedItem, orderedItem.Key, itemsToTake);
								packsReserved -= itemsToTake;
								if (packsReserved <= 0)
								{
									break;
								}
							}
						}
						// if packsReserved > 0 - Order validation didn't work properly.
					}
				}
			}
		}

		#endregion

		#endregion

		#region AddAvailableItemsToAllocations

		void AddAvailableItemsToAllocations(Dictionary<ZString, PackageGroup> packagesForAllocation)
		{
			var processedInventory = new HashSet<WhsInventoryView>();
			foreach (WhsPickOrderedInventory orderedInventory in Pick.OrderedInventories)
			{
				foreach (WhsPickAvailableInventory availableInventory in orderedInventory.AvailableInventories)
				{
					var firstInventory = (WhsInventoryView)availableInventory.Inventory.FirstOrDefault();
					if (firstInventory != null && !processedInventory.Contains(firstInventory)) // same inventory can be part of multiple available inventory records
					{
						if (!availableInventory.PackageGroupId.IsEmpty)
						{
							AddPackageGroupForAllocations(packagesForAllocation, availableInventory);
						}
						else
						{
							AddSingleProductPackagesForAllocations(packagesForAllocation, availableInventory);
						}

						processedInventory.UnionWith(availableInventory.Inventory.Cast<WhsInventoryView>());
					}
				}
			}
		}

		#region AddPackageGroupForAllocations

		void AddPackageGroupForAllocations(Dictionary<ZString, PackageGroup> packagesForAllocation, WhsPickAvailableInventory availableInventory)
		{
			var packageGroup = GetOrCreatePackageGroup(packagesForAllocation, availableInventory.PackageGroupId);
			var availableProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(availableInventory);
			var matchingPackageItem = packageGroup.PackedItems.Single(i => i.Key.Equals(availableProductWithAttributes));
			if (availableInventory.PerPackageQty != 0) // should never be 0 when Package Group ID is not empty.
			{
				matchingPackageItem.PacksAvailable += (long)(availableInventory.QuantityAvailableToPick / availableInventory.PerPackageQty); // should always be int value
			}
		}

		#endregion

		#region AddSingleProductPackagesForAllocations

		void AddSingleProductPackagesForAllocations(Dictionary<ZString, PackageGroup> packagesForAllocation, WhsPickAvailableInventory availableInventory)
		{
			var availableProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(availableInventory);
			var hashCode = GetAttributesHashKey(availableInventory);
			var perPackageQty = GetPerPackageQty(availableInventory);

			if (!packagesForAllocation.TryGetValue(hashCode, out var packageGroup))
			{
				packageGroup = new PackageGroup();
				var packedItem = new PackedItem(availableProductWithAttributes, perPackageQty);
				packageGroup.PackedItems.Add(packedItem);
				packagesForAllocation.Add(hashCode, packageGroup);
			}

			packageGroup.PackedItems[0].PacksAvailable += (long)(availableInventory.QuantityAvailableToPick / perPackageQty); // should always be int value
		}

		#endregion

		#endregion

		#region RecordAlreadyPickedInventory

		void RecordAlreadyPickedInventory()
		{
			foreach (WhsPickOrderedInventory orderedInventory in Pick.OrderedInventories)
			{
				if (orderedInventory.PickLineQuantity > 0m)
				{
					foreach (WhsPickAvailableInventory availableInventory in orderedInventory.AvailableInventories)
					{
						var qtyPicked = availableInventory.PickLineQuantity;
						if (qtyPicked > 0m)
						{
							OnInventoryPicked(orderedInventory, availableInventory, qtyPicked);
						}
					}
				}
			}
		}

		#endregion

		#region DetermineBestAllocations

		void DetermineBestAllocations(Dictionary<ZString, PackageGroup> allocations, Dictionary<WhsPickOrderedInventory, OrderedItem> orderedItems)
		{
			if (orderedItems.Count > 0)
			{
				var seperatedDictionariesByEntryKey = new Dictionary<string, Dictionary<WhsPickOrderedInventory, OrderedItem>>();
				foreach (var item in orderedItems)
				{
					var index = item.Key.BondedEntryKey.LastIndexOf('-');
					var entryNumber = item.Key.BondedEntryKey.Left(index);

					if (!seperatedDictionariesByEntryKey.TryGetValue(entryNumber, out var seperatedOrderedItemsByEntryNumber))
					{
						seperatedOrderedItemsByEntryNumber = new Dictionary<WhsPickOrderedInventory, OrderedItem>();
						seperatedDictionariesByEntryKey.Add(entryNumber, seperatedOrderedItemsByEntryNumber);
					}
					seperatedOrderedItemsByEntryNumber.Add(item.Key, item.Value);
				}

				foreach (var seperatedOrderedItems in seperatedDictionariesByEntryKey.Values)
				{
					DetermineBestAllocations_PerEntryNumber(allocations, seperatedOrderedItems);
				}
			}
			else
			{
				DetermineBestAllocations_PerEntryNumber(allocations, orderedItems);
			}
		}

		// We split by entry key for performance. ie two keys with 2 products each will have a factor of 4 recursion
		// where as split keys will have 2 x factor of 2.
		// 2 x 2^2 (8) is still less than 1 x 2^4 (16). more complexity would make this worse.
		void DetermineBestAllocations_PerEntryNumber(Dictionary<ZString, PackageGroup> allocations, Dictionary<WhsPickOrderedInventory, OrderedItem> orderedItems)
		{
			var orderedItemsSorted = orderedItems.OrderByDescending(i => i.Value.QtyOrdered).ToArray();
			foreach (var orderedItem in orderedItemsSorted)
			{
				AllocateOrderedItems(allocations, orderedItem);
			}

			foreach (var allocation in allocations)
			{
				allocation.Value.PackagesToPick = Math.Min(allocation.Value.PackagesOrdered, allocation.Value.PackagesAvailable);
			}
		}

		void AllocateOrderedItems(Dictionary<ZString, PackageGroup> allocations, KeyValuePair<WhsPickOrderedInventory, OrderedItem> currentOrderedItem)
		{
			minQtyLeftToAllocate = decimal.MaxValue;
			var currentlyAvailableItems = GetAvailableToPickItems(allocations, currentOrderedItem);
			var qtyOrdered = currentOrderedItem.Value.QtyOrdered;
			var qtyAvailable = currentlyAvailableItems.Sum(a => a.Value.PackedItems.Sum(p => p.PacksAvailable * p.PerPackageQty));
			if (qtyOrdered < qtyAvailable)
			{
				var success = AllocateAvailableItemsRecursively(currentOrderedItem, currentOrderedItem.Value.QtyOrdered, currentlyAvailableItems, 0, tryToFindBestAllocation: true);
				if (!success)
				{
					// populate allocations from cache if order cannot be fulfilled fully
					foreach (var orderedInventory in currentlyAvailableItems.SelectMany(a => a.Value.PackedItems).SelectMany(p => p.PickOrderedInventories))
					{
						orderedInventory.Value.CurrentValue = orderedInventory.Value.BestValueCached;
					}
				}
			}
			else
			{
				AllocateAvailableItemsRecursively(currentOrderedItem, currentOrderedItem.Value.QtyOrdered, currentlyAvailableItems, 0, tryToFindBestAllocation: false);
			}
		}

		ZDecimal minQtyLeftToAllocate;

		#region GetAvailableToPickItems

		KeyValuePair<ZString, PackageGroup>[] GetAvailableToPickItems(Dictionary<ZString, PackageGroup> allocations, KeyValuePair<WhsPickOrderedInventory, OrderedItem> currentOrderedItem)
		{
			var result = new List<KeyValuePair<ZDecimal, KeyValuePair<ZString, PackageGroup>>>();
			var orderedProductWithAttributes = ProductWithAttributes.GetProductWithAttributes(currentOrderedItem.Key);
			foreach (var allocation in allocations)
			{
				var matchingInventories = GetMatchingInventories(orderedProductWithAttributes, allocation.Value.PackedItems);
				if (matchingInventories.Count > 0)
				{
					var perPackageQty = matchingInventories.Sum(i => i.PerPackageQty);
					var packagesAvailable = allocation.Value.PackagesAvailable - matchingInventories.Max(i => i.PacksOrdered);
					if (packagesAvailable > 0 && perPackageQty <= currentOrderedItem.Value.QtyOrdered)
					{
						result.Add(new KeyValuePair<ZDecimal, KeyValuePair<ZString, PackageGroup>>(perPackageQty, new KeyValuePair<ZString, PackageGroup>(allocation.Key, allocation.Value)));
					}
				}
			}

			return result.OrderByDescending(k => k.Key).Select(k => k.Value).ToArray();
		}

		#endregion

		#region GetMatchingInventories

		static List<PackedItem> GetMatchingInventories(ProductWithAttributes orderedProductWithAttributes, List<PackedItem> packedItems)
		{
			return packedItems.Where(i => ProductWithAttributes.IsMatchingWithEmptyCheck(orderedProductWithAttributes, i.Key)).ToList();
		}

		#endregion

		#region AllocateAvailableItemsRecursively

		bool AllocateAvailableItemsRecursively(KeyValuePair<WhsPickOrderedInventory, OrderedItem> currentOrderedItem, ZDecimal qtyLeftToAllocate, KeyValuePair<ZString, PackageGroup>[] availableItems, int availableItemIndex, bool tryToFindBestAllocation)
		{
			var success = false;
			if (availableItemIndex < availableItems.Length)
			{
				var currentAvailableItem = availableItems[availableItemIndex];
				var matchingInventories = GetMatchingInventories(currentOrderedItem.Value.ProductWithAttributes, currentAvailableItem.Value.PackedItems);
				var perPackageQty = matchingInventories.Sum(i => i.PerPackageQty);
				if (perPackageQty != 0) // should never be 0
				{
					var packagesAllocated = (int)Math.Min(qtyLeftToAllocate / perPackageQty, currentAvailableItem.Value.PackagesAvailable);
					for (int i = packagesAllocated; i >= 0 && !success; i--)
					{
						// temp allocate items
						if (i > 0)
						{
							foreach (var matchingInventory in matchingInventories)
							{
								if (!matchingInventory.PickOrderedInventories.TryGetValue(currentOrderedItem.Key, out var orderedInventoryCache))
								{
									matchingInventory.PickOrderedInventories[currentOrderedItem.Key] = orderedInventoryCache = new OrderedInventoryCache();
								}

								orderedInventoryCache.CurrentValue += i;
							}
						}

						var tempQtyLeftToAllocate = qtyLeftToAllocate - i * perPackageQty;
						var lastAvailable = availableItems.Length == availableItemIndex + 1;
						if (tempQtyLeftToAllocate == 0 || lastAvailable && !tryToFindBestAllocation)
						{
							success = true;
						}
						else if (lastAvailable)
						{
							if (tempQtyLeftToAllocate < minQtyLeftToAllocate)
							{
								minQtyLeftToAllocate = tempQtyLeftToAllocate;

								// cache currently best allocations
								foreach (var orderedInventory in availableItems.SelectMany(a => a.Value.PackedItems).SelectMany(p => p.PickOrderedInventories))
								{
									orderedInventory.Value.BestValueCached = orderedInventory.Value.CurrentValue;
								}
							}
						}

						if (!success)
						{
							// allocate next available inventory
							success = AllocateAvailableItemsRecursively(currentOrderedItem, tempQtyLeftToAllocate, availableItems, availableItemIndex + 1, tryToFindBestAllocation);
						}

						// rollback allocations
						if (i > 0 && !success)
						{
							foreach (var matchingInventory in matchingInventories)
							{
								matchingInventory.PickOrderedInventories[currentOrderedItem.Key].CurrentValue -= i;
							}
						}
					}
				}
			}
			return success;
		}

		#endregion

		#endregion

		#region PackageGroup class

		class PackageGroup
		{
			#region PackedItems

			public List<PackedItem> PackedItems
			{
				get { return packedItems ?? (packedItems = new List<PackedItem>()); }
			}

			List<PackedItem> packedItems;

			#endregion

			#region PackagesToPick

			public long PackagesToPick { get; set; }

			#endregion

			#region PackagesOrdered

			public long PackagesOrdered
			{
				get { return PackedItems.Max(i => i.PacksOrdered); }
			}

			#endregion

			#region PackagesAvailable

			public long PackagesAvailable
			{
				get { return PackedItems.Min(i => i.PacksAvailable); }
			}

			#endregion
		}

		#endregion

		#region PackedItem class

		class PackedItem
		{
			public PackedItem(ProductWithAttributes key, ZDecimal perPackageQty)
			{
				Key = key;
				PerPackageQty = perPackageQty;
			}

			public readonly ProductWithAttributes Key;
			public readonly ZDecimal PerPackageQty;

			#region Properties

			#region PacksOrdered

			public long PacksOrdered
			{
				get { return PickOrderedInventories.Sum(i => i.Value.CurrentValue); }
			}

			#endregion

			#region PackagesPicked

			public int PackagesPicked
			{
				get { return PickLocations.Sum(l => l.Value); }
			}

			#endregion

			#region PacksAvailable

			public long PacksAvailable { get; set; }

			#endregion

			#endregion

			#region PickLocations

			public Dictionary<ZString, ZInt> PickLocations
			{
				get { return pickLocations ?? (pickLocations = new Dictionary<ZString, ZInt>()); }
			}

			Dictionary<ZString, ZInt> pickLocations;

			#endregion

			#region PickOrderedInventories

			public Dictionary<WhsPickOrderedInventory, OrderedInventoryCache> PickOrderedInventories
			{
				get { return pickOrderedInventories ?? (pickOrderedInventories = new Dictionary<WhsPickOrderedInventory, OrderedInventoryCache>()); }
			}

			Dictionary<WhsPickOrderedInventory, OrderedInventoryCache> pickOrderedInventories;

			#endregion
		}

		#endregion

		#region OrderedInventoryCache class

		class OrderedInventoryCache
		{
			public ZInt CurrentValue { get; set; }
			public ZInt BestValueCached { get; set; }
		}

		#endregion

		#region OrderedItem class

		class OrderedItem
		{
			public OrderedItem(WhsPickOrderedInventory orderedInventory)
			{
				OrderedInventory = orderedInventory;
			}

			public readonly WhsPickOrderedInventory OrderedInventory;

			public ZDecimal QtyOrdered { get; set; }

			public ZDecimal QtyPicked { get; set; }

			#region ProductWithAttributes

			public ProductWithAttributes ProductWithAttributes
			{
				get
				{
					if (productWithAttributes == null)
					{
						productWithAttributes = ProductWithAttributes.GetProductWithAttributes(OrderedInventory);
					}
					return productWithAttributes;
				}
			}

			ProductWithAttributes productWithAttributes;

			#endregion
		}

		#endregion

		#endregion

		#endregion

		#region CanAllocateInQuantitiesDifferentToAutoAllocateQuantity

		protected override bool CanAllocateInQuantitiesDifferentToAutoAllocateQuantityCore(WhsPickAvailableInventory availableInventory)
		{
			return availableInventory.PackageGroupId.IsEmpty && availableInventory.PerPackageQty == 0m;
		}

		#endregion

		#region OnInventoryPickedCore

		protected override void OnInventoryPickedCore(WhsPickOrderedInventory orderedInventory, WhsPickAvailableInventory availableInventory, ZDecimal qtyDelta)
		{
			base.OnInventoryPickedCore(orderedInventory, availableInventory, qtyDelta);

			var group = GetMatchingPackageGroup(availableInventory, availableInventory.PackageGroupId);
			if (group != null)
			{
				var productWithAttributes = ProductWithAttributes.GetProductWithAttributes(availableInventory);
				var packedItem = group.PackedItems.FirstOrDefault(i => i.Key.Equals(productWithAttributes));
				if (packedItem != null)
				{
					var locationKey = GetLocationHashKey(availableInventory);
					packedItem.PickLocations.TryGetValue(locationKey, out var prevLocationPackedItems);

					var locationPackedItems = prevLocationPackedItems + (int)(qtyDelta / packedItem.PerPackageQty); // should always be int

					if (locationPackedItems <= 0)
					{
						packedItem.PickLocations.Remove(locationKey);
					}
					else
					{
						packedItem.PickLocations[locationKey] = locationPackedItems;
					}
				}
			}
		}

		#endregion
	}
}
