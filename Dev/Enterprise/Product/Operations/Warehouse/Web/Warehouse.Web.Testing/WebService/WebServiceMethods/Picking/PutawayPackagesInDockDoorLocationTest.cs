using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PutawayPackagesInDockDoorLocationTest : WhsSecureServiceTestCase
	{
		public void TestPutawayPackagesInDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);
			package1.KP_ClosedTimeUtc = DateTime.UtcNow;

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 15m);
			package2.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			AssertEquals("Precondition", true, package1.IsClosed);
			AssertEquals("Precondition", true, package2.IsClosed);

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 2, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackagesInDockDoorLocation(new[] { package1.PK.ToGuid(), package2.PK.ToGuid() }, "DOCKDOOR");
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(4, transferInNewFactory.Lines.Count);

			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var newTransferLinePart1 = transfer.Lines.Single(l => l.PK != transferLine1.PK && l.WE_OP == data.Part1.PK);
			AssertEquals(nameof(newTransferLinePart1.WE_TransactionQuantity), 10m, newTransferLinePart1.WE_TransactionQuantity);
			AssertEquals(nameof(newTransferLinePart1.WE_StockOnHand), 10m, newTransferLinePart1.WE_StockOnHand);
			AssertEquals(nameof(newTransferLinePart1.WE_WL), dockDoorLocation.PK, newTransferLinePart1.WE_WL);
			Assert(newTransferLinePart1.IsFinalised);

			var newTransferLinePart2 = transfer.Lines.Single(l => l.PK != transferLine2.PK && l.WE_OP == data.Part2.PK);
			AssertEquals(nameof(newTransferLinePart2.WE_TransactionQuantity), 15m, newTransferLinePart2.WE_TransactionQuantity);
			AssertEquals(nameof(newTransferLinePart2.WE_StockOnHand), 15m, newTransferLinePart2.WE_StockOnHand);
			AssertEquals(nameof(newTransferLinePart2.WE_WL), dockDoorLocation.PK, newTransferLinePart2.WE_WL);
			Assert(newTransferLinePart2.IsFinalised);
		}

		public void TestPutawayPackagesInDockDoorLocation_OverrideDDL()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var pstRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = pstRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var dockDoorLocationType = Helper.CreateLocationType("DDL", "DDL Test", false, 0, LocationClasses.Codes.DDL);
			var ddlRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "DDL");
			var otherDDL = ddlRow.Locations[0];
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			pickPackParam.WPP_IsPickAndPackEnabled = false;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);
			package1.KP_ClosedTimeUtc = DateTime.UtcNow;

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 15m);
			package2.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			AssertEquals("Precondition", true, package1.IsClosed);
			AssertEquals("Precondition", true, package2.IsClosed);

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 2, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackagesInDockDoorLocation([package1.PK.ToGuid(), package2.PK.ToGuid()], otherDDL.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(4, transferInNewFactory.Lines.Count);

			var newTransferLinePart1 = transfer.Lines.Single(l => l.PK != transferLine1.PK && l.WE_OP == data.Part1.PK);
			AssertEquals(nameof(newTransferLinePart1.WE_TransactionQuantity), 10m, newTransferLinePart1.WE_TransactionQuantity);
			AssertEquals(nameof(newTransferLinePart1.WE_StockOnHand), 10m, newTransferLinePart1.WE_StockOnHand);
			AssertEquals(nameof(newTransferLinePart1.WE_WL), otherDDL.PK, newTransferLinePart1.WE_WL);
			Assert(newTransferLinePart1.IsFinalised);

			var newTransferLinePart2 = transfer.Lines.Single(l => l.PK != transferLine2.PK && l.WE_OP == data.Part2.PK);
			AssertEquals(nameof(newTransferLinePart2.WE_TransactionQuantity), 15m, newTransferLinePart2.WE_TransactionQuantity);
			AssertEquals(nameof(newTransferLinePart2.WE_StockOnHand), 15m, newTransferLinePart2.WE_StockOnHand);
			AssertEquals(nameof(newTransferLinePart2.WE_WL), otherDDL.PK, newTransferLinePart2.WE_WL);
			Assert(newTransferLinePart2.IsFinalised);
		}

		public void TestPutawayPackagesInDockDoorLocation_OverrideDDL_ParamOff()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var pstRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = pstRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var dockDoorLocationType = Helper.CreateLocationType("DDL", "DockDoor", false, 0, LocationClasses.Codes.DDL);
			var ddlRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "DDL");
			var otherDDL = ddlRow.Locations[0];
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = false;
			pickPackParam.WPP_IsPickAndPackEnabled = false;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);
			package1.KP_ClosedTimeUtc = DateTime.UtcNow;

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 15m);
			package2.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			AssertEquals("Precondition", true, package1.IsClosed);
			AssertEquals("Precondition", true, package2.IsClosed);

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 2, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackagesInDockDoorLocation([package1.PK.ToGuid(), package2.PK.ToGuid()], otherDDL.WLV_LocationString);
			AssertEquals("Should be error.", $"Cannot override Dock Door Location as Client '{data.Org1.OH_FullNameTruncated}' does not allow overrides", response.ErrorMessage);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(2, transferInNewFactory.Lines.Count);
		}

		public void TestPutawayPackagesInDockDoorLocation_OverrideDDL_PickByLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var pstRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = pstRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var dockDoorLocationType = Helper.CreateLocationType("DDL", "Dock door location", false, 0, LocationClasses.Codes.DDL);
			var ddlRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "DDL");
			var otherDDL = ddlRow.Locations[0];
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			pickPackParam.WPP_IsPickAndPackEnabled = false;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);
			package1.KP_ClosedTimeUtc = DateTime.UtcNow;

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 15m);
			package2.KP_ClosedTimeUtc = DateTime.UtcNow;

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package2.PK);
			Factory.Save();

			AssertEquals("Precondition", true, package1.IsClosed);
			AssertEquals("Precondition", true, package2.IsClosed);
			AssertEquals("Precondition", data.Whs1.WW_DefaultOutboundDockDoor, pickByLabelJob.WTK_WL_DockDoor);

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 2, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackagesInDockDoorLocation([package1.PK.ToGuid()], otherDDL.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(3, transferInNewFactory.Lines.Count);

			var newTransferLinePart1 = transfer.Lines.Single(l => l.PK != transferLine1.PK && l.WE_OP == data.Part1.PK);
			AssertEquals(nameof(newTransferLinePart1.WE_WL), otherDDL.PK, newTransferLinePart1.WE_WL);
			Assert(newTransferLinePart1.IsFinalised);

			AssertEquals("DDL on pick by label job updated", otherDDL.PK, pickByLabelJob.WTK_WL_DockDoor);
		}

		public void TestPutawayPackagesInDockDoorLocation_OverrideDDL_OtherPackageInConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingStationLocation = newRow.Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var dockDoorLocationType = Helper.CreateLocationType("DDL", "Dock door location", false, 0, LocationClasses.Codes.DDL);
			var ddlRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "DDL");
			var otherDDL = ddlRow.Locations[0];
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			pickPackParam.WPP_IsPickAndPackEnabled = false;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingStationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingStationLocation.PK;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var webService1 = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingStationLocation.WLV_LocationString_UserFriendly);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);
			package1.KP_ClosedTimeUtc = DateTime.UtcNow;

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 15m);
			package2.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			AssertEquals("Precondition", true, package1.IsClosed);
			AssertEquals("Precondition", true, package2.IsClosed);

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var pickLine1InNewFactory = orderLine1InNewFactory.PickLines.Single();
			var newTransferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine1.WE_WL = consolidationLocation.PK;
			newTransferLine1.FinaliseDocketLine();

			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			var pickLine2InNewFactory = orderLine2InNewFactory.PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine2InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newFactory.Save();

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(4, transferInNewFactory.Lines.Count);

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.PutawayPackagesInDockDoorLocation([package2.PK.ToGuid()], otherDDL.WLV_LocationString);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals($"Package cannot be putaway to '{otherDDL.WLV_LocationString}' as some packages on the order is in 'PS1'.", response.ErrorMessage);
		}

		public void TestPutawayPackagesInDockDoorLocation_TransferFromPackingStation_OtherPackageInConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingStationLocation = newRow.Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingStationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingStationLocation.PK;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var webService1 = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingStationLocation.WLV_LocationString_UserFriendly);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);
			package1.KP_ClosedTimeUtc = DateTime.UtcNow;

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 15m);
			package2.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			AssertEquals("Precondition", true, package1.IsClosed);
			AssertEquals("Precondition", true, package2.IsClosed);

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var pickLine1InNewFactory = orderLine1InNewFactory.PickLines.Single();
			var newTransferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine1.WE_WL = consolidationLocation.PK;
			newTransferLine1.FinaliseDocketLine();

			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			var pickLine2InNewFactory = orderLine2InNewFactory.PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine2InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newFactory.Save();

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(4, transferInNewFactory.Lines.Count);

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.PutawayPackagesInDockDoorLocation(new[] { package2.PK.ToGuid() }, "DOCKDOOR");
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package cannot be putaway to 'DOCKDOOR' as some packages on the order is in 'PS1'.", response.ErrorMessage);
		}

		public void TestPutawayPackagesInDockDoorLocation_OutboundTransferExists()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine1.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			AssertEquals("Precondition: Outbound Transfer to dockdoor", transferLine.WE_WL, data.Whs1.DefaultInboundDockDoorLocation.PK);
			AssertEquals("Precondition: Outbound Transfer to dockdoor", false, transferLine.IsFinalised);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 1, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackagesInDockDoorLocation(new[] { package.PK.ToGuid() }, "DOCKDOOR");
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals("No new transfer lines created.", 1, transferInNewFactory.Lines.Count);

			var transferLineInNewFactory = transferInNewFactory.Lines[0];
			Assert(transferLineInNewFactory.IsFinalised);
		}

		public void TestPutawayPackagesInDockDoorLocation_OutboundTransferExists_TransferToConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = consolidationLocation.PK;
			AssertEquals("Precondition: Outbound Transfer to dockdoor", false, transferLine.IsFinalised);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 1, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackagesInDockDoorLocation(new[] { package.PK.ToGuid() }, "DOCKDOOR");
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals("No new transfer lines created.", 1, transferInNewFactory.Lines.Count);

			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var transferLineInNewFactory = transferInNewFactory.Lines[0];
			AssertEquals(dockDoorLocation.PK, transferLineInNewFactory.WE_WL);
			Assert(transferLineInNewFactory.IsFinalised);
		}

		public void TestPutawayPackagesInDockDoorLocation_LocationCannotBeFound()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var webService = GetNewWebService(data.Whs1);
			var errorResponse = webService.PutawayPackagesInDockDoorLocation(new[] { Guid.NewGuid() }, "NONEXISTINGLOCATIONCODE");
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse.Error);
			AssertEquals("Location NONEXISTINGLOCATIONCODE does not exist in warehouse 1", errorResponse.ErrorMessage);
		}

		public void TestPutawayPackagesInDockDoorLocation_NotADockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var webService = GetNewWebService(data.Whs1);
			Factory.Save();

			var locationString = data.Whs1.DefaultLocation.WLV_LocationString;
			var errorResponse = webService.PutawayPackagesInDockDoorLocation(new[] { Guid.NewGuid() }, locationString);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse.Error);
			AssertEquals($"Location {locationString} is not an Outbound Dock Door Location.", errorResponse.ErrorMessage);
		}

		public void TestPutawayPackagesInDockDoorLocation_PackageNotFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var errorResponse = webService.PutawayPackagesInDockDoorLocation(new[] { package.PK.ToGuid(), Guid.NewGuid() }, "DOCKDOOR");
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse.Error);
			AssertEquals("Some Package(s) were not found in Warehouse. 2 were expected, but 1 packages were found.", errorResponse.ErrorMessage);
		}

		public void TestPutawayPackagesInDockDoorLocation_PackageIsOpen()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			AssertEquals("Precondition: Outbound Transfer to dockdoor", transferLine.WE_WL, data.Whs1.DefaultInboundDockDoorLocation.PK);
			AssertEquals("Precondition: Outbound Transfer to dockdoor", false, transferLine.IsFinalised);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Factory.Save();

			AssertEquals("Precondition", false, package.IsClosed);

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 1, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var errorResponse = webService.PutawayPackagesInDockDoorLocation(new[] { package.PK.ToGuid() }, "DOCKDOOR");
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse.Error);
			AssertEquals("Not all packages that are to be put away to the dock door are closed.", errorResponse.ErrorMessage);
		}

		public void TestPutawayPackagesInDockDoorLocation_PickLineAlreadyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var errorResponse = webService.PutawayPackagesInDockDoorLocation(new[] { package.PK.ToGuid() }, "DOCKDOOR");
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse.Error);
			AssertEquals("Some Pick Lines on the package have been picked already or is not picking from a packing station location.", errorResponse.ErrorMessage);
		}

		public void TestPutawayPackagesInDockDoorLocation_ConcurrencyException()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 1, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)package).Row, Db.Connection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Factory);

			var response = webService.PutawayPackagesInDockDoorLocation(new[] { package.PK.ToGuid() }, "DOCKDOOR");
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Another user has made changes while you have been working on the packages. Please restart the operation and try again.", response.ErrorMessage);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals("No new outbound transfer lines.", 1, transferInNewFactory.Lines.Count);
		}

		public void TestPutawayPackagesInDockDoorLocation_PickPalletByLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Helper.CreateProductUnit(data.Part1, "PLT", 10);
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingStationLocation = newRow.Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_PalletID = "PLT1";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			var packagePLT = pick.OuterPackages.Single(package => package.PackType.F3_Code.Equals("PLT"));
			var pickLine = orderLine.PickLines.Single();
			AssertEquals("Precondition.", UOMPackTypesList.Codes.Pallet, pickLine.AllocatedPackType.F3_UOMType);
			Factory.Save();

			var pickLine1 = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingStationLocation.PK;
			AssertEquals("PLT1", transferLine.WE_PalletID);
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, packagePLT.PK);
			Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = webService1.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, packingStationLocation.WLV_LocationString_UserFriendly);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);
			var pickLine1InNewFactory = orderLine1InNewFactory.PickLines.Single();
			var newTransferLine = Helper.PickAndMakeInTransitTransfer(pickLine1InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			AssertEquals("PLT1", newTransferLine.WE_PalletID);
			newFactory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.PutawayPackagesInDockDoorLocation(new[] { packagePLT.PK.ToGuid() }, "DOCKDOOR");
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);
		}

		public void TestPutawayPackagesInDockDoorLocation_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct($"P1{i}", data.Org1);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", product, 10m);
				products.Add(product);
			}
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLines = new List<WhsOrderLine>();
			for (var i = 0; i < 10; i++)
			{
				var product = products[i];
				var orderLine = Helper.CreateWhsOrderLine(order, product, 10m);
				orderLines.Add(orderLine);
			}
			var pick = Helper.CreatePickNew(order);

			foreach (var orderLine in orderLines)
			{
				var pickLine = orderLine.PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_WL = packingLocation.PK;
				transferLine.FinaliseDocketLine();
			}
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packages = new List<PkgPackage>();
			foreach (var orderLine in orderLines)
			{
				var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
				package.Pack(orderLine.ReleaseLines[0], 10m);
				package.KP_ClosedTimeUtc = DateTime.UtcNow;
				packages.Add(package);
			}
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 10, transfer.Lines.Count);

			var expectedDBHits = new Dictionary<string, int>
			{
				{ CusClassPartPivotSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartBOMSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 4 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 5 },
				{ WhsRowSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.PutawayPackagesInDockDoorLocation(packages.Select(p => p.PK.ToGuid()).ToArray(), "DOCKDOOR");
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			}

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(20, transferInNewFactory.Lines.Count);
		}

		#region Implementation

		BusinessObjectFactory Factory => Helper.Factory;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}
