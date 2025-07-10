using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PutawayStockInDockDoorOrPackingStationTest : WhsSecureServiceTestCase
	{
		#region TestPutawayStockInDockDoorOrPackingStation_LocationCannotBeFound

		public void TestPutawayStockInDockDoorOrPackingStation_LocationCannotBeFound()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var pick = helper.CreatePickNew();
			webService.Factory.Save();

			var errorResponse = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, "NONEXISTINGLOCATIONCODE");
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse.Error);
			AssertEquals("Location NONEXISTINGLOCATIONCODE does not exist in warehouse 1", errorResponse.ErrorMessage);
			AssertExpectedLocationDetails(errorResponse, location: null);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_JobCannotBeFound

		public void TestPutawayStockInDockDoorOrPackingStation_JobCannotBeFound()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.Factory.Save();

			var errorResponse1 = webService.PutawayStockInDockDoorOrPackingStation(Guid.NewGuid(), PickJobType.Pick, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse1.Error);
			AssertEquals("Error loading job.", errorResponse1.ErrorMessage);
			AssertExpectedLocationDetails(errorResponse1, location: null);

			var errorResponse2 = webService.PutawayStockInDockDoorOrPackingStation(Guid.NewGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse2.Error);
			AssertEquals("Error loading job.", errorResponse2.ErrorMessage);
			AssertExpectedLocationDetails(errorResponse2, location: null);

			var errorResponse3 = webService.PutawayStockInDockDoorOrPackingStation(Guid.NewGuid(), PickJobType.TrolleyJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse3.Error);
			AssertEquals("Error loading job.", errorResponse3.ErrorMessage);
			AssertExpectedLocationDetails(errorResponse3, location: null);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_NotADockDoorLocation

		public void TestPutawayStockInDockDoorOrPackingStation_NotADockDoorLocationNorPackingLocation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var pick = helper.CreatePickNew();
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultOutboundDockDoor;
			webService.Factory.Save();

			var locationString = data.Whs1.DefaultLocation.WLV_LocationString;
			var errorResponse = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse.Error);
			AssertEquals($"Location {locationString} is not an Outbound Dock Door Location nor a Packing Station Location.", errorResponse.ErrorMessage);
			AssertExpectedLocationDetails(errorResponse, location: null);
			AssertEquals("Should not have changed the dock door on the pick.", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PackingStation_IncorrectLocationStatus

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_IncorrectLocationStatus()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P", levels: 2);
			var damagedLocation = newRow.Locations[0];
			damagedLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			damagedLocation.WLV_LocationStatus = LocationStatus.Codes.Damaged;

			var packingLocation = newRow.Locations[1];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var pick = helper.CreatePickNew();
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultOutboundDockDoor;
			webService.Factory.Save();

			var errorResponse = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, damagedLocation.WLV_LocationString);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse.Error);
			AssertEquals($"Location {damagedLocation.WLV_LocationString} is an invalid Packing Station Location.", errorResponse.ErrorMessage);
			AssertEquals("Should not have changed the dock door on the pick.", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);
			AssertExpectedLocationDetails(errorResponse, location: null);
		}

		#endregion

		#region TestPutawayStockInDockDoor

		public void TestPutawayStockInDockDoor()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLine_InFactory2 = factory2.Load<WhsTransferLine>(inTransitTransferLine.PK);
			AssertLinePutaway(inTransitTransferLine_InFactory2);
			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);
			AssertEquals("Should have changed the dock door on the pick.", ZGuid.Empty, pick.WP_WL_DockDoor);
			AssertEquals("Should have added dock door assignment for correct DDL to pick.", data.Whs1.WW_DefaultInboundDockDoor, pick.DockDoorAssignment.WDA_WL_AssignedDockDoor);
		}

		public void TestPutawayStockInDockDoor_DockDoorAssignment()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var dda = helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick);
			webService.Factory.Save();

			AssertEquals(
				"Precondition: DockDoorAssignment WDA_FirstPutawayToDockDoorUtc should be empty",
				true,
				dda.WDA_FirstPutawayToDockDoorUtc.IsEmpty);

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);
			AssertEquals(
				"DockDoorAssignment WDA_FirstPutawayToDockDoorUtc should be set",
				false,
				dda.WDA_FirstPutawayToDockDoorUtc.IsEmpty);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLine_InFactory2 = factory2.Load<WhsTransferLine>(inTransitTransferLine.PK);
			AssertLinePutaway(inTransitTransferLine_InFactory2);
			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);
			AssertEquals("Should not have changed the dock door on the pick's dock door assignment.", data.Whs1.WW_DefaultOutboundDockDoor, pick.DockDoorAssignment.WDA_WL_AssignedDockDoor);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_Pick_UOMTypesWithoutCartonisation

		public void TestPutawayStockInDockDoorOrPackingStation_Pick_UOMTypesWithoutCartonisation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType_SplitCase = packingHelper.CreateRefPackType("1", "1", 1m, 2m, 3m, Length.Metres, 3, Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			var refType_Case = packingHelper.CreateRefPackType("2", "2", 1m, 4m, 9m, Length.Metres, 3, Weight.Kilograms, UOMPackTypesList.Codes.Case);
			var refType_Pallet = packingHelper.CreateRefPackType("3", "3", 1m, 1m, 3m, Length.Metres, 3, Weight.Kilograms, UOMPackTypesList.Codes.Pallet);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part1.OP_StockKeepingUnit = refType_SplitCase.F3_Code;
			helper.CreateProductUnit(data.Part1, refType_Pallet.F3_Code, 100m);
			helper.CreateProductUnit(data.Part1, refType_Case.F3_Code, 10m);
			webService.Factory.Save();

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 111m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 111m);
			helper.CreateWhsOrderLine(order, data.Part2, 1m);
			var pick = helper.CreatePickNew(order);

			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory2 = factory2.Load<WhsPick>(pick.PK);
			var transferInFactory2 = pickInFactory2.Transfers.Single();

			foreach (var transferLine in transferInFactory2.Lines)
			{
				AssertLinePutaway(transferLine);
			}
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_MultipleLines

		public void TestPutawayStockInDockDoorOrPackingStation_MultipleLines()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();
			AssertEquals("Precondition.", true, pick.Transfers.Single().Lines.Any());
			AssertEquals("Precondition: Should be In-Transit.", true, pick.Transfers.Single().Lines.All(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			foreach (var transferLine in pick.Transfers.SelectMany(t => t.Lines))
			{
				AssertLinePutaway(transferLine);
			}
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_MultipleLines_SavedTogether

		public void TestPutawayStockInDockDoorOrPackingStation_MultipleLines_SavedTogether()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();
			var transfer = pick.Transfers.Single();
			AssertEquals("Precondition.", true, transfer.Lines.Any());
			AssertEquals("Precondition: Should be In-Transit.", true, transfer.Lines.All(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));
			AssertNull("Precondition:LastFinalisingLines_ForTest should be null.", transfer.LastFinalisingLines_ForTest);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			foreach (var transferLine in pick.Transfers.SelectMany(t => t.Lines))
			{
				AssertLinePutaway(transferLine);
			}

			AssertNotNull("Precondition:LastFinalisingLines_ForTest should be null.", transfer.LastFinalisingLines_ForTest);
			AssertContainsExactElementsInAnyOrder(transfer.Lines, transfer.LastFinalisingLines_ForTest);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_MultipleTransfers

		public void TestPutawayStockInDockDoorOrPackingStation_MultipleTransfers()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var client2 = helper.CreateClient("C2");
			helper.CreateProductClientRelationShip(client2, data.Part1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var receive1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part1, 5m);
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "O2", data.Part1, 5m);

			var pick = helper.CreatePickNew(order1, order2);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();
			// Add precondition that we have intransit transfer lines
			AssertEquals("Precondition: Should be In-Transit.", true, pick.Transfers.SelectMany(t => t.Lines).All(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			foreach (var transferLine in pick.Transfers.SelectMany(t => t.Lines))
			{
				AssertLinePutaway(transferLine);
			}
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_LineAlreadyFinalised

		public void TestPutawayStockInDockDoorOrPackingStation_LineAlreadyFinalised()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var staff1 = helper.CreateGlbStaff("BRS", "BRS");
			var staff2 = helper.CreateGlbStaff("MMC", "MMC");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff1.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff1.StaffPlainTextPassword);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();
			AssertEquals("Precondition: Should be In-Transit.", true, pick.Transfers.Single().Lines.All(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));

			var inTransitLine1 = pick.Transfers.Single().Lines[0];
			var inTransitLine2 = pick.Transfers.Single().Lines[1];
			inTransitLine1.FinaliseDocketLine();
			inTransitLine1.WE_GS_NKPutawayBy = "MMC";
			inTransitLine2.WE_GS_NKPutawayBy = "BRS";
			AssertIsFinalisedPrecondition(inTransitLine1);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			AssertLinePutaway(inTransitLine2);
			AssertEquals("First line's Putaway By should not have been changed.", "MMC", inTransitLine1.WE_GS_NKPutawayBy);
			AssertEquals("Second line should have been putaway by RF User.", "BRS", inTransitLine2.WE_GS_NKPutawayBy);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_AssignedPutawayToDifferentLocation

		public void TestPutawayStockInDockDoorOrPackingStation_AssignedPutawayToDifferentLocation_ToDockdoor()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var packingStationType = Helper.CreateLocationType("PST", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;

			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = dockdoorLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var locationString = packingStationLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertBusinessValidationError(webService, "Invalid Location error", "Location PST is not valid for this job as some inventory has already been putaway to DOCKDOOR.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(data.Whs1.DefaultOutboundDockDoorLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_AssignedPutawayToDifferentLocation_ToPackingStation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingStationLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			AssertEquals("Precondition", packingStationLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);

			pick.WP_WL_PackingStation = packingStationLocation.PK;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertBusinessValidationError(webService, "Invalid Location error", "Location DOCKDOOR is not valid for this job as some inventory has already been putaway to PST.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(packingStationLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_AssignedPutawayToDifferentLocation_ToConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("CON", "Consolidation", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CON", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			AssertEquals("Precondition", consolidationLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertExpectedLocationDetails(response, location: null);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_AssignedPutawayToDifferentLocation_TrolleyJob_ToDockdoor()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;

			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			package1.SetIsTote(true);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			package2.SetIsTote(true);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			var slot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, package1, 2);
			var slot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, package2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var locationString = packingStationLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertBusinessValidationError(webService, "Invalid Location error", "Location PST is not valid for this job as some inventory has already been putaway to DOCKDOOR.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(data.Whs1.DefaultOutboundDockDoorLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_AssignedPutawayToDifferentLocation_TrolleyJob_ToPackingStation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;

			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			pick.WP_WL_PackingStation = packingStationLocation.PK;

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingStationLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			package1.SetIsTote(true);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			package2.SetIsTote(true);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertBusinessValidationError(webService, "Invalid Location error", "Location DOCKDOOR is not valid for this job as some inventory has already been putaway to PST.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(packingStationLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_AssignedPutawayToDifferentLocation_TrolleyJob_ToConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("CON", "Consolidation", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CON", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertExpectedLocationDetails(response, location: null);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_AssignedPutawayToDifferentLocation_PickByLabel_ToDockdoor()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);

			Helper.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package2.PK);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var locationString = packingStationLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			AssertBusinessValidationError(webService, "Invalid Location error", "Location PST is not valid for this job as some inventory has already been putaway to DOCKDOOR.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(data.Whs1.DefaultOutboundDockDoorLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_AssignedPutawayToDifferentLocation_PickByLabel_ToPackingStation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingStationLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);

			pick.WP_WL_PackingStation = packingStationLocation.PK;
			Helper.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package2.PK);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			AssertBusinessValidationError(webService, "Invalid Location error", "Location DOCKDOOR is not valid for this job as some inventory has already been putaway to PST.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(packingStationLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_AssignedPutawayToDifferentLocation_PickByLabel_ToConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var consolidationLocationType = Helper.CreateLocationType("CON", "Consolidation", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CON", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);

			Helper.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package2.PK);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertExpectedLocationDetails(response, location: null);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PickFinalised

		public void TestPutawayStockInDockDoorOrPackingStation_PickFinalised()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var staff = helper.CreateGlbStaff("BRS", "BRS");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();
			AssertEquals("Precondition: Should be In-Transit.", true, pick.Transfers.Single().Lines.All(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));

			var inTransitLine = pick.Transfers.Single().Lines[0];
			inTransitLine.WE_GS_NKPutawayBy = "BRS";

			order.FinaliseDocket();
			pick.FinalisePick();
			webService.Factory.Save();

			AssertIsFinalisedPrecondition(order);
			AssertNull("Precondition: PackageJob deleted.", order.PackageJob);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be a warning.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be a warning.", "Cannot complete dock door putaway for a finalized, canceled or already putaway Pick.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_OnlyPutsAwayLinesAssignedToTheUser

		public void TestPutawayStockInDockDoorOrPackingStation_OnlyPutsAwayLinesAssignedToTheUser()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var staff1 = helper.CreateGlbStaff("BRS", "BRS");
			var staff2 = helper.CreateGlbStaff("MMC", "MMC");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff1.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff1.StaffPlainTextPassword);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine3 = helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();
			AssertEquals("Precondition: Should be In-Transit.", true, pick.Transfers.Single().Lines.All(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));

			var inTransitLineAssignedToRfUser = pick.Transfers.Single().Lines[0];
			var inTransitLineAssignedToOtherUser = pick.Transfers.Single().Lines[1];
			var inTransitLineAssignedToNoOne = pick.Transfers.Single().Lines[2];
			inTransitLineAssignedToRfUser.WE_GS_NKPutawayBy = "BRS";
			inTransitLineAssignedToOtherUser.WE_GS_NKPutawayBy = "MMC";
			inTransitLineAssignedToNoOne.WE_GS_NKPutawayBy = "";

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			AssertLinePutaway(inTransitLineAssignedToRfUser);
			AssertLineNotPutaway(inTransitLineAssignedToNoOne);
			AssertLineNotPutaway(inTransitLineAssignedToOtherUser);

			var errorResponse = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be a warning, no stock for this user to putaway.", ErrorTypes.WarningOnly, errorResponse.Error);
			AssertEquals("Should be a warning, no stock for this user to putaway.", "No stock found to Put. Either nothing is In-Transit for the current job and staff, or Put has already been completed.", errorResponse.ErrorMessage);
			AssertExpectedLocationDetails(errorResponse, location: null);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_NothingToPutaway

		public void TestPutawayStockInDockDoorOrPackingStation_NothingToPutaway()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();
			AssertEquals("Precondition: Should be In-Transit.", true, pick.Transfers.Single().Lines.All(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));

			var inTransitLine = pick.Transfers.Single().Lines[0];
			inTransitLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(inTransitLine);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var warningResponse = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be a warning.", ErrorTypes.WarningOnly, warningResponse.Error);
			AssertEquals("Should be a warning.", "No stock found to Put. Either nothing is In-Transit for the current job and staff, or Put has already been completed.", warningResponse.ErrorMessage);
			AssertExpectedLocationDetails(warningResponse, location: null);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_LineNotInTransit

		public void TestPutawayStockInDockDoorOrPackingStation_LineNotInTransit()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var staff = helper.CreateGlbStaff("BRS", "BRS");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			inTransitTransferLine.WE_GS_NKPutawayBy = "BRS";
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			var otherTransferLine = helper.CreateWhsTransferLine(inTransitTransferLine.Docket, data.Part1, 5m, inTransitTransferLine.WE_WL_TransferFrom, inTransitTransferLine.WE_WL);
			otherTransferLine.WE_GS_NKPutawayBy = "BRS";
			otherTransferLine.RunPreSaveValidation();
			webService.Factory.Save();

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLine_InFactory2 = factory2.Load<WhsTransferLine>(inTransitTransferLine.PK);
			AssertLinePutaway(inTransitTransferLine_InFactory2);
			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);
			AssertEquals("Should *not* have finalised other transfer line.", false, otherTransferLine.IsFinalised);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_DifferentLocation

		public void TestPutawayStockInDockDoorOrPackingStation_DifferentLocation()
		{
			// No longer supported, but keeping this code here ensures that the correct location is used on staged stock if it somehow happens due to concurrency
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var client2 = helper.CreateClient("C2");
			helper.CreateProductClientRelationShip(client2, data.Part1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var dockDoorLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "D");
			var otherDDL = newRow.Locations[0];
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part1, 10m);
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order1Line1 = order1.Lines[0];
			var order1Line2 = helper.CreateWhsOrderLine(order1, data.Part1, 5m);

			var order2 = helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "O2", data.Part1, 5m);
			var order2Line1 = order2.Lines[0];
			var order2Line2 = helper.CreateWhsOrderLine(order2, data.Part1, 5m);

			var pick = helper.CreatePickNew(order1, order2);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();
			AssertEquals("Precondition: Should be In-Transit.", true, pick.Transfers.SelectMany(t => t.Lines).All(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));

			var inTransitLineOnTransfer1 = pick.Transfers[0].Lines[0];
			var inTransitLineOnTransfer2 = pick.Transfers[1].Lines[0];
			var putawayLineOnTransfer1 = pick.Transfers[0].Lines[1];
			var putawayLineOnTransfer2 = pick.Transfers[1].Lines[1];
			putawayLineOnTransfer1.FinaliseDocketLine();
			putawayLineOnTransfer2.FinaliseDocketLine();
			inTransitLineOnTransfer2.WE_WL = ZGuid.Empty; // Ensure also set empty dock doors, to handle case where the locations is somehow not set (i.e. bugs)
			AssertIsFinalisedPrecondition(putawayLineOnTransfer1);
			AssertIsFinalisedPrecondition(putawayLineOnTransfer2);

			webService.Factory.Save();

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertBusinessValidationError(webService, "Invalid Location error", "Location D is not valid for this job as some inventory has already been putaway to DOCKDOOR.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(data.Whs1.DefaultOutboundDockDoorLocation));

			AssertEquals("Should *not* have changed Location on the lines that were not Putaway.", data.Whs1.WW_DefaultOutboundDockDoor, inTransitLineOnTransfer1.WE_WL);
			AssertEquals("Should *not* have changed Location on the lines that were not Putaway.", Guid.Empty, inTransitLineOnTransfer2.WE_WL);
			AssertEquals("Should *not* have changed Location on the lines that were already Putaway.", data.Whs1.WW_DefaultOutboundDockDoor, putawayLineOnTransfer1.WE_WL);
			AssertEquals("Should *not* have changed Location on the lines that were already Putaway.", data.Whs1.WW_DefaultOutboundDockDoor, putawayLineOnTransfer2.WE_WL);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_FinalisationFails

		public void TestPutawayStockInDockDoorOrPackingStation_FinalisationFails()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var dockDoorLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "D");
			var otherDDL = newRow.Locations[0];
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();
			AssertEquals("Precondition: Should be In-Transit.", true, pick.Transfers.Single().Lines.All(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));

			var inTransitTransferLine = pick.Transfers[0].Lines[0];
			inTransitTransferLine.WE_WLInfo.ValueChanged += (s, e) => inTransitTransferLine.AddRowError("Some Error!");

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be an error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be an error.", "Failed to Putaway inventory: Error - Docket Line: Some Error!.", response.ErrorMessage);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var transfer_InFactory2 = factory2.Load<WhsTransfer>(pick.Transfers[0].PK);

			foreach (var transferLine in transfer_InFactory2.Lines)
			{
				AssertLineNotPutaway(transferLine);
			}

			AssertEquals("Should *not* have changed the DockDoor on the pick.", data.Whs1.WW_DefaultOutboundDockDoor, factory2.Load<WhsPick>(pick.PK).WP_WL_DockDoor);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_TrolleyJob

		public void TestPutawayStockInDockDoorOrPackingStation_TrolleyJob()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (int i = 0; i < 10; i++) // For 10 pick lines later on
			{
				helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 10 pick lines.", 10, pick.GetAllPickLines().Count());
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var pkg_OnTrolley2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			var pkg_OnTrolley_NotPicked = packingHelper.CreatePackage(pkgJob, "PKG3", 1, PkgUnit.Box);
			var pkg_OnTrolley_PutawayAssignedToAnotherUser = packingHelper.CreatePackage(pkgJob, "PKG4", 1, PkgUnit.Box);
			var pkg_NotOnTrolley = packingHelper.CreatePackage(pkgJob, "PKG5", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg_OnTrolley1, pickLines[0]);
			packingHelper.CreatePackageDivot(pkg_OnTrolley2, pickLines[1]);
			packingHelper.CreatePackageDivot(pkg_OnTrolley_NotPicked, pickLines[2]);
			packingHelper.CreatePackageDivot(pkg_OnTrolley_PutawayAssignedToAnotherUser, pickLines[3]);
			packingHelper.CreatePackageDivot(pkg_NotOnTrolley, pickLines[4]);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			var trolleySlot1 = helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			var trolleySlot2 = helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley2, 2);
			webService.Factory.Save();

			foreach (var pickLine in pickLines.Except(new[] { pickLines[2] }))
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			webService.Factory.Save();

			AssertEquals("Precondition.", 9, pick.Transfers.Single().Lines.Count);
			pickLines[3].InventoryLine.WE_GS_NKPutawayBy = "";
			webService.Factory.Save();

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTrolleyTransferLine1_InFactory2 = factory2.Load<WhsTransferLine>(pickLines[0].WZ_WE_InventoryLine);
			var inTransitTrolleyTransferLine2_InFactory2 = factory2.Load<WhsTransferLine>(pickLines[1].WZ_WE_InventoryLine);
			AssertLinePutaway(inTransitTrolleyTransferLine1_InFactory2);
			AssertLinePutaway(inTransitTrolleyTransferLine2_InFactory2);

			foreach (var transferLine in factory2.Load<WhsPick>(pick.PK).Transfers.Single().Lines.Except(new[] { inTransitTrolleyTransferLine1_InFactory2, inTransitTrolleyTransferLine2_InFactory2 }))
			{
				AssertLineNotPutaway(transferLine);
			}

			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_TrolleyJob_AutoClosePackages_TotePicking()
		{
			TestPutawayStockInDockDoor_TrolleyJob_AutoClosePackagesCore(isPickByTote: true);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_TrolleyJob_AutoClosePackages_CartonPicking()
		{
			TestPutawayStockInDockDoor_TrolleyJob_AutoClosePackagesCore(isPickByTote: false);
		}

		void TestPutawayStockInDockDoor_TrolleyJob_AutoClosePackagesCore(bool isPickByTote)
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (int i = 0; i < 10; i++) // For 10 pick lines later on
			{
				helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 10 pick lines.", 10, pick.GetAllPickLines().Count());
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley1.SetIsTote(isPickByTote);
			packingHelper.CreatePackageDivot(pkg_OnTrolley1, pickLines[0]);

			var pkg_OnTrolley2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			pkg_OnTrolley2.SetIsTote(isPickByTote);
			packingHelper.CreatePackageDivot(pkg_OnTrolley2, pickLines[1]);

			var pkg_OnTrolley_NotPicked = packingHelper.CreatePackage(pkgJob, "PKG3", 1, PkgUnit.Box);
			pkg_OnTrolley_NotPicked.SetIsTote(isPickByTote);
			packingHelper.CreatePackageDivot(pkg_OnTrolley_NotPicked, pickLines[2]);

			var pkg_OnTrolley_PutawayAssignedToAnotherUser = packingHelper.CreatePackage(pkgJob, "PKG4", 1, PkgUnit.Box);
			pkg_OnTrolley_PutawayAssignedToAnotherUser.SetIsTote(isPickByTote);
			packingHelper.CreatePackageDivot(pkg_OnTrolley_PutawayAssignedToAnotherUser, pickLines[3]);

			var pkg_NotOnTrolley = packingHelper.CreatePackage(pkgJob, "PKG5", 1, PkgUnit.Box);
			pkg_NotOnTrolley.SetIsTote(isPickByTote);
			packingHelper.CreatePackageDivot(pkg_NotOnTrolley, pickLines[4]);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley2, 2);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley_NotPicked, 3);
			webService.Factory.Save();

			foreach (var pickLine in pickLines.Except(new[] { pickLines[2] }))
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			webService.Factory.Save();

			AssertEquals("Precondition.", 9, pick.Transfers.Single().Lines.Count);
			pickLines[3].InventoryLine.WE_GS_NKPutawayBy = "";
			webService.Factory.Save();

			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley1.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley2.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley_NotPicked.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley_PutawayAssignedToAnotherUser.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_NotOnTrolley.IsClosed);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTrolleyTransferLine1_InFactory2 = factory2.Load<WhsTransferLine>(pickLines[0].WZ_WE_InventoryLine);
			var inTransitTrolleyTransferLine2_InFactory2 = factory2.Load<WhsTransferLine>(pickLines[1].WZ_WE_InventoryLine);
			AssertLinePutaway(inTransitTrolleyTransferLine1_InFactory2);
			AssertLinePutaway(inTransitTrolleyTransferLine2_InFactory2);

			foreach (var transferLine in factory2.Load<WhsPick>(pick.PK).Transfers.Single().Lines.Except(new[] { inTransitTrolleyTransferLine1_InFactory2, inTransitTrolleyTransferLine2_InFactory2 }))
			{
				AssertLineNotPutaway(transferLine);
			}

			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);

			CombineAssertions(() =>
			{
				AssertEquals("Package closed info is correct.", !isPickByTote, pkg_OnTrolley1.IsClosed);
				AssertEquals("Package closed info is correct.", !isPickByTote, pkg_OnTrolley2.IsClosed);
				AssertEquals("Package is not closed.", false, pkg_OnTrolley_NotPicked.IsClosed);
				AssertEquals("Package is not closed.", false, pkg_OnTrolley_PutawayAssignedToAnotherUser.IsClosed);
				AssertEquals("Package is not closed.", false, pkg_NotOnTrolley.IsClosed);
			});
		}

		public void TestPutawayStockInDockDoorOrPackingStation_TrolleyJob_PickByBOM_CartonPicking()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			var frame = helper.CreateProduct(data.Org1, "FRAME");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			helper.Factory.Save();
			var kitOrder = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = helper.CreatePickNew(kitOrder);
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var kitPickLine2 = kitPickLine1.Split(4m);
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = pick.GetAllPickLines().Single(l => l.WZ_Units == 20m);
			var framePickLine = pick.GetAllPickLines().Single(l => l.WZ_Units == 10m);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var pkg_OnTrolley1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley1.SetIsTote(false);
			packingHelper.CreatePackageDivot(pkg_OnTrolley1, kitPickLine1);

			var pkg_NotOnTrolley = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			pkg_NotOnTrolley.SetIsTote(false);
			packingHelper.CreatePackageDivot(pkg_NotOnTrolley, kitPickLine2);

			var trolley = helper.CreateEquipment("T001", 1, Weight.Kilograms, 1, Volume.Litre);
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			helper.Factory.Save();

			// Getting trolley for picking
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response1, webService1);
			var trolleyJobInfo = response1.Job;
			AssertNotNull(trolleyJobInfo);

			wheelOrderLine = webService1.Factory.Load<WhsOrderLine>(wheelOrderLine.PK);
			frameOrderLine = webService1.Factory.Load<WhsOrderLine>(frameOrderLine.PK);
			var wheelPickLineInSlot1 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 12m);
			var framePickLineInSlot1 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 6m);
			wheelPickLineInSlot1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLineInSlot1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			webService1.Factory.Save();

			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley1.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_NotOnTrolley.IsClosed);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			kitOrderLine1 = factory2.Load<WhsOrderLine>(kitOrderLine1.PK);
			var pickLines = kitOrderLine1.PickLines;
			kitPickLine1 = pickLines.Single(l => l.WZ_Units == 6m);
			kitPickLine2 = pickLines.Single(l => l.WZ_Units == 4m);
			var wheelPickLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines.Single(l => l.WZ_Units == 12m);
			var framePickLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines.Single(l => l.WZ_Units == 6m);
			var wheelTransferLine = wheelPickLine1.InventoryLine;
			var frameTransferLine = framePickLine1.InventoryLine;
			var kitTransferLine1 = kitPickLine1.InventoryLine;
			AssertLinePutaway(wheelTransferLine);
			AssertLinePutaway(frameTransferLine);
			AssertLinePutaway(kitTransferLine1);

			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);

			pkg_OnTrolley1 = factory2.Load<PkgPackage>(pkg_OnTrolley1.PK);
			pkg_NotOnTrolley = factory2.Load<PkgPackage>(pkg_NotOnTrolley.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Package closed info is correct.", true, pkg_OnTrolley1.IsClosed);
				AssertEquals("Only the Kit Pick Line was packed.", 1, pkg_OnTrolley1.PackedItemDivots.Count);
				AssertEquals("Only the Kit Pick Line was packed.", kitPickLine1.PK, pkg_OnTrolley1.PackedItemDivots[0].KI_ParentID);
				AssertEquals("Only the Kit Pick Line was packed.", kitPickLine1.WZ_Units, pkg_OnTrolley1.PackedItemDivots[0].KI_PackedQty);

				AssertEquals("Package is not closed.", false, pkg_NotOnTrolley.IsClosed);
				AssertEquals("Pre-packed package remains the same.", 1, pkg_NotOnTrolley.PackedItemDivots.Count);
				AssertEquals("Pre-packed package remains the same.", kitPickLine2.PK, pkg_NotOnTrolley.PackedItemDivots[0].KI_ParentID);
				AssertEquals("Pre-packed package remains the same.", kitPickLine2.WZ_Units, pkg_NotOnTrolley.PackedItemDivots[0].KI_PackedQty);
			});
		}

		public void TestPutawayStockInDockDoorOrPackingStation_TrolleyJob_PickByBOM_TotePicking()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);

			packingHelper.SetRefPackTypeUOM(PkgUnit.Carton, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(PkgUnit.Box, UOMPackTypesList.Codes.Case);

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			var frame = helper.CreateProduct(data.Org1, "FRAME");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			helper.Factory.Save();
			var kitOrder = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = helper.CreatePickNew(kitOrder);
			pick.WP_CartoniseSplitCases = false;
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine = frameOrderLine.PickLines.Single();

			wheelPickLine.WZ_F3_NKAllocatedPackType = PkgUnit.Tote;
			kitPickLine1.WZ_F3_NKAllocatedPackType = PkgUnit.Tote;

			var trolley = helper.CreateEquipment("T001", 1, Weight.Kilograms, 1, Volume.Litre);
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			helper.Factory.Save();

			var webService0 = GetNewWebService(data.Whs1);
			var response0 = webService0.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "TOTE1", 1, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response0, webService0);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors.", ErrorTypes.None, response0.Error);
				AssertEquals("Should have no error message.", null, response0.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory();
			trolleyJob = newFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK);
			AssertEquals(1, trolleyJob.Slots.Count);

			var response = webService0.ChangeTrolleyJobStatus(trolleyJob.PK.ToGuid(), TrolleyJobStatus.Picking);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", null, response.ErrorMessage);
			});

			kitOrder = newFactory.Load<WhsOrder>(kitOrder.PK);
			var packages = kitOrder.PackageJob.Packages;
			AssertEquals(1, packages.Count);
			var tote = packages.Single();

			// Getting trolley for picking
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Tote);
			AssertSuccessfulResponse(response1, webService1);
			var trolleyJobInfo = response1.Job;
			AssertNotNull(trolleyJobInfo);

			wheelOrderLine = webService1.Factory.Load<WhsOrderLine>(wheelOrderLine.PK);
			frameOrderLine = webService1.Factory.Load<WhsOrderLine>(frameOrderLine.PK);
			var wheelPickLineInSlot1 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 20m);
			var framePickLineInSlot1 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 10m);
			wheelPickLineInSlot1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLineInSlot1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			webService1.Factory.Save();

			tote = webService1.Factory.Load<PkgPackage>(tote.PK);
			AssertEquals("Precondition: Tote is not closed.", false, tote.IsClosed);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			kitOrderLine1 = factory2.Load<WhsOrderLine>(kitOrderLine1.PK);
			var pickLines = kitOrderLine1.PickLines;
			kitPickLine1 = pickLines.Single();
			AssertEquals(10m, kitPickLine1.WZ_Units);
			var wheelPickLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines.Single(l => l.WZ_Units == 20m);
			var framePickLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines.Single(l => l.WZ_Units == 10m);
			var wheelTransferLine = wheelPickLine1.InventoryLine;
			var frameTransferLine = framePickLine1.InventoryLine;
			var kitTransferLine1 = kitPickLine1.InventoryLine;
			AssertLinePutaway(wheelTransferLine);
			AssertLinePutaway(frameTransferLine);
			AssertLinePutaway(kitTransferLine1);

			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);

			tote = factory2.Load<PkgPackage>(tote.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Package closed info is correct.", false, tote.IsClosed);
				AssertEquals("Only the Kit Pick Line was packed.", 1, tote.PackedItemDivots.Count);
				AssertEquals("Only the Kit Pick Line was packed.", kitPickLine1.PK, tote.PackedItemDivots[0].KI_ParentID);
				AssertEquals("Only the Kit Pick Line was packed.", kitPickLine1.WZ_Units, tote.PackedItemDivots[0].KI_PackedQty);
			});
		}

		public void TestPutawayStockInDockDoorOrPackingStation_TrolleyJob_PickByBOM_TotePicking_AfterToteIsFull()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);

			packingHelper.SetRefPackTypeUOM(PkgUnit.Carton, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(PkgUnit.Box, UOMPackTypesList.Codes.Case);

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			var frame = helper.CreateProduct(data.Org1, "FRAME");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			helper.Factory.Save();
			var kitOrder = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = helper.CreatePickNew(kitOrder);
			pick.WP_CartoniseSplitCases = false;
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var wheelOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine = frameOrderLine.PickLines.Single();

			wheelPickLine.WZ_F3_NKAllocatedPackType = PkgUnit.Tote;
			kitPickLine1.WZ_F3_NKAllocatedPackType = PkgUnit.Tote;

			var trolley = helper.CreateEquipment("T001", 1, Weight.Kilograms, 1, Volume.Litre);
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			helper.Factory.Save();

			var webService0 = GetNewWebService(data.Whs1);
			var response0 = webService0.AddSlotToTrolleyUsingToteID(trolleyJob.PK.ToGuid(), "TOTE1", 1, new SearchFilterCriteriaInfo());
			AssertSuccessfulResponse(response0, webService0);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors.", ErrorTypes.None, response0.Error);
				AssertEquals("Should have no error message.", null, response0.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory();
			trolleyJob = newFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK);
			AssertEquals(1, trolleyJob.Slots.Count);

			var response = webService0.ChangeTrolleyJobStatus(trolleyJob.PK.ToGuid(), TrolleyJobStatus.Picking);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no errors.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error message.", null, response.ErrorMessage);
			});

			kitOrder = newFactory.Load<WhsOrder>(kitOrder.PK);
			var packages = kitOrder.PackageJob.Packages;
			AssertEquals(1, packages.Count);
			var tote = packages.Single();

			// Getting trolley for picking
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Tote);
			AssertSuccessfulResponse(response1, webService1);
			var trolleyJobInfo = response1.Job;
			AssertNotNull(trolleyJobInfo);

			wheelOrderLine = webService1.Factory.Load<WhsOrderLine>(wheelOrderLine.PK);
			frameOrderLine = webService1.Factory.Load<WhsOrderLine>(frameOrderLine.PK);
			var wheelPickLine1InSlot1 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 20m);
			var wheelPickLine2InSlot1 = wheelPickLine1InSlot1.Split(14m);
			var framePickLine1InSlot1 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 10m);
			var framePickLine2InSlot1 = framePickLine1InSlot1.Split(7m);
			wheelPickLine1InSlot1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine1InSlot1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			webService1.Factory.Save();

			tote = webService1.Factory.Load<PkgPackage>(tote.PK);
			AssertEquals("Precondition: Tote is not closed.", false, tote.IsClosed);

			// call ToteOnTrolleyIsFull to split kit pick lines, and we want to assert that the packed line get picked by putaway later
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response2 = webService2.ToteOnTrolleyIsFull(trolleyJob.PK.ToGuid(), tote.KP_PackageID);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			kitOrderLine1 = factory2.Load<WhsOrderLine>(kitOrderLine1.PK);
			var pickLines = kitOrderLine1.PickLines;
			AssertEquals("Kit Pick Line get split through ToteOnTrolleyIsFull.", 2, pickLines.Count);
			kitPickLine1 = pickLines.Single(pl => pl.WZ_Units == 3m);
			var kitPickLine2 = pickLines.Single(pl => pl.WZ_Units == 7m);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService3.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response3 = webService3.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			kitOrderLine1 = factory2.Load<WhsOrderLine>(kitOrderLine1.PK);
			pickLines = kitOrderLine1.PickLines;
			kitPickLine1 = pickLines.Single(pl => pl.WZ_Units == 3m);
			kitPickLine2 = pickLines.Single(pl => pl.WZ_Units == 7m);
			var kitTransferLine1 = kitPickLine1.InventoryLine;
			AssertLinePutaway(kitTransferLine1);

			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);

			tote = factory2.Load<PkgPackage>(tote.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Package closed info is correct.", false, tote.IsClosed);
				AssertEquals("Only the Kit Pick Line was packed.", 1, tote.PackedItemDivots.Count);
				AssertEquals("Only the Kit Pick Line was packed.", kitPickLine1.PK, tote.PackedItemDivots[0].KI_ParentID);
				AssertEquals("Only the Kit Pick Line was packed.", kitPickLine1.WZ_Units, tote.PackedItemDivots[0].KI_PackedQty);
				AssertEquals(true, kitPickLine1.IsPickedFromPutawayLocation);
				AssertEquals(false, kitPickLine2.IsPickedFromPutawayLocation);
			});
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_TrolleyJob_RCAs

		public void TestPutawayStockInDockDoorOrPackingStation_TrolleyJob_RCAs()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = helper.CreatePickNew(order);
			webService.Factory.Save();

			pick.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			order.Lines[0].ReleaseLines.Cast<WhsReleaseLine>().Single().PartAttribute1 = "RED";
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var pickLine = order.Lines[0].PickLines[0];
			var pkgDivot = packingHelper.CreatePackageDivot(pkg, pickLine);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			var trolleySlot1 = helper.CreateWhsPickTrolleySlot(trolleyJob, pkg, 1);
			webService.Factory.Save();
			AssertType<WhsPickLine>("Precondition.", pkg.PackedItemDivots[0].PackedItem);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTrolleyTransferLine_InFactory2 = factory2.Load<WhsTransferLine>(pick.Transfers.Single().Lines.Single().PK);
			AssertLinePutaway(inTransitTrolleyTransferLine_InFactory2);
			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_Pick_FiltersOtherJobs_CartonisedOrPickByLabel

		public void TestPutawayStockInDockDoorOrPackingStation_Pick_FiltersOtherJobs_CartonisedOrPickByLabel()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType_SplitCase = packingHelper.CreateRefPackType("1", "1", 1m, 2m, 3m, Length.Metres, 3, Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			var refType_Case = packingHelper.CreateRefPackType("2", "2", 1m, 4m, 9m, Length.Metres, 3, Weight.Kilograms, UOMPackTypesList.Codes.Case);
			var refType_Pallet = packingHelper.CreateRefPackType("3", "3", 1m, 1m, 3m, Length.Metres, 3, Weight.Kilograms, UOMPackTypesList.Codes.Pallet);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			data.Part1.OP_StockKeepingUnit = refType_SplitCase.F3_Code;
			helper.CreateProductUnit(data.Part1, refType_Pallet.F3_Code, 100m);
			helper.CreateProductUnit(data.Part1, refType_Case.F3_Code, 10m);
			webService.Factory.Save();

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 111m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 111m);
			helper.CreateWhsOrderLine(order, data.Part2, 1m);
			var pick = helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickPalletsByLabel = true;
			pick.WP_PickCasesByLabel = true;

			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory2 = factory2.Load<WhsPick>(pick.PK);
			var transferInFactory2 = pickInFactory2.Transfers.Single();

			var part1TransferLines = transferInFactory2.Lines.Where(l => l.WE_OP == data.Part1.PK);
			var part2TransferLine = transferInFactory2.Lines.Single(l => l.WE_OP == data.Part2.PK);

			AssertLinePutaway(part2TransferLine);
			foreach (var transferLine in part1TransferLines)
			{
				AssertLineNotPutaway(transferLine);
			}
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_Pick_FiltersOtherJobs_TotePicking

		public void TestPutawayStockInDockDoorOrPackingStation_Pick_FiltersOtherJobs_TotePicking()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var pick = helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: Should be two pick lines.", 2, pickLines.Length);
			var totePickLine = pickLines[0];
			var nonTotePickLine = pickLines[1];

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "AAA", 1, PkgUnit.Tote);
			package.SetIsTote(true);
			packingHelper.CreatePackageDivot(package, totePickLine);
			webService.Factory.Save();

			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var totePickLineInFactory2 = factory2.Load<WhsPickLine>(totePickLine.PK);
			var nonTotePickLineInFactory2 = factory2.Load<WhsPickLine>(nonTotePickLine.PK);

			AssertLinePutaway(nonTotePickLineInFactory2.InventoryLine);
			AssertLineNotPutaway(totePickLineInFactory2.InventoryLine);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_ConcurrencyExceptionThrown

		public void TestPutawayStockInDockDoorOrPackingStation_ConcurrencyExceptionThrown()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			factory.Save();

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			factory.Saving += f =>
			{
				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var transferLineInOtherFactory = otherFactory.Load<WhsTransferLine>(inTransitTransferLine.PK);
				transferLineInOtherFactory.WE_CustomAttrib1 = "US1"; // Change cause concurrency exception
				otherFactory.Save();
			};

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should not be able to save.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should not be able to save.", "While you have been working with this job another user has made changes. Please restart the operation and try again.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);
		}

		#endregion

		#region PickByLabel

		#region TestPutawayStockInDockDoorOrPackingStation_PickByLabel

		[TestDate(2018, 09, 11, 12, 10, 20)]
		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (int i = 0; i < 10; i++) // For 10 pick lines later on
			{
				helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 10 pick lines.", 10, pick.GetAllPickLines().Count());
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var otherPackage = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			packingHelper.CreatePackageDivot(package, pickLines[1]);
			packingHelper.CreatePackageDivot(otherPackage, pickLines[4]);
			webService.Factory.Save();

			foreach (var pickLine in pickLines)
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			webService.Factory.Save();

			AssertEquals("Precondition.", 10, pick.Transfers.Single().Lines.Count);
			pickLines[3].InventoryLine.WE_GS_NKPutawayBy = "";
			webService.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			webService.Factory.Save();

			var labelJobInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("Before put packages in dock door location FinalisedDate should be empty.", ZDateTimeOffset.Empty, labelJobInNewFactory.WTK_FinalisedDate);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLine1_InFactory2 = factory2.Load<WhsTransferLine>(pickLines[0].WZ_WE_InventoryLine);
			var inTransitTransferLine2_InFactory2 = factory2.Load<WhsTransferLine>(pickLines[1].WZ_WE_InventoryLine);
			AssertLinePutaway(inTransitTransferLine1_InFactory2);
			AssertLinePutaway(inTransitTransferLine2_InFactory2);

			foreach (var transferLine in factory2.Load<WhsPick>(pick.PK).Transfers.Single().Lines.Except(new[] { inTransitTransferLine1_InFactory2, inTransitTransferLine2_InFactory2 }))
			{
				AssertLineNotPutaway(transferLine);
			}

			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);

			var jobInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("After putting packages in dock door location, FinalisedDate should be current date time.", new ZDateTimeOffset(2018, 09, 11, 12, 10, 20, TimeSpan.Zero), jobInNewFactory.WTK_FinalisedDate);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, jobInNewFactory.WTK_WL_PutawayLocation);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PickByLabel_PartiallyPicked

		[TestDate(2018, 09, 11, 12, 10, 20)]
		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_PartiallyPicked()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);

			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().ElementAt(0);
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

			var pickLine2 = pick.GetAllPickLines().ElementAt(1);
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			webService.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			webService.Factory.Save();

			var jobInNewFactory1 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("FinalisedDate should be empty.", ZDateTimeOffset.Empty, jobInNewFactory1.WTK_FinalisedDate);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			webService.Factory.Save();
			AssertEquals("Should have a warning.", ErrorTypes.WarningOnly, response.Error);
			AssertEquals("Should have a warning.", "No stock found to Put. Either nothing is In-Transit for the current job and staff, or Put has already been completed.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);
			AssertLineNotPutaway(pickLine1.InventoryLine); // Should *not* be putaway

			var jobInNewFactory2 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("FinalisedDate should still be empty.", ZDateTimeOffset.Empty, jobInNewFactory2.WTK_FinalisedDate);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PickByLabel_PartiallyPicked_SplitsJob

		[TestDate(2018, 09, 11, 12, 10, 20)]
		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_PartiallyPicked_SplitsJob()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);

			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 5m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15m);
			var pick = helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().ElementAt(0);
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

			var pickLine2 = pick.GetAllPickLines().ElementAt(1);
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;

			var pickLine3 = pick.GetAllPickLines().ElementAt(2);
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			packingHelper.CreatePackageDivot(package2, pickLine3);
			webService.Factory.Save();

			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			webService.Factory.Save();

			var jobInNewFactory1 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("FinalisedDate should be empty.", ZDateTimeOffset.Empty, jobInNewFactory1.WTK_FinalisedDate);
			AssertEquals("Should have three labels.", 2, jobInNewFactory1.Labels.Count);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			webService.Factory.Save();
			AssertEquals("Should have no error.", ErrorTypes.None, response.Error);
			AssertNull("Should have no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);
			AssertLinePutaway(pickLine1.InventoryLine); // For package 1, should be putaway
			AssertLineNotPutaway(pickLine2.InventoryLine); // For package 2, should *not* be putaway

			var jobInNewFactory2 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("FinalisedDate should be set.", new ZDateTimeOffset(2018, 09, 11, 12, 10, 20, TimeSpan.Zero), jobInNewFactory2.WTK_FinalisedDate);
			AssertEquals("Should have only a single label.", 1, jobInNewFactory2.Labels.Count);
			AssertEquals("Single label should be for picked PKG1.", package1.PK, jobInNewFactory2.Labels[0].WTL_KP_Package);

			var newJob = WhsPickByLabelHelper.GetWhsPickByLabelJob(webService.Factory, data.Whs1.PK, GlbStaff.CurrentUser.GS_Code);
			AssertNotNull("Should have created a new job.", newJob);
			AssertEquals("FinalisedDate should be empty.", ZDateTimeOffset.Empty, newJob.WTK_FinalisedDate);
			AssertEquals("Should have a single label.", 1, newJob.Labels.Count);
			AssertEquals("Single label should be for unpicked PKG2.", package2.PK, newJob.Labels[0].WTL_KP_Package);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PickByLabel_UnpickedPackages_SplitsJob

		[TestDate(2018, 09, 11, 12, 10, 20)]
		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_UnpickedPackages_SplitsJob()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);

			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().ElementAt(0);
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

			var pickLine2 = pick.GetAllPickLines().ElementAt(1);
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			webService.Factory.Save();

			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			webService.Factory.Save();

			var jobInNewFactory1 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("FinalisedDate should be empty.", ZDateTimeOffset.Empty, jobInNewFactory1.WTK_FinalisedDate);
			AssertEquals("Should have two labels.", 2, jobInNewFactory1.Labels.Count);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			webService.Factory.Save();
			AssertEquals("Should have no error.", ErrorTypes.None, response.Error);
			AssertNull("Should have no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);
			AssertLinePutaway(pickLine1.InventoryLine); // For package 1, should be putaway

			var jobInNewFactory2 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("FinalisedDate should be set.", new ZDateTimeOffset(2018, 09, 11, 12, 10, 20, TimeSpan.Zero), jobInNewFactory2.WTK_FinalisedDate);
			AssertEquals("Should have only a single label.", 1, jobInNewFactory2.Labels.Count);
			AssertEquals("Single label should be for picked PKG1.", package1.PK, jobInNewFactory2.Labels[0].WTL_KP_Package);

			var newJob = WhsPickByLabelHelper.GetWhsPickByLabelJob(webService.Factory, data.Whs1.PK, GlbStaff.CurrentUser.GS_Code);
			AssertNotNull("Should have created a new job.", newJob);
			AssertEquals("FinalisedDate should be empty.", ZDateTimeOffset.Empty, newJob.WTK_FinalisedDate);
			AssertEquals("Should have one label.", 1, newJob.Labels.Count);
			AssertEquals("Single label should be for unpicked PKG2.", package2.PK, newJob.Labels[0].WTL_KP_Package);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PickByLabel_UnpickedPackages_SplitsJob_MultiplePackages

		[TestDate(2018, 09, 11, 12, 10, 20)]
		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_UnpickedPackages_SplitsJob_MultiplePackages()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);

			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 5m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15m);
			var pick = helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().ElementAt(0);
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

			var pickLine2 = pick.GetAllPickLines().ElementAt(1);
			var pickLine3 = pick.GetAllPickLines().ElementAt(2);
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			var package3 = packingHelper.CreatePackage(pkgJob, "PKG3", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			packingHelper.CreatePackageDivot(package3, pickLine3);
			webService.Factory.Save();

			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package3.PK);
			webService.Factory.Save();

			var jobInNewFactory1 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("FinalisedDate should be empty.", ZDateTimeOffset.Empty, jobInNewFactory1.WTK_FinalisedDate);
			AssertEquals("Should have three labels.", 3, jobInNewFactory1.Labels.Count);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			webService.Factory.Save();
			AssertEquals("Should have no error.", ErrorTypes.None, response.Error);
			AssertNull("Should have no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);
			AssertLinePutaway(pickLine1.InventoryLine); // For package 1, should be putaway

			var jobInNewFactory2 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("FinalisedDate should be set.", new ZDateTimeOffset(2018, 09, 11, 12, 10, 20, TimeSpan.Zero), jobInNewFactory2.WTK_FinalisedDate);
			AssertEquals("Should have only a single label.", 1, jobInNewFactory2.Labels.Count);
			AssertEquals("Single label should be for picked PKG1.", package1.PK, jobInNewFactory2.Labels[0].WTL_KP_Package);

			var newJob = WhsPickByLabelHelper.GetWhsPickByLabelJob(webService.Factory, data.Whs1.PK, GlbStaff.CurrentUser.GS_Code);
			AssertNotNull("Should have created a new job.", newJob);
			AssertEquals("FinalisedDate should be empty.", ZDateTimeOffset.Empty, newJob.WTK_FinalisedDate);
			AssertEquals("Should have two labels.", 2, newJob.Labels.Count);
			AssertContainsExactElementsInAnyOrder("Labels should be for unpicked packages.", new[] { package2.PK, package3.PK }, newJob.Labels.Cast<WhsPickByLabelLabel>().Select(l => l.WTL_KP_Package));
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PickByLabel_RCAs

		[TestDate(2018, 09, 11, 12, 10, 20)]
		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_RCAs()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = helper.CreatePickNew(order);
			webService.Factory.Save();

			pick.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			order.Lines[0].ReleaseLines.Cast<WhsReleaseLine>().Single().PartAttribute1 = "RED";
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var pickLine = order.Lines[0].PickLines[0];
			var pkgDivot = packingHelper.CreatePackageDivot(pkg, pickLine);
			webService.Factory.Save();

			AssertType<WhsPickLine>("Precondition.", pkg.PackedItemDivots[0].PackedItem);

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, pkg.PK);
			webService.Factory.Save();
			AssertEquals("Before put packages in dock door location FinalisedDate should be empty.", ZDateTimeOffset.Empty, new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK).WTK_FinalisedDate);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTrolleyTransferLine_InFactory2 = factory2.Load<WhsTransferLine>(pick.Transfers.Single().Lines.Single().PK);
			AssertLinePutaway(inTransitTrolleyTransferLine_InFactory2);
			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);

			var jobInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("After putting packages in dock door location, FinalisedDate should be current date time.", new ZDateTimeOffset(2018, 09, 11, 12, 10, 20, TimeSpan.Zero), jobInNewFactory.WTK_FinalisedDate);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PickByLabel_RCAs_UnpickedPackages_SplitsJob

		[TestDate(2018, 09, 11, 12, 10, 20)]
		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_RCAs_UnpickedPackages_SplitsJob()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 1m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			var pick = helper.CreatePickNew(order);
			var pickLine1 = order.Lines[0].PickLines[0];
			var pickLine2 = order.Lines[0].PickLines[1];
			var pickLine3 = order.Lines[0].PickLines[2];
			webService.Factory.Save();

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			order.Lines[0].ReleaseLines.Cast<WhsReleaseLine>().Single().PartAttribute1 = "RED";
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var pkg2 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine1);
			packingHelper.CreatePackageDivot(pkg2, pickLine2);
			packingHelper.CreatePackageDivot(pkg2, pickLine3);
			webService.Factory.Save();

			AssertType<WhsPickLine>("Precondition.", pkg1.PackedItemDivots[0].PackedItem);
			AssertType<WhsPickLine>("Precondition.", pkg2.PackedItemDivots[0].PackedItem);
			AssertType<WhsPickLine>("Precondition.", pkg2.PackedItemDivots[1].PackedItem);

			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, pkg1.PK);
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, pkg2.PK);
			webService.Factory.Save();
			AssertEquals("FinalisedDate should be empty.", ZDateTimeOffset.Empty, new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK).WTK_FinalisedDate);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			AssertEquals("Should have no error.", ErrorTypes.None, response.Error);
			AssertNull("Should have no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			AssertLinePutaway(pickLine1.InventoryLine); // For package 1, should be putaway
			AssertLineNotPutaway(pickLine2.InventoryLine); // For package 2, should *not* be putaway

			var jobInNewFactory2 = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("FinalisedDate should be set.", new ZDateTimeOffset(2018, 09, 11, 12, 10, 20, TimeSpan.Zero), jobInNewFactory2.WTK_FinalisedDate);
			AssertEquals("Should have only a single label.", 1, jobInNewFactory2.Labels.Count);
			AssertEquals("Single label should be for picked PKG1.", pkg1.PK, jobInNewFactory2.Labels[0].WTL_KP_Package);

			var newJob = WhsPickByLabelHelper.GetWhsPickByLabelJob(webService.Factory, data.Whs1.PK, GlbStaff.CurrentUser.GS_Code);
			AssertNotNull("Should have created a new job.", newJob);
			AssertEquals("FinalisedDate should be empty.", ZDateTimeOffset.Empty, newJob.WTK_FinalisedDate);
			AssertEquals("Should have a single label.", 1, newJob.Labels.Count);
			AssertEquals("Single label should be for unpicked PKG2.", pkg2.PK, newJob.Labels[0].WTL_KP_Package);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PickByLabel_MultiPick

		[TestDate(2018, 09, 11, 12, 10, 20)]
		public void TestPickByLabel_PutawayStockInDockDoorOrPackingStation_MultiPick()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);

			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick1 = helper.CreatePickNew(order1);
			var pickLine1 = pick1.GetAllPickLines().Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);
			webService.Factory.Save();

			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick2 = helper.CreatePickNew(order2);
			var pickLine2 = pick2.GetAllPickLines().Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			webService.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			webService.Factory.Save();

			AssertEquals("Before packages are placed in dock door location, FinalisedDate should be empty.", ZDateTimeOffset.Empty, new BusinessObjectFactory().Load<WhsPickByLabelJob>(pickByLabelJob.PK).WTK_FinalisedDate);

			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			webService.Factory.Save();
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			foreach (var transferLine in pick1.Transfers.Union(pick2.Transfers).SelectMany(t => t.Lines))
			{
				AssertLinePutaway(transferLine);
			}

			var jobInNewFactory = new BusinessObjectFactory().Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("After packages are placed in dock door location, FinalisedDate should be current date time.", new ZDateTimeOffset(2018, 09, 11, 12, 10, 20, TimeSpan.Zero), jobInNewFactory.WTK_FinalisedDate);
		}

		#endregion

		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobs()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickByLabelJob1 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package1.PK);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);

			var pickByLabelJob2 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "BBB", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package2.PK);
			Helper.Factory.Save();

			AssertEquals(ZGuid.Empty, pickByLabelJob1.WTK_WL_PutawayLocation);
			AssertEquals(ZGuid.Empty, pickByLabelJob2.WTK_WL_PutawayLocation);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob1.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertExpectedLocationDetails(response, location: null);

			var newFactory = new BusinessObjectFactory();
			var pickByLabelJob1InNewFactory = newFactory.Load<WhsPickByLabelJob>(pickByLabelJob1.PK);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, pickByLabelJob1InNewFactory.WTK_WL_PutawayLocation);

			var pickByLabelJob2InNewFactory = newFactory.Load<WhsPickByLabelJob>(pickByLabelJob2.PK);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, pickByLabelJob2InNewFactory.WTK_WL_PutawayLocation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobs_LinkedByCommonOrder()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part2, 15m);
			Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(order2, data.Part2, 15m);
			Helper.CreatePickNew(order2);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine5 = Helper.CreateWhsOrderLine(order3, data.Part1, 10m);
			Helper.CreatePickNew(order3);

			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine3 = orderLine3.PickLines.Single();
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine4 = orderLine4.PickLines.Single();
			pickLine4.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine5 = orderLine5.PickLines.Single();
			pickLine5.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickByLabelJob1 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package1.PK);

			var package2 = packingHelper.CreatePackage(pkgJob1, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package3 = packingHelper.CreatePackage(pkgJob2, "PKG3", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package3, pickLine3);

			var pickByLabelJob2 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "BBB", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package2.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package3.PK);

			var package4 = packingHelper.CreatePackage(pkgJob2, "PKG4", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package4, pickLine4);

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var package5 = packingHelper.CreatePackage(pkgJob3, "PKG5", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package5, pickLine5);

			var pickByLabelJob3 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "CCC", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "CCC", package4.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "CCC", package5.PK);
			Helper.Factory.Save();

			AssertEquals(ZGuid.Empty, pickByLabelJob1.WTK_WL_PutawayLocation);
			AssertEquals(ZGuid.Empty, pickByLabelJob2.WTK_WL_PutawayLocation);
			AssertEquals(ZGuid.Empty, pickByLabelJob3.WTK_WL_PutawayLocation);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob1.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertExpectedLocationDetails(response, location: null);

			var newFactory = new BusinessObjectFactory();
			var pickByLabelJob1InNewFactory = newFactory.Load<WhsPickByLabelJob>(pickByLabelJob1.PK);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, pickByLabelJob1InNewFactory.WTK_WL_PutawayLocation);

			var pickByLabelJob2InNewFactory = newFactory.Load<WhsPickByLabelJob>(pickByLabelJob2.PK);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, pickByLabelJob2InNewFactory.WTK_WL_PutawayLocation);

			var pickByLabelJob3InNewFactory = newFactory.Load<WhsPickByLabelJob>(pickByLabelJob3.PK);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, pickByLabelJob3InNewFactory.WTK_WL_PutawayLocation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobs_LinkedByCommonOrder_MultipleLevels()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var orders = new List<WhsOrder>();
			for (var i = 0; i < 10; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O{i}");
				Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				Helper.CreateWhsOrderLine(order, data.Part2, 1m);
				Helper.CreatePickNew(order);

				orders.Add(order);
			}

			foreach (var order in orders)
			{
				var pickLine1 = order.Lines[0].PickLines.Single();
				pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

				var pickLine2 = order.Lines[1].PickLines.Single();
				pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}
			Helper.Factory.Save();

			var pickByLabelJobs = new List<WhsPickByLabelJob>();
			PkgPackageJob prevPkgJob = null;
			for (var i = 0; i <= 10; i++)
			{
				var user = $"A{i}";
				var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, user, data.Whs1.WW_DefaultOutboundDockDoor);

				if (i > 0)
				{
					var prevOrder = orders[i - 1];
					var package1 = packingHelper.CreatePackage(prevPkgJob, $"PKG{i}2", 1, PkgUnit.Box);
					packingHelper.CreatePackageDivot(package1, prevOrder.Lines[1].PickLines.Single());
					WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, user, package1.PK);
				}

				if (i < 10)
				{
					var currentOrder = orders[i];
					var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(currentOrder);
					var package2 = packingHelper.CreatePackage(pkgJob2, $"PKG{i}1", 1, PkgUnit.Box);
					packingHelper.CreatePackageDivot(package2, currentOrder.Lines[0].PickLines.Single());
					WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, user, package2.PK);

					prevPkgJob = pkgJob2;
				}

				pickByLabelJobs.Add(pickByLabelJob);
			}
			Helper.Factory.Save();

			foreach (var pickByLabelJob in pickByLabelJobs)
			{
				AssertEquals(ZGuid.Empty, pickByLabelJob.WTK_WL_PutawayLocation);
			}

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJobs[0].PK.ToGuid(), PickJobType.PickByLabelJob, packingStationLocation.WLV_LocationString);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertExpectedLocationDetails(response, location: null);

			var newFactory = new BusinessObjectFactory();
			var pickByLabelJobsInNewFactory = newFactory.Load<WhsPickByLabelJob>(new ZQuery());
			AssertEquals(11, pickByLabelJobsInNewFactory.Length);

			foreach (var pickByLabelJob in pickByLabelJobsInNewFactory)
			{
				AssertEquals(packingStationLocation.PK, pickByLabelJob.WTK_WL_PutawayLocation);
			}

			AssertEquals("Should reported over 10 depth search", "The depth of search all sibling pick by label jobs related to the job is more than 10.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobs_ConcurrencyError()
			=> TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobs_ConcurrencyErrorCore(isSiblingConcurrencyIssue: false);

		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobs_ConcurrencyErrorWithSiblingJob()
			=> TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobs_ConcurrencyErrorCore(isSiblingConcurrencyIssue: true);

		void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobs_ConcurrencyErrorCore(bool isSiblingConcurrencyIssue)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickByLabelJob1 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package1.PK);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);

			var pickByLabelJob2 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "BBB", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package2.PK);
			Helper.Factory.Save();

			AssertEquals(ZGuid.Empty, pickByLabelJob1.WTK_WL_PutawayLocation);
			AssertEquals(ZGuid.Empty, pickByLabelJob2.WTK_WL_PutawayLocation);

			var webService = GetNewWebService(data.Whs1);
			webService.Factory.Saving += delegate
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var pickByLabelJobPkWithConcurrencyIssue = isSiblingConcurrencyIssue ? pickByLabelJob2.PK : pickByLabelJob1.PK;
				var pickByLabelJobInNewFactory = newFactory.Load<WhsPickByLabelJob>(pickByLabelJobPkWithConcurrencyIssue);
				pickByLabelJobInNewFactory.WTK_WL_PutawayLocation = packingStationLocation.PK;
				newFactory.Save();
			};

			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob1.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should have error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should have error.", "While you have been working with this job another user has made changes. Please restart the operation and try again.", response.ErrorMessage);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithPutawayLocation()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLine);

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			pickByLabelJob.WTK_WL_PutawayLocation = packingStationLocation.PK;
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package.PK);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should have error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should have error.", "Location DOCKDOOR is not valid for this job as some inventory has already been putaway to PST.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(packingStationLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobWithPutawayLocation()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickByLabelJob1 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package1.PK);
			pickByLabelJob1.WTK_WL_PutawayLocation = packingStationLocation.PK;
			Helper.Factory.Save();

			var putawayPickByLabel1WebService = GetNewWebService(data.Whs1);
			var response1 = putawayPickByLabel1WebService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob1.PK.ToGuid(), PickJobType.PickByLabelJob, packingStationLocation.WLV_LocationString);
			AssertSuccessfulResponseWithNoErrors(response1, putawayPickByLabel1WebService);
			AssertExpectedLocationDetails(response1, location: null);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);

			var pickByLabelJob2 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "BBB", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package2.PK);
			Helper.Factory.Save();

			AssertEquals(packingStationLocation.PK, pickByLabelJob1.WTK_WL_PutawayLocation);
			AssertEquals(ZGuid.Empty, pickByLabelJob2.WTK_WL_PutawayLocation);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob2.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should have error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should have error.", "Location DOCKDOOR is not valid for this job as some inventory has already been putaway to PST.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(packingStationLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobsWithPutawayLocation_LinkedByCommonOrder()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part2, 15m);
			Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(order2, data.Part2, 15m);
			Helper.CreatePickNew(order2);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine5 = Helper.CreateWhsOrderLine(order3, data.Part1, 10m);
			Helper.CreatePickNew(order3);

			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine3 = orderLine3.PickLines.Single();
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine4 = orderLine4.PickLines.Single();
			pickLine4.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine5 = orderLine5.PickLines.Single();
			pickLine5.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickByLabelJob1 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package1.PK);
			pickByLabelJob1.WTK_WL_PutawayLocation = packingStationLocation.PK;
			Helper.Factory.Save();

			var putawayPickByLabel1WebService = GetNewWebService(data.Whs1);
			var response1 = putawayPickByLabel1WebService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob1.PK.ToGuid(), PickJobType.PickByLabelJob, packingStationLocation.WLV_LocationString);
			AssertSuccessfulResponseWithNoErrors(response1, putawayPickByLabel1WebService);
			AssertExpectedLocationDetails(response1, location: null);

			var package2 = packingHelper.CreatePackage(pkgJob1, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package3 = packingHelper.CreatePackage(pkgJob2, "PKG3", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package3, pickLine3);

			var pickByLabelJob2 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "BBB", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package2.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package3.PK);

			var package4 = packingHelper.CreatePackage(pkgJob2, "PKG4", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package4, pickLine4);

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var package5 = packingHelper.CreatePackage(pkgJob3, "PKG5", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package5, pickLine5);

			var pickByLabelJob3 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "CCC", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "CCC", package4.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "CCC", package5.PK);
			Helper.Factory.Save();

			AssertEquals(packingStationLocation.PK, pickByLabelJob1.WTK_WL_PutawayLocation);
			AssertEquals(ZGuid.Empty, pickByLabelJob2.WTK_WL_PutawayLocation);
			AssertEquals(ZGuid.Empty, pickByLabelJob3.WTK_WL_PutawayLocation);

			var webService = GetNewWebService(data.Whs1);
			var response2 = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob3.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should have error.", ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Should have error.", "Location DOCKDOOR is not valid for this job as some inventory has already been putaway to PST.", response2.ErrorMessage);
			AssertExpectedLocationDetails(response2, location: GetWhsLocationInfo(packingStationLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobsWithPutawayLocation_LinkedByCommonOrder_MultipleLevels()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var orders = new List<WhsOrder>();
			for (var i = 0; i < 5; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O{i}");
				Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				Helper.CreateWhsOrderLine(order, data.Part2, 1m);
				Helper.CreatePickNew(order);

				orders.Add(order);
			}

			foreach (var order in orders)
			{
				var pickLine1 = order.Lines[0].PickLines.Single();
				pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

				var pickLine2 = order.Lines[1].PickLines.Single();
				pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}
			Helper.Factory.Save();

			var pickByLabelJobToPutaway = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(orders[0]);
			var package = packingHelper.CreatePackage(pkgJob, $"PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, orders[0].Lines[0].PickLines.Single());
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package.PK);
			pickByLabelJobToPutaway.WTK_WL_PutawayLocation = packingStationLocation.PK;
			Helper.Factory.Save();

			var putawayPickByLabel1WebService = GetNewWebService(data.Whs1);
			var response1 = putawayPickByLabel1WebService.PutawayStockInDockDoorOrPackingStation(pickByLabelJobToPutaway.PK.ToGuid(), PickJobType.PickByLabelJob, packingStationLocation.WLV_LocationString);
			AssertSuccessfulResponseWithNoErrors(response1, putawayPickByLabel1WebService);
			AssertExpectedLocationDetails(response1, location: null);

			var pickByLabelJobs = new List<WhsPickByLabelJob>();
			var prevPkgJob = pkgJob;
			for (var i = 1; i <= 5; i++)
			{
				var user = $"A{i}";
				var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, user, data.Whs1.WW_DefaultOutboundDockDoor);

				if (i > 0)
				{
					var prevOrder = orders[i - 1];
					var package1 = packingHelper.CreatePackage(prevPkgJob, $"PKG{i}2", 1, PkgUnit.Box);
					packingHelper.CreatePackageDivot(package1, prevOrder.Lines[1].PickLines.Single());
					WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, user, package1.PK);
				}

				if (i < 5)
				{
					var currentOrder = orders[i];
					var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(currentOrder);
					var package2 = packingHelper.CreatePackage(pkgJob2, $"PKG{i}1", 1, PkgUnit.Box);
					packingHelper.CreatePackageDivot(package2, currentOrder.Lines[0].PickLines.Single());
					WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, user, package2.PK);

					prevPkgJob = pkgJob2;
				}

				pickByLabelJobs.Add(pickByLabelJob);
			}
			Helper.Factory.Save();

			foreach (var pickByLabelJob in pickByLabelJobs)
			{
				AssertEquals(ZGuid.Empty, pickByLabelJob.WTK_WL_PutawayLocation);
			}

			var webService = GetNewWebService(data.Whs1);
			var response2 = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJobs[4].PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertEquals("Should have error.", ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Should have error.", "Location DOCKDOOR is not valid for this job as some inventory has already been putaway to PST.", response2.ErrorMessage);
			AssertExpectedLocationDetails(response2, location: GetWhsLocationInfo(packingStationLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithOtherPickByLabelJobs()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 15m);

			Helper.CreatePickNew(order1, order2);
			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickByLabelJob1 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package1.PK);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);

			var pickByLabelJob2 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "BBB", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package2.PK);
			Helper.Factory.Save();

			AssertEquals(ZGuid.Empty, pickByLabelJob1.WTK_WL_PutawayLocation);
			AssertEquals(ZGuid.Empty, pickByLabelJob2.WTK_WL_PutawayLocation);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob1.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertExpectedLocationDetails(response, location: null);

			var newFactory = new BusinessObjectFactory();
			var pickByLabelJob1InNewFactory = newFactory.Load<WhsPickByLabelJob>(pickByLabelJob1.PK);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, pickByLabelJob1InNewFactory.WTK_WL_PutawayLocation);

			var pickByLabelJob2InNewFactory = newFactory.Load<WhsPickByLabelJob>(pickByLabelJob2.PK);
			AssertEquals("Not a sibling pick by label job", ZGuid.Empty, pickByLabelJob2InNewFactory.WTK_WL_PutawayLocation);
		}

		[TestDate(2024, 07, 31)]
		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabel_WithSiblingPickByLabelJobs_DBHits()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			for (var i = 0; i < 10; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, $"R{i}");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
			}
			Helper.Factory.Save();

			var orders = new List<WhsOrder>();
			for (var i = 0; i < 10; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O{i}");
				Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				Helper.CreateWhsOrderLine(order, data.Part2, 1m);
				Helper.CreatePickNew(order);

				orders.Add(order);
			}

			foreach (var order in orders)
			{
				var pickLine1 = order.Lines[0].PickLines.Single();
				pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;

				var pickLine2 = order.Lines[1].PickLines.Single();
				pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}
			Helper.Factory.Save();

			var pickByLabelJobs = new List<WhsPickByLabelJob>();
			PkgPackageJob prevPkgJob = null;
			for (var i = 0; i <= 10; i++)
			{
				var user = $"A{i}";
				var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, user, data.Whs1.WW_DefaultOutboundDockDoor);

				if (i > 0)
				{
					var prevOrder = orders[i - 1];
					var package1 = packingHelper.CreatePackage(prevPkgJob, $"PKG{i}2", 1, PkgUnit.Box);
					packingHelper.CreatePackageDivot(package1, prevOrder.Lines[1].PickLines.Single());
					WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, user, package1.PK);
				}

				if (i < 10)
				{
					var currentOrder = orders[i];
					var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(currentOrder);
					var package2 = packingHelper.CreatePackage(pkgJob2, $"PKG{i}1", 1, PkgUnit.Box);
					packingHelper.CreatePackageDivot(package2, currentOrder.Lines[0].PickLines.Single());
					WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, user, package2.PK);

					prevPkgJob = pkgJob2;
				}

				pickByLabelJobs.Add(pickByLabelJob);
			}
			Helper.Factory.Save();

			foreach (var pickByLabelJob in pickByLabelJobs)
			{
				AssertEquals(ZGuid.Empty, pickByLabelJob.WTK_WL_PutawayLocation);
			}

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 4 },
				{ WhsDocketLineSchema.Constants.TableName, 5 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickByLabelJobSchema.Constants.TableName, 12 },
				{ WhsPickByLabelLabelSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 3 },
				{ WhsPickTrolleyJobSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var webService = GetNewWebService(data.Whs1);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJobs[0].PK.ToGuid(), PickJobType.PickByLabelJob, packingStationLocation.WLV_LocationString);
				AssertSuccessfulResponseWithNoErrors(response, webService);
				AssertExpectedLocationDetails(response, location: null);
			}

			var newFactory = new BusinessObjectFactory();
			var pickByLabelJobsInNewFactory = newFactory.Load<WhsPickByLabelJob>(new ZQuery());
			AssertEquals(11, pickByLabelJobsInNewFactory.Length);

			foreach (var pickByLabelJob in pickByLabelJobsInNewFactory)
			{
				AssertEquals(packingStationLocation.PK, pickByLabelJob.WTK_WL_PutawayLocation);
			}

			AssertEquals("Should reported over 10 depth search", "The depth of search all sibling pick by label jobs related to the job is more than 10.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region DBHits

		public void TestPutawayStockInDockDoorOrPackingStation_Pick_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var orderList = new List<WhsOrder>();
			for (var i = 1; i <= 10; i++)
			{
				var order = CreateOrder(data, i);
				orderList.Add(order);
			}

			var pick = Helper.CreatePickNew(orderList.ToArray());
			var pickLines = pick.GetAllPickLines();
			pickLines.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);
			Helper.Factory.Save();

			var inTransitTransferLines = pick.Transfers.Single().Lines;
			AssertEquals("Precondition: Should have 20 transfer lines", 20, inTransitTransferLines.Count);
			Assert("Precondition: Should be In-Transit.", inTransitTransferLines.All(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit));
			var packingHelper = new PackingTestHelper(Helper.Factory);
			orderList.ForEach(o => CreatePackageForOrder(o.WD_ExternalReference, o, packingHelper));
			Helper.Factory.Save();
			var locationString = data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString;

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ GenAddOnColumnSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 6 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			}
		}

		WhsOrder CreateOrder(TestDataSimpleEnvironment data, int index)
		{
			var part = Helper.CreateProduct("Part1-" + index, data.Org1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1-" + index, part, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2-" + index, part, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + index, part, 10m);
			Helper.Factory.Save();
			return order;
		}

		PkgPackage CreatePackageForOrder(string index, WhsOrder order, PackingTestHelper packingHelper)
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(packageJob, "PKG" + index, 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			return package;
		}

		public void TestPutawayStockInDockDoorOrPackingStation_TrolleyJob_DbHits()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 10; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 2m);

			var pick1 = Helper.CreatePickNew(order1, order2, order3);
			var pickLines1 = pick1.GetAllPickLines();
			pickLines1.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);

			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 2m);
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 2m);
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part1, 2m);

			var pick2 = Helper.CreatePickNew(order4, order5, order6);
			var pickLines2 = pick2.GetAllPickLines().ToArray();
			pickLines2.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);

			var order7 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O7", data.Part1, 2m);
			var order8 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O8", data.Part1, 2m);
			var order9 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O9", data.Part1, 2m);
			var order10 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "10", data.Part1, 2m);

			var pick3 = Helper.CreatePickNew(order7, order8, order9, order10);
			var pickLines3 = pick3.GetAllPickLines().ToArray();
			pickLines3.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);
			Helper.Factory.Save();

			var pkg1 = CreateTotePackageForOrder(packingHelper, order1, pickLines1);
			var pkg2 = CreateTotePackageForOrder(packingHelper, order2, pickLines1);
			var pkg3 = CreateTotePackageForOrder(packingHelper, order3, pickLines1);
			var pkg4 = CreateTotePackageForOrder(packingHelper, order4, pickLines2);
			var pkg5 = CreateTotePackageForOrder(packingHelper, order5, pickLines2);
			var pkg6 = CreateTotePackageForOrder(packingHelper, order6, pickLines2);
			var pkg7 = CreateTotePackageForOrder(packingHelper, order7, pickLines3);
			var pkg8 = CreateTotePackageForOrder(packingHelper, order8, pickLines3);
			var pkg9 = CreateTotePackageForOrder(packingHelper, order9, pickLines3);
			var pkg10 = CreateTotePackageForOrder(packingHelper, order10, pickLines3);

			Helper.Factory.Save();

			var trolley1 = Helper.CreateTrolley("T001");
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg1, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg3, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg5, 3);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg6, 4);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg7, 5);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg8, 6);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg9, 7);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg10, 8);

			var trolley2 = Helper.CreateTrolley("T002");
			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob2, pkg2, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob2, pkg4, 2);

			Helper.Factory.Save();

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ GenAddOnColumnSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 3 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 5 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 2 },
				{ WhsPickTrolleyJobSchema.Constants.TableName, 2 },
				{ WhsPickTrolleySlotSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob1.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
				Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Should be no error", ErrorTypes.None, response.Error);
			}
		}

		PkgPackage CreateTotePackageForOrder(PackingTestHelper packingHelper, WhsOrder order, IEnumerable<WhsPickLine> pickLines)
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(packageJob, "PKG" + order.WD_ExternalReference, 1, PkgUnit.Box);
			package.SetIsTote(true);
			packingHelper.CreatePackageDivot(package, pickLines.FirstOrDefault(x => x.WZ_WE_TransactionLine == order.Lines[0].PK));
			return package;
		}

		[TestDate(2024, 05, 09)]
		public void TestPutawayStockInDockDoorOrPackingStation_PickByLabelJob_DbHits()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 10; i++) // For 10 pick lines later on
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var package0 = CreatePackageForPickByLabelPicking(0, data, packingHelper);
			var package1 = CreatePackageForPickByLabelPicking(1, data, packingHelper);
			var package2 = CreatePackageForPickByLabelPicking(2, data, packingHelper);
			var package3 = CreatePackageForPickByLabelPicking(3, data, packingHelper);
			var package4 = CreatePackageForPickByLabelPicking(4, data, packingHelper);
			var package5 = CreatePackageForPickByLabelPicking(5, data, packingHelper);
			var package6 = CreatePackageForPickByLabelPicking(6, data, packingHelper);
			var package7 = CreatePackageForPickByLabelPicking(7, data, packingHelper);
			var package8 = CreatePackageForPickByLabelPicking(8, data, packingHelper);
			var package9 = CreatePackageForPickByLabelPicking(9, data, packingHelper);

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package0.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package3.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package4.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package5.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package6.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package7.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package8.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package9.PK);
			Helper.Factory.Save();

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 4 },
				{ WhsDocketLineSchema.Constants.TableName, 5 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickByLabelJobSchema.Constants.TableName, 2 },
				{ WhsPickByLabelLabelSchema.Constants.TableName, 1 },
				{ WhsPickTrolleyJobSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, packingLocation.WLV_LocationString);
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			}
		}

		PkgPackage CreatePackageForPickByLabelPicking(int index, TestDataSimpleEnvironment data, PackingTestHelper packingHelper)
		{
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + index, data.Part1, 1m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1-" + index, 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg, pickLines[0]);
			Helper.Factory.Save();

			foreach (var pickLine in pickLines)
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}
			Helper.Factory.Save();

			return pkg;
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PackingStation

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation()
		{
			TestPutawayStockInDockDoorOrPackingStation_PackingStationCore();
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_PickAlreadyHasSamePackingStation()
		{
			TestPutawayStockInDockDoorOrPackingStation_PackingStationCore(pickAlreadyHasSamePackingStation: true);
		}

		void TestPutawayStockInDockDoorOrPackingStation_PackingStationCore(bool pickAlreadyHasSamePackingStation = false)
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			if (pickAlreadyHasSamePackingStation)
			{
				pick.WP_WL_PackingStation = packingLocation.PK;
			}
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;

			var dda = helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick);
			factory.Save();

			AssertEquals("Precondition: Pick DDL.", ZGuid.Empty, pick.WP_WL_DockDoor);
			AssertEquals("Precondition: DDA DDL.", data.Whs1.WW_DefaultOutboundDockDoor, dda.WDA_WL_AssignedDockDoor);
			AssertEquals("Precondition: DDA Putaway date.", ZDateTime.Empty, dda.WDA_FirstPutawayToDockDoorUtc);

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			var webService = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLineInNewFactory = newFactory.Load<WhsTransferLine>(inTransitTransferLine.PK);
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			var ddaInNewFactory = newFactory.Load<WhsDockDoorAssignment>(dda.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Inventory should be Ready To Pack.", InventoryStatus.Codes.ReadyToPack, inTransitTransferLineInNewFactory.WE_CurrentInventoryStatus);
				AssertEquals("Line should be finalised.", true, inTransitTransferLineInNewFactory.IsFinalised);
				AssertEquals("Inventory should have been putaway.", false, inTransitTransferLineInNewFactory.WE_PutawayTime.IsEmpty);
				AssertEquals("Inventory should have been putaway to expected Packing station.", packingLocation.PK, inTransitTransferLineInNewFactory.WE_WL);
			});

			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);
			AssertEquals("Pick DDL should not of changed.", ZGuid.Empty, pickInNewFactory.WP_WL_DockDoor);
			AssertEquals("DDA DDL should not of changed.", data.Whs1.WW_DefaultOutboundDockDoor, ddaInNewFactory.WDA_WL_AssignedDockDoor);
			AssertEquals("DDA Putaway date should not of changed.", ZDateTime.Empty, ddaInNewFactory.WDA_FirstPutawayToDockDoorUtc);
			AssertEquals("Pick's Packing Station should be set", packingLocation.PK, pickInNewFactory.WP_WL_PackingStation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_PickAlreadyHasDifferentPackingStation()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var newRow2 = helper.CreateRowAndGenerateLocations(data.Whs1, "P2");
			var packingLocation2 = newRow2.Locations[0];
			packingLocation2.WLV_WLT_LocationType = packingStationLocationType.PK;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);

			pick.WP_WL_PackingStation = packingLocation.PK;

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			factory.Save();

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			var webService = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation2.WLV_LocationString); // Use different location
			AssertBusinessValidationError(webService, "Invalid Location error", "Location P2 is not valid for this job as some inventory has already been putaway to P.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(packingLocation));

			var newFactory = new BusinessObjectFactory();
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);

			AssertEquals("Pick's Packing Station should be not be changed", packingLocation.PK, pickInNewFactory.WP_WL_PackingStation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_PickByLabel()
		{
			TestPutawayStockInDockDoorOrPackingStation_PackingStation_PickByLabelCore();
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_PickByLabel_PickAlreadyHasSamePackingStation()
		{
			TestPutawayStockInDockDoorOrPackingStation_PackingStation_PickByLabelCore(pickAlreadyHasSamePackingStation: true);
		}

		void TestPutawayStockInDockDoorOrPackingStation_PackingStation_PickByLabelCore(bool pickAlreadyHasSamePackingStation = false)
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (int i = 0; i < 10; i++) // For 10 pick lines later on
			{
				helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			if (pickAlreadyHasSamePackingStation)
			{
				pick.WP_WL_PackingStation = packingLocation.PK;
			}
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 10 pick lines.", 10, pick.GetAllPickLines().Count());
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var otherPackage = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			packingHelper.CreatePackageDivot(package, pickLines[1]);
			packingHelper.CreatePackageDivot(otherPackage, pickLines[4]);
			webService.Factory.Save();

			foreach (var pickLine in pickLines)
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			webService.Factory.Save();

			AssertEquals("Precondition.", 10, pick.Transfers.Single().Lines.Count);
			pickLines[3].InventoryLine.WE_GS_NKPutawayBy = "";
			webService.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			webService.Factory.Save();

			var labelJobInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("Before put packages in packing station location FinalisedDate should be empty.", ZDateTimeOffset.Empty, labelJobInNewFactory.WTK_FinalisedDate);

			var locationString = packingLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLine1_InFactory2 = factory2.Load<WhsTransferLine>(pickLines[0].WZ_WE_InventoryLine);
			var inTransitTransferLine2_InFactory2 = factory2.Load<WhsTransferLine>(pickLines[1].WZ_WE_InventoryLine);
			var pick_InFactory2 = factory2.Load<WhsPick>(pick.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Inventory should be Ready To Pack.", InventoryStatus.Codes.ReadyToPack, inTransitTransferLine1_InFactory2.WE_CurrentInventoryStatus);
				AssertEquals("Line should be finalised.", true, inTransitTransferLine1_InFactory2.IsFinalised);
				AssertEquals("Inventory should have been putaway.", false, inTransitTransferLine1_InFactory2.WE_PutawayTime.IsEmpty);
				AssertEquals("Inventory should have been putaway to expected Packing station.", packingLocation.PK, inTransitTransferLine1_InFactory2.WE_WL);

				AssertEquals("Inventory should be Ready To Pack.", InventoryStatus.Codes.ReadyToPack, inTransitTransferLine2_InFactory2.WE_CurrentInventoryStatus);
				AssertEquals("Line should be finalised.", true, inTransitTransferLine2_InFactory2.IsFinalised);
				AssertEquals("Inventory should have been putaway.", false, inTransitTransferLine2_InFactory2.WE_PutawayTime.IsEmpty);
				AssertEquals("Inventory should have been putaway to expected Packing station.", packingLocation.PK, inTransitTransferLine2_InFactory2.WE_WL);
			});

			AssertEquals("Should *not* have finalised Transfer.", false, pick_InFactory2.Transfers.Single().IsFinalised);
			AssertEquals("Should not have changed the dock door on the pick.", data.Whs1.WW_DefaultOutboundDockDoor, pick_InFactory2.WP_WL_DockDoor);
			AssertEquals("Pick's Packing Station should be set", packingLocation.PK, pick_InFactory2.WP_WL_PackingStation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_PickByLabel_PickAlreadyHasDifferentPackingStation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var newRow2 = helper.CreateRowAndGenerateLocations(data.Whs1, "P2");
			var packingLocation2 = newRow2.Locations[0];
			packingLocation2.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (int i = 0; i < 10; i++) // For 10 pick lines later on
			{
				helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			pick.WP_WL_PackingStation = packingLocation.PK;

			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 10 pick lines.", 10, pick.GetAllPickLines().Count());
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var otherPackage = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			packingHelper.CreatePackageDivot(package, pickLines[1]);
			packingHelper.CreatePackageDivot(otherPackage, pickLines[4]);
			webService.Factory.Save();

			foreach (var pickLine in pickLines)
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			webService.Factory.Save();

			AssertEquals("Precondition.", 10, pick.Transfers.Single().Lines.Count);
			pickLines[3].InventoryLine.WE_GS_NKPutawayBy = "";
			webService.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			webService.Factory.Save();

			var labelJobInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("Before put packages in packing station location FinalisedDate should be empty.", ZDateTimeOffset.Empty, labelJobInNewFactory.WTK_FinalisedDate);

			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, packingLocation2.WLV_LocationString); // Use different location
			AssertBusinessValidationError(webService, "Invalid Location error", "Location P2 is not valid for this job as some inventory has already been putaway to P.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(packingLocation));

			var newFactory = new BusinessObjectFactory();
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);

			AssertEquals("Pick's Packing Station should be not be changed", packingLocation.PK, pickInNewFactory.WP_WL_PackingStation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_PickByLabel_MultiplePicks_PickAlreadyHasDifferentPackingStation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var newRow2 = helper.CreateRowAndGenerateLocations(data.Whs1, "P2");
			var packingLocation2 = newRow2.Locations[0];
			packingLocation2.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);

			var pickLines = pick.GetAllPickLines().ToArray();
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			pickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");

			helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);

			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocket();
			webService.Factory.Save();
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick2 = helper.CreatePickNew(order2);
			pick2.WP_WL_PackingStation = packingLocation.PK;

			var pickLines2 = pick2.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 1 pick lines.", 1, pickLines2.Length);
			webService.Factory.Save();

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLines2[0]);

			pickLines2[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var receive3 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");

			helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m);

			receive3.AllocateLocationsWithMock();
			receive3.FinaliseDocket();
			webService.Factory.Save();
			var order3 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			var pick3 = helper.CreatePickNew(order3);

			var pickLines3 = pick3.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 1 pick lines.", 1, pickLines3.Length);
			webService.Factory.Save();

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package3 = packingHelper.CreatePackage(pkgJob3, "PKG3", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package3, pickLines3[0]);

			pickLines3[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package3.PK);
			webService.Factory.Save();

			var labelJobInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			var labels = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>();
			AssertEquals("Count of Labels should be 3", 3, labels.Count());
			AssertEquals("Package2 should be the first package", package2.PK, labels.ElementAt(0).Package.PK);
			AssertEquals("Package3 should be the last package", package3.PK, labels.ElementAt(2).Package.PK);
			AssertEquals("Before put packages in packing station location FinalisedDate should be empty.", ZDateTimeOffset.Empty, labelJobInNewFactory.WTK_FinalisedDate);

			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, packingLocation2.WLV_LocationString); // Use a different location
			AssertBusinessValidationError(webService, "Invalid Location error", "Location P2 is not valid for this job as some inventory has already been putaway to P.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(packingLocation));

			var newFactory = new BusinessObjectFactory();
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			var pick2InNewFactory = newFactory.Load<WhsPick>(pick2.PK);
			var pick3InNewFactory = newFactory.Load<WhsPick>(pick3.PK);
			AssertEquals("Pick's Packing Station should be not be changed", Guid.Empty, pickInNewFactory.WP_WL_PackingStation);
			AssertEquals("Pick2's Packing Station should be not be changed", packingLocation.PK, pick2InNewFactory.WP_WL_PackingStation);
			AssertEquals("Pick3's Packing Station should be not be changed", Guid.Empty, pick3InNewFactory.WP_WL_PackingStation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_PickByLabel_MultiplePicks_AllPicksUpdatedWithPackingStation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var receive1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			var receive3 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m);
			webService.Factory.Save();

			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);
			AssertIsFinalisedPrecondition(receive3);

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick1 = helper.CreatePickNew(order1);
			var pickLines1 = pick1.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 1 pick line.", 1, pickLines1.Length);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLines1[0]);
			pickLines1[0].WZ_PickedDateTime = ZDateTimeOffset.Today;

			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick2 = helper.CreatePickNew(order2);
			var pickLines2 = pick2.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 1 pick line.", 1, pickLines2.Length);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLines2[0]);
			pickLines2[0].WZ_PickedDateTime = ZDateTimeOffset.Today;

			var order3 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			var pick3 = helper.CreatePickNew(order3);
			var pickLines3 = pick3.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 1 pick line.", 1, pickLines3.Length);

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package3 = packingHelper.CreatePackage(pkgJob3, "PKG3", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package3, pickLines3[0]);
			webService.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package3.PK);
			webService.Factory.Save();

			var labelJobInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			var labels = labelJobInNewFactory.Labels.Cast<WhsPickByLabelLabel>();
			AssertEquals("Count of Labels should be 3", 3, labels.Count());

			AssertEquals("Precondition", ZGuid.Empty, pick1.WP_WL_PackingStation);
			AssertEquals("Precondition", ZGuid.Empty, pick2.WP_WL_PackingStation);
			AssertEquals("Precondition", ZGuid.Empty, pick3.WP_WL_PackingStation);

			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var newFactory = new BusinessObjectFactory();
			var pick1InNewFactory = newFactory.Load<WhsPick>(pick1.PK);
			AssertEquals("Pick1's Packing Station should be changed", packingLocation.PK, pick1InNewFactory.WP_WL_PackingStation);
			var pick2InNewFactory = newFactory.Load<WhsPick>(pick2.PK);
			AssertEquals("Pick2's Packing Station should be changed", packingLocation.PK, pick2InNewFactory.WP_WL_PackingStation);
			var pick3InNewFactory = newFactory.Load<WhsPick>(pick3.PK);
			AssertEquals("Pick3's Packing Station should be changed", packingLocation.PK, pick3InNewFactory.WP_WL_PackingStation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_TrolleyJob()
		{
			TestPutawayStockInDockDoorOrPackingStation_PackingStation_TrolleyJobCore();
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_TrolleyJob_PickAlreadyHasSamePackingStation()
		{
			TestPutawayStockInDockDoorOrPackingStation_PackingStation_TrolleyJobCore(pickAlreadyHasSamePackingStation: true);
		}

		void TestPutawayStockInDockDoorOrPackingStation_PackingStation_TrolleyJobCore(bool pickAlreadyHasSamePackingStation = false)
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (int i = 0; i < 10; i++) // For 10 pick lines later on
			{
				helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			if (pickAlreadyHasSamePackingStation)
			{
				pick.WP_WL_PackingStation = packingLocation.PK;
			}

			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 10 pick lines.", 10, pick.GetAllPickLines().Count());
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley1, pickLines[0]);

			var pkg_OnTrolley2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			pkg_OnTrolley2.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley2, pickLines[1]);

			var pkg_OnTrolley_NotPicked = packingHelper.CreatePackage(pkgJob, "PKG3", 1, PkgUnit.Box);
			pkg_OnTrolley_NotPicked.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley_NotPicked, pickLines[2]);

			var pkg_OnTrolley_PutawayAssignedToAnotherUser = packingHelper.CreatePackage(pkgJob, "PKG4", 1, PkgUnit.Box);
			pkg_OnTrolley_PutawayAssignedToAnotherUser.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley_PutawayAssignedToAnotherUser, pickLines[3]);

			var pkg_NotOnTrolley = packingHelper.CreatePackage(pkgJob, "PKG5", 1, PkgUnit.Box);
			pkg_NotOnTrolley.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_NotOnTrolley, pickLines[4]);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley2, 2);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley_NotPicked, 3);
			webService.Factory.Save();

			foreach (var pickLine in pickLines.Except(new[] { pickLines[2] }))
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			webService.Factory.Save();

			AssertEquals("Precondition.", 9, pick.Transfers.Single().Lines.Count);
			pickLines[3].InventoryLine.WE_GS_NKPutawayBy = "";
			webService.Factory.Save();

			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley1.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley2.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley_NotPicked.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley_PutawayAssignedToAnotherUser.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_NotOnTrolley.IsClosed);

			var locationString = packingLocation.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTrolleyTransferLine1_InFactory2 = factory2.Load<WhsTransferLine>(pickLines[0].WZ_WE_InventoryLine);
			var inTransitTrolleyTransferLine2_InFactory2 = factory2.Load<WhsTransferLine>(pickLines[1].WZ_WE_InventoryLine);
			var pick_InFactory2 = factory2.Load<WhsPick>(pick.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Inventory should be Ready To Pack.", InventoryStatus.Codes.ReadyToPack, inTransitTrolleyTransferLine1_InFactory2.WE_CurrentInventoryStatus);
				AssertEquals("Line should be finalised.", true, inTransitTrolleyTransferLine1_InFactory2.IsFinalised);
				AssertEquals("Inventory should have been putaway.", false, inTransitTrolleyTransferLine1_InFactory2.WE_PutawayTime.IsEmpty);
				AssertEquals("Inventory should have been putaway to expected Packing station.", packingLocation.PK, inTransitTrolleyTransferLine1_InFactory2.WE_WL);

				AssertEquals("Inventory should be Ready To Pack.", InventoryStatus.Codes.ReadyToPack, inTransitTrolleyTransferLine2_InFactory2.WE_CurrentInventoryStatus);
				AssertEquals("Line should be finalised.", true, inTransitTrolleyTransferLine2_InFactory2.IsFinalised);
				AssertEquals("Inventory should have been putaway.", false, inTransitTrolleyTransferLine2_InFactory2.WE_PutawayTime.IsEmpty);
				AssertEquals("Inventory should have been putaway to expected Packing station.", packingLocation.PK, inTransitTrolleyTransferLine2_InFactory2.WE_WL);
			});

			AssertEquals("Should *not* have finalised Transfer.", false, pick_InFactory2.Transfers.Single().IsFinalised);
			AssertEquals("Should not have changed the dock door on the pick.", data.Whs1.WW_DefaultOutboundDockDoor, pick_InFactory2.WP_WL_DockDoor);
			AssertEquals("Pick's Packing Station should be set", packingLocation.PK, pick_InFactory2.WP_WL_PackingStation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_TrolleyJob_PickAlreadyHasDifferentPackingStation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var newRow2 = helper.CreateRowAndGenerateLocations(data.Whs1, "P2");
			var packingLocation2 = newRow2.Locations[0];
			packingLocation2.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (int i = 0; i < 10; i++) // For 10 pick lines later on
			{
				helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			pick.WP_WL_PackingStation = packingLocation.PK;

			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 10 pick lines.", 10, pick.GetAllPickLines().Count());
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley1, pickLines[0]);

			var pkg_OnTrolley2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			pkg_OnTrolley2.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley2, pickLines[1]);

			var pkg_OnTrolley_NotPicked = packingHelper.CreatePackage(pkgJob, "PKG3", 1, PkgUnit.Box);
			pkg_OnTrolley_NotPicked.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley_NotPicked, pickLines[2]);

			var pkg_OnTrolley_PutawayAssignedToAnotherUser = packingHelper.CreatePackage(pkgJob, "PKG4", 1, PkgUnit.Box);
			pkg_OnTrolley_PutawayAssignedToAnotherUser.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley_PutawayAssignedToAnotherUser, pickLines[3]);

			var pkg_NotOnTrolley = packingHelper.CreatePackage(pkgJob, "PKG5", 1, PkgUnit.Box);
			pkg_NotOnTrolley.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_NotOnTrolley, pickLines[4]);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley2, 2);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley_NotPicked, 3);
			webService.Factory.Save();

			foreach (var pickLine in pickLines.Except(new[] { pickLines[2] }))
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			webService.Factory.Save();

			AssertEquals("Precondition.", 9, pick.Transfers.Single().Lines.Count);
			pickLines[3].InventoryLine.WE_GS_NKPutawayBy = "";
			webService.Factory.Save();

			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley1.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley2.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley_NotPicked.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_OnTrolley_PutawayAssignedToAnotherUser.IsClosed);
			AssertEquals("Precondition: Package is not closed.", false, pkg_NotOnTrolley.IsClosed);

			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation2.WLV_LocationString); // Use different location
			AssertBusinessValidationError(webService, "Invalid Location error", "Location P2 is not valid for this job as some inventory has already been putaway to P.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(packingLocation));

			var newFactory = new BusinessObjectFactory();
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);

			AssertEquals("Pick's Packing Station should be not be changed", packingLocation.PK, pickInNewFactory.WP_WL_PackingStation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_TrolleyJob_MultiplePicks_PickAlreadyHasDifferentPackingStation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var newRow2 = helper.CreateRowAndGenerateLocations(data.Whs1, "P2");
			var packingLocation2 = newRow2.Locations[0];
			packingLocation2.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);

			var pickLines = pick.GetAllPickLines().ToArray();
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg, pickLines[0]);
			pickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;

			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocket();
			webService.Factory.Save();

			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick2 = helper.CreatePickNew(order2);
			pick2.WP_WL_PackingStation = packingLocation.PK;

			var pickLines2 = pick2.GetAllPickLines().ToArray();
			webService.Factory.Save();

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			pkg2.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg2, pickLines2[0]);
			pickLines2[0].WZ_PickedDateTime = ZDateTimeOffset.Today;

			var receive3 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m);
			receive3.AllocateLocationsWithMock();
			receive3.FinaliseDocket();
			webService.Factory.Save();

			var order3 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			var pick3 = helper.CreatePickNew(order3);

			var pickLines3 = pick3.GetAllPickLines().ToArray();
			webService.Factory.Save();

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var pkg3 = packingHelper.CreatePackage(pkgJob3, "PKG3", 1, PkgUnit.Box);
			pkg3.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg3, pickLines3[0]);
			pickLines3[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg3, 3);

			webService.Factory.Save();

			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation2.WLV_LocationString); // Use different location

			AssertBusinessValidationError(webService, "Invalid Location error", "Location P2 is not valid for this job as some inventory has already been putaway to P.", response);
			AssertExpectedLocationDetails(response, GetWhsLocationInfo(packingLocation));

			var newFactory = new BusinessObjectFactory();
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			var pick2InNewFactory = newFactory.Load<WhsPick>(pick2.PK);
			var pick3InNewFactory = newFactory.Load<WhsPick>(pick3.PK);
			AssertEquals("Pick's Packing Station should be not be changed", Guid.Empty, pickInNewFactory.WP_WL_PackingStation);
			AssertEquals("Pick2's Packing Station should be not be changed", packingLocation.PK, pick2InNewFactory.WP_WL_PackingStation);
			AssertEquals("Pick3's Packing Station should be not be changed", Guid.Empty, pick3InNewFactory.WP_WL_PackingStation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStation_CartonTrolleyJob_SomePartsOfTheOrderAlreadyInPackingStation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pickByLabelPackage = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pickByLabelPackage, pickLine1);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine2);
			webService.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, pickByLabelPackage.PK);
			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);
			webService.Factory.Save();

			var pickByLabelPutawayToPackingStationResponse = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, pickByLabelPutawayToPackingStationResponse.Error);
			AssertNull("Should be no error.", pickByLabelPutawayToPackingStationResponse.ErrorMessage);

			var cartonTrolleyJobPutawayToPackingStationResponse = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, cartonTrolleyJobPutawayToPackingStationResponse.Error);
			AssertNull("Should be no error.", cartonTrolleyJobPutawayToPackingStationResponse.ErrorMessage);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_DockDoor_CartonTrolleyJob_SomePartsOfTheOrderAlreadyInPackingStation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pickByLabelPackage = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pickByLabelPackage, pickLine1);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine2);
			webService.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, pickByLabelPackage.PK);
			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);
			webService.Factory.Save();

			var pickByLabelPutawayToPackingStationResponse = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, pickByLabelPutawayToPackingStationResponse.Error);
			AssertNull("Should be no error.", pickByLabelPutawayToPackingStationResponse.ErrorMessage);

			var cartonTrolleyJobPutawayToPackingStationResponse = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertBusinessValidationError(webService, "Invalid Location error", "Location DOCKDOOR is not valid for this job as some inventory has already been putaway to P.", cartonTrolleyJobPutawayToPackingStationResponse);
			AssertExpectedLocationDetails(cartonTrolleyJobPutawayToPackingStationResponse, GetWhsLocationInfo(packingLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStationAllowed_NotPackingStationNorDockDoorLocation()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			factory.Save();

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			factory.Save();

			var randomLocation = helper.CreateRowAndGenerateLocations(data.Whs1, "R").Locations[0];
			factory.Save();

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			var webService = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, randomLocation.WLV_LocationString);
			AssertEquals("Should have error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should have error.", "Location R is not an Outbound Dock Door Location nor a Packing Station Location.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_TrolleyPickingPickByCarton_PackingStationNotAllowed_TrolleyPickToCarton()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 1 pick line.", 1, pickLines.Length);
			factory.Save();

			var packingHelper = new PackingTestHelper(factory);
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);

			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLines[0]);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);
			factory.Save();

			pickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			factory.Save();

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			factory.Save();

			var webService = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
			AssertEquals("Should have error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should have error.", "Location P is not an Outbound Dock Door Location.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_TrolleyPickingPickByCarton_PackingStationNotAllowed_PickAndPack()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_IsPickAndPackEnabled = true;

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			factory.Save();

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			factory.Save();

			var webService = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString);
			AssertEquals("Should have error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should have error.", "Location P is not an Outbound Dock Door Location.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_ConcurrencyErrorWhenSettingPackingStationToPick()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var newRow2 = helper.CreateRowAndGenerateLocations(data.Whs1, "P2");
			var packingLocation2 = newRow2.Locations[0];
			packingLocation2.WLV_WLT_LocationType = packingStationLocationType.PK;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			factory.Save();

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			factory.Save();

			factory.Saving += delegate
			{
				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
				pickInOtherFactory.WP_WL_PackingStation = packingLocation2.PK;
				otherFactory.Save();
			};

			PutawayStockInDockDoorOrPackingStationResponse response = null;
			AssertNoExceptionThrown(() => response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString));

			AssertEquals("Should have error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should have error.", "While you have been working with this job another user has made changes. Please restart the operation and try again.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLineInNewFactory = newFactory.Load<WhsTransferLine>(inTransitTransferLine.PK);
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);

			AssertEquals("Pick's Packing Station should be set by another factory", packingLocation2.PK, pickInNewFactory.WP_WL_PackingStation);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_SetPackingStationForTotePicking_ShouldBumpVersionIDOnAllRelatedTrolleys()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 6; i++)
			{
				helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			var order3 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 2m);
			var order4 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 2m);
			var order5 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 2m);
			var order6 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part1, 2m);

			var pick1 = helper.CreatePickNew(order1, order2);
			var pickLines1 = pick1.GetAllPickLines().ToArray();
			pickLines1.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);
			AssertEquals("Precondition: 2 pick lines.", 2, pick1.GetAllPickLines().Count());

			var pick2 = helper.CreatePickNew(order3, order4);
			var pickLines2 = pick2.GetAllPickLines().ToArray();
			pickLines2.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);
			AssertEquals("Precondition: 2 pick lines.", 2, pick2.GetAllPickLines().Count());
			webService.Factory.Save();

			var pick3 = helper.CreatePickNew(order5);
			var pickLines3 = pick3.GetAllPickLines().ToArray();
			pickLines3.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);
			AssertEquals("Precondition: 1 pick lines.", 1, pick3.GetAllPickLines().Count());
			webService.Factory.Save();

			var pick4 = helper.CreatePickNew(order6);
			var pickLines4 = pick4.GetAllPickLines().ToArray();
			pickLines4.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);
			AssertEquals("Precondition: 1 pick lines.", 1, pick4.GetAllPickLines().Count());
			webService.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			pkg1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg1, pickLines1.Where(x => x.WZ_WE_TransactionLine == order1.Lines[0].PK).FirstOrDefault());

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			pkg2.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg2, pickLines1.Where(x => x.WZ_WE_TransactionLine == order2.Lines[0].PK).FirstOrDefault());

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var pkg3 = packingHelper.CreatePackage(pkgJob3, "PKG3", 1, PkgUnit.Box);
			pkg3.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg3, pickLines2.Where(x => x.WZ_WE_TransactionLine == order3.Lines[0].PK).FirstOrDefault());

			var pkgJob4 = PkgPackageJob.LoadOrCreatePackageJob(order4);
			var pkg4 = packingHelper.CreatePackage(pkgJob4, "PKG4", 1, PkgUnit.Box);
			pkg4.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg4, pickLines2.Where(x => x.WZ_WE_TransactionLine == order4.Lines[0].PK).FirstOrDefault());

			var pkgJob5 = PkgPackageJob.LoadOrCreatePackageJob(order5);
			var pkg5 = packingHelper.CreatePackage(pkgJob5, "PKG5", 1, PkgUnit.Box);
			pkg5.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg5, pickLines3.Where(x => x.WZ_WE_TransactionLine == order5.Lines[0].PK).FirstOrDefault());

			var pkgJob6 = PkgPackageJob.LoadOrCreatePackageJob(order6);
			var pkg6 = packingHelper.CreatePackage(pkgJob6, "PKG6", 1, PkgUnit.Box);
			pkg6.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg6, pickLines4.Where(x => x.WZ_WE_TransactionLine == order6.Lines[0].PK).FirstOrDefault());

			var trolley1 = helper.CreateTrolley("T001");
			var trolleyJob1 = helper.CreateWhsPickTrolleyJob(trolley1);
			var trolley2 = helper.CreateTrolley("T002");
			var trolleyJob2 = helper.CreateWhsPickTrolleyJob(trolley2);
			var trolley3 = helper.CreateTrolley("T003");
			var trolleyJob3 = helper.CreateWhsPickTrolleyJob(trolley3);
			var trolley4 = helper.CreateTrolley("T004");
			var trolleyJob4 = helper.CreateWhsPickTrolleyJob(trolley4);
			helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg3, 2);
			helper.CreateWhsPickTrolleySlot(trolleyJob2, pkg2, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob3, pkg4, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob3, pkg5, 2);
			helper.CreateWhsPickTrolleySlot(trolleyJob4, pkg6, 1);
			webService.Factory.Save();

			AssertEquals("Precondition - CriticalVersionID on trolley1", ZGuid.Empty, trolleyJob1.WTJ_CriticalChangesVersionID);
			AssertEquals("Precondition - CriticalVersionID on trolley2", ZGuid.Empty, trolleyJob2.WTJ_CriticalChangesVersionID);
			AssertEquals("Precondition - CriticalVersionID on trolley3", ZGuid.Empty, trolleyJob3.WTJ_CriticalChangesVersionID);
			AssertEquals("Precondition - CriticalVersionID on trolley4", ZGuid.Empty, trolleyJob4.WTJ_CriticalChangesVersionID);

			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob1.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			AssertEquals("Pick1 should have set WP_WL_PackingStation", packingLocation.PK, pick1.WP_WL_PackingStation);
			AssertEquals("Pick2 should have set WP_WL_PackingStation", packingLocation.PK, pick2.WP_WL_PackingStation);
			AssertEquals("Pick3 should have set WP_WL_PackingStation", packingLocation.PK, pick3.WP_WL_PackingStation);
			Assert("Pick4 should NOT set WP_WL_PackingStation", pick4.WP_WL_PackingStation.IsEmpty);

			AssertNotEquals("Should have bump CriticalVersionID on trolley1", ZGuid.Empty, trolleyJob1.WTJ_CriticalChangesVersionID);
			AssertNotEquals("Should have bump CriticalVersionID on trolley2", ZGuid.Empty, trolleyJob2.WTJ_CriticalChangesVersionID);
			AssertNotEquals("Should have bump CriticalVersionID on trolley3", ZGuid.Empty, trolleyJob3.WTJ_CriticalChangesVersionID);
			AssertEquals("Should NOT have bump CriticalVersionID on trolley4", ZGuid.Empty, trolleyJob4.WTJ_CriticalChangesVersionID);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_SetPackingStationForTotePicking()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 4; i++)
			{
				helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			var pick1 = helper.CreatePickNew(order1, order2);
			var pickLines1 = pick1.GetAllPickLines().ToArray();
			pickLines1.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);
			AssertEquals("Precondition: 2 pick lines.", 2, pick1.GetAllPickLines().Count());

			var order3 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 2m);
			var order4 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 2m);
			var pick2 = helper.CreatePickNew(order3, order4);
			var pickLines2 = pick2.GetAllPickLines().ToArray();
			pickLines2.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);
			AssertEquals("Precondition: 2 pick lines.", 2, pick2.GetAllPickLines().Count());
			webService.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			pkg1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg1, pickLines1.Where(x => x.WZ_WE_TransactionLine == order1.Lines[0].PK).FirstOrDefault());

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			pkg2.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg2, pickLines1.Where(x => x.WZ_WE_TransactionLine == order2.Lines[0].PK).FirstOrDefault());

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var pkg3 = packingHelper.CreatePackage(pkgJob3, "PKG3", 1, PkgUnit.Box);
			pkg3.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg3, pickLines2.Where(x => x.WZ_WE_TransactionLine == order3.Lines[0].PK).FirstOrDefault());

			var pkgJob4 = PkgPackageJob.LoadOrCreatePackageJob(order4);
			var pkg4 = packingHelper.CreatePackage(pkgJob4, "PKG4", 1, PkgUnit.Box);
			pkg4.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg4, pickLines2.Where(x => x.WZ_WE_TransactionLine == order4.Lines[0].PK).FirstOrDefault());

			var trolley1 = helper.CreateTrolley("T001");
			var trolleyJob1 = helper.CreateWhsPickTrolleyJob(trolley1);
			helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg3, 2);

			var trolley2 = helper.CreateTrolley("T002");
			var trolleyJob2 = helper.CreateWhsPickTrolleyJob(trolley2);
			helper.CreateWhsPickTrolleySlot(trolleyJob2, pkg2, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob2, pkg4, 2);
			webService.Factory.Save();

			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob1.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			AssertEquals("Pick should have set WP_WL_PackingStation", packingLocation.PK, pick1.WP_WL_PackingStation);
			AssertEquals("Pick should have set WP_WL_PackingStation", packingLocation.PK, pick2.WP_WL_PackingStation);
		}

		[UseSnapshotProtection(skipTransaction: true)] // We care about accurate rollback behaviour in this test, post order is intermittent and interferes with assertions.
		public void TestPutawayStockInDockDoorOrPackingStation_SetPackingStationForTotePicking_ThrowErrorIfAddSlotToTrolleyAtTheSameTime()
		{
			var factory = Helper.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var pick = helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 2 pick lines.", 2, pick.GetAllPickLines().Count());
			factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley1, pickLines[0]);

			var pkg_OnTrolley2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			pkg_OnTrolley2.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley2, pickLines[1]);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley2, 2);
			factory.Save();
			pickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			factory.Save();
			packingHelper.SetRefPackTypeUOM(PkgUnit.Carton, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(PkgUnit.Tote, UOMPackTypesList.Codes.SplitCase);
			packingHelper.SetRefPackTypeUOM(PkgUnit.Box, UOMPackTypesList.Codes.Case);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			factory.Save();
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_CartoniseSplitCases = false;
			orderLine1.PickLines.Single().WZ_F3_NKAllocatedPackType = PkgUnit.Tote;
			factory.Save();

			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			var webService = GetNewWebService();
			webService.Factory.RefreshEnabled = false;
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			webService.Factory.Saving += delegate
			{
				using (Db.DisposableActionForDbConnection())
				using (var connection2 = Db.NewExtraConnectionToMainDb())
				{
					var factory2 = new BusinessObjectFactory(connection2);
					var helper2 = new WhsTestHelperFunctions(factory2);
					var packingHelper2 = new PackingTestHelper(factory2);

					var packageJobInFactory2 = factory2.Load<PkgPackageJob>(pkgJob.PK);
					var pkg3 = packingHelper2.CreatePackage(packageJobInFactory2, "PKG3", 1, PkgUnit.Box);
					pkg3.SetIsTote(true);

					var trolleyJobInFactory2 = factory2.Load<WhsPickTrolleyJob>(trolleyJob.PK);
					helper2.CreateWhsPickTrolleySlot(trolleyJobInFactory2, pkg3, 3);
					factory2.Save();
				}
			};

			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
			AssertSuccessfulResponse(response, webService);

			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Concurrency error message should be prompted",
				"While you have been working with this job another user has made changes. Please restart the operation and try again.",
				response.ErrorMessage);
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInDB = newFactory.Load<WhsPick>(pick.PK);
			var pick1InDB = newFactory.Load<WhsPick>(pick1.PK);
			Assert("Pick's Packing Station should NOT be set: ", pickInDB.WP_WL_PackingStation.IsEmpty);
			Assert("Pick1's Packing Station should NOT be set: ", pick1InDB.WP_WL_PackingStation.IsEmpty);
			var trolleyJobInDB = newFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK);
			AssertEquals("Should add a new slot.", 3, trolleyJobInDB.Slots.Count);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_SetPackingStationForTotePicking_DbHits()
		{
			var helper = new WhsTestHelperFunctions(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 4; i++)
			{
				helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			var pick1 = helper.CreatePickNew(order1, order2);
			var pickLines1 = pick1.GetAllPickLines().ToArray();
			pickLines1.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);
			AssertEquals("Precondition: 2 pick lines.", 2, pick1.GetAllPickLines().Count());

			var order3 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 2m);
			var order4 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 2m);
			var pick2 = helper.CreatePickNew(order3, order4);
			var pickLines2 = pick2.GetAllPickLines().ToArray();
			pickLines2.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);
			AssertEquals("Precondition: 2 pick lines.", 2, pick2.GetAllPickLines().Count());
			Helper.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			pkg1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg1, pickLines1.Where(x => x.WZ_WE_TransactionLine == order1.Lines[0].PK).FirstOrDefault());

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			pkg2.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg2, pickLines1.Where(x => x.WZ_WE_TransactionLine == order2.Lines[0].PK).FirstOrDefault());

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var pkg3 = packingHelper.CreatePackage(pkgJob3, "PKG3", 1, PkgUnit.Box);
			pkg3.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg3, pickLines2.Where(x => x.WZ_WE_TransactionLine == order3.Lines[0].PK).FirstOrDefault());

			var pkgJob4 = PkgPackageJob.LoadOrCreatePackageJob(order4);
			var pkg4 = packingHelper.CreatePackage(pkgJob4, "PKG4", 1, PkgUnit.Box);
			pkg4.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg4, pickLines2.Where(x => x.WZ_WE_TransactionLine == order4.Lines[0].PK).FirstOrDefault());

			var trolley1 = helper.CreateTrolley("T001");
			var trolleyJob1 = helper.CreateWhsPickTrolleyJob(trolley1);
			helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg3, 2);

			var trolley2 = helper.CreateTrolley("T002");
			var trolleyJob2 = helper.CreateWhsPickTrolleyJob(trolley2);
			helper.CreateWhsPickTrolleySlot(trolleyJob2, pkg2, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob2, pkg4, 2);
			Helper.Factory.Save();

			var expectedDBHits = new Dictionary<string, int>();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob1.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
				Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Should be no error", ErrorTypes.None, response.Error);
			}
		}

		public void TestPutawayStockInDockDoorOrPackingStation_SetPackingStationForTotePicking_OverDepthOfTen()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var packingStationLocationType = helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 44m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			webService.Factory.Save();

			var allPicks = new List<WhsPick>();
			var allTrolleyJob = new List<WhsPickTrolleyJob>();

			for (var i = 0; i < 22; i++)
			{
				var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i + "1", data.Part1, 1m);
				var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i + "2", data.Part1, 1m);
				var pick = helper.CreatePickNew(order1, order2);
				var pickLines = pick.GetAllPickLines().ToArray();
				pickLines.ForEach(x => x.WZ_PickedDateTime = ZDateTimeOffset.Today);
				AssertEquals("Precondition: 2 pick lines.", 2, pick.GetAllPickLines().Count());

				allPicks.Add(pick);
			}
			webService.Factory.Save();

			for (var i = 0; i < allPicks.Count; i++)
			{
				var order1 = (WhsOrder)allPicks[i].Orders[1];
				var index = i + 1 == allPicks.Count ? 0 : i + 1;
				var order2 = (WhsOrder)allPicks[index].Orders[0];

				var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
				var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG" + i + "1", 1, PkgUnit.Box);
				pkg1.SetIsTote(true);
				packingHelper.CreatePackageDivot(pkg1, order1.Lines[0].PickLines[0]);

				var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
				var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG" + i + "2", 1, PkgUnit.Box);
				pkg2.SetIsTote(true);
				packingHelper.CreatePackageDivot(pkg2, order2.Lines[0].PickLines[0]);

				var trolley = helper.CreateTrolley("T00" + i);
				var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
				helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
				helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2);
				allTrolleyJob.Add(trolleyJob);
			}

			webService.Factory.Save();

			var response = webService.PutawayStockInDockDoorOrPackingStation(allTrolleyJob[0].PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);

			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			AssertEquals("All related Pick have set WP_WL_PackingStation", true, allPicks.All(x => x.WP_WL_PackingStation == packingLocation.PK));

			AssertEquals("Should reported over 10 depth search", "The depth of search all Picks related to the Trolley is more than 10.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PickByBOM

		public void TestPutawayStockInDockDoorOrPackingStation_PickByBOM_Pick()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);
			factory.Save();
			helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			factory.Save();

			var packingStationLocationType = helper.CreateLocationType("XYZ","Test", false, 0, LocationClasses.Codes.PST);
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

			var webService = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no error.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByBOM_TrolleyJob()
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
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);
			webService.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var response = webService2.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, packingLocation.WLV_LocationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no error.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByBOM_PickByLabel()
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
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLine);
			webService.Factory.Save();

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var newFactory = webService2.Factory;

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(newFactory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			newFactory.Save();

			var locationString = packingLocation.WLV_LocationString;
			var response = webService2.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no error.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByBOM_PartAttributes()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			helper.SetClientAllAttributeType(data.Org1, mandatoryAttributeType: true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: false);
			factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.FindLocation("A-1"), "PID456", ZDate.Today.AddDays(20), ZDate.Today, "PA1", "PA2", "PA3", string.Empty);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var pick = helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			helper.CreateWhsOrderLine(order, data.Part1, 1m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			factory.Save();

			var pickLine = order.Lines.Single(l => l.WE_OP == data.Part2.PK).PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			factory.Save();

			var webService = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no error.", ErrorTypes.None, response.Error);
				AssertEquals("Should have no error.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PickByBOM_PartAttributes_PST_ThenDDL()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			helper.SetClientAllAttributeType(data.Org1, mandatoryAttributeType: true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var packingStationType = helper.CreateLocationType("PST", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = data.Whs1.FindLocation("A-2");
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.FindLocation("A-1"), "PID456", ZDate.Today.AddDays(20), ZDate.Today, "PA1", "PA2", "PA3", string.Empty);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var pick = helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			helper.CreateWhsOrderLine(order, data.Part1, 1m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			factory.Save();

			var pickLine = order.Lines.Single(l => l.WE_OP == data.Part2.PK).PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			factory.Save();

			var webService1 = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var response1 = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingStationLocation.WLV_LocationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no error - PST.", ErrorTypes.None, response1.Error);
				AssertEquals("Should have no error - PST.", true, string.IsNullOrEmpty(response1.ErrorMessage));
			});

			var orderLine = order.Lines.Cast<WhsOrderLine>().Single(l => l.WE_OP == data.Part1.PK);
			var orderpickline = orderLine.PickLines.Single();
			orderpickline.WZ_PickedDateTime = ZDateTimeOffset.Now;
			factory.Save();

			var packingHelper = new PackingTestHelper(factory);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 1m);
			package.KP_ClosedTimeUtc = DateTime.UtcNow;
			factory.Save();

			AssertEquals("Precondition", true, package.IsClosed);

			var webService2 = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var response2 = webService2.PutawayPackagesInDockDoorLocation([package.PK.ToGuid()], "DOCKDOOR");
			CombineAssertions(() =>
			{
				AssertEquals("Should have no error - DDL.", ErrorTypes.None, response2.Error);
				AssertEquals("Should have no error - DDL.", true, string.IsNullOrEmpty(response2.ErrorMessage));
			});
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_SingleOrder_SingleComponent_PutawayMultipleTimes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var date2 = new ZDateTime(2024, 4, 1, 8, 0, 0);
			var date3 = new ZDateTime(2024, 4, 1, 9, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			var dateOffset2 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date2);
			var dateOffset3 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date3);
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 1m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 3m, location1);
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 6m, location1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			var componentOrderLine = orderLine.ChildComponentLines.First();
			AssertEquals("Should be allocated.", 3, componentOrderLine.PickLines.Count);
			var componentPickLine1 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 1m);
			var componentPickLine2 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 3m);
			var componentPickLine3 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 6m);
			componentPickLine1.WZ_PickedDateTime = dateOffset1;
			Helper.Factory.Save();

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);

			var componentTransferLine1 = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, componentTransferLine1.WE_CurrentInventoryStatus);

			// putaway 1 wheel
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLines = transfer.Lines;
			AssertEquals("Kit Transfer Line was not created because there was not enough components.", 1, transferLines.Count);
			componentTransferLine1 = newFactory.Load<WhsTransferLine>(componentTransferLine1.PK);
			AssertLinePutaway(componentTransferLine1);
			var componentPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(componentPickLineForTransfer, componentTransferLine1.PK, receiveLine1.PK, ZGuid.Empty, 1m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var componentPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, componentTransferLine1.PK)).Single();
			AssertPickLine(componentPickLineForOrder, componentOrderLine.PK, componentTransferLine1.PK, receiveLine1.PK, 1m, ZDateTimeOffset.Empty, ZString.Empty);

			componentPickLine2.WZ_PickedDateTime = dateOffset2;
			Helper.Factory.Save();

			// putaway 3 wheels
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			transferLines = transfer.Lines;
			AssertEquals("2 Lines for Components and 1 for Kit.", 3, transferLines.Count);

			componentTransferLine1 = newFactory.Load<WhsTransferLine>(componentTransferLine1.PK);
			AssertComponentLinePutaway(componentTransferLine1, InventoryStatus.Codes.Staged, dockdoorLocation.PK);
			var componentPickLineForTransfer1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(componentPickLineForTransfer1, componentTransferLine1.PK, receiveLine1.PK, ZGuid.Empty, 1m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var componentPickLineForOrder1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, componentTransferLine1.PK)).Single();
			AssertPickLine(componentPickLineForOrder1, componentOrderLine.PK, componentTransferLine1.PK, receiveLine1.PK, 1m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var componentTransferLine2 = transferLines.Single(l => l.WE_TransactionQuantity == 3m);
			AssertComponentLinePutaway(componentTransferLine2, InventoryStatus.Codes.Staged, location1.PK);
			var componentPickLineForTransfer2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine2.PK)).Single();
			AssertPickLine(componentPickLineForTransfer2, componentTransferLine2.PK, receiveLine2.PK, ZGuid.Empty, 3m, dateOffset2, GlbStaff.CurrentUser.GS_Code);
			var componentPickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, componentTransferLine2.PK)).Single();
			AssertPickLine(componentPickLineForOrder2, componentOrderLine.PK, componentTransferLine2.PK, receiveLine2.PK, 3m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK && l.WE_TransactionQuantity == 2m);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location1.PK, dockdoorLocation.PK, 2m, GlbStaff.CurrentUser.GS_Code, dateOffset2);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("2 Receive Lines now because putaway will split Lines, to make sure the DocketLine/Inventory Statuses are correct.", 2, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			var kitReceiveLine2 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			AssertKitReceiveLine(kitReceiveLine1, location1.PK, dateOffset2, 2m);
			AssertKitReceiveLineNotChanged(kitReceiveLine2, 3m);

			var kitPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK)).Single();
			AssertPickLine(kitPickLineForTransfer, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 2m, dateOffset2, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 2m, ZDateTimeOffset.Empty, string.Empty);

			// putaway remaining 6 wheels
			componentPickLine3.WZ_PickedDateTime = dateOffset3;
			Helper.Factory.Save();
			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService3.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response3 = webService3.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response3.Error);
				AssertNull("Should be no error.", response3.ErrorMessage);
			});

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			transferLines = transfer.Lines;
			AssertEquals("2 more Lines, 1 for Component and 1 for Kit.", 5, transferLines.Count);

			componentTransferLine1 = newFactory.Load<WhsTransferLine>(componentTransferLine1.PK);
			AssertComponentLinePutaway(componentTransferLine1, InventoryStatus.Codes.Staged, dockdoorLocation.PK);
			componentPickLineForTransfer1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(componentPickLineForTransfer1, componentTransferLine1.PK, receiveLine1.PK, ZGuid.Empty, 1m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			componentPickLineForOrder1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, componentTransferLine1.PK)).Single();
			AssertPickLine(componentPickLineForOrder1, componentOrderLine.PK, componentTransferLine1.PK, receiveLine1.PK, 1m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			componentTransferLine2 = transferLines.Single(l => l.WE_OP == wheel.PK && l.WE_TransactionQuantity == 3m);
			AssertComponentLinePutaway(componentTransferLine2, InventoryStatus.Codes.Staged, location1.PK);
			componentPickLineForTransfer2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine2.PK)).Single();
			AssertPickLine(componentPickLineForTransfer2, componentTransferLine2.PK, receiveLine2.PK, ZGuid.Empty, 3m, dateOffset2, GlbStaff.CurrentUser.GS_Code);
			componentPickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, componentTransferLine2.PK)).Single();
			AssertPickLine(componentPickLineForOrder2, componentOrderLine.PK, componentTransferLine2.PK, receiveLine2.PK, 3m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var componentTransferLine3 = transferLines.Single(l => l.WE_OP == wheel.PK && l.WE_TransactionQuantity == 6m);
			AssertComponentLinePutaway(componentTransferLine3, InventoryStatus.Codes.Staged, location1.PK);
			var componentPickLineForTransfer3 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine3.PK)).Single();
			AssertPickLine(componentPickLineForTransfer3, componentTransferLine3.PK, receiveLine3.PK, ZGuid.Empty, 6m, dateOffset3, GlbStaff.CurrentUser.GS_Code);
			var componentPickLineForOrder3 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, componentTransferLine3.PK)).Single();
			AssertPickLine(componentPickLineForOrder3, componentOrderLine.PK, componentTransferLine3.PK, receiveLine3.PK, 6m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK && l.WE_TransactionQuantity == 2m);
			var kitTransferLine2 = transferLines.Single(l => l.WE_OP == bike.PK && l.WE_TransactionQuantity == 3m);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location1.PK, dockdoorLocation.PK, 2m, GlbStaff.CurrentUser.GS_Code, dateOffset2);
			AssertKitLinePutaway(kitTransferLine2, InventoryStatus.Codes.Staged, kitReceiveLine2.PK, location1.PK, dockdoorLocation.PK, 3m, GlbStaff.CurrentUser.GS_Code, dateOffset3);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("2 Receive Lines now because putaway will split Lines, to make sure the DocketLine/Inventory Statuses are correct.", 2, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			kitReceiveLine2 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			AssertKitReceiveLine(kitReceiveLine1, location1.PK, dateOffset2, 2m);
			AssertKitReceiveLine(kitReceiveLine2, location1.PK, dateOffset3, 3m);

			kitPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK)).Single();
			AssertPickLine(kitPickLineForTransfer, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 2m, dateOffset2, GlbStaff.CurrentUser.GS_Code);

			kitPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 2m, ZDateTimeOffset.Empty, string.Empty);

			var kitPickLineForTransfer2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine2.PK)).Single();
			AssertPickLine(kitPickLineForTransfer2, kitTransferLine2.PK, kitReceiveLine2.PK, ZGuid.Empty, 3m, dateOffset3, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine2.PK)).Single();
			AssertPickLine(kitPickLineForOrder2, orderLine.PK, kitTransferLine2.PK, kitReceiveLine2.PK, 3m, ZDateTimeOffset.Empty, string.Empty);
		}

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_SingleOrder_SingleComponent_MultipleUnpickedKitPickLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 8m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 2m, location1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			var componentOrderLine = orderLine.ChildComponentLines.First();
			AssertEquals("Should be allocated.", 2, componentOrderLine.PickLines.Count);
			var componentPickLine1 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 8m);
			var componentPickLine2 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 2m);
			componentPickLine1.WZ_PickedDateTime = dateOffset1;
			Helper.Factory.Save();

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			var kitPickLine1 = kitPickLines.Single();
			var kitPickLine2 = kitPickLine1.Split(3m); // Split could happen by Packing or Cartonising
			Helper.Factory.Save();
			kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Manually split the kit pick line.", 2, kitPickLines.Length);

			var componentTransferLine1 = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, componentTransferLine1.WE_CurrentInventoryStatus);

			// putaway 8 wheel
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLines = transfer.Lines;

			var componentTransferLine = transferLines.Single(l => l.WE_OP == wheel.PK && l.WE_TransactionQuantity == 8m);
			AssertComponentLinePutaway(componentTransferLine, InventoryStatus.Codes.Staged, location1.PK);
			var componentPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(componentPickLineForTransfer, componentTransferLine.PK, receiveLine1.PK, ZGuid.Empty, 8m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var componentPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, componentTransferLine.PK)).Single();
			AssertPickLine(componentPickLineForOrder, componentOrderLine.PK, componentTransferLine.PK, receiveLine1.PK, 8m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK && l.WE_TransactionQuantity == 3m);
			var kitTransferLine2 = transferLines.Single(l => l.WE_OP == bike.PK && l.WE_TransactionQuantity == 1m);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location1.PK, dockdoorLocation.PK, 3m, GlbStaff.CurrentUser.GS_Code, dateOffset1);
			AssertKitLinePutaway(kitTransferLine2, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location1.PK, dockdoorLocation.PK, 1m, GlbStaff.CurrentUser.GS_Code, dateOffset1);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("2 Receive Lines now because putaway will split Lines, to make sure the DocketLine/Inventory Statuses are correct.", 2, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 4m);
			var kitReceiveLine2 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 1m);
			AssertKitReceiveLine(kitReceiveLine1, location1.PK, dateOffset1, 4m);
			AssertKitReceiveLineNotChanged(kitReceiveLine2, 1m);

			var kitPickLineForTransfers = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("2 Pick Lines attached to this Receive Line.", 2, kitPickLineForTransfers.Length);

			var kitPickLineForTransfer = kitPickLineForTransfers.Single(l => l.WZ_Units == 3m);
			AssertPickLine(kitPickLineForTransfer, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 3m, dateOffset1, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 3m, ZDateTimeOffset.Empty, string.Empty);

			var kitPickLineForTransfer2 = kitPickLineForTransfers.Single(l => l.WZ_Units == 1m);
			AssertPickLine(kitPickLineForTransfer2, kitTransferLine2.PK, kitReceiveLine1.PK, ZGuid.Empty, 1m, dateOffset1, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine2.PK)).Single();
			AssertPickLine(kitPickLineForOrder2, orderLine.PK, kitTransferLine2.PK, kitReceiveLine1.PK, 1m, ZDateTimeOffset.Empty, string.Empty);

			var unpickedPickLines = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine2.PK));
			AssertEquals("1 pick lines not picked yet.", 1, unpickedPickLines.Length);
			AssertPickLine(unpickedPickLines[0], orderLine.PK, kitReceiveLine2.PK, ZGuid.Empty, 1m, ZDateTimeOffset.Empty, string.Empty);
		}

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_SingleOrder_SingleComponent_MultipleUnpickedKitPickLines_OnlyOneLineCanBePicked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 6m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 4m, location1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			var componentOrderLine = orderLine.ChildComponentLines.First();
			AssertEquals("Should be allocated.", 2, componentOrderLine.PickLines.Count);
			var componentPickLine1 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 6m);
			var componentPickLine2 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 4m);
			componentPickLine1.WZ_PickedDateTime = dateOffset1;
			Helper.Factory.Save();

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			var kitPickLine1 = kitPickLines.Single();
			var kitPickLine2 = kitPickLine1.Split(1m); // Split could happen by Packing or Cartonising
			var kitPickLine3 = kitPickLine1.Split(1m); // Split could happen by Packing or Cartonising
			var kitPickLine4 = kitPickLine1.Split(1m); // Split could happen by Packing or Cartonising
			Helper.Factory.Save();
			kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Manually split the kit pick line.", 4, kitPickLines.Length);

			var componentTransferLine1 = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, componentTransferLine1.WE_CurrentInventoryStatus);

			// putaway 4 wheel
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLines = transfer.Lines;

			var componentTransferLine = transferLines.Single(l => l.WE_OP == wheel.PK && l.WE_TransactionQuantity == 6m);
			AssertComponentLinePutaway(componentTransferLine, InventoryStatus.Codes.Staged, location1.PK);
			var componentPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(componentPickLineForTransfer, componentTransferLine.PK, receiveLine1.PK, ZGuid.Empty, 6m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var componentPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, componentTransferLine.PK)).Single();
			AssertPickLine(componentPickLineForOrder, componentOrderLine.PK, componentTransferLine.PK, receiveLine1.PK, 6m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK && l.WE_TransactionQuantity == 2m);
			var kitTransferLine2 = transferLines.Single(l => l.WE_OP == bike.PK && l.WE_TransactionQuantity == 1m);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location1.PK, dockdoorLocation.PK, 2m, GlbStaff.CurrentUser.GS_Code, dateOffset1);
			AssertKitLinePutaway(kitTransferLine2, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location1.PK, dockdoorLocation.PK, 1m, GlbStaff.CurrentUser.GS_Code, dateOffset1);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("2 Receive Lines now because putaway will split Lines, to make sure the DocketLine/Inventory Statuses are correct.", 2, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			var kitReceiveLine2 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			AssertKitReceiveLine(kitReceiveLine1, location1.PK, dateOffset1, 3m);
			AssertKitReceiveLineNotChanged(kitReceiveLine2, 2m);

			var kitPickLineForTransfers = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("2 Pick Lines attached to this Receive Line.", 2, kitPickLineForTransfers.Length);

			var kitPickLineForTransfer = kitPickLineForTransfers.Single(l => l.WZ_Units == 2m);
			AssertPickLine(kitPickLineForTransfer, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 2m, dateOffset1, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 2m, ZDateTimeOffset.Empty, string.Empty);

			var kitPickLineForTransfer2 = kitPickLineForTransfers.Single(l => l.WZ_Units == 1m);
			AssertPickLine(kitPickLineForTransfer2, kitTransferLine2.PK, kitReceiveLine1.PK, ZGuid.Empty, 1m, dateOffset1, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine2.PK)).Single();
			AssertPickLine(kitPickLineForOrder2, orderLine.PK, kitTransferLine2.PK, kitReceiveLine1.PK, 1m, ZDateTimeOffset.Empty, string.Empty);

			var unpickedPickLines = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine2.PK));
			AssertEquals("2 pick lines not picked yet.", 2, unpickedPickLines.Length);
			AssertPickLine(unpickedPickLines[0], orderLine.PK, kitReceiveLine2.PK, ZGuid.Empty, 1m, ZDateTimeOffset.Empty, string.Empty);
			AssertPickLine(unpickedPickLines[1], orderLine.PK, kitReceiveLine2.PK, ZGuid.Empty, 1m, ZDateTimeOffset.Empty, string.Empty);
		}

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_SingleOrder_SingleComponent_PutawayWhileSomeAreInTransitByAnotherUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var date2 = new ZDateTime(2024, 4, 1, 8, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			var dateOffset2 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date2);
			var staffA = Helper.CreateGlbStaff("AAA", "AAA");
			var staffB = Helper.CreateGlbStaff("BBB", "BBB");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 7m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 13m, location2);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 10m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			var wheelOrderLine = orderLine.ChildComponentLines.Single();
			var wheelPickLine1 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 7m);
			var wheelPickLine2 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 13m);
			wheelPickLine1.WZ_PickedDateTime = dateOffset1;
			wheelPickLine1.WZ_GS_NKAssignedTo = staffA.GS_Code;
			wheelPickLine2.WZ_PickedDateTime = dateOffset2;
			wheelPickLine2.WZ_GS_NKAssignedTo = staffB.GS_Code;
			Helper.Factory.Save();

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);

			var wheelTransferLine1 = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_TransactionQuantity == 7m);
			var wheelTransferLine2 = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_TransactionQuantity == 13m);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, wheelTransferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, wheelTransferLine2.WE_CurrentInventoryStatus);

			// putaway 7 wheels by AAA
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staffA.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staffA.StaffPlainTextPassword);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("New Kit Receive Line created by split.", 2, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			var kitReceiveLine2 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 7m);
			AssertKitReceiveLine(kitReceiveLine1, location1.PK, dateOffset1, 3m);
			AssertKitReceiveLineNotChanged(kitReceiveLine2, 7m);

			var transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLines = transfer.Lines;
			AssertEquals("Kit Transfer Line was created.", 3, transferLines.Count);
			wheelTransferLine1 = newFactory.Load<WhsTransferLine>(wheelTransferLine1.PK);
			wheelTransferLine2 = newFactory.Load<WhsTransferLine>(wheelTransferLine2.PK);
			AssertComponentLinePutaway(wheelTransferLine1, InventoryStatus.Codes.Staged, location1.PK, stockOnHand: 1m);
			AssertLineNotPutaway(wheelTransferLine2);
			var wheelPickLineForTransfer1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer1, wheelTransferLine1.PK, receiveLine1.PK, ZGuid.Empty, 7m, dateOffset1, staffA.GS_Code);

			var wheelPickLinesForOrder1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine1.PK));
			AssertEquals("The Pick Line was split to make 6m picked and 1m not picked.", 2, wheelPickLinesForOrder1.Length);
			var wheelPickLineForOrder1_1 = wheelPickLinesForOrder1.Single(l => l.WZ_Units == 6m);
			var wheelPickLineForOrder1_2 = wheelPickLinesForOrder1.Single(l => l.WZ_Units == 1m);
			AssertPickLine(wheelPickLineForOrder1_1, wheelOrderLine.PK, wheelTransferLine1.PK, receiveLine1.PK, 6m, ZDateTimeOffset.Now, staffA.GS_Code);
			AssertPickLine(wheelPickLineForOrder1_2, wheelOrderLine.PK, wheelTransferLine1.PK, receiveLine1.PK, 1m, ZDateTimeOffset.Empty, ZString.Empty);

			var wheelPickLineForTransfer2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine2.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer2, wheelTransferLine2.PK, receiveLine2.PK, ZGuid.Empty, 13m, dateOffset2, staffB.GS_Code);
			var wheelPickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine2.PK)).Single();
			AssertPickLine(wheelPickLineForOrder2, wheelOrderLine.PK, wheelTransferLine2.PK, receiveLine2.PK, 13m, ZDateTimeOffset.Empty, ZString.Empty);

			// putaway 13 wheels by BBB
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = staffB.GS_LoginName;
			webService2.SecurityHeader.Password = GetEncryptedText(staffB.StaffPlainTextPassword);
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			transferLines = transfer.Lines;
			AssertEquals("2 Component Lines and 2 Kit Lines.", 4, transferLines.Count);
			wheelTransferLine1 = newFactory.Load<WhsTransferLine>(wheelTransferLine1.PK);
			wheelTransferLine2 = newFactory.Load<WhsTransferLine>(wheelTransferLine2.PK);
			AssertComponentLinePutaway(wheelTransferLine1, InventoryStatus.Codes.Staged, location1.PK);
			AssertComponentLinePutaway(wheelTransferLine2, InventoryStatus.Codes.Staged, location2.PK);

			wheelPickLineForTransfer1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer1, wheelTransferLine1.PK, receiveLine1.PK, ZGuid.Empty, 7m, dateOffset1, staffA.GS_Code);
			wheelPickLinesForOrder1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine1.PK));
			AssertEquals("The Pick Line was split to make 6m picked and 1m not picked.", 2, wheelPickLinesForOrder1.Length);
			wheelPickLineForOrder1_1 = wheelPickLinesForOrder1.Single(l => l.WZ_Units == 6m);
			wheelPickLineForOrder1_2 = wheelPickLinesForOrder1.Single(l => l.WZ_Units == 1m);
			AssertPickLine(wheelPickLineForOrder1_1, wheelOrderLine.PK, wheelTransferLine1.PK, receiveLine1.PK, 6m, ZDateTimeOffset.Now, staffA.GS_Code);
			AssertPickLine(wheelPickLineForOrder1_2, wheelOrderLine.PK, wheelTransferLine1.PK, receiveLine1.PK, 1m, ZDateTimeOffset.Now, staffB.GS_Code);

			wheelPickLineForTransfer2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine2.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer2, wheelTransferLine2.PK, receiveLine2.PK, ZGuid.Empty, 13m, dateOffset2, staffB.GS_Code);
			wheelPickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine2.PK)).Single();
			AssertPickLine(wheelPickLineForOrder2, wheelOrderLine.PK, wheelTransferLine2.PK, receiveLine2.PK, 13m, ZDateTimeOffset.Now, staffB.GS_Code);

			var kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK && l.WE_TransactionQuantity == 3m);
			var kitTransferLine2 = transferLines.Single(l => l.WE_OP == bike.PK && l.WE_TransactionQuantity == 7m);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location1.PK, dockdoorLocation.PK, 3m, staffA.GS_Code, dateOffset1);
			AssertKitLinePutaway(kitTransferLine2, InventoryStatus.Codes.Staged, kitReceiveLine2.PK, location2.PK, dockdoorLocation.PK, 7m, staffB.GS_Code, dateOffset2);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals(2, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			kitReceiveLine2 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 7m);
			AssertKitReceiveLine(kitReceiveLine1, location1.PK, dateOffset1, 3m, putawayBy: "AAA");
			AssertKitReceiveLine(kitReceiveLine2, location2.PK, dateOffset2, 7m, putawayBy: "BBB");

			var kitPickLineForTransfer1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK)).Single();
			AssertPickLine(kitPickLineForTransfer1, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 3m, dateOffset1, staffA.GS_Code);

			var kitPickLineForOrder1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder1, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 3m, ZDateTimeOffset.Empty, string.Empty);

			var kitPickLineForTransfer2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine2.PK)).Single();
			AssertPickLine(kitPickLineForTransfer2, kitTransferLine2.PK, kitReceiveLine2.PK, ZGuid.Empty, 7m, dateOffset2, staffB.GS_Code);

			var kitPickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine2.PK)).Single();
			AssertPickLine(kitPickLineForOrder2, orderLine.PK, kitTransferLine2.PK, kitReceiveLine2.PK, 7m, ZDateTimeOffset.Empty, string.Empty);
		}

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_SingleOrder_TwoComponents_PutawayOneThenAnother()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var date2 = new ZDateTime(2024, 4, 1, 8, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			var dateOffset2 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date2);
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, frame, 5m, location2);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine = frameOrderLine.PickLines.Single();
			wheelPickLine.WZ_PickedDateTime = dateOffset1;
			Helper.Factory.Save();

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);

			var wheelTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, wheelTransferLine.WE_CurrentInventoryStatus);

			// putaway 10 wheels
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLines = transfer.Lines;
			AssertEquals("Kit Transfer Line was not created because there was not enough components.", 1, transferLines.Count);
			wheelTransferLine = newFactory.Load<WhsTransferLine>(wheelTransferLine.PK);
			AssertLinePutaway(wheelTransferLine);
			var wheelPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer, wheelTransferLine.PK, receiveLine1.PK, ZGuid.Empty, 10m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var wheelPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine.PK)).Single();
			AssertPickLine(wheelPickLineForOrder, wheelOrderLine.PK, wheelTransferLine.PK, receiveLine1.PK, 10m, ZDateTimeOffset.Empty, ZString.Empty);

			// putaway 5 frames
			framePickLine.WZ_PickedDateTime = dateOffset2;
			Helper.Factory.Save();
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			transferLines = transfer.Lines;
			AssertEquals("2 Component Lines and 1 Kit Line.", 3, transferLines.Count);
			wheelTransferLine = newFactory.Load<WhsTransferLine>(wheelTransferLine.PK);
			var frameTransferLine = transferLines.Single(l => l.WE_OP == frame.PK);
			AssertComponentLinePutaway(wheelTransferLine, InventoryStatus.Codes.Staged, dockdoorLocation.PK);
			AssertComponentLinePutaway(frameTransferLine, InventoryStatus.Codes.Staged, location2.PK);

			wheelPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer, wheelTransferLine.PK, receiveLine1.PK, ZGuid.Empty, 10m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			wheelPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine.PK)).Single();
			AssertPickLine(wheelPickLineForOrder, wheelOrderLine.PK, wheelTransferLine.PK, receiveLine1.PK, 10m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var framePickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine2.PK)).Single();
			AssertPickLine(framePickLineForTransfer, frameTransferLine.PK, receiveLine2.PK, ZGuid.Empty, 5m, dateOffset2, GlbStaff.CurrentUser.GS_Code);
			var framePickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, frameTransferLine.PK)).Single();
			AssertPickLine(framePickLineForOrder, frameOrderLine.PK, frameTransferLine.PK, receiveLine2.PK, 5m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location2.PK, dockdoorLocation.PK, 5m, GlbStaff.CurrentUser.GS_Code, dateOffset2);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("No need to split Kit Receive Line as all Components has been putaway-ed.", 1, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single();
			AssertKitReceiveLine(kitReceiveLine1, location2.PK, dateOffset2, 5m);

			var kitPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK)).Single();
			AssertPickLine(kitPickLineForTransfer, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 5m, dateOffset2, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 5m, ZDateTimeOffset.Empty, string.Empty);
		}

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_SingleOrder_TwoComponents_SplitStagedPickLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var date2 = new ZDateTime(2024, 4, 1, 8, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			var dateOffset2 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date2);
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, frame, 4m, location2);
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive1, frame, 1m, location2);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine1 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 4m);
			var framePickLine2 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 1m);
			wheelPickLine.WZ_PickedDateTime = dateOffset1;
			Helper.Factory.Save();

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);

			var wheelTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, wheelTransferLine.WE_CurrentInventoryStatus);

			// putaway 10 wheels
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLines = transfer.Lines;
			AssertEquals("Kit Transfer Line was not created because there was not enough components.", 1, transferLines.Count);
			wheelTransferLine = newFactory.Load<WhsTransferLine>(wheelTransferLine.PK);
			AssertLinePutaway(wheelTransferLine);
			var wheelPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer, wheelTransferLine.PK, receiveLine1.PK, ZGuid.Empty, 10m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var wheelPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine.PK)).Single();
			AssertPickLine(wheelPickLineForOrder, wheelOrderLine.PK, wheelTransferLine.PK, receiveLine1.PK, 10m, ZDateTimeOffset.Empty, ZString.Empty);

			// putaway 4 frames
			framePickLine1.WZ_PickedDateTime = dateOffset2;
			Helper.Factory.Save();
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			transferLines = transfer.Lines;
			AssertEquals("2 Component Lines and 1 Kit Line.", 3, transferLines.Count);
			wheelTransferLine = newFactory.Load<WhsTransferLine>(wheelTransferLine.PK);
			var frameTransferLine = transferLines.Single(l => l.WE_OP == frame.PK);
			AssertComponentLinePutaway(wheelTransferLine, InventoryStatus.Codes.Staged, dockdoorLocation.PK, stockOnHand: 2m);
			AssertComponentLinePutaway(frameTransferLine, InventoryStatus.Codes.Staged, location2.PK);

			var wheelPickLineForTransfer1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer1, wheelTransferLine.PK, receiveLine1.PK, ZGuid.Empty, 10m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var wheelPickLinesForOrder1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine.PK));
			AssertEquals("This Pick Line has been split to 2 lines, one Picked and one not.", 2, wheelPickLinesForOrder1.Length);
			var wheelPickLineForOrder1 = wheelPickLinesForOrder1.Single(l => l.WZ_Units == 8m);
			AssertPickLine(wheelPickLineForOrder1, wheelOrderLine.PK, wheelTransferLine.PK, receiveLine1.PK, 8m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);
			var wheelPickLineForOrder2 = wheelPickLinesForOrder1.Single(l => l.WZ_Units == 2m);
			AssertPickLine(wheelPickLineForOrder2, wheelOrderLine.PK, wheelTransferLine.PK, receiveLine1.PK, 2m, ZDateTimeOffset.Empty, string.Empty);

			var framePickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine2.PK)).Single();
			AssertPickLine(framePickLineForTransfer, frameTransferLine.PK, receiveLine2.PK, ZGuid.Empty, 4m, dateOffset2, GlbStaff.CurrentUser.GS_Code);
			var framePickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, frameTransferLine.PK)).Single();
			AssertPickLine(framePickLineForOrder, frameOrderLine.PK, frameTransferLine.PK, receiveLine2.PK, 4m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location2.PK, dockdoorLocation.PK, 4m, GlbStaff.CurrentUser.GS_Code, dateOffset2);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("Kit Receive Line has been split.", 2, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 4m);
			var kitReceiveLine2 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 1m);
			AssertKitReceiveLine(kitReceiveLine1, location2.PK, dateOffset2, 4m);
			AssertKitReceiveLineNotChanged(kitReceiveLine2, 1m);

			var kitPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK)).Single();
			AssertPickLine(kitPickLineForTransfer, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 4m, dateOffset2, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 4m, ZDateTimeOffset.Empty, string.Empty);
		}

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_SingleOrder_TwoComponents_PutawayByDifferentUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var date2 = new ZDateTime(2024, 4, 1, 8, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			var dateOffset2 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date2);
			var staffA = Helper.CreateGlbStaff("AAA", "AAA");
			var staffB = Helper.CreateGlbStaff("BBB", "BBB");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, frame, 5m, location2);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine = frameOrderLine.PickLines.Single();
			wheelPickLine.WZ_PickedDateTime = dateOffset1;
			wheelPickLine.WZ_GS_NKAssignedTo = staffA.GS_Code;
			framePickLine.WZ_PickedDateTime = dateOffset2;
			framePickLine.WZ_GS_NKAssignedTo = staffB.GS_Code;
			Helper.Factory.Save();

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);

			var wheelTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_OP == wheel.PK);
			var frameTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, wheelTransferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, frameTransferLine.WE_CurrentInventoryStatus);

			// putaway 10 wheels by AAA
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staffA.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staffA.StaffPlainTextPassword);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLines = transfer.Lines;
			AssertEquals("Kit Transfer Line was not created because there was not enough components, frames are picked by AAA and haven't been put yet.", 2, transferLines.Count);
			wheelTransferLine = newFactory.Load<WhsTransferLine>(wheelTransferLine.PK);
			frameTransferLine = newFactory.Load<WhsTransferLine>(frameTransferLine.PK);
			AssertLinePutaway(wheelTransferLine);
			AssertLineNotPutaway(frameTransferLine);
			var wheelPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer, wheelTransferLine.PK, receiveLine1.PK, ZGuid.Empty, 10m, dateOffset1, staffA.GS_Code);
			var wheelPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine.PK)).Single();
			AssertPickLine(wheelPickLineForOrder, wheelOrderLine.PK, wheelTransferLine.PK, receiveLine1.PK, 10m, ZDateTimeOffset.Empty, ZString.Empty);

			var framePickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine2.PK)).Single();
			AssertPickLine(framePickLineForTransfer, frameTransferLine.PK, receiveLine2.PK, ZGuid.Empty, 5m, dateOffset2, staffB.GS_Code);
			var framePickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, frameTransferLine.PK)).Single();
			AssertPickLine(framePickLineForOrder, frameOrderLine.PK, frameTransferLine.PK, receiveLine2.PK, 5m, ZDateTimeOffset.Empty, string.Empty);

			// putaway 5 frames by BBB
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = staffB.GS_LoginName;
			webService2.SecurityHeader.Password = GetEncryptedText(staffB.StaffPlainTextPassword);
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			transferLines = transfer.Lines;
			AssertEquals("2 Component Lines and 1 Kit Line.", 3, transferLines.Count);
			wheelTransferLine = newFactory.Load<WhsTransferLine>(wheelTransferLine.PK);
			frameTransferLine = newFactory.Load<WhsTransferLine>(frameTransferLine.PK);
			AssertComponentLinePutaway(wheelTransferLine, InventoryStatus.Codes.Staged, dockdoorLocation.PK);
			AssertComponentLinePutaway(frameTransferLine, InventoryStatus.Codes.Staged, location2.PK);

			wheelPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer, wheelTransferLine.PK, receiveLine1.PK, ZGuid.Empty, 10m, dateOffset1, staffA.GS_Code);
			wheelPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine.PK)).Single();
			AssertPickLine(wheelPickLineForOrder, wheelOrderLine.PK, wheelTransferLine.PK, receiveLine1.PK, 10m, ZDateTimeOffset.Now, staffB.GS_Code);

			framePickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine2.PK)).Single();
			AssertPickLine(framePickLineForTransfer, frameTransferLine.PK, receiveLine2.PK, ZGuid.Empty, 5m, dateOffset2, staffB.GS_Code);
			framePickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, frameTransferLine.PK)).Single();
			AssertPickLine(framePickLineForOrder, frameOrderLine.PK, frameTransferLine.PK, receiveLine2.PK, 5m, ZDateTimeOffset.Now, staffB.GS_Code);

			var kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location2.PK, dockdoorLocation.PK, 5m, staffB.GS_Code, dateOffset2);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("No need to split Kit Receive Line as all Components has been putaway-ed.", 1, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single();
			AssertKitReceiveLine(kitReceiveLine1, location2.PK, dateOffset2, 5m);

			var kitPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK)).Single();
			AssertPickLine(kitPickLineForTransfer, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 5m, dateOffset2, staffB.GS_Code);

			var kitPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 5m, ZDateTimeOffset.Empty, string.Empty);
		}

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_SingleOrder_TwoComponents_PutawayBothAtTheSameTime()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var date2 = new ZDateTime(2024, 4, 1, 8, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			var dateOffset2 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date2);
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, frame, 5m, location2);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var framePickLine = frameOrderLine.PickLines.Single();
			wheelPickLine.WZ_PickedDateTime = dateOffset1;
			framePickLine.WZ_PickedDateTime = dateOffset2;
			Helper.Factory.Save();

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);

			var wheelTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_OP == wheel.PK);
			var frameTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, wheelTransferLine.WE_CurrentInventoryStatus);

			Helper.Factory.Save();
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLines = transfer.Lines;
			AssertEquals("2 Component Lines and 1 Kit Line.", 3, transferLines.Count);
			wheelTransferLine = newFactory.Load<WhsTransferLine>(wheelTransferLine.PK);
			frameTransferLine = newFactory.Load<WhsTransferLine>(frameTransferLine.PK);
			var kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK);
			AssertComponentLinePutaway(wheelTransferLine, InventoryStatus.Codes.Staged, location2.PK);
			AssertComponentLinePutaway(frameTransferLine, InventoryStatus.Codes.Staged, location2.PK);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location2.PK, dockdoorLocation.PK, 5m, GlbStaff.CurrentUser.GS_Code, dateOffset2);

			var wheelPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer, wheelTransferLine.PK, receiveLine1.PK, ZGuid.Empty, 10m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var wheelPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine.PK)).Single();
			AssertPickLine(wheelPickLineForOrder, wheelOrderLine.PK, wheelTransferLine.PK, receiveLine1.PK, 10m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var framePickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine2.PK)).Single();
			AssertPickLine(framePickLineForTransfer, frameTransferLine.PK, receiveLine2.PK, ZGuid.Empty, 5m, dateOffset2, GlbStaff.CurrentUser.GS_Code);
			var framePickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, frameTransferLine.PK)).Single();
			AssertPickLine(framePickLineForOrder, frameOrderLine.PK, frameTransferLine.PK, receiveLine2.PK, 5m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("No need to split Kit Receive Line as all Components has been putaway-ed.", 1, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single();
			AssertKitReceiveLine(kitReceiveLine1, location2.PK, dateOffset2, 5m);

			var kitPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK)).Single();
			AssertPickLine(kitPickLineForTransfer, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 5m, dateOffset2, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 5m, ZDateTimeOffset.Empty, string.Empty);
		}

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_LastPickedLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var date2 = new ZDateTime(2024, 4, 1, 8, 0, 0);
			var date3 = new ZDateTime(2024, 4, 1, 9, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			var dateOffset2 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date2);
			var dateOffset3 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date3);
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 1m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 3m, location2);
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 6m, location3);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			var componentOrderLine = orderLine.ChildComponentLines.First();
			AssertEquals("Should be allocated.", 3, componentOrderLine.PickLines.Count);
			var componentPickLine1 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 1m);
			var componentPickLine2 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 3m);
			var componentPickLine3 = componentOrderLine.PickLines.Single(l => l.WZ_Units == 6m);
			componentPickLine1.WZ_PickedDateTime = dateOffset2;
			componentPickLine2.WZ_PickedDateTime = dateOffset3;
			componentPickLine3.WZ_PickedDateTime = dateOffset1;
			Helper.Factory.Save();

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);

			var componentTransferLine1 = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_TransactionQuantity == 1m);
			var componentTransferLine2 = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_TransactionQuantity == 3m);
			var componentTransferLine3 = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_TransactionQuantity == 6m);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, componentTransferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, componentTransferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, componentTransferLine3.WE_CurrentInventoryStatus);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLines = transfer.Lines;
			AssertEquals("4 Lines, 3 for Component and 1 for Kit.", 4, transferLines.Count);

			componentTransferLine1 = newFactory.Load<WhsTransferLine>(componentTransferLine1.PK);
			AssertComponentLinePutaway(componentTransferLine1, InventoryStatus.Codes.Staged, location2.PK);
			var componentPickLineForTransfer1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(componentPickLineForTransfer1, componentTransferLine1.PK, receiveLine1.PK, ZGuid.Empty, 1m, dateOffset2, GlbStaff.CurrentUser.GS_Code);
			var componentPickLineForOrder1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, componentTransferLine1.PK)).Single();
			AssertPickLine(componentPickLineForOrder1, componentOrderLine.PK, componentTransferLine1.PK, receiveLine1.PK, 1m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			componentTransferLine2 = (WhsTransferLine)transferLines.Single(l => l.WE_OP == wheel.PK && l.WE_TransactionQuantity == 3m);
			AssertComponentLinePutaway(componentTransferLine2, InventoryStatus.Codes.Staged, location2.PK);
			var componentPickLineForTransfer2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine2.PK)).Single();
			AssertPickLine(componentPickLineForTransfer2, componentTransferLine2.PK, receiveLine2.PK, ZGuid.Empty, 3m, dateOffset3, GlbStaff.CurrentUser.GS_Code);
			var componentPickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, componentTransferLine2.PK)).Single();
			AssertPickLine(componentPickLineForOrder2, componentOrderLine.PK, componentTransferLine2.PK, receiveLine2.PK, 3m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			componentTransferLine3 = (WhsTransferLine)transferLines.Single(l => l.WE_OP == wheel.PK && l.WE_TransactionQuantity == 6m);
			AssertComponentLinePutaway(componentTransferLine3, InventoryStatus.Codes.Staged, location2.PK);
			var componentPickLineForTransfer3 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine3.PK)).Single();
			AssertPickLine(componentPickLineForTransfer3, componentTransferLine3.PK, receiveLine3.PK, ZGuid.Empty, 6m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var componentPickLineForOrder3 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, componentTransferLine3.PK)).Single();
			AssertPickLine(componentPickLineForOrder3, componentOrderLine.PK, componentTransferLine3.PK, receiveLine3.PK, 6m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location2.PK, dockdoorLocation.PK, 5m, GlbStaff.CurrentUser.GS_Code, dateOffset3);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("All Putaway-ed in a single run, no split needed.", 1, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single();
			AssertKitReceiveLine(kitReceiveLine1, location2.PK, dateOffset3, 5m);

			var kitPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK)).Single();
			AssertPickLine(kitPickLineForTransfer, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 5m, dateOffset3, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 5m, ZDateTimeOffset.Empty, string.Empty);
		}

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			var org2 = Helper.CreateClient("ORG2");
			var date1 = new ZDateTime(2024, 4, 1, 6, 0, 0);
			var date2 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var date3 = new ZDateTime(2024, 4, 1, 8, 0, 0);
			var date4 = new ZDateTime(2024, 4, 1, 9, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			var dateOffset2 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date2);
			var dateOffset3 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date3);
			var dateOffset4 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date4);
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);
			Helper.CreateProductClientRelationShip(org2, bike);
			Helper.CreateProductClientRelationShip(org2, wheel);
			Helper.CreateProductClientRelationShip(org2, frame);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var location4 = data.Whs1.FindLocation("A-4");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, frame, 5m, location2);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive2, wheel, 8m, location3);
			var receiveLine4 = Helper.CreateWhsReceiveInventoryLine(receive2, frame, 4m, location4);
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 5m);

			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 4m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 2, orderLine2.ChildComponentLines.Count);
			var wheelOrderLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = orderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelOrderLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = orderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine1 = wheelOrderLine1.PickLines.Single();
			var framePickLine1 = frameOrderLine1.PickLines.Single();
			var wheelPickLine2 = wheelOrderLine2.PickLines.Single();
			var framePickLine2 = frameOrderLine2.PickLines.Single();
			wheelPickLine1.WZ_PickedDateTime = dateOffset1;
			framePickLine1.WZ_PickedDateTime = dateOffset2;
			wheelPickLine2.WZ_PickedDateTime = dateOffset3;
			framePickLine2.WZ_PickedDateTime = dateOffset4;
			Helper.Factory.Save();

			var kitReceives = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertEquals("2 Receives created for 2 Clients.", 2, kitReceives.Length);
			var kitReceive1 = kitReceives.Single(d => d.WD_OH_Client == data.Org1.PK);
			var kitReceive2 = kitReceives.Single(d => d.WD_OH_Client == org2.PK);
			var kitReceiveLine1 = (WhsReceiveLine)kitReceive1.Lines.Single();
			var kitReceiveLine2 = (WhsReceiveLine)kitReceive2.Lines.Single();

			var transfer1 = pick.Transfers.Single(d => d.WD_OH_Client == data.Org1.PK);
			var transfer2 = pick.Transfers.Single(d => d.WD_OH_Client == org2.PK);
			var wheelTransferLine1 = (WhsTransferLine)transfer1.Lines.Single(l => l.WE_OP == wheel.PK && l.WE_TransactionQuantity == 10m);
			var frameTransferLine1 = (WhsTransferLine)transfer1.Lines.Single(l => l.WE_OP == frame.PK && l.WE_TransactionQuantity == 5m);
			var wheelTransferLine2 = (WhsTransferLine)transfer2.Lines.Single(l => l.WE_OP == wheel.PK && l.WE_TransactionQuantity == 8m);
			var frameTransferLine2 = (WhsTransferLine)transfer2.Lines.Single(l => l.WE_OP == frame.PK && l.WE_TransactionQuantity == 4m);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, wheelTransferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, frameTransferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, wheelTransferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, frameTransferLine2.WE_CurrentInventoryStatus);

			Helper.Factory.Save();
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			transfer1 = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single(d => d.WD_OH_Client == data.Org1.PK);
			transfer2 = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single(d => d.WD_OH_Client == org2.PK);
			var transferLines1 = transfer1.Lines;
			var transferLines2 = transfer2.Lines;
			AssertEquals("2 Component Lines and 1 Kit Line.", 3, transferLines1.Count);
			AssertEquals("2 Component Lines and 1 Kit Line.", 3, transferLines2.Count);
			wheelTransferLine1 = newFactory.Load<WhsTransferLine>(wheelTransferLine1.PK);
			frameTransferLine1 = newFactory.Load<WhsTransferLine>(frameTransferLine1.PK);
			wheelTransferLine2 = newFactory.Load<WhsTransferLine>(wheelTransferLine2.PK);
			frameTransferLine2 = newFactory.Load<WhsTransferLine>(frameTransferLine2.PK);
			var kitTransferLine1 = transferLines1.Single(l => l.WE_OP == bike.PK && l.WE_TransactionQuantity == 5m);
			var kitTransferLine2 = transferLines2.Single(l => l.WE_OP == bike.PK && l.WE_TransactionQuantity == 4m);
			AssertComponentLinePutaway(wheelTransferLine1, InventoryStatus.Codes.Staged, location2.PK);
			AssertComponentLinePutaway(frameTransferLine1, InventoryStatus.Codes.Staged, location2.PK);
			AssertComponentLinePutaway(wheelTransferLine2, InventoryStatus.Codes.Staged, location4.PK);
			AssertComponentLinePutaway(frameTransferLine2, InventoryStatus.Codes.Staged, location4.PK);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location2.PK, dockdoorLocation.PK, 5m, GlbStaff.CurrentUser.GS_Code, dateOffset2);
			AssertKitLinePutaway(kitTransferLine2, InventoryStatus.Codes.Staged, kitReceiveLine2.PK, location4.PK, dockdoorLocation.PK, 4m, GlbStaff.CurrentUser.GS_Code, dateOffset4);

			var wheelPickLineForTransfer1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer1, wheelTransferLine1.PK, receiveLine1.PK, ZGuid.Empty, 10m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var wheelPickLineForOrder1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine1.PK)).Single();
			AssertPickLine(wheelPickLineForOrder1, wheelOrderLine1.PK, wheelTransferLine1.PK, receiveLine1.PK, 10m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var framePickLineForTransfer1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine2.PK)).Single();
			AssertPickLine(framePickLineForTransfer1, frameTransferLine1.PK, receiveLine2.PK, ZGuid.Empty, 5m, dateOffset2, GlbStaff.CurrentUser.GS_Code);
			var framePickLineForOrder1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, frameTransferLine1.PK)).Single();
			AssertPickLine(framePickLineForOrder1, frameOrderLine1.PK, frameTransferLine1.PK, receiveLine2.PK, 5m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var wheelPickLineForTransfer2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine3.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer2, wheelTransferLine2.PK, receiveLine3.PK, ZGuid.Empty, 8m, dateOffset3, GlbStaff.CurrentUser.GS_Code);
			var wheelPickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine2.PK)).Single();
			AssertPickLine(wheelPickLineForOrder2, wheelOrderLine2.PK, wheelTransferLine2.PK, receiveLine3.PK, 8m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var framePickLineForTransfer2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine4.PK)).Single();
			AssertPickLine(framePickLineForTransfer2, frameTransferLine2.PK, receiveLine4.PK, ZGuid.Empty, 4m, dateOffset4, GlbStaff.CurrentUser.GS_Code);
			var framePickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, frameTransferLine2.PK)).Single();
			AssertPickLine(framePickLineForOrder2, frameOrderLine2.PK, frameTransferLine2.PK, receiveLine4.PK, 4m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			kitReceive1 = newFactory.Load<WhsReceive>(kitReceive1.PK);
			kitReceive2 = newFactory.Load<WhsReceive>(kitReceive2.PK);
			AssertEquals("No need to split Kit Receive Line as all Components has been putaway-ed.", 1, kitReceive1.Lines.Count);
			AssertEquals("No need to split Kit Receive Line as all Components has been putaway-ed.", 1, kitReceive2.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive1.Lines.Single();
			kitReceiveLine2 = (WhsReceiveLine)kitReceive2.Lines.Single();
			AssertKitReceiveLine(kitReceiveLine1, location2.PK, dateOffset2, 5m);
			AssertKitReceiveLine(kitReceiveLine2, location4.PK, dateOffset4, 4m);

			var kitPickLineForTransfer1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK)).Single();
			AssertPickLine(kitPickLineForTransfer1, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 5m, dateOffset2, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder1 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder1, orderLine1.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 5m, ZDateTimeOffset.Empty, string.Empty);

			var kitPickLineForTransfer2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine2.PK)).Single();
			AssertPickLine(kitPickLineForTransfer2, kitTransferLine2.PK, kitReceiveLine2.PK, ZGuid.Empty, 4m, dateOffset4, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder2 = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine2.PK)).Single();
			AssertPickLine(kitPickLineForOrder2, orderLine2.PK, kitTransferLine2.PK, kitReceiveLine2.PK, 4m, ZDateTimeOffset.Empty, string.Empty);
		}

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_KitLinesWerePacked()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var date2 = new ZDateTime(2024, 4, 1, 8, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			var dateOffset2 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date2);
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 4m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 6m, location1);
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive1, frame, 2m, location2);
			var receiveLine4 = Helper.CreateWhsReceiveInventoryLine(receive1, frame, 3m, location2);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine1 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 4m);
			var wheelPickLine2 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 6m);
			var framePickLine1 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 2m);
			var framePickLine2 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 3m);
			var kitPickLine = orderLine.PickLines.Single();
			wheelPickLine1.WZ_PickedDateTime = dateOffset1;
			framePickLine1.WZ_PickedDateTime = dateOffset2;

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg, kitPickLine);

			Helper.Factory.Save();

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);

			var wheelTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_OP == wheel.PK);
			var frameTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, wheelTransferLine.WE_CurrentInventoryStatus);

			Helper.Factory.Save();
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLines = transfer.Lines;
			AssertEquals("2 Component Lines and 1 Kit Line.", 3, transferLines.Count);
			wheelTransferLine = newFactory.Load<WhsTransferLine>(wheelTransferLine.PK);
			frameTransferLine = newFactory.Load<WhsTransferLine>(frameTransferLine.PK);
			var kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK);
			AssertComponentLinePutaway(wheelTransferLine, InventoryStatus.Codes.Staged, location2.PK);
			AssertComponentLinePutaway(frameTransferLine, InventoryStatus.Codes.Staged, location2.PK);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location2.PK, dockdoorLocation.PK, 2m, GlbStaff.CurrentUser.GS_Code, dateOffset2);

			var wheelPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer, wheelTransferLine.PK, receiveLine1.PK, ZGuid.Empty, 4m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var wheelPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine.PK)).Single();
			AssertPickLine(wheelPickLineForOrder, wheelOrderLine.PK, wheelTransferLine.PK, receiveLine1.PK, 4m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var framePickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine3.PK)).Single();
			AssertPickLine(framePickLineForTransfer, frameTransferLine.PK, receiveLine3.PK, ZGuid.Empty, 2m, dateOffset2, GlbStaff.CurrentUser.GS_Code);
			var framePickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, frameTransferLine.PK)).Single();
			AssertPickLine(framePickLineForOrder, frameOrderLine.PK, frameTransferLine.PK, receiveLine3.PK, 2m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("2 units of bikes were putawayed", 2, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			var kitReceiveLine2 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			AssertKitReceiveLine(kitReceiveLine1, location2.PK, dateOffset2, 2m);
			AssertKitReceiveLineNotChanged(kitReceiveLine2, 3m);

			var kitPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK)).Single();
			AssertPickLine(kitPickLineForTransfer, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 2m, dateOffset2, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 2m, ZDateTimeOffset.Empty, string.Empty);

			orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			var unpickedKitPickLine = orderLine.PickLines.Single(l => l.WZ_Units == 3m);
			var pickedKitPickLine = orderLine.PickLines.Single(l => l.WZ_Units == 2m);
			pkg = newFactory.Load<PkgPackage>(pkg.PK);
			AssertEquals(2, pkg.PackedItemDivots.Count);
			AssertEquals(2m, pkg.PackedItemDivots.Single(ki => ki.KI_ParentID == pickedKitPickLine.PK).KI_PackedQty);
			AssertEquals(3m, pkg.PackedItemDivots.Single(ki => ki.KI_ParentID == unpickedKitPickLine.PK).KI_PackedQty);
		}

		[TestDate(2024, 4, 8)]
		public void TestPutawayStockInDockDoorOrPackingStation_ComponentsOfPickByBOM_MultipleKitLines_OneWasPacked()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var date2 = new ZDateTime(2024, 4, 1, 8, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			var dateOffset2 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date2);
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 4m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 6m, location1);
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive1, frame, 2m, location2);
			var receiveLine4 = Helper.CreateWhsReceiveInventoryLine(receive1, frame, 3m, location2);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine1 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 4m);
			var wheelPickLine2 = wheelOrderLine.PickLines.Single(l => l.WZ_Units == 6m);
			var framePickLine1 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 2m);
			var framePickLine2 = frameOrderLine.PickLines.Single(l => l.WZ_Units == 3m);
			var kitPickLine1 = orderLine.PickLines.Single();
			var kitPickLine2 = kitPickLine1.Split(3m);
			wheelPickLine1.WZ_PickedDateTime = dateOffset1;
			framePickLine1.WZ_PickedDateTime = dateOffset2;

			var releaseLine = orderLine.ReleaseLines[0];
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg, kitPickLine1);

			Helper.Factory.Save();

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: 2 kit pick lines.", 2, kitPickLines.Length);

			var wheelTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_OP == wheel.PK);
			var frameTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, wheelTransferLine.WE_CurrentInventoryStatus);

			Helper.Factory.Save();
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response2.Error);
				AssertNull("Should be no error.", response2.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLines = transfer.Lines;
			AssertEquals("2 Component Lines and 1 Kit Line.", 3, transferLines.Count);
			wheelTransferLine = newFactory.Load<WhsTransferLine>(wheelTransferLine.PK);
			frameTransferLine = newFactory.Load<WhsTransferLine>(frameTransferLine.PK);
			var kitTransferLine1 = transferLines.Single(l => l.WE_OP == bike.PK);
			AssertComponentLinePutaway(wheelTransferLine, InventoryStatus.Codes.Staged, location2.PK);
			AssertComponentLinePutaway(frameTransferLine, InventoryStatus.Codes.Staged, location2.PK);
			AssertKitLinePutaway(kitTransferLine1, InventoryStatus.Codes.Staged, kitReceiveLine1.PK, location2.PK, dockdoorLocation.PK, 2m, GlbStaff.CurrentUser.GS_Code, dateOffset2);

			var wheelPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine1.PK)).Single();
			AssertPickLine(wheelPickLineForTransfer, wheelTransferLine.PK, receiveLine1.PK, ZGuid.Empty, 4m, dateOffset1, GlbStaff.CurrentUser.GS_Code);
			var wheelPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, wheelTransferLine.PK)).Single();
			AssertPickLine(wheelPickLineForOrder, wheelOrderLine.PK, wheelTransferLine.PK, receiveLine1.PK, 4m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			var framePickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine3.PK)).Single();
			AssertPickLine(framePickLineForTransfer, frameTransferLine.PK, receiveLine3.PK, ZGuid.Empty, 2m, dateOffset2, GlbStaff.CurrentUser.GS_Code);
			var framePickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, frameTransferLine.PK)).Single();
			AssertPickLine(framePickLineForOrder, frameOrderLine.PK, frameTransferLine.PK, receiveLine3.PK, 2m, ZDateTimeOffset.Now, GlbStaff.CurrentUser.GS_Code);

			kitReceive = newFactory.Load<WhsReceive>(kitReceive.PK);
			AssertEquals("2 units of bikes were putawayed", 2, kitReceive.Lines.Count);
			kitReceiveLine1 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			var kitReceiveLine2 = (WhsReceiveLine)kitReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			AssertKitReceiveLine(kitReceiveLine1, location2.PK, dateOffset2, 2m);
			AssertKitReceiveLineNotChanged(kitReceiveLine2, 3m);

			var kitPickLineForTransfer = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK)).Single();
			AssertPickLine(kitPickLineForTransfer, kitTransferLine1.PK, kitReceiveLine1.PK, ZGuid.Empty, 2m, dateOffset2, GlbStaff.CurrentUser.GS_Code);

			var kitPickLineForOrder = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitTransferLine1.PK)).Single();
			AssertPickLine(kitPickLineForOrder, orderLine.PK, kitTransferLine1.PK, kitReceiveLine1.PK, 2m, ZDateTimeOffset.Empty, string.Empty);

			orderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			kitPickLines = orderLine.PickLines.ToArray();
			AssertEquals(2, kitPickLines.Length);
			var unpickedKitPickLine = orderLine.PickLines.Single(l => l.WZ_Units == 3m);
			var pickedKitPickLine = orderLine.PickLines.Single(l => l.WZ_Units == 2m);
			AssertEquals(true, pickedKitPickLine.IsPickedFromPutawayLocation);
			AssertEquals(false, unpickedKitPickLine.IsPickedFromPutawayLocation);
			pkg = newFactory.Load<PkgPackage>(pkg.PK);
			AssertEquals(1, pkg.PackedItemDivots.Count);
			var divot = pkg.PackedItemDivots.Single();
			AssertEquals(2m, divot.KI_PackedQty);
			AssertEquals(pickedKitPickLine.PK, divot.KI_ParentID);
			AssertEquals(kitTransferLine1.PK, pickedKitPickLine.WZ_WE_InventoryLine);
			AssertEquals(kitReceiveLine2.PK, unpickedKitPickLine.WZ_WE_InventoryLine);
		}

		#endregion

		#region TestReconcilePickLines_AfterComponentsBeenPutawayedToDockDoor

		public void TestReconcilePickLines_PickByBOM_KitsPickedThroughPutawayComponents_MoveDirectly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			Helper.Factory.Save();
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No enough stock", false, kitOrderLine2.ChildComponentLines.Count > 0);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine1.PickLines[0];
			var framePickLine = frameOrderLine1.PickLines[0];
			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceiveLine1 = createdReceive.Lines.Single();
			var createdReceiveLine1PK = createdReceiveLine1.PK;
			var bomLinks1 = createdReceiveLine1.BOMComponentLinks;
			AssertEquals(2, bomLinks1.Count());
			var bomLink1 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 20m);
			var bomLink2 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 10m);

			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService1.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response1 = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response1.Error);
				AssertNull("Should be no error.", response1.ErrorMessage);
			});

			AssertEquals("Precondition - Kit Pick Line created from Children.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Precondition - No Children, no Pick Lines.", 0, kitOrderLine2.PickLines.Count);
			var kitPickLine1 = kitOrderLine1.PickLines[0];
			var kitPickLine1PK = kitPickLine1.PK;
			AssertEquals("Precondition - Kit Pick Line created from Children.", 10m, kitPickLine1.WZ_Units);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 10m, kitOrderLine1.ReleaseLines[0].Quantity);

			var newFactory = new BusinessObjectFactory();
			kitOrderLine1 = newFactory.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory.Load<WhsOrderLine>(kitOrderLine2.PK);
			wheelOrderLine1 = newFactory.Load<WhsOrderLine>(wheelOrderLine1.PK);
			frameOrderLine1 = newFactory.Load<WhsOrderLine>(frameOrderLine1.PK);
			bomLink1 = newFactory.Load<WhsBOMInventoryPivot>(bomLink1.PK);
			bomLink2 = newFactory.Load<WhsBOMInventoryPivot>(bomLink2.PK);
			createdReceive = newFactory.Load<WhsReceive>(createdReceive.PK);
			AssertEquals("The Kit Receive Line was not changed because it was picked.", 1, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single();
			AssertEquals(10m, createdReceiveLine1.WE_TransactionQuantity);

			kitOrderLine1.ReleaseLines[0].Quantity = 0m;
			kitOrderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("The Component Lines were directly moved to kitOrderLine2.", 0, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2.", kitOrderLine2.PK, wheelOrderLine1.WE_WE_ParentDocketLine);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2.", kitOrderLine2.PK, frameOrderLine1.WE_WE_ParentDocketLine);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", false, bomLink1.IsDeleted);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", false, bomLink2.IsDeleted);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", wheelOrderLine1.PK, bomLink1.WIP_WE_ComponentLine);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", frameOrderLine1.PK, bomLink2.WIP_WE_ComponentLine);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", 20m, bomLink1.WIP_ComponentQuantity);
			AssertEquals("The Component Lines were directly moved to kitOrderLine2, so as the links.", 10m, bomLink2.WIP_ComponentQuantity);

			AssertEquals("The Kit Receive Line was directly moved to kitOrderLine2.", 1, createdReceive.Lines.Count);
			AssertEquals("The Kit Receive Line was directly moved to kitOrderLine2.", true, createdReceive.Lines.Single().PK == createdReceiveLine1PK);
			AssertEquals("The Kit Receive Line was directly moved to kitOrderLine2.", true, createdReceive.Lines.Single().WE_TransactionQuantity == 10m);
			AssertEquals("The Kit Receive Line was directly moved to kitOrderLine2.", true, createdReceive.Lines.Single().WE_ClientOrderedUnits == 10m);

			AssertEquals("The Kit Pick Line was directly moved to kitOrderLine2.", 0, kitOrderLine1.PickLines.Count);
			AssertEquals("The Kit Pick Line was directly moved to kitOrderLine2.", 1, kitOrderLine2.PickLines.Count);
			AssertEquals("The Kit Pick Line was directly moved to kitOrderLine2.", true, kitOrderLine2.PickLines.Single().PK == kitPickLine1PK);
			AssertEquals("The Kit Pick Line was directly moved to kitOrderLine2.", true, kitOrderLine2.PickLines.Single().WZ_Units == 10m);

			AssertNoExceptionThrown(newFactory.Save);
		}

		public void TestReconcilePickLines_PickByBOM_KitsPickedThroughPutawayComponents()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			Helper.Factory.Save();
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No enough stock", false, kitOrderLine2.ChildComponentLines.Count > 0);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine1.PickLines[0];
			var framePickLine = frameOrderLine1.PickLines[0];
			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceiveLine1 = createdReceive.Lines.Single();
			var createdReceiveLine1PK = createdReceiveLine1.PK;
			var bomLinks1 = createdReceiveLine1.BOMComponentLinks;
			AssertEquals(2, bomLinks1.Count());
			var bomLink1 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 20m);
			var bomLink2 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 10m);

			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService1.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response1 = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response1.Error);
				AssertNull("Should be no error.", response1.ErrorMessage);
			});

			AssertEquals("Precondition - Kit Pick Line created from Children.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Precondition - No Children, no Pick Lines.", 0, kitOrderLine2.PickLines.Count);
			var kitPickLine1 = kitOrderLine1.PickLines[0];
			var kitPickLine1PK = kitPickLine1.PK;
			AssertEquals("Precondition - Kit Pick Line created from Children.", 10m, kitPickLine1.WZ_Units);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 10m, kitOrderLine1.ReleaseLines[0].Quantity);

			var newFactory = new BusinessObjectFactory();
			kitOrderLine1 = newFactory.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory.Load<WhsOrderLine>(kitOrderLine2.PK);
			bomLink1 = newFactory.Load<WhsBOMInventoryPivot>(bomLink1.PK);
			bomLink2 = newFactory.Load<WhsBOMInventoryPivot>(bomLink2.PK);
			createdReceive = newFactory.Load<WhsReceive>(createdReceive.PK);
			AssertEquals("The Kit Receive Line was not changed because it was picked.", 1, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single();
			AssertEquals(10m, createdReceiveLine1.WE_TransactionQuantity);

			kitOrderLine1.ReleaseLines[0].Quantity = 1m;
			kitOrderLine2.ReleaseLines[0].Quantity = 9m;

			AssertEquals("Component Lines reconciled.", 2, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);

			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals("2 links for each kit order line.", 4, createdReceiveLine1.BOMComponentLinks.Count());
			bomLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			var bomLink3 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			var bomLink4 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals(2m, bomLink1.WIP_ComponentQuantity);
			AssertEquals(1m, bomLink2.WIP_ComponentQuantity);
			AssertEquals(18m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(9m, bomLink4.WIP_ComponentQuantity);

			AssertEquals("Kit Pick Line was split.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line was split.", 1, kitOrderLine2.PickLines.Count);
			AssertEquals("Kit Pick Line was split.", 1m, kitOrderLine1.PickLines.Single().WZ_Units);
			AssertEquals("Kit Pick Line was split.", 9m, kitOrderLine2.PickLines.Single().WZ_Units);

			AssertNoExceptionThrown(newFactory.Save);

			kitOrderLine1.ReleaseLines[0].Quantity = 0m;
			kitOrderLine2.ReleaseLines[0].Quantity = 10m;

			createdReceive = newFactory.Load<WhsReceive>(createdReceive.PK);
			AssertEquals("The Kit Receive Line was not changed because it was picked.", 1, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single();
			AssertEquals(10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals(10m, createdReceiveLine1.WE_ClientOrderedUnits);

			AssertEquals("Component Lines were deleted.", 0, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines exists.", 2, kitOrderLine2.ChildComponentLines.Count);

			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals("kitOrderLine1 has no Components, thus no Links.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			bomLink3 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink4 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals(20m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(10m, bomLink4.WIP_ComponentQuantity);

			AssertEquals("Kit Pick Line was reconciled.", 0, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line was reconciled.", 1, kitOrderLine2.PickLines.Count);
			AssertEquals("Kit Pick Line was reconciled.", 10m, kitOrderLine2.PickLines.Single().WZ_Units);

			AssertNoExceptionThrown(newFactory.Save);
		}

		public void TestReconcilePickLines_PickByBOM_KitsPickedThroughPutawayComponents_ThreeOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			Helper.Factory.Save();
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);
			var kitOrderLine3 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine3.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No enough stock", false, kitOrderLine2.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No enough stock", false, kitOrderLine3.ChildComponentLines.Count > 0);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine1.PickLines[0];
			var framePickLine = frameOrderLine1.PickLines[0];
			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceiveLine1 = createdReceive.Lines.Single();
			var createdReceiveLine1PK = createdReceiveLine1.PK;
			var bomLinks1 = createdReceiveLine1.BOMComponentLinks;
			AssertEquals(2, bomLinks1.Count());
			var bomLink1 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 20m);
			var bomLink2 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 10m);

			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService1.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response1 = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response1.Error);
				AssertNull("Should be no error.", response1.ErrorMessage);
			});

			AssertEquals("Precondition - Kit Pick Line created from Children.", 1, kitOrderLine1.PickLines.Count);
			AssertEquals("Precondition - No Children, no Pick Lines.", 0, kitOrderLine2.PickLines.Count);
			var kitPickLine1 = kitOrderLine1.PickLines[0];
			var kitPickLine1PK = kitPickLine1.PK;
			AssertEquals("Precondition - Kit Pick Line created from Children.", 10m, kitPickLine1.WZ_Units);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Precondition - Receive Line created.", 10m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine3.ReleaseLines.Count);
			AssertEquals("Precondition.", 10m, kitOrderLine1.ReleaseLines[0].Quantity);

			var newFactory = new BusinessObjectFactory();
			kitOrderLine1 = newFactory.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory.Load<WhsOrderLine>(kitOrderLine2.PK);
			kitOrderLine3 = newFactory.Load<WhsOrderLine>(kitOrderLine3.PK);
			wheelOrderLine1 = newFactory.Load<WhsOrderLine>(wheelOrderLine1.PK);
			frameOrderLine1 = newFactory.Load<WhsOrderLine>(frameOrderLine1.PK);
			bomLink1 = newFactory.Load<WhsBOMInventoryPivot>(bomLink1.PK);
			bomLink2 = newFactory.Load<WhsBOMInventoryPivot>(bomLink2.PK);
			createdReceive = newFactory.Load<WhsReceive>(createdReceive.PK);
			AssertEquals("The Kit Receive Line was not changed because it was picked.", 1, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single();
			AssertEquals(10m, createdReceiveLine1.WE_TransactionQuantity);

			kitOrderLine1.ReleaseLines[0].Quantity = 9m;
			kitOrderLine2.ReleaseLines[0].Quantity = 1m;

			AssertEquals("Component Lines reconciled.", 2, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);
			AssertEquals("Component Lines not reconciled.", 0, kitOrderLine3.ChildComponentLines.Count);

			AssertEquals("1 Picked Kit Receive Lines not changed.", 1, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 10m);

			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals(1, kitOrderLine1.PickLines.Count);
			AssertEquals(1, kitOrderLine2.PickLines.Count);
			AssertEquals(9m, kitOrderLine1.PickLines[0].WZ_Units);
			AssertEquals(1m, kitOrderLine2.PickLines[0].WZ_Units);

			kitPickLine1 = kitOrderLine1.PickLines.Single();
			var kitPickLine2 = kitOrderLine2.PickLines.Single();

			AssertEquals(4, createdReceiveLine1.BOMComponentLinks.Count());
			bomLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			var bomLink3 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			var bomLink4 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals(18m, bomLink1.WIP_ComponentQuantity);
			AssertEquals(9m, bomLink2.WIP_ComponentQuantity);
			AssertEquals(2m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(1m, bomLink4.WIP_ComponentQuantity);

			AssertNoExceptionThrown(newFactory.Save);

			kitOrderLine1.ReleaseLines[0].Quantity = 8m;
			kitOrderLine2.ReleaseLines[0].Quantity = 2m;

			AssertEquals("Component Lines reconciled.", 2, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);
			AssertEquals("Component Lines not reconciled.", 0, kitOrderLine3.ChildComponentLines.Count);

			AssertEquals("1 Picked Kit Receive Lines not changed.", 1, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 10m);
			AssertEquals(10m, createdReceiveLine1.WE_ClientOrderedUnits);

			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals(1, kitOrderLine1.PickLines.Count);
			AssertEquals(1, kitOrderLine2.PickLines.Count);
			AssertEquals(8m, kitOrderLine1.PickLines[0].WZ_Units);
			AssertEquals(2m, kitOrderLine2.PickLines[0].WZ_Units);

			kitPickLine1 = kitOrderLine1.PickLines.Single();
			kitPickLine2 = kitOrderLine2.PickLines.Single();

			AssertEquals(4, createdReceiveLine1.BOMComponentLinks.Count());
			bomLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			bomLink3 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink4 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals(16m, bomLink1.WIP_ComponentQuantity);
			AssertEquals(8m, bomLink2.WIP_ComponentQuantity);
			AssertEquals(4m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(2m, bomLink4.WIP_ComponentQuantity);

			AssertNoExceptionThrown(newFactory.Save);

			kitOrderLine1.ReleaseLines[0].Quantity = 8m;
			kitOrderLine2.ReleaseLines[0].Quantity = 1m;
			kitOrderLine3.ReleaseLines[0].Quantity = 1m;

			AssertEquals("Component Lines reconciled.", 2, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine3.ChildComponentLines.Count);

			AssertEquals("1 Picked Kit Receive Lines not changed.", 1, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 10m);
			AssertEquals(10m, createdReceiveLine1.WE_ClientOrderedUnits);

			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelOrderLine3 = kitOrderLine3.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine3 = kitOrderLine3.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals(1, kitOrderLine1.PickLines.Count);
			AssertEquals(1, kitOrderLine2.PickLines.Count);
			AssertEquals(1, kitOrderLine3.PickLines.Count);
			AssertEquals(8m, kitOrderLine1.PickLines[0].WZ_Units);
			AssertEquals(1m, kitOrderLine2.PickLines[0].WZ_Units);
			AssertEquals(1m, kitOrderLine3.PickLines[0].WZ_Units);

			kitPickLine1 = kitOrderLine1.PickLines.Single();
			kitPickLine2 = kitOrderLine2.PickLines.Single();
			var kitPickLine3 = kitOrderLine3.PickLines.Single();

			AssertEquals(6, createdReceiveLine1.BOMComponentLinks.Count());
			bomLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			bomLink3 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink4 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			var bomLink5 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine3.PK);
			var bomLink6 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine3.PK);
			AssertEquals(16m, bomLink1.WIP_ComponentQuantity);
			AssertEquals(8m, bomLink2.WIP_ComponentQuantity);
			AssertEquals(2m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(1m, bomLink4.WIP_ComponentQuantity);
			AssertEquals(2m, bomLink5.WIP_ComponentQuantity);
			AssertEquals(1m, bomLink6.WIP_ComponentQuantity);

			AssertNoExceptionThrown(newFactory.Save);

			kitOrderLine1.ReleaseLines[0].Quantity = 0m;
			kitOrderLine2.ReleaseLines[0].Quantity = 1m;
			kitOrderLine3.ReleaseLines[0].Quantity = 9m;

			AssertEquals("Component Lines reconciled.", 0, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine3.ChildComponentLines.Count);

			AssertEquals("1 Picked Kit Receive Lines not changed.", 1, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 10m);
			AssertEquals(10m, createdReceiveLine1.WE_ClientOrderedUnits);

			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			wheelOrderLine3 = kitOrderLine3.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine3 = kitOrderLine3.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals(0, kitOrderLine1.PickLines.Count);
			AssertEquals(1, kitOrderLine2.PickLines.Count);
			AssertEquals(1, kitOrderLine3.PickLines.Count);
			AssertEquals(1m, kitOrderLine2.PickLines[0].WZ_Units);
			AssertEquals(9m, kitOrderLine3.PickLines[0].WZ_Units);

			kitPickLine2 = kitOrderLine2.PickLines.Single();
			kitPickLine3 = kitOrderLine3.PickLines.Single();

			AssertEquals(4, createdReceiveLine1.BOMComponentLinks.Count());
			bomLink3 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink4 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			bomLink5 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine3.PK);
			bomLink6 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine3.PK);
			AssertEquals(2m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(1m, bomLink4.WIP_ComponentQuantity);
			AssertEquals(18m, bomLink5.WIP_ComponentQuantity);
			AssertEquals(9m, bomLink6.WIP_ComponentQuantity);

			AssertNoExceptionThrown(newFactory.Save);
		}

		public void TestReconcilePickLines_PickByBOM_KitsPickedThroughPutawayComponents_MultiplePickedKitInventories()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			Helper.Factory.Save();
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 3m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 7m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No enough stock", false, kitOrderLine2.ChildComponentLines.Count > 0);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine1.PickLines[0];
			var framePickLine1 = frameOrderLine1.PickLines.Single(l => l.WZ_Units == 3m);
			var framePickLine2 = frameOrderLine1.PickLines.Single(l => l.WZ_Units == 7m);
			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceiveLine1 = createdReceive.Lines.Single();
			var createdReceiveLine1PK = createdReceiveLine1.PK;
			var bomLinks1 = createdReceiveLine1.BOMComponentLinks;
			AssertEquals(2, bomLinks1.Count());
			var bomLink1 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 20m);
			var bomLink2 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 10m);

			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			// putaway 
			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService1.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response1 = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response1.Error);
				AssertNull("Should be no error.", response1.ErrorMessage);
			});

			// pick and putaway remaining frames, this will generate the second picked kit receive line
			framePickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService2.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response2 = webService2.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response1.Error);
				AssertNull("Should be no error.", response1.ErrorMessage);
			});

			AssertEquals("Precondition - Kit Pick Line created from Children.", 2, kitOrderLine1.PickLines.Count);
			AssertEquals("Precondition - No Children, no Pick Lines.", 0, kitOrderLine2.PickLines.Count);
			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 10m, kitOrderLine1.ReleaseLines[0].Quantity);

			var newFactory = new BusinessObjectFactory();
			kitOrderLine1 = newFactory.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory.Load<WhsOrderLine>(kitOrderLine2.PK);
			bomLink1 = newFactory.Load<WhsBOMInventoryPivot>(bomLink1.PK);
			bomLink2 = newFactory.Load<WhsBOMInventoryPivot>(bomLink2.PK);
			createdReceive = newFactory.Load<WhsReceive>(createdReceive.PK);
			AssertEquals("2 Picked Kit Receive Lines", 2, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			var createdReceiveLine2 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 7m);
			AssertEquals(3m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals(7m, createdReceiveLine2.WE_ClientOrderedUnits);
			AssertEquals(2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals(2, createdReceiveLine2.BOMComponentLinks.Count());

			kitOrderLine1.ReleaseLines[0].Quantity = 9m;
			kitOrderLine2.ReleaseLines[0].Quantity = 1m;

			AssertEquals("Component Lines reconciled.", 2, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);

			AssertEquals("2 Picked Kit Receive Lines", 2, createdReceive.Lines.Count);
			AssertEquals("Picked Kit Receive Line not changes.", 3m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Picked Kit Receive Line not changes.", 3m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Picked Kit Receive Line not changes.", 7m, createdReceiveLine2.WE_TransactionQuantity);
			AssertEquals("Picked Kit Receive Line not changes.", 7m, createdReceiveLine2.WE_ClientOrderedUnits);

			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals("Kit Pick Line was split.", 2, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line was split.", 1, kitOrderLine2.PickLines.Count);
			AssertEquals("Kit Pick Line was split.", 9m, kitOrderLine1.PickLines.Sum(l => l.WZ_Units));
			AssertEquals("Kit Pick Line was split.", 1m, kitOrderLine2.PickLines.Single().WZ_Units);
			var reconciledPickLine1 = kitOrderLine2.PickLines[0];
			var receiveLineOfReconciledPickLine1 = reconciledPickLine1.InventoryLineForAvailableInventory;
			var theOtherKitReceiveLine = createdReceive.Lines.Single(l => l.PK != receiveLineOfReconciledPickLine1.PK);
			AssertEquals("2 more links for kitOrderLine2.", 4, receiveLineOfReconciledPickLine1.BOMComponentLinks.Count());
			AssertEquals("Didn't change.", 2, theOtherKitReceiveLine.BOMComponentLinks.Count());
			bomLink1 = receiveLineOfReconciledPickLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = receiveLineOfReconciledPickLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			var bomLink3 = receiveLineOfReconciledPickLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			var bomLink4 = receiveLineOfReconciledPickLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			var bomLink5 = theOtherKitReceiveLine.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			var bomLink6 = theOtherKitReceiveLine.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			AssertEquals(2m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(1m, bomLink4.WIP_ComponentQuantity);
			if (receiveLineOfReconciledPickLine1.WE_TransactionQuantity == 3m)
			{
				AssertEquals(4m, bomLink1.WIP_ComponentQuantity);
				AssertEquals(2m, bomLink2.WIP_ComponentQuantity);
				AssertEquals(14m, bomLink5.WIP_ComponentQuantity);
				AssertEquals(7m, bomLink6.WIP_ComponentQuantity);
			}
			else
			{
				AssertEquals(12m, bomLink1.WIP_ComponentQuantity);
				AssertEquals(6m, bomLink2.WIP_ComponentQuantity);
				AssertEquals(6m, bomLink5.WIP_ComponentQuantity);
				AssertEquals(3m, bomLink6.WIP_ComponentQuantity);
			}
			AssertNoExceptionThrown(newFactory.Save);

			kitOrderLine1.ReleaseLines[0].Quantity = 0m;
			kitOrderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Component Lines reconciled.", 0, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);

			AssertEquals("2 Picked Kit Receive Lines", 2, createdReceive.Lines.Count);
			AssertEquals("Picked Kit Receive Line not changes.", 3m, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals("Picked Kit Receive Line not changes.", 3m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals("Picked Kit Receive Line not changes.", 7m, createdReceiveLine2.WE_TransactionQuantity);
			AssertEquals("Picked Kit Receive Line not changes.", 7m, createdReceiveLine2.WE_ClientOrderedUnits);

			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals("Kit Pick Lines were reconciled.", 0, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Lines were reconciled.", 2, kitOrderLine2.PickLines.Count);
			reconciledPickLine1 = kitOrderLine2.PickLines.Single(l => l.WZ_Units == 3m);
			var reconciledPickLine2 = kitOrderLine2.PickLines.Single(l => l.WZ_Units == 7m);
			AssertEquals("Only Links for children of kitOrderLine2.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals("Only Links for children of kitOrderLine2.", 2, createdReceiveLine2.BOMComponentLinks.Count());
			bomLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink2 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			bomLink3 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink4 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals(6m, bomLink1.WIP_ComponentQuantity);
			AssertEquals(3m, bomLink2.WIP_ComponentQuantity);
			AssertEquals(14m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(7m, bomLink4.WIP_ComponentQuantity);

			AssertNoExceptionThrown(newFactory.Save);
		}

		public void TestReconcilePickLines_PickByBOM_KitsPickedThroughPutawayComponents_MixedWithUnpicked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			Helper.Factory.Save();
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 3m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 7m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No enough stock", false, kitOrderLine2.ChildComponentLines.Count > 0);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine1.PickLines[0];
			var framePickLine1 = frameOrderLine1.PickLines.Single(l => l.WZ_Units == 3m);
			var framePickLine2 = frameOrderLine1.PickLines.Single(l => l.WZ_Units == 7m);
			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceiveLine1 = createdReceive.Lines.Single();
			var createdReceiveLine1PK = createdReceiveLine1.PK;
			var bomLinks1 = createdReceiveLine1.BOMComponentLinks;
			AssertEquals(2, bomLinks1.Count());
			var bomLink1 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 20m);
			var bomLink2 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 10m);

			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			// putaway 
			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService1.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response1 = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response1.Error);
				AssertNull("Should be no error.", response1.ErrorMessage);
			});

			AssertEquals("Precondition - Kit Pick Line created from Children.", 2, kitOrderLine1.PickLines.Count);
			AssertEquals("Precondition - No Children, no Pick Lines.", 0, kitOrderLine2.PickLines.Count);
			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 10m, kitOrderLine1.ReleaseLines[0].Quantity);

			var newFactory = new BusinessObjectFactory();
			kitOrderLine1 = newFactory.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory.Load<WhsOrderLine>(kitOrderLine2.PK);
			bomLink1 = newFactory.Load<WhsBOMInventoryPivot>(bomLink1.PK);
			bomLink2 = newFactory.Load<WhsBOMInventoryPivot>(bomLink2.PK);
			createdReceive = newFactory.Load<WhsReceive>(createdReceive.PK);
			AssertEquals("2 Picked Kit Receive Lines", 2, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			var createdReceiveLine2 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 7m);
			AssertEquals(3m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals(7m, createdReceiveLine2.WE_ClientOrderedUnits);
			AssertEquals(2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals(2, createdReceiveLine2.BOMComponentLinks.Count());

			kitOrderLine1.ReleaseLines[0].Quantity = 9m;
			kitOrderLine2.ReleaseLines[0].Quantity = 1m;

			AssertEquals("Component Lines reconciled.", 2, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);

			AssertEquals("1 Picked Kit Receive Lines, 2 unpicked.", 3, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			createdReceiveLine2 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 6m);
			var createdReceiveLine3 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 1m);
			AssertEquals(3m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals(6m, createdReceiveLine2.WE_ClientOrderedUnits);
			AssertEquals(1m, createdReceiveLine3.WE_ClientOrderedUnits);

			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals("Kit Pick Line was split.", 2, kitOrderLine1.PickLines.Count);
			AssertEquals("Kit Pick Line was split.", 1, kitOrderLine2.PickLines.Count);
			AssertEquals("Kit Pick Line was split.", 9m, kitOrderLine1.PickLines.Sum(l => l.WZ_Units));
			AssertEquals("Kit Pick Line was split.", 1m, kitOrderLine2.PickLines.Single().WZ_Units);

			var kitPickLine1 = kitOrderLine1.PickLines.Single(l => l.WZ_Units == 3m);
			var kitPickLine2 = kitOrderLine1.PickLines.Single(l => l.WZ_Units == 6m);
			var kitPickLine3 = kitOrderLine2.PickLines.Single(l => l.WZ_Units == 1m);

			AssertEquals(2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals(2, createdReceiveLine2.BOMComponentLinks.Count());
			AssertEquals(2, createdReceiveLine3.BOMComponentLinks.Count());
			bomLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			var bomLink3 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			var bomLink4 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			var bomLink5 = createdReceiveLine3.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			var bomLink6 = createdReceiveLine3.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals(6m, bomLink1.WIP_ComponentQuantity);
			AssertEquals(3m, bomLink2.WIP_ComponentQuantity);
			AssertEquals(12m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(6m, bomLink4.WIP_ComponentQuantity);
			AssertEquals(2m, bomLink5.WIP_ComponentQuantity);
			AssertEquals(1m, bomLink6.WIP_ComponentQuantity);

			AssertNoExceptionThrown(newFactory.Save);

			kitOrderLine1.ReleaseLines[0].Quantity = 1m;
			kitOrderLine2.ReleaseLines[0].Quantity = 9m;

			AssertEquals("Component Lines reconciled.", 2, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);

			AssertEquals("1 Picked Kit Receive Lines, 1 unpicked.", 2, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			createdReceiveLine2 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 7m);
			AssertEquals(3m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals(7m, createdReceiveLine2.WE_ClientOrderedUnits);

			wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals(1, kitOrderLine1.PickLines.Count);
			AssertEquals(2, kitOrderLine2.PickLines.Count);
			AssertEquals(1m, kitOrderLine1.PickLines.Single().WZ_Units);
			AssertEquals(9m, kitOrderLine2.PickLines.Sum(l => l.WZ_Units));

			kitPickLine1 = kitOrderLine1.PickLines.Single(l => l.WZ_Units == 1m);
			kitPickLine2 = kitOrderLine2.PickLines.Single(l => l.WZ_Units == 2m);
			kitPickLine3 = kitOrderLine2.PickLines.Single(l => l.WZ_Units == 7m);

			AssertEquals(4, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals(2, createdReceiveLine2.BOMComponentLinks.Count());
			bomLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine1.PK);
			bomLink2 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine1.PK);
			bomLink3 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink4 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			bomLink5 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink6 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals(2m, bomLink1.WIP_ComponentQuantity);
			AssertEquals(1m, bomLink2.WIP_ComponentQuantity);
			AssertEquals(4m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(2m, bomLink4.WIP_ComponentQuantity);
			AssertEquals(14m, bomLink5.WIP_ComponentQuantity);
			AssertEquals(7m, bomLink6.WIP_ComponentQuantity);

			AssertNoExceptionThrown(newFactory.Save);

			kitOrderLine1.ReleaseLines[0].Quantity = 0m;
			kitOrderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Component Lines reconciled.", 0, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);

			AssertEquals("1 Picked Kit Receive Lines, 1 unpicked.", 2, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			createdReceiveLine2 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 7m);
			AssertEquals(3m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals(7m, createdReceiveLine2.WE_ClientOrderedUnits);

			wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals(0, kitOrderLine1.PickLines.Count);
			AssertEquals(2, kitOrderLine2.PickLines.Count);
			AssertEquals(10m, kitOrderLine2.PickLines.Sum(l => l.WZ_Units));

			kitPickLine1 = kitOrderLine2.PickLines.Single(l => l.WZ_Units == 3m);
			kitPickLine2 = kitOrderLine2.PickLines.Single(l => l.WZ_Units == 7m);

			AssertEquals(2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals(2, createdReceiveLine2.BOMComponentLinks.Count());
			bomLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink2 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			bomLink3 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink4 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals(6m, bomLink1.WIP_ComponentQuantity);
			AssertEquals(3m, bomLink2.WIP_ComponentQuantity);
			AssertEquals(14m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(7m, bomLink4.WIP_ComponentQuantity);

			AssertNoExceptionThrown(newFactory.Save);
		}

		public void TestReconcilePickLines_PickByBOM_KitsPickedThroughPutawayComponents_MixedWithUnpicked_MoveDirectly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			Helper.Factory.Save();
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			Helper.CreateProductBOM(bike, frame, 1m, "UNT");
			var location = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var locationString = dockdoorLocation.WLV_LocationString;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 3m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 7m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();

			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var kitOrderLine2 = Helper.CreateWhsOrderLine(kitOrder, bike, 10m);

			var pick = Helper.CreatePickNew(kitOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine2.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine1.ChildComponentLines.Count > 0);
			AssertEquals("Precondition - No enough stock", false, kitOrderLine2.ChildComponentLines.Count > 0);

			var wheelOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine1 = kitOrderLine1.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			var wheelPickLine = wheelOrderLine1.PickLines[0];
			var framePickLine1 = frameOrderLine1.PickLines.Single(l => l.WZ_Units == 3m);
			var framePickLine2 = frameOrderLine1.PickLines.Single(l => l.WZ_Units == 7m);
			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceiveLine1 = createdReceive.Lines.Single();
			var createdReceiveLine1PK = createdReceiveLine1.PK;
			var bomLinks1 = createdReceiveLine1.BOMComponentLinks;
			AssertEquals(2, bomLinks1.Count());
			var bomLink1 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 20m);
			var bomLink2 = bomLinks1.Single(l => l.WIP_ComponentQuantity == 10m);

			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			framePickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			// putaway 
			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService1.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response1 = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, response1.Error);
				AssertNull("Should be no error.", response1.ErrorMessage);
			});

			AssertEquals("Precondition - Kit Pick Line created from Children.", 2, kitOrderLine1.PickLines.Count);
			AssertEquals("Precondition - No Children, no Pick Lines.", 0, kitOrderLine2.PickLines.Count);
			AssertEquals("Precondition.", 1, kitOrderLine1.ReleaseLines.Count);
			AssertEquals("Precondition.", 0, kitOrderLine2.ReleaseLines.Count);
			AssertEquals("Precondition.", 10m, kitOrderLine1.ReleaseLines[0].Quantity);

			var newFactory = new BusinessObjectFactory();
			kitOrderLine1 = newFactory.Load<WhsOrderLine>(kitOrderLine1.PK);
			kitOrderLine2 = newFactory.Load<WhsOrderLine>(kitOrderLine2.PK);
			bomLink1 = newFactory.Load<WhsBOMInventoryPivot>(bomLink1.PK);
			bomLink2 = newFactory.Load<WhsBOMInventoryPivot>(bomLink2.PK);
			createdReceive = newFactory.Load<WhsReceive>(createdReceive.PK);
			AssertEquals("2 Picked Kit Receive Lines", 2, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			var createdReceiveLine2 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 7m);
			AssertEquals(3m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals(7m, createdReceiveLine2.WE_ClientOrderedUnits);
			AssertEquals(2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals(2, createdReceiveLine2.BOMComponentLinks.Count());

			kitOrderLine1.ReleaseLines[0].Quantity = 0m;
			kitOrderLine2.ReleaseLines[0].Quantity = 10m;

			AssertEquals("Component Lines reconciled.", 0, kitOrderLine1.ChildComponentLines.Count);
			AssertEquals("Component Lines reconciled.", 2, kitOrderLine2.ChildComponentLines.Count);

			AssertEquals("1 Picked Kit Receive Lines, 1 unpicked.", 2, createdReceive.Lines.Count);
			createdReceiveLine1 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 3m);
			createdReceiveLine2 = createdReceive.Lines.Single(l => l.WE_TransactionQuantity == 7m);
			AssertEquals(3m, createdReceiveLine1.WE_ClientOrderedUnits);
			AssertEquals(7m, createdReceiveLine2.WE_ClientOrderedUnits);

			var wheelOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameOrderLine2 = kitOrderLine2.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals(0, kitOrderLine1.PickLines.Count);
			AssertEquals(2, kitOrderLine2.PickLines.Count);
			AssertEquals(10m, kitOrderLine2.PickLines.Sum(l => l.WZ_Units));

			var kitPickLine1 = kitOrderLine2.PickLines.Single(l => l.WZ_Units == 3m);
			var kitPickLine2 = kitOrderLine2.PickLines.Single(l => l.WZ_Units == 7m);

			AssertEquals(2, createdReceiveLine1.BOMComponentLinks.Count());
			AssertEquals(2, createdReceiveLine2.BOMComponentLinks.Count());
			bomLink1 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			bomLink2 = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			var bomLink3 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelOrderLine2.PK);
			var bomLink4 = createdReceiveLine2.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameOrderLine2.PK);
			AssertEquals(6m, bomLink1.WIP_ComponentQuantity);
			AssertEquals(3m, bomLink2.WIP_ComponentQuantity);
			AssertEquals(14m, bomLink3.WIP_ComponentQuantity);
			AssertEquals(7m, bomLink4.WIP_ComponentQuantity);

			AssertNoExceptionThrown(newFactory.Save);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_PartOfPickOnToteTrolleyPickingJob

		public void TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnToteTrolleyPickingJob_Pick()
		{
			TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnToteTrolleyPickingJob_PickCore(hasPackingStations: true);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnToteTrolleyPickingJob_Pick_NoPackingStations()
		{
			TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnToteTrolleyPickingJob_PickCore(hasPackingStations: false);
		}

		void TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnToteTrolleyPickingJob_PickCore(bool hasPackingStations)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			if (hasPackingStations)
			{
				var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
				var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
				packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			}
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine1);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			if (hasPackingStations)
			{
				AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Should be error.", "Location DOCKDOOR is not valid for this job as some parts of the pick requires a Packing Station.", response.ErrorMessage);
				AssertExpectedLocationDetails(response, new WhsLocationInfo(Guid.Empty, string.Empty, string.Empty, LocationClasses.Codes.PST));
			}
			else
			{
				AssertSuccessfulResponseWithNoErrors(response, webService);
				AssertExpectedLocationDetails(response, location: null);
			}
		}

		public void TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnToteTrolleyPickingJob_PickByLabel()
		{
			TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnToteTrolleyPickingJob_PickByLabelCore(hasPackingStations: true);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnToteTrolleyPickingJob_PickByLabel_NoPackingStations()
		{
			TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnToteTrolleyPickingJob_PickByLabelCore(hasPackingStations: false);
		}

		void TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnToteTrolleyPickingJob_PickByLabelCore(bool hasPackingStations)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			if (hasPackingStations)
			{
				var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
				var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
				packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			}
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine1);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package2.PK);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			if (hasPackingStations)
			{
				AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Should be error.", "Location DOCKDOOR is not valid for this job as some parts of the pick requires a Packing Station.", response.ErrorMessage);
				AssertExpectedLocationDetails(response, new WhsLocationInfo(Guid.Empty, string.Empty, string.Empty, LocationClasses.Codes.PST));
			}
			else
			{
				AssertSuccessfulResponseWithNoErrors(response, webService);
				AssertExpectedLocationDetails(response, location: null);
			}
		}

		public void TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnCartonTrolleyPickingJob_Pick()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine1);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertExpectedLocationDetails(response, location: null);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_DockDoorPutaway_PartOfPickOnToteTrolleyPickingJob_WithAssignedDockDoorPutaway()
		{
			// this should not happen, but if it did then the assigned putaway location should take precedence
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var packingStationType = Helper.CreateLocationType("PST", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;

			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 20m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			var pickLine3 = orderLine3.PickLines.Single();

			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine1);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);

			var transferLinePickLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);
			transferLinePickLine3.WE_WL = dockdoorLocation.PK;
			transferLinePickLine3.FinaliseDocketLine();

			Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, dockdoorLocation.WLV_LocationString);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertExpectedLocationDetails(response, location: null);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_PackingStationPutaway_PartOfPickOnToteTrolleyPickingJob_PickByLabel()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var packingStationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var packingStationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST", 1, 1).Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			pkg_OnTrolley.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine1);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package2.PK);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, packingStationLocation.WLV_LocationString);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertExpectedLocationDetails(response, location: null);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_HCCAdjustedInPalletisedStock

		public void TestPutawayStockInDockDoorOrPackingStation_HCCAdjustedInPalletisedStock()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 2);

			var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 4m, data.Whs1.DefaultLocation.WLV_LocationString, "PalletID1", ZDateTimeOffset.Today);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			var pick = helper.CreatePickNew(order);
			factory.Save();

			var webService1 = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var responsePAP = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { order.Lines[0].PickLines[0].PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(3m, isVerifiedNonEmpty: false),
				packagePackInto: null,
				createPackagesForPickedPacks: false);
			Assert("Should be no error message.", string.IsNullOrEmpty(responsePAP.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, responsePAP.Error);

			var outboundTransferLines = webService1.Factory.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer));
			AssertEquals("Outbound transfer line count correct", 1, outboundTransferLines.Length);

			var outboundTransferLine = outboundTransferLines[0];
			AssertEquals("Outbound transfer line should have correct WE_TransactionQuantity", 3m, outboundTransferLine.WE_TransactionQuantity);
			Assert("Outbound transfer line should not have split pallet ID", string.IsNullOrEmpty(outboundTransferLine.WE_PalletID));

			var webService2 = GetNewWebService(data.Whs1, GlbStaff.CurrentUser);
			var responsePTD = webService2.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString);
			Assert("Should be no error message.", string.IsNullOrEmpty(responsePTD.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, responsePTD.Error);
		}

		#endregion

		#region TestPutawayStockInDockDoorOrPackingStation_OverrideDDL

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 2);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var otherDDL = data.Whs1.FindLocation("A-1-1");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be info error.", ErrorTypes.Information, response.Error);
			AssertEquals("Should be info error.", $"Overriding Dock Door Location to {otherDDL.WLV_LocationString}.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(otherDDL));

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLine_InFactory2 = factory2.Load<WhsTransferLine>(inTransitTransferLine.PK);
			AssertLinePutaway(inTransitTransferLine_InFactory2);
			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);
			AssertEquals("Should have changed the dock door on the pick to empty.", ZGuid.Empty, pick.WP_WL_DockDoor);
			AssertEquals("Should have changed the added Dock door assignment to pick.", true, pick.WP_WDA_DockDoorAssignment.IsValid);
			AssertEquals("Should have changed the dock door location on DDA.", otherDDL.PK, pick.DockDoorAssignment.WDA_WL_AssignedDockDoor);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_NotADDL()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 2);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			var otherLoc = data.Whs1.FindLocation("A-1-1");
			var locationString = otherLoc.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Location A-1-1 is not an Outbound Dock Door Location.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: null);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLine_InFactory2 = factory2.Load<WhsTransferLine>(inTransitTransferLine.PK);
			AssertLineNotPutaway(inTransitTransferLine_InFactory2);
			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);
			AssertEquals("Should have changed the dock door on the pick.", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_ParamNotSet()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 2);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var otherDDL = data.Whs1.FindLocation("A-1-1");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = false;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", $"Cannot override Dock Door Location as Client '{data.Org1.OH_FullNameTruncated}' does not allow overrides", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(data.Whs1.DefaultInboundDockDoorLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_ParamNotSetOnOtherOrder()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 2);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var otherDDL = data.Whs1.FindLocation("A-1-1");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam1 = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam1.WPP_AllowPickDockDoorLocationOverride = true;

			var client2 = helper.CreateClient("C2");
			helper.CreateProductClientRelationShip(client2, data.Part1);

			var pickPackParam2 = helper.CreatePickPackParameter(client2, data.Whs1);
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = false;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part1, 10m);
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "O2", data.Part1, 10m);
			var pick = helper.CreatePickNew(order1, order2);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", $"Cannot override Dock Door Location as Client '{client2.OH_FullNameTruncated}' does not allow overrides", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(data.Whs1.DefaultInboundDockDoorLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_StockAlreadyPutaway()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 2);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var otherDDL = data.Whs1.FindLocation("A-1-1");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var pick = helper.CreatePickNew(order1, order2);
			pick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();

			var inTransitTransfer = pick.Transfers.Single();
			var inTransitTransferLine1 = inTransitTransfer.Lines[0];
			AssertEquals("Precondition: inTransitTransferLine1 Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine1.WE_CurrentInventoryStatus);
			var inTransitTransferLine2 = inTransitTransfer.Lines[1];
			AssertEquals("Precondition: inTransitTransferLine2 Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine2.WE_CurrentInventoryStatus);

			inTransitTransferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(inTransitTransferLine1);
			webService.Factory.Save();

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Location A-1-1 is not valid for this job as some inventory has already been putaway to DOCKDOOR.", response.ErrorMessage);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_ConcurrencyExceptionThrown()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			factory.Save();

			var inTransitTransferLine = (WhsTransferLine)pick.Transfers.Single().Lines.Single();
			AssertEquals("Precondition: Should be In-Transit.", InventoryStatus.Codes.InTransit, inTransitTransferLine.WE_CurrentInventoryStatus);

			factory.Saving += f =>
			{
				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var transferLineInOtherFactory = otherFactory.Load<WhsTransferLine>(inTransitTransferLine.PK);
				transferLineInOtherFactory.WE_CustomAttrib1 = "US1"; // Change cause concurrency exception
				otherFactory.Save();
			};

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, locationString);
			AssertEquals("Should not be able to save.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should not be able to save.", "While you have been working with this job another user has made changes. Please restart the operation and try again.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(otherDDL));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_Trolley()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = helper.CreatePickNew(order);
			webService.Factory.Save();

			pick.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var pickLine = order.Lines[0].PickLines[0];
			packingHelper.CreatePackageDivot(pkg, pickLine);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg, 1);
			webService.Factory.Save();
			AssertType<WhsPickLine>("Precondition.", pkg.PackedItemDivots[0].PackedItem);

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertEquals("Should be info error.", ErrorTypes.Information, response.Error);
			AssertEquals("Should be info error.", $"Overriding Dock Door Location to {otherDDL.WLV_LocationString}.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(otherDDL));

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTrolleyTransferLine_InFactory2 = factory2.Load<WhsTransferLine>(pick.Transfers.Single().Lines.Single().PK);
			AssertLinePutaway(inTransitTrolleyTransferLine_InFactory2);
			AssertEquals("Should have changed the dock door on the pick.", ZGuid.Empty, pick.WP_WL_DockDoor);
			AssertEquals("Should have added dock door assignment for correct DDL to pick.", otherDDL.PK, pick.DockDoorAssignment.WDA_WL_AssignedDockDoor);
			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_Trolley_PickByLabelOnPick()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderline2 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = helper.CreatePickNew(order);
			webService.Factory.Save();

			pick.GetAllPickLines().ForEach(p => p.WZ_PickedDateTime = ZDateTimeOffset.Today);
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var pickLine1 = orderline1.PickLines[0];
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			var pickLine2 = orderline2.PickLines[0];
			packingHelper.CreatePackageDivot(package2, pickLine2);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, package1, 1);

			var pickByLabelJob =
				WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(
					webService.Factory,
					data.Whs1.PK,
					data.Whs1.WW_DefaultOutboundDockDoor,
					GlbStaff.CurrentUser.GS_Code,
					package1.PK);

			webService.Factory.Save();

			AssertType<WhsPickLine>("Precondition.", package1.PackedItemDivots[0].PackedItem);

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertEquals("Should be info error.", ErrorTypes.Information, response.Error);
			AssertEquals("Should be info error.", $"Overriding Dock Door Location to {otherDDL.WLV_LocationString}.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(otherDDL));

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTrolleyTransferLine_InFactory2 = factory2.Load<WhsTransferLine>(pick.Transfers.Single().Lines[0].PK);
			AssertLinePutaway(inTransitTrolleyTransferLine_InFactory2);
			AssertEquals("Should have changed the dock door on the pick.", ZGuid.Empty, pick.WP_WL_DockDoor);
			AssertEquals("Should have added dock door assignment for correct DDL to pick.", otherDDL.PK, pick.DockDoorAssignment.WDA_WL_AssignedDockDoor);
			AssertEquals("Should have changed the dock door on the pick by label job.", otherDDL.PK, pickByLabelJob.WTK_WL_DockDoor);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_Trolley_ParamNotSet()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var packingHelper = new PackingTestHelper(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 2);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;

			var otherDDL = data.Whs1.FindLocation("A-1-1");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = false;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			webService.Factory.Save();

			pick.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var pickLine = order.Lines[0].PickLines[0];
			packingHelper.CreatePackageDivot(pkg, pickLine);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg, 1);
			webService.Factory.Save();
			AssertType<WhsPickLine>("Precondition.", pkg.PackedItemDivots[0].PackedItem);

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", $"Cannot override Dock Door Location as Client '{data.Org1.OH_FullNameTruncated}' does not allow overrides", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(data.Whs1.DefaultInboundDockDoorLocation));
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_Trolley_MultiplePicks()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1m);
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick1 = helper.CreatePickNew(order1);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 1m);
			var pick2 = helper.CreatePickNew(order2);
			webService.Factory.Save();

			pick1.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			pick2.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			var pickLine1 = order1.Lines[0].PickLines[0];
			packingHelper.CreatePackageDivot(pkg1, pickLine1);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			var pickLine2 = order2.Lines[0].PickLines[0];
			packingHelper.CreatePackageDivot(pkg2, pickLine2);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2);
			var dda = helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			webService.Factory.Save();
			AssertType<WhsPickLine>("Precondition.", pkg1.PackedItemDivots[0].PackedItem);
			AssertType<WhsPickLine>("Precondition.", pkg2.PackedItemDivots[0].PackedItem);

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertEquals("Should be info error.", ErrorTypes.Information, response.Error);
			AssertEquals("Should be info error.", $"Overriding Dock Door Location to {otherDDL.WLV_LocationString}.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(otherDDL));

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTrolleyTransferLine1_InFactory2 = factory2.Load<WhsTransferLine>(pick1.Transfers.Single().Lines.Single().PK);
			AssertLinePutaway(inTransitTrolleyTransferLine1_InFactory2);
			var inTransitTrolleyTransferLine2_InFactory2 = factory2.Load<WhsTransferLine>(pick2.Transfers.Single().Lines.Single().PK);
			AssertLinePutaway(inTransitTrolleyTransferLine2_InFactory2);

			AssertEquals("Dock door on the pick1.", ZGuid.Empty, pick1.WP_WL_DockDoor);
			AssertEquals("Dock door on the pick2.", ZGuid.Empty, pick2.WP_WL_DockDoor);
			AssertEquals("Dock door on the dock door assignment.", otherDDL.PK, dda.WDA_WL_AssignedDockDoor);
			AssertNotEquals("WDA_FirstPutawayToDockDoorUtc on the dockdoor assignment should be set.", ZDateTime.Empty, dda.WDA_FirstPutawayToDockDoorUtc);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_Trolley_ParamNotSetOnOtherPick()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			var client2 = helper.CreateClient("C2");
			helper.CreateProductClientRelationShip(client2, data.Part2);

			var pickPackParam2 = helper.CreatePickPackParameter(client2, data.Whs1);
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = false;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part2, 1m);
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick1 = helper.CreatePickNew(order1);
			var order2 = helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "O2", data.Part2, 1m);
			var pick2 = helper.CreatePickNew(order2);
			webService.Factory.Save();

			pick1.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			pick2.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			var pickLine1 = order1.Lines[0].PickLines[0];
			packingHelper.CreatePackageDivot(pkg1, pickLine1);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			var pickLine2 = order2.Lines[0].PickLines[0];
			packingHelper.CreatePackageDivot(pkg2, pickLine2);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2);
			var dda = helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			webService.Factory.Save();
			AssertType<WhsPickLine>("Precondition.", pkg1.PackedItemDivots[0].PackedItem);
			AssertType<WhsPickLine>("Precondition.", pkg2.PackedItemDivots[0].PackedItem);

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", $"Cannot override Dock Door Location as Client '{client2.OH_FullNameTruncated}' does not allow overrides", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(data.Whs1.DefaultInboundDockDoorLocation));

			AssertEquals("Dock door on the pick1.", ZGuid.Empty, pick1.WP_WL_DockDoor);
			AssertEquals("Dock door on the pick2.", ZGuid.Empty, pick2.WP_WL_DockDoor);
			AssertEquals("Dock door on the dock door assignment.", data.Whs1.DefaultOutboundDockDoorLocation.PK, dda.WDA_WL_AssignedDockDoor);
			AssertEquals("WDA_FirstPutawayToDockDoorUtc on the dockdoor assignment should not be set.", ZDateTime.Empty, dda.WDA_FirstPutawayToDockDoorUtc);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_Trolley_FirstPutawaySet()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 1m);
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick1 = helper.CreatePickNew(order1);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 1m);
			var pick2 = helper.CreatePickNew(order2);
			webService.Factory.Save();

			pick1.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			pick2.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			var pickLine1 = order1.Lines[0].PickLines[0];
			packingHelper.CreatePackageDivot(pkg1, pickLine1);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			var pickLine2 = order2.Lines[0].PickLines[0];
			packingHelper.CreatePackageDivot(pkg2, pickLine2);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2);
			var dda = helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			webService.Factory.Save();
			AssertType<WhsPickLine>("Precondition.", pkg1.PackedItemDivots[0].PackedItem);
			AssertType<WhsPickLine>("Precondition.", pkg2.PackedItemDivots[0].PackedItem);

			var time = ZDateTime.UtcNow;
			dda.WDA_FirstPutawayToDockDoorUtc = time;
			webService.Factory.Save();

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, locationString);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", $"Cannot override Dock Door Location as Stock is already putaway to '{data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly}'", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(data.Whs1.DefaultInboundDockDoorLocation));

			AssertEquals("Dock door on the pick1.", ZGuid.Empty, pick1.WP_WL_DockDoor);
			AssertEquals("Dock door on the pick2.", ZGuid.Empty, pick2.WP_WL_DockDoor);
			AssertEquals("Dock door on the dock door assignment.", data.Whs1.DefaultOutboundDockDoorLocation.PK, dda.WDA_WL_AssignedDockDoor);
			AssertEquals("WDA_FirstPutawayToDockDoorUtc on the dockdoor assignment should be set to correct value.", time, dda.WDA_FirstPutawayToDockDoorUtc);
		}

		[TestDate(2018, 09, 11, 12, 10, 20)]
		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_PickByLabel()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			webService.Factory.Save();

			var pickLine = pick.GetAllPickLines().Single();
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLine);
			webService.Factory.Save();

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			AssertEquals("Precondition.", 1, pick.Transfers.Single().Lines.Count);

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			webService.Factory.Save();

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			AssertEquals("Should be info error.", ErrorTypes.Information, response.Error);
			AssertEquals("Should be info error.", $"Overriding Dock Door Location to {otherDDL.WLV_LocationString}.", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(otherDDL));

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLine_InFactory2 = factory2.Load<WhsTransferLine>(pickLine.WZ_WE_InventoryLine);
			AssertLinePutaway(inTransitTransferLine_InFactory2);
			AssertEquals("Should *not* have finalised Transfer.", false, pick.Transfers.Single().IsFinalised);
			AssertEquals("Should have changed the dock door on the pick.", ZGuid.Empty, pick.WP_WL_DockDoor);
			AssertEquals("Should have added dock door assignment for correct DDL to pick.", otherDDL.PK, pick.DockDoorAssignment.WDA_WL_AssignedDockDoor);

			var jobInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsPickByLabelJob>(pickByLabelJob.PK);
			AssertEquals("After putting packages in dock door location, FinalisedDate should be current date time.", new ZDateTimeOffset(2018, 09, 11, 12, 10, 20, TimeSpan.Zero), jobInNewFactory.WTK_FinalisedDate);
			AssertEquals("WTK_WL_DockDoor should be correct", otherDDL.PK, jobInNewFactory.WTK_WL_DockDoor);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_PickByLabel_ParamNotSet()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = false;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			webService.Factory.Save();

			var pickLine = pick.GetAllPickLines().Single();
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLine);
			webService.Factory.Save();

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			AssertEquals("Precondition.", 1, pick.Transfers.Single().Lines.Count);

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(webService.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package.PK);
			webService.Factory.Save();

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", $"Cannot override Dock Door Location as Client '{data.Org1.OH_FullNameTruncated}' does not allow overrides", response.ErrorMessage);

			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(dockDoorLocation));
			AssertEquals("Should have not updated Pick DDL.", dockDoorLocation.PK, pick.WP_WL_DockDoor);
			AssertEquals("WTK_WL_DockDoor should be correct.", dockDoorLocation.PK, pickByLabelJob.WTK_WL_DockDoor);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_PickByLabel_MultiplePicks()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			var client2 = helper.CreateClient("C2");
			helper.CreateProductClientRelationShip(client2, data.Part2);

			var pickPackParam2 = helper.CreatePickPackParameter(client2, data.Whs1);
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = true;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part2, 1m);
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick1 = helper.CreatePickNew(order1);
			var order2 = helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "O2", data.Part2, 1m);
			var pick2 = helper.CreatePickNew(order2);
			webService.Factory.Save();

			var pickLine1 = pick1.GetAllPickLines().Single();
			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickLine2 = pick2.GetAllPickLines().Single();
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			webService.Factory.Save();

			var dda = helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultInboundDockDoorLocation, pick1, pick2);

			pick1.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			pick2.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			AssertEquals("Precondition pick1.", 1, pick1.Transfers.Single().Lines.Count);
			AssertEquals("Precondition pick2.", 1, pick2.Transfers.Single().Lines.Count);

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(webService.Factory, data.Whs1.PK, GlbStaff.CurrentUser.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package1.PK);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package2.PK);
			webService.Factory.Save();

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			CombineAssertions(() =>
			{
				AssertEquals("Should be info error.", ErrorTypes.Information, response.Error);
				AssertEquals("Should be info error.", $"Overriding Dock Door Location to {otherDDL.WLV_LocationString}.", response.ErrorMessage);
			});
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(otherDDL));

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLine1_InFactory2 = factory2.Load<WhsTransferLine>(pickLine1.WZ_WE_InventoryLine);
			AssertLinePutaway(inTransitTransferLine1_InFactory2);
			var inTransitTransferLine2_InFactory2 = factory2.Load<WhsTransferLine>(pickLine2.WZ_WE_InventoryLine);
			AssertLinePutaway(inTransitTransferLine2_InFactory2);
			AssertEquals("Should *not* have finalised Pick1 Transfer.", false, pick1.Transfers.Single().IsFinalised);
			AssertEquals("Should *not* have finalised Pick2 Transfer.", false, pick2.Transfers.Single().IsFinalised);
			AssertEquals("Pick DDL correct.", ZGuid.Empty, pick1.WP_WL_DockDoor);
			AssertEquals("Pick DDL correct.", ZGuid.Empty, pick2.WP_WL_DockDoor);
			AssertEquals("DDA DDL correct.", otherDDL.PK, dda.WDA_WL_AssignedDockDoor);
			AssertEquals("WTK_WL_DockDoor should be correct", otherDDL.PK, pickByLabelJob.WTK_WL_DockDoor);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_PickByLabel_ParamNotSetOnOtherPick()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			var client2 = helper.CreateClient("C2");
			helper.CreateProductClientRelationShip(client2, data.Part2);

			var pickPackParam2 = helper.CreatePickPackParameter(client2, data.Whs1);
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = false;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part2, 1m);
			webService.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick1 = helper.CreatePickNew(order1);
			var order2 = helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "O2", data.Part2, 1m);
			var pick2 = helper.CreatePickNew(order2);
			webService.Factory.Save();

			var pickLine1 = pick1.GetAllPickLines().Single();
			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickLine2 = pick2.GetAllPickLines().Single();
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			webService.Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var dda = helper.CreateWhsDockDoorAssignment(dockDoorLocation, pick1, pick2);

			pick1.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			pick2.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			AssertEquals("Precondition pick1.", 1, pick1.Transfers.Single().Lines.Count);
			AssertEquals("Precondition pick2.", 1, pick2.Transfers.Single().Lines.Count);

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(webService.Factory, data.Whs1.PK, GlbStaff.CurrentUser.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package1.PK);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package2.PK);
			webService.Factory.Save();

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", $"Cannot override Dock Door Location as Client '{client2.OH_FullNameTruncated}' does not allow overrides", response.ErrorMessage);
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(dockDoorLocation));
			AssertEquals("Pick DDL correct.", ZGuid.Empty, pick1.WP_WL_DockDoor);
			AssertEquals("Pick DDL correct.", ZGuid.Empty, pick2.WP_WL_DockDoor);

			AssertEquals("DDA DDL correct.", dockDoorLocation.PK, dda.WDA_WL_AssignedDockDoor);
			AssertEquals("WTK_WL_DockDoor should be correct.", dockDoorLocation.PK, pickByLabelJob.WTK_WL_DockDoor);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_PickByLabel_FirstPutawaySet()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;

			var client2 = helper.CreateClient("C2");
			helper.CreateProductClientRelationShip(client2, data.Part2);

			var pickPackParam2 = helper.CreatePickPackParameter(client2, data.Whs1);
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = true;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part2, 1m);
			factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick1 = helper.CreatePickNew(order1);
			var order2 = helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "O2", data.Part2, 1m);
			var pick2 = helper.CreatePickNew(order2);
			factory.Save();

			var pickLine1 = pick1.GetAllPickLines().Single();
			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickLine2 = pick2.GetAllPickLines().Single();
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			factory.Save();

			var dda = helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			factory.Save();

			var time = ZDateTime.UtcNow;
			dda.WDA_FirstPutawayToDockDoorUtc = time;

			pick1.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			pick2.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			factory.Save();

			AssertEquals("Precondition pick1.", 1, pick1.Transfers.Single().Lines.Count);
			AssertEquals("Precondition pick2.", 1, pick2.Transfers.Single().Lines.Count);

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(webService.Factory, data.Whs1.PK, GlbStaff.CurrentUser.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package1.PK);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package2.PK);
			factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, otherDDL.WLV_LocationString);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", $"Cannot override Dock Door Location as Stock is already putaway to '{dockDoorLocation.WLV_LocationString_UserFriendly}'", response.ErrorMessage);

			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(data.Whs1.DefaultInboundDockDoorLocation));
			AssertEquals("Pick DDL correct.", ZGuid.Empty, pick1.WP_WL_DockDoor);
			AssertEquals("Pick DDL correct.", ZGuid.Empty, pick2.WP_WL_DockDoor);

			AssertEquals("DDA DDL correct.", dockDoorLocation.PK, dda.WDA_WL_AssignedDockDoor);
			AssertEquals("DDA FirstPutawayToDockDoorUtc correct.", time, dda.WDA_FirstPutawayToDockDoorUtc);
			AssertEquals("WTK_WL_DockDoor should be correct.", dockDoorLocation.PK, pickByLabelJob.WTK_WL_DockDoor);
		}

		public void TestPutawayStockInDockDoorOrPackingStation_OverrideDDL_MultipleLinks()
		{
			var webService = GetNewWebService();
			var factory = webService.Factory;
			var helper = new WhsTestHelperFunctions(factory);
			var packingHelper = new PackingTestHelper(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			factory.Save();

			var otherDDL = data.Whs1.FindLocation("A-2");
			otherDDL.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var pickPackParam = helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			webService.Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			helper.Factory.Save();

			// PBL
			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = helper.CreatePickNew(order1);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 5m);
			var pick2 = helper.CreatePickNew(order2);

			// Trolley
			var order3 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var pick3 = helper.CreatePickNew(order3);
			var order4 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 5m);
			var pick4 = helper.CreatePickNew(order4);
			webService.Factory.Save();

			var pickLine1 = pick1.GetAllPickLines().Single();
			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickLine2 = pick2.GetAllPickLines().Single();
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package2, pickLine2);

			var pickLine3 = pick3.GetAllPickLines().Single();
			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var package3 = packingHelper.CreatePackage(pkgJob3, "PKG3", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package3, pickLine3);

			var pickLine4 = pick4.GetAllPickLines().Single();
			var pkgJob4 = PkgPackageJob.LoadOrCreatePackageJob(order4);
			var package4 = packingHelper.CreatePackage(pkgJob4, "PKG4", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(package4, pickLine4);
			webService.Factory.Save();

			var dda = helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultInboundDockDoorLocation, pick1, pick2, pick3, pick4);

			pick1.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			pick2.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			pick3.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			pick4.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Today;
			webService.Factory.Save();

			AssertEquals("Precondition pick1.", 1, pick1.Transfers.Single().Lines.Count);
			AssertEquals("Precondition pick2.", 1, pick2.Transfers.Single().Lines.Count);
			AssertEquals("Precondition pick3.", 1, pick3.Transfers.Single().Lines.Count);
			AssertEquals("Precondition pick4.", 1, pick4.Transfers.Single().Lines.Count);

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(webService.Factory, data.Whs1.PK, GlbStaff.CurrentUser.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package1.PK);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package2.PK);

			var trolley = helper.CreateTrolley("T001");
			var trolleyJob = helper.CreateWhsPickTrolleyJob(trolley);
			helper.CreateWhsPickTrolleySlot(trolleyJob, package3, 1);
			helper.CreateWhsPickTrolleySlot(trolleyJob, package4, 2);
			webService.Factory.Save();

			AssertEquals("Precondition pickByLabelJob DDL.", data.Whs1.WW_DefaultOutboundDockDoor, pickByLabelJob.WTK_WL_DockDoor);

			var locationString = otherDDL.WLV_LocationString;
			var response = webService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, locationString);

			CombineAssertions(() =>
			{
				AssertEquals("Should be info error.", ErrorTypes.Information, response.Error);
				AssertEquals("Should be info error.", $"Overriding Dock Door Location to {otherDDL.WLV_LocationString}.", response.ErrorMessage);
			});
			AssertExpectedLocationDetails(response, location: GetWhsLocationInfo(otherDDL));

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var inTransitTransferLine1_InFactory2 = factory2.Load<WhsTransferLine>(pickLine1.WZ_WE_InventoryLine);
			AssertLinePutaway(inTransitTransferLine1_InFactory2);
			var inTransitTransferLine2_InFactory2 = factory2.Load<WhsTransferLine>(pickLine2.WZ_WE_InventoryLine);
			AssertLinePutaway(inTransitTransferLine2_InFactory2);
			var inTransitTransferLine3_InFactory2 = factory2.Load<WhsTransferLine>(pickLine3.WZ_WE_InventoryLine);
			AssertLineNotPutaway(inTransitTransferLine3_InFactory2);
			var inTransitTransferLine4_InFactory2 = factory2.Load<WhsTransferLine>(pickLine4.WZ_WE_InventoryLine);
			AssertLineNotPutaway(inTransitTransferLine4_InFactory2);

			AssertEquals("Should *not* have finalised Pick1 Transfer.", false, pick1.Transfers.Single().IsFinalised);
			AssertEquals("Should *not* have finalised Pick2 Transfer.", false, pick2.Transfers.Single().IsFinalised);

			AssertEquals("Pick1 DDL correct.", ZGuid.Empty, pick1.WP_WL_DockDoor);
			AssertEquals("Pick2 DDL correct.", ZGuid.Empty, pick2.WP_WL_DockDoor);
			AssertEquals("Pick3 DDL correct.", ZGuid.Empty, pick3.WP_WL_DockDoor);
			AssertEquals("Pick4 DDL correct.", ZGuid.Empty, pick4.WP_WL_DockDoor);

			AssertEquals("DDA DDL correct.", otherDDL.PK, dda.WDA_WL_AssignedDockDoor);
			AssertEquals("DDA WDA_FirstPutawayToDockDoorUtc set.", true, dda.WDA_FirstPutawayToDockDoorUtc.IsValid);

			AssertEquals("WTK_WL_PutawayLocation should be correct", otherDDL.PK, pickByLabelJob.WTK_WL_PutawayLocation);
			AssertEquals("Precondition pickByLabelJob DDL.", otherDDL.PK, pickByLabelJob.WTK_WL_DockDoor);
		}

		#endregion

		#region Implementation

		static void AssertLinePutaway(WhsDocketLine transferLine)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Inventory should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
				AssertEquals("Line should be finalised.", true, transferLine.IsFinalised);
				AssertEquals("Inventory should have been putaway.", false, transferLine.WE_PutawayTime.IsEmpty);
			});
		}

		static void AssertLineNotPutaway(WhsDocketLine transferLine)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Inventory should remain In-Transit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
				AssertEquals("Line should *not* be finalised.", false, transferLine.IsFinalised);
				AssertEquals("Inventory should *not* have been putaway.", true, transferLine.WE_PutawayTime.IsEmpty);
			});
		}

		static void AssertKitReceiveLine(WhsReceiveLine kitReceiveLine, ZGuid toLocation, ZDateTimeOffset lastPickedDate, decimal quantity, string putawayBy = "", decimal stockOnHand = 0m)
		{
			AssertEquals(nameof(kitReceiveLine.WE_WL), toLocation, kitReceiveLine.WE_WL);
			AssertEquals(nameof(kitReceiveLine.WE_CurrentInventoryStatus), InventoryStatus.Codes.Putaway, kitReceiveLine.WE_CurrentInventoryStatus);
			AssertEquals(nameof(kitReceiveLine.WE_OriginalInventoryStatus), InventoryStatus.Codes.Putaway, kitReceiveLine.WE_OriginalInventoryStatus);
			AssertEquals(nameof(kitReceiveLine.WE_DocketLineStatus), DocketLineStatus.Codes.PickedForUnload, kitReceiveLine.WE_DocketLineStatus);
			AssertEquals(nameof(kitReceiveLine.WE_AdjustmentArrivalDate), lastPickedDate, kitReceiveLine.WE_AdjustmentArrivalDate);
			AssertEquals(nameof(kitReceiveLine.WE_TransactionQuantity), quantity, kitReceiveLine.WE_TransactionQuantity);
			AssertEquals(nameof(kitReceiveLine.WE_ClientOrderedUnits), quantity, kitReceiveLine.WE_ClientOrderedUnits);
			AssertEquals(nameof(kitReceiveLine.WE_StockOnHand), stockOnHand, kitReceiveLine.WE_StockOnHand);
			AssertEquals(nameof(kitReceiveLine.WE_UnloadedTime), lastPickedDate, kitReceiveLine.WE_UnloadedTime);
			AssertEquals(nameof(kitReceiveLine.WE_GS_NKUnloadedBy), string.IsNullOrEmpty(putawayBy) ? GlbStaff.CurrentUser.GS_Code : putawayBy, kitReceiveLine.WE_GS_NKUnloadedBy);
		}

		static void AssertKitReceiveLineNotChanged(WhsReceiveLine kitReceiveLine, decimal quantity)
		{
			AssertEquals(nameof(kitReceiveLine.WE_WL), ZGuid.Empty, kitReceiveLine.WE_WL);
			AssertEquals(nameof(kitReceiveLine.WE_CurrentInventoryStatus), InventoryStatus.Codes.Pending, kitReceiveLine.WE_CurrentInventoryStatus);
			AssertEquals(nameof(kitReceiveLine.WE_OriginalInventoryStatus), InventoryStatus.Codes.Pending, kitReceiveLine.WE_OriginalInventoryStatus);
			AssertEquals(nameof(kitReceiveLine.WE_DocketLineStatus), string.Empty, kitReceiveLine.WE_DocketLineStatus);
			AssertEquals(nameof(kitReceiveLine.WE_AdjustmentArrivalDate), ZDateTimeOffset.Empty, kitReceiveLine.WE_AdjustmentArrivalDate);
			AssertEquals(nameof(kitReceiveLine.WE_TransactionQuantity), quantity, kitReceiveLine.WE_TransactionQuantity);
			AssertEquals(nameof(kitReceiveLine.WE_ClientOrderedUnits), quantity, kitReceiveLine.WE_ClientOrderedUnits);
			AssertEquals(nameof(kitReceiveLine.WE_StockOnHand), quantity, kitReceiveLine.WE_StockOnHand);
			AssertEquals(nameof(kitReceiveLine.WE_UnloadedTime), ZDateTimeOffset.Empty, kitReceiveLine.WE_UnloadedTime);
			AssertEquals(nameof(kitReceiveLine.WE_GS_NKUnloadedBy), string.Empty, kitReceiveLine.WE_GS_NKUnloadedBy);
		}

		static void AssertKitLinePutaway(WhsDocketLine kitTransferLine, string inventoryStatus, ZGuid originInventoryPK, ZGuid fromLocation, ZGuid toLocation, decimal quantity, ZString putawayBy, ZDateTimeOffset arrivalDate)
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(kitTransferLine.WE_CurrentInventoryStatus), inventoryStatus, kitTransferLine.WE_CurrentInventoryStatus);
				AssertEquals(nameof(kitTransferLine.WE_OriginalInventoryStatus), InventoryStatus.Codes.Available, kitTransferLine.WE_OriginalInventoryStatus);
				AssertEquals(nameof(kitTransferLine.IsFinalised), true, kitTransferLine.IsFinalised);
				AssertEquals(nameof(kitTransferLine.WE_PutawayTime), ZDateTimeOffset.Now, kitTransferLine.WE_PutawayTime);
				AssertEquals(nameof(kitTransferLine.WE_GS_NKPutawayBy), putawayBy, kitTransferLine.WE_GS_NKPutawayBy);
				AssertEquals(nameof(kitTransferLine.WE_WL_TransferFrom), fromLocation, kitTransferLine.WE_WL_TransferFrom);
				AssertEquals(nameof(kitTransferLine.WE_WL), toLocation, kitTransferLine.WE_WL);
				AssertEquals(nameof(kitTransferLine.WE_TransactionQuantity), quantity, kitTransferLine.WE_TransactionQuantity);
				AssertEquals(nameof(kitTransferLine.WE_StockOnHand), quantity, kitTransferLine.WE_StockOnHand);
				AssertEquals(nameof(kitTransferLine.WE_WE_OriginalDocketLineForRating), originInventoryPK, kitTransferLine.WE_WE_OriginalDocketLineForRating);
				AssertEquals(nameof(kitTransferLine.WE_AdjustmentArrivalDate), arrivalDate, kitTransferLine.WE_AdjustmentArrivalDate);
			});
		}

		static void AssertComponentLinePutaway(WhsDocketLine componentTransferLine, ZString inventoryStatus, ZGuid toLocation, decimal stockOnHand = 0m)
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(componentTransferLine.WE_CurrentInventoryStatus), inventoryStatus, componentTransferLine.WE_CurrentInventoryStatus);
				AssertEquals(nameof(componentTransferLine.IsFinalised), true, componentTransferLine.IsFinalised);
				AssertEquals(nameof(componentTransferLine.WE_PutawayTime), ZDateTimeOffset.Now, componentTransferLine.WE_PutawayTime);
				AssertEquals(nameof(componentTransferLine.WE_WL), toLocation, componentTransferLine.WE_WL);
				AssertEquals(nameof(componentTransferLine.WE_StockOnHand), stockOnHand, componentTransferLine.WE_StockOnHand);
			});
		}

		static void AssertPickLine(WhsPickLine pickLine, ZGuid transactionLinePK, ZGuid inventoryLinePK, ZGuid originalPickedInventoryLinePK, decimal units, ZDateTimeOffset pickedTime, ZString assignedTo)
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(pickLine.WZ_Units), units, pickLine.WZ_Units);
				AssertEquals(nameof(pickLine.WZ_WE_TransactionLine), transactionLinePK, pickLine.WZ_WE_TransactionLine);
				AssertEquals(nameof(pickLine.WZ_WE_InventoryLine), inventoryLinePK, pickLine.WZ_WE_InventoryLine);
				AssertEquals(nameof(pickLine.WZ_WE_OriginalPickedInventoryLine), originalPickedInventoryLinePK, pickLine.WZ_WE_OriginalPickedInventoryLine);
				AssertEquals(nameof(pickLine.WZ_PickedDateTime), pickedTime, pickLine.WZ_PickedDateTime);
				AssertEquals(nameof(pickLine.WZ_GS_NKAssignedTo), assignedTo, pickLine.WZ_GS_NKAssignedTo);
			});
		}

		static void AssertExpectedLocationDetails(PutawayStockInDockDoorOrPackingStationResponse response, WhsLocationInfo location)
		{
			CombineAssertions(() =>
			{
				AssertEquals(nameof(response.ExpectedLocationPK), location?.LocationPK ?? Guid.Empty, response.ExpectedLocationPK);
				AssertEquals(nameof(response.ExpectedLocationClass), location?.LocationClass, response.ExpectedLocationClass);
				AssertEquals(nameof(response.ExpectedLocationString), location?.LocationString, response.ExpectedLocationString);
				AssertEquals(nameof(response.ExpectedLocationString_UserFriendly), location?.LocationString_UserFriendly, response.ExpectedLocationString_UserFriendly);
			});
		}

		static WhsLocationInfo GetWhsLocationInfo(WhsLocation location)
			=> new WhsLocationInfo(location.PK.ToGuid(), location.WLV_LocationString, location.WLV_LocationString_UserFriendly, location.WLV_LocationClass);

		#endregion
	}
}
