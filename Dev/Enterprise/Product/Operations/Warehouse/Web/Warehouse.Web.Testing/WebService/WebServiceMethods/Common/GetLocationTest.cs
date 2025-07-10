using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetLocationTest : WhsSecureServiceTestCase
	{
		#region TestGetLocation

		#region TestGetLocation

		public void TestGetLocation()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse1, "RT", 10, 5);
			var location = row.Locations[5];
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = warehouse1.WW_WarehouseCode;
			var errorResponse1 = webService1.GetLocation("");
			AssertEquals("", errorResponse1.Location);
			AssertEquals("Location  does not exist in warehouse WHS1", errorResponse1.ErrorMessage);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = warehouse1.WW_WarehouseCode;
			var errorResponse2 = webService2.GetLocation("NONEXISTINGLOCATIONCODE");
			AssertEquals("", errorResponse2.Location);
			AssertEquals("Location NONEXISTINGLOCATIONCODE does not exist in warehouse WHS1", errorResponse2.ErrorMessage);

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = warehouse1.WW_WarehouseCode;
			webService3.AllowedToRunServiceHasBeenCalled = false;
			var response1 = webService3.GetLocation(location.OldBarcode);
			AssertSuccessfulResponse(response1, webService3);
			AssertEquals(location.ToLocationString(), response1.Location);

			var webService4 = GetNewWebService();
			webService4.SecurityHeader.WarehouseCode = warehouse1.WW_WarehouseCode;
			var response2 = webService4.GetLocation(location.ToLocationString());
			AssertEquals(location.ToLocationString(), response2.Location);
		}

		public void TestGetLocation_FixedWidthLocation()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "ABC", 5, 5, 5);
			Helper.Factory.Save();

			var location = warehouse.FindLocation("ABC040302");
			var webService1 = GetNewWebService(warehouse);
			var errorResponse1 = webService1.GetLocation("");
			AssertEquals("", errorResponse1.Location);
			AssertEquals("Location  does not exist in warehouse ZZ", errorResponse1.ErrorMessage);

			var webService2 = GetNewWebService(warehouse);
			var response1 = webService2.GetLocation("ABC-04-03-02");
			AssertEquals("ABC040302", response1.Location);
			AssertEquals("ABC-04-03-02", response1.LocationUserFriendly);
			AssertEquals(location.PK, response1.LocationPK);
			Assert(string.IsNullOrEmpty(response1.ErrorMessage));

			var webService3 = GetNewWebService(warehouse);
			webService3.AllowedToRunServiceHasBeenCalled = false;
			var response2 = webService3.GetLocation(location.OldBarcode);
			AssertSuccessfulResponse(response2, webService3);
			AssertEquals("ABC040302", response2.Location);
			AssertEquals("ABC-04-03-02", response2.LocationUserFriendly);
			AssertEquals(location.PK, response2.LocationPK);

			var webService4 = GetNewWebService(warehouse);
			var response3 = webService4.GetLocation("ABC040302");
			AssertEquals("ABC040302", response3.Location);
			AssertEquals("ABC-04-03-02", response3.LocationUserFriendly);
			AssertEquals(location.PK, response3.LocationPK);
		}

		#endregion

		#region TestGetLocation_DbHits

		public void TestGetLocation_DbHits()
		{
			const int numberOfRecords = 100;
			const int expectedMaxNumberOfDBHits = 6; // 3 for Staff / Branch / Department for Login (does not cache per enterprise instance any longer. Reason: Thread Sentry)

			var whs = Helper.CreateWarehouse("WHS");
			for (int i = 0; i < numberOfRecords; i++)
			{
				Helper.CreateRowAndGenerateLocations(whs, "Row" + i, 3, 3);
			}
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;

			var dbHitsBefore = webService.Factory.DatabaseLoadCount;
			webService.GetLocation($"Row{numberOfRecords - 1}-3-3");
			AssertGreaterThanOrEqualTo(expectedMaxNumberOfDBHits, webService.Factory.DatabaseLoadCount - dbHitsBefore);
		}

		#endregion

		#region TestGetLocation_IsVoidLocation

		public void TestGetLocation_IsVoidLocation()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 4, 1);
			Helper.Factory.Save();

			whs.FindLocation("A-1").WLV_LocationStatus = LocationStatus.Codes.Damaged;
			whs.FindLocation("A-2").WLV_LocationStatus = LocationStatus.Codes.Held;
			whs.FindLocation("A-3").WLV_LocationStatus = LocationStatus.Codes.Normal;
			whs.FindLocation("A-4").WLV_LocationStatus = LocationStatus.Codes.Void;

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			AssertEquals(false, webService1.GetLocation("A-1").IsVoidLocation);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			AssertEquals(false, webService2.GetLocation("A-2").IsVoidLocation);

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			AssertEquals(false, webService3.GetLocation("A-3").IsVoidLocation);

			var webService4 = GetNewWebService();
			webService4.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			AssertEquals(true, webService4.GetLocation("A-4").IsVoidLocation);

			var webService5 = GetNewWebService();
			webService5.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var errorResponse = webService5.GetLocation("B-1");
			AssertEquals("", errorResponse.Location);
			AssertEquals("Location B-1 does not exist in warehouse WHS", errorResponse.ErrorMessage);
		}

		#endregion

		#region TestGetLocation_IsDockDoorLocation

		public void TestGetLocation_IsDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XY1", "Test1", false, 0, LocationClasses.Codes.DDL);
			var normalLocationType = Helper.CreateLocationType("XY2", "Test2", false, 0, LocationClasses.Codes.NOR);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WLT_LocationType = normalLocationType.PK;
			locations[1].WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertEquals(false, webService1.GetLocation(locations[0].ToLocationString()).IsDockDoorLocation);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertEquals(true, webService2.GetLocation(locations[1].ToLocationString()).IsDockDoorLocation);
		}

		#endregion

		#region TestGetLocation_IsPackingStation

		public void TestGetLocation_IsPackingStation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var packingStationType = Helper.CreateLocationType("XY1", "Test1", false, 0, LocationClasses.Codes.PST);
			var normalLocationType = Helper.CreateLocationType("XY2", "Test2", false, 0, LocationClasses.Codes.NOR);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WLT_LocationType = normalLocationType.PK;
			locations[1].WLV_WLT_LocationType = packingStationType.PK;

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertEquals(false, webService1.GetLocation(locations[0].ToLocationString()).IsPackingStation);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertEquals(true, webService2.GetLocation(locations[1].ToLocationString()).IsPackingStation);
		}

		#endregion

		#region TestGetLocation_IsPackingStation_InvalidStatus

		public void TestGetLocation_IsPackingStation_InvalidStatus()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var packingStationType = Helper.CreateLocationType("XY1", "Test1", false, 0, LocationClasses.Codes.PST);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WLT_LocationType = packingStationType.PK;
			locations[0].WLV_LocationStatus = LocationStatus.Codes.Damaged;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetLocation(locations[0].WLV_LocationString);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals($"Location {locations[0].WLV_LocationString} is an invalid Packing Station Location in Warehouse {data.Whs1.WW_WarehouseNameMultilingual}", response.ErrorMessage);
			AssertEquals("", response.Location);
		}

		#endregion

		#region TestGetLocation_IsPackingConsolidation

		public void TestGetLocation_IsPackingConsolidation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var packingConsolidationType = Helper.CreateLocationType("XY1", "Test1", false, 0, LocationClasses.Codes.CON);
			var normalLocationType = Helper.CreateLocationType("XY2", "Test2", false, 0, LocationClasses.Codes.NOR);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WLT_LocationType = normalLocationType.PK;
			locations[1].WLV_WLT_LocationType = packingConsolidationType.PK;

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertEquals(false, webService1.GetLocation(locations[0].ToLocationString()).IsPackingConsolidation);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertEquals(true, webService2.GetLocation(locations[1].ToLocationString()).IsPackingConsolidation);
		}

		#endregion

		#region TestGetLocation_CapacityChecks

		public void TestGetLocation_CapacityChecks()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 3, 1);
			Helper.Factory.Save();

			var locA1 = whs.FindLocation("A-1");
			var locA2 = whs.FindLocation("A-2");
			var locA3 = whs.FindLocation("A-3");
			locA1.WLV_MaxQuantity = 0;
			locA2.WLV_MaxQuantity = 5;
			locA3.WLV_MaxQuantity = 5;

			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");

			var receiveFinalised = Helper.CreateWhsReceive(client, whs, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveFinalised, product, 1m, locA2);
			Helper.CreateWhsReceiveInventoryLine(receiveFinalised, product, 1m, locA3);
			receiveFinalised.FinaliseDocketWithoutUserConfirmation();

			var receiveUnFinalised = Helper.CreateWhsReceive(client, whs, "R2");
			Helper.CreateWhsReceiveInventoryLine(receiveUnFinalised, product, 4m, locA2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var responseA1 = webService1.GetLocation("A-1");
			AssertEquals(decimal.MaxValue, responseA1.QuantityLeftUntilFull);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var responseA2 = webService2.GetLocation("A-2");
			AssertEquals(0m, responseA2.QuantityLeftUntilFull);

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var responseA3 = webService3.GetLocation("A-3");
			AssertEquals(4m, responseA3.QuantityLeftUntilFull);
		}

		public void TestGetLocation_CapacityChecks_ExcludesCancelledReceive()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 3, 1);
			Helper.Factory.Save();

			var location = whs.DefaultOutboundDockDoorLocation;
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			location.WLV_MaxQuantity = 10;
			Helper.Factory.Save();

			var cancelledReceive = Helper.CreateWhsReceive(client, whs, "R1");
			var cancelledReceiveLine = Helper.CreateWhsReceiveLine(cancelledReceive, product, 2m, location);
			cancelledReceive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			cancelledReceiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
			cancelledReceiveLine.WE_StockOnHand = 0m;
			Helper.Factory.Save();

			AssertEquals(DocketStatus.Codes.Cancelled, cancelledReceive.WD_DocketStatus);
			AssertEquals(DocketLineStatus.Codes.Cancelled, cancelledReceiveLine.WE_DocketLineStatus);

			var webService = GetNewWebService(whs);
			var response = webService.GetLocation(location.ToLocationString());
			AssertEquals("Quantity left until full is max quantity.", 10m, response.QuantityLeftUntilFull);
		}

		public void TestGetLocation_CapacityChecks_PickedForUnload()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var normalLocation = data.Whs1.DefaultLocation;
			dockDoorLocation.WLV_MaxQuantity = 10m;
			normalLocation.WLV_MaxQuantity = 10m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, dockDoorLocation, "PLT-1");
			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.CreateWhsTransferLine(putawayTransfer, data.Part1, 7m, dockDoorLocation.ToLocationString(), "PLT-1", normalLocation.ToLocationString(), "PLT-1");
			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			transferLine.RunPreSaveValidation(); // to commit inventory
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition", DocketLineStatus.Codes.PickedForUnload, receiveLine.WE_DocketLineStatus);
			AssertEquals("Precondition", 7m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var service1 = GetNewWebService(data.Whs1);
			var response1 = service1.GetLocation(dockDoorLocation.ToLocationString());
			AssertEquals("Quantity left until full is max quantity as the stock is already picked from DDL.", 10m, response1.QuantityLeftUntilFull);

			var service2 = GetNewWebService(data.Whs1);
			var response2 = service2.GetLocation(normalLocation.ToLocationString());
			AssertEquals("Quantity left until full should take picked for putaway stock into account.", 3m, response2.QuantityLeftUntilFull);
		}

		#endregion

		#region TestGetLocation_LocationFormattedCheckDigit

		public void TestGetLocation_LocationFormattedCheckDigit()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 3, 1);
			Helper.Factory.Save();

			var location1 = whs.FindLocation("A-1");
			var location2 = whs.FindLocation("A-2");
			location1.FormattedCheckDigit = "11";
			location2.FormattedCheckDigit = "22";

			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");

			var receive1 = Helper.CreateWhsReceive(client, whs, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, product, 1m, location1);

			var receive2 = Helper.CreateWhsReceive(client, whs, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, product, 1m, location2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var response1 = webService1.GetLocation("A-1");
			AssertEquals("11", response1.LocationFormattedCheckDigit);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var response2 = webService2.GetLocation("A-2");
			AssertEquals("22", response2.LocationFormattedCheckDigit);
		}

		#endregion

		#endregion
	}
}
