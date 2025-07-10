using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class CreateNewWhsTransfersTest : WhsTransferSecureServiceTestCase
	{
		#region TestCreateNewTransfers

		[TestDate(2012, 2, 13)]
		public void TestCreateNewTransfers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1", today.AddDays(3), today.AddDays(-4), "PA1", "PA2", "PA3", "BEK");
			inventory.WI_ArrivalDate = today.ToZDateTime().ToOffset();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var sourceLocation = "A-1";
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers(sourceLocation);
			AssertSuccessfulResponse(response, webService);

			AssertTransferCreatedSuccesfully(response, sourceLocation);
			AssertEquals(Guid.Empty, response.Transfers.Single().TaskPK);

			// check bizO
			var transfer = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsTransfer>(new ZGuid(response.Transfers[0].PK));
			AssertNotNull("New transfer should be created.", transfer);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", data.Org1.PK, transfer.WD_OH_Client);
				AssertEquals("Transfer should have correct warehouse.", data.Whs1.PK, transfer.WD_WW_Whs);
				AssertEquals("Transfer should be internal.", TransferType.Codes.Internal, transfer.WD_DocketSubType);
				AssertEquals("Transfer should not be finalised.", false, transfer.IsFinalised);
				AssertEquals("Transfer should be saved to DB.", true, transfer.IsInDatabase);
				AssertEquals("Transfer should have one line.", 1, transfer.Lines.Count);
				AssertEquals("Transfer should have committed stock.", 10m, transfer.Lines[0].GetQtyCommittedToThisLine());
			});

			AssertTransferLineMatchInventory(inventory, transfer.Lines[0], 10m, webService);

			// check response
			var transferInfo = response.Transfers[0];
			AssertNotNull("New transfer should be created.", transferInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", data.Org1.OH_Code, transferInfo.ClientCode);
				AssertEquals("No transfer lines should be passed.", false, transferInfo.Lines.Any());
			});
		}

		[TestDate(2012, 2, 13)]
		public void TestCreateNewTransfers_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT-1", today.AddDays(3), today.AddDays(-4), "PA1", "PA2", "PA3", "BEK");
			inventory.WI_SerialNumber = "SER1";
			inventory.WI_ArrivalDate = today.ToZDateTime().ToOffset();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var sourceLocation = "A-1";
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers(sourceLocation);
			AssertSuccessfulResponse(response, webService);

			AssertTransferCreatedSuccesfully(response, sourceLocation);

			// check bizO
			var transfer = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsTransfer>(new ZGuid(response.Transfers[0].PK));
			AssertNotNull("New transfer should be created.", transfer);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", data.Org1.PK, transfer.WD_OH_Client);
				AssertEquals("Transfer should have correct warehouse.", data.Whs1.PK, transfer.WD_WW_Whs);
				AssertEquals("Transfer should be internal.", TransferType.Codes.Internal, transfer.WD_DocketSubType);
				AssertEquals("Transfer should not be finalised.", false, transfer.IsFinalised);
				AssertEquals("Transfer should be saved to DB.", true, transfer.IsInDatabase);
				AssertEquals("Transfer should have one line.", 1, transfer.Lines.Count);
				AssertEquals("Transfer should have committed stock.", 1m, transfer.Lines[0].GetQtyCommittedToThisLine());
			});

			AssertTransferLineMatchInventory(inventory, transfer.Lines[0], 1m, webService);

			// check response
			var transferInfo = response.Transfers[0];
			AssertNotNull("New transfer should be created.", transferInfo);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", data.Org1.OH_Code, transferInfo.ClientCode);
				AssertEquals("No transfer lines should be passed.", false, transferInfo.Lines.Any());
			});
		}

		#endregion

		#region TestCreateNewTransfers_ForLocation

		public void TestCreateNewTransfers_ForLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, locationA2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var location1 = "A-1";
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.CreateNewWhsTransfers(location1);
			AssertSuccessfulResponse(response1, webService1);

			AssertTransferCreatedSuccesfully(response1, location1);

			var transfer1 = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsTransfer>(new ZGuid(response1.Transfers[0].PK));
			AssertNotNull(transfer1);

			CombineAssertions(() =>
			{
				AssertEquals("The Transfer should have 2 lines.", 2, transfer1.Lines.Count);
				AssertNoExceptionThrown(() => transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part1.PK && l.QtyToMoveIncludingMatchingLines == 15m));
				AssertNoExceptionThrown(() => transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part2.PK && l.QtyToMoveIncludingMatchingLines == 20m));
			});

			var location2 = "A-2";
			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.CreateNewWhsTransfers(location2);
			AssertSuccessfulResponse(response2, webService2);

			AssertTransferCreatedSuccesfully(response2, location2);

			var transfer2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsTransfer>(new ZGuid(response2.Transfers[0].PK));
			AssertNotNull(transfer2);

			CombineAssertions(() =>
			{
				AssertEquals("The Transfer should have 1 lines.", 1, transfer2.Lines.Count);
				AssertNoExceptionThrown(() => transfer2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part1.PK && l.QtyToMoveIncludingMatchingLines == 30m));
			});

			var location3 = "A-3";
			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.CreateNewWhsTransfers(location3);
			AssertSuccessfulResponse(response3, webService3);

			AssertTransferCreationFailed(response3, $"No stock found to transfer from Location Or Pallet ID '{location3}'.");
		}

		#endregion

		#region TestCreateNewTransfers_ForPalletID

		public void TestCreateNewTransfers_ForPalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var locationString = "A-1";
			var locationA1 = data.Whs1.FindLocation(locationString);

			var palletID1 = "PLT-1";
			var palletID2 = "PLT-2";
			var palletID3 = "PLT-3";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locationA1, palletID1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, palletID1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, locationA1, palletID1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, locationA1, palletID2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.CreateNewWhsTransfers(palletID1);
			AssertSuccessfulResponse(response1, webService1);

			AssertTransferCreatedSuccesfully(response1, locationString);

			var transfer1 = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsTransfer>(new ZGuid(response1.Transfers[0].PK));
			AssertNotNull(transfer1);

			CombineAssertions(() =>
			{
				AssertEquals("The Transfer should have 2 lines.", 2, transfer1.Lines.Count);
				AssertNoExceptionThrown(() => transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part1.PK && l.QtyToMoveIncludingMatchingLines == 15m));
				AssertNoExceptionThrown(() => transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part2.PK && l.QtyToMoveIncludingMatchingLines == 20m));
			});

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.CreateNewWhsTransfers(palletID2);
			AssertSuccessfulResponse(response2, webService2);

			AssertTransferCreatedSuccesfully(response2, locationString);

			var transfer2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsTransfer>(new ZGuid(response2.Transfers[0].PK));
			AssertNotNull(transfer2);

			CombineAssertions(() =>
			{
				AssertEquals("The Transfer should have 1 lines.", 1, transfer2.Lines.Count);
				transfer2.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part1.PK && l.QtyToMoveIncludingMatchingLines == 30m);
			});

			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.CreateNewWhsTransfers(palletID3);
			AssertSuccessfulResponse(response3, webService3);

			AssertTransferCreationFailed(response3, $"No stock found to transfer from Location Or Pallet ID '{palletID3}'.");
		}

		#endregion

		#region TestCreateNewTransfers_WithHeldAndDamagedInventory

		public void TestCreateNewTransfers_WithHeldAndDamagedInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var damagedInventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10m, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			var heldInventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10m, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
			var availableInventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10m, "");
			var statusChangeDamagedInventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10m, "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			statusChangeDamagedInventory.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			statusChangeDamagedInventory.InDocketLine.IsInventoryEditForm = true;

			Helper.Factory.Save();

			var sourceLocation = data.Whs1.DefaultLocation.ToLocationString();
			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewWhsTransfers(sourceLocation);
			AssertSuccessfulResponse(response, webService);

			AssertTransferCreatedSuccesfully(response, sourceLocation);

			var transfer = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsTransfer>(new ZGuid(response.Transfers[0].PK));
			AssertNotNull(transfer);

			AssertEquals("The Transfer should have 3 lines + 1 matching damaged line.", 3, transfer.Lines.Count);

			AssertNoExceptionThrown(() => transfer.Lines.Single(
				l => l.WE_OP == data.Part1.PK && l.WE_TransactionQuantity == 10m &&
				l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit &&
				l.WE_WHC_NKOriginalInventoryHeldCode == string.Empty)
			);

			var damagedLines = transfer.Lines.Where(
				l => l.WE_OP == data.Part1.PK &&
				l.WE_TransactionQuantity == 10m &&
				l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit &&
				l.WE_WHC_NKOriginalInventoryHeldCode == InventoryHoldCodes.Codes.Damaged
			);

			AssertEquals(1, damagedLines.Count());

			AssertNoExceptionThrown(() => ((WhsTransferLine)damagedLines.First()).MatchingLines.Single(
				l => l.WE_OP == data.Part1.PK &&
				l.WE_TransactionQuantity == 10m &&
				l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit &&
				l.WE_WHC_NKOriginalInventoryHeldCode == InventoryHoldCodes.Codes.Damaged
			));
		}

		#endregion

		#region TestCreateNewTransfers_WithCommittedInventory

		public void TestCreateNewTransfers_WithCommittedInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewWhsTransfers(data.Whs1.DefaultLocation.ToLocationString());
			AssertSuccessfulResponse(response, webService);

			AssertTransferCreationFailed(response, "All stock in this location is committed, reserved or putaway and cannot be transferred.");
		}

		#endregion

		#region TestCreateNewTransfers_AddEventsForTransfer

		public void TestCreateNewTransfers_AddEventsForTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1", today.AddDays(3), today.AddDays(-4), "PA1", "PA2", "PA3", "BEK");
			inventory.WI_ArrivalDate = today.ToZDateTime().ToOffset();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var sourceLocation = "A-1";
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers(sourceLocation);
			AssertSuccessfulResponse(response, webService);

			AssertTransferCreatedSuccesfully(response, sourceLocation);

			// check bizO
			var transfer = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsTransfer>(new ZGuid(response.Transfers[0].PK));
			AssertNotNull("New transfer should be created.", transfer);

			var svmLog = transfer.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == "SVM").Single();
			AssertNotNull("SVM Event log should added for the transfer", svmLog);
			AssertEquals("log should have right Parent", transfer.PK, svmLog.SL_Parent);
			AssertEquals("log should have right Reference", string.Format("{0}|TYP=Transfer", transfer.WD_DocketID), svmLog.SL_Reference);
		}

		#endregion

		#region TestCreateNewTransfers_FactorySaveError

		public void TestCreateNewTransfers_FactorySaveError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1", today.AddDays(3), today.AddDays(-4), "PA1", "PA2", "PA3", "BEK");
			inventory.WI_ArrivalDate = today.ToZDateTime().ToOffset();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var sourceLocation = "A-1";
			var webService = GetNewWebService(data.Whs1, user);

			void action(BusinessObjectFactory factory)
			{
				UnitTestUserNotification.Instance.ClearMessages();
				webService.Factory.Saving -= action;
				throw new ZCannotSaveException("Test - Cannot Save", "Test Exception");
			}
			webService.Factory.Saving += action;

			var response = webService.CreateNewWhsTransfers(sourceLocation);
			AssertBusinessValidationError(webService, "Test - Cannot Save", response);
		}

		#endregion

		#region TestCreateNewTransfers_WithReservedInventory

		public void TestCreateNewTransfers_WithReservedInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);

			Helper.CreateReservePickLine(order.Lines[0], receive.Inventory[0], 5m);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewWhsTransfers(data.Whs1.DefaultLocation.ToLocationString());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("System should identify that stock is reserved.", true, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		#endregion

		#region TestCreateNewTransfers_WithPuttingAwayInventory

		public void TestCreateNewTransfers_WithPuttingAwayInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive_Finalised = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var receive_UnFinalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive_UnFinalised, data.Part1, 10m, data.Whs1.DefaultLocation); // with PUT status
			Helper.CreateWhsReceiveInventoryLine(receive_UnFinalised, data.Part1, 10m); // with ARV status

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewWhsTransfers(data.Whs1.DefaultLocation.ToLocationString());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("System should identify that stock is reserved.", true, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		#endregion

		#region TestCreateNewTransfers_WithArrivedInventory

		public void TestCreateNewTransfers_WithArrivedInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive_UnFinalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive_UnFinalised, data.Part1.PK, 10m, ZGuid.Empty, "PLT-123"); // with ARV status

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CreateNewWhsTransfers("PLT-123");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);
				AssertNull("No transfers should be created.", response.Transfers);
			});
		}

		#endregion

		#region TestCreateNewTransfers_WithMultipleClients

		[TestDate(2012, 2, 13)]
		public void TestCreateNewTransfers_WithMultipleClients()
		{
			var rowName = "A";
			var whs = Helper.CreateWarehouse("WHS", rowName, 1, 1);

			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P2");

			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, whs, "R1", part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, whs, "R1", part2, 20m);

			Helper.Factory.Save();

			var webService = GetNewWebService(whs);
			var response = webService.CreateNewWhsTransfers(rowName);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Transfers);
			AssertTransferCreatedSuccesfully(response, rowName, 2);

			// CLIENT 1

			// transfer info
			var transfer1Info = response.Transfers.Single(t => t.ClientCode == client1.OH_Code);
			AssertNotNull("New transfer should be created.", transfer1Info);
			AssertEquals(Guid.Empty, transfer1Info.TaskPK);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", client1.OH_Code, transfer1Info.ClientCode);
				AssertEquals("No lines should be passed.", false, transfer1Info.Lines.Any());
			});

			// transfer bizO
			var transfer1 = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsTransfer>(new ZGuid(transfer1Info.PK));
			AssertNotNull("New transfer should be created.", transfer1);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", client1.PK, transfer1.WD_OH_Client);
				AssertEquals("Transfer should have correct warehouse.", whs.PK, transfer1.WD_WW_Whs);
				AssertEquals("Transfer should be internal.", TransferType.Codes.Internal, transfer1.WD_DocketSubType);
				AssertEquals("Transfer should not be finalised.", false, transfer1.IsFinalised);
				AssertEquals("Transfer should have one line.", 1, transfer1.Lines.Count);
			});

			AssertTransferLineMatchInventory(receive1.Inventory[0], transfer1.Lines[0], 10m, webService);

			// CLIENT 2

			// transfer info
			var transfer2Info = response.Transfers.Single(t => t.ClientCode == client2.OH_Code);
			AssertNotNull("New transfer should be created.", transfer2Info);
			AssertEquals(Guid.Empty, transfer2Info.TaskPK);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", client2.OH_Code, transfer2Info.ClientCode);
				AssertEquals("No lines should be passed.", false, transfer2Info.Lines.Any());
			});

			// transfer bizO
			var transfer2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsTransfer>(new ZGuid(transfer2Info.PK));
			AssertNotNull("New transfer should be created.", transfer2);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", client2.PK, transfer2.WD_OH_Client);
				AssertEquals("Transfer should have correct warehouse.", whs.PK, transfer2.WD_WW_Whs);
				AssertEquals("Transfer should be internal.", TransferType.Codes.Internal, transfer2.WD_DocketSubType);
				AssertEquals("Transfer should not be finalised.", false, transfer2.IsFinalised);
				AssertEquals("Transfer should have one line.", 1, transfer2.Lines.Count);
			});

			AssertTransferLineMatchInventory(receive2.Inventory[0], transfer2.Lines[0], 20m, webService);
		}

		#endregion

		#region TestCreateNewWhsTransfers_WithMultipleInventoryForSameProduct

		[TestDate(2016, 1, 1)]
		public void TestCreateNewWhsTransfers_WithMultipleInventoryForSameProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locationString1 = "A-1";
			var locationString2 = "A-2";
			var locationA1 = data.Whs1.FindLocation(locationString1);
			var locationA2 = data.Whs1.FindLocation(locationString2);
			var staffCode = "AA";
			var user = Helper.CreateGlbStaff(staffCode, "Antman");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locationA1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, locationA1);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, locationA2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers(locationString1);
			AssertSuccessfulResponse(response, webService);

			AssertTransferCreatedSuccesfully(response, locationString1);

			var now = new ZDateTimeOffset(2016, 1, 1);
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZGuid(response.Transfers[0].PK));
			AssertNotNull(transfer);

			CombineAssertions(() =>
			{
				AssertEquals("The Transfer should have 2 lines.", 2, transfer.Lines.Count);
				AssertEquals($"Should have Allocated and Picked all Inventory in Location {locationString1}.", 1, transfer.Lines.Cast<WhsTransferLine>().Count(l =>
					l.WE_OP == data.Part1.PK &&
					l.QtyToMoveIncludingMatchingLines == 15m &&
					l.QtyCommittedIncludingMatchingLines == 15m &&
					l.PickedTime == now &&
					l.GS_NKPickedBy == staffCode)
				);
				AssertEquals($"Should have Allocated and Picked all Inventory in Location {locationString1}.", 1, transfer.Lines.Cast<WhsTransferLine>().Count(l =>
					l.WE_OP == data.Part2.PK &&
					l.QtyToMoveIncludingMatchingLines == 20m &&
					l.QtyCommittedIncludingMatchingLines == 20m &&
					l.PickedTime == now &&
					l.GS_NKPickedBy == staffCode
				));
			});

			var inventoryLine1 = newFactory.Load<WhsReceiveLine>(inventory1.WI_WE_InDocketLine);
			var inventoryLine2 = newFactory.Load<WhsReceiveLine>(inventory2.WI_WE_InDocketLine);
			var inventoryLine3 = newFactory.Load<WhsReceiveLine>(inventory3.WI_WE_InDocketLine);

			CombineAssertions(() =>
			{
				AssertEquals("Inventory should have been Picked.", 0m, inventoryLine1.WE_StockOnHand);
				AssertEquals("Inventory should have been Picked.", 0m, inventoryLine2.WE_StockOnHand);
				AssertEquals("Inventory should have been Picked.", 0m, inventoryLine3.WE_StockOnHand);
			});
		}

		#endregion

		#region TestCreateTransfers_ShowStockOnHandWarningOnPutaway

		[TestDate(2012, 2, 13)]
		public void TestCreateTransfers_ShowStockOnHandWarningOnPutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationString1 = "A-1";
			var locationString2 = "A-2";
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var today = ZDate.Today;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.FindLocation(locationString1), "PLT-1", today.AddDays(3), today.AddDays(-4), "PA1", "PA2", "PA3", "BEK");
			inventory1.WI_ArrivalDate = today.ToZDateTime().ToOffset();
			receive1.FinaliseDocket();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.FindLocation(locationString2), "PLT-2", today.AddDays(3), today.AddDays(-4), "PA1", "PA2", "PA3", "BEK");
			inventory2.WI_ArrivalDate = today.ToZDateTime().ToOffset();
			receive2.FinaliseDocket();

			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			Helper.Factory.Save();

			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var webService1 = GetNewWebService(data.Whs1, user);
			var response1 = webService1.CreateNewWhsTransfers(locationString1);
			AssertSuccessfulResponse(response1, webService1);

			AssertTransferCreatedSuccesfully(response1, locationString1);
			AssertEquals(true, response1.ShowStockOnHandWarningOnPutaway);

			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var webService2 = GetNewWebService(data.Whs1, user);
			var response2 = webService2.CreateNewWhsTransfers(locationString2);
			AssertSuccessfulResponse(response2, webService2);

			AssertTransferCreatedSuccesfully(response2, locationString2);
			AssertEquals(false, response2.ShowStockOnHandWarningOnPutaway);
		}

		#endregion

		#region TestCreateNewTransfers_OrgSupplierPartsAreInactive

		public void TestCreateNewTransfers_OrgSupplierPartsAreInactive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationString = "A-1";

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation(locationString), "PLT-1", today.AddDays(3), today.AddDays(-4), "PA1", "PA2", "PA3", "BEK");
			inventory.WI_ArrivalDate = today.ToZDateTime().ToOffset();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			data.Part1.OP_IsActive = false;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers(locationString);
			AssertSuccessfulResponse(response, webService);

			AssertTransferCreationFailed(response, "Stock was found with Inactive Product, Set Product to Active for use.");
			AssertEquals("Stock has been commited with an inactive part.", false, response.IsStockCommittedOrReserved);
		}

		#endregion

		#region TestCreateNewTransfers_InventoryStatus

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_ReceivedToDDL_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			AssertEquals("Precondition - ensure inventory is Received to DDL.", "REC", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("PLT-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);

				AssertEquals("System should identify that stock is Not reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is not found.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_ReceivedToDDL_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			AssertEquals("Precondition - ensure inventory is Received to DDL.", "REC", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("DOCKDOOR");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);

				AssertEquals("System should identify that stock is not reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is not found.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_InTransit_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var locA1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locA1, "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var othertransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TRO");
			var otherTransferLine = Helper.CreateWhsTransferLineWithInTransitInventory(othertransfer, data.Part1, 50m, locA1, "PLT-1", data.Whs1.FindLocation("A-2"), "PLT-1", user);
			AssertEquals("Precondition - ensure inventory is In-Transit.", "INT", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("A-2");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock found to transfer from Location Or Pallet ID 'A-2'.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_InTransit_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locA1, "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var othertransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TRO");
			var otherTransferLine = Helper.CreateWhsTransferLineWithInTransitInventory(othertransfer, data.Part1, 50m, locA1, "PLT-1", locA2, "PLT-1", user);
			AssertEquals("Precondition - ensure inventory is In-Transit.", "INT", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("PLT-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock found to transfer from Location Or Pallet ID 'PLT-1'.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_PuttingAway_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var otherTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			otherTransfer.WD_IsPutawayTransfer = true;
			var otherTransferLine = Helper.SetupTransferLineForDockDoorLocation(otherTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			otherTransfer.RunPreSaveValidation();
			otherTransferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Putting Away.", "PTA", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("A-2");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock found to transfer from Location Or Pallet ID 'A-2'.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_PuttingAway_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var otherTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			otherTransfer.WD_IsPutawayTransfer = true;
			var otherTransferLine = Helper.SetupTransferLineForDockDoorLocation(otherTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			otherTransfer.RunPreSaveValidation();
			otherTransferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Putting Away.", "PTA", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("PLT-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("No stock found to transfer from Location Or Pallet ID 'PLT-1'.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is not found.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_Putaway_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var otherTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			otherTransfer.WD_IsPutawayTransfer = true;
			var otherTransferLine = Helper.SetupTransferLineForDockDoorLocation(otherTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			otherTransfer.RunPreSaveValidation();
			otherTransferLine.PickedTime = ZDateTimeOffset.Now;
			otherTransfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(otherTransfer);
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Putaway.", "PUT", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("A-2");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is no found.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_Putaway_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var otherTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			otherTransfer.WD_IsPutawayTransfer = true;
			var otherTransferLine = Helper.SetupTransferLineForDockDoorLocation(otherTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			otherTransfer.RunPreSaveValidation();
			otherTransferLine.PickedTime = ZDateTimeOffset.Now;
			otherTransfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(otherTransfer);
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Putaway.", "PUT", otherTransferLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("PLT-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is not found.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_Staged_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var inventoryLine = receive.Lines[0];
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var stagedInventoryLine = Helper.PickAndMakeInTransitTransfer(order.Lines[0].PickLines.Single(), ZDateTimeOffset.Now);

			var ddlTransfer = pick.Transfers.Single();
			ddlTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure inventory is Staged.", InventoryStatus.Codes.Staged, stagedInventoryLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("DOCKDOOR");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_Staged_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTimeOffset.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var inventoryLine = receive.Lines[0];
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var stagedInventoryLine = Helper.PickAndMakeInTransitTransfer(order.Lines[0].PickLines.Single(), now);

			var ddlTransfer = pick.Transfers.Single();
			ddlTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure inventory is Putaway.", InventoryStatus.Codes.Staged, stagedInventoryLine.Inventory[0].WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("PLT-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_ReceivedToDDLNotDefaultDDL_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var otherDDL = data.Whs1.FindLocation("A-1");
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure otherDDL is DDL.", true, otherDDL.IsDockDoorLocation);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, otherDDL, "PLT-1");
			AssertEquals("Precondition - ensure inventory is Received to DDL.", "REC", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("A-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_ReceivedToDDLNotDefaultDDL_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var otherDDL = data.Whs1.FindLocation("A-1");
			otherDDL.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure otherDDL is DDL.", true, otherDDL.IsDockDoorLocation);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, otherDDL, "PLT-1");
			AssertEquals("Precondition - ensure inventory is Received to DDL.", "REC", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("PLT-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_DirectPutaway_Location()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			AssertEquals("Precondition - ensure inventory is Putaway.", "PUT", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("A-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		[TestDate(2012, 6, 5)]
		public void TestCreateNewTransfers_InventoryStatus_DirectPutaway_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var now = ZDateTime.Now;
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"), "PLT-1");
			AssertEquals("Precondition - ensure inventory is Putaway.", "PUT", inventory.WI_InventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("PLT-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);

				AssertEquals("System should identify that stock is reserved.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		#endregion

		[TestDate(2024, 2, 13)]
		public void TestCreateNewTransfers_TransferPutaway()
		{
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, dockDoorLocation, "PLT1", false,	false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, nonDockDoorLocation, "PLT2");
			Helper.Factory.Save();

			var transferPutaway = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transferPutaway.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transferPutaway, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "PLT1", 10m);
			transferLine.FinaliseDocketLine();
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, receive1.Lines[0].WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, transferLine.Inventory[0].WI_InventoryStatus);
			AssertEquals("Precondition", 10m, transferLine.Inventory[0].WI_TotalUnits);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("A-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals("System should identify that stock is reserved.", true, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created if stock is reserved.", response.Transfers);
			});
		}

		[TestDate(2024, 2, 13)]
		public void TestCreateNewTransfers_TransferPutaway_PutawayOnly()
		{
			var user = Helper.CreateGlbStaff("A.A", "AAA");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, dockDoorLocation, "PLT1", false, false);
			Helper.Factory.Save();

			var transferPutaway = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transferPutaway.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transferPutaway, data.Part1, dockDoorLocation,
				nonDockDoorLocation, "PLT1", 10m);
			transferLine.FinaliseDocketLine();
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, receive1.Lines[0].WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, transferLine.Inventory[0].WI_InventoryStatus);
			AssertEquals("Precondition", 10m, transferLine.Inventory[0].WI_TotalUnits);

			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers("A-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All stock in this location is committed, reserved or putaway and cannot be transferred.", response.ErrorMessage);

				AssertEquals("Should identify that stock is putaway.", false, response.IsStockCommittedOrReserved);
				AssertNull("No transfers should be created for stock that has already been put away.", response.Transfers);
			});
		}

		[TestDate(2025, 5, 19)]
		public void TestCreateNewTransfers_TaskManagementEnabled()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1", today.AddDays(3), today.AddDays(-4), "PA1", "PA2", "PA3", "BEK");
			inventory.WI_ArrivalDate = today.ToZDateTime().ToOffset();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var sourceLocation = "A-1";
			var webService = GetNewWebService(data.Whs1, user);
			var response = webService.CreateNewWhsTransfers(sourceLocation);
			AssertSuccessfulResponse(response, webService);

			AssertTransferCreatedSuccesfully(response, sourceLocation);

			// check bizO
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transfer = newFactory.Load<WhsTransfer>(new ZGuid(response.Transfers[0].PK));
			AssertNotNull("New transfer should be created.", transfer);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", data.Org1.PK, transfer.WD_OH_Client);
				AssertEquals("Transfer should have correct warehouse.", data.Whs1.PK, transfer.WD_WW_Whs);
				AssertEquals("Transfer should be internal.", TransferType.Codes.Internal, transfer.WD_DocketSubType);
				AssertEquals("Transfer should not be finalised.", false, transfer.IsFinalised);
				AssertEquals("Transfer should be saved to DB.", true, transfer.IsInDatabase);
				AssertEquals("Transfer should have one line.", 1, transfer.Lines.Count);
				AssertEquals("Transfer should have committed stock.", 10m, transfer.Lines[0].GetQtyCommittedToThisLine());
			});

			var transferLine = transfer.Lines[0];
			AssertTransferLineMatchInventory(inventory, transferLine, 10m, webService);
			AssertNotEquals(Guid.Empty, transferLine.WE_P9_Task);

			// check response
			var transferInfo = response.Transfers[0];
			AssertNotNull("New transfer should be created.", transferInfo);
			AssertNotEquals(Guid.Empty, transferInfo.TaskPK);

			AssertEquals("Process task is created and assigned to the transfer line.", transferLine.WE_P9_Task, transferInfo.TaskPK);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", data.Org1.OH_Code, transferInfo.ClientCode);
				AssertEquals("No transfer lines should be passed.", false, transferInfo.Lines.Any());
			});

			var transferTaskProcess = newFactory.Load<WhsTransferProcessTasks>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Single();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferTaskProcess.P9_Status);
			AssertEquals("A.A", transferTaskProcess.P9_GS_NKAssignedStaffMember);
		}

		[TestDate(2025, 5, 19)]
		public void TestCreateNewTransfers_TaskManagementEnabled_MultipleClients()
		{
			var rowName = "A";
			var whs = Helper.CreateWarehouse("WHS", rowName, 1, 1);
			var user = Helper.CreateGlbStaff("A.A", "AAA");

			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P2");

			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, whs, "R1", part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, whs, "R1", part2, 20m);

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			whs.WW_GG_ReleaseGroup = releaseGroup.PK;

			Helper.Factory.Save();

			var webService = GetNewWebService(whs, user);
			var response = webService.CreateNewWhsTransfers(rowName);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Transfers);
			AssertTransferCreatedSuccesfully(response, rowName, 2);

			// CLIENT 1

			// transfer info
			var transfer1Info = response.Transfers.Single(t => t.ClientCode == client1.OH_Code);
			AssertNotNull("New transfer should be created.", transfer1Info);
			AssertNotEquals(Guid.Empty, transfer1Info.TaskPK);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", client1.OH_Code, transfer1Info.ClientCode);
				AssertEquals("No lines should be passed.", false, transfer1Info.Lines.Any());
			});

			// transfer bizO
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transfer1 = newFactory.Load<WhsTransfer>(new ZGuid(transfer1Info.PK));
			AssertNotNull("New transfer should be created.", transfer1);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", client1.PK, transfer1.WD_OH_Client);
				AssertEquals("Transfer should have correct warehouse.", whs.PK, transfer1.WD_WW_Whs);
				AssertEquals("Transfer should be internal.", TransferType.Codes.Internal, transfer1.WD_DocketSubType);
				AssertEquals("Transfer should not be finalised.", false, transfer1.IsFinalised);
				AssertEquals("Transfer should have one line.", 1, transfer1.Lines.Count);
			});

			var transfer1TaskProcess = newFactory.Load<WhsTransferProcessTasks>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer1.PK)).Single();
			var transfer1Line = transfer1.Lines[0];
			AssertTransferLineMatchInventory(receive1.Inventory[0], transfer1Line, 10m, webService);
			AssertEquals(transfer1TaskProcess.PK, transfer1Line.WE_P9_Task);

			// CLIENT 2

			// transfer info
			var transfer2Info = response.Transfers.Single(t => t.ClientCode == client2.OH_Code);
			AssertNotNull("New transfer should be created.", transfer2Info);
			AssertNotEquals(Guid.Empty, transfer2Info.TaskPK);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", client2.OH_Code, transfer2Info.ClientCode);
				AssertEquals("No lines should be passed.", false, transfer2Info.Lines.Any());
			});

			// transfer bizO
			var transfer2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsTransfer>(new ZGuid(transfer2Info.PK));
			AssertNotNull("New transfer should be created.", transfer2);

			CombineAssertions(() =>
			{
				AssertEquals("Transfer should have correct client.", client2.PK, transfer2.WD_OH_Client);
				AssertEquals("Transfer should have correct warehouse.", whs.PK, transfer2.WD_WW_Whs);
				AssertEquals("Transfer should be internal.", TransferType.Codes.Internal, transfer2.WD_DocketSubType);
				AssertEquals("Transfer should not be finalised.", false, transfer2.IsFinalised);
				AssertEquals("Transfer should have one line.", 1, transfer2.Lines.Count);
			});

			var transfer2TaskProcess = newFactory.Load<WhsTransferProcessTasks>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer2.PK)).Single();
			var transfer2Line = transfer2.Lines[0];
			AssertTransferLineMatchInventory(receive2.Inventory[0], transfer2Line, 20m, webService);
			AssertEquals(transfer2TaskProcess.PK, transfer2Line.WE_P9_Task);

			AssertEquals(transfer1TaskProcess.PK, transfer1Info.TaskPK);
			AssertEquals(transfer1TaskProcess.PK, transfer2Info.TaskPK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transfer1TaskProcess.P9_Status);
			AssertEquals("A.A", transfer1TaskProcess.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transfer2TaskProcess.P9_Status);
			AssertEquals("A.A", transfer2TaskProcess.P9_GS_NKAssignedStaffMember);
		}

		#region Asserts

		void AssertTransferLineMatchInventory(WhsInventoryView inventory, WhsTransferLine transferLine, ZDecimal expectedUnits, WhsSecureService webService)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Product", inventory.WI_OP, transferLine.WE_OP);
				AssertEquals("Source Location", inventory.WI_WL, transferLine.WE_WL_TransferFrom);
				AssertEquals("Source PalletID", inventory.WI_PalletID, transferLine.WE_TransferFromPalletId);
				AssertEquals("Quantity", expectedUnits, transferLine.WE_TransactionQuantity);
				AssertEquals("UQ", inventory.WI_UnitsUQ, transferLine.ProductUQ);
				AssertEquals("Part Attrib 1", inventory.WI_PartAttrib1, transferLine.WE_PartAttrib1);
				AssertEquals("Part Attrib 2", inventory.WI_PartAttrib2, transferLine.WE_PartAttrib2);
				AssertEquals("Part Attrib 3", inventory.WI_PartAttrib3, transferLine.WE_PartAttrib3);
				AssertEquals("Serial Number", inventory.WI_SerialNumber, transferLine.WE_SerialNumber);
				AssertEquals("Expiry Date", inventory.WI_ExpiryDate, transferLine.WE_ExpiryDate);
				AssertEquals("Packing Date", inventory.WI_PackingDate, transferLine.WE_PackingDate);
				AssertEquals("Picked By", webService.SecurityHeader.UserName, transferLine.PickedBy.GS_LoginName);
				AssertEquals("Picked By Date", ZDateTimeOffset.Today, transferLine.PickedTime);
			});
		}

		void AssertTransferCreatedSuccesfully(WhsTransfersWebServiceResponse response, string expectedSourceLocation, int expectedTransfers = 1)
		{
			AssertNotNull(response.Transfers);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));

				AssertEquals($"Only {expectedTransfers} transfer should be created.", expectedTransfers, response.Transfers.Length);
				AssertEquals("Source Location should be passed back.", expectedSourceLocation, response.SourceLocation);
				AssertEquals("No stock is committed and system should let user know.", false, response.IsStockCommittedOrReserved);
			});
		}

		void AssertTransferCreationFailed(WhsTransfersWebServiceResponse response, string expectedError)
		{
			AssertEquals(expectedError, response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertNull(response.Transfers);
		}

		#endregion
	}
}

