using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ManualPutawayMultiplePalletTest : WhsSecureServiceTestCase
	{
		#region TestManualPutawayMultiplePallet

		public void TestManualPutawayMultiplePallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whs = data.Whs1;
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var location1 = data.Whs1.FindLocation("A-1");
			var part = Helper.CreateProduct(data.Org1, "PART");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 40m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 50m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			inventory3.WI_PalletID = "003";
			var palletIDsToPutaway = new string[] { inventory1.WI_PalletID, inventory2.WI_PalletID, inventory3.WI_PalletID };
			Helper.Factory.Save();

			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(whs, staff1, palletIDsToPutaway);

			AssertNull((inventory1.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory2.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory3.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			var inventories = new WhsInventoryView[] { inventory1, inventory2, inventory3 };

			var webService1 = GetNewWebService(whs, staff1);
			var response1 = webService1.ManualPutawayMultiplePallet(location1.ToLocationString());

			AssertSuccessfulResponse(response1, webService1);
			palletIDsToPutaway.ForEach(x => PutawayPalletTest.AssertPutawayJobAndPutawayLine(palletID: x, user: "ST1", whsPK: whs.PK));
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertEquals(location1, transferLine.Location);
				var putawayLine = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, x.WI_PalletID)).Single();
				AssertEquals("WE_WPL_PutawayLine correct", putawayLine.PK, transferLine.WE_WPL_PutawayLine);
			});
		}

		public void TestManualPutawayMultiplePallet_ConsolidatedPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var part = Helper.CreateProduct(data.Org1, "PART");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 40m, data.Whs1.DefaultInboundDockDoorLocation, "PL2");
			Helper.Factory.Save();

			var palletIDsToPutaway = new string[] { "PL1", "PL2" };
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff, palletIDsToPutaway);

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLines = new WhsTransferLine[] { transferLine1, transferLine2 };
			transferLines.ForEach(transferLine =>
			{
				AssertNull("Precondition: TransferLine location should be empty.", transferLine.Location);
			});

			// Allocation/Consolidate Pallet 1
			var location1 = data.Whs1.FindLocation("A-1");
			transferLine1.WE_WL = location1.PK;
			transferLine1.WE_PalletID = "PLT5";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ManualPutawayMultiplePallet(location1.ToLocationString());
			AssertSuccessfulResponse(response, webService);
			AssertNoResponseError(response);
			palletIDsToPutaway.ForEach(x => PutawayPalletTest.AssertPutawayJobAndPutawayLine(palletID: x, user: "ST1", whsPK: data.Whs1.PK));

			transferLines.ForEach(transferLine =>
			{
				AssertEquals(location1, transferLine.Location);
				var putawayLine = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, transferLine.WE_TransferFromPalletId)).Single();
				AssertEquals("WE_WPL_PutawayLine correct", putawayLine.PK, transferLine.WE_WPL_PutawayLine);
			});
		}

		#endregion

		#region TestManualPutawayMultiplePallet_FinalisedPutawayJobCreated

		[TestDate(2022, 7, 1, 10, 10, 10)]
		public void TestManualPutawayMultiplePallet_FinalisedPutawayJobCreated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var putawayLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			var palletIDsToPutaway = new string[] { inventory1.WI_PalletID, inventory2.WI_PalletID };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff1, palletIDsToPutaway);
			AssertNull((inventory1.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory2.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			var inventories = new WhsInventoryView[] { inventory1, inventory2 };

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ManualPutawayMultiplePallet(putawayLocation.ToLocationString());

			AssertSuccessfulResponse(response, webService);
			palletIDsToPutaway.ForEach(x => PutawayPalletTest.AssertPutawayJobAndPutawayLine(palletID: x, user: "S1", whsPK: data.Whs1.PK, finalisedDate: new ZDateTime(2022, 7, 1, 10, 10, 10)));
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertEquals(putawayLocation, transferLine.Location);
				var putawayLine = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, x.WI_PalletID)).Single();
				AssertEquals("WE_WPL_PutawayLine correct", putawayLine.PK, transferLine.WE_WPL_PutawayLine);
			});
		}

		public void TestManualPutawayMultiplePallet_FinalisedPutawayJobCreated_CannotSaveException()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var putawayLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			var palletIDsToPutaway = new string[] { inventory1.WI_PalletID, inventory2.WI_PalletID };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff1, palletIDsToPutaway);
			AssertNull((inventory1.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory2.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			var inventories = new WhsInventoryView[] { inventory1, inventory2 };

			var webService = GetNewWebService(data.Whs1, staff1);
			webService.Factory.Saving += f => throw new ZCannotSaveException("Test", "TestDesc");
			var response = webService.ManualPutawayMultiplePallet(putawayLocation.ToLocationString());

			AssertEquals("Test", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertNull(transferLine.Location);
				AssertEquals(ZGuid.Empty, transferLine.WE_WPL_PutawayLine);
			});
		}

		public void TestManualPutawayMultiplePallet_FinalisedPutawayJobCreated_ZSaveConcurrencyException()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var putawayLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			var palletIDsToPutaway = new string[] { inventory1.WI_PalletID, inventory2.WI_PalletID };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff1, palletIDsToPutaway);
			AssertNull((inventory1.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory2.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			var inventories = new WhsInventoryView[] { inventory1, inventory2 };

			var webService = GetNewWebService(data.Whs1, staff1);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)putawayLocation).Row, Db.Connection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);
			var response = webService.ManualPutawayMultiplePallet(putawayLocation.ToLocationString());

			AssertEquals("Another user has changed the putaway job while you have been creating it. Please restart the operation and try again.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertNull(transferLine.Location);
				AssertEquals(ZGuid.Empty, transferLine.WE_WPL_PutawayLine);
			});
		}

		#endregion

		#region TestManualPutawayMultiplePallet_PalletInAnotherLocation

		public void TestManualPutawayMultiplePallet_PalletAlreadyInAnotherLocation_LineNotFinalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var part = Helper.CreateProduct(data.Org1, "PART");
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 20m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 30m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			inventory3.WI_PalletID = "003";
			var palletIDsToPutaway = new string[] { inventory1.WI_PalletID, inventory2.WI_PalletID, inventory3.WI_PalletID };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff1, palletIDsToPutaway);
			var receiveLine = inventory3.InDocketLine as WhsReceiveLine;
			var putawaytransferline = receiveLine.PutawayTransferLine;
			putawaytransferline.LocationString = "A-2";
			AssertEquals("A-2", putawaytransferline.LocationString);
			AssertEquals(false, putawaytransferline.IsFinalised);
			Helper.Factory.Save();
			AssertNull((inventory1.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory2.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertEquals("A-2", (inventory3.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location.ToLocationString());
			var inventories = new WhsInventoryView[] { inventory1, inventory2, inventory3 };

			var webService = GetNewWebService(data.Whs1, staff1);

			var response1 = webService.ManualPutawayMultiplePallet(location1.ToLocationString());
			AssertSuccessfulResponse(response1, webService);
			palletIDsToPutaway.ForEach(x => PutawayPalletTest.AssertPutawayJobAndPutawayLine(palletID: x, user: "ST1", whsPK: data.Whs1.PK));
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertEquals(location1, transferLine.Location);
				var putawayLine = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, x.WI_PalletID)).Single();
				AssertEquals("WE_WPL_PutawayLine correct", putawayLine.PK, transferLine.WE_WPL_PutawayLine);
			});
		}

		#endregion

		#region TestManualPutawayMultiplePallet_WithInvalidLocation

		public void TestManualPutawayMultiplePallet_LocationNotGiven()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var part = Helper.CreateProduct(data.Org1, "PART");
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 20m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			var palletIDsToPutaway = new string[] { inventory1.WI_PalletID, inventory2.WI_PalletID };
			var inventories = new WhsInventoryView[] { inventory1, inventory2 };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff1, palletIDsToPutaway);

			var webService = GetNewWebService(data.Whs1, staff1);

			AssertBusinessValidationError(webService, "Invalid location",
				webService.ManualPutawayMultiplePallet(""));
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertNull(transferLine.Location);
				AssertEquals(ZGuid.Empty, transferLine.WE_WPL_PutawayLine);
			});
		}

		public void TestManualPutawayMultiplePallet_LocationIsDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var part = Helper.CreateProduct(data.Org1, "PART");
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 20m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			var palletIDsToPutaway = new string[] { inventory1.WI_PalletID, inventory2.WI_PalletID };
			var inventories = new WhsInventoryView[] { inventory1, inventory2 };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff1, palletIDsToPutaway);

			var webService = GetNewWebService(data.Whs1, staff1);

			AssertBusinessValidationError(webService, "You cannot putaway to Dock Door locations.",
				webService.ManualPutawayMultiplePallet(location1.ToLocationString()));
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertNull(transferLine.Location);
				AssertEquals(ZGuid.Empty, transferLine.WE_WPL_PutawayLine);
			});
		}

		public void TestManualPutawayMultiplePallet_LocationDoesNotExist()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var part = Helper.CreateProduct(data.Org1, "PART");
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 20m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			var palletIDsToPutaway = new string[] { inventory1.WI_PalletID, inventory2.WI_PalletID };
			var inventories = new WhsInventoryView[] { inventory1, inventory2 };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff1, palletIDsToPutaway);

			var webService = GetNewWebService(data.Whs1, staff1);

			AssertBusinessValidationError(webService, "Invalid location",
				webService.ManualPutawayMultiplePallet("B-1"));
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertNull(transferLine.Location);
				AssertEquals(ZGuid.Empty, transferLine.WE_WPL_PutawayLine);
			});
		}

		public void TestManualPutawayMultiplePallet_LocationDoesNotHaveRequiredCapacity()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_MaxQuantity = 25;
			var part = Helper.CreateProduct(data.Org1, "PART");
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 20m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			var palletIDsToPutaway = new string[] { inventory1.WI_PalletID, inventory2.WI_PalletID };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff1, palletIDsToPutaway);
			AssertNull((inventory1.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory2.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			var inventories = new WhsInventoryView[] { inventory1, inventory2 };

			var webService = GetNewWebService(data.Whs1, staff1);

			AssertBusinessValidationError(webService, "Putaway quantity 30.000 will exceed current available location capacity 25.000",
				webService.ManualPutawayMultiplePallet(location1.ToLocationString()));
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertNull(transferLine.Location);
				AssertEquals(ZGuid.Empty, transferLine.WE_WPL_PutawayLine);
			});
		}

		public void TestManualPutawayMultiplePallet_NormalPallet_BondedLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var bondedLocation = data.Whs1.FindLocation("A-2");
			bondedLocation.WLV_WA_PutawayArea = bondedArea.PK;
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var part = Helper.CreateProduct(data.Org1, "PART");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveLine(receive1, part, 5m, data.Whs1.DefaultInboundDockDoorLocation, "PLT1");
			var inventory2 = Helper.CreateWhsReceiveLine(receive1, part, 15m, data.Whs1.DefaultInboundDockDoorLocation, "PLT2");
			var palletIDsToPutaway = new string[] { inventory1.WE_PalletID, inventory2.WE_PalletID };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff, palletIDsToPutaway);
			AssertNull(inventory1.PutawayTransferLine.Location);
			AssertNull(inventory2.PutawayTransferLine.Location);
			AssertEquals("Precondition: Location is in bonded area", true, bondedLocation.IsInBondedArea);
			var inventories = new WhsReceiveLine[] { inventory1, inventory2 };

			var webService = GetNewWebService(data.Whs1, staff);

			AssertBusinessValidationError(webService, "You cannot putaway stock 'from a non-bonded area to a bonded area' or 'from a bonded area to a non-bonded area'.",
				webService.ManualPutawayMultiplePallet(bondedLocation.ToLocationString()));
			inventories.ForEach(x =>
			{
				var transferLine = x.PutawayTransferLine;
				AssertNull(transferLine.Location);
				AssertEquals(ZGuid.Empty, transferLine.WE_WPL_PutawayLine);
			});
		}

		public void TestManualPutawayMultiplePallet_BondedPallet_NonBondedLocation()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var bondedDockDoor = data.Whs1.FindLocation("A-2");
			bondedDockDoor.WLV_WA_PutawayArea = bondedArea.PK;
			bondedDockDoor.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonBondedLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, bondedDockDoor, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, bondedDockDoor, "PLT2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 52m, bondedDockDoor, "PLT3");
			var palletIDsToPutaway = new string[] { receiveLine1.WE_PalletID, receiveLine2.WE_PalletID, receiveLine3.WE_PalletID };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff, palletIDsToPutaway);
			AssertEquals("Precondition: Location is not in bonded area", false, nonBondedLocation.IsInBondedArea);
			AssertNull(receiveLine1.PutawayTransferLine.Location);
			AssertNull(receiveLine2.PutawayTransferLine.Location);
			AssertNull(receiveLine3.PutawayTransferLine.Location);
			var inventories = new WhsReceiveLine[] { receiveLine1, receiveLine2, receiveLine3 };

			var webService = GetNewWebService(data.Whs1, staff);

			AssertBusinessValidationError(webService, "You cannot putaway stock 'from a non-bonded area to a bonded area' or 'from a bonded area to a non-bonded area'.",
				webService.ManualPutawayMultiplePallet(nonBondedLocation.ToLocationString()));

			inventories.ForEach(x =>
			{
				var transferLine = x.PutawayTransferLine;
				AssertNull(transferLine.Location);
				AssertEquals(ZGuid.Empty, transferLine.WE_WPL_PutawayLine);
			});
		}

		#endregion

		#region TestManualPutawayMultiplePallet_InwardProcessingArea

		public void TestManualPutawayMultiplePallet_InwardProcessingArea()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var iprLocation = data.Whs1.FindLocation("A-2");
			iprLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			iprLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var part = Helper.CreateProduct(data.Org1, "PART");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveLine(receive1, part, 5m, data.Whs1.DefaultInboundDockDoorLocation, "PLT1");
			var inventory2 = Helper.CreateWhsReceiveLine(receive1, part, 15m, data.Whs1.DefaultInboundDockDoorLocation, "PLT2");
			var palletIDsToPutaway = new string[] { inventory1.WE_PalletID, inventory2.WE_PalletID };
			Helper.Factory.Save();

			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(data.Whs1, staff, palletIDsToPutaway);
			AssertNull(inventory1.PutawayTransferLine.Location);
			AssertNull(inventory2.PutawayTransferLine.Location);
			AssertEquals("Precondition: Location is in inward processing area", true, iprLocation.IsInInwardProcessingArea);

			var inventories = new WhsReceiveLine[] { inventory1, inventory2 };
			var webService = GetNewWebService(data.Whs1, staff);

			AssertBusinessValidationError(webService, "You cannot putaway stock in an Inward Processing area.",
				webService.ManualPutawayMultiplePallet(iprLocation.ToLocationString()));
			inventories.ForEach(x =>
			{
				var transferLine = x.PutawayTransferLine;
				AssertNull(transferLine.Location);
				AssertEquals(ZGuid.Empty, transferLine.WE_WPL_PutawayLine);
			});
		}

		#endregion

		#region TestManualPutawayMultiplePallet_MultipleDockets

		public void TestManualPutawayMultiplePallet_MultipleDockets()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whs = data.Whs1;
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var location1 = data.Whs1.FindLocation("A-1");

			var beer = Helper.CreateProduct(data.Org1, "BEER");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, beer, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, beer, 40m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var wine = Helper.CreateProduct(data.Org1, "WINE");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, wine, 50m);
			inventory3.WI_PalletID = "003";

			var palletIDsToPutaway = new string[] { inventory1.WI_PalletID, inventory2.WI_PalletID, inventory3.WI_PalletID };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(whs, staff1, palletIDsToPutaway);
			AssertNull((inventory1.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory2.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory3.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			var inventories = new WhsInventoryView[] { inventory1, inventory2, inventory3 };

			var webService1 = GetNewWebService(whs, staff1);
			var response1 = webService1.ManualPutawayMultiplePallet(location1.ToLocationString());

			AssertSuccessfulResponse(response1, webService1);
			palletIDsToPutaway.ForEach(x => PutawayPalletTest.AssertPutawayJobAndPutawayLine(palletID: x, user: "ST1", whsPK: whs.PK));
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertEquals(location1, transferLine.Location);
				var putawayLine = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, x.WI_PalletID)).Single();
				AssertEquals("WE_WPL_PutawayLine correct", putawayLine.PK, transferLine.WE_WPL_PutawayLine);
			});
		}

		#endregion

		#region TestManualPutawayMultiplePallet_PutawayJobNotCreated

		public void TestManualPutawayMultiplePallet_PutawayJobNotCreated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var part = Helper.CreateProduct(data.Org1, "PART");
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 20m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			var inventories = new WhsInventoryView[] { inventory1, inventory2 };
			Helper.Factory.Save();
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertNull(transferLine);
			});

			var webService = GetNewWebService(data.Whs1, staff1);

			AssertBusinessValidationError(webService, "Putaway Job and Putaway Transfer Lines not found.",
				webService.ManualPutawayMultiplePallet("A-1"));
		}

		#endregion

		#region TestManualPutawayMultiplePallet_ReferencesOFReceiveToAutoFinalise

		public void TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinalise()
		{
			//Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whs = data.Whs1;
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var location1 = data.Whs1.FindLocation("A-1");
			var part = Helper.CreateProduct(data.Org1, "PART");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 40m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 50m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			inventory3.WI_PalletID = "003";

			// Iteration 1 - put away just 1 pallet id

			var palletIDsToPutaway = new string[] { inventory1.WI_PalletID };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(whs, staff1, palletIDsToPutaway);
			AssertNull((inventory1.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			var inventories = new WhsInventoryView[] { inventory1 };

			var webService1 = GetNewWebService(whs, staff1);
			var response1 = webService1.ManualPutawayMultiplePallet(location1.ToLocationString());

			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(0, response1.ReferencesOfReceiveThatCouldBeAutoFinalised.Count);
			palletIDsToPutaway.ForEach(x => PutawayPalletTest.AssertPutawayJobAndPutawayLine(palletID: x, user: "ST1", whsPK: whs.PK));
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertEquals(location1, transferLine.Location);
				var putawayLine = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, x.WI_PalletID)).Single();
				AssertEquals("WE_WPL_PutawayLine correct", putawayLine.PK, transferLine.WE_WPL_PutawayLine);
			});

			// Iteration 2 - putaway remaining pallet ids

			var palletIDsToPutaway2 = new string[] { inventory2.WI_PalletID, inventory3.WI_PalletID };
			Helper.Factory.Save();
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(whs, staff1, palletIDsToPutaway2);
			AssertNull((inventory2.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory3.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			var inventories2 = new WhsInventoryView[] { inventory2, inventory3 };

			var webService2 = GetNewWebService(whs, staff1);
			var response2 = webService2.ManualPutawayMultiplePallet(location1.ToLocationString());

			AssertSuccessfulResponse(response2, webService2);
			AssertEquals(1, response2.ReferencesOfReceiveThatCouldBeAutoFinalised.Count);
			palletIDsToPutaway2.ForEach(x => PutawayPalletTest.AssertPutawayJobAndPutawayLine(palletID: x, user: "ST1", whsPK: whs.PK));
			inventories2.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertEquals(location1, transferLine.Location);
				var putawayLine = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, x.WI_PalletID)).Single();
				AssertEquals("WE_WPL_PutawayLine correct", putawayLine.PK, transferLine.WE_WPL_PutawayLine);
			});
		}

		public void TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinalise_PlannedReceive_WithTask_WorkingTask()
			=> TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinaliseWithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinalise_NotPlannedReceive_WithTask_WorkingTask()
			=> TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinaliseWithTaskManagement(isPlannedReceive: false, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinalise_PlannedReceive_WithTask_NotWorkingTask()
			=> TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinaliseWithTaskManagement(isPlannedReceive: true, isWorkingTask: false, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinalise_PlannedReceive_WithTask_WorkingTask_NotUnloadTask()
			=> TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinaliseWithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: false, isTaskForOtherUser: true);

		public void TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinalise_NotPlannedReceive_WithTask_AssignedToUser()
			=> TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinaliseWithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: false);

		void TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinaliseWithTaskManagement(bool isPlannedReceive, bool isWorkingTask, bool isUnloadTask, bool isTaskForOtherUser)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whs = data.Whs1;
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var otherStaff = Helper.CreateGlbStaff("ST2", "Staff2");
			var location = data.Whs1.FindLocation("A-1");
			var part = Helper.CreateProduct(data.Org1, "PART");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			if (isPlannedReceive)
			{
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			}
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, part, 40m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, part, 50m);
			inventory1.WI_PalletID = "001";
			inventory2.WI_PalletID = "002";
			inventory3.WI_PalletID = "003";
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, isTaskForOtherUser ? otherStaff : staff);
			if (isWorkingTask)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			}
			if (!isUnloadTask)
			{
				task.P9_FormFlowType = string.Empty;
			}
			Helper.Factory.Save();

			AssertEquals("Precondition", isPlannedReceive, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", isUnloadTask, task.P9_FormFlowType.EqualsIgnoringCase(WarehouseTaskFormFlowTypes.UnloadJob));
			AssertEquals("Precondition", isWorkingTask, task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working));
			AssertEquals("Precondition", false, receive.IsFinalised);

			var palletIDsToPutaway = new string[] { "001", "002", "003" };
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(whs, staff, palletIDsToPutaway);
			AssertNull((inventory1.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory2.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);
			AssertNull((inventory3.InDocketLine as WhsReceiveLine).PutawayTransferLine.Location);

			var webService = GetNewWebService(whs, staff);
			var response = webService.ManualPutawayMultiplePallet(location.ToLocationString());
			AssertSuccessfulResponse(response, webService);

			var receiveCannotFinalise = isPlannedReceive && isWorkingTask && isUnloadTask && isTaskForOtherUser;
			AssertEquals(receiveCannotFinalise ? 0 : 1, response.ReferencesOfReceiveThatCouldBeAutoFinalised.Count);
		}

		public void TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinaliseWithTaskManagement_HasActiveWorkingPutawayTransferTaskAssignedToAnotherUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whs = data.Whs1;
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var otherStaff = Helper.CreateGlbStaff("ST2", "Staff2");
			var location = data.Whs1.FindLocation("A-1");
			var part = Helper.CreateProduct(data.Org1, "PART");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			var inventory1 = Helper.CreateWhsReceiveLine(receive, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveLine(receive, part, 40m);
			var inventory3 = Helper.CreateWhsReceiveLine(receive, part, 50m);
			inventory1.WE_PalletID = "001";
			inventory2.WE_PalletID = "002";
			inventory3.WE_PalletID = "003";
			Helper.Factory.Save();

			var palletIDsToPutaway = new string[] { "001", "002", "003" };
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(whs, staff, palletIDsToPutaway);
			AssertNull(inventory1.PutawayTransferLine.Location);
			AssertNull(inventory2.PutawayTransferLine.Location);
			AssertNull(inventory3.PutawayTransferLine.Location);

			var task = Helper.CreateProcessTaskForTransfer(inventory1.PutawayTransfer, otherStaff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Helper.Factory.Save();

			AssertEquals("Precondition", true, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", true, task.P9_FormFlowType.EqualsIgnoringCase(WarehouseTaskFormFlowTypes.PutawayJob));
			AssertEquals("Precondition", true, task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working));
			AssertEquals("Precondition", false, receive.IsFinalised);

			var webService = GetNewWebService(whs, staff);
			var response = webService.ManualPutawayMultiplePallet(location.ToLocationString());
			AssertSuccessfulResponse(response, webService);

			AssertEquals(0, response.ReferencesOfReceiveThatCouldBeAutoFinalised.Count);
		}

		public void TestManualPutawayMultiplePallet_ReferencesOfReceiveToAutoFinaliseWithTaskManagement_MultipleReceives_PutawayTaskOnAnotherReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whs = data.Whs1;
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var otherStaff = Helper.CreateGlbStaff("ST2", "Staff2");
			var location = data.Whs1.FindLocation("A-1");
			var part = Helper.CreateProduct(data.Org1, "PART");
			Helper.Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			receive1.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			var inventory1 = Helper.CreateWhsReceiveLine(receive1, part, 10m);
			inventory1.WE_PalletID = "001";
			var inventory2 = Helper.CreateWhsReceiveLine(receive1, part, 40m);
			inventory2.WE_PalletID = "002";

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			receive2.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			var inventory3 = Helper.CreateWhsReceiveLine(receive2, part, 50m);
			inventory3.WE_PalletID = "003";
			var inventory4 = Helper.CreateWhsReceiveLine(receive2, part, 60m);
			inventory4.WE_PalletID = "004";

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW3");
			receive3.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			var inventory5 = Helper.CreateWhsReceiveLine(receive3, part, 80m);
			inventory5.WE_PalletID = "005";
			var inventory6 = Helper.CreateWhsReceiveLine(receive3, part, 90m);
			inventory6.WE_PalletID = "006";
			Helper.Factory.Save();

			var palletIDsToPutaway = new string[] { "001", "002", "003", "004", "005", "006" };
			ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(whs, staff, palletIDsToPutaway);
			AssertNull(inventory1.PutawayTransferLine.Location);
			AssertNull(inventory2.PutawayTransferLine.Location);
			AssertNull(inventory3.PutawayTransferLine.Location);
			AssertNull(inventory4.PutawayTransferLine.Location);
			AssertNull(inventory5.PutawayTransferLine.Location);
			AssertNull(inventory6.PutawayTransferLine.Location);

			var task = Helper.CreateProcessTaskForTransfer(inventory1.PutawayTransfer, otherStaff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Helper.Factory.Save();

			AssertEquals("Precondition", true, receive1.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", true, receive2.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", true, receive3.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", true, task.P9_FormFlowType.EqualsIgnoringCase(WarehouseTaskFormFlowTypes.PutawayJob));
			AssertEquals("Precondition", true, task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working));
			AssertEquals("Precondition", false, receive1.IsFinalised);
			AssertEquals("Precondition", false, receive2.IsFinalised);
			AssertEquals("Precondition", false, receive3.IsFinalised);

			var webService = GetNewWebService(whs, staff);
			var response = webService.ManualPutawayMultiplePallet(location.ToLocationString());
			AssertSuccessfulResponse(response, webService);

			AssertContainsExactElementsInAnyOrder(new[] { receive2.WD_DocketID, receive3.WD_DocketID }, response.ReferencesOfReceiveThatCouldBeAutoFinalised);
		}

		#endregion

		#region TestManualPutawayMultiplePallet_CrossDock

		public void TestManualPutawayMultiplePallet_CrossDock()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, null, "PL2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order1.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine1 = order1.Lines[0].ReserveStockIfAbleTo(inventory1);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine1.ReservedQuantity);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 15m);
			order2.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine2 = order2.Lines[0].ReserveStockIfAbleTo(inventory2);
			AssertEquals("Precondition: Stock is reserved.", 15m, reservedLine2.ReservedQuantity);
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory1.Location);
			AssertNull("Precondition: location should be empty.", inventory2.Location);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Inventory has putaway transfer.", true, inventory1.HasPutawayTransfer);
			AssertEquals("Inventory has putaway transfer.", true, inventory2.HasPutawayTransfer);

			var inventories = new WhsInventoryView[] { inventory1, inventory2 };

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.ManualPutawayMultiplePallet("XDOCK");

			AssertSuccessfulResponse(response1, webService1);
			new[] { "PL1", "PL2" }.ForEach(x => PutawayPalletTest.AssertPutawayJobAndPutawayLine(palletID: x, user: "ST1", whsPK: data.Whs1.PK));
			inventories.ForEach(x =>
			{
				var transferLine = (x.InDocketLine as WhsReceiveLine).PutawayTransferLine;
				AssertEquals(crossDockLocation, transferLine.Location);
				var putawayLine = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, x.WI_PalletID)).Single();
				AssertEquals("WE_WPL_PutawayLine correct", putawayLine.PK, transferLine.WE_WPL_PutawayLine);
			});
		}

		public void TestManualPutawayMultiplePallet_CrossDock_NotAllReceiveLinesAreCrossDocked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, null, "PL1");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, null, "PL2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order1.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine1 = order1.Lines[0].ReserveStockIfAbleTo(inventory1);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine1.ReservedQuantity);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			order2.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine2 = order2.Lines[0].ReserveStockIfAbleTo(inventory3);
			AssertEquals("Precondition: Stock is reserved.", 20m, reservedLine2.ReservedQuantity);
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory1.Location);
			AssertNull("Precondition: location should be empty.", inventory2.Location);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Inventory has putaway transfer.", true, inventory1.HasPutawayTransfer);
			AssertEquals("Inventory has putaway transfer.", true, inventory2.HasPutawayTransfer);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.ManualPutawayMultiplePallet("XDOCK");
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Pallet 'PL1' cannot be put away to the cross dock location as some inventories on this pallet are not cross docked.", response2.ErrorMessage);
		}

		#endregion

		#region HelperMethods

		void ValidatePalletIdsAndCreatePutawayTransferLinesAndPutawayJob(WhsWarehouse whs, GlbStaff staff, string[] palletIds)
		{
			var webService1 = GetNewWebService(whs, staff);
			var responseForValidateOnPutaway = webService1.ValidatePalletIDsForMultiplePutaway(palletIds, false);
			AssertSuccessfulResponse(responseForValidateOnPutaway, webService1);
			Helper.Factory.Save();
		}

		#endregion
	}
}
