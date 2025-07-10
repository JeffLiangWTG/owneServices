using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WhsPutawayJobDO = CargoWise.Database.TestFramework.ObjectModel.WhsPutawayJob;
using WhsPutawayLineDO = CargoWise.Database.TestFramework.ObjectModel.WhsPutawayLine;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	abstract class ValidatePalletIDOnPutawaySharedTest : WhsSecureServiceTestCase
	{
		#region Abstract Properties and Methods

		protected abstract WebServiceResponse GetValidatePalletIDWebServiceResponse(WhsSecureService secureService, string palletID);

		#endregion

		#region TestValidatePalletIDOnPutaway_MultipleWarehouses

		public void TestValidatePalletIDOnPutaway_MultipleWarehouses()
		{
			var whs1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "A", 2, 1);
			var client = Helper.CreateClient();
			var product = Helper.CreateProduct(client, "P1");

			// create finalised receive with pallet "12345"
			Helper.CreateWhsReceiveWithInventory(client, whs1, "R1", product, 10m, whs1.DefaultLocation, "12345");
			// create not finalised receive in another warehouse with same palletID
			var receive2 = Helper.CreateWhsReceiveWithInventory(client, whs2, "R2", product, 7m, null, "12345", false, false);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(whs1);
			var response1 = GetValidatePalletIDWebServiceResponse(webService1, "12345");
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(UnfinalisedReceiptError(), response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			var webService2 = GetNewWebService(whs2);
			GetValidatePalletIDWebServiceResponse(webService2, "12345");
			var transferLines = Helper.Factory.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer));
			AssertEquals("Should create only one putaway transferLine", 1, transferLines.Length);
			AssertEquals("Should be for receive2", receive2.Inventory.Single().PK.ToGuid(), transferLines[0].PickLines[0].WZ_WE_InventoryLine);
		}

		protected abstract string UnfinalisedReceiptError();

		#endregion

		#region TestValidatePalletIDOnPutaway_CreatePutawayTransfers

		[TestDate(2016, 05, 30)]
		public void TestValidatePalletIDOnPutaway_CreatePutawayTransfers()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var nonDockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation2 = data.Whs1.FindLocation("A-2");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "12345", 20m);
			var inventory3 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "12345", 30m);
			var inventory4 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "", 40m);
			var inventory5 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "1234567", 50m);
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory3.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory4.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory5.WI_InventoryStatus);

			var webService1 = GetNewWebService(data.Whs1, staff1);
			// Validate Pallet ID On Putaway - First scan of the pallet Id
			var responseForValidateOnPutaway = GetValidatePalletIDWebServiceResponse(webService1, "12345");
			AssertSuccessfulResponse(responseForValidateOnPutaway, webService1);
			AssertInventoryStatusAndEventCount(inventory1, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory2, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory3, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory4, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory5, InventoryStatus.Codes.Received);

			var transfer = (WhsTransfer)inventory1.AllPickLines.First().DocketLine.Docket;
			AssertPutawayTransfer(transfer, data.Org1, data.Whs1, 3);

			var transferLineForPart1 = (WhsTransferLine)transfer.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertPutawayTransferLine(transferLineForPart1, data.Part1, dockDoorLocation, null, "12345", 10m, staff1, ZDateTimeOffset.Now);

			var transferLineForPart2 = (WhsTransferLine)transfer.Lines.Single(l => l.WE_OP == data.Part2.PK && l.WE_TransactionQuantity == 20m);
			AssertPutawayTransferLine(transferLineForPart2, data.Part2, dockDoorLocation, null, "12345", 20m, staff1, ZDateTimeOffset.Now);

			var transferLineForPart3 = (WhsTransferLine)transfer.Lines.Single(l => l.WE_OP == data.Part2.PK && l.WE_TransactionQuantity == 30m);
			AssertPutawayTransferLine(transferLineForPart3, data.Part2, dockDoorLocation, null, "12345", 30m, staff1, ZDateTimeOffset.Now);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var transferInFactory2 = factory2.Load<WhsTransfer>(transfer.PK);
			AssertNotNull("Should have saved transfer to the database.", transferInFactory2);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				action: (receives, receiveLines, notifcations, refEquipment, skipLocations, concurrencyCondtion, rebuildLocationCondition) =>
				{
					var putawayTransferLines = receiveLines.Select(inv => inv.PutawayTransferLine);
					foreach (var putawayTransferLine in putawayTransferLines)
					{
						putawayTransferLine.WE_WL = nonDockDoorLocation1.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				// Allocate a location for the picked pallet ID
				var responseForAllocateLocation = webService2.Putaway_AutoAllocateInventory("12345", "");
				AssertSuccessfulResponse(responseForAllocateLocation, webService2);
				AssertEquals(nonDockDoorLocation1.PK, transferLineForPart1.WE_WL);
				AssertEquals(InventoryStatus.Codes.PuttingAway, transferLineForPart1.WE_OriginalInventoryStatus);
				AssertEquals(10m, transferLineForPart1.QtyCommittedIncludingMatchingLines);

				AssertEquals(nonDockDoorLocation1.PK, transferLineForPart2.WE_WL);
				AssertEquals(InventoryStatus.Codes.PuttingAway, transferLineForPart2.WE_OriginalInventoryStatus);
				AssertEquals(20m, transferLineForPart2.QtyCommittedIncludingMatchingLines);

				AssertEquals(nonDockDoorLocation1.PK, transferLineForPart3.WE_WL);
				AssertEquals(InventoryStatus.Codes.PuttingAway, transferLineForPart3.WE_OriginalInventoryStatus);
				AssertEquals(30m, transferLineForPart3.QtyCommittedIncludingMatchingLines);

				AssertInventoryStatusAndEventCount(inventory1, InventoryStatus.Codes.Received);
				AssertInventoryStatusAndEventCount(inventory2, InventoryStatus.Codes.Received);
				AssertInventoryStatusAndEventCount(inventory3, InventoryStatus.Codes.Received);
				AssertInventoryStatusAndEventCount(inventory4, InventoryStatus.Codes.Received);
				AssertInventoryStatusAndEventCount(inventory5, InventoryStatus.Codes.Received);
			}

			var webService3 = GetNewWebService(data.Whs1, staff1);
			// Putaway pallet ID into a location
			var responseForPutawayPallet = webService3.PutawayPallet("12345", nonDockDoorLocation2.ToLocationString(), "12345", isMultiPalletPutaway: false, Guid.Empty);
			AssertSuccessfulResponse(responseForPutawayPallet, webService3);
			AssertEquals(nonDockDoorLocation2.PK, transferLineForPart1.WE_WL);
			AssertEquals(InventoryStatus.Codes.Putaway, transferLineForPart1.WE_OriginalInventoryStatus);
			AssertEquals(10m, transferLineForPart1.QtyCommittedIncludingMatchingLines);

			AssertEquals(nonDockDoorLocation2.PK, transferLineForPart2.WE_WL);
			AssertEquals(InventoryStatus.Codes.Putaway, transferLineForPart2.WE_OriginalInventoryStatus);
			AssertEquals(20m, transferLineForPart2.QtyCommittedIncludingMatchingLines);

			AssertEquals(nonDockDoorLocation2.PK, transferLineForPart3.WE_WL);
			AssertEquals(InventoryStatus.Codes.Putaway, transferLineForPart3.WE_OriginalInventoryStatus);
			AssertEquals(30m, transferLineForPart3.QtyCommittedIncludingMatchingLines);

			AssertEquals(InventoryStatus.Codes.Putaway, PutawayHelper.GetPutawayTransferLineFromInventory(inventory1).WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Putaway, PutawayHelper.GetPutawayTransferLineFromInventory(inventory2).WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Putaway, PutawayHelper.GetPutawayTransferLineFromInventory(inventory3).WE_OriginalInventoryStatus);

			var webService4 = GetNewWebService(data.Whs1, staff1);
			var responseForAlreadyPutawayPallet = GetValidatePalletIDWebServiceResponse(webService4, "12345");
			AssertEquals(ErrorTypes.BusinessValidationError, responseForAlreadyPutawayPallet.Error);
			AssertEquals("Putaway has been already completed for the Pallet ID 12345.", responseForAlreadyPutawayPallet.ErrorMessage);

			var webService5 = GetNewWebService(data.Whs1, staff1);
			var responseWhenAutoAllocatingPutawayPallet = webService5.Putaway_AutoAllocateInventory("12345", "");
			AssertEquals(ErrorTypes.BusinessValidationError, responseWhenAutoAllocatingPutawayPallet.Error);
			AssertEquals("Putaway has been already completed for the Pallet ID 12345.", responseWhenAutoAllocatingPutawayPallet.ErrorMessage);

			var webService6 = GetNewWebService(data.Whs1, staff1);
			var responseWhenPutawayPallet = webService6.PutawayPallet("12345", nonDockDoorLocation2.ToLocationString(), "12345", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals(ErrorTypes.BusinessValidationError, responseWhenPutawayPallet.Error);
			AssertEquals("Putaway has been already completed for the Pallet ID 12345.", responseWhenPutawayPallet.ErrorMessage);
		}

		[TestDate(2016, 05, 30)]
		public void TestValidatePalletIDOnPutaway_CreatePutawayTransfers_SaveFails()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var nonDockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation2 = data.Whs1.FindLocation("A-2");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "12345", 20m);
			var inventory3 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "12345", 30m);
			var inventory4 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "", 40m);
			var inventory5 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "1234567", 50m);
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory3.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory4.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory5.WI_InventoryStatus);

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)receive).Row, TestConnection);
			webService1.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			// Validate Pallet ID On Putaway - First scan of the pallet Id
			var response = GetValidatePalletIDWebServiceResponse(webService1, "12345");
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has changed the putaway job while you have been working on it. Please restart the operation and try again.", response.ErrorMessage);
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", ErrorTypes.BusinessValidationError, response.Error);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var transferInFactory2 = factory2.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertNull("Should *not* have saved transfer to the database.", transferInFactory2);
		}

		[TestDate(2016, 05, 30)]
		public void TestValidatePalletIDOnPutaway_CreatePutawayTransfers_ReferencesOfReceiveThatCouldBeAutoFinalised()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location
			var nonDockDoorLocation2 = data.Whs1.FindLocation("A-3");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "B", 20m);
			var inventory3 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "B", 30m);
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory3.WI_InventoryStatus);

			var webService1 = GetNewWebService();
			SetupSecurityHeader(webService1, data.Whs1, staff1);
			// Validate Pallet ID On Putaway - First scan of the pallet Id
			GetValidatePalletIDWebServiceResponse(webService1, "a");
			GetValidatePalletIDWebServiceResponse(webService1, "b");
			AssertInventoryStatusAndEventCount(inventory1, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory2, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory3, InventoryStatus.Codes.Received);

			var transferForPalletA = (WhsTransfer)inventory1.AllPickLines.First().DocketLine.Docket;
			var transferForPalletB = (WhsTransfer)inventory2.AllPickLines.First().DocketLine.Docket;
			AssertEquals(transferForPalletA.PK, transferForPalletB.PK);
			AssertPutawayTransfer(transferForPalletA, data.Org1, data.Whs1, 3);

			var transferLineForPart1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			AssertPutawayTransferLine(transferLineForPart1, data.Part1, dockDoorLocation, null, "A", 10m, staff1, ZDateTimeOffset.Now);

			var transferLineForPart2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			AssertPutawayTransferLine(transferLineForPart2, data.Part2, dockDoorLocation, null, "B", 20m, staff1, ZDateTimeOffset.Now);

			var transferLineForPart3 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory3);
			AssertPutawayTransferLine(transferLineForPart3, data.Part2, dockDoorLocation, null, "B", 30m, staff1, ZDateTimeOffset.Now);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				action: (recieves, receiveLines, notifications, refEquipment, skipLocations, concurrencyCondition, rebuildCacheCondition) =>
				{
					var putawayTransferLines = receiveLines.Select(inv => inv.PutawayTransferLine);
					foreach (var putawayTransferLine in putawayTransferLines)
					{
						putawayTransferLine.WE_WL = putawayTransferLine.WE_PalletID.EqualsIgnoringCase("A") ? nonDockDoorLocation1.PK : nonDockDoorLocation2.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService();
				SetupSecurityHeader(webService2, data.Whs1, staff1);
				// Allocate a location for the picked pallet ID
				webService2.Putaway_AutoAllocateInventory("a", "");
				webService2.Putaway_AutoAllocateInventory("b", "");
			}

			AssertEquals(nonDockDoorLocation1.PK, transferLineForPart1.WE_WL);
			AssertEquals(InventoryStatus.Codes.PuttingAway, transferLineForPart1.WE_OriginalInventoryStatus);
			AssertEquals(10m, transferLineForPart1.QtyCommittedIncludingMatchingLines);

			AssertEquals(nonDockDoorLocation2.PK, transferLineForPart2.WE_WL);
			AssertEquals(InventoryStatus.Codes.PuttingAway, transferLineForPart2.WE_OriginalInventoryStatus);
			AssertEquals(20m, transferLineForPart2.QtyCommittedIncludingMatchingLines);

			AssertEquals(nonDockDoorLocation2.PK, transferLineForPart3.WE_WL);
			AssertEquals(InventoryStatus.Codes.PuttingAway, transferLineForPart3.WE_OriginalInventoryStatus);
			AssertEquals(30m, transferLineForPart3.QtyCommittedIncludingMatchingLines);

			AssertInventoryStatusAndEventCount(inventory1, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory2, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory3, InventoryStatus.Codes.Received);

			var webService3 = GetNewWebService();
			SetupSecurityHeader(webService3, data.Whs1, staff1);
			// Putaway pallet ID into a location
			var responseForPutawayPalletA = webService3.PutawayPallet("a", nonDockDoorLocation2.ToLocationString(), "a", isMultiPalletPutaway: false, Guid.Empty);
			var responseForPutawayPalletB = webService3.PutawayPallet("b", nonDockDoorLocation2.ToLocationString(), "b", isMultiPalletPutaway: false, Guid.Empty);
			AssertEquals(0, responseForPutawayPalletA.ReferencesOfReceiveThatCouldBeAutoFinalised.Count);
			AssertEquals(receive.WD_DocketID, responseForPutawayPalletB.ReferencesOfReceiveThatCouldBeAutoFinalised.Single());
		}

		[TestDate(2016, 05, 30)]
		public void TestValidatePalletIDOnPutaway_PutawayTransferExists()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "12345");
			inventory1.WI_ArrivalDate = ZDateTimeOffset.Now.AddDays(-2);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "12345");
			inventory2.WI_ArrivalDate = ZDateTimeOffset.Now.AddDays(-2);
			Helper.Factory.Save();

			GetValidatePalletIDWebServiceResponse(GetNewWebService(data.Whs1, staff), "12345");

			var transferLineForPart1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			var transferLineForPart2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);

			AssertEquals("Precondition: inventory1 (receiveLine) status is RECEIVED.", true, inventory1.IsReceivedIntoDockDoor);
			AssertEquals("Precondition: inventory2 (receiveLine) status is RECEIVED.", true, inventory2.IsReceivedIntoDockDoor);
			AssertNotNull("Precondition: inventory1 has putaway transfer.", transferLineForPart1);
			AssertNotNull("Precondition: inventory2 has putaway transfer.", transferLineForPart2);
			AssertEquals("Precondition: transferLineForPart1 is picked.", true, transferLineForPart1.IsPicked);
			AssertEquals("Precondition: transferLineForPart2 is picked.", true, transferLineForPart2.IsPicked);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			AssertEquals("Precondition: there is 1 transfer.", 1, Helper.Factory.Load<WhsTransfer>(query).Length);

			GetValidatePalletIDWebServiceResponse(GetNewWebService(data.Whs1, staff), "12345");

			AssertEquals("No new transfer is created.", 1, Helper.Factory.Load<WhsTransfer>(query).Length);
			AssertPutawayTransferLine(transferLineForPart1, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, null, "12345", 10m, staff, ZDateTimeOffset.Now);
			AssertEquals("Receive line status is PFU.", DocketLineStatus.Codes.PickedForUnload, inventory1.InDocketLine.WE_DocketLineStatus);
			AssertEquals("Qty to move for transferLineForPart1 is correct.", 10m, transferLineForPart1.QtyToMoveIncludingMatchingLines);

			AssertEquals("transferLineForPart2 is picked.", true, transferLineForPart2.IsPicked);
			AssertPutawayTransferLine(transferLineForPart2, data.Part2, data.Whs1.DefaultOutboundDockDoorLocation, null, "12345", 20m, staff, ZDateTimeOffset.Now);
			AssertEquals("Receive line status is PFU.", DocketLineStatus.Codes.PickedForUnload, inventory2.InDocketLine.WE_DocketLineStatus);
			AssertEquals("Qty to move for transferLineForPart2 is correct.", 20m, transferLineForPart2.QtyToMoveIncludingMatchingLines);
		}

		public void TestValidatePalletIDOnPutaway_PutawayTransferExistsAndAlreadyPicked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "12345");
			inventory1.WI_ArrivalDate = ZDateTimeOffset.Now.AddDays(-2);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "12345");
			inventory2.WI_ArrivalDate = ZDateTimeOffset.Now.AddDays(-2);
			Helper.Factory.Save();

			var pickedTime = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now).AddDays(-1);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, nonDockDoorLocation, "12345", 10m);
			transferLineForPart1.PickedTime = pickedTime;
			var transferLineForPart2 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part2, data.Whs1.DefaultOutboundDockDoorLocation, nonDockDoorLocation, "12345", 20m);
			transferLineForPart2.PickedTime = pickedTime;
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			AssertEquals("Precondition: inventory1 status is RECEIVED.", true, inventory1.IsReceivedIntoDockDoor);
			AssertEquals("Precondition: inventory2 status is RECEIVED.", true, inventory2.IsReceivedIntoDockDoor);
			AssertNotNull("Precondition: inventory1 has putaway transfer.", ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransfer);
			AssertNotNull("Precondition: inventory2 has putaway transfer.", ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransfer);
			AssertEquals("Precondition: transferLineForPart1 is picked.", true, transferLineForPart1.IsPicked);
			AssertEquals("Precondition: transferLineForPart1.PickedTime is pickedTime.", pickedTime, transferLineForPart1.PickedTime);
			AssertEquals("Precondition: transferLineForPart2 is picked.", true, transferLineForPart2.IsPicked);
			AssertEquals("Precondition: transferLineForPart2.PickedTime is pickedTime.", pickedTime, transferLineForPart2.PickedTime);

			GetValidatePalletIDWebServiceResponse(GetNewWebService(data.Whs1, staff1), "12345");

			AssertEquals("transferLineForPart1 is picked.", true, transferLineForPart1.IsPicked);
			AssertEquals("transferLineForPart1.PickedTime is still the same.", pickedTime, transferLineForPart1.PickedTime);
			AssertEquals("transferLineForPart2 is picked.", true, transferLineForPart2.IsPicked);
			AssertEquals("transferLineForPart2.PickedTime is still the same.", pickedTime, transferLineForPart2.PickedTime);
		}

		public void TestValidatePalletIDOnPutaway_MultipleInventories_PalletIdOnPutawayAndReceivedInventories()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation, "12345", 10m);
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = GetValidatePalletIDWebServiceResponse(webService1, "12345");
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("No error in the response.", null, response1.ErrorMessage);

			AssertEquals("Precondition", true, inventory1.HasPutawayTransfer);

			var receive2 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW2", ZDateTimeOffset.Empty);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, dockDoorLocation, "12345", 20m);
			Helper.Factory.Save();

			AssertEquals("Inventory with the same pallet id on the same location can be created.", false, inventory2.HasErrors);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);

			// Added to set up situation in multiple pallet putaway
			var putawayLines = WhsPutawayLineDO.ShallowLoadFromDB(TestConnection);
			putawayLines.ForEach(pl => WhsPutawayLineDO.UpdateWhere(pl.PK).Set(l => l.WPL_IsFinalized, true));
			var putawayJobs = WhsPutawayJobDO.ShallowLoadFromDB(TestConnection);
			putawayJobs.ForEach(j => WhsPutawayJobDO.UpdateWhere(j.PK).Set(l => l.WPJ_FinalizedTimeUtc, DateTime.UtcNow));

			var response2 = GetValidatePalletIDWebServiceResponse(GetNewWebService(data.Whs1, staff1), "12345");
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Putaway Failed: Inventories with Pallet ID 12345 are currently being putaway and unloaded at the same time.", response2.ErrorMessage);
		}

		[TestDate(2016, 05, 30)]
		public void TestValidatePalletIDOnPutaway_CreatePutawayTransfers_InventoryWithNoQuantity()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 20m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, null, "12345");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, dockDoorLocation, "12345");
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, inventory3.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory4.WI_InventoryStatus);

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = GetValidatePalletIDWebServiceResponse(webService, "12345");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("No error in the response.", null, response.ErrorMessage);

			var transfer = (WhsTransfer)inventory1.AllPickLines.First().DocketLine.Docket;
			AssertPutawayTransfer(transfer, data.Org1, data.Whs1, 2);
			var transferLine1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			AssertPutawayTransferLine(transferLine1, data.Part1, dockDoorLocation, null, "12345", 10m, staff1, ZDateTimeOffset.Now);
			var transferLine2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			AssertPutawayTransferLine(transferLine2, data.Part1, dockDoorLocation, null, "12345", 20m, staff1, ZDateTimeOffset.Now);

			AssertEquals(true, inventory1.HasPutawayTransfer);
			AssertEquals(true, inventory2.HasPutawayTransfer);
			AssertEquals(false, inventory3.HasPutawayTransfer);
			AssertEquals(false, inventory4.HasPutawayTransfer);
		}

		public void TestValidatePalletIDOnPutaway_CreatePutawayTransfers_NoQuantityToPutaway()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, null, "12345");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, dockDoorLocation, "12345");
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Pending, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);

			var response = GetValidatePalletIDWebServiceResponse(GetNewWebService(data.Whs1, staff1), "12345");
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("There are no items to putaway.", response.ErrorMessage);
		}

		protected static void AssertInventoryStatusAndEventCount(WhsInventoryView inventory1, string inventoryStatus)
		{
			AssertEquals(inventoryStatus, inventory1.WI_InventoryStatus);
			AssertEquals(0, inventory1.InDocketLine.Logs.Find(l => l.SL_SE_NKEvent == Events.WarehouseReceiptConfirmedPutaway.Code).Count());
		}

		protected void AssertPutawayTransfer(WhsTransfer putawayTransfer, OrgHeader client, WhsWarehouse warehouse, int expectednumberOfLines)
		{
			AssertEquals(true, putawayTransfer.WD_IsPutawayTransfer);
			AssertEquals(client.PK, putawayTransfer.WD_OH_Client);
			AssertEquals(warehouse.PK, putawayTransfer.WD_WW_Whs);
			AssertEquals(expectednumberOfLines, putawayTransfer.Lines.Count);
		}

		protected abstract void AssertPutawayTransferLine(WhsTransferLine line, OrgSupplierPart product, WhsLocation sourceLocation, WhsLocation destinationLocation,
			string palletID, decimal quantity, GlbStaff pickedBy, ZDateTimeOffset pickedTime);

		#endregion

		#region TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory10UPart1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			var inventory5UPart1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 5m);
			var inventory27UPart2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "PalletID1", 27m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 16m);

			orderLine1.ReserveStockIfAbleTo(inventory10UPart1);
			orderLine2.ReserveStockIfAbleTo(inventory5UPart1);
			orderLine3.ReserveStockIfAbleTo(inventory27UPart2);
			AssertEquals("Inventory should have 1 reserved PickLines.", 1, inventory10UPart1.ReservedPickLines.Count);
			AssertEquals("Inventory should have 1 reserved PickLines.", 1, inventory5UPart1.ReservedPickLines.Count);
			AssertEquals("Inventory should have 1 reserved PickLines.", 1, inventory27UPart2.ReservedPickLines.Count);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PalletID1");
			AssertSuccessfulResponse(response, webService);

			AssertEquals("Receive line's inventory has no more reserved pick lines.", false, inventory10UPart1.ReservedPickLines.Any());
			AssertEquals("Receive line's inventory has no more reserved pick lines.", false, inventory5UPart1.ReservedPickLines.Any());
			AssertEquals("Receive line's inventory has no more reserved pick lines.", false, inventory27UPart2.ReservedPickLines.Any());

			var putawayTransferLine10UPart1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory10UPart1);
			AssertEquals("Part1 Putaway transfer line inventory has 1 reserved pick line.", 1, putawayTransferLine10UPart1.ReservedPickLines.Count);
			AssertEquals("Part1 Reserved Quantity is correct.", 3m, putawayTransferLine10UPart1.ReservedQuantity);

			var putawayTransferLine5UPart1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory5UPart1);
			AssertEquals("Part1 Putaway transfer line inventory has 1 reserved pick line.", 1, putawayTransferLine5UPart1.ReservedPickLines.Count);
			AssertEquals("Part1 Reserved Quantity is correct.", 5m, putawayTransferLine5UPart1.ReservedQuantity);

			var putawayTransferLine27UPart2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory27UPart2);
			AssertEquals("Part2 Putaway transfer line inventory has 1 reserved pick line.", 1, putawayTransferLine27UPart2.ReservedPickLines.Count);
			AssertEquals("Part2 Reserved Quantity is correct.", 16m, putawayTransferLine27UPart2.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			orderLine1.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLines.", 1, inventory.ReservedPickLines.Count);
			AssertEquals("Precondition", 10m, inventory.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory.WI_InDocketLineUnits = 6m;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PalletID1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("No error in the response.", null, response.ErrorMessage);

			var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 1, putawayTransferLine.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 6m, putawayTransferLine.ReservedQuantity);

			AssertEquals("Receive line got split.", 2, receive.Lines.Count);
			AssertEquals("Original inventory still has no reserved pick lines.", false, inventory.ReservedPickLines.Any());
			AssertEquals("Original inventory still has no reserved pick lines.", 0m, inventory.InDocketLine.ReservedQuantity);

			var newInventory = receive.Lines.Single(line => line.WE_TransactionQuantity == 0);
			AssertEquals(4m, newInventory.WE_ClientOrderedUnits);
			AssertEquals("New inventory has reserved pick lines.", true, newInventory.ReservedPickLines.Any());
			AssertEquals("New inventory has reserved pick lines.", 4m, newInventory.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_MultipleReserveLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 7m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);

			orderLine1.ReserveStockIfAbleTo(inventory);
			orderLine2.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLines.", 2, inventory.ReservedPickLines.Count);
			AssertEquals("Precondition", 10m, inventory.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory.WI_InDocketLineUnits = 4m;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PalletID1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("No error in the response.", null, response.ErrorMessage);

			var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 2, putawayTransferLine.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 4m, putawayTransferLine.ReservedQuantity);

			AssertEquals("Receive line got split.", 2, receive.Lines.Count);
			AssertEquals("Original inventory has no reserved pick lines.", false, inventory.ReservedPickLines.Any());
			AssertEquals("Original inventory has no reserved pick lines.", 0m, inventory.InDocketLine.ReservedQuantity);

			var newInventory = receive.Lines.Single(line => line.WE_TransactionQuantity == 0);
			AssertEquals(6m, newInventory.WE_ClientOrderedUnits);
			AssertEquals("New inventory has reserved pick lines.", 1, newInventory.ReservedPickLines.Count);
			AssertEquals("New inventory has reserved pick lines.", 6m, newInventory.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_MultipleReserveLinesMovedToNewInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 7m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);

			orderLine1.ReserveStockIfAbleTo(inventory);
			orderLine2.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLines.", 2, inventory.ReservedPickLines.Count);
			AssertEquals("Precondition", 10m, inventory.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory.WI_InDocketLineUnits = 2m;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PalletID1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("No error in the response.", null, response.ErrorMessage);

			var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 1, putawayTransferLine.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 2m, putawayTransferLine.ReservedQuantity);

			AssertEquals("Receive line got split.", 2, receive.Lines.Count);
			AssertEquals("Original inventory has no reserved pick lines.", false, inventory.ReservedPickLines.Any());
			AssertEquals("Original inventory has no reserved pick lines.", 0m, inventory.InDocketLine.ReservedQuantity);

			var newInventory = receive.Lines.Single(line => line.WE_TransactionQuantity == 0);
			AssertEquals(8m, newInventory.WE_ClientOrderedUnits);
			AssertEquals("New inventory has reserved pick lines.", 2, newInventory.ReservedPickLines.Count);
			AssertEquals("New inventory has reserved pick lines.", 8m, newInventory.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_MultipleReserveLinesFromMultipleInventories()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 20m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);

			orderLine1.ReserveStockIfAbleTo(inventory1);
			orderLine2.ReserveStockIfAbleTo(inventory2);
			AssertEquals("Precondition: Inventory1 should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition", 8m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition: Inventory2 should have 1 reserved PickLine.", 1, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition", 15m, inventory2.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory1.WI_InDocketLineUnits = 6m;
			inventory2.WI_InDocketLineUnits = 12m;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PalletID1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("No error in the response.", null, response.ErrorMessage);

			var putawayTransferLine1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 1, putawayTransferLine1.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 6m, putawayTransferLine1.ReservedQuantity);

			var putawayTransferLine2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 1, putawayTransferLine2.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 12m, putawayTransferLine2.ReservedQuantity);

			AssertEquals("Receive lines got split.", 4, receive.Lines.Count);
			AssertEquals("Inventory1 has been split.", 6m, inventory1.WI_ExpectedReceiptQuantity);
			AssertEquals("Inventory1 has no reserved pick lines.", false, inventory2.ReservedPickLines.Any());
			AssertEquals("Inventory1 has no reserved pick lines.", 0m, inventory2.InDocketLine.ReservedQuantity);

			var newInventory1 = receive.Lines.Single(line => line.WE_ClientOrderedUnits == 4m);
			AssertEquals("New inventory has reserved pick lines.", 1, newInventory1.ReservedPickLines.Count);
			AssertEquals("New inventory has reserved pick lines.", 2m, newInventory1.ReservedQuantity);

			AssertEquals("Inventory2 has been split.", 12m, inventory2.WI_ExpectedReceiptQuantity);
			AssertEquals("Inventory2 has no reserved pick lines.", false, inventory2.ReservedPickLines.Any());
			AssertEquals("Inventory2 has no reserved pick lines.", 0m, inventory2.InDocketLine.ReservedQuantity);

			var newInventory2 = receive.Lines.Single(line => line.WE_ClientOrderedUnits == 8m);
			AssertEquals("New inventory has reserved pick lines.", 1, newInventory2.ReservedPickLines.Count);
			AssertEquals("New inventory has reserved pick lines.", 3m, newInventory2.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_MultipleReserveLinesOnInventory()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 40m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);

			orderLine1.ReserveStockIfAbleTo(inventory1);
			orderLine2.ReserveStockIfAbleTo(inventory2, 20m);
			orderLine3.ReserveStockIfAbleTo(inventory2, 20m);
			AssertEquals("Precondition: Inventory1 should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition", 10m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition: Inventory2 should have 2 reserved PickLine.", 2, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition", 40m, inventory2.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PalletID1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("No error in the response.", null, response.ErrorMessage);

			var putawayTransferLine1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 1, putawayTransferLine1.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 10m, putawayTransferLine1.ReservedQuantity);

			var putawayTransferLine2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 2, putawayTransferLine2.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 40m, putawayTransferLine2.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_SameOrderLine()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 70m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 70m);
			orderLine.ReserveStockIfAbleTo(inventory1, 40m);
			orderLine.ReserveStockIfAbleTo(inventory2, 30m);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition", 40m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 30m, inventory2.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PalletID1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("No error in the response.", null, response.ErrorMessage);
			Helper.Factory.Save();

			var putawayTransferLine1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			AssertEquals("Reserve Lines should exist.", true, putawayTransferLine1.ReservedPickLines.Any());
			AssertEquals("Should reserve 40 units.", 40m, putawayTransferLine1.ReservedQuantity);

			var putawayTransferLine2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			AssertEquals("Reserve Lines should exist.", true, putawayTransferLine2.ReservedPickLines.Any());
			AssertEquals("Should reserve 30 units.", 30m, putawayTransferLine2.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesStayOnSimilarInventory()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 70m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 90m);
			orderLine.ReserveStockIfAbleTo(inventory1, 60m);
			orderLine.ReserveStockIfAbleTo(inventory2, 30m);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition", 60m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 30m, inventory2.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PalletID1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("No error in the response.", null, response.ErrorMessage);
			Helper.Factory.Save();

			var allTransferLines = new List<WhsTransferLine>();
			var putawayTransferLine1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			allTransferLines.Add(putawayTransferLine1);
			allTransferLines.AddRange(putawayTransferLine1.MatchingLines.Cast<WhsTransferLine>());
			var line1 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Reserve Lines should exist.", true, line1.ReservedPickLines.Any());
			AssertEquals("Should reserve 60 units.", 60m, line1.ReservedQuantity);

			var putawayTransferLine2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			allTransferLines.Add(putawayTransferLine2);
			allTransferLines.AddRange(putawayTransferLine2.MatchingLines.Cast<WhsTransferLine>());
			var line2 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertEquals("Reserve Lines should exist.", true, line2.ReservedPickLines.Any());
			AssertEquals("Should reserve 30 units.", 30m, line2.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_MultipleSplitPallets()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 70m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 25m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 60m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID2");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 90m);
			orderLine.ReserveStockIfAbleTo(inventory1, 15m);
			orderLine.ReserveStockIfAbleTo(inventory2, 30m);
			orderLine.ReserveStockIfAbleTo(inventory3, 5m);
			orderLine.ReserveStockIfAbleTo(inventory4, 40m);

			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory3.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory4.ReservedPickLines.Count);

			AssertEquals("Precondition", 15m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 30m, inventory2.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 5m, inventory3.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 40m, inventory4.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = GetValidatePalletIDWebServiceResponse(webService1, "PalletID1");
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("No error in the response.", null, response1.ErrorMessage);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = GetValidatePalletIDWebServiceResponse(webService2, "PalletID2");
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("No error in the response.", null, response2.ErrorMessage);

			var allTransferLines = new List<WhsTransferLine>();
			foreach (var inventory in new[] { inventory1, inventory2, inventory3, inventory4 })
			{
				var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);
				allTransferLines.Add(putawayTransferLine);
				allTransferLines.AddRange(putawayTransferLine.MatchingLines.Cast<WhsTransferLine>());
			}

			var lines1 = allTransferLines.Where(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Only 1 distinct transferLine should exist.", 1, lines1.Distinct().Count());
			AssertEquals("Reserve Lines should exist.", true, lines1.First().ReservedPickLines.Any());
			AssertEquals("Should reserve 15 units.", 15m, lines1.First().ReservedQuantity);

			var line2 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertEquals("Reserve Lines should exist.", true, line2.ReservedPickLines.Any());
			AssertEquals("Should reserve 30 units.", 30m, line2.ReservedQuantity);

			var line3 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory3.WI_WE_InDocketLine);
			AssertEquals("Reserve Lines should exist.", true, line3.ReservedPickLines.Any());
			AssertEquals("Should reserve 5 units.", 5m, line3.ReservedQuantity);

			var line4 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory4.WI_WE_InDocketLine);
			AssertEquals("Reserve Lines should exist.", true, line4.ReservedPickLines.Any());
			AssertEquals("Should reserve 40 units.", 40m, line4.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_SameOrderLine_Mix()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 70m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 70m);
			orderLine.ReserveStockIfAbleTo(inventory1, 40m);
			orderLine.ReserveStockIfAbleTo(inventory3, 30m);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory have no reserved PickLine.", 0, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory3.ReservedPickLines.Count);
			AssertEquals("Precondition", 40m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 0m, inventory2.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 30m, inventory3.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PalletID1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("No error in the response.", null, response.ErrorMessage);

			var allTransferLines = new List<WhsTransferLine>();
			allTransferLines.Add(PutawayHelper.GetPutawayTransferLineFromInventory(inventory1));
			allTransferLines.Add(PutawayHelper.GetPutawayTransferLineFromInventory(inventory2));
			allTransferLines.Add(PutawayHelper.GetPutawayTransferLineFromInventory(inventory3));
			var line1 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals(true, line1.ReservedPickLines.Any());
			AssertEquals(40m, line1.ReservedQuantity);

			var line2 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertEquals(false, line2.ReservedPickLines.Any());
			AssertEquals(0m, line2.ReservedQuantity);

			var line3 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory3.WI_WE_InDocketLine);
			AssertEquals(true, line3.ReservedPickLines.Any());
			AssertEquals(30m, line3.ReservedQuantity);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_PalletWithBondedStock

		public void TestValidatePalletIDOnPutaway_PalletWithBondedStock_NoUnloadLocation()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, null, "PLT1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PLT1");
			AssertEquals("Should return error", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should return error", BondedPalletError(), response.ErrorMessage);
		}

		protected abstract string BondedPalletError();

		#endregion

		#region TestValidatePalletIDOnPutaway_HoldPalletIDPutaway()

		public void TestValidatePalletIDOnPutaway_HoldPalletIDPutaway()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.WD_HoldPalletIDPutaway = true;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, null, "PLT1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PLT1");
			AssertEquals("Should return error", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should return error", "A desktop user will need to audit the unloaded pallets for the following receive(s):\r\nW00000001", response.ErrorMessage);
		}

		public void TestValidatePalletIDOnPutaway_HoldPalletIDPutaway_WithMultipleReceives()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive1Line = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT1");
			receive1.WD_HoldPalletIDPutaway = true;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receive2Line = Helper.CreateWhsReceiveLine(receive2, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT1");
			receive2.WD_HoldPalletIDPutaway = false;

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var receive3Line = Helper.CreateWhsReceiveLine(receive3, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT1");
			receive3.WD_HoldPalletIDPutaway = true;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = GetValidatePalletIDWebServiceResponse(webService, "PLT1");
			AssertEquals("Should return error", ErrorTypes.BusinessValidationError, response.Error);
			AssertContains("Should return error message like", "A desktop user will need to audit the unloaded pallets for the following receive(s):", response.ErrorMessage);
			AssertContains("Should return all job with flag set to true", "W00000001", response.ErrorMessage);
			AssertContains("Should return all job with flag set to true", "W00000003", response.ErrorMessage);
			AssertNotContains("Should return all job with flag set to true", "W00000002", response.ErrorMessage);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_OnePutawayTransfer

		public void TestValidatePalletIDOnPutaway_OnePutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-2");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine1.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result1 = GetValidatePalletIDWebServiceResponse(webService1, "PLT-1");
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine1.HasPutawayTransfer);

			AssertEquals("Precondition", false, receiveLine2.HasPutawayTransfer);
			var webService2 = GetNewWebService(data.Whs1);
			var result2 = GetValidatePalletIDWebServiceResponse(webService2, "PLT-2");
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine2.HasPutawayTransfer);

			AssertEquals("Only 1 putaway transfer was created.", receiveLine1.PutawayTransfer.PK, receiveLine2.PutawayTransfer.PK);
			var transferCountInDatabase = Helper.Factory.GetDatabaseCount(typeof(WhsDocket), new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("No extra transfers are created.", 1, transferCountInDatabase);
		}

		public void TestValidatePalletIDOnPutaway_OnePutawayTransfer_ExistingFinalisedTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine1.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result1 = GetValidatePalletIDWebServiceResponse(webService1, "PLT-1");
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine1.HasPutawayTransfer);

			var webService2 = GetNewWebService(data.Whs1);
			var result2 = webService2.PutawayPallet("PLT-1", "A-1", "PLT-1", isMultiPalletPutaway: false, Guid.Empty);
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);

			var putawayTransfer1 = receiveLine1.PutawayTransfer;
			putawayTransfer1.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();
			AssertEquals("Putaway transfer is finalised.", true, putawayTransfer1.IsFinalised);
			AssertEquals("Receive is not finalised.", false, receive.IsFinalised);

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-2");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine2.HasPutawayTransfer);
			var webService3 = GetNewWebService(data.Whs1);
			var result3 = GetValidatePalletIDWebServiceResponse(webService3, "PLT-2");
			AssertNull("No errors from webservice call.", result3.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result3.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine2.HasPutawayTransfer);

			AssertNotEquals("Receive lines does not share the same putaway transfer.", receiveLine1.PutawayTransfer.PK, receiveLine2.PutawayTransfer.PK);

			var transferCountInDatabase = Helper.Factory.GetDatabaseCount(typeof(WhsDocket), new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("Number of transfers created is correct.", 2, transferCountInDatabase);
		}

		public void TestValidatePalletIDOnPutaway_OnePutawayTransfer_InventoryOnMultipleReceives()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive1Line = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receive2Line = Helper.CreateWhsReceiveLine(receive2, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receive1Line.HasPutawayTransfer);
			AssertEquals("Precondition", false, receive2Line.HasPutawayTransfer);
			var webService = GetNewWebService(data.Whs1);
			var result = GetValidatePalletIDWebServiceResponse(webService, "PLT-1");
			AssertNull("No errors from webservice call.", result.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result.Error);
			AssertEquals("Receive line has putaway transfer.", true, receive1Line.HasPutawayTransfer);
			AssertEquals("Receive line has putaway transfer.", true, receive2Line.HasPutawayTransfer);

			AssertEquals("Only 1 putaway transfer was created.", receive1Line.PutawayTransfer.PK, receive2Line.PutawayTransfer.PK);

			var transferCountInDatabase = Helper.Factory.GetDatabaseCount(typeof(WhsDocket), new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("No extra transfers are created.", 1, transferCountInDatabase);
		}

		public void TestValidatePalletIDOnPutaway_OnePutawayTransfer_BothReceivesWithPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive1Line1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			var receive1Line2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-SAME");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receive2Line1 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-2");
			var receive2Line2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 40m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-SAME");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receive1Line1.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result1 = GetValidatePalletIDWebServiceResponse(webService1, "PLT-1");
			AssertNull("No errors from webservice call.", result1.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result1.Error);
			AssertEquals("Receive line has putaway transfer.", true, receive1Line1.HasPutawayTransfer);

			AssertEquals("Precondition", false, receive2Line1.HasPutawayTransfer);
			var webService2 = GetNewWebService(data.Whs1);
			var result2 = GetValidatePalletIDWebServiceResponse(webService2, "PLT-2");
			AssertNull("No errors from webservice call.", result2.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result2.Error);
			AssertEquals("Receive line has putaway transfer.", true, receive2Line1.HasPutawayTransfer);

			AssertNotEquals("Receive lines does not share the same putaway transfer.", receive1Line1.PutawayTransfer.PK, receive2Line1.PutawayTransfer.PK);

			var webService3 = GetNewWebService(data.Whs1);
			var result3 = GetValidatePalletIDWebServiceResponse(webService3, "PLT-SAME");
			AssertNull("No errors from webservice call.", result3.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result3.Error);
			AssertEquals("Receive line has putaway transfer.", true, receive1Line2.HasPutawayTransfer);
			AssertEquals("Receive line has putaway transfer.", true, receive2Line2.HasPutawayTransfer);

			AssertEquals("Receive lines share the same putaway transfer.", receive1Line2.PutawayTransfer.PK, receive2Line2.PutawayTransfer.PK);
			AssertEquals("Putaway transfer with earlier docket id was selected.", receive1Line1.PutawayTransfer.PK, receive1Line2.PutawayTransfer.PK);

			var transferCountInDatabase = Helper.Factory.GetDatabaseCount(typeof(WhsDocket), new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("No extra transfers are created.", 2, transferCountInDatabase);
		}

		protected void TestValidatePalletIDOnPutaway_OnePutawayTransfer_DBHits(int expectedDockets, int expectedInvs, Func<Dictionary<string, int>> expectedDBHits)
		{
			TestValidatePalletIDOnPutaway_OnePutawayTransfer_DBHitsCore(useTaskManagement: false, expectedDockets, expectedInvs, expectedDBHits);
		}

		protected void TestValidatePalletIDOnPutaway_OnePutawayTransfer_TaskManagement_DBHits(int expectedDockets, int expectedInvs, Func<Dictionary<string, int>> expectedDBHits)
		{
			TestValidatePalletIDOnPutaway_OnePutawayTransfer_DBHitsCore(useTaskManagement: true, expectedDockets, expectedInvs, expectedDBHits);
		}

		void TestValidatePalletIDOnPutaway_OnePutawayTransfer_DBHitsCore(bool useTaskManagement, int expectedDockets, int expectedInvs, Func<Dictionary<string, int>> expectedDBHits)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			if (useTaskManagement)
			{
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			}
			var samePalletId = "PLT-SAME";
			var webService = GetNewWebService(data.Whs1);

			for (var i = 0; i < 10; i++)
			{
				var palletCount = i.ToString();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, palletCount);
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, palletCount);
				var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, samePalletId);
				line2.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
				Helper.Factory.Save();

				GetValidatePalletIDWebServiceResponse(webService, palletCount);
			}

			var receive11 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R11");
			Helper.CreateWhsReceiveLine(receive11, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, samePalletId);
			Helper.Factory.Save();

			AssertEquals("Precondition: Correct number of dockets.", expectedDockets, Helper.Factory.GetDatabaseCount(typeof(WhsDocket)));
			AssertEquals("Precondition: Correct number of inventories.", expectedInvs, Helper.Factory.GetDatabaseCount(typeof(WhsInventoryView)));

			var newWebService = GetNewWebService(data.Whs1);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits(), newWebService.Factory))
			using (RowFactory.SetCachedTables())
			{
				var result = GetValidatePalletIDWebServiceResponse(newWebService, samePalletId);
				AssertNull("No errors from webservice call.", result.ErrorMessage);
				AssertEquals("No errors from webservice call.", ErrorTypes.None, result.Error);
			}
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_WithHoldReason

		public void TestValidatePalletIDOnPutaway_WithHoldReason()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receiveLine2.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receiveLine1.WE_CurrentHoldReason = "Whatever";
			receiveLine2.WE_CurrentHoldReason = "Whenever";
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine1.HasPutawayTransfer);
			var webService1 = GetNewWebService(data.Whs1);
			var result = GetValidatePalletIDWebServiceResponse(webService1, "PLT-1");
			AssertNull("No errors from webservice call.", result.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine1.HasPutawayTransfer);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine2.HasPutawayTransfer);

			var putawayTransferLine1 = receiveLine1.PutawayTransferLine;
			AssertEquals("Putaway Transfer Line should copy Hold Reason.", "Whatever", putawayTransferLine1.WE_CurrentHoldReason);

			var putawayTransferLine2 = receiveLine2.PutawayTransferLine;
			AssertEquals("Putaway Transfer Line should copy Hold Reason.", "Whenever", putawayTransferLine2.WE_CurrentHoldReason);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_WithCustomAttributes

		public void TestValidatePalletIDOnPutaway_WithCustomAttributes()
		{
			TestValidatePalletIDOnPutaway_WithCustomAttributes_Core(swapLines: false);
		}

		public void TestValidatePalletIDOnPutaway_WithCustomAttributes_SwapLines()
		{
			TestValidatePalletIDOnPutaway_WithCustomAttributes_Core(swapLines: true);
		}

		void TestValidatePalletIDOnPutaway_WithCustomAttributes_Core(bool swapLines)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, swapLines ? 10m : 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, swapLines ? 20m : 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");

			receiveLine1.WE_CustomAttrib1 = "23x23x12 inches";
			receiveLine1.WE_CustomAttrib2 = "24 lbs";
			receiveLine1.WE_CustomAttrib3 = "Carton#2";

			receiveLine2.WE_CustomAttrib1 = "12x12x12 inches";
			receiveLine2.WE_CustomAttrib2 = "34 lbs";
			receiveLine2.WE_CustomAttrib3 = "Carton#1";

			AssertEquals("Precondition: Line is Received to DockDoor", InventoryStatus.Codes.Received, receiveLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Line is Received to DockDoor", InventoryStatus.Codes.Received, receiveLine2.WE_CurrentInventoryStatus);
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine1.HasPutawayTransfer);
			var webService = GetNewWebService(data.Whs1);
			var result = GetValidatePalletIDWebServiceResponse(webService, "PLT-1");
			AssertNull("No errors from webservice call.", result.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine1.HasPutawayTransfer);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine2.HasPutawayTransfer);

			var putawayTransferLine1 = receiveLine1.PutawayTransferLine;
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "23x23x12 inches", putawayTransferLine1.WE_CustomAttrib1);
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "24 lbs", putawayTransferLine1.WE_CustomAttrib2);
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "Carton#2", putawayTransferLine1.WE_CustomAttrib3);
			AssertEquals("Putaway transfer line should be linked to the correct receive line.", receiveLine1.Inventory[0].PK, putawayTransferLine1.PickLines.Single().Inventory.PK);

			var putawayTransferLine2 = receiveLine2.PutawayTransferLine;
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "12x12x12 inches", putawayTransferLine2.WE_CustomAttrib1);
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "34 lbs", putawayTransferLine2.WE_CustomAttrib2);
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "Carton#1", putawayTransferLine2.WE_CustomAttrib3);
			AssertEquals("Putaway transfer line should be linked to the correct receive line.", receiveLine2.Inventory[0].PK, putawayTransferLine2.PickLines.Single().Inventory.PK);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_WithCustomAttributes_PutawayTransferLineCustomAttributesAreMatchWithInventory

		public void TestValidatePalletIDOnPutaway_WithCustomAttributes_PutawayTransferLineCustomAttributesAreMatchWithInventory()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.SetInventoryCustomAttributes(receiveLine1.Inventory[0], "CA11", "CA21", "CA31", "CA41", "CA51", "CA61", 11m, 12m, 13m, 14m, 15m, today.AddDays(1), today.AddDays(2), today.AddDays(3), today.AddDays(4), today.AddDays(5), true, true, true, true, true, "TB1");

			AssertEquals("Precondition: Line is Received to DockDoor", InventoryStatus.Codes.Received, receiveLine1.WE_CurrentInventoryStatus);
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine1.HasPutawayTransfer);
			var webService = GetNewWebService(data.Whs1);
			var result = GetValidatePalletIDWebServiceResponse(webService, "PLT-1");
			AssertNull("No errors from webservice call.", result.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine1.HasPutawayTransfer);

			var putawayTransferLine1 = receiveLine1.PutawayTransferLine;
			var assertMsg = "If do not copy exact attributes to transfer line, please check Inventory Committer will match with expected inventory. (WhsInventoryCommitterWithMatchingLines.AllocateStockToNewPickLines)";
			AssertEquals(assertMsg, "CA11", putawayTransferLine1.WE_CustomAttrib1);
			AssertEquals(assertMsg, "CA21", putawayTransferLine1.WE_CustomAttrib2);
			AssertEquals(assertMsg, "CA31", putawayTransferLine1.WE_CustomAttrib3);
			AssertEquals(assertMsg, "CA41", putawayTransferLine1.WE_CustomAttrib4);
			AssertEquals(assertMsg, "CA51", putawayTransferLine1.WE_CustomAttrib5);
			AssertEquals(assertMsg, "CA61", putawayTransferLine1.WE_CustomAttrib6);
			AssertEquals(assertMsg, 11m, putawayTransferLine1.WE_CustomDecimal1);
			AssertEquals(assertMsg, 12m, putawayTransferLine1.WE_CustomDecimal2);
			AssertEquals(assertMsg, 13m, putawayTransferLine1.WE_CustomDecimal3);
			AssertEquals(assertMsg, 14m, putawayTransferLine1.WE_CustomDecimal4);
			AssertEquals(assertMsg, 15m, putawayTransferLine1.WE_CustomDecimal5);
			AssertEquals(assertMsg, today.AddDays(1), putawayTransferLine1.WE_CustomDate1);
			AssertEquals(assertMsg, today.AddDays(2), putawayTransferLine1.WE_CustomDate2);
			AssertEquals(assertMsg, today.AddDays(3), putawayTransferLine1.WE_CustomDate3);
			AssertEquals(assertMsg, today.AddDays(4), putawayTransferLine1.WE_CustomDate4);
			AssertEquals(assertMsg, today.AddDays(5), putawayTransferLine1.WE_CustomDate5);
			AssertEquals(assertMsg, true, putawayTransferLine1.WE_CustomFlag1);
			AssertEquals(assertMsg, true, putawayTransferLine1.WE_CustomFlag2);
			AssertEquals(assertMsg, true, putawayTransferLine1.WE_CustomFlag3);
			AssertEquals(assertMsg, true, putawayTransferLine1.WE_CustomFlag4);
			AssertEquals(assertMsg, true, putawayTransferLine1.WE_CustomFlag5);
			AssertEquals(assertMsg, "TB1", putawayTransferLine1.WE_CustomTextBlob1);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesMovedToNewTransfer_CustomAttributes

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesMovedToNewTransfer_CustomAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 25m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");

			receiveLine1.WE_CustomAttrib1 = "23x23x12 inches";
			receiveLine1.WE_CustomAttrib2 = "24 lbs";
			receiveLine1.WE_CustomAttrib3 = "Carton#2";

			receiveLine2.WE_CustomAttrib1 = "12x12x12 inches";
			receiveLine2.WE_CustomAttrib2 = "34 lbs";
			receiveLine2.WE_CustomAttrib3 = "Carton#1";

			AssertEquals("Precondition: Line is Received to DockDoor", InventoryStatus.Codes.Received, receiveLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Line is Received to DockDoor", InventoryStatus.Codes.Received, receiveLine2.WE_CurrentInventoryStatus);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 7m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 25m);

			var inventory1 = receiveLine1.Inventory[0];
			orderLine1.ReserveStockIfAbleTo(inventory1);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLines.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition", 7m, inventory1.InDocketLine.ReservedQuantity);

			var inventory2 = receiveLine2.Inventory[0];
			var pickLine2 = orderLine2.ReserveStockIfAbleTo(inventory2);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLines.", 1, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition", 25m, inventory2.InDocketLine.ReservedQuantity);
			inventory2.WI_InDocketLineUnits = 20m;
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine1.HasPutawayTransfer);
			AssertEquals("Precondition", 25m, inventory2.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 25m, pickLine2.WZ_OriginalReservedQty);
			var webService = GetNewWebService(data.Whs1);
			var result = GetValidatePalletIDWebServiceResponse(webService, "PLT-1");
			AssertNull("No errors from webservice call.", result.ErrorMessage);
			AssertEquals("No errors from webservice call.", ErrorTypes.None, result.Error);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine1.HasPutawayTransfer);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine2.HasPutawayTransfer);

			var putawayTransferLine1 = receiveLine1.PutawayTransferLine;
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "23x23x12 inches", putawayTransferLine1.WE_CustomAttrib1);
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "24 lbs", putawayTransferLine1.WE_CustomAttrib2);
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "Carton#2", putawayTransferLine1.WE_CustomAttrib3);
			AssertEquals("Putaway transfer line should be linked to the correct receive line.", receiveLine1.Inventory[0].PK, putawayTransferLine1.PickLines.Single().Inventory.PK);
			AssertEquals("Putaway transfer line should be linked to the correct receive line.", 10m, putawayTransferLine1.WE_TransactionQuantity);
			var movedPickLine1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, orderLine1.PK)).Single();
			AssertEquals("Pick lines should move to new transfer line.", 7m, movedPickLine1.WZ_OriginalReservedQty);
			AssertEquals("Pick lines should move to new transfer line.", 7m, movedPickLine1.WZ_Units);
			AssertEquals("Reserved line should be linked to the correct order line.", putawayTransferLine1.PK, movedPickLine1.WZ_WE_InventoryLine);

			var putawayTransferLine2 = receiveLine2.PutawayTransferLine;
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "12x12x12 inches", putawayTransferLine2.WE_CustomAttrib1);
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "34 lbs", putawayTransferLine2.WE_CustomAttrib2);
			AssertEquals("Putaway Transfer Line should Custom Attribs.", "Carton#1", putawayTransferLine2.WE_CustomAttrib3);
			AssertEquals("Putaway transfer line should be linked to the correct receive line.", receiveLine2.Inventory[0].PK, putawayTransferLine2.PickLines.Single().Inventory.PK);

			var movedPickLine2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, orderLine2.PK)).Single(o => o.WZ_Units == 20m);
			AssertEquals("Pick lines should move to new transfer line.", 25m, movedPickLine2.WZ_OriginalReservedQty);
			AssertEquals("Reserved line should be linked to the correct order line.", putawayTransferLine2.PK, movedPickLine2.WZ_WE_InventoryLine);

			var unmovedPickLine2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, orderLine2.PK)).Single(o => o.WZ_Units != 20m);
			AssertEquals("Pick line should split correctly.", 5m, unmovedPickLine2.WZ_OriginalReservedQty);
			AssertEquals("Pick line should split correctly.", 5m, unmovedPickLine2.WZ_Units);
			AssertNotEquals("Over reserved should not link to the new transfer .", putawayTransferLine2.PK, unmovedPickLine2.WZ_WE_InventoryLine);

			// it create new line when split reserve line
			var newReceiveLine = receive.Lines.Single(l => l.PK != receiveLine1.PK && l.PK != receiveLine2.PK);
			AssertEquals("Over reserved should link to the new receive line.", newReceiveLine.PK, unmovedPickLine2.WZ_WE_InventoryLine);
		}

		#endregion
	}
}
