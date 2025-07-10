using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickAvailableInventoryCollection : NonPersistentBusinessObjectCollection<WhsPickAvailableInventory>
	{
		#region Constructors

		public WhsPickAvailableInventoryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region LoadForOrderedInventory

		public void LoadForOrderedInventory(WhsPickOrderedInventory whsOrderedInventory)
		{
			LoadForOrderedInventoryCore(whsOrderedInventory, null);
		}

		public void LoadSpecificPalletIDsForOrderedInventory(WhsPickOrderedInventory whsOrderedInventory, IEnumerable<string> palletIDs)
		{
			Argument.NotNull(palletIDs, nameof(palletIDs));
			LoadForOrderedInventoryCore(whsOrderedInventory, p => p.GetFilteredInventories(new ZQuery(WhsInventoryViewSchema.WI_PalletID, palletIDs)));
		}

		void LoadForOrderedInventoryCore(WhsPickOrderedInventory whsOrderedInventory, Func<WhsPick, IEnumerable<WhsInventoryView>> loadInventories)
		{
			this.orderedInventory = whsOrderedInventory;
			RemoveAll();
			AvailableInventoryByAttributeLookup.Clear();
			AvailableInventoryByInventoryLookup.Clear();

			if (IsOrderedInventoryValid)
			{
				var inventoryList = LoadAndFilterInventory(loadInventories);
				if (IsOrderedSerial(orderedInventory))
				{
					PutInventoryIntoThisCollectionWithoutMerge(inventoryList);
				}
				else if (IsProductUsingSerialNumberAttribute)
				{
					if (!WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
					{
						PutInventoryIntoThisCollectionWithoutMerge(inventoryList);
					}
					else
					{
						MergeInventoryIntoThisCollection(inventoryList);

						WhsSerialNumberHelper.AddWhsSerialNumberAndPivotFetchHint(Factory, inventoryList.Select(i => i.PK));
					}
				}
				else
				{
					MergeInventoryIntoThisCollection(inventoryList);
				}
			}
		}

		static bool IsOrderedSerial(WhsPickOrderedInventory orderedInventory) => !orderedInventory?.SerialNumber.IsEmpty ?? false;

		bool IsOrderedInventoryValid
		{
			get
			{
				return
					orderedInventory != null &&
					orderedInventory.Pick != null &&
					orderedInventory.Pick.Warehouse != null &&
					orderedInventory.Client != null &&
					orderedInventory.Product != null;
			}
		}

		#region LoadAndFilterInventory

		IEnumerable<WhsInventoryView> LoadAndFilterInventory(Func<WhsPick, IEnumerable<WhsInventoryView>> loadInventories)
		{
			var inventoryList = new List<WhsInventoryView>();

			var pick = orderedInventory?.Pick;
			if (pick != null)
			{
				var matchingInventory = loadInventories != null
					? pick.FindInventoryByOrderedInventory(orderedInventory, loadInventories)
					: pick.FindInventoryByOrderedInventoryFromAllInventories(orderedInventory);

				var pickStrategy = pick.CurrentPickStrategy;
				foreach (var inventory in matchingInventory)
				{
					if (pickStrategy.CanInventoryBeAllocated(orderedInventory, inventory))
					{
						inventoryList.Add(inventory);
					}
				}
			}

			return inventoryList;
		}

		#endregion

		#region PutInventoryIntoThisCollectionWithoutMerge

		void PutInventoryIntoThisCollectionWithoutMerge(IEnumerable<WhsInventoryView> inventoryList)
		{
			var isRegistryEnableHeldGoodsForOrders = WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value;
			foreach (var inventory in inventoryList)
			{
				if (IsInventorySuitableForAllocation(inventory, isRegistryEnableHeldGoodsForOrders))
				{
					CreateNewAvailableLine(inventory);
				}
			}
		}

		#endregion

		#region MergeInventoryIntoThisCollection

		internal void MergeInventoryIntoThisCollection(IEnumerable<WhsInventoryView> inventoryList)
		{
			Argument.NotNull(inventoryList, nameof(inventoryList));

			var availableInventoryDictionary = new Dictionary<string, WhsPickAvailableInventory>();
			var isRegistryEnableHeldGoodsForOrders = WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value;

			foreach (var inventory in inventoryList)
			{
				if (IsInventorySuitableForAllocation(inventory, isRegistryEnableHeldGoodsForOrders))
				{
					Merge(inventory, availableInventoryDictionary);
				}
			}
		}

		#endregion

		#region IsInventorySuitableForAllocation

		bool IsInventorySuitableForAllocation(WhsInventoryView inventory, bool isRegistryEnableHeldGoodsForOrders) => IsValidStatusForOrderedInventory(inventory, isRegistryEnableHeldGoodsForOrders)
			&& IsValidLocationForOrderedInventory(inventory)
			&& (inventory.WI_TotalUnits > 0 || IsInventoryAttachedToPick(inventory));

		bool IsValidStatusForOrderedInventory(WhsInventoryView inventory, bool isRegistryEnableHeldGoodsForOrders) => ((isRegistryEnableHeldGoodsForOrders && orderedInventory.Pick.IsHeldInventoryOrderPick) ? inventory.IsHeld : inventory.IsAvailable)
			|| inventory.IsPickByBOMKitInventory;

		bool IsValidLocationForOrderedInventory(WhsInventoryView inventory)
		{
			bool isValid;

			var pickAreaType = inventory.LocationPickAreaType;
			if (!orderedInventory.IsCustomsTransaction)
			{
				isValid = pickAreaType != AreaTypes.Codes.Bonded && pickAreaType != AreaTypes.Codes.InwardProcessing;
			}
			else if (orderedInventory.IsInwardProcessingJob)
			{
				isValid = pickAreaType == AreaTypes.Codes.InwardProcessing;
			}
			else
			{
				isValid = pickAreaType != AreaTypes.Codes.InwardProcessing;
			}

			return isValid;
		}

		bool IsInventoryAttachedToPick(WhsInventoryView inventory)
		{
			return orderedInventory.Owners.SelectMany(dl => dl.PickLines).Any(pl => pl.InventoryLinePKForAvailableInventory == inventory.WI_WE_InDocketLine);
		}

		#endregion

		#endregion

		#region Merging

		void Merge(WhsInventoryView inventory, Dictionary<string, WhsPickAvailableInventory> existingLines)
		{
			var hashCode = GetInventoryHashCodeIncludingOrderedSerial(inventory, orderedInventory);

			if (!existingLines.TryGetValue(hashCode, out var availableInventory))
			{
				availableInventory = CreateNewAvailableLine(inventory);
				existingLines.Add(hashCode, availableInventory);
			}
			else
			{
				availableInventory.Inventory.Add(inventory);
				AvailableInventoryByInventoryLookup[inventory.PK] = availableInventory;
			}
		}

		public static string GetInventoryHashCodeIncludingOrderedSerial(WhsInventoryView inventory, WhsPickOrderedInventory orderedInventory)
		{
			var hash = string.Join(
				AttributeComparer.OptionalValueSeparator,
				inventory.WI_OH_Client.ToString(),
				inventory.WI_OP.ToString(),
				inventory.WI_WL.ToString(),
				inventory.WI_ArrivalDate.ToString(),
				inventory.WI_InventoryStatus,
				inventory.WI_PalletID,
				inventory.WI_BondedEntryKey,
				inventory.WI_AllocationKey,
				inventory.WI_PartAttrib1,
				inventory.WI_PartAttrib2,
				inventory.WI_PartAttrib3,
				GetSerialNumber(inventory, orderedInventory),
				inventory.WI_PackingDate.ToString(),
				inventory.WI_ExpiryDate.ToString());

			if (inventory.Docket.WD_DocketSubType == ReceiveType.Codes.Customs)
			{
				var customsData = inventory.CustomsData;
				hash = string.Join(
					AttributeComparer.OptionalValueSeparator,
					hash,
					inventory.PackageGroupId,
					inventory.PerPackageQty.ToString(),
					customsData.WB_ValueForDuty.ToString(),
					customsData.WB_BondedWhsQty.ToString());
			}

			return hash;
		}

		static ZString GetSerialNumber(WhsInventoryView inventory, WhsPickOrderedInventory orderedInventory)
		{
			return WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value || IsAttributeNeutralUsed(inventory.Factory, inventory.WI_OP, inventory.WI_OH_Client, orderedInventory)
				? ZString.Empty
				: inventory.WI_SerialNumber;
		}

		#endregion

		#region NonPersistentBusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WhsPickAvailableInventory(Factory);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region Properties

		public bool IsProductUsingSerialNumberAttribute
		{
			get
			{
				bool result = false;

				var product = orderedInventory.Product;
				if (product != null)
				{
					var client = orderedInventory.Client;
					result = product.IsSerialNumberUsedAndNotReleaseCaptured(client) && !product.IsAttributeNeutralUsed(client);
				}

				return result;
			}
		}

		#region GetSerialNeutralAttributeNumber

		internal static bool IsAttributeNeutralUsed(BusinessObjectFactory factory, ZGuid productPK, ZGuid clientPK, WhsPickOrderedInventory orderedInventory)
		{
			return !IsOrderedSerial(orderedInventory) && factory.GetCachedValue(string.Format(Culture.Invariant, "WhsPickAvailableInventoryCollection|IsAttributeNeutralUsed|{0}|{1}", productPK, clientPK), // this is a hash key
				() => LoadIsAttributeNeutralUsed(factory, productPK, clientPK), CacheStalenessPolicy.StaleOnFactorySave);
		}

		static bool LoadIsAttributeNeutralUsed(BusinessObjectFactory factory, ZGuid productPK, ZGuid clientPK)
		{
			var result = false;

			var whsProduct = WhsProduct.GetWhsProduct(factory, productPK);
			if (whsProduct != null)
			{
				var client = factory.Load<OrgHeader>(clientPK);
				result = whsProduct.IsAttributeNeutralUsed(client);
			}

			return result;
		}

		#endregion

		#endregion

		#region GetAvailableInventoryFromInventoryPK

		public WhsPickAvailableInventory GetAvailableInventoryFromInventoryPK(ZGuid inventoryPK) => AvailableInventoryByInventoryLookup[inventoryPK];

		#endregion

		#region GetTotalAvailableToPickUnits

		public ZDecimal GetTotalAvailableToPickUnits(HashSet<WhsLocation> locationsToExclude)
		{
			return this.Cast<WhsPickAvailableInventory>().
				Where(a => !locationsToExclude.Contains(a.Location)).
				Sum(a => a.QuantityUnPicked + a.Inventory.Cast<WhsInventoryView>().Sum(i => i.WI_CommittedToTransferQuantity));
		}

		#endregion

		#region Implementation

		protected WhsPickAvailableInventory CreateNewAvailableLine(WhsInventoryView inventory)
		{
			var result = AddNew();
			((IWhsPickAvailableInventoryInternals)result).SetAllProperties(orderedInventory, inventory);

			var key = AttributeComparer.GetHashCodeForConsolidation(result);
			if (!AvailableInventoryByAttributeLookup.TryGetValue(key, out var matchingAvailableInventories))
			{
				AvailableInventoryByAttributeLookup[key] = matchingAvailableInventories = new List<WhsPickAvailableInventory>();
			}

			matchingAvailableInventories.Add(result);
			AvailableInventoryByInventoryLookup[inventory.PK] = result;

			return result;
		}

		WhsPickOrderedInventory orderedInventory;

		Dictionary<string, List<WhsPickAvailableInventory>> AvailableInventoryByAttributeLookup
		{
			get { return availableInventoryByAttributeLookup ?? (availableInventoryByAttributeLookup = new Dictionary<string, List<WhsPickAvailableInventory>>()); }
		}

		Dictionary<string, List<WhsPickAvailableInventory>> availableInventoryByAttributeLookup;

		Dictionary<ZGuid, WhsPickAvailableInventory> AvailableInventoryByInventoryLookup => availableInventoryByInventoryLookup ?? (availableInventoryByInventoryLookup = new Dictionary<ZGuid, WhsPickAvailableInventory>());
		Dictionary<ZGuid, WhsPickAvailableInventory> availableInventoryByInventoryLookup;

		#endregion

		#region Overrides

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			switch (property.Name)
			{
				case nameof(WhsPickAvailableInventory.LocationString):
				case nameof(WhsPickAvailableInventory.LocationPK):
					return new LocationComparer<WhsPickAvailableInventory>(property, direction, pickAvailableInventory => pickAvailableInventory.Location);
				default:
					return base.GetComparerForSort(property, direction);
			}
		}

		#endregion
	}
}
