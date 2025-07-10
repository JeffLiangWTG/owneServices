using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PutawayPalletTest : WhsSecureServiceTestCase
	{
		#region TestPutawayPallet

		public void TestPutawayPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whs = data.Whs1;
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("ST2", "Staff2");
			var staff3 = Helper.CreateGlbStaff("ST3", "Staff3");

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var part = Helper.CreateProduct(data.Org1, "PART");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, part, 20m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, part, 30m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 40m);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 50m);

			inventory1.WI_PalletID = "12345";
			inventory2.WI_PalletID = "12345";
			inventory3.WI_PalletID = "12345";
			inventory4.WI_PalletID = ZString.Empty;
			inventory5.WI_PalletID = "1234567";

			Helper.Factory.Save();

			AssertNull(inventory1.Location);
			AssertNull(inventory2.Location);
			AssertNull(inventory3.Location);
			AssertNull(inventory4.Location);
			AssertNull(inventory5.Location);

			var webService1 = GetNewWebService(whs, staff1);
			var response1 = webService1.PutawayPallet("12345", location1.ToLocationString(), "12345", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(location1, inventory1.Location);
			AssertEquals(location1, inventory2.Location);
			AssertEquals(location1, inventory3.Location);
			AssertNull(inventory4.Location);
			AssertNull(inventory5.Location);
			AssertPutawayJobAndPutawayLine(palletID: "12345", user: "ST1", whsPK: whs.PK);

			var webService2 = GetNewWebService(whs, staff2);
			var response2 = webService2.PutawayPallet("1234567", location2.ToLocationString(), "1234567", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals(location1, inventory1.Location);
			AssertEquals(location1, inventory2.Location);
			AssertEquals(location1, inventory3.Location);
			AssertNull(inventory4.Location);
			AssertEquals(location2, inventory5.Location);
			AssertPutawayJobAndPutawayLine(palletID: "1234567", user: "ST2", whsPK: whs.PK);

			var webService3 = GetNewWebService(whs, staff3);
			var response3 = webService3.PutawayPallet("12345", location2.ToLocationString(), "12345", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response3, webService3);
			AssertEquals(location2, inventory1.Location);
			AssertEquals(location2, inventory2.Location);
			AssertEquals(location2, inventory3.Location);
			AssertNull(inventory4.Location);
			AssertEquals(location2, inventory5.Location);
			AssertPutawayJobAndPutawayLine(palletID: "12345", user: "ST1", whsPK: whs.PK);
		}

		#endregion

		#region TestPutawayPallet_FinalisedPutawayJobCreated

		[TestDate(2022, 7, 1, 10, 10, 10)]
		public void TestPutawayPallet_FinalisedPutawayJobCreated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var putawayLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_PalletID = "12345";
			Helper.Factory.Save();

			AssertNull(inventory.Location);

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.PutawayPallet("12345", putawayLocation.ToLocationString(), "PLTID", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Putaway Location should be correct.", putawayLocation, inventory.Location);
			AssertEquals("WI_PalletID should be correct.", "PLTID", inventory.WI_PalletID);

			AssertPutawayJobAndPutawayLine(palletID: "12345", user: "S1", whsPK: data.Whs1.PK, finalisedDate: new ZDateTime(2022, 7, 1, 10, 10, 10));
		}

		public static void AssertPutawayJobAndPutawayLine(string palletID, string user, ZGuid whsPK, ZDateTime? finalisedDate = null)
		{
			var jobQuery = new ZDBOnlySubQuery(typeof(WhsPutawayJob), WhsPutawayJobSchema.PK);
			jobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_WW_Warehouse, whsPK);
			jobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_GS_NKUser, user);

			var lineQuery = new ZDBOnlyQuery(typeof(WhsPutawayLine));
			lineQuery.AddToFilter(WhsPutawayLineSchema.WPL_PalletID, palletID);
			lineQuery.AddSubQuery(WhsPutawayLineSchema.WPL_WPJ_PutawayJob, jobQuery, JoinCondition.And);

			var testFactory = new BusinessObjectFactory();
			var putawayLine = testFactory.LoadTop1<WhsPutawayLine>(lineQuery);
			AssertNotNull(putawayLine);
			AssertEquals("IsPuttingAway correct", false, putawayLine.WPL_IsPuttingAway);
			AssertEquals("Is the value of Is Finalised correct", true, putawayLine.WPL_IsFinalized);

			var putawayJob = putawayLine.PutawayJob;
			AssertNotNull(putawayJob);
			AssertEquals("User correct", user, putawayJob.WPJ_GS_NKUser);
			AssertEquals("Warehouse correct", whsPK, putawayJob.WPJ_WW_Warehouse);

			if (finalisedDate != null)
			{
				AssertEquals("Finalised Date correct", finalisedDate, putawayJob.WPJ_FinalizedTimeUtc);
			}
			else
			{
				AssertNotNull(putawayJob.WPJ_FinalizedTimeUtc);
			}
		}

		void AssertNoPutawayLine(string palletID) => AssertNull(Helper.Factory.LoadTop1<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, palletID)));

		public void TestPutawayPallet_FinalisedPutawayJobCreated_CannotSaveException()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var putawayLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_PalletID = "12345";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			webService.Factory.Saving += f => throw new ZCannotSaveException("Test", "TestDesc");
			var response = webService.PutawayPallet("12345", putawayLocation.ToLocationString(), "PLTID", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals("Test", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestPutawayPallet_FinalisedPutawayJobCreated_ZSaveConcurrencyException()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var putawayLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_PalletID = "12345";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)putawayLocation).Row, Db.Connection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			var response = webService.PutawayPallet("12345", putawayLocation.ToLocationString(), "PLTID", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals("Another user has changed the putaway job while you have been creating it. Please restart the operation and try again.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		#endregion

		#region TestPutawayPallet_WithInvalidPalletID

		public void TestPutawayPallet_WithInvalidPalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var part = Helper.CreateProduct(data.Org1, "PART");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, part, 20m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, part, 30m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 40m);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 50m);

			inventory1.WI_PalletID = "12345";
			inventory2.WI_PalletID = "12345";
			inventory3.WI_PalletID = "12345";
			inventory4.WI_PalletID = "";
			inventory5.WI_PalletID = "1234567";

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertBusinessValidationError(webService, "Pallet ID(s) cannot be found.",
				webService.PutawayPallet("123456", location1.ToLocationString(), "123456", isMultiPalletPutaway: false, Guid.Empty));
			AssertNoPutawayLine(palletID: "123456");
		}

		#endregion

		#region TestPutawayPallet_PutawayTransferWithMatchingLines

		public void TestPutawayPallet_PutawayTransferWithMatchingLines()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var putawayLocation = data.Whs1.DefaultLocation;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 20m);
			var inventory3 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "12345", 30m);
			var inventory4 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 40m);
			var inventory5 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "12345", 50m);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var responseForValidateOnPutaway = webService1.ValidatePalletIDOnPutaway("12345", false);
			AssertSuccessfulResponse(responseForValidateOnPutaway, webService1);
			Helper.Factory.Save();

			var putawayTransfer = (WhsTransfer)inventory1.AllPickLines.First().DocketLine.Docket;
			var transferLineQuery = new ZDBOnlyQuery(typeof(WhsTransferLine));
			transferLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, putawayTransfer.PK);
			var transferLines = Helper.Factory.Load<WhsTransferLine>(transferLineQuery);
			AssertEquals("Precondition: Putaway Transfer has 5 Lines.", 5, transferLines.Length);
			AssertEquals("Precondition: Putaway Transfer does not have Matching Lines.", 0, transferLines.Where(l => l.WE_WE_MatchingLine.IsValid).Count());

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					var putawayTransferLines = receiveLines.Select(inv => inv.PutawayTransferLine);
					foreach (var putawayTransferLine in putawayTransferLines)
					{
						putawayTransferLine.WE_WL = putawayLocation.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService();
				SetupSecurityHeader(webService2, data.Whs1, staff1);
				var responseForAllocateLocation = webService2.Putaway_AutoAllocateInventory("12345", "");
			}

			var webService3 = GetNewWebService(data.Whs1, staff1);
			var responseForPutawayPallet = webService3.PutawayPallet("12345", putawayLocation.ToLocationString(), "12345", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(responseForPutawayPallet, webService3);
			AssertEquals("Putaway Transfer is not finalised.", false, putawayTransfer.IsFinalised);
			AssertEquals("Putaway Transfer Lines in correct location.", true, transferLines.All(l => l.WE_WL.Equals(putawayLocation.PK)));
			AssertPutawayJobAndPutawayLine(palletID: "12345", user: "S1", whsPK: data.Whs1.PK);
		}

		#endregion

		#region TestPutawayPallet_WithInvalidLocation

		public void TestPutawayPallet_WithInvalidLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var part = Helper.CreateProduct(data.Org1, "PART");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, part, 20m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, part, 30m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 40m);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 50m);

			inventory1.WI_PalletID = "12345";
			inventory2.WI_PalletID = "12345";
			inventory3.WI_PalletID = "12345";
			inventory4.WI_PalletID = ZString.Empty;
			inventory5.WI_PalletID = "1234567";

			Helper.Factory.Save();
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			AssertBusinessValidationError(webService, "Invalid location", webService.PutawayPallet("12345", "", "12345", isMultiPalletPutaway: false, Guid.Empty));
			AssertNoPutawayLine(palletID: "12345");

			AssertBusinessValidationError(webService, "Invalid location", webService.PutawayPallet("12345", "NON EXISTING LOC", "12345", isMultiPalletPutaway: false, Guid.Empty));
			AssertNoPutawayLine(palletID: "12345");
		}

		#endregion

		#region TestPutawayPallet_LogsAreAddedCorrectly

		public void TestPutawayPallet_LogsAreAddedCorrectly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("ST3", "Staff3");
			var locations = data.Whs1.Rows[0].Locations;
			var part = Helper.CreateProduct(data.Org1, "PR1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, part, 1m, locations[0], "").InDocketLine;
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, part, 2m, null, "PLT-1").InDocketLine;
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, part, 3m, locations[0], "PLT-1").InDocketLine;
			var inventoryLine4 = Helper.CreateWhsReceiveInventoryLine(receive, part, 4m, locations[0], "PLT-2").InDocketLine;

			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService1.PutawayPallet("PLT-1", "A-1", "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);
			AssertPutawayJobAndPutawayLine(palletID: "PLT-1", user: "ST3", whsPK: data.Whs1.PK);

			var receiptConfirmation1 = Helper.FindLogs(inventoryLine1.Logs, Events.WarehouseReceiptConfirmedPutaway);
			var receiptConfirmation2 = Helper.FindLogs(inventoryLine2.Logs, Events.WarehouseReceiptConfirmedPutaway);
			var receiptConfirmation3 = Helper.FindLogs(inventoryLine3.Logs, Events.WarehouseReceiptConfirmedPutaway);
			var receiptConfirmation4 = Helper.FindLogs(inventoryLine4.Logs, Events.WarehouseReceiptConfirmedPutaway);

			AssertEquals(0, receiptConfirmation1.Length);
			AssertEquals(1, receiptConfirmation2.Length);
			AssertEquals(1, receiptConfirmation3.Length);
			AssertEquals(0, receiptConfirmation4.Length);
			AssertEquals("Should populate Event Reference with Putaway Details.", "RF: Putaway for Line 2, Pallet ID PLT-1, Location A-1.", receiptConfirmation2[0].SL_Reference);
			AssertEquals("Should populate Event Reference with Putaway Details.", "RF: Putaway for Line 3, Pallet ID PLT-1, Location A-1.", receiptConfirmation3[0].SL_Reference);

			var event2Time = receiptConfirmation2[0].SL_EventTime;
			var event3Time = receiptConfirmation3[0].SL_EventTime;

			// check that log events are updated
			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.PutawayPallet("PLT-1", "A-2", "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);

			var receiptConfirmation2_2 = Helper.FindLogs(inventoryLine2.Logs, Events.WarehouseReceiptConfirmedPutaway);
			var receiptConfirmation3_2 = Helper.FindLogs(inventoryLine3.Logs, Events.WarehouseReceiptConfirmedPutaway);

			AssertEquals(1, receiptConfirmation2_2.Length);
			AssertEquals(1, receiptConfirmation3_2.Length);

			AssertNotEquals(event2Time, receiptConfirmation2_2[0].SL_EventTime);
			AssertNotEquals(event3Time, receiptConfirmation3_2[0].SL_EventTime);

			AssertEquals("Should populate Event Reference with Putaway Details.", "RF: Putaway for Line 2, Pallet ID PLT-1, Location A-2.", receiptConfirmation2_2[0].SL_Reference);
			AssertEquals("Should populate Event Reference with Putaway Details.", "RF: Putaway for Line 3, Pallet ID PLT-1, Location A-2.", receiptConfirmation3_2[0].SL_Reference);

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response3 = webService3.PutawayPallet("PLT-2", "A-1", "PLT-2", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response3, webService3);

			var receiptConfirmation4_2 = Helper.FindLogs(inventoryLine4.Logs, Events.WarehouseReceiptConfirmedPutaway);
			AssertEquals("Should populate Event Reference with Putaway Details.", "RF: Putaway for Line 4, Pallet ID PLT-2, Location A-1.", receiptConfirmation4_2[0].SL_Reference);
		}

		#endregion

		#region TestPutawayPallet_WithLegacyLogs

		public void TestPutawayPallet_WithLegacyLogs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var locations = data.Whs1.Rows[0].Locations;
			var part = Helper.CreateProduct(data.Org1, "PR1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, part, 4m, locations[0], "PLT-2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService1.PutawayPallet("PLT-2", "A-1", "PLT-2", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);

			var receiptConfirmation = Helper.FindLogs(receiveLine.Logs, Events.WarehouseReceiptConfirmedPutaway);
			AssertEquals("Should populate Event Reference with Putaway Details.", "RF: Putaway for Line 1, Pallet ID PLT-2, Location A-1.", receiptConfirmation[0].SL_Reference);

			// check that behaviour doesn't change for legacy WarehouseReceiptConfirmedPutaway events
			using (receiptConfirmation[0].LockForUpdatingKeyFieldsForTesting())
			{
				receiptConfirmation[0].SL_Reference = "RF"; // Change SL_Reference to mimic a legacy reference prior to extra details being added for WI00055702
			}

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.PutawayPallet("PLT-2", "A-1", "PLT-2", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);

			var receiptConfirmation_2 = Helper.FindLogs(receiveLine.Logs, Events.WarehouseReceiptConfirmedPutaway);
			AssertEquals("Only should be one putaway event active", 1, receiptConfirmation_2.Length);
			AssertEquals("Should populate Event Reference with Putaway Details.", "RF: Putaway for Line 1, Pallet ID PLT-2, Location A-1.", receiptConfirmation_2[0].SL_Reference);
		}

		#endregion

		#region TestPutawayPallet_WarehouseReceiptConfirmedAlternateLocationLogs

		public void TestPutawayPallet_WarehouseReceiptConfirmedAlternateLocationLogs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var part = Helper.CreateProduct(data.Org1, "PR1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, part, 1m, null, "PLT-1").InDocketLine;
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, part, 2m, data.Whs1.FindLocation("A-1"), "PLT-1").InDocketLine;
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, part, 3m, data.Whs1.FindLocation("A-2"), "PLT-1").InDocketLine;

			Helper.Factory.Save();

			var alternateLocationLogs1 = Helper.FindLogs(inventoryLine1.Logs, Events.WarehouseReceiptConfAltLocn);
			var alternateLocationLogs2 = Helper.FindLogs(inventoryLine2.Logs, Events.WarehouseReceiptConfAltLocn);
			var alternateLocationLogs3 = Helper.FindLogs(inventoryLine3.Logs, Events.WarehouseReceiptConfAltLocn);
			AssertEquals("Precondition: alternateLocationLogs1", 0, alternateLocationLogs1.Length);
			AssertEquals("Precondition: alternateLocationLogs2", 0, alternateLocationLogs2.Length);
			AssertEquals("Precondition: alternateLocationLogs3", 0, alternateLocationLogs3.Length);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.PutawayPallet("PLT-1", "A-1", "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			alternateLocationLogs1 = Helper.FindLogs(inventoryLine1.Logs, Events.WarehouseReceiptConfAltLocn);
			alternateLocationLogs2 = Helper.FindLogs(inventoryLine2.Logs, Events.WarehouseReceiptConfAltLocn);
			alternateLocationLogs3 = Helper.FindLogs(inventoryLine3.Logs, Events.WarehouseReceiptConfAltLocn);
			AssertEquals(0, alternateLocationLogs1.Length);
			AssertEquals(0, alternateLocationLogs2.Length);
			AssertEquals(1, alternateLocationLogs3.Length);

			AssertEquals("RF: Allocated Location for Line 3 and Pallet ID PLT-1 was A-2. Putaway confirmed A-1 and Pallet ID PLT-1.", alternateLocationLogs3[0].SL_Reference);
		}

		#endregion

		#region TestPutawayPallet_CycleCountOnAlternatePutaway

		public void TestPutawayPallet_CycleCountOnAlternatePutaway_DifferentLoc_CycleCountOnAlternatePutawayOn()
		{
			TestPutawayPallet_CycleCountOnAlternatePutaway_DifferentLocCore(paramsOn: true, differentLoc: true);
		}

		public void TestPutawayPallet_CycleCountOnAlternatePutaway_DifferentLoc_CycleCountOnAlternatePutawayOff()
		{
			TestPutawayPallet_CycleCountOnAlternatePutaway_DifferentLocCore(paramsOn: false, differentLoc: true);
		}

		public void TestPutawayPallet_CycleCountOnAlternatePutaway_DifferentPLT_CycleCountOnAlternatePutawayOn()
		{
			TestPutawayPallet_CycleCountOnAlternatePutaway_DifferentLocCore(paramsOn: true, differentLoc: false);
		}

		public void TestPutawayPallet_CycleCountOnAlternatePutaway_DifferentPLT_CycleCountOnAlternatePutawayOff()
		{
			TestPutawayPallet_CycleCountOnAlternatePutaway_DifferentLocCore(paramsOn: false, differentLoc: false);
		}

		void TestPutawayPallet_CycleCountOnAlternatePutaway_DifferentLocCore(bool paramsOn, bool differentLoc)
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			receiveCategories.Add("RC2", (NoResString)"Receive Category 2");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(data.Org1, data.Whs1);
			whsClientParameterByWarehouse1.WY_CycleCountOnAlternatePutaway = false;
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			var whsClientParameterByWarehouse2 = Helper.CreateWhsClientParameterByWarehouse(data.Org1, data.Whs1);
			whsClientParameterByWarehouse2.WY_CycleCountOnAlternatePutaway = paramsOn;
			whsClientParameterByWarehouse2.WY_ReceiveCategory = "RC2";
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ReceiveCategory = "RC2";
			var location = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, location, "PLT-1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = differentLoc
				? webService.PutawayPallet("PLT-1", "A-1", "PLT-1", isMultiPalletPutaway: false	, Guid.Empty)
				: webService.PutawayPallet("PLT-1", "A-2", "PLT-5", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			var createCycleCounts = Helper.Factory.Load<WhsCycleCountLocation>(new ZQuery());
			if (paramsOn)
			{
				AssertEquals("Only 1 task should be created", 1, createCycleCounts.Length);

				var cycleCountTask = createCycleCounts[0];
				AssertEquals("Cycle Count 1 should be created to the specified location", location.PK, cycleCountTask.WCL_WL_Location);
				AssertEquals("Cycle Count 1 should be created to the specified granularity", CycleCountGranularity.Codes.ProductWithAttributes, cycleCountTask.WCL_Granularity);
				AssertEquals("Cycle Count 1 should be created to the specified priority", (byte)1, cycleCountTask.WCL_Priority);
			}
			else
			{
				AssertEquals("No tasks should be created", 0, createCycleCounts.Length);
			}
		}

		public void TestPutawayPallet_CycleCountOnAlternatePutaway_DifferentLocation_FromTransfer()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			receiveCategories.Add("RC2", (NoResString)"Receive Category 2");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(data.Org1, data.Whs1);
			whsClientParameterByWarehouse1.WY_CycleCountOnAlternatePutaway = false;
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			var whsClientParameterByWarehouse2 = Helper.CreateWhsClientParameterByWarehouse(data.Org1, data.Whs1);
			whsClientParameterByWarehouse2.WY_CycleCountOnAlternatePutaway = true;
			whsClientParameterByWarehouse2.WY_ReceiveCategory = "RC2";
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ReceiveCategory = "RC2";
			var dockdoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, dockdoorLocation, "PLT-1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PLT-1", isReassigning: false);
			AssertSuccessfulResponse(response1, webService1);
			var transferLine = Helper.Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer)).Single();
			AssertEquals("Precondition: Transfer created", "PLT-1", transferLine.WE_PalletID);

			transferLine.WE_WL = location1.PK;
			Helper.Factory.Save();
			AssertEquals("Precondition: Location A-1 should be allocated", location1.PK, transferLine.WE_WL);

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.PutawayPallet("PLT-1", "A-2", "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);

			var createCycleCounts = Helper.Factory.Load<WhsCycleCountLocation>(new ZQuery());

			AssertEquals("1 task should be created", 1, createCycleCounts.Length);

			var cycleCountTask = createCycleCounts[0];
			AssertEquals("Cycle Count 1 should be created to A-1", location1.PK, cycleCountTask.WCL_WL_Location);
			AssertEquals("Cycle Count 1 should be created to the specified granularity", CycleCountGranularity.Codes.ProductWithAttributes, cycleCountTask.WCL_Granularity);
			AssertEquals("Cycle Count 1 should be created to the specified priority", (byte)1, cycleCountTask.WCL_Priority);
		}

		public void TestPutawayPallet_NotTriggerCycleCountUniqueIndexConflict_WhenReceiveLinesHaveSamePalletID()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(data.Org1, data.Whs1);
			whsClientParameterByWarehouse1.WY_CycleCountOnAlternatePutaway = true;
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ReceiveCategory = "RC1";
			var location = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, location, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, location, "PLT-1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPallet("PLT-1", "A-2", "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);

			var createCycleCounts = Helper.Factory.Load<WhsCycleCountLocation>(new ZQuery());
			AssertEquals("one task should be created successfully", 1, createCycleCounts.Length);

			var cycleCountTask = createCycleCounts[0];
			AssertEquals("Cycle Count 1 should be created to the specified location", location.PK, cycleCountTask.WCL_WL_Location);
		}

		#endregion

		#region TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalised

		public void TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location1, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location1, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location1, "PLT-3");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, location1, "PLT-2");

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m, location1, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m, location1, "PLT-3");

			var receive4 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R4", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive4, data.Part1, 10m, location1, "PLT-3");
			receive4.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive4);

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService1.PutawayPallet("PLT-1", location1.ToLocationString(), "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("None of the receives should be ready for auto-finalize", 0, response1.ReferencesOfReceiveThatCouldBeAutoFinalised.Count);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.PutawayPallet("PLT-2", location1.ToLocationString(), "PLT-2", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals(1, response2.ReferencesOfReceiveThatCouldBeAutoFinalised.Count);
			AssertEquals(true, response2.ReferencesOfReceiveThatCouldBeAutoFinalised.Contains(receive2.WD_DocketID));

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response3 = webService3.PutawayPallet("PLT-3", location1.ToLocationString(), "PLT-3", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response3, webService3);
			AssertEquals(2, response3.ReferencesOfReceiveThatCouldBeAutoFinalised.Count); // shouldn't include Receive4 since it already was finalised.
			AssertEquals(true, response3.ReferencesOfReceiveThatCouldBeAutoFinalised.Contains(receive1.WD_DocketID));
			AssertEquals(true, response3.ReferencesOfReceiveThatCouldBeAutoFinalised.Contains(receive3.WD_DocketID));
		}

		public void TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalised_PlannedReceiveWithTask_WorkingTask()
			=> TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalisedWithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalised_NotPlannedReceiveWithTask_WorkingTask()
			=> TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalisedWithTaskManagement(isPlannedReceive: false, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalised_PlannedReceiveWithTask_WorkingTask_NotUnloadTask()
			=> TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalisedWithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: false, isTaskForOtherUser: true);

		public void TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalised_PlannedReceiveWithTask_NotWorkingTask()
			=> TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalisedWithTaskManagement(isPlannedReceive: true, isWorkingTask: false, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalised_PlannedReceiveWithTask_WorkingTask_AssignedToCurrentUser()
			=> TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalisedWithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: false);

		void TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalisedWithTaskManagement(bool isPlannedReceive, bool isWorkingTask, bool isUnloadTask, bool isTaskForOtherUser)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			if (isPlannedReceive)
			{
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			}
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PLT-2");
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

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.PutawayPallet("PLT-2", location.ToLocationString(), "PLT-2", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(isPlannedReceive && isWorkingTask && isUnloadTask && isTaskForOtherUser ? 0 : 1, response.ReferencesOfReceiveThatCouldBeAutoFinalised.Count);
		}

		public void TestPutawayPallet_ReferencesOfReceiveThatCouldBeAutoFinalisedWithTaskManagement_HasPutawayTransferTaskAssignedToAnotherUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, "PLT-2");
			Helper.Factory.Save();

			var webServiceValidatePalletIDOnPutaway = GetNewWebService(data.Whs1, staff);
			var responseValidatePalletIDOnPutaway = webServiceValidatePalletIDOnPutaway.ValidatePalletIDOnPutaway("PLT-2", false);
			AssertEquals(ErrorTypes.None, responseValidatePalletIDOnPutaway.Error);

			AssertEquals("Precondition: inventory has putaway transfer.", true, receiveLine.HasPutawayTransfer);
			var task = Helper.CreateProcessTaskForTransfer(receiveLine.PutawayTransfer, otherStaff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Helper.Factory.Save();

			AssertEquals("Precondition", true, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", true, task.P9_FormFlowType.EqualsIgnoringCase(WarehouseTaskFormFlowTypes.PutawayJob));
			AssertEquals("Precondition", true, task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working));
			AssertEquals("Precondition", false, receive.IsFinalised);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.PutawayPallet("PLT-2", location.ToLocationString(), "PLT-2", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(0, response.ReferencesOfReceiveThatCouldBeAutoFinalised.Count);
		}

		#endregion

		#region TestPutawayPallet_WithSplitReceives

		public void TestPutawayPallet_WithSplitReceives()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PLT1");
			inventory.WI_SplitQuantity = 2;
			inventory.InDocketLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;

			Helper.Factory.Save();
			receive.SplitReceiptByQuantity();

			var childReceive = (WhsReceive)receive.RelatedSplits.Single();
			var childReceiveInventory = (WhsInventoryView)childReceive.Inventory.Single();
			childReceiveInventory.WI_PalletID = "PLT1";
			childReceiveInventory.InDocketLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			AssertEquals("Precondition", 8m, inventory.WI_InDocketLineUnits);
			AssertEquals("Precondition", "PLT1", inventory.WI_PalletID);
			AssertEquals("Precondition", 2m, childReceiveInventory.WI_InDocketLineUnits);
			AssertEquals("Precondition", "PLT1", childReceiveInventory.WI_PalletID);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService.PutawayPallet("PLT1", "A-1-1", "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response1, webService);
			AssertContainsExactElementsInAnyOrder(new[] { receive.WD_DocketID, childReceive.WD_DocketID }, response1.ReferencesOfReceiveThatCouldBeAutoFinalised);
		}

		#endregion

		#region TestPutawayPallet_WorksForOldLocationBarcodes

		public void TestPutawayPallet_WorksForOldLocationBarcodes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var part = Helper.CreateProduct(data.Org1, "PART");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, part, 20m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, part, 30m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 40m);

			inventory1.WI_PalletID = "12345";
			inventory2.WI_PalletID = "12345";
			inventory3.WI_PalletID = "98765";
			inventory4.WI_PalletID = ZString.Empty;

			Helper.Factory.Save();

			AssertNull(inventory1.Location);
			AssertNull(inventory2.Location);
			AssertNull(inventory3.Location);
			AssertNull(inventory4.Location);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.PutawayPallet("12345", locationA1.OldBarcode, "12345", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(locationA1, inventory1.Location);
			AssertEquals(locationA1, inventory2.Location);
			AssertNull(inventory3.Location);
			AssertNull(inventory4.Location);
		}

		#endregion

		#region TestPutawayPallet_SamePalletIDInMultipleWarehouses

		public void TestPutawayPallet_SamePalletIDInMultipleWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "A", 3, 1);
			var product = Helper.CreateProduct(data.Org1, "PR1");
			Helper.Factory.Save();

			AssertNotNull(data.Whs1.FindLocation("A-2"));
			AssertNotNull(whs2.FindLocation("A-2"));

			var inventoryFinalised = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", product, 10m, data.Whs1.FindLocation("A-1"), "12345").Inventory[0];
			var inventoryNotFinalised = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", product, 10m, null, "12345", false, false).Inventory[0];
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = whs2.WW_WarehouseCode;
			var response = webService1.PutawayPallet("12345", "A-2", "12345", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals("This inventory is in different warehouse and finalised, location should not be changed by putaway logic.", data.Whs1.FindLocation("A-1").PK, inventoryFinalised.WI_WL);
			AssertEquals("This inventory should be allocated to the new location in whs2.", whs2.FindLocation("A-2").PK, inventoryNotFinalised.WI_WL);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertBusinessValidationError(webService2, "This pallet should be putaway into A-1 Pallet ID 12345, as some inventory from this Pallet ID was already finalized into the location.",
				webService2.PutawayPallet("12345", "A-3", "12345", isMultiPalletPutaway: false, Guid.Empty));

			AssertEquals("This inventory is finalised, location should not be changed by putaway logic.", data.Whs1.FindLocation("A-1").PK, inventoryFinalised.WI_WL);
			AssertEquals("This inventory is in different warehouse so location should not be changed.", whs2.FindLocation("A-2").PK, inventoryNotFinalised.WI_WL);
		}

		public void TestPutawayPallet_TransferWithSamePalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var client1 = data.Org1;
			var product = Helper.CreateProduct(data.Org1, "PR1");
			Helper.Factory.Save();

			var inventoryFinalised = Helper.CreateWhsReceiveWithInventory(client1, data.Whs1, "R1", product, 10m, data.Whs1.FindLocation("A-1"), "PID1").Inventory[0];
			var inventoryNotFinalised = Helper.CreateWhsReceiveWithInventory(client1, data.Whs1, "R2", product, 10m, null, "PID2", allocateLocations: false, finalise: false).Inventory[0];
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(client1, data.Whs1, "TR");
			Helper.CreateWhsTransferLineWithInTransitInventory(transfer, product, 10m, data.Whs1.FindLocation("A-1"), "PID1", data.Whs1.FindLocation("A-2"), "PID2", staff1, ZDateTimeOffset.Now);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff1);
			var response = webService.PutawayPallet("PID2", "A-2", "PID2", isMultiPalletPutaway: false, Guid.Empty);

			AssertSuccessfulResponse(response, webService);

			AssertPutawayJobAndPutawayLine(palletID: "PID2", user: "ST1", whsPK: data.Whs1.PK);
		}

		public void TestPutawayPallet_AdjustmentInWithSamePalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			var client1 = data.Org1;
			var product = Helper.CreateProduct(data.Org1, "PR1");
			Helper.Factory.Save();

			var inventoryNotFinalised = Helper.CreateWhsReceiveWithInventory(client1, data.Whs1, "R2", product, 10m, null, "PID1", allocateLocations: false, finalise: false).Inventory[0];
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsAdjustment(client1, data.Whs1, "TR");
			var line = Helper.CreateWhsAdjustmentLine(transfer, product, 10m, data.Whs1.FindLocation("A-1"));
			line.WE_PalletID = "PID1";
			transfer.FinaliseDocket();
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff1);
			var response = webService.PutawayPallet("PID1", "A-2", "PID1", isMultiPalletPutaway: false, Guid.Empty);

			AssertSuccessfulResponse(response, webService);

			AssertPutawayJobAndPutawayLine(palletID: "PID1", user: "ST1", whsPK: data.Whs1.PK);
		}

		#endregion

		#region TestPutawayPallet_DockDoorLocations

		[TestDate(2016, 05, 30)]
		public void TestPutawayPallet_DockDoorLocations_NotCrossDockedInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var client = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(client, data.Part2);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);

			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 10m);
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDOnPutaway("12345", false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);

			var webService2 = GetNewWebService(data.Whs1, staff1);
			var invalidLocationScannedResponse = webService2.PutawayPallet("12345", dockDoorLocation.ToLocationString(), "12345", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals(ErrorTypes.BusinessValidationError, invalidLocationScannedResponse.Error);
			AssertEquals("You cannot putaway to Dock Door locations.", invalidLocationScannedResponse.ErrorMessage);
			AssertNoPutawayLine(palletID: "12345");
		}

		#endregion

		#region TestPutawayPallet_ReceiveArrivalNotSet

		[TestDate(2017, 8, 11, 2, 1, 0)]
		public void TestPutawayPallet_ReceiveArrival_NotSet()
		{
			TestPutawayPallet_ReceiveArrivalNotSetCore(hasArrivalDate: false);
		}

		[TestDate(2017, 8, 11, 2, 1, 0)]
		public void TestPutawayPallet_ReceiveArrival_AlreadySet()
		{
			TestPutawayPallet_ReceiveArrivalNotSetCore(hasArrivalDate: true);
		}

		void TestPutawayPallet_ReceiveArrivalNotSetCore(bool hasArrivalDate)
		{
			var arrivalDate = hasArrivalDate ? ZDateTimeOffset.Now.AddDays(-1) : ZDateTimeOffset.Empty;
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var location = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", arrivalDate);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PLT1");

			Helper.Factory.Save();
			AssertEquals("Precondition", hasArrivalDate ? InventoryStatus.Codes.Arrived : InventoryStatus.Codes.Pending, inventory.InDocketLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", 10m, inventory.InDocketLine.WE_StockOnHand);
			AssertEquals("Precondition", arrivalDate, receive.WD_ArrivalDate);
			AssertEquals("Precondition", arrivalDate, inventory.InDocketLine.WE_AdjustmentArrivalDate);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.PutawayPallet("PLT1", location.ToLocationString(), "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			var receiveLine = new BusinessObjectFactory().Load<WhsReceiveLine>(inventory.InDocketLine.PK);
			AssertEquals($"PutawayPallet should {(hasArrivalDate ? "not " : "")}update recieve arraival date", hasArrivalDate ? ZDateTimeOffset.Now.AddDays(-1) : ZDateTimeOffset.Now, receive.WD_ArrivalDate);
			AssertEquals($"PutawayPallet should {(hasArrivalDate ? "not " : "")}update recieve line with not error", InventoryStatus.Codes.Putaway, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals($"PutawayPallet should {(hasArrivalDate ? "not " : "")}update recieve line with not error", hasArrivalDate ? ZDateTimeOffset.Now.AddDays(-1) : ZDateTimeOffset.Now, receiveLine.WE_AdjustmentArrivalDate);
			AssertEquals($"PutawayPallet should {(hasArrivalDate ? "not " : "")}update recieve line with not error", 10m, inventory.InDocketLine.WE_StockOnHand);
		}

		#endregion

		#region TestPutawayPallet_BondedPallet

		public void TestPutawayPallet_NormalPallet_PutawayToBondedLocation()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var bondedLocation = data.Whs1.FindLocation("A-2");
			bondedLocation.WLV_WA_PutawayArea = bondedArea.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultInboundDockDoorLocation, "PLT1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var responseForValidateOnPutaway = webService1.ValidatePalletIDOnPutaway("PLT1", false);
			AssertSuccessfulResponse(responseForValidateOnPutaway, webService1);
			Helper.Factory.Save();

			AssertEquals("Precondition: Has putaway transfer", true, receiveLine.HasPutawayTransfer);
			AssertEquals("Precondition: Location is in bonded area", true, bondedLocation.IsInBondedArea);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.PutawayPallet("PLT1", bondedLocation.ToLocationString(), "PLT1", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals("Should return error", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should return error", "You cannot putaway stock 'from a non-bonded area to a bonded area' or 'from a bonded area to a non-bonded area'.", response.ErrorMessage);
			AssertNoPutawayLine(palletID: "PLT1");
		}

		public void TestPutawayPallet_BondedPallet_PutawayToNonBondedLocation()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var bondedDockDoor = data.Whs1.FindLocation("A-2");
			bondedDockDoor.WLV_WA_PutawayArea = bondedArea.PK;
			bondedDockDoor.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var locationA1 = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, bondedDockDoor, "PLT1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var responseForValidateOnPutaway = webService1.ValidatePalletIDOnPutaway("PLT1", false);
			AssertSuccessfulResponse(responseForValidateOnPutaway, webService1);
			Helper.Factory.Save();

			AssertEquals("Precondition: Has putaway transfer", true, receiveLine.HasPutawayTransfer);
			AssertEquals("Precondition: Location is not in bonded area", false, locationA1.IsInBondedArea);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.PutawayPallet("PLT1", locationA1.ToLocationString(), "PLT1", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals("Should return error", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should return error", "You cannot putaway stock 'from a non-bonded area to a bonded area' or 'from a bonded area to a non-bonded area'.", response.ErrorMessage);
			AssertNoPutawayLine(palletID: "PLT1");
		}

		public void TestPutawayPallet_BondedPallet()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var bondedDockDoor = data.Whs1.FindLocation("A-1");
			bondedDockDoor.WLV_WA_PutawayArea = bondedArea.PK;
			bondedDockDoor.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var bondedLocation = data.Whs1.FindLocation("A-2");
			bondedLocation.WLV_WA_PutawayArea = bondedArea.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, bondedDockDoor, "PLT1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var responseForValidateOnPutaway = webService1.ValidatePalletIDOnPutaway("PLT1", false);
			AssertSuccessfulResponse(responseForValidateOnPutaway, webService1);
			Helper.Factory.Save();

			var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.PutawayPallet("PLT1", bondedLocation.ToLocationString(), "PLT1", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response, webService2);
			AssertEquals(bondedLocation.ToLocationString(), putawayTransferLine.LocationString);
			AssertPutawayJobAndPutawayLine(palletID: "PLT1", user: "ST1", whsPK: data.Whs1.PK);
		}

		#endregion

		#region TestPutawayPallet_InwardProcessingArea

		public void TestPutawayPallet_InwardProcessingArea()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.Factory.Save();

			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var iprLocation = data.Whs1.FindLocation("A-2");
			iprLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			iprLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultInboundDockDoorLocation, "PLT1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var responseForValidateOnPutaway = webService1.ValidatePalletIDOnPutaway("PLT1", false);
			AssertSuccessfulResponse(responseForValidateOnPutaway, webService1);
			Helper.Factory.Save();

			AssertEquals("Precondition: Has putaway transfer", true, receiveLine.HasPutawayTransfer);
			AssertEquals("Precondition: Location is in inwards processing area", true, iprLocation.IsInInwardProcessingArea);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.PutawayPallet("PLT1", iprLocation.ToLocationString(), "PLT1", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals("Should return error", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should return error", "You cannot putaway stock in an Inward Processing area.", response.ErrorMessage);
			AssertNoPutawayLine(palletID: "PLT1");
		}

		#endregion

		#region TestPutawayPallet_ValidationError

		public void TestPutawayPallet_ValidationError_PutawayTransferHasError()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var location = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultInboundDockDoorLocation, "PLT1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var responseForValidateOnPutaway = webService1.ValidatePalletIDOnPutaway("PLT1", false);
			AssertSuccessfulResponse(responseForValidateOnPutaway, webService1);
			Helper.Factory.Save();

			var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var transferLineInServiceFactory = webService2.Factory.Load<WhsTransferLine>(putawayTransferLine.PK);
			transferLineInServiceFactory.AddRowError("Transfer Line Row Error");

			var response = webService2.PutawayPallet("PLT1", location.ToLocationString(), "PLT1", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response, webService2);
			AssertEquals("Should return error", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should return error", @"Transfer Line Row Error", response.ErrorMessage);
			AssertNoPutawayLine(palletID: "PLT1");
		}

		#endregion

		#region TestPutawayPallet_WE_WPL_PutawayLine

		public void TestPutawayPallet_WE_WPL_PutawayLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result1 = webService1.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine.HasPutawayTransfer);

			var putawayTransfer = receiveLine.PutawayTransfer;
			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);
			var webService2 = GetNewWebService(data.Whs1);
			var result2 = webService2.PutawayPallet("PLT-1", "A-2", "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);

			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);
			var putawayLine = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, "PLT-1")).First();
			AssertEquals("WE_WPL_PutawayLine correct", putawayLine.PK, putawayTransfer.Lines[0].WE_WPL_PutawayLine);
		}

		public void TestPutawayPallet_WE_WPL_PutawayLine_ConsolidatedPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result1 = webService1.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine.HasPutawayTransfer);

			var putawayTransfer = receiveLine.PutawayTransfer;
			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);
			var webService2 = GetNewWebService(data.Whs1);
			var result2 = webService2.PutawayPallet("PLT-1", "A-2", "PLT-2", isMultiPalletPutaway: false, Guid.Empty);
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);

			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);
			var putawayLine = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, "PLT-1")).First();
			AssertEquals("WE_WPL_PutawayLine correct", putawayLine.PK, putawayTransfer.Lines[0].WE_WPL_PutawayLine);
		}

		#endregion

		#region TestPutawayPallet_PutawayTransferFinalisation

		public void TestPutawayPallet_PutawayTransferFinalisation_AllReceiveInventoriesArePutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result1 = webService1.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine.HasPutawayTransfer);

			var putawayTransfer = receiveLine.PutawayTransfer;
			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);
			var webService2 = GetNewWebService(data.Whs1);
			var result2 = webService2.PutawayPallet("PLT-1", "A-2", "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);

			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);
			var putawayLine = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_PalletID, "PLT-1")).First();
			AssertEquals("WE_WPL_PutawayLine correct", putawayLine.PK, putawayTransfer.Lines[0].WE_WPL_PutawayLine);
		}

		public void TestPutawayPallet_PutawayTransferFinalisation_NotAllReceiveInventoriesArePutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-2");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine1.HasPutawayTransfer);
			AssertEquals("Precondition", false, receiveLine2.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result1 = webService1.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine1.HasPutawayTransfer);

			var putawayTransfer = receiveLine1.PutawayTransfer;
			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);
			var webService2 = GetNewWebService(data.Whs1);
			var result2 = webService2.PutawayPallet("PLT-1", "A-2", "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);

			AssertEquals("Receive line 2 is not yet putaway.", false, receiveLine2.HasPutawayTransfer);
			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);
		}

		public void TestPutawayPallet_PutawayTransferFinalisation_AllInventoriesOnOneReceiveArePutaway()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);

			var data = new TestDataSimpleEnvironment(helper.Factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive1Line1 = helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			var receive1Line2 = helper.CreateWhsReceiveLine(receive1, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-SAME");

			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receive2Line1 = helper.CreateWhsReceiveLine(receive2, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-2");
			var receive2Line2 = helper.CreateWhsReceiveLine(receive2, data.Part1, 40m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-SAME");
			helper.Factory.Save();

			AssertEquals("Precondition", false, receive1Line1.HasPutawayTransfer);
			AssertEquals("Precondition", false, receive1Line2.HasPutawayTransfer);
			AssertEquals("Precondition", false, receive2Line1.HasPutawayTransfer);
			AssertEquals("Precondition", false, receive2Line2.HasPutawayTransfer);
			var result1 = webService.ValidatePalletIDOnPutaway("PLT-SAME", false);
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receive1Line2.HasPutawayTransfer);
			AssertEquals("Receive line has putaway transfer.", true, receive2Line2.HasPutawayTransfer);

			var putawayTransfer = receive1Line2.PutawayTransfer;
			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);
			var result2 = webService.PutawayPallet("PLT-SAME", "A-2", "PLT-SAME", isMultiPalletPutaway: false, Guid.Empty);
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);
			AssertEquals("No receive can be finalised yet.", false, result2.ReferencesOfReceiveThatCouldBeAutoFinalised.Any());

			AssertEquals(false, receive1Line1.HasPutawayTransfer);
			AssertEquals(true, receive1Line2.HasPutawayTransfer);
			AssertEquals(false, receive2Line1.HasPutawayTransfer);
			AssertEquals(true, receive2Line2.HasPutawayTransfer);
			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);

			var result3 = webService.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertNull("No errors from webservice call.", result3.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result3.Error);
			AssertEquals("Receive line has putaway transfer.", true, receive1Line1.HasPutawayTransfer);

			AssertEquals("Receive lines have the same putaway transfer", putawayTransfer.PK, receive1Line1.PutawayTransfer.PK);

			var result4 = webService.PutawayPallet("PLT-1", "A-2", "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertNull("No errors from webservice call.", result4.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result4.Error);

			AssertEquals(true, receive1Line1.HasPutawayTransfer);
			AssertEquals(true, receive1Line2.HasPutawayTransfer);
			AssertEquals(false, receive2Line1.HasPutawayTransfer);
			AssertEquals(true, receive2Line2.HasPutawayTransfer);
			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);
			AssertContainsExactElementsInAnyOrder("R1 can be finalised.", new[] { receive1.WD_DocketID }, result4.ReferencesOfReceiveThatCouldBeAutoFinalised);

			var putawayTransferLine1 = PutawayHelper.GetPutawayTransferLineFromInventory(receive1Line1.Inventory[0]);
			AssertEquals("Putaway as receive is not yet finalised.", InventoryStatus.Codes.Putaway, putawayTransferLine1.WE_CurrentInventoryStatus);

			var putawayTransferLine2 = PutawayHelper.GetPutawayTransferLineFromInventory(receive1Line2.Inventory[0]);
			AssertEquals("Putaway as receive is not yet finalised.", InventoryStatus.Codes.Putaway, putawayTransferLine2.WE_CurrentInventoryStatus);

			var putawayTransferLine3 = PutawayHelper.GetPutawayTransferLineFromInventory(receive2Line2.Inventory[0]);
			AssertEquals("Putaway as receive is not yet finalised.", InventoryStatus.Codes.Putaway, putawayTransferLine3.WE_CurrentInventoryStatus);

			AssertEquals("Received, no putaway transfer yet.", InventoryStatus.Codes.Received, receive2Line1.WE_CurrentInventoryStatus);
			helper.Factory.Save();

			receive1.FinaliseDocketWithoutUserConfirmation();

			Assert("Receive is finalised.", receive1.IsFinalised);
			AssertEquals("Putaway transfer is finalised.", true, putawayTransfer.IsFinalised);
			AssertEquals("Available as receive is finalised.", InventoryStatus.Codes.Available, putawayTransferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Available as receive is finalised.", InventoryStatus.Codes.Available, putawayTransferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Putaway as receive is not yet finalised.", InventoryStatus.Codes.Putaway, putawayTransferLine3.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestPutawayPallet_DifferentPutawayPalletId

		public void TestPutawayPallet_DifferentPutawayPalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var putawayLocation = data.Whs1.FindLocation("A-1");
			var part = Helper.CreateProduct(data.Org1, "PART");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 10m);
			inventory.WI_PalletID = "PLT";
			Helper.Factory.Save();

			AssertNull(inventory.Location);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.PutawayPallet("PLT", putawayLocation.ToLocationString(), "6789", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(putawayLocation, inventory.Location);
			AssertEquals("6789", inventory.WI_PalletID);

			var alternatePalletIdLog = Helper.FindLogs(receive.Lines[0].Logs, Events.WarehouseReceiptConfAltLocn);
			AssertEquals("RF: Allocated Location for Line 1 and Pallet ID PLT was A-1. Putaway confirmed A-1 and Pallet ID 6789.", alternatePalletIdLog[0].SL_Reference);
			AssertPutawayJobAndPutawayLine(palletID: "PLT", user: "ST1", whsPK: data.Whs1.PK);
		}

		public void TestPutawayPallet_DifferentPutawayPalletId_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result1 = webService1.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine.HasPutawayTransfer);

			var putawayTransferLine = receiveLine.PutawayTransferLine;
			putawayTransferLine.WE_PalletID = "PLT-2";
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var result2 = webService2.PutawayPallet("PLT-1", "A-2", "PLT-2", isMultiPalletPutaway: false, Guid.Empty);
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);

			AssertEquals("A-2", putawayTransferLine.LocationString);
			AssertEquals("PLT-2", putawayTransferLine.WE_PalletID);
			AssertPutawayJobAndPutawayLine(palletID: "PLT-1", user: "ST1", whsPK: data.Whs1.PK);
			AssertEquals("No 'putaway pallet id is different from allocated pallet id' log.", false, Helper.FindLogs(receiveLine.Logs, Events.WarehouseReceiptConfAltLocn).Any());
		}

		public void TestPutawayPallet_DifferentPutawayPalletId_OverriddenAllocatedPalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result1 = webService1.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine.HasPutawayTransfer);

			var putawayTransfer = receiveLine.PutawayTransfer;
			AssertEquals("Putaway transfer is not finalised.", false, putawayTransfer.IsFinalised);

			var putawayTransferLine = receiveLine.PutawayTransferLine;
			putawayTransferLine.WE_PalletID = "PLT-2";
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var result2 = webService2.PutawayPallet("PLT-1", "A-2", "PLT-3", isMultiPalletPutaway: false, Guid.Empty);
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);

			AssertEquals("A-2", putawayTransferLine.LocationString);
			AssertEquals("PLT-3", putawayTransferLine.WE_PalletID);

			var alternatePalletIdLog = Helper.FindLogs(receiveLine.Logs, Events.WarehouseReceiptConfAltLocn);
			AssertEquals("RF: Allocated Location for Line 1 and Pallet ID PLT-2 was A-2. Putaway confirmed A-2 and Pallet ID PLT-3.", alternatePalletIdLog[0].SL_Reference);
		}

		public void TestPutawayPallet_DifferentPutawayPalletId_PutawayTransferWithMatchingLines()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var putawayLocation = data.Whs1.DefaultLocation;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 20m);
			var inventory3 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "12345", 30m);
			var inventory4 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 40m);
			var inventory5 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "12345", 50m);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var responseForValidateOnPutaway = webService1.ValidatePalletIDOnPutaway("12345", false);
			AssertSuccessfulResponse(responseForValidateOnPutaway, webService1);
			Helper.Factory.Save();

			var putawayTransfer = (WhsTransfer)inventory1.AllPickLines.First().DocketLine.Docket;
			var transferLineQuery = new ZDBOnlyQuery(typeof(WhsTransferLine));
			transferLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, putawayTransfer.PK);
			var transferLines = Helper.Factory.Load<WhsTransferLine>(transferLineQuery);
			AssertEquals("Precondition: Putaway Transfer has 5 Lines.", 5, transferLines.Length);
			AssertEquals("Precondition: Putaway Transfer does not have Matching Lines.", 0, transferLines.Where(l => l.WE_WE_MatchingLine.IsValid).Count());

			var webService2 = GetNewWebService(data.Whs1, staff1);
			var responseForPutawayPallet = webService2.PutawayPallet("12345", putawayLocation.ToLocationString(), "6789", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(responseForPutawayPallet, webService2);
			AssertEquals("Putaway Transfer is not finalised.", false, putawayTransfer.IsFinalised);
			AssertEquals("Putaway Transfer Lines in correct location.", true, transferLines.All(l => l.WE_WL.Equals(putawayLocation.PK)));
			AssertEquals("Putaway Transfer Lines in correct location.", true, transferLines.All(l => l.WE_PalletID.EqualsIgnoringCase("6789")));
		}

		public void TestPutawayPallet_FinalisedInventory_PutawayToDifferentPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var product = Helper.CreateProduct(data.Org1, "PR1");
			Helper.Factory.Save();

			var inventoryFinalised = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", product, 10m, data.Whs1.FindLocation("A-1"), "12345").Inventory[0];
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);

			webService.ValidatePalletIDOnPutaway("12345", false);

			AssertBusinessValidationError(webService,
				"This pallet should be putaway into A-1 Pallet ID 12345, as some inventory from this Pallet ID was already finalized into the location.",
				webService.PutawayPallet("12345", "A-1", "6789", isMultiPalletPutaway: false, Guid.Empty));

			AssertEquals("This inventory is finalised, location should not be changed by putaway logic.", data.Whs1.FindLocation("A-1").PK, inventoryFinalised.WI_WL);
			AssertEquals("This inventory is finalised, location should not be changed by putaway logic.", "12345", inventoryFinalised.WI_PalletID);
		}

		public void TestPutawayPallet_DifferentPutawayPalletId_PalletIdDoesNotExist()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result1 = webService1.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine.HasPutawayTransfer);

			var putawayTransferLine = receiveLine.PutawayTransferLine;
			putawayTransferLine.WE_PalletID = "PLT-2";
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			AssertBusinessValidationError(webService2, "Pallet ID(s) cannot be found.", webService2.PutawayPallet("PLT-3", "A-2", "PLT-3", isMultiPalletPutaway: false, Guid.Empty));
		}

		public void TestPutawayPallet_DifferentPutawayPalletId_FinalisedInventoryExcluded()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);

			var data = new TestDataSimpleEnvironment(helper.Factory, 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine1.HasPutawayTransfer);
			var result1 = webService.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine1.HasPutawayTransfer);

			var putawayTransferLine1 = receiveLine1.PutawayTransferLine;
			putawayTransferLine1.WE_PalletID = "PLT-2";
			helper.Factory.Save();

			var result2 = webService.PutawayPallet("PLT-1", "A-2", "PLT-2", isMultiPalletPutaway: false, Guid.Empty);
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);

			AssertEquals("A-2", putawayTransferLine1.LocationString);
			AssertEquals("PLT-2", putawayTransferLine1.WE_PalletID);
			AssertEquals("PLT-1", putawayTransferLine1.WE_TransferFromPalletId);
			AssertEquals(true, putawayTransferLine1.IsFinalised);

			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive1.IsFinalised);

			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			helper.Factory.Save();

			var result3 = webService.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertNull("No errors from webservice call.", result3.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result3.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine2.HasPutawayTransfer);

			var putawayTransferLine2 = receiveLine2.PutawayTransferLine;
			putawayTransferLine2.WE_PalletID = "PLT-3";
			helper.Factory.Save();

			var result4 = webService.PutawayPallet("PLT-1", "A-2", "PLT-3", isMultiPalletPutaway: false, Guid.Empty);
			AssertNull("No errors from webservice call.", result4.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result4.Error);

			AssertEquals("A-2", putawayTransferLine1.LocationString);
			AssertEquals("PutawayTransferLine1 did not change.", "PLT-2", putawayTransferLine1.WE_PalletID);

			AssertEquals("A-2", putawayTransferLine2.LocationString);
			AssertEquals("PLT-3", putawayTransferLine2.WE_PalletID);
			AssertEquals("PLT-1", putawayTransferLine2.WE_TransferFromPalletId);
			AssertEquals(true, putawayTransferLine2.IsFinalised);
		}

		public void TestPutawayPallet_DifferentPutawayPalletId_PutawayTransfer_ErrorOnFinalise()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var location = data.Whs1.FindLocation("A-3");
			var finalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location, "PLT-2", finalise: true);
			AssertIsFinalisedPrecondition(finalisedReceive);
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result1 = webService1.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine.HasPutawayTransfer);

			var putawayTransferLine = receiveLine.PutawayTransferLine;
			putawayTransferLine.WE_PalletID = "PLT-2";
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var result2 = webService2.PutawayPallet("PLT-1", "A-2", "PLT-2", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals("Web service call returned an error.", "Another location (A-3) was already used for the same Pallet ID. Please select another location or Pallet ID.", result2.ErrorMessage);
			AssertEquals("Web service call returned an error.", ErrorTypes.BusinessValidationError, result2.Error);
			AssertEquals("Putaway transfer line didn't get finalised.", false, putawayTransferLine.IsFinalised);
		}

		#endregion

		#region TestPutawayPallet_MultiPalletPutaway

		public void TestPutawayPallet_MultiPalletPutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL3");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2", "PL3" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine5 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine5 location should be empty.", transferLine3.Location);

			var putawayJob = PutawayHelper.LoadUnfinalisedPutawayJob(Helper.Factory, data.Whs1, staff);
			AssertNotNull(nameof(putawayJob), putawayJob);
			AssertEquals("Precondition: PutawayJob not finalised", false, putawayJob.IsFinalised);
			AssertEquals("Precondition: PutawayJob has 3 lines", 3, putawayJob.Lines.Count);
			var sortedPutawayLines = putawayJob.Lines.OrderBy(l => l.WPL_PalletID);
			var putawayLinePL1 = sortedPutawayLines.First();
			AssertEquals("Precondition: PutawayLinePL1 palletID correct", "PL1", putawayLinePL1.WPL_PalletID);
			AssertEquals("Precondition: PutawayLinePL1 WPL_IsPuttingAway true", true, putawayLinePL1.WPL_IsPuttingAway);
			AssertEquals("Precondition: PutawayLinePL1 WPL_IsFinalised false", false, putawayLinePL1.WPL_IsFinalized);

			var putawayLinePL2 = sortedPutawayLines.Skip(1).First();
			AssertEquals("Precondition: PutawayLinePL2 palletID correct", "PL2", putawayLinePL2.WPL_PalletID);
			AssertEquals("Precondition: PutawayLinePL2 WPL_IsPuttingAway true", true, putawayLinePL2.WPL_IsPuttingAway);
			AssertEquals("Precondition: PutawayLinePL2 WPL_IsFinalised false", false, putawayLinePL2.WPL_IsFinalized);

			var putawayLinePL3 = sortedPutawayLines.Last();
			AssertEquals("Precondition: PutawayLinePL3 palletID correct", "PL3", putawayLinePL3.WPL_PalletID);
			AssertEquals("Precondition: PutawayLinePL3 WPL_IsPuttingAway true", true, putawayLinePL3.WPL_IsPuttingAway);
			AssertEquals("Precondition: PutawayLinePL3 WPL_IsFinalised false", false, putawayLinePL3.WPL_IsFinalized);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var result2 = webService2.PutawayPallet("PL1", "A-2", "PLT-1", isMultiPalletPutaway: true, Guid.Empty);
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);

			AssertEquals("TransferLine1 Location correct", "A-2", transferLine1.LocationString);
			AssertEquals("TransferLine1 PalletID correct", "PLT-1", transferLine1.WE_PalletID);
			AssertEquals("PutawayLinePL1 WPL_IsPuttingAway false", false, putawayLinePL1.WPL_IsPuttingAway);
			AssertEquals("PutawayLinePL1 WPL_IsFinalised correct", true, putawayLinePL1.WPL_IsFinalized);
			AssertEquals("PutawayJob IsFinalised should be false", false, putawayJob.IsFinalised);
			AssertEquals("TransferLine1 is finalised", true, transferLine1.IsFinalised);
			AssertEquals("TransferLine1 WE_WPL_PutawayLine is correct", putawayLinePL1.PK, transferLine1.WE_WPL_PutawayLine);

			var webService3 = GetNewWebService(data.Whs1, staff);
			var result3 = webService3.PutawayPallet("PL2", "A-1", "PLT-2", isMultiPalletPutaway: true, Guid.Empty);
			AssertNull("No errors from webservice call.", result3.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result3.Error);

			AssertEquals("TransferLine1 Location correct", "A-1", transferLine2.LocationString);
			AssertEquals("TransferLine1 PalletID correct", "PLT-2", transferLine2.WE_PalletID);
			AssertEquals("PutawayLinePL2 WPL_IsPuttingAway false", false, putawayLinePL2.WPL_IsPuttingAway);
			AssertEquals("PutawayLinePL2 WPL_IsFinalised correct", true, putawayLinePL2.WPL_IsFinalized);
			AssertEquals("PutawayJob IsFinalised should be false", false, putawayJob.IsFinalised);
			AssertEquals("TransferLine2 is finalised", true, transferLine2.IsFinalised);
			AssertEquals("TransferLine2 WE_WPL_PutawayLine is correct", putawayLinePL2.PK, transferLine2.WE_WPL_PutawayLine);

			var webService4 = GetNewWebService(data.Whs1, staff);
			var result4 = webService4.PutawayPallet("PL3", "A-3", "PLT-3", isMultiPalletPutaway: true, Guid.Empty);
			AssertNull("No errors from webservice call.", result4.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result4.Error);

			AssertEquals("TransferLine3 Location correct", "A-3", transferLine3.LocationString);
			AssertEquals("TransferLine3 PalletID correct", "PLT-3", transferLine3.WE_PalletID);
			AssertEquals("PutawayLinePL3 WPL_IsPuttingAway false", false, putawayLinePL3.WPL_IsPuttingAway);
			AssertEquals("PutawayLinePL3 WPL_IsFinalised correct", true, putawayLinePL3.WPL_IsFinalized);
			AssertEquals("TransferLine3 is finalised", true, transferLine3.IsFinalised);
			AssertEquals("TransferLine3 WE_WPL_PutawayLine is correct", putawayLinePL3.PK, transferLine3.WE_WPL_PutawayLine);

			AssertEquals("PutawayJob IsFinalised should be true", true, putawayJob.IsFinalised);
		}

		public void TestPutawayPallet_MultiPalletPutaway_NoPutawayJob()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockdoorLocation, null, "PL1", 10m);
			transferLine.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var result = webService.PutawayPallet("PL1", "A-2", "PLT-1", isMultiPalletPutaway: true, Guid.Empty);
			AssertEquals("Error should be correct.", "Pallets putaway during a multiple pallet putaway must have a putaway job.", result.ErrorMessage);
			AssertEquals("Should be BusinessValidationError", ErrorTypes.BusinessValidationError, result.Error);
		}

		public void TestPutawayPallet_MultiPalletPutaway_NoPutawayLine()
		{
			TestPutawayPallet_MultiPalletPutaway_NoPutawayLineCore(job => { });
		}

		public void TestPutawayPallet_MultiPalletPutaway_NoPutawayLine_WrongPalletID()
		{
			TestPutawayPallet_MultiPalletPutaway_NoPutawayLineCore(job =>
			{
				Helper.CreateWhsPutawayLine(job, "PL2");
			});
		}

		void TestPutawayPallet_MultiPalletPutaway_NoPutawayLineCore(Action<WhsPutawayJob> createLine)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockdoorLocation, null, "PL1", 10m);
			transferLine.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			createLine(Helper.CreateWhsPutawayJob(data.Whs1, staff));
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var result = webService.PutawayPallet("PL1", "A-2", "PLT-1", isMultiPalletPutaway: true, Guid.Empty);
			AssertEquals("Error should be correct.", "Pallets putaway during a multiple pallet putaway must have valid un-finalized putaway line.", result.ErrorMessage);
			AssertEquals("Should be BusinessValidationError", ErrorTypes.BusinessValidationError, result.Error);
		}

		public void TestPutawayPallet_MultiPalletPutaway_NoUnfinalisedPutawayLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockdoorLocation, null, "PL1", 10m);
			transferLine.RunPreSaveValidation(); // to commit inventory

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff);
			var putawayLine = Helper.CreateWhsPutawayLine(putawayJob, "PL1");
			putawayLine.WPL_IsFinalized = true;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var result = webService.PutawayPallet("PL1", "A-2", "PLT-1", isMultiPalletPutaway: true, Guid.Empty);
			AssertEquals("Error should be correct.", "Pallets putaway during a multiple pallet putaway must have valid un-finalized putaway line.", result.ErrorMessage);
			AssertEquals("Should be BusinessValidationError", ErrorTypes.BusinessValidationError, result.Error);
		}

		public void TestPutawayPallet_MultiPalletPutaway_FinalisedReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine5 location should be empty.", transferLine2.Location);

			var putawayJob = PutawayHelper.LoadUnfinalisedPutawayJob(Helper.Factory, data.Whs1, staff);
			AssertNotNull(nameof(putawayJob), putawayJob);
			AssertEquals("Precondition: PutawayJob not finalised", false, putawayJob.IsFinalised);
			AssertEquals("Precondition: PutawayJob has only 1 line", 1, putawayJob.Lines.Count);
			var putawayLinePL1 = putawayJob.Lines.First();
			AssertEquals("Precondition: PutawayLinePL1 palletID correct", "PL1", putawayLinePL1.WPL_PalletID);
			AssertEquals("Precondition: PutawayLinePL1 WPL_IsPuttingAway true", true, putawayLinePL1.WPL_IsPuttingAway);
			AssertEquals("Precondition: PutawayLinePL1 WPL_IsFinalised false", false, putawayLinePL1.WPL_IsFinalized);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var result2 = webService2.PutawayPallet("PL1", "A-2", "PLT-1", isMultiPalletPutaway: true, Guid.Empty);
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);
			AssertEquals("Receives to finalise count correct", 2, result2.ReferencesOfReceiveThatCouldBeAutoFinalised.Count);
			AssertEquals("Receive1 DocketID in list", true, result2.ReferencesOfReceiveThatCouldBeAutoFinalised.Contains(receive1.WD_DocketID));
			AssertEquals("Receive2 DocketID in list", true, result2.ReferencesOfReceiveThatCouldBeAutoFinalised.Contains(receive2.WD_DocketID));

			AssertEquals("TransferLine1 Location correct", "A-2", transferLine1.LocationString);
			AssertEquals("TransferLine1 PalletID correct", "PLT-1", transferLine1.WE_PalletID);
			AssertEquals("TransferLine1 is finalised", true, transferLine1.IsFinalised);
			AssertEquals("TransferLine1 WE_WPL_PutawayLine is correct", putawayLinePL1.PK, transferLine1.WE_WPL_PutawayLine);

			AssertEquals("TransferLine2 Location correct", "A-2", transferLine2.LocationString);
			AssertEquals("TransferLine2 PalletID correct", "PLT-1", transferLine2.WE_PalletID);
			AssertEquals("TransferLine2 is finalised", true, transferLine2.IsFinalised);
			AssertEquals("TransferLine2 WE_WPL_PutawayLine is correct", putawayLinePL1.PK, transferLine2.WE_WPL_PutawayLine);

			AssertEquals("PutawayLinePL1 WPL_IsPuttingAway false", false, putawayLinePL1.WPL_IsPuttingAway);
			AssertEquals("PutawayLinePL1 WPL_IsFinalised correct", true, putawayLinePL1.WPL_IsFinalized);
			AssertEquals("PutawayJob IsFinalised should be false", true, putawayJob.IsFinalised);
		}

		#endregion

		#region TestPutawayPallet_AreAllTranferLineFinalized

		public void TestPutawayPallet_AreAllPutawayTransfersUnfinalized()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PL1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PL1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);
			AssertEquals("Precondition: inventory1 has putaway transfer.", true, inventory1.HasPutawayTransfer);

			var transferLine1 = receive.Lines[1].PutawayTransferLine;
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			Helper.Factory.Save();

			var response2 = webService1.PutawayPallet("PL1", location.ToLocationString(), "PL1", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Putaway has been already completed for the Pallet ID PL1.", response2.ErrorMessage);
		}

		#endregion

		#region TestPutawayPallet_CrossDock

		public void TestPutawayPallet_CrossDock()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine = order.Lines[0].ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory.WI_WL = data.Whs1.DefaultInboundDockDoorLocation.PK;
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PutawayPallet("PL1", "XDOCK", "PL1", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals(ErrorTypes.None, response2.Error);
			AssertPutawayJobAndPutawayLine(palletID: "PL1", user: "ST1", whsPK: data.Whs1.PK);
		}

		public void TestPutawayPallet_CrossDock_NotAllReceiveLinesAreCrossDocked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, null, "PL1");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine = order.Lines[0].ReserveStockIfAbleTo(inventory1);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory1.WI_WL = data.Whs1.DefaultInboundDockDoorLocation.PK;
			inventory2.WI_WL = data.Whs1.DefaultInboundDockDoorLocation.PK;
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory1.HasPutawayTransfer);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PutawayPallet("PL1", "XDOCK", "PL1", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Pallet 'PL1' cannot be put away to the cross dock location as some inventories on this pallet are not cross docked.", response2.ErrorMessage);
		}

		public void TestPutawayPallet_CrossDock_WrongLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine = order.Lines[0].ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory.WI_WL = data.Whs1.DefaultInboundDockDoorLocation.PK;
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response1.Error);

			AssertEquals("Precondition: inventory has putaway transfer.", true, inventory.HasPutawayTransfer);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PutawayPallet("PL1", "A", "PL1", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Putaway location is not the same as the Order Cross Dock Location.", response2.ErrorMessage);
		}

		#endregion

		#region TestPutawayPallet_Task

		public void TestPutawayPallet_Task()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff = Helper.CreateGlbStaff("S1", "S1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part2, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT2");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT1", 10m);
			transferLine.RunPreSaveValidation();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.PutawayPallet("PLT1", "A-2", "PLT1", isMultiPalletPutaway: false, task.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("TransferLine1 Location correct", "A-2", transferLine.LocationString);
			AssertEquals("TransferLine1 is finalised", true, transferLine.IsFinalised);
			AssertEquals("Task status should be Closed", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}

		public void TestPutawayPallet_Task_MultiplePallets()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff = Helper.CreateGlbStaff("S1", "S1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part2, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT2");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT1", 10m);
			transferLine1.RunPreSaveValidation();
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part2, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT2", 10m);
			transferLine2.RunPreSaveValidation();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.PutawayPallet("PLT1", "A-2", "PL11", isMultiPalletPutaway: false, task.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("TransferLine1 Location correct", "A-2", transferLine1.LocationString);
			AssertEquals("TransferLine1 is finalised", true, transferLine1.IsFinalised);
			AssertEquals("TransferLine2 Location correct", string.Empty, transferLine2.LocationString);
			AssertEquals("TransferLine2 is finalised", false, transferLine2.IsFinalised);
			AssertEquals("Task status should be still Working", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void TestPutawayPallet_Single_TaskAssignedToOtherUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part2, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT2");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT1", 10m);
			transferLine.RunPreSaveValidation();

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff2);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.PutawayPallet("PLT1", "A-2", "PLT1", isMultiPalletPutaway: false, task.PK.ToGuid());
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("This task is not assigned to the current user. Please perform a different task.", response.ErrorMessage);

			AssertEquals("TransferLine1 Location correct", string.Empty, transferLine.LocationString);
			AssertEquals("TransferLine1 is NOT finalised", false, transferLine.IsFinalised);
			AssertEquals("Task status should NOT be Closed", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void TestPutawayPallet_Single_Task_InvalidTaskPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT1", 10m);
			transferLine.RunPreSaveValidation();

			var invalidTask = Helper.CreateProcessTaskForTransfer(putawayTransfer);
			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff2);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.PutawayPallet("PLT1", "A-2", "PLT1", isMultiPalletPutaway: false, invalidTask.PK.ToGuid());
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("The task is currently unassigned to any user and cannot be updated. Please check the task status and try again.", response.ErrorMessage);

			AssertEquals("TransferLine1 Location correct", string.Empty, transferLine.LocationString);
			AssertEquals("TransferLine1 is NOT finalised", false, transferLine.IsFinalised);
			AssertEquals("Task status should NOT be Closed", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		#endregion
	}
}
