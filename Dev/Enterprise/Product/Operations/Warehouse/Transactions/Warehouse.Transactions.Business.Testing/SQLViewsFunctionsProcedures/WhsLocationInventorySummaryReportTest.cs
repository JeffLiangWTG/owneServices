using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsLocationInventorySummaryReportTest : WhsTestCaseWithFactory
	{
		#region TestView_FiltersInTransitLines

		public void TestView_FiltersInTransitLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, inventory.LocationString, "");
			var inTransitTransferLine =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, inventory.LocationString, "");
			transfer.RunPreSaveValidation(); // to commit inventory
			AssertEquals("Precondition: Transfer Line should not be in transit.", false,
				inTransitTransferLine.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit);

			inTransitTransferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line should be in transit.", true,
				inTransitTransferLine.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit);
			Factory.Save();

			var results = GetResults();
			AssertEquals("WhsLocationInventorySummaryReport should not return In Transit (INT) lines.", 2,
				results.Count);
			AssertData(results[0], data.Whs1, data.Whs1.DefaultLocation, receive.Lines[0]);
			AssertData(results[1], data.Whs1, data.Whs1.DefaultOutboundDockDoorLocation, null);
		}

		#endregion

		#region TestView_ShowsStagedLines

		public void TestView_ShowsStagedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var results = GetResults();
			AssertEquals("WhsLocationInventorySummaryReport should return staged lines.", 2, results.Count);
			AssertData(results[0], data.Whs1, data.Whs1.DefaultLocation, receive.Lines[0]);
			AssertData(results[1], data.Whs1, data.Whs1.DefaultOutboundDockDoorLocation, transferLine);
		}

		#endregion

		#region TestView_FiltersPuttingAwayLines

		public void TestView_FiltersPuttingAwayLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receiveWithDockDoor = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveWithDockDoor, data.Part1, 10m, dockDoorLocation, "12345");
			Factory.Save();

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m,
				data.Whs1.DefaultLocation, "1");

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "12345", 10m);
			putawayLine.RunPreSaveValidation();
			putawayLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Transfer line inventory status should be changed to Putting Away.",
				InventoryStatus.Codes.PuttingAway, putawayLine.WE_CurrentInventoryStatus);

			var results = GetResults();
			AssertEquals("LocationInventorySummaryReport should not return put away (PTA) lines.", 3, results.Count);
			AssertData(results[0], data.Whs1, data.Whs1.DefaultLocation, receive2.Lines[0]);
			AssertData(results[1], data.Whs1, dockDoorLocation, null);
			AssertData(results[2], data.Whs1, data.Whs1.DefaultOutboundDockDoorLocation, null);
		}

		#endregion

		#region TestReportFiltersOutTransitWarehouses

		public void TestReportFiltersOutTransitWarehouses()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS", "A");
			var transitWarehouse = Helper.CreateWarehouse("TRA", "A");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();

			var results = GetResults();
			AssertEquals("Should only have returned the Product Warehouse.", 2, results.Count);
			AssertData(results[0], productWarehouse, productWarehouse.DefaultLocation, null);
			AssertData(results[1], productWarehouse, productWarehouse.DefaultOutboundDockDoorLocation, null);
		}

		#endregion

		#region TestReportShowsDockDoorLocations

		public void TestReportShowsDockDoorLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation1 = data.Whs1.FindLocation("A-2");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-3");
			var nonDockDoorLocation2 = data.Whs1.FindLocation("A-4");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receiveWithoutPutawayTransfer = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1");
			var receivedInventory = Helper.CreateWhsReceiveLine(receiveWithoutPutawayTransfer, data.Part1, 10m,
				dockDoorLocation1, "1245");
			var putawayInventory = Helper.CreateWhsReceiveLine(receiveWithoutPutawayTransfer, data.Part1, 10m,
				nonDockDoorLocation1, "12345");

			var receiveWithPutawayTransfer = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW2");
			var pickedReceivedInventory =
				Helper.CreateWhsReceiveLine(receiveWithPutawayTransfer, data.Part1, 10m, dockDoorLocation2, "1A5");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1,
				dockDoorLocation2, nonDockDoorLocation2, "1A5", 10m);
			putawayTransferLine.RunPreSaveValidation();
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertEquals("PutawayTransfer is finalised", true, putawayTransfer.IsFinalised);

			var results = GetResults();
			AssertEquals("Should have only returned the 5 locations in Product Warehouse.", 5, results.Count);
			AssertData(results[0], data.Whs1, nonDockDoorLocation1, putawayInventory);
			AssertData(results[1], data.Whs1, dockDoorLocation1, receivedInventory);
			AssertData(results[2], data.Whs1, dockDoorLocation2, null);
			AssertData(results[3], data.Whs1, nonDockDoorLocation2, putawayTransferLine);
			AssertData(results[4], data.Whs1, data.Whs1.DefaultOutboundDockDoorLocation, null);
		}

		#endregion

		#region Implementation

		DynamicBusinessObjectCollection GetResults()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				"SELECT * FROM dbo.WhsLocationInventorySummaryReport ORDER BY WW_WarehouseCode, WR_Name, WLV_FormattedColumn");
			return result;
		}

		void AssertData(DynamicBusinessObject row, WhsWarehouse expectedWarehouse, WhsLocation expectedLocation,
			WhsDocketLine expectedInventory)
		{
			AssertEquals("WW_PK", expectedWarehouse.PK, row["WW_PK"]);
			AssertEquals("WW_WarehouseName", expectedWarehouse.WW_WarehouseName, row["WW_WarehouseName"]);
			AssertEquals("WW_WarehouseCode", expectedWarehouse.WW_WarehouseCode, row["WW_WarehouseCode"]);
			AssertEquals("WR_Name", expectedLocation.RowName, row["WR_Name"]);
			AssertEquals("WLV_ColumnFormatted", expectedLocation.FormattedColumn, row["WLV_FormattedColumn"]);
			AssertEquals("WLV_LevelFormatted", expectedLocation.FormattedLevel, row["WLV_FormattedLevel"]);
			AssertEquals("WLV_TrayFormatted", expectedLocation.FormattedTray, row["WLV_FormattedTray"]);
			AssertEquals("WLV_WA_PutawayArea", expectedLocation.WLV_WA_PutawayArea, row["WLV_WA_PutawayArea"]);
			var locationType = Factory.Load<WhsLocationType>(expectedLocation.WLV_WLT_LocationType);
			AssertEquals("WLV_WLT_LocationType", locationType.WLT_Code, row["WLV_LocationType"]);
			AssertEquals("WLV_LocationStatus", expectedLocation.WLV_LocationStatus, row["WLV_LocationStatus"]);
			AssertEquals("WLV_PickMethod", expectedLocation.WLV_PickMethod, row["WLV_PickMethod"]);
			AssertEquals("WLV_MaxWeight", expectedLocation.WLV_MaxWeight, row["WLV_MaxWeight"]);
			AssertEquals("WLV_MaxCubic", expectedLocation.WLV_MaxCubic, row["WLV_MaxCubic"]);
			AssertEquals("WLV_MaxWidth", expectedLocation.WLV_MaxWidth, row["WLV_MaxWidth"]);
			AssertEquals("WLV_MaxHeight", expectedLocation.WLV_MaxHeight, row["WLV_MaxHeight"]);
			AssertEquals("WLV_MaxDepth", expectedLocation.WLV_MaxDepth, row["WLV_MaxDepth"]);
			AssertEquals("WLV_MaxDimensionUnit", expectedLocation.WLV_MaxDimensionUnit, row["WLV_MaxDimensionUnit"]);
			AssertEquals("WLV_ApprovedKnownLocation", expectedLocation.WLV_ApprovedKnownLocation,
				row["WLV_ApprovedKnownLocation"]);
			AssertEquals("WA_Name", expectedLocation.PickingArea.WA_Name, row["WA_Name"]);
			AssertEquals("WA_PK", expectedLocation.WLV_WA_PickingArea, row["WA_PK"]);
			AssertEquals("WA_AreaType", expectedLocation.PickingArea.WA_AreaType, row["WA_AreaType"]);

			if (expectedInventory == null)
			{
				AssertEquals("WLV_TotalUnits", 0m, row["WLV_TotalUnits"]);
				AssertEquals("WLV_Inventory_PK", ZGuid.Empty, row["WLV_Inventory_PK"]);
			}
			else
			{
				AssertEquals("WLV_TotalUnits", expectedInventory.WE_StockOnHand, row["WLV_TotalUnits"]);
				AssertEquals("WLV_Inventory_PK", expectedLocation.PK, row["WLV_Inventory_PK"]);
			}
		}

		#endregion
	}
}
