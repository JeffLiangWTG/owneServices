using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetOrdersReadyToPackInPackingLocationTest : WhsSecureServiceTestCase
	{
		public void TestGetOrdersReadyToPackInPackingLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order2.Lines.Single().PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();

			order1.WD_GS_NKAssignedPacker = "AAA";
			order2.WD_GS_NKAssignedPacker = "AAA";

			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order1.WarehouseOrderStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order2.WarehouseOrderStatus);
			AssertEquals("Precondition", "AAA", order1.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", "AAA", order2.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", 0, order1.PackageJob.Packages.Count);
			AssertEquals("Precondition", 0, order2.PackageJob.Packages.Count);
			AssertEquals("Precondition", packingLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", packingLocation.PK, transferLine2.WE_WL);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.GetOrdersReadyToPackInPackingLocation(packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			AssertNotNull(response.Orders);
			AssertEquals(2, response.Orders.Length);
			AssertContainsExactElementsInAnyOrder(response.Orders.Select(o => o.PK), new[] { order1.PK.ToGuid(), order2.PK.ToGuid() });
		}

		public void TestGetOrdersReadyToPackInPackingLocation_OrderNotReadyToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderline1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderline2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;

			order.WD_GS_NKAssignedPacker = "AAA";

			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.InTransit, transferLine2.WE_CurrentInventoryStatus);
			AssertNotEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("Precondition", "AAA", order.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", 0, order.PackageJob.Packages.Count);
			AssertEquals("Precondition", packingLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", packingLocation.PK, transferLine2.WE_WL);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.GetOrdersReadyToPackInPackingLocation(packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			AssertNotNull(response.Orders);
			AssertEquals(0, response.Orders.Length);
		}

		public void TestGetOrdersReadyToPackInPackingLocation_PackedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order2.Lines.Single().PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();

			order1.WD_GS_NKAssignedPacker = "AAA";
			order2.WD_GS_NKAssignedPacker = "AAA";

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order1.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order1.WarehouseOrderStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order2.WarehouseOrderStatus);
			AssertEquals("Precondition", "AAA", order1.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", "AAA", order2.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", 1, order1.PackageJob.Packages.Count);
			AssertEquals("Precondition", 0, order2.PackageJob.Packages.Count);
			AssertEquals("Precondition", packingLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", packingLocation.PK, transferLine2.WE_WL);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.GetOrdersReadyToPackInPackingLocation(packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			AssertNotNull(response.Orders);
			AssertEquals(1, response.Orders.Length);
			AssertContainsExactElementsInAnyOrder(response.Orders.Select(o => o.PK), new[] { order2.PK.ToGuid() });
		}

		public void TestGetOrdersReadyToPackInPackingLocation_NotInPackingStation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order2.Lines.Single().PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.FinaliseDocketLine();

			order1.WD_GS_NKAssignedPacker = "AAA";
			order2.WD_GS_NKAssignedPacker = "AAA";

			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Staged, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Staged, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.Staged, order1.WarehouseOrderStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.Staged, order2.WarehouseOrderStatus);
			AssertEquals("Precondition", "AAA", order1.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", "AAA", order2.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", 0, order1.PackageJob.Packages.Count);
			AssertEquals("Precondition", 0, order2.PackageJob.Packages.Count);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.GetOrdersReadyToPackInPackingLocation(packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			AssertNotNull(response.Orders);
			AssertEquals(0, response.Orders.Length);
		}

		public void TestGetOrdersReadyToPackInPackingLocation_UnassignedOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order2.Lines.Single().PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();

			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order1.WarehouseOrderStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order2.WarehouseOrderStatus);
			AssertEquals("Precondition", string.Empty, order1.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", string.Empty, order2.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", 0, order1.PackageJob.Packages.Count);
			AssertEquals("Precondition", 0, order2.PackageJob.Packages.Count);
			AssertEquals("Precondition", packingLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", packingLocation.PK, transferLine2.WE_WL);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.GetOrdersReadyToPackInPackingLocation(packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			AssertNotNull(response.Orders);
			AssertEquals(2, response.Orders.Length);
			AssertContainsExactElementsInAnyOrder(response.Orders.Select(o => o.PK), new[] { order1.PK.ToGuid(), order2.PK.ToGuid() });
		}

		public void TestGetOrdersReadyToPackInPackingLocation_OrderAssignedToOtherUser()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user1 = Helper.CreateGlbStaff("AAA", "AAAAA");
			var user2 = Helper.CreateGlbStaff("BBB", "BBBBB");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order2.Lines.Single().PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();

			order1.WD_GS_NKAssignedPacker = "AAA";
			order2.WD_GS_NKAssignedPacker = "BBB";

			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order1.WarehouseOrderStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order2.WarehouseOrderStatus);
			AssertEquals("Precondition", "AAA", order1.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", "BBB", order2.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", 0, order1.PackageJob.Packages.Count);
			AssertEquals("Precondition", 0, order2.PackageJob.Packages.Count);
			AssertEquals("Precondition", packingLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", packingLocation.PK, transferLine2.WE_WL);

			var webService = GetNewWebService(data.Whs1, staff: user1);
			var response = webService.GetOrdersReadyToPackInPackingLocation(packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			AssertNotNull(response.Orders);
			AssertEquals(1, response.Orders.Length);
			AssertContainsExactElementsInAnyOrder(response.Orders.Select(o => o.PK), new[] { order1.PK.ToGuid() });
		}

		public void TestGetOrdersReadyToPackInPackingLocation_NotReadyToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;

			var pickLine2 = order2.Lines.Single().PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();

			order1.WD_GS_NKAssignedPacker = "AAA";
			order2.WD_GS_NKAssignedPacker = "AAA";

			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.InTransit, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.Staged, order1.WarehouseOrderStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order2.WarehouseOrderStatus);
			AssertEquals("Precondition", "AAA", order1.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", "AAA", order2.WD_GS_NKAssignedPacker);
			AssertEquals("Precondition", 0, order1.PackageJob.Packages.Count);
			AssertEquals("Precondition", 0, order2.PackageJob.Packages.Count);
			AssertEquals("Precondition", packingLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", packingLocation.PK, transferLine2.WE_WL);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.GetOrdersReadyToPackInPackingLocation(packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			AssertNotNull(response.Orders);
			AssertEquals(1, response.Orders.Length);
			AssertContainsExactElementsInAnyOrder(response.Orders.Select(o => o.PK), new[] { order2.PK.ToGuid() });
		}

		public void TestGetOrdersReadyToPackInPackingLocation_PickByBOM()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);
			factory.Save();
			helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			factory.Save();

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = order.Lines.Single(l => l.WE_OP == data.Part2.PK).PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			factory.Save();

			var webService1 = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var putawayResponse = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no error.", ErrorTypes.None, putawayResponse.Error);
				AssertEquals("Should have no error.", true, string.IsNullOrEmpty(putawayResponse.ErrorMessage));
			});

			var webService2 = GetNewWebService(data.Whs1, staff: GlbStaff.CurrentUser);
			var getOrdersToPackResponse = webService2.GetOrdersReadyToPackInPackingLocation(packingLocation.WLV_LocationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no error.", ErrorTypes.None, getOrdersToPackResponse.Error);
				AssertEquals("Should have no error.", true, string.IsNullOrEmpty(getOrdersToPackResponse.ErrorMessage));
			});

			AssertNotNull(getOrdersToPackResponse.Orders);
			AssertEquals(1, getOrdersToPackResponse.Orders.Length);
			AssertContainsExactElementsInAnyOrder(getOrdersToPackResponse.Orders.Select(o => o.PK), new[] { order.PK.ToGuid() });
		}

		public void TestGetOrdersReadyToPackInPackingLocation_PickByBOM_ToteTrolleyPicked()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = order.Lines.Single(l => l.WE_OP == data.Part2.PK).PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLineToPack = order.Lines.Single(l => l.WE_OP == data.Part1.PK).PickLines.Single();
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLineToPack);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);
			webService.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var putawayResponse = webService1.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no error.", ErrorTypes.None, putawayResponse.Error);
				AssertEquals("Should have no error.", true, string.IsNullOrEmpty(putawayResponse.ErrorMessage));
			});

			var webService2 = GetNewWebService(data.Whs1, staff: GlbStaff.CurrentUser);
			var getOrdersToPackResponse = webService2.GetOrdersReadyToPackInPackingLocation(packingLocation.WLV_LocationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no error.", ErrorTypes.None, getOrdersToPackResponse.Error);
				AssertEquals("Should have no error.", true, string.IsNullOrEmpty(getOrdersToPackResponse.ErrorMessage));
			});

			AssertNotNull(getOrdersToPackResponse.Orders);
			AssertEquals(0, getOrdersToPackResponse.Orders.Length);
		}

		public void TestGetOrdersReadyToPackInPackingLocation_LocationNotPackingStation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetOrdersReadyToPackInPackingLocation(data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals($"Location '{data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString}' is not a Packing Station.", response.ErrorMessage);
		}

		public void TestGetOrdersReadyToPackInPackingLocation_PackingStationDoesNotExistInWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetOrdersReadyToPackInPackingLocation("Idonotexist");
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals($"Packing Station 'Idonotexist' does not exist in warehouse {data.Whs1.WW_WarehouseNameMultilingual}.", response.ErrorMessage);
		}

		public void TestGetOrdersReadyToPackInPackingLocation_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var numberOfOrders = 10;
			var transferLines = new List<WhsTransferLine>();
			var orders = new List<WhsOrder>();
			for (var i = 0; i < numberOfOrders; i++)
			{
				var product1 = Helper.CreateProduct(data.Org1, $"P1{i}");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R1{i}", product1, 10m);

				var product2 = Helper.CreateProduct(data.Org1, $"P2{i}");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R2{i}", product2, 10m);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O{i}");
				var orderLine1 = Helper.CreateWhsOrderLine(order, product1, 10m);
				var orderLine2 = Helper.CreateWhsOrderLine(order, product2, 10m);
				var pick = Helper.CreatePickNew(order);

				var pickLine1 = orderLine1.PickLines.Single();
				var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
				transferLine1.WE_WL = packingLocation.PK;
				transferLine1.FinaliseDocketLine();
				transferLines.Add(transferLine1);

				var pickLine2 = orderLine2.PickLines.Single();
				var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
				transferLine2.WE_WL = packingLocation.PK;
				transferLine2.FinaliseDocketLine();
				transferLines.Add(transferLine2);

				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
				var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
				package.Pack(orderLine2.ReleaseLines[0], 10m);

				order.WD_GS_NKAssignedPacker = "AAA";
				orders.Add(order);
				Factory.Save();
			}

			Assert("Precondition", transferLines.All(line => line.WE_CurrentInventoryStatus == InventoryStatus.Codes.ReadyToPack && line.WE_WL == packingLocation.PK));
			Assert("Precondition", orders.All(order => order.WD_GS_NKAssignedPacker == "AAA"));

			var webService = GetNewWebService(data.Whs1, staff: user);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.GetOrdersReadyToPackInPackingLocation(packingLocation.WLV_LocationString);
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);

				AssertNotNull(response.Orders);
				AssertEquals(10, response.Orders.Length);
				AssertContainsExactElementsInAnyOrder(response.Orders.Select(o => o.ExternalReference), new[] { "O0", "O1", "O2", "O3", "O4", "O5", "O6", "O7", "O8", "O9" });
			}
		}

		#region Implementation

		BusinessObjectFactory Factory => Helper.Factory;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}
