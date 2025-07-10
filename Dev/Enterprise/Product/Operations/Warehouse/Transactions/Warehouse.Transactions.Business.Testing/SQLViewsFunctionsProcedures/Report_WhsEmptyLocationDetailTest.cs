using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class Report_WhsEmptyLocationDetailTest : WhsTestCaseWithFactory
	{
		#region TestFunction

		public void TestFunction()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var receives = SetupData(warehouse);

			var viewLoadedResult = LoadView(warehouse);
			var manuallyLoadedResult = LoadManually(warehouse, receives);
			AssertEquals("View and Manual load should return same Count", manuallyLoadedResult.Count,
				viewLoadedResult.Count);

			for (int i = 0; i < manuallyLoadedResult.Count; i++)
			{
				AssertEqualInformation(manuallyLoadedResult[i], viewLoadedResult[i]);
			}
		}

		void AssertEqualInformation(WhsLocation location, DynamicBusinessObject obj)
		{
			var row = location.Row;
			AssertEquals("WW_PK", row.WR_WW_Whs, obj["WW_PK"]);
			AssertEquals("WW_WarehouseName", row.Warehouse.WW_WarehouseName, obj["WW_WarehouseName"]);
			AssertEquals("WW_WarehouseCode", row.Warehouse.WW_WarehouseCode, obj["WW_WarehouseCode"]);
			AssertEquals("WR_Name", row.WR_Name, obj["WR_Name"]);
			AssertEquals("WLV_Column", location.WLV_Column.ToString(), obj["WLV_Column"]);
			AssertEquals("WLV_Level", location.WLV_Level.ToString(), obj["WLV_Level"]);
			AssertEquals("WLV_Tray", location.WLV_Tray.ToString(), obj["WLV_Tray"]);
			AssertEquals("WLV_WA_PutawayArea", location.WLV_WA_PutawayArea, obj["WLV_WA_PutawayArea"]);
			var locationType = Factory.Load<WhsLocationType>(location.WLV_WLT_LocationType);
			AssertEquals("WLV_WLT_LocationType", locationType.WLT_Code, obj["WLV_LocationType"]);
			AssertEquals("WLV_LocationStatus", location.WLV_LocationStatus, obj["WLV_LocationStatus"]);
			AssertEquals("WLV_PickMethod", location.WLV_PickMethod, obj["WLV_PickMethod"]);
			AssertEquals("WLV_MaxWeight", location.WLV_MaxWeight, obj["WLV_MaxWeight"]);
			AssertEquals("WLV_MaxCubic", location.WLV_MaxCubic, obj["WLV_MaxCubic"]);
			AssertEquals("WLV_MaxWidth", location.WLV_MaxWidth, obj["WLV_MaxWidth"]);
			AssertEquals("WLV_MaxHeight", location.WLV_MaxHeight, obj["WLV_MaxHeight"]);
			AssertEquals("WLV_MaxDepth", location.WLV_MaxDepth, obj["WLV_MaxDepth"]);
			AssertEquals("WLV_MaxDimensionUnit", location.WLV_MaxDimensionUnit, obj["WLV_MaxDimensionUnit"]);
			AssertEquals("WLV_ApprovedKnownLocation", location.WLV_ApprovedKnownLocation,
				obj["WLV_ApprovedKnownLocation"]);
			AssertEquals("WA_Name", location.PutawayArea.WA_Name, obj["WA_Name"]);
			AssertEquals("WA_AreaType", location.PutawayArea.WA_AreaType, obj["WA_AreaType"]);
		}

		#endregion

		#region Loaders

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			#region Sql query

			var sql = @"
SELECT
	*
FROM
	WhsEmptyLocationDetailReport(@WarehousePK, null, null, null, null, null, null)
ORDER BY
	WR_Name, WLV_Column, WLV_Level, WLV_Tray asc";

			#endregion

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WarehousePK", whs.PK, WhsWarehouseSchema.PK);

			result.Load(sql, sqlParams);

			return result;
		}

		List<WhsLocation> LoadManually(WhsWarehouse whs, WhsReceive[] receives)
		{
			var result = new List<WhsLocation>();
			var tempLocationCollection = new WhsLocationCollection(whs);
			var tempInventoryCollection = new WhsInventoryViewCollection(Factory);

			tempInventoryCollection.AddRange(receives[0].Inventory);
			tempInventoryCollection.AddRange(receives[1].Inventory);
			tempInventoryCollection.AddRange(receives[2].Inventory);
			tempInventoryCollection.AddRange(receives[3].Inventory);

			foreach (WhsLocation location in tempLocationCollection.OrderBy(l => l.WLV_LocationString))
			{
				if (location.WLV_WW_Whs == whs.PK && IsTotalUnitsZero(tempInventoryCollection, location))
				{
					result.Add(location);
				}
			}

			return result;
		}

		bool IsTotalUnitsZero(WhsInventoryViewCollection inventoryCollection, WhsLocation location)
		{
			foreach (WhsInventoryView inventory in inventoryCollection)
			{
				if (inventory.WI_WL == location.PK && inventory.WI_TotalUnits > 0)
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region SetupData

		WhsReceive[] SetupData(WhsWarehouse warehouse)
		{
			var client = Helper.CreateClient("1", "Client");
			var warehouse2 = Helper.CreateWarehouse("2");
			var area1 = Helper.CreateArea(warehouse, "Bottom", "FRE");
			var area2 = Helper.CreateArea(warehouse, "Upper", "FRE");
			var row1 = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 3);
			var row2 = Helper.CreateRowAndGenerateLocations(warehouse, "B", 2, 2, 2);
			var row3 = Helper.CreateRowAndGenerateLocations(warehouse2, "C", 2, 2);
			var part = Helper.CreateProduct(client, "P1");

			SetAreasForWarehouseLocations(warehouse, area1, area2);

			var receives = new[]
			{
				SetReceive(client, warehouse, "R1", part, row1.Locations[0], row1.Locations[0], row1.Locations[1]),
				SetReceive(client, warehouse, "R2", part, row1.Locations[1], row1.Locations[3], row1.Locations[4]),
				SetReceive(client, warehouse, "R3", part, row2.Locations[0], row2.Locations[5], row2.Locations[6]),
				SetReceive(client, warehouse2, "R4", part, row3.Locations[0], row3.Locations[2], row3.Locations[3])
			};
			Factory.Save();

			var order1 = SetOrders(client, warehouse, "O1", part, 3m);
			var order2 = SetOrders(client, warehouse, "O2", part, 6m);
			var order3 = SetOrders(client, warehouse2, "O3", part, 3m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			pick1.FinaliseAllOrders();
			AssertEquals("Order should be finalized", true, order1.IsFinalised);
			pick1.FinalisePick();
			AssertEquals("Pick should be finalized", true, pick1.IsFinalised);

			Factory.Save();

			return receives;
		}

		void SetAreasForWarehouseLocations(WhsWarehouse whs, WhsArea bottomLevelArea, WhsArea upperLevelArea)
		{
			foreach (var row in whs.Rows)
			{
				foreach (WhsLocation location in row.Locations)
				{
					if (location.WLV_Level == 1)
					{
						location.WLV_WA_PickingArea = bottomLevelArea.PK;
					}
					else if (location.WLV_Level == row.WR_Levels)
					{
						location.WLV_WA_PickingArea = upperLevelArea.PK;
					}
				}
			}
		}

		WhsReceive SetReceive(OrgHeader client, WhsWarehouse whs, ZString reference, OrgSupplierPart part,
			WhsLocation location1, WhsLocation location2, WhsLocation location3)
		{
			var receive = Helper.CreateWhsReceive(client, whs, reference, Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 1m);
			inventory.WI_WL = location1.PK;
			inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 1m);
			inventory.WI_WL = location2.PK;
			inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 1m);
			inventory.WI_WL = location3.PK;

			receive.FinaliseDocket();
			AssertEquals("Receive IsFinalised", true, receive.IsFinalised);
			return receive;
		}

		WhsOrder SetOrders(OrgHeader client, WhsWarehouse whs, ZString reference, OrgSupplierPart part, ZDecimal units)
		{
			var order = Helper.CreateWhsOrder(client, whs, reference);
			Helper.CreateWhsOrderLine(order, part, units);
			return order;
		}

		#endregion
	}
}
