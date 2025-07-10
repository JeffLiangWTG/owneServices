using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class USBondedHelper
	{
		#region IsUSWarehouseBondedEnabled

		public static bool IsWarehouseBondedEnabledAndUSJurisdiction(this WhsDocket docket)
		{
			return docket != null && docket.Warehouse.IsBondedEnabledAndUSJurisdiction();
		}

		public static bool IsBondedEnabledAndUSJurisdiction(this WhsWarehouse warehouse)
		{
			return warehouse != null && IsUSJurisdiction(warehouse) && warehouse.IsWarehouseBondEnabled;
		}

		public static bool IsUSJurisdiction(this WhsWarehouse warehouse)
		{
			return warehouse != null && BondedHelper.IsCountrySupportedForFTZPermits(warehouse.CountryCode);
		}

		public static bool IsFTZBondedEnabledAndUSJurisdiction(this WhsWarehouse warehouse)
		{
			return warehouse != null && warehouse.IsFTZWarehouse && warehouse.IsBondedEnabledAndUSJurisdiction();
		}

		public static bool IsFTZAndUSJurisdiction(this WhsWarehouse warehouse)
		{
			return warehouse != null && warehouse.IsFTZWarehouse && IsUSJurisdiction(warehouse);
		}

		#endregion

		#region GetDistinctPackageGroupIDInventories

		public static Dictionary<ProductWithAttributes, ZDecimal> GetDistinctPackageGroupIDInventories(BusinessObjectFactory factory, WhsWarehouse warehouse, ZString packageGroupID)
		{
			return GetPackageGroupIDInventoriesCore(factory, warehouse, packageGroupID, useBondedEntryKey: true);
		}

		static Dictionary<ProductWithAttributes, ZDecimal> GetPackageGroupIDInventoriesCore(BusinessObjectFactory factory, WhsWarehouse warehouse, ZString packageGroupID, bool useBondedEntryKey)
		{
			var result = new Dictionary<ProductWithAttributes, ZDecimal>();
			if (!packageGroupID.IsEmpty)
			{
				var matchingInventoryLines = GetInventoryMatchingPackageGroupID(factory, warehouse, packageGroupID);
				result = GetUniqueProductListWithMatchingInventory(matchingInventoryLines, useBondedEntryKey);
			}
			return result;
		}

		static Dictionary<ProductWithAttributes, ZDecimal> GetUniqueProductListWithMatchingInventory(WhsInventoryView[] matchingInventoryLines, bool useBondedEntryKey)
		{
			var otherProductsWithPackageID = new Dictionary<ProductWithAttributes, ZDecimal>();
			foreach (var inventory in matchingInventoryLines)
			{
				var productWithAttributes = ProductWithAttributes.GetProductWithAttributes(inventory, useBondedEntryKey);
				if (!otherProductsWithPackageID.ContainsKey(productWithAttributes))
				{
					otherProductsWithPackageID.Add(productWithAttributes, inventory.PerPackageQty);
				}
			}

			return otherProductsWithPackageID;
		}

		#endregion

		public static Dictionary<ProductWithAttributes, ZDecimal> GetPackageGroupIDContent(BusinessObjectFactory factory, WhsWarehouse warehouse, ZString packageGroupID)
		{
			return GetPackageGroupIDInventoriesCore(factory, warehouse, packageGroupID, useBondedEntryKey: false);
		}

		#region GetInventoryMatchingPackageGroupID

		public static WhsInventoryView[] GetInventoryMatchingPackageGroupID(BusinessObjectFactory factory, WhsWarehouse warehouse, ZString packageGroupID)
		{
			var result = new List<WhsInventoryView>();
			if (!packageGroupID.IsEmpty)
			{
				var inventoriesModifiedInMemory = GetChangedInventoriesInMemory(factory, warehouse, packageGroupID);
				result.AddRange(inventoriesModifiedInMemory);
				result.AddRange(GetInventoriesFromDB(factory, warehouse, packageGroupID, inventoriesModifiedInMemory));
			}

			return result.ToArray();
		}

		#region GetChangedInventoriesInMemory

		static WhsInventoryView[] GetChangedInventoriesInMemory(BusinessObjectFactory factory, WhsWarehouse warehouse, ZString packageGroupID)
		{
			var result = new List<WhsInventoryView>();
			var query = new ZQuery(WhsDocketLineSchema.WE_PackageGroupId, packageGroupID);
			query.FetchOnlyFromLocalCache = true;

			foreach (var docketLine in factory.Load<WhsDocketLine>(query))
			{
				var docket = docketLine.Docket;
				if (docket != null && docket.WD_WW_Whs == warehouse.PK)
				{
					foreach (WhsInventoryView inventory in docketLine.Inventory)
					{
						if (inventory.HasChanges && inventory.WI_TotalUnits > 0m && !inventory.IsInTransit)
						{
							result.Add(inventory);
						}
					}
				}
			}

			return result.ToArray();
		}

		#endregion

		#region GetInventoriesFromDB

		static WhsInventoryView[] GetInventoriesFromDB(BusinessObjectFactory factory, WhsWarehouse warehouse, ZString packageGroupID, WhsInventoryView[] inventoriesModifiedInMemory)
		{
			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsInventoryViewSchema.WI_WE_InDocketLine);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_PackageGroupId, packageGroupID);

			var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsInventoryViewSchema.WI_WL);
			locationSubQuery.AddToFilter(WhsLocationViewSchema.WLV_WW_Whs, warehouse.PK);

			var inventoryQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

			if (inventoriesModifiedInMemory.Length > 1)
			{
				inventoryQuery.AddToFilter(WhsInventoryViewSchema.PK, SQLComparisonOperator.NotEqual, inventoriesModifiedInMemory.Select(i => i.PK));
			}

			inventoryQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);
			inventoryQuery.AddSubQuery(locationSubQuery, JoinCondition.And);

			return factory.Load<WhsInventoryView>(inventoryQuery);
		}

		#endregion

		#endregion
	}
}
