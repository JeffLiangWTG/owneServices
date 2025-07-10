using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class InventoryCacheByAttribs
	{
		public void AddInventory(WhsInventoryView inventory)
		{
			Argument.NotNull(inventory, nameof(inventory));

			All.Add(inventory);
		}

		public void CacheByAttributes(WhsInventoryView inventory)
		{
			AddInventory(inventory);
			Memoise(PartAttrib1Cache, inventory.WI_PartAttrib1, inventory, IsEmpty);
			Memoise(PartAttrib2Cache, inventory.WI_PartAttrib2, inventory, IsEmpty);
			Memoise(PartAttrib3Cache, inventory.WI_PartAttrib3, inventory, IsEmpty);
			if (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				MemoiseSerialNumber(inventory);
			}
			else
			{
				Memoise(SerialNumberCache, inventory.WI_SerialNumber, inventory, IsEmpty);
			}
			Memoise(ExpiryCache, inventory.WI_ExpiryDate, inventory, IsEmpty);
			Memoise(PackingCache, inventory.WI_PackingDate, inventory, IsEmpty);
			Memoise(BondedEntryKeyCache, inventory.WI_BondedEntryKey, inventory, IsEmpty);
			Memoise(AllocationKeyCache, inventory.WI_AllocationKey, inventory, IsEmpty);
			Memoise(PalletIDCache, inventory.WI_PalletID, inventory, IsEmpty);

			if (IsRegistryEnableHeldGoodsForOrders.Value)
			{
				Memoise(HoldCodeCache, inventory.WI_HeldCode, inventory, IsEmpty);
			}
		}

		bool IsEmpty(string value) => string.IsNullOrWhiteSpace(value);
		bool IsEmpty(ZDate value) => value.IsEmpty;

		void Memoise<T>(Dictionary<T, HashSet<WhsInventoryView>> cache, T key, WhsInventoryView inventory, Func<T, bool> isEmpty)
		{
			if (!isEmpty(key))
			{
				AddToCache(cache, key, inventory);
			}
		}

		void MemoiseSerialNumber(WhsInventoryView inventory)
		{
			if (inventory.InDocketLine is ISerialNumberParent parent)
			{
				foreach (var serialNumber in parent.SerialNumbers)
				{
					AddToCache(SerialNumberCache, serialNumber.SerialNumberValue, inventory);
				}
			}
		}

		static void AddToCache<T>(Dictionary<T, HashSet<WhsInventoryView>> cache, T key, WhsInventoryView inventory)
		{
			if (!cache.TryGetValue(key, out var matchingInventory))
			{
				cache[key] = matchingInventory = new HashSet<WhsInventoryView>();
			}

			matchingInventory.Add(inventory);
		}

		public IEnumerable<WhsInventoryView> FindMatchingInventoryByAttribs(WhsPickableDocketLine owner)
		{
			Argument.NotNull(owner, nameof(owner));

			IEnumerable<WhsInventoryView> result;

			if (GetMatchingInventory(PartAttrib1Cache, owner.WE_PartAttrib1, out var attrib1s, IsEmpty)
				&& GetMatchingInventory(PartAttrib2Cache, owner.WE_PartAttrib2, out var attrib2s, IsEmpty)
				&& GetMatchingInventory(PartAttrib3Cache, owner.WE_PartAttrib3, out var attrib3s, IsEmpty)
				&& GetMatchingInventory(SerialNumberCache, owner.WE_SerialNumber, out var serials, IsEmpty)
				&& GetMatchingInventory(ExpiryCache, owner.WE_ExpiryDate, out var expirys, IsEmpty)
				&& GetMatchingInventory(PackingCache, owner.WE_PackingDate, out var packings, IsEmpty)
				&& GetMatchingInventory(BondedEntryKeyCache, owner.WE_BondedEntryKey, out var beks, IsEmpty)
				&& GetMatchingInventory(AllocationKeyCache, owner.WE_AllocationKey, out var allocationKeys, IsEmpty)
				&& GetMatchingInventory(PalletIDCache, owner.WE_PalletID, out var palletID, IsEmpty)
				&& GetMatchingHeldInventory(HoldCodeCache, owner.WE_WHC_NKOrderedHeldCode, out var holdCodes, IsEmpty))
			{
				var inventories = new[] { attrib1s, attrib2s, attrib3s, serials, expirys, packings, beks, allocationKeys, palletID, holdCodes }.Where(i => i != null).OrderBy(i => i.Count).ToArray();

				if (inventories.Length == 0)
				{
					result = All;
				}
				else if (inventories.Length == 1)
				{
					result = inventories[0];
				}
				else
				{
					var smallest = new HashSet<WhsInventoryView>(inventories[0]);
					foreach (var otherInventory in inventories.Skip(1))
					{
						smallest.IntersectWith(otherInventory);
						if (smallest.Count == 0)
						{
							break;
						}
					}

					return smallest;
				}
			}
			else
			{
				result = Enumerable.Empty<WhsInventoryView>();
			}

			return result;
		}

		bool GetMatchingHeldInventory(Dictionary<string, HashSet<WhsInventoryView>> cache, string key, out HashSet<WhsInventoryView> matchingInventory, Func<string, bool> isEmpty)
		{
			if (IsRegistryEnableHeldGoodsForOrders.Value)
			{
				return GetMatchingInventory(cache, key, out matchingInventory, isEmpty);
			}

			matchingInventory = null;
			return true;
		}

		bool GetMatchingInventory<T>(Dictionary<T, HashSet<WhsInventoryView>> cache, T key, out HashSet<WhsInventoryView> matchingInventory, Func<T, bool> isEmpty)
		{
			bool result;

			if (isEmpty(key))
			{
				matchingInventory = null;
				result = true;
			}
			else
			{
				result = cache.TryGetValue(key, out matchingInventory);
			}

			return result;
		}

		public void RemoveInventoryFromAll(ZGuid inventoryPK) => All.RemoveAll(l => l.PK == inventoryPK);

		List<WhsInventoryView> All { get; } = new List<WhsInventoryView>();
		Dictionary<string, HashSet<WhsInventoryView>> PartAttrib1Cache { get; } = new Dictionary<string, HashSet<WhsInventoryView>>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, HashSet<WhsInventoryView>> PartAttrib2Cache { get; } = new Dictionary<string, HashSet<WhsInventoryView>>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, HashSet<WhsInventoryView>> PartAttrib3Cache { get; } = new Dictionary<string, HashSet<WhsInventoryView>>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, HashSet<WhsInventoryView>> SerialNumberCache { get; } = new Dictionary<string, HashSet<WhsInventoryView>>(StringComparer.OrdinalIgnoreCase);
		Dictionary<ZDate, HashSet<WhsInventoryView>> ExpiryCache { get; } = new Dictionary<ZDate, HashSet<WhsInventoryView>>();
		Dictionary<ZDate, HashSet<WhsInventoryView>> PackingCache { get; } = new Dictionary<ZDate, HashSet<WhsInventoryView>>();
		Dictionary<string, HashSet<WhsInventoryView>> BondedEntryKeyCache { get; } = new Dictionary<string, HashSet<WhsInventoryView>>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, HashSet<WhsInventoryView>> HoldCodeCache { get; } = new Dictionary<string, HashSet<WhsInventoryView>>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, HashSet<WhsInventoryView>> AllocationKeyCache { get; } = new Dictionary<string, HashSet<WhsInventoryView>>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, HashSet<WhsInventoryView>> PalletIDCache { get; } = new Dictionary<string, HashSet<WhsInventoryView>>(StringComparer.OrdinalIgnoreCase);
		Lazy<bool> IsRegistryEnableHeldGoodsForOrders { get; } = new Lazy<bool>(() => WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value);
	}
}
