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
	class Report_WhsEmptyLocationSummaryTest : WhsTestCaseWithFactory
	{
		#region TestFunction

		public void TestFunction()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var receives = SetupData(warehouse);

			var viewLoadedResult = LoadView(warehouse);
			var manuallyLoadedResult = LoadManually(warehouse, receives);

			AssertEquals("View and Manual load should return same Count", viewLoadedResult.Count,
				manuallyLoadedResult.Count);

			for (int i = 0; i < manuallyLoadedResult.Count; i++)
			{
				AssertEqualInformation(manuallyLoadedResult[i], viewLoadedResult[i]);
			}
		}

		void AssertEqualInformation(SummaryWhsEmptyLocations summary, DynamicBusinessObject obj)
		{
			var area = summary.Area;
			AssertEquals("WW_PK", area.WA_WW_Whs, obj["WW_PK"]);
			AssertEquals("WA_Name", area.WA_Name, obj["WA_Name"]);
			AssertEquals("WA_AreaType", area.WA_AreaType, obj["WA_AreaType"]);
			AssertEquals("WLV_WLT_LocationType", summary.LocationType, obj["WLV_LocationType"]);
			AssertEquals("EmptyLocation", summary.EmptyLocation, obj["EmptyLocation"]);
			AssertEquals("NotEmptyLocation", summary.NotEmptyLocation, obj["NotEmptyLocation"]);
			AssertEquals("TotalLocation", summary.TotalLocation, obj["TotalLocation"]);
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
			foreach (WhsRow row in whs.Rows)
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

		#region Loaders

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			#region Sql query

			var sql = @"
SELECT
	*
FROM
	WhsEmptyLocationSummaryReport(@WarehousePK, null, null, null, null, null, null)
ORDER BY
	WA_Name, WA_AreaType asc";

			#endregion

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WarehousePK", whs.PK, WhsWarehouseSchema.PK);

			result.Load(sql, sqlParams);

			return result;
		}

		List<SummaryWhsEmptyLocations> LoadManually(WhsWarehouse whs, WhsReceive[] receives)
		{
			var result = new List<SummaryWhsEmptyLocations>();

			var tempLocationCollection = new WhsLocationCollection(whs);
			var tempInventoryCollection = new WhsInventoryViewCollection(Factory);

			tempInventoryCollection.AddRange(receives[0].Inventory);
			tempInventoryCollection.AddRange(receives[1].Inventory);
			tempInventoryCollection.AddRange(receives[2].Inventory);
			tempInventoryCollection.AddRange(receives[3].Inventory);

			foreach (WhsLocation location in tempLocationCollection)
			{
				if (location.Row.WR_WW_Whs == whs.PK)
				{
					var summary = FindSummary(result, location);
					if (IsTotalUnitsZero(tempInventoryCollection, location))
					{
						summary.EmptyLocation++;
					}
					else
					{
						summary.NotEmptyLocation++;
					}

					summary.TotalLocation++;
				}
			}

			result = result.OrderBy(r => r.Area.WA_Name).ThenBy(r => r.Area.WA_AreaType).ToList();
			return result;
		}

		SummaryWhsEmptyLocations FindSummary(List<SummaryWhsEmptyLocations> listToSearch, WhsLocation location)
		{
			foreach (var summary in listToSearch)
			{
				if (summary.Area.PK == location.WLV_WA_PutawayArea &&
					summary.LocationType == location.LocationType.WLT_Code)
				{
					return summary;
				}
			}

			var newSummary =
				new SummaryWhsEmptyLocations(location.PutawayArea, location.LocationType.WLT_Code, 0, 0, 0);
			listToSearch.Add(newSummary);
			return newSummary;
		}

		bool IsTotalUnitsZero(WhsInventoryViewCollection inventoryCollection, WhsLocation location)
		{
			foreach (WhsInventoryView inventory in inventoryCollection)
			{
				if (inventory.WI_WL == location.PK &&
					inventory.WI_TotalUnits > 0)
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region SummaryWhsEmptyLocations class

		class SummaryWhsEmptyLocations
		{
			public SummaryWhsEmptyLocations(WhsArea area, ZString locationType, ZInt emptyLocation,
				ZInt notEmptyLocation, ZInt totalLocation)
			{
				Area = area;
				LocationType = locationType;
				EmptyLocation = emptyLocation;
				NotEmptyLocation = notEmptyLocation;
				TotalLocation = totalLocation;
			}

			public WhsArea Area;
			public ZString LocationType;
			public ZInt EmptyLocation;
			public ZInt NotEmptyLocation;
			public ZInt TotalLocation;
		}

		#endregion
	}
}
