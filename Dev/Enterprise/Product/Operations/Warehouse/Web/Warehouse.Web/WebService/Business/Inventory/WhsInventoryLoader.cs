using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public class WhsInventoryLoader
	{
		public WhsInventoryLoader(BusinessObjectFactory factory, string warehouseCode)
		{
			Factory = factory;
			WarehouseCode = warehouseCode;
		}

		readonly BusinessObjectFactory Factory;
		readonly string WarehouseCode;

		#region LoadWhsInventory

		public WhsInventoryView[] LoadWhsInventory(WhsInventorySearchCriteriaInfo criteriaInfo)
		{
			var conditionOperator = criteriaInfo.JoinCondition == SearchJoinCondition.And ? JoinCondition.And : JoinCondition.Or;
			var isValidProduct = GetProduct(criteriaInfo) != null;
			var cachedLocation = criteriaInfo.Location;
			var cachedPalletID = criteriaInfo.PalletID;

			if (criteriaInfo.DestinationInventoryLevel == WhsInventoryLevel.Level1)
			{
				if (isValidProduct)
				{
					criteriaInfo.Location = string.Empty;
					criteriaInfo.PalletID = string.Empty;
				}
				else
				{
					criteriaInfo.ProductCode = string.Empty;
					criteriaInfo.ProductPK = Guid.Empty;
				}
			}

			var inventories = LoadWhsInventory(criteriaInfo, conditionOperator);

			// We must clear invalid Location/PalletID on criteria to assist Level2/3 inventory query
			if (criteriaInfo.DestinationInventoryLevel == WhsInventoryLevel.Level1 && !isValidProduct)
			{
				var isValidLocation = !string.IsNullOrEmpty(cachedLocation) && inventories.Any(i => WhsLocationHelper.LocationStringCompare(i.Location, cachedLocation));
				var isValidPalletID = !string.IsNullOrEmpty(cachedPalletID) && inventories.Any(i => i.WI_PalletID.EqualsIgnoringCase(cachedPalletID));

				criteriaInfo.Location = isValidLocation ? cachedLocation : string.Empty;
				criteriaInfo.PalletID = isValidPalletID ? cachedPalletID : string.Empty;
			}

			return inventories;
		}

		#region GetProduct

		OrgSupplierPart GetProduct(WhsInventorySearchCriteriaInfo criteriaInfo)
		{
			OrgSupplierPart part = null;

			if (criteriaInfo.ProductPK != Guid.Empty)
			{
				part = Factory.Load<OrgSupplierPart>(criteriaInfo.ProductPK);
			}
			if (part == null && !string.IsNullOrEmpty(criteriaInfo.ProductCode))
			{
				part = WebServiceHelper.GetPartByPartNumOrBarcode(Factory, WarehouseCode, criteriaInfo.ProductCode, criteriaInfo.ClientCode, false);
			}

			return part;
		}

		#endregion

		public WhsInventoryView[] LoadWhsInventory(WhsInventorySearchCriteriaInfo criteriaInfo, JoinCondition conditionOperator, bool excludeIntransit = true)
		{
			var warehouse = WebServiceHelper.GetWarehouse(Factory, WarehouseCode) ?? throw new InvalidOperationException("Warehouse should not be null.");

			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.ReLoadExistingRows = true;

			if (!string.IsNullOrEmpty(criteriaInfo.PalletID))
			{
				query.AddToFilter(conditionOperator, WhsInventoryViewSchema.WI_PalletID, criteriaInfo.PalletID);
			}

			if (!string.IsNullOrEmpty(criteriaInfo.Location))
			{
				var locationSubQuery = new ZDBOnlySubQuery(typeof(IWhsLocation), WhsLocationViewSchema.PK);
				WebServiceHelper.AddLocationStringFilter(warehouse, locationSubQuery, criteriaInfo.Location);

				var palletIDAndLocationJoinCondition = string.IsNullOrEmpty(criteriaInfo.PalletID) || string.IsNullOrEmpty(criteriaInfo.Location)
					? conditionOperator
					: criteriaInfo.PalletIDAndLocationJoinCondition == SearchJoinCondition.Or ? JoinCondition.Or : JoinCondition.And;

				query.AddSubQuery(WhsInventoryViewSchema.WI_WL, locationSubQuery, palletIDAndLocationJoinCondition);
			}

			// Attributes
			if (criteriaInfo.DestinationInventoryLevel == WhsInventoryLevel.Level3)
			{
				query.AddToFilter(conditionOperator, WhsInventoryViewSchema.WI_PartAttrib1, criteriaInfo.Attribute1);
				query.AddToFilter(conditionOperator, WhsInventoryViewSchema.WI_PartAttrib2, criteriaInfo.Attribute2);
				query.AddToFilter(conditionOperator, WhsInventoryViewSchema.WI_PartAttrib3, criteriaInfo.Attribute3);
				query.AddToFilter(conditionOperator, WhsInventoryViewSchema.WI_SerialNumber, criteriaInfo.SerialNumber);
				query.AddToFilter(conditionOperator, WhsInventoryViewSchema.WI_ExpiryDate, GetZDateTime(criteriaInfo.ExpiryDate));
				query.AddToFilter(conditionOperator, WhsInventoryViewSchema.WI_PackingDate, GetZDateTime(criteriaInfo.PackingDate));
			}

			if (criteriaInfo.ProductPK == Guid.Empty && !string.IsNullOrEmpty(criteriaInfo.ProductCode))
			{
				query.AddSubQuery(WebServiceHelper.GetProductQueryCore(WhsInventoryViewSchema.WI_OP, criteriaInfo.ProductCode), conditionOperator);
			}

			// Warehouse
			query.AddSubQuery(WebServiceHelper.GetWarehouseSubQuery(warehouse), JoinCondition.And);

			// Part
			if (criteriaInfo.ProductPK != Guid.Empty)
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_OP, criteriaInfo.ProductPK);
			}

			// Client Code
			var client = WebServiceHelper.GetOrgHeader(Factory, criteriaInfo.ClientCode);
			if (client != null)
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, client.PK);
			}
			query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0);

			if (excludeIntransit)
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_InventoryStatus, SQLComparisonOperator.NotEqual, new[] { InventoryStatus.Codes.PuttingAway, InventoryStatus.Codes.InTransit });
			}

			return LoadWhsInventory(Factory, query);
		}

		static ZDateTime GetZDateTime(DateTime dateTime) => dateTime == DateTime.MinValue ? ZDateTime.Empty : new ZDateTime(dateTime);

		// Needs to be ZDBOnlyQuery for the OrderBy to work. If a ZQuery is required, rethink how to do this.
		public static WhsInventoryView[] LoadWhsInventory(BusinessObjectFactory factory, ZDBOnlyQuery query)
		{
			var inventories = factory.Load<WhsInventoryView>(query);
			foreach (var inventory in inventories)
			{
				factory.AddFetchHint(OrgSupplierPartSchema.PK, inventory.WI_OP);
			}

			return inventories.OrderBy(inv => inv.WI_OP_PartNum).ToArray();
		}

		#endregion
	}
}
