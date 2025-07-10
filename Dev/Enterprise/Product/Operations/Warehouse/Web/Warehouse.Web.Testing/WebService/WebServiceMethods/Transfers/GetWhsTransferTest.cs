using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetWhsTransferTest : WhsSecureServiceTestCase
	{
		#region TestGetWhsTransfer

		public void TestGetWhsTransfer()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_Created = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine_HeldForTransfer = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine_HeldForTransfer.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation(); // to commit inventory

			var transferLine_Finalised = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine_Finalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine_Finalised);

			webService.Factory.Save();

			var expectedErrorMessage = "Can't find un finalized Transfer with Reference: {0}.";

			var response1 = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find transfer by its external reference.", transfer.PK.ToGuid(), response1.Docket.PK);
			AssertEquals("Only lines that need Allocation should be send to device.", 1, response1.Docket.Lines.Count);
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine_Created.PK.ToGuid()));
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);

			var response2 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find transfer by its docket ID.", transfer.PK.ToGuid(), response2.Docket.PK);
			AssertEquals("Only lines that need Allocation should be send to device.", 1, response2.Docket.Lines.Count);
			response2.Docket.Lines.Single(l => l.PK.Equals(transferLine_Created.PK.ToGuid()));
			transferLine_Created.PickedTime = ZDateTimeOffset.Now; // change line to be picked.
			AssertTransferEventsCreated(transfer, 1, 2, 0, 0, 0, 0);

			var response3 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find transfer by its docket ID.", transfer.PK.ToGuid(), response3.Docket.PK);
			AssertEquals("If no lines that need Allocation could be found, system should find not transferred lines.", 2, response3.Docket.Lines.Count);
			response3.Docket.Lines.Single(l => l.PK.Equals(transferLine_Created.PK.ToGuid()));
			response3.Docket.Lines.Single(l => l.PK.Equals(transferLine_HeldForTransfer.PK.ToGuid()));
			AssertTransferEventsCreated(transfer, 1, 3, 0, 0, 0, 0);

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			webService.Factory.Save();
			AssertTransferEventsCreated(transfer, 1, 3, 0, 0, 1, 0);

			AssertBusinessValidationError(webService, string.Format(expectedErrorMessage, transfer.WD_DocketID),
				webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null));

			AssertBusinessValidationError(webService, string.Format(expectedErrorMessage, "TEST DESC"),
				webService.GetWhsTransfer("TEST DESC", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null));
		}

		void AssertTransferEventsCreated(WhsTransfer transfer, int countOfJobEnteredEvent, int countOfServiceCommenced, int countOfServiceSuspended, int countOfChangeOfIdentifier, int countOfJobFinalized, int countOfServiceCompleted)
		{
			AssertTransferEventLog(transfer, Events.WarehouseJobEntered, countOfJobEnteredEvent);
			AssertTransferEventLog(transfer, Events.ServiceCommenced, countOfServiceCommenced);
			AssertTransferEventLog(transfer, Events.ServiceSuspended, countOfServiceSuspended);
			AssertTransferEventLog(transfer, Events.ChangeOfIdentifier, countOfChangeOfIdentifier);
			AssertTransferEventLog(transfer, Events.ItemDocumentJobFinalised, countOfJobFinalized);
			AssertTransferEventLog(transfer, Events.ServiceCompleted, countOfServiceCompleted);
		}

		void AssertTransferEventLog(WhsTransfer transfer, Event expectedEventType, int expectedEventCount)
		{
			AssertEquals(string.Format("Should find {0} of Event: {1}.", expectedEventCount, expectedEventType.Description), expectedEventCount, Helper.FindLogs(transfer.Logs, expectedEventType).Length);
		}

		#endregion

		#region TestGetWhsTransferExcludeExceptedPKs

		public void TestGetWhsTransferExcludeExceptedPKs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("A", "A");
			var webService1 = GetNewWebService(data.Whs1, staff);
			var helper = new WhsTestHelperFunctions(Helper.Factory);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_Created = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine_HeldForTransfer = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine_HeldForTransfer.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation(); // to commit inventory

			var transferLine_Finalised = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine_Finalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine_Finalised);

			Helper.Factory.Save();

			var expectedErrorMessage = "Un-finalized transfer could not be found. Possible mismatch on registered equipment, registered area, registered client or transfers have been assigned to another operator.";

			var response1 = webService1.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find transfer without external reference.", transfer.PK.ToGuid(), response1.Docket.PK);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, new Guid[] { Guid.NewGuid() });
			AssertEquals("System should find transfer if is not excluded.(other transfer excluded)", transfer.PK.ToGuid(), response2.Docket.PK);

			var webService3 = GetNewWebService(data.Whs1, staff);
			var response3 = webService3.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, new Guid[] { transfer.PK.ToGuid() });
			AssertEquals("System should find transfer by its external reference even ask to exclude.(only exclude if we do not send refrence)", transfer.PK.ToGuid(), response3.Docket.PK);

			var webService4 = GetNewWebService(data.Whs1, staff);
			var response4 = webService4.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, new Guid[] { transfer.PK.ToGuid() });
			AssertEquals("System should find transfer by docket id even ask to exclude.(only excluse if we do not send refrence)", transfer.PK.ToGuid(), response4.Docket.PK);

			var webService5 = GetNewWebService(data.Whs1, staff);
			AssertBusinessValidationError(webService5, string.Format(expectedErrorMessage, transfer.WD_DocketID), webService5.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, new Guid[] { transfer.PK.ToGuid() }));
		}

		#endregion

		#region TestGetWhsTransfer_PalletIdNeutral

		public void TestGetWhsTransfer_PalletIdNeutral()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_MultipleNeutralPallets()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-001");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-002");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location1, "PLT-003");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 10m, location2, "PLT-004");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m, location2, "PLT-005");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part1, 10m, location2, "PLT-006");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-002", "", "");
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-003", "", "");
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-2", "PLT-006", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-001", "PLT-002", "PLT-003", "PLT-004", "PLT-005", "PLT-006" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_DoesNotConsiderPalletsAllocatedToRFGun()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			pickLine.WZ_IsPicking = true;
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_DoesNotConsiderInTransitPallets()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location1, "PLT-789");
			webService.Factory.Save();

			var transfer1 = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer1.RunPreSaveValidation();

			var transfer2 = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var inTransitPallet = helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, "A-1", "PLT-456", "", "");
			inTransitPallet.PickedTime = ZDateTimeOffset.Now;
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer1.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-789" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_DoesNotConsiderPuttingAwayPallets()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-789");
			webService.Factory.Save();

			var putawayTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, location1, "PLT-789", 10m);
			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_DoesNotConsiderUnfinalisedReceives()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT-789");
			AssertEquals("Precondition: Has Stock on Hand.", 10m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: Is Putaway.", InventoryStatus.Codes.Putaway, receiveLine.WE_CurrentInventoryStatus);
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_DoesNotConsiderInTransitPalletsOnTheTransferForPalletNeutral()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-2", "PLT-789", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-789" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_DoesNotConsiderNonPalletLinesOnTheTransferForPalletNeutral()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location2, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location1, "");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "", "", "");
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-2", "PLT-456", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-456" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_MustMatchProduct()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_MustMatchPartAttribs()
		{
			var year = ZDateTime.Today.Year;
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-001", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-002", new ZDate(year, 2, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-003", new ZDate(year, 1, 1), new ZDate(year, 2, 2), "PA1", "PA2", "PA3", "");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-004", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "XXX", "PA2", "PA3", "");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-005", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "XXX", "PA3", "");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-006", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "XXX", "");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-007", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, "PLT-001", location2, new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-001", "PLT-007" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_MustMatchHeldCode()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;

			helper.CreateInventoryHeldCode("COVID", "Corona Virus");
			helper.CreateInventoryHeldCode("FLU", "Influenza");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-001", "PUT", "COVID");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-002", "PUT", "COVID");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-003", "PUT", "FLU");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-001", "A-2", "", "COVID");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-001", "PLT-002" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_MustMatchQuantity()
		{
			var year = ZDateTime.Today.Year;
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-001");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 11m, location1, "PLT-002");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-003");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 9m, location1, "PLT-004");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location1, "PLT-005");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, location1, "PLT-005");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-006");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, location1, "PLT-006");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-001", "A-2", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-001", "PLT-003" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_DoesNotNeedToMatchSerials()
		{
			var year = ZDateTime.Today.Year;
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "PLT-001", ZDate.Empty, ZDate.Empty, "BIG", "RED", "LONG", "");
			line1.WI_SerialNumber = "SN1";
			var line2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "PLT-002", ZDate.Empty, ZDate.Empty, "BIG", "RED", "LONG", "");
			line2.WI_SerialNumber = "SN2";
			var line3 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "PLT-003", ZDate.Empty, ZDate.Empty, "BIG", "RED", "LONG", "");
			line3.WI_SerialNumber = "SN3";
			var line4 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "PLT-004", ZDate.Empty, ZDate.Empty, "BIG", "RED", "LONG", "");
			line4.WI_SerialNumber = "SN4";
			var line5 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "PLT-005", ZDate.Empty, ZDate.Empty, "SMALL", "RED", "LONG", "");
			line5.WI_SerialNumber = "SN5";
			var line6 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "PLT-006", ZDate.Empty, ZDate.Empty, "BIG", "BLUE", "LONG", "");
			line6.WI_SerialNumber = "SN6";
			var line7 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "PLT-007", ZDate.Empty, ZDate.Empty, "BIG", "RED", "SHORT", "");
			line7.WI_SerialNumber = "SN7";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location1, "PLT-001", location2, ZDate.Empty, ZDate.Empty, "BIG", "RED", "LONG");
			transferLine1.WE_SerialNumber = "SN1";
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location1, "PLT-002", location2, ZDate.Empty, ZDate.Empty, "BIG", "RED", "LONG");
			transferLine2.WE_SerialNumber = "SN2";
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-001", "PLT-002", "PLT-003", "PLT-004" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_DoesNotConsiderPalletsCommittedToAdjustment()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();

			var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, "A-1", "PLT-456");
			adjustment.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_DoesNotConsiderPalletsCommittedToAnotherTransfer()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();

			var differentTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(differentTransfer, data.Part1, 1m, "A-1", "PLT-456", "", "");
			differentTransfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_AllowsChangingToPalletCommittedToOrder()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var palletIdNeutralType = helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;
			location2.WLV_WLT_LocationType = palletIdNeutralType.PK;
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
			helper.CreatePickNew(order);

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456" },
				response.Docket.PalletsToTransferCompletely);
		}

		public void TestGetWhsTransfer_PalletIdNeutral_NonNeutralLocation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-123");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, DefaultCriteria, false, Array.Empty<Guid>());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123" },
				response.Docket.PalletsToTransferCompletely);
		}

		#endregion

		#region TestGetWhsTransfer_InterWhsChild

		public void TestGetWhsTransfer_InterWhsChild_Source()
		{
			TestGetWhsTransfer_InterWhsChild(isSource: true);
		}

		public void TestGetWhsTransfer_InterWhsChild_Dest()
		{
			TestGetWhsTransfer_InterWhsChild(isSource: false);
		}

		void TestGetWhsTransfer_InterWhsChild(bool isSource)
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var warehouse2 = helper.CreateWarehouse("WH2", "A", 1, 1);
			helper.CreateWhsReceiveWithInventory(data.Org1, isSource ? data.Whs1 : warehouse2, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", warehouse2.PK, "A");
			transferLine.RunPreSaveValidation();

			var now = ZDateTimeOffset.Now;
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var childTransfer = transfer.ChildTransfers.First();
			AssertNotNull("Precondition: Created child transfer.", childTransfer);
			webService.Factory.Save();

			webService.SecurityHeader.WarehouseCode = warehouse2.WW_WarehouseCode;
			AssertBusinessValidationError(webService, $"Cannot transfer an Inter-Warehouse Transfer using the child job.", webService.GetWhsTransfer(childTransfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null));

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find transfer by its external reference.", transfer.PK.ToGuid(), response1.Docket.PK);
		}

		#endregion

		#region TestGetWhsTransfer_DifferentClients

		public void TestGetWhsTransfer_DifferentClients()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 3, 1);
			var org2 = helper.CreateClient("O2");
			helper.CreateProductClientRelationShip(org2, data.Part1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "", false, true);
			helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 10m, locationA1, "", false, true);

			var transferForOrg1 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transferForOrg1, data.Part1, 1m, locationA1, locationA2);
			transferForOrg1.RunPreSaveValidation(); // to commit inventory

			var transferForOrg2 = helper.CreateWhsTransfer(org2, data.Whs1, "TR2", Notify);
			helper.CreateWhsTransferLine(transferForOrg2, data.Part1, 1m, locationA1, locationA2);
			transferForOrg2.RunPreSaveValidation(); // to commit inventory
			webService.Factory.Save();

			var expectedErrorMessage = "Can't find un finalized Transfer with Reference: {0}.";

			var response1 = webService.GetWhsTransfer(transferForOrg1.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for Org1 should be returned.", transferForOrg1.WD_DocketID, response1.Docket.DocketID);

			var response2 = webService.GetWhsTransfer(transferForOrg2.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "" }, false, null);
			AssertEquals("Transfer for Org2 should be returned.", transferForOrg2.WD_DocketID, response2.Docket.DocketID);

			AssertBusinessValidationError(webService, string.Format(expectedErrorMessage, transferForOrg1.WD_DocketID), webService.GetWhsTransfer(transferForOrg1.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "O2" }, false, null));
		}

		#endregion

		#region TestGetWhsTransfer_DifferentAreas

		public void TestGetWhsTransfer_DifferentAreas()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 3, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var area1 = helper.CreateArea(data.Whs1, "AREA1");
			var area2 = helper.CreateArea(data.Whs1, "AREA2");
			var area3 = helper.CreateArea(data.Whs1, "AREA3"); // no transfer lines for this area.

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			locationA2.WLV_WA_PickingArea = area1.PK;
			locationA3.WLV_WA_PickingArea = area2.PK;
			locationA3.WLV_PickMethod = "PM3";

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA2);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA3);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2); // From default to Area1
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA2, locationA3); // From Area1 to Area2
			var transferLine3 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2"); // From default to Area1
			var transferLine4 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3"); // From default to Area2
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLine3.PickedTime = ZDateTimeOffset.Now;
			transferLine4.PickedTime = ZDateTimeOffset.Now;
			webService.Factory.Save();

			var expectedErrorMessage = "Un-finalized transfer could not be found for Reference / Pallet ID : {0}. Possible mismatch on registered equipment, registered area, registered client or transfer / Pallet ID  has been assigned to another operator";

			var response1 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("When Area is Any, then user should receive lines from all areas.", 2, response1.Docket.Lines.Count);
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));

			var response2 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "DEFAULT", PickMethod = "ANY" }, false, null);
			AssertEquals("When Area is specified, then user should receive lines from that area alone. System should not care about dest area.", 1, response2.Docket.Lines.Count);
			response2.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));

			var response3 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "AREA1", PickMethod = "ANY" }, false, null);
			AssertEquals("When Area is specified, then user should receive lines from that area alone. System should not care about dest area.", 1, response3.Docket.Lines.Count);
			response3.Docket.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));

			var response4 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "WrongAreaName", PickMethod = "ANY" }, false, null);
			AssertEquals("When Area is specified, then user should receive lines from all areas alone, putaway linees and picking lines for all areas.", 2, response4.Docket.Lines.Count);
			response4.Docket.Lines.Single(l => l.PK.Equals(transferLine4.PK.ToGuid()));

			// can putaway in new area, System should not care about dest area.
			var response5 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "AREA3", PickMethod = "WRONGPICK" }, false, null);
			AssertSuccessfulResponse(response5, webService);
			AssertEquals(ErrorTypes.None, response5.Error);

			// no lines match with PickMethod even does not check the area.
			locationA1.WLV_PickMethod = "PM1";
			locationA2.WLV_PickMethod = "PM2";
			locationA3.WLV_PickMethod = "PM3";
			AssertBusinessValidationError(webService, string.Format(expectedErrorMessage, transfer.WD_DocketID), webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "AREA3", PickMethod = "WRONGPICK" }, false, null));
		}

		public void TestGetWhsTransfer_DifferentAreas_EmptyLocation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.WLV_LocationString, ""); // From ANY to {null}
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "PM3", PickMethod = "ANY" }, false, null);
			AssertEquals("Empty locations should be treated the same as an ALL location, as the user can choose the locations.", 1, response.Docket.Lines.Count);
			AssertNotNull(response.Docket.Lines.SingleOrDefault(l => l.PK.Equals(transferLine1.PK.ToGuid())));
		}

		#endregion

		#region TestGetWhsTransfer_DifferentPickMethods

		public void TestGetWhsTransfer_DifferentPickMethods()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 4, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			var locationA4 = data.Whs1.FindLocation("A-4");
			locationA2.WLV_PickMethod = "PM1";
			locationA3.WLV_PickMethod = "PM2";
			locationA4.WLV_PickMethod = "PM3";

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA2);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA3);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2); // From ANY to LADDER
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA2, locationA3); // From LADDER to FORK LIFT
			var transferLine3 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA3, locationA1); // From FORK LIFT to ANY
			var transferLine4 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2"); // From ANY to LADDER
			var transferLine5 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-4"); // From ANY to HIGH LADDER
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLine4.PickedTime = ZDateTimeOffset.Now;
			transferLine5.PickedTime = ZDateTimeOffset.Now;
			webService.Factory.Save();

			var expectedErrorMessage = "Un-finalized transfer could not be found for Reference / Pallet ID : {0}. Possible mismatch on registered equipment, registered area, registered client or transfer / Pallet ID  has been assigned to another operator";

			var response1 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("When Pick Method is Any, then user should receive lines from all locations.", 3, response1.Docket.Lines.Count);
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine3.PK.ToGuid()));

			var response2 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "PM1" }, false, null);
			AssertEquals("When Pick Method is specified, then user should receive lines from locations for this pick method + locations with ANY pick method.", 2, response2.Docket.Lines.Count);
			response2.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
			response2.Docket.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));

			transferLine1.FinaliseDocketLine(); // finalise line with ANY status so it will not interfere with putaway lines.
			AssertIsFinalisedPrecondition(transferLine1);

			var response3 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "PM3" }, false, null);
			AssertEquals("When Pick Method is specified, then user should receive lines from locations for this pick method + locations with ANY pick method" +
							", even if it is putaway lines and not picked lines from other locations exist.", 1, response3.Docket.Lines.Count);
			response3.Docket.Lines.Single(l => l.PK.Equals(transferLine5.PK.ToGuid()));

			// no lines for this Pick Method.
			AssertBusinessValidationError(webService, string.Format(expectedErrorMessage, transfer.WD_DocketID), webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "PM9" }, false, null));
		}

		public void TestGetWhsTransfer_DifferentPickMethods_EmptyLocation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.WLV_LocationString, ""); // From ANY to {null}
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "PM3" }, false, null);
			AssertEquals("Empty locations should be treated the same as an ALL location, as the user can choose the locations.", 1, response.Docket.Lines.Count);
			AssertNotNull(response.Docket.Lines.SingleOrDefault(l => l.PK.Equals(transferLine1.PK.ToGuid())));
		}

		#endregion

		#region TestGetWhsTransfer_DifferentUsers

		public void TestGetWhsTransfer_DifferentUsers()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");

			var staff1 = helper.CreateGlbStaff("S1", "S1");
			var staff2 = helper.CreateGlbStaff("S2", "S2");
			var staff3 = helper.CreateGlbStaff("S3", "S3");
			var staff4 = helper.CreateGlbStaff("S4", "S4");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff1.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff1.StaffPlainTextPassword);

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine3 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			SetPickedByAndPutawayBy(transferLine2, staff1, staff1);
			SetPickedByAndPutawayBy(transferLine3, staff2, staff2);

			var transferLine4 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine5 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLine4.PickedTime = ZDateTimeOffset.Now;
			transferLine5.PickedTime = ZDateTimeOffset.Now;
			SetPickedByAndPutawayBy(transferLine4, staff1, staff2);
			SetPickedByAndPutawayBy(transferLine5, staff1, staff3);

			webService.Factory.Save();

			var expectedErrorMessage = "Un-finalized transfer could not be found for Reference / Pallet ID : {0}. Possible mismatch on registered equipment, registered area, registered client or transfer / Pallet ID  has been assigned to another operator";

			var response1 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find only lines not allocated to anyone or lines allocated to current user.", 2, response1.Docket.Lines.Count);
			AssertEquals("Transfer line should be allocated to current user if it was not allocated previously.", staff1.GS_Code, transferLine1.GS_NKPickedBy);
			AssertEquals("Transfer line should be allocated to current user if it was not allocated previously.", staff1.GS_Code, transferLine1.WE_GS_NKPutawayBy);
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));

			webService.SecurityHeader.UserName = staff2.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff2.StaffPlainTextPassword);
			var response2 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find only lines not allocated to anyone or lines allocated to current user.", 1, response2.Docket.Lines.Count);
			response2.Docket.Lines.Single(l => l.PK.Equals(transferLine3.PK.ToGuid()));

			webService.SecurityHeader.UserName = staff3.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff3.StaffPlainTextPassword);

			var response3 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find only lines not allocated to anyone or lines allocated to current user, even if it's putaway line and not picked lines for another user exists.", 1, response3.Docket.Lines.Count);
			response3.Docket.Lines.Single(l => l.PK.Equals(transferLine5.PK.ToGuid()));

			// no lines for this user.
			webService.SecurityHeader.UserName = staff4.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff4.StaffPlainTextPassword);
			AssertBusinessValidationError(webService, string.Format(expectedErrorMessage, transfer.WD_DocketID), webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "AREA3", PickMethod = "ANY" }, false, null));
		}

		void SetPickedByAndPutawayBy(WhsTransferLine transferLine, GlbStaff pickedBy, GlbStaff putawayBy)
		{
			transferLine.GS_NKPickedBy = pickedBy.GS_Code;
			transferLine.WE_GS_NKPutawayBy = putawayBy.GS_Code;
		}

		#endregion

		#region TestGetWhsTransfer_IsPicking

		public void TestGetWhsTransfer_IsPicking()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, locationA1);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, locationA1);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 40m, locationA1, locationA2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transfer.RunPreSaveValidation(); // to commit inventory
			SetPickedByAndPutawayBy(transferLine2, staff1, staff1);
			transferLine2.PickedTime = ZDateTimeOffset.Now;

			Helper.Factory.Save();

			AssertEquals("Precondition: transfer line is not Picked", false, transferLine1.IsPicked);
			AssertEquals("Precondition: transfer line has matching lines", true, transferLine1.MatchingLines.Any());
			AssertEquals("Precondition: transfer line is Picked", true, transferLine2.IsPicked);
			AssertEquals("Precondition: transfer line has matching lines", false, transferLine2.MatchingLines.Any());
			AssertEquals("Precondition: transfer line is not Picked", false, transferLine3.IsPicked);
			AssertEquals("Precondition: transfer line has no matching lines", false, transferLine3.MatchingLines.Any());

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff1.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff1.StaffPlainTextPassword);

			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find only lines not allocated to anyone or lines allocated to current user.", 2, response.Docket.Lines.Count);
			AssertNotNull("Transfer line should in response", response.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid())));
			AssertNotNull("Transfer line should in response", response.Docket.Lines.Single(l => l.PK.Equals(transferLine3.PK.ToGuid())));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transferLine1New = newFactory.Load<WhsTransferLine>(transferLine1.PK);
			AssertEquals("Transfer Line has pick line that is picking", true, transferLine1New.PickLines.All(pl => pl.WZ_IsPicking));
			AssertEquals("Matching line in transfer Line has pick line that is picking", true, transferLine1New.MatchingLines.All(ml => ml.PickLines.All(pl => pl.WZ_IsPicking)));

			var transferLine2New = newFactory.Load<WhsTransferLine>(transferLine2.PK);
			AssertEquals("Transfer Line has no pick line that is picking", false, transferLine2New.PickLines.All(pl => pl.WZ_IsPicking));

			var transferLine3New = newFactory.Load<WhsTransferLine>(transferLine3.PK);
			AssertEquals("Transfer Line has pick line that is picking", true, transferLine3New.PickLines.All(pl => pl.WZ_IsPicking));
		}

		#endregion

		#region TestGetWhsTransfer_WithoutReference

		public void TestGetWhsTransfer_WithoutReference()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 3, 1);
			var area1 = helper.CreateArea(data.Whs1, "AREA1");
			var area2 = helper.CreateArea(data.Whs1, "AREA2");
			var staff = helper.CreateGlbStaff("S2", "S2");

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");

			locationA1.WLV_WA_PickingArea = area1.PK;
			locationA1.WLV_PickMethod = "PM1";

			locationA2.WLV_WA_PickingArea = area2.PK;
			locationA2.WLV_PickMethod = "PM2";

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transferWithNotMatchingLines = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transferWithNotMatchingLines.WD_BookingDate = ZDateTimeOffset.Today.AddDays(-10);
			var transferLine1 = helper.CreateWhsTransferLine(transferWithNotMatchingLines, data.Part1, 10m, locationA1, locationA2);
			transferWithNotMatchingLines.RunPreSaveValidation();

			var transferWithMatchingButNotAssignedLines = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			transferWithMatchingButNotAssignedLines.WD_BookingDate = ZDateTimeOffset.Today.AddDays(-7);
			var transferLine2 = helper.CreateWhsTransferLine(transferWithMatchingButNotAssignedLines, data.Part1, 10m, locationA2, locationA3);
			transferWithMatchingButNotAssignedLines.RunPreSaveValidation();

			var transferWithMatchingAndAssignedLines = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", Notify);
			transferWithMatchingAndAssignedLines.WD_BookingDate = ZDateTimeOffset.Today.AddDays(-4);
			var transferLine3 = helper.CreateWhsTransferLine(transferWithMatchingAndAssignedLines, data.Part1, 10m, locationA2, locationA3);
			SetPickedByAndPutawayBy(transferLine3, staff, staff);
			transferWithMatchingAndAssignedLines.RunPreSaveValidation();

			var transferWithMatchingAndAssignedLinesButNewerTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR4", Notify);
			transferWithMatchingAndAssignedLinesButNewerTransfer.WD_BookingDate = ZDateTimeOffset.Today.AddDays(-2);
			var transferLine4 = helper.CreateWhsTransferLine(transferWithMatchingAndAssignedLinesButNewerTransfer, data.Part1, 10m, locationA2, locationA3);
			SetPickedByAndPutawayBy(transferLine4, staff, staff);
			transferWithMatchingAndAssignedLinesButNewerTransfer.RunPreSaveValidation();

			var transferWithNotMatchingAndAssignedLines = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR5", Notify);
			transferWithNotMatchingAndAssignedLines.WD_BookingDate = ZDateTimeOffset.Today;
			var transferLine5 = helper.CreateWhsTransferLine(transferWithNotMatchingAndAssignedLines, data.Part1, 10m, locationA1, locationA2);
			SetPickedByAndPutawayBy(transferLine5, staff, staff);
			transferWithNotMatchingAndAssignedLines.RunPreSaveValidation();

			webService.Factory.Save();

			var expectedErrorMessage = "Un-finalized transfer could not be found. Possible mismatch on registered equipment, registered area, registered client or transfers have been assigned to another operator.";

			var response1 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "AREA2", PickMethod = "PM2" }, false, null);
			AssertEquals("System should find oldest mathcing pre-assigned transfer", transferWithMatchingAndAssignedLines.PK.ToGuid(), response1.Docket.PK);
			AssertEquals(1, response1.Docket.Lines.Count);
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine3.PK.ToGuid()));
			transferWithMatchingAndAssignedLines.FinaliseDocket(); // finalise docket to take it out.
			AssertIsFinalisedPrecondition(transferWithMatchingAndAssignedLines);
			webService.Factory.Save();

			var response2 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "AREA2", PickMethod = "PM2" }, false, null);
			AssertEquals("System should find oldest mathcing pre-assigned transfer", transferWithMatchingAndAssignedLinesButNewerTransfer.PK.ToGuid(), response2.Docket.PK);
			AssertEquals(1, response2.Docket.Lines.Count);
			response2.Docket.Lines.Single(l => l.PK.Equals(transferLine4.PK.ToGuid()));
			transferWithMatchingAndAssignedLinesButNewerTransfer.FinaliseDocket(); // finalise docket to take it out.
			AssertIsFinalisedPrecondition(transferWithMatchingAndAssignedLinesButNewerTransfer);
			webService.Factory.Save();

			var response3 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "AREA2", PickMethod = "PM2" }, false, null);
			AssertEquals("If no matching pre-assigned transfer exist, system should find oldest matching not assigned transfer.", transferWithMatchingButNotAssignedLines.PK.ToGuid(), response3.Docket.PK);
			AssertEquals(1, response3.Docket.Lines.Count);
			response3.Docket.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));
			transferWithMatchingButNotAssignedLines.FinaliseDocket(); // finalise docket to take it out.
			AssertIsFinalisedPrecondition(transferWithMatchingButNotAssignedLines);
			webService.Factory.Save();

			AssertBusinessValidationError(webService, expectedErrorMessage, webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "AREA2", PickMethod = "PM2" }, false, null));

			var response4 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("When no Area or Pick Method specified, then system should find oldest transfer assigned to user.", transferWithNotMatchingAndAssignedLines.PK.ToGuid(), response4.Docket.PK);
			AssertEquals(1, response4.Docket.Lines.Count);
			response4.Docket.Lines.Single(l => l.PK.Equals(transferLine5.PK.ToGuid()));
			transferWithNotMatchingAndAssignedLines.FinaliseDocket(); // finalise docket to take it out.
			AssertIsFinalisedPrecondition(transferWithNotMatchingAndAssignedLines);
			webService.Factory.Save();

			var response5 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("When no Area or Pick Method specified and no assigned transfers exist, then system should find oldest not assigned transfer.", transferWithNotMatchingLines.PK.ToGuid(), response5.Docket.PK);
			AssertEquals(1, response5.Docket.Lines.Count);
			response5.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
			transferWithNotMatchingLines.FinaliseDocket(); // finalise docket to take it out.
			AssertIsFinalisedPrecondition(transferWithNotMatchingLines);
		}

		#endregion

		#region TestGetWhsTransfer_WithoutReference_ClientCode

		public void TestGetWhsTransfer_WithoutReference_ClientCode()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var factory = webService.Factory;
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var org2 = helper.CreateClient("O2");
			var org3 = helper.CreateClient("O3");
			helper.CreateProductClientRelationShip(org2, data.Part1);
			var staff = helper.CreateGlbStaff("S2", "S2");

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "", false);
			helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 10m, sourceLocation, "", false);
			factory.Save();

			var transfer1 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer1.WD_BookingDate = ZDateTimeOffset.Today.AddDays(-10);
			helper.CreateWhsTransferLine(transfer1, data.Part1, 1m, sourceLocation, destinationLocation);
			transfer1.RunPreSaveValidation();

			var transfer2 = helper.CreateWhsTransfer(org2, data.Whs1, "TR2", Notify);
			transfer2.WD_BookingDate = ZDateTimeOffset.Today.AddDays(-7);
			helper.CreateWhsTransferLine(transfer2, data.Part1, 1m, sourceLocation, destinationLocation);
			transfer2.RunPreSaveValidation();

			var transfer3 = helper.CreateWhsTransfer(org2, data.Whs1, "TR3", Notify);
			transfer3.WD_BookingDate = ZDateTimeOffset.Today.AddDays(-1);
			helper.CreateWhsTransferLine(transfer3, data.Part1, 1m, sourceLocation, destinationLocation);
			transfer3.RunPreSaveValidation();

			webService.Factory.Save();

			var expectedErrorMessage = "Un-finalized transfer could not be found. Possible mismatch on registered equipment, registered area, registered client or transfers have been assigned to another operator.";

			var responseForOrg2 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", ClientCode = org2.OH_Code }, false, null);
			AssertEquals("System should find oldest mathching transfer for ORG2.", transfer2.PK.ToGuid(), responseForOrg2.Docket.PK);
			AssertEquals(1, responseForOrg2.Docket.Lines.Count);
			responseForOrg2.Docket.Lines.Single(l => l.PK.Equals(transfer2.Lines.Single().PK.ToGuid()));

			var responseForOrg1 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", ClientCode = data.Org1.OH_Code }, false, null);
			AssertEquals("System should find oldest mathching transfer for ORG1.", transfer1.PK.ToGuid(), responseForOrg1.Docket.PK);
			AssertEquals(1, responseForOrg1.Docket.Lines.Count);
			responseForOrg1.Docket.Lines.Single(l => l.PK.Equals(transfer1.Lines.Single().PK.ToGuid()));

			var responseForNoOrgCode = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", }, false, null);
			AssertEquals("System should find oldest mathching transfer.", transfer1.PK.ToGuid(), responseForNoOrgCode.Docket.PK);
			AssertEquals(1, responseForNoOrgCode.Docket.Lines.Count);
			responseForNoOrgCode.Docket.Lines.Single(l => l.PK.Equals(transfer1.Lines.Single().PK.ToGuid()));

			AssertBusinessValidationError(webService, expectedErrorMessage, webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", ClientCode = org3.OH_Code }, false, null));

			AssertBusinessValidationError(webService, expectedErrorMessage, webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", ClientCode = "InvalidClientCode" }, false, null));
		}

		#endregion

		#region TestGetWhsTransfer_WithoutReference_InterWhsChild

		public void TestGetWhsTransfer_WithoutReference_InterWhsChild_Source()
		{
			TestGetWhsTransfer_WithoutReference_InterWhsChild(isSource: true);
		}

		public void TestGetWhsTransfer_WithoutReference_InterWhsChild_Dest()
		{
			TestGetWhsTransfer_WithoutReference_InterWhsChild(isSource: false);
		}

		public void TestGetWhsTransfer_WithoutReference_InterWhsChild(bool isSource)
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var warehouse2 = helper.CreateWarehouse("WH2", "A", 1, 1);
			helper.CreateWhsReceiveWithInventory(data.Org1, isSource ? data.Whs1 : warehouse2, "R1", data.Part1, 10m);
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", warehouse2.PK, "A");
			transferLine.RunPreSaveValidation();

			var now = ZDateTimeOffset.Now;
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var childTransfer = transfer.ChildTransfers.First();
			AssertNotNull("Precondition: Created child transfer.", childTransfer);
			webService.Factory.Save();

			webService.SecurityHeader.WarehouseCode = warehouse2.WW_WarehouseCode;
			var expectedErrorMessage = "Un-finalized transfer could not be found. Possible mismatch on registered equipment, registered area, registered client or transfers have been assigned to another operator.";
			AssertBusinessValidationError(webService, "Should not find the child job.", expectedErrorMessage, webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null));
		}

		#endregion

		#region TestGetWhsTransfer_ForcedPutaway

		public void TestGetWhsTransfer_ForcedPutaway()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_Created = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine_HeldForTransfer = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			transferLine_HeldForTransfer.PickedTime = ZDateTimeOffset.Now;

			webService.Factory.Save();

			var expectedErrorMessage = "None of the lines on this transfer is ready to be putaway. Please pick something first.";
			var isForcePutaway = true;

			var response1 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, !isForcePutaway, null);
			AssertEquals("System should find transfer by its external reference.", transfer.PK.ToGuid(), response1.Docket.PK);
			AssertEquals("Only lines that need Allocation should be send to device.", 1, response1.Docket.Lines.Count);
			response1.Docket.Lines.Single(l => l.PK.Equals(transferLine_Created.PK.ToGuid()));

			var response2 = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, isForcePutaway, null);
			AssertEquals("System should find transfer by its external reference.", transfer.PK.ToGuid(), response2.Docket.PK);
			AssertEquals("If it is forced putaway, then only lines that need Putaway should be send to device.", 1, response2.Docket.Lines.Count);
			response2.Docket.Lines.Single(l => l.PK.Equals(transferLine_HeldForTransfer.PK.ToGuid()));

			transferLine_HeldForTransfer.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine_HeldForTransfer);
			AssertBusinessValidationError(webService, expectedErrorMessage, webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, isForcePutaway, null));
		}

		#endregion

		#region TestGetWhsTransfer_TransfersNeededForDepletedPickfaces

		[TestDate(2014, 01, 09)]
		public void TestGetWhsTransfer_TransfersNeededForDepletedPickfaces_WithoutAssigningToUser()
		{
			AssertTransfersNeededForDepletedPickfaces();
		}

		[TestDate(2014, 01, 09)]
		public void TestGetWhsTransfer_TransfersNeededForDepletedPickfaces_AssigningToUser()
		{
			AssertTransfersNeededForDepletedPickfaces(true);
		}

		void AssertTransfersNeededForDepletedPickfaces(bool isAssignedToUser = false)
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var part3 = helper.CreateProduct(data.Org1, "P3");
			var part4 = helper.CreateProduct(data.Org1, "P4");
			var part5 = helper.CreateProduct(data.Org1, "P5");

			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceBulkLocation = data.Whs1.FindLocation("A-1");
			var destinationBulkLocation = data.Whs1.FindLocation("A-2");
			var depletedPickFaceLocationWithAvlInvInPick = data.Whs1.FindLocation("A-3");
			var depletedPickFaceLocationWithNoAvlInvInPick = data.Whs1.FindLocation("A-4");
			var pickFaceLocation = data.Whs1.FindLocation("A-5");

			helper.CreateProductPickFace(data.Part1, data.Org1, depletedPickFaceLocationWithAvlInvInPick, 0m, 1m);
			helper.CreateProductPickFace(data.Part2, data.Org1, depletedPickFaceLocationWithNoAvlInvInPick, 0m, 1m);
			helper.CreateProductPickFace(part4, data.Org1, pickFaceLocation, 0m, 2m);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, sourceBulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m, sourceBulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 5m, sourceBulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", part4, 5m, sourceBulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", part5, 5m, sourceBulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part1, 1m, depletedPickFaceLocationWithAvlInvInPick, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", part4, 1m, pickFaceLocation, "");
			webService.Factory.Save();

			// creating picks
			var orderWithAvlInventoryOnPickFace = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			helper.CreateWhsOrderLine(orderWithAvlInventoryOnPickFace, data.Part1, 1m);
			var waitingReplenishmentPickWithAvlInventoryOnPickFace = helper.CreatePickNew(orderWithAvlInventoryOnPickFace);
			waitingReplenishmentPickWithAvlInventoryOnPickFace.WP_IsAwaitingReplenishment = true;
			var orderedInventory = (WhsPickOrderedInventory)waitingReplenishmentPickWithAvlInventoryOnPickFace.OrderedInventories.Single();
			var avaialableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == depletedPickFaceLocationWithAvlInvInPick);
			avaialableInventory.Allocate = true;

			var orderWithoutAvlInventoryOnPickFace = helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", WhsPickOption.Codes.Manual);
			helper.CreateWhsOrderLine(orderWithoutAvlInventoryOnPickFace, data.Part2, 1m);
			var waitingReplenishmentPickWithoutAvlInventoryOnPickFace = helper.CreatePickNew(orderWithoutAvlInventoryOnPickFace);
			waitingReplenishmentPickWithoutAvlInventoryOnPickFace.WP_IsAwaitingReplenishment = true;

			// creating transfers
			var pickedBy = isAssignedToUser ? staff : null;
			var transferForWaitingReplenishmentPickWithAvlInventory = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", ZDateTimeOffset.Today.AddDays(-1), Notify);
			helper.CreateWhsTransferLine(transferForWaitingReplenishmentPickWithAvlInventory, data.Part1, 1m, sourceBulkLocation, depletedPickFaceLocationWithAvlInvInPick, pickedBy);
			helper.CreateWhsTransferLine(transferForWaitingReplenishmentPickWithAvlInventory, data.Part2, 1m, sourceBulkLocation, depletedPickFaceLocationWithNoAvlInvInPick, pickedBy);
			helper.CreateWhsTransferLine(transferForWaitingReplenishmentPickWithAvlInventory, part4, 1m, sourceBulkLocation, pickFaceLocation, pickedBy);
			helper.CreateWhsTransferLine(transferForWaitingReplenishmentPickWithAvlInventory, part5, 1m, sourceBulkLocation, destinationBulkLocation, pickedBy);
			transferForWaitingReplenishmentPickWithAvlInventory.RunPreSaveValidation(); // to generate pick lines

			var transferForWaitingReplenishmentPickWithoutAvlInventory = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", ZDateTimeOffset.Today, Notify);
			helper.CreateWhsTransferLine(transferForWaitingReplenishmentPickWithoutAvlInventory, data.Part2, 1m, sourceBulkLocation, depletedPickFaceLocationWithNoAvlInvInPick, pickedBy);
			helper.CreateWhsTransferLine(transferForWaitingReplenishmentPickWithoutAvlInventory, part4, 1m, sourceBulkLocation, pickFaceLocation, pickedBy);
			helper.CreateWhsTransferLine(transferForWaitingReplenishmentPickWithoutAvlInventory, part5, 1m, sourceBulkLocation, destinationBulkLocation, pickedBy);
			transferForWaitingReplenishmentPickWithoutAvlInventory.RunPreSaveValidation(); // to generate pick lines

			var transferForPickFace = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", ZDateTimeOffset.Today.AddDays(-1), Notify);
			helper.CreateWhsTransferLine(transferForPickFace, part4, 1m, sourceBulkLocation, pickFaceLocation, pickedBy);
			helper.CreateWhsTransferLine(transferForPickFace, part5, 1m, sourceBulkLocation, destinationBulkLocation, pickedBy);
			transferForPickFace.RunPreSaveValidation(); // to generate pick lines

			var transferForBulkLocation = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR4", ZDateTimeOffset.Today.AddDays(-2), Notify);
			helper.CreateWhsTransferLine(transferForBulkLocation, part5, 1m, sourceBulkLocation, destinationBulkLocation, pickedBy);
			transferForBulkLocation.RunPreSaveValidation(); // to generate pick lines

			webService.Factory.Save();

			var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find transfer with waiting replenishment pick and also older booking date.", transferForWaitingReplenishmentPickWithAvlInventory.PK.ToGuid(), response.Docket.PK);
			AssertEquals(4, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.DestLocation == depletedPickFaceLocationWithAvlInvInPick.ToLocationString());
			response.Docket.Lines.Single(l => l.DestLocation == depletedPickFaceLocationWithNoAvlInvInPick.ToLocationString());
			response.Docket.Lines.Single(l => l.DestLocation == pickFaceLocation.ToLocationString());
			response.Docket.Lines.Single(l => l.DestLocation == destinationBulkLocation.ToLocationString());
			transferForWaitingReplenishmentPickWithAvlInventory.Delete();
			webService.Factory.Save();

			response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find transfer with waiting replenishment pick with newer booking date.", transferForWaitingReplenishmentPickWithoutAvlInventory.PK.ToGuid(), response.Docket.PK);
			AssertEquals(3, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.DestLocation == depletedPickFaceLocationWithNoAvlInvInPick.ToLocationString());
			response.Docket.Lines.Single(l => l.DestLocation == pickFaceLocation.ToLocationString());
			response.Docket.Lines.Single(l => l.DestLocation == destinationBulkLocation.ToLocationString());
			transferForWaitingReplenishmentPickWithoutAvlInventory.Delete();
			webService.Factory.Save();

			response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find oldest matching transfer for the pickface", transferForPickFace.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.DestLocation == pickFaceLocation.ToLocationString());
			response.Docket.Lines.Single(l => l.DestLocation == destinationBulkLocation.ToLocationString());
			transferForPickFace.Delete();
			webService.Factory.Save();

			response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for a bulk location should be given lower priority.", transferForBulkLocation.PK.ToGuid(), response.Docket.PK);
			AssertEquals(1, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.DestLocation == destinationBulkLocation.ToLocationString());
		}

		#endregion

		#region TestGetWhsTransfer_TransferPriority_PickfacesAndPickFacesHasWaitingReplenishmentPicks

		[TestDate(2014, 1, 1)]
		public void TestGetWhsTransfer_TransferPriority_PickfacesAndPickFacesHasWaitingReplenishmentPicks()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			var part3 = helper.CreateProduct(data.Org1, "P3");

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var depletedPickfaceLocationForWaitingReplenishment = data.Whs1.FindLocation("A-2");
			var pickfaceLocationForWaitingReplenishment = data.Whs1.FindLocation("A-3");
			var depletedPickface = data.Whs1.FindLocation("A-4");
			var pickfaceLocation2 = data.Whs1.FindLocation("A-5");
			var destinationBulkLocation = data.Whs1.FindLocation("A-6");

			helper.CreateProductPickFace(data.Part1, data.Org1, depletedPickfaceLocationForWaitingReplenishment, 2m, 5m);
			helper.CreateProductPickFace(part3, data.Org1, pickfaceLocationForWaitingReplenishment, 1m, 5m);
			helper.CreateProductPickFace(data.Part2, data.Org1, depletedPickface, 2m, 5m);
			helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation2, 1m, 5m);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 1m, depletedPickfaceLocationForWaitingReplenishment, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", part3, 3m, pickfaceLocationForWaitingReplenishment, "");
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part2, 1m, depletedPickface, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", data.Part2, 3m, pickfaceLocation2, "");

			var reservedOrder = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var reservedOrderLine = helper.CreateWhsOrderLine(reservedOrder, data.Part2, 2m);
			var reservedPickLine = reservedOrderLine.ReserveStockIfAbleTo(receive.Inventory[0]);
			((IBusinessObjectInternals)reservedPickLine).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = 1m;

			// create waiting replenishment pick
			var orderForWaitingReplenishmentPick = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			helper.CreateWhsOrderLine(orderForWaitingReplenishmentPick, data.Part1, 1m);
			helper.CreateWhsOrderLine(orderForWaitingReplenishmentPick, part3, 5m);
			var waitingReplenishmentPick = helper.CreatePickNew(orderForWaitingReplenishmentPick);
			waitingReplenishmentPick.WP_IsAwaitingReplenishment = true;

			// creating transfers
			var transferForDepletedPickFaceWaitingReplenishment = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", ZDateTimeOffset.Today.AddDays(-1), Notify);
			CreateTransferLineWithPickLines(helper, transferForDepletedPickFaceWaitingReplenishment, data.Part1, bulkLocation, depletedPickfaceLocationForWaitingReplenishment, 1m);

			var transferForPickFaceWaitingReplenishment = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", ZDateTimeOffset.Today.AddDays(-2), Notify);
			CreateTransferLineWithPickLines(helper, transferForPickFaceWaitingReplenishment, part3, bulkLocation, pickfaceLocationForWaitingReplenishment, 1m);

			var transferForDepletedPickFaceLocation = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", ZDateTimeOffset.Today.AddDays(-3), Notify);
			CreateTransferLineWithPickLines(helper, transferForDepletedPickFaceLocation, data.Part2, bulkLocation, depletedPickface, 1m);

			var transferForPickFaceLocation = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR4", ZDateTimeOffset.Today.AddDays(-4), Notify);
			CreateTransferLineWithPickLines(helper, transferForPickFaceLocation, data.Part2, bulkLocation, pickfaceLocation2, 1m);

			var transferForBulkLocation = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR5", ZDateTimeOffset.Today.AddDays(-5), Notify);
			CreateTransferLineWithPickLines(helper, transferForBulkLocation, data.Part1, bulkLocation, destinationBulkLocation, 1m);
			webService.Factory.Save();

			// test Transfer priority
			var response1 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer with depleted pick face and waiting replenishment should get higher priority.", transferForDepletedPickFaceWaitingReplenishment.PK, response1.Docket.PK);
			AssertEquals(depletedPickfaceLocationForWaitingReplenishment.ToLocationString(), response1.Docket.Lines.Single().DestLocation);
			transferForDepletedPickFaceWaitingReplenishment.Delete();
			webService.Factory.Save();

			var response2 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer with pick face and waiting replenishment should get next priority.", transferForPickFaceWaitingReplenishment.PK, response2.Docket.PK);
			AssertEquals(pickfaceLocationForWaitingReplenishment.ToLocationString(), response2.Docket.Lines.Single().DestLocation);
			transferForPickFaceWaitingReplenishment.Delete();
			webService.Factory.Save();

			var response3 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Reserved stock makes available qty less than minimum. Transfer for depleted pick face should get the next priority.", transferForDepletedPickFaceLocation.PK, response3.Docket.PK);
			AssertEquals(depletedPickface.ToLocationString(), response3.Docket.Lines.Single().DestLocation);
			transferForDepletedPickFaceLocation.Delete();
			webService.Factory.Save();

			var response4 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for pick face should get the next priority.", transferForPickFaceLocation.PK, response4.Docket.PK);
			AssertEquals(pickfaceLocation2.ToLocationString(), response4.Docket.Lines.Single().DestLocation);
			transferForPickFaceLocation.Delete();
			webService.Factory.Save();

			var response5 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for a bulk location should get the lowest prority.", transferForBulkLocation.PK, response5.Docket.PK);
			AssertEquals(destinationBulkLocation.ToLocationString(), response5.Docket.Lines.Single().DestLocation);
		}

		void CreateTransferLineWithPickLines(WhsTestHelperFunctions helper, WhsTransfer transfer, OrgSupplierPart part, WhsLocation bulkLocation, WhsLocation pickfaceLocation, decimal quantity)
		{
			helper.CreateWhsTransferLine(transfer, part, quantity, bulkLocation, pickfaceLocation);
			transfer.RunPreSaveValidation(); // to generate pick lines
		}

		#endregion

		#region TestGetWhsTransfer_TransferPriority_TranferForDepletedPickFaceAndPickFaceHasAdjustment

		[TestDate(2016, 12, 05)]
		public void TestGetWhsTransfer_TransferPriority_TranferForDepletedPickFaceAndPickFaceHasAdjustment()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 4, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var depletedPickfaceLocation = data.Whs1.FindLocation("A-2");
			var pickfaceHasAdjustment = data.Whs1.FindLocation("A-3");
			var pickfaceLocation = data.Whs1.FindLocation("A-4");

			helper.CreateProductPickFace(data.Part1, data.Org1, depletedPickfaceLocation, 2m, 5m);
			helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceHasAdjustment, 1m, 5m);
			helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 1m, 5m);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 1m, depletedPickfaceLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 2m, pickfaceHasAdjustment, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part2, 3m, pickfaceLocation, "");

			var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine1 = helper.CreateWhsAdjustmentLine(adjustment, data.Part2, -2m, pickfaceHasAdjustment);
			adjustment.RunPreSaveValidation();
			AssertEquals("Precondition - Committed Quantity is correct.", 2m, adjustmentLine1.CommittedQuantity);

			// creating transfers
			var transferForDepletedPickFace = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", ZDateTimeOffset.Today.AddDays(-1), Notify);
			CreateTransferLineWithPickLines(helper, transferForDepletedPickFace, data.Part1, bulkLocation, depletedPickfaceLocation, 1m);

			var transferForPickFaceHasAdjustment = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", ZDateTimeOffset.Today.AddDays(-2), Notify);
			CreateTransferLineWithPickLines(helper, transferForPickFaceHasAdjustment, data.Part2, bulkLocation, pickfaceHasAdjustment, 1m);

			var transferForPickFaceLocation = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", ZDateTimeOffset.Today.AddDays(-3), Notify);
			CreateTransferLineWithPickLines(helper, transferForPickFaceLocation, data.Part2, bulkLocation, pickfaceLocation, 1m);
			webService.Factory.Save();

			// test Transfer priority
			var response1 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer with depleted pick face and waiting replenishment should get higher priority.", transferForDepletedPickFace.PK, response1.Docket.PK);
			AssertEquals(depletedPickfaceLocation.ToLocationString(), response1.Docket.Lines.Single().DestLocation);
			transferForDepletedPickFace.Delete();
			webService.Factory.Save();

			var response2 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer with pick face should get the next priority.", transferForPickFaceLocation.PK, response2.Docket.PK);
			AssertEquals(pickfaceLocation.ToLocationString(), response2.Docket.Lines.Single().DestLocation);
			transferForPickFaceLocation.Delete();
			webService.Factory.Save();

			var response3 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for pick face has adjustment should get the next priority.", transferForPickFaceHasAdjustment.PK, response3.Docket.PK);
			AssertEquals(pickfaceHasAdjustment.ToLocationString(), response3.Docket.Lines.Single().DestLocation);
		}

		#endregion

		#region TestGetWhsTransfer_TransferPriority_WaitingReplenishmentPickHasProductsForDepletedPickFacePickFaceAndBulkLocation

		public void TestGetWhsTransfer_TransferPriority_WaitingReplenishmentPickHasProductsForDepletedPickFacePickFaceAndBulkLocation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 4, 1);
			var part3 = helper.CreateProduct(data.Org1, "P3");

			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var depletedPickfaceLocation = data.Whs1.FindLocation("A-2");
			var pickfaceLocation = data.Whs1.FindLocation("A-3");
			var destinationBulkLocation = data.Whs1.FindLocation("A-4");

			helper.CreateProductPickFace(data.Part1, data.Org1, depletedPickfaceLocation, 2m, 5m);
			helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 1m, 5m);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 1m, depletedPickfaceLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part2, 3m, pickfaceLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", part3, 1m, destinationBulkLocation, "");
			webService.Factory.Save();

			// create waiting replenishment pick
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			helper.CreateWhsOrderLine(order, data.Part1, 2m);
			helper.CreateWhsOrderLine(order, data.Part2, 1m);
			helper.CreateWhsOrderLine(order, part3, 1m);
			var waitingReplenishmentPick = helper.CreatePickNew(order);
			waitingReplenishmentPick.WP_IsAwaitingReplenishment = true;
			AllocateInventory(waitingReplenishmentPick, data.Part1, depletedPickfaceLocation);
			AllocateInventory(waitingReplenishmentPick, data.Part2, pickfaceLocation);
			AllocateInventory(waitingReplenishmentPick, part3, destinationBulkLocation);

			// creating transfers
			var transferForPickFaceWaitingReplenishment = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", ZDateTimeOffset.Today.AddDays(-1), Notify);
			CreateTransferLineWithPickLines(helper, transferForPickFaceWaitingReplenishment, data.Part1, bulkLocation, depletedPickfaceLocation, 1m);

			var transferForDepletedPickFaceLocation = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", ZDateTimeOffset.Today.AddDays(-2), Notify);
			CreateTransferLineWithPickLines(helper, transferForDepletedPickFaceLocation, data.Part2, bulkLocation, pickfaceLocation, 1m);

			var transferForBulkLocation = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", ZDateTimeOffset.Today.AddDays(-3), Notify);
			CreateTransferLineWithPickLines(helper, transferForBulkLocation, part3, bulkLocation, destinationBulkLocation, 1m);
			webService.Factory.Save();

			// test Transfer priority
			var response1 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for the depleted pick face should get the higher priority.", transferForPickFaceWaitingReplenishment.PK.ToGuid(), response1.Docket.PK);
			AssertEquals(depletedPickfaceLocation.ToLocationString(), response1.Docket.Lines.Single().DestLocation);
			transferForPickFaceWaitingReplenishment.Delete();
			webService.Factory.Save();

			var response2 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for the pick face should get the second priority.", transferForDepletedPickFaceLocation.PK.ToGuid(), response2.Docket.PK);
			AssertEquals(pickfaceLocation.ToLocationString(), response2.Docket.Lines.Single().DestLocation);
			transferForDepletedPickFaceLocation.Delete();
			webService.Factory.Save();

			var response3 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for the bulk location should get the lowest priority.", transferForBulkLocation.PK.ToGuid(), response3.Docket.PK);
			AssertEquals(destinationBulkLocation.ToLocationString(), response3.Docket.Lines.Single().DestLocation);
		}

		static void AllocateInventory(WhsPick waitingReplenishmentPick, OrgSupplierPart part, WhsLocation depletedPickfaceLocation)
		{
			var orderedInventory = waitingReplenishmentPick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == part);
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Location == depletedPickfaceLocation);
			availableInventory.Allocate = true;
		}

		#endregion

		#region TestGetWhsTransfer_TransferPriority_TransferForDepletedPickFaceAndPickFace

		public void TestGetWhsTransfer_TransferPriority_TransferForDepletedPickFaceAndPickFace()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 3, 1);

			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var depletedPickfaceLocation = data.Whs1.FindLocation("A-2");
			var pickfaceLocation = data.Whs1.FindLocation("A-3");

			helper.CreateProductPickFace(data.Part1, data.Org1, depletedPickfaceLocation, 2m, 5m);
			helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 1m, 5m);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, depletedPickfaceLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 2m, pickfaceLocation, "");

			// creating transfers
			var transferForDepletedPickFace = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", ZDateTimeOffset.Today.AddDays(-1), Notify);
			CreateTransferLineWithPickLines(helper, transferForDepletedPickFace, data.Part1, bulkLocation, depletedPickfaceLocation, 2m);

			var transferForPickFaceLocation = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", ZDateTimeOffset.Today.AddDays(-2), Notify);
			CreateTransferLineWithPickLines(helper, transferForPickFaceLocation, data.Part1, bulkLocation, pickfaceLocation, 1m);
			webService.Factory.Save();

			// test Transfer priority
			var response1 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for the depleted pick face should get the higher priority.", transferForDepletedPickFace.PK.ToGuid(), response1.Docket.PK);
			AssertEquals(depletedPickfaceLocation.ToLocationString(), response1.Docket.Lines.Single().DestLocation);
			transferForDepletedPickFace.Delete();
			webService.Factory.Save();

			var response2 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for the non-depleted pick face should get the lowest priority.", transferForPickFaceLocation.PK.ToGuid(), response2.Docket.PK);
			AssertEquals(pickfaceLocation.ToLocationString(), response2.Docket.Lines.Single().DestLocation);
		}

		#endregion

		#region TestGetWhsTransfer_TransferPriority_TransferForWaitingOnReplenishmentPickHasDepletedPickfaceAndBulkLocation

		public void TestGetWhsTransfer_TransferPriority_TransferForWaitingOnReplenishmentPickHasDepletedPickfaceAndBulkLocation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 3, 1);

			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var depletedPickfaceLocation = data.Whs1.FindLocation("A-2");
			var destinationBulkLocation = data.Whs1.FindLocation("A-3");

			helper.CreateProductPickFace(data.Part1, data.Org1, depletedPickfaceLocation, 2m, 5m);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m, bulkLocation, "");

			// creating waiting on replenishment pick
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			helper.CreateWhsOrderLine(order, data.Part1, 1m);
			helper.CreateWhsOrderLine(order, data.Part2, 1m);
			var waitingReplenishmentPick = helper.CreatePickNew(order);
			waitingReplenishmentPick.WP_IsAwaitingReplenishment = true;

			// creating transfers
			var transferForDepletedPickFace = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", ZDateTimeOffset.Today.AddDays(-1), Notify);
			CreateTransferLineWithPickLines(helper, transferForDepletedPickFace, data.Part1, bulkLocation, depletedPickfaceLocation, 3m);

			var transferForBulkLocation = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", ZDateTimeOffset.Today.AddDays(-2), Notify);
			CreateTransferLineWithPickLines(helper, transferForBulkLocation, data.Part2, bulkLocation, destinationBulkLocation, 1m);
			webService.Factory.Save();

			// test Transfer priority
			var response1 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for the depleted pick face should get the higher priority.", transferForDepletedPickFace.PK.ToGuid(), response1.Docket.PK);
			AssertEquals(depletedPickfaceLocation.ToLocationString(), response1.Docket.Lines.Single().DestLocation);
			transferForDepletedPickFace.Delete();
			webService.Factory.Save();

			var response2 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for the bulk location should get the lowest priority.", transferForBulkLocation.PK.ToGuid(), response2.Docket.PK);
			AssertEquals(destinationBulkLocation.ToLocationString(), response2.Docket.Lines.Single().DestLocation);
		}

		#endregion

		#region TestGetWhsTransfer_TransferPriority_TransferForWaitingOnReplenishmentPickHasDepletedPickfaceAndPickfaceLocation

		[TestDate(2019, 6, 10)]
		public void TestGetWhsTransfer_TransferPriority_TransferForWaitingOnReplenishmentPickHasDepletedPickfaceAndPickfaceLocation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 3, 1);

			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var depletedPickfaceLocation = data.Whs1.FindLocation("A-2");
			var pickfaceLocation = data.Whs1.FindLocation("A-3");

			helper.CreateProductPickFace(data.Part1, data.Org1, depletedPickfaceLocation, 2m, 5m);
			helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 2m, 5m);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 1m, pickfaceLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 4m, pickfaceLocation, "");
			webService.Factory.Save();

			// creating waiting on replenishment pick
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			helper.CreateWhsOrderLine(order, data.Part1, 1m);
			helper.CreateWhsOrderLine(order, data.Part2, 1m);
			var waitingReplenishmentPick = helper.CreatePickNew(order);
			waitingReplenishmentPick.WP_IsAwaitingReplenishment = true;
			AllocateInventory(waitingReplenishmentPick, data.Part2, pickfaceLocation);

			// creating transfers
			var transferForDepletedPickFace = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", ZDateTimeOffset.Today.AddDays(-1), Notify);
			CreateTransferLineWithPickLines(helper, transferForDepletedPickFace, data.Part1, bulkLocation, depletedPickfaceLocation, 3m);

			var transferForNonDepletedPickFace = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", ZDateTimeOffset.Today.AddDays(-2), Notify);
			CreateTransferLineWithPickLines(helper, transferForNonDepletedPickFace, data.Part2, bulkLocation, pickfaceLocation, 1m);
			webService.Factory.Save();

			// test Transfer priority
			var response1 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for the depleted pick face should get the higher priority.", transferForDepletedPickFace.PK.ToGuid(), response1.Docket.PK);
			AssertEquals(depletedPickfaceLocation.ToLocationString(), response1.Docket.Lines.Single().DestLocation);
			transferForDepletedPickFace.Delete();
			webService.Factory.Save();

			var response2 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for the pickface location which is not depleted should get the lowest priority.", transferForNonDepletedPickFace.PK.ToGuid(), response2.Docket.PK);
			AssertEquals(pickfaceLocation.ToLocationString(), response2.Docket.Lines.Single().DestLocation);
		}

		#endregion

		#region TestGetWhsTransfer_TransferPriority_TransferForWaitingOnReplenishmentPickHasDepletedPickfaceAndPickfaceLocation_WithoutAllocatedPicklines

		public void TestGetWhsTransfer_TransferPriority_TransferForWaitingOnReplenishmentPickHasDepletedPickfaceAndPickfaceLocation_WithoutAllocatedPicklines()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 3, 1);

			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var depletedPickfaceLocation = data.Whs1.FindLocation("A-2");
			var pickfaceLocation = data.Whs1.FindLocation("A-3");

			helper.CreateProductPickFace(data.Part1, data.Org1, depletedPickfaceLocation, 2m, 5m);
			helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 2m, 5m);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 1m, pickfaceLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 4m, pickfaceLocation, "");

			// creating waiting on replenishment pick
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			helper.CreateWhsOrderLine(order, data.Part1, 1m);
			helper.CreateWhsOrderLine(order, data.Part2, 1m);
			var waitingReplenishmentPick = helper.CreatePickNew(order);
			waitingReplenishmentPick.WP_IsAwaitingReplenishment = true;

			// creating transfers
			var transferForDepletedPickFace = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", ZDateTimeOffset.Today.AddDays(-1), Notify);
			CreateTransferLineWithPickLines(helper, transferForDepletedPickFace, data.Part1, bulkLocation, depletedPickfaceLocation, 3m);

			var transferForNonDepletedPickFace = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", ZDateTimeOffset.Today.AddDays(-2), Notify);
			CreateTransferLineWithPickLines(helper, transferForNonDepletedPickFace, data.Part2, bulkLocation, pickfaceLocation, 1m);
			webService.Factory.Save();

			// test Transfer priority
			var response1 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for the depleted pick face should get the higher priority.", transferForDepletedPickFace.PK.ToGuid(), response1.Docket.PK);
			AssertEquals(depletedPickfaceLocation.ToLocationString(), response1.Docket.Lines.Single().DestLocation);
			transferForDepletedPickFace.Delete();
			webService.Factory.Save();

			var response2 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for the pickface location which is not depleted should get the lowest priority.", transferForNonDepletedPickFace.PK.ToGuid(), response2.Docket.PK);
			AssertEquals(pickfaceLocation.ToLocationString(), response2.Docket.Lines.Single().DestLocation);
		}

		#endregion

		#region TestGetWhsTransfer_TransferPriority_TransferForWaitingOnReplenishmentPickHasDepletedPickFaceAndStockAllocatedPickFace

		[TestDate(2016, 12, 05)]
		public void TestGetWhsTransfer_TransferPriority_TransferForWaitingOnReplenishmentPickHasDepletedPickFaceAndStockAllocatedPickFace()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 4, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var bulkLocation = data.Whs1.FindLocation("A-1");
			var depletedPickfaceLocation = data.Whs1.FindLocation("A-2");
			var pickfaceHadAllocated = data.Whs1.FindLocation("A-3");
			var pickfaceLocation = data.Whs1.FindLocation("A-4");

			helper.CreateProductPickFace(data.Part1, data.Org1, depletedPickfaceLocation, 2m, 5m);
			helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceHadAllocated, 1m, 5m);
			helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 1m, 5m);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m, bulkLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 1m, depletedPickfaceLocation, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 2m, pickfaceHadAllocated, "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part2, 3m, pickfaceLocation, "");
			webService.Factory.Save();

			// create waiting replenishment pick
			var orderForWaitingReplenishmentPick = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			helper.CreateWhsOrderLine(orderForWaitingReplenishmentPick, data.Part1, 1m);
			helper.CreateWhsOrderLine(orderForWaitingReplenishmentPick, data.Part2, 3m);
			var waitingReplenishmentPick = helper.CreatePickNew(orderForWaitingReplenishmentPick);
			waitingReplenishmentPick.WP_IsAwaitingReplenishment = true;
			AllocateInventory(waitingReplenishmentPick, data.Part2, pickfaceHadAllocated);

			// creating transfers
			var transferForDepletedPickFace = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", ZDateTimeOffset.Today.AddDays(-1), Notify);
			CreateTransferLineWithPickLines(helper, transferForDepletedPickFace, data.Part1, bulkLocation, depletedPickfaceLocation, 1m);

			var transferForPickFaceHadAllocated = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", ZDateTimeOffset.Today.AddDays(-2), Notify);
			CreateTransferLineWithPickLines(helper, transferForPickFaceHadAllocated, data.Part2, bulkLocation, pickfaceHadAllocated, 1m);

			var transferForPickFaceLocation = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", ZDateTimeOffset.Today.AddDays(-3), Notify);
			CreateTransferLineWithPickLines(helper, transferForPickFaceLocation, data.Part2, bulkLocation, pickfaceLocation, 1m);
			webService.Factory.Save();

			// test Transfer priority
			var response1 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer with depleted pick face and waiting replenishment should get higher priority.", transferForDepletedPickFace.PK, response1.Docket.PK);
			AssertEquals(depletedPickfaceLocation.ToLocationString(), response1.Docket.Lines.Single().DestLocation);
			transferForDepletedPickFace.Delete();
			webService.Factory.Save();

			var response2 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer with pick face should get the next priority.", transferForPickFaceLocation.PK, response2.Docket.PK);
			AssertEquals(pickfaceLocation.ToLocationString(), response2.Docket.Lines.Single().DestLocation);
			transferForPickFaceLocation.Delete();
			webService.Factory.Save();

			var response3 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("Transfer for pick face(stock on location) has allocated should get the next priority.", transferForPickFaceHadAllocated.PK, response3.Docket.PK);
			AssertEquals(pickfaceHadAllocated.ToLocationString(), response3.Docket.Lines.Single().DestLocation);
		}

		#endregion

		#region TestGetWhsTransfer_TransferPriority_PrioritisesAutomatedTransfers

		public void TestGetWhsTransfer_TransferPriority_PrioritisesAutomatedTransfers()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 3, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var automatedPickFaceLocation = data.Whs1.FindLocation("A-2");
			var manualPickFaceLocation = data.Whs1.FindLocation("A-3");

			helper.CreateProductPickFace(data.Part1, data.Org1, automatedPickFaceLocation, 1m, 5m);
			helper.CreateProductPickFace(data.Part1, data.Org1, manualPickFaceLocation, 1m, 5m);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, sourceLocation, "");

			var automaticStockTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", ZDateTimeOffset.Today.AddDays(-1), Notify);
			CreateTransferLineWithPickLines(helper, automaticStockTransfer, data.Part1, sourceLocation, automatedPickFaceLocation, 1m);
			automaticStockTransfer.WD_IsPickFaceReplenishment = true;

			var manualStockTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", ZDateTimeOffset.Today.AddDays(-1), Notify);
			CreateTransferLineWithPickLines(helper, manualStockTransfer, data.Part1, sourceLocation, manualPickFaceLocation, 1m);

			webService.Factory.Save();

			var response1 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(automaticStockTransfer.PK, response1.Docket.PK);
			AssertEquals(automatedPickFaceLocation.ToLocationString(), response1.Docket.Lines.Single().DestLocation);
			automaticStockTransfer.Delete();
			webService.Factory.Save();

			var response2 = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(manualStockTransfer.PK, response2.Docket.PK);
			AssertEquals(manualPickFaceLocation.ToLocationString(), response2.Docket.Lines.Single().DestLocation);
		}

		#endregion

		#region TestGetWhsTransfer_PalletsToTransferCompletely

		public void TestGetWhsTransfer_PalletsToTransferCompletely()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var differentWarehouse = helper.CreateWarehouse("W2");
			var location = helper.CreateRowAndGenerateLocations(differentWarehouse, "A", 1, 2);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			helper.CreateWhsReceiveWithInventory(data.Org1, differentWarehouse, "R", data.Part1, 5m, null, "PLT2", false, false); // Same pallet in different warehouse
			helper.CreateWhsReceiveWithInventory(data.Org1, differentWarehouse, "R0", data.Part1, 5m, differentWarehouse.DefaultLocation, "PLT2"); // Same pallet in different warehouse
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, sourceLocation, "PLT1"); // transferring fully 1 product
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m, sourceLocation, "PLT1");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 2m, sourceLocation, "PLT2"); // transferring fully the palllet
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 2m, sourceLocation, "PLT3"); // transferring partially the pallet
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 1m, sourceLocation, "PLT4"); // not transferred
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part1, 1m, sourceLocation, "PLT5"); // transferring fully both products
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", data.Part2, 1m, sourceLocation, "PLT5");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R8", data.Part1, 4m, sourceLocation, "PLT6"); // transferring fully in multiple transfer lines
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R9", data.Part1, 5m, sourceLocation, "PLT7"); // transferring partially in multiple transfer lines
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R10", data.Part1, 4m, sourceLocation, "PLT8"); // Part of the pallet is fully transferred
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R11", data.Part2, 4m, sourceLocation, ""); // transferring fully without PalletID

			// creating transfers
			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation.ToLocationString(), "PLT1", destinationLocation.ToLocationString(), ""); // PLT1 has par2 on it
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocation.ToLocationString(), "PLT2", destinationLocation.ToLocationString(), ""); // PLT2 has only part1
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), "PLT3", destinationLocation.ToLocationString(), ""); // Part of PLT3 part1 is transferred
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), "PLT5", destinationLocation.ToLocationString(), ""); // PLT5 has part1 and part2 both transferred
			helper.CreateWhsTransferLine(transfer, data.Part2, 1m, sourceLocation.ToLocationString(), "PLT5", destinationLocation.ToLocationString(), "");
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), "PLT6", destinationLocation.ToLocationString(), ""); //PLT6 fully transferred in different transfer lines
			helper.CreateWhsTransferLine(transfer, data.Part1, 3m, sourceLocation.ToLocationString(), "PLT6", destinationLocation.ToLocationString(), "");
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocation.ToLocationString(), "PLT7", destinationLocation.ToLocationString(), ""); // part of PLT7 is transferred
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), "PLT7", destinationLocation.ToLocationString(), "");
			var partOfPalletTransferToFinalise = helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocation.ToLocationString(), "PLT8", destinationLocation.ToLocationString(), "");
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocation.ToLocationString(), "PLT8", destinationLocation.ToLocationString(), "");
			helper.CreateWhsTransferLine(transfer, data.Part2, 4m, sourceLocation.ToLocationString(), "", destinationLocation.ToLocationString(), "");

			transfer.RunPreSaveValidation(); // generate picklines
			webService.Factory.Save();
			partOfPalletTransferToFinalise.FinaliseDocketLine();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT2", "PLT5", "PLT6", "PLT8" }, response.Docket.PalletsToTransferCompletely);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
		}

		#endregion

		#region TestGetWhsTransfer_SelectTransferWithPalletId

		public void TestGetWhsTransfer_SelectTransferWithPalletId()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var user = helper.CreateGlbStaff("S1", "S1");
			var differentUser = helper.CreateGlbStaff("S2", "S2");
			var differentWarehouse = helper.CreateWarehouse("W2", "A", 2, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = user.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(user.StaffPlainTextPassword);

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "PLT1");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, sourceLocation, "PLT2");
			helper.CreateWhsReceiveWithInventory(data.Org1, differentWarehouse, "R3", data.Part1, 10m, differentWarehouse.FindLocation("A-1"), "PLT3");

			// creating transfers
			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation.ToLocationString(), "PLT1", destinationLocation.ToLocationString(), "");
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation.ToLocationString(), "PLT2", destinationLocation.ToLocationString(), "");
			transfer.RunPreSaveValidation(); // generate picklines
			SetPickedByAndPutawayBy(transferLine2, differentUser, differentUser);
			webService.Factory.Save();

			var transferInDifferentWarehouse = helper.CreateWhsTransfer(data.Org1, differentWarehouse, "TR2", Notify);
			helper.CreateWhsTransferLine(transferInDifferentWarehouse, data.Part1, 5m, "A-1", "PLT3", "A-2", "");
			transferInDifferentWarehouse.RunPreSaveValidation(); // generate picklines
			webService.Factory.Save();

			var responseForExistingPalletId = webService.GetWhsTransfer("PLT1", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.WD_DocketID, responseForExistingPalletId.Docket.DocketID);

			AssertBusinessValidationError(webService, "PLT2 is assigned to a different user.", string.Format("Can't find un finalized Transfer with Reference: PLT2.", transfer.WD_DocketID),
				webService.GetWhsTransfer("PLT2", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null));

			AssertBusinessValidationError(webService, "PLT3 is in different warehouse.", string.Format("Can't find un finalized Transfer with Reference: PLT3.", transfer.WD_DocketID),
				webService.GetWhsTransfer("PLT3", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null));

			transferLine1.FinaliseDocketLine();
			webService.Factory.Save();
			AssertIsFinalisedPrecondition(transferLine1);
			Assert("Precondition", !transferLine2.IsFinalised);
			AssertBusinessValidationError(webService, "PLT1 is in a transfer line which is already finalized.", string.Format("Can't find un finalized Transfer with Reference: PLT1.", transfer.WD_DocketID),
				webService.GetWhsTransfer("PLT1", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null));
		}

		#endregion

		#region TestGetWhsTransfer_SelectTransferWithPalletId_InterWhs

		public void TestGetWhsTransfer_SelectTransferWithPalletId_InterWhs_Source()
		{
			TestGetWhsTransfer_SelectTransferWithPalletId_InterWhs(isSource: true);
		}

		public void TestGetWhsTransfer_SelectTransferWithPalletId_InterWhs_Dest()
		{
			TestGetWhsTransfer_SelectTransferWithPalletId_InterWhs(isSource: false);
		}

		[TestDate(2017, 07, 14)]
		public void TestGetWhsTransfer_SelectTransferWithPalletId_InterWhs(bool isSource)
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var staff = helper.CreateGlbStaff("S2", "S2");
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var warehouse2 = helper.CreateWarehouse("WH2", "A", 1, 1);
			helper.CreateWhsReceiveWithInventory(data.Org1, isSource ? data.Whs1 : warehouse2, "R1", data.Part1, 10m, null, "PLT");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1.PK, 5m, "A", "PLT", warehouse2.PK, "A", "PLT", ZDateTimeOffset.Today);
			transferLine.RunPreSaveValidation();

			var now = ZDateTimeOffset.Now;
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var childTransfer = transfer.ChildTransfers.First();
			AssertNotNull("Precondition: Created child transfer.", childTransfer);
			webService.Factory.Save();

			webService.SecurityHeader.WarehouseCode = warehouse2.WW_WarehouseCode;
			AssertBusinessValidationError(webService, $"Cannot transfer an Inter-Warehouse Transfer using the child job.",
				webService.GetWhsTransfer(childTransfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null));

			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals("System should find transfer by its external reference.", transfer.PK.ToGuid(), response.Docket.PK);
		}

		#endregion

		#region TestGetWhsTransfer_ShowStockOnHandWarningOnPutaway

		public void TestGetWhsTransfer_ShowStockOnHandWarningOnPutaway()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var user = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = user.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(user.StaffPlainTextPassword);

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "PLT1");

			// creating transfers
			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation.ToLocationString(), "PLT1", destinationLocation.ToLocationString(), "");
			transfer.RunPreSaveValidation(); // generate picklines
			webService.Factory.Save();

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var response1 = webService.GetWhsTransfer("PLT1", new SearchFilterCriteriaInfo { PickMethod = "ANY", AreaCode = "ANY" }, false, null);
				AssertEquals(transfer.WD_DocketID, response1.Docket.DocketID);
				AssertEquals(true, response1.ShowStockOnHandWarningOnPutaway);
			}

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var response2 = webService.GetWhsTransfer("PLT1", new SearchFilterCriteriaInfo { PickMethod = "ANY", AreaCode = "ANY" }, false, null);
				AssertEquals(transfer.WD_DocketID, response2.Docket.DocketID);
				AssertEquals(false, response2.ShowStockOnHandWarningOnPutaway);
			}
		}

		#endregion

		#region TestGetWhsTransfer_DoesNotReturnDockDoorTransfers_WithClient

		public void TestGetWhsTransfer_DoesNotReturnDockDoorTransfers_WithClient()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var user = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = user.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(user.StaffPlainTextPassword);

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "PLT1");
			webService.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			webService.Factory.Save();
			AssertEquals("Precondition: Transfer is created.", 1, pick.Transfers.Count);

			var transfer = pick.Transfers.Single();
			AssertBusinessValidationError(webService, $"Can't find un finalized Transfer with Reference: {transfer.WD_DocketID}.", webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo { PickMethod = "ANY", AreaCode = "ANY", ClientCode = data.Org1.OH_Code }, false, null));
		}

		#endregion

		#region TestGetWhsTransfer_DoesNotReturnDockDoorTransfers_WithPalletID

		public void TestGetWhsTransfer_DoesNotReturnDockDoorTransfers_WithPalletID()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var user = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = user.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(user.StaffPlainTextPassword);

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "PLT1");
			webService.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			webService.Factory.Save();
			AssertEquals("Precondition: Transfer is created.", 1, pick.Transfers.Count);

			AssertBusinessValidationError(webService, "Can't find un finalized Transfer with Reference: PLT1.", webService.GetWhsTransfer("PLT1", new SearchFilterCriteriaInfo { PickMethod = "ANY", AreaCode = "ANY" }, false, null));
		}

		#endregion

		#region TestGetWhsTransfer_DoesNotReturnDockDoorTransfers_WithNoReference

		public void TestGetWhsTransfer_DoesNotReturnDockDoorTransfers_WithNoReference()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var user = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = user.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(user.StaffPlainTextPassword);

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "PLT1");
			webService.Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = user.GS_Code;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			webService.Factory.Save();
			AssertEquals("Precondition: Transfer is created.", 1, pick.Transfers.Count);

			AssertBusinessValidationError(webService, "Un-finalized transfer could not be found. Possible mismatch on registered equipment, registered area, registered client or transfers have been assigned to another operator.", webService.GetWhsTransfer("", new SearchFilterCriteriaInfo { PickMethod = "ANY", AreaCode = "ANY" }, false, null));
		}

		#endregion

		#region TestGetWhsTransfer_InPickSequence

		public void TestGetWhsTransfer_InPickSequence()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			var destinationLocation = data.Whs1.FindLocation("A-6");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertTransferResponseContainLocationsSortedByPickSequence(new[] { "A-1", "A-2", "A-3", "A-4", "A-5" }, response);
		}

		public void TestGetWhsTransfer_InModifiedPickSequence()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			sourceLocations[0].WLV_PickPathSequence = 3;
			sourceLocations[1].WLV_PickPathSequence = 5;
			sourceLocations[2].WLV_PickPathSequence = 1;
			sourceLocations[3].WLV_PickPathSequence = 4;
			sourceLocations[4].WLV_PickPathSequence = 2;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertTransferResponseContainLocationsSortedByPickSequence(new[] { "A-3", "A-5", "A-1", "A-4", "A-2" }, response);
		}

		public void TestGetWhsTransfer_InModifiedPickSequence_IncludingSequenceNumberZero()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			sourceLocations[0].WLV_PickPathSequence = 3;
			sourceLocations[1].WLV_PickPathSequence = 0;
			sourceLocations[2].WLV_PickPathSequence = 1;
			sourceLocations[3].WLV_PickPathSequence = 4;
			sourceLocations[4].WLV_PickPathSequence = 2;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertTransferResponseContainLocationsSortedByPickSequence(new[] { "A-3", "A-5", "A-1", "A-4", "A-2" }, response);
		}

		public void TestGetWhsTransfer_InModifiedRowPathSequence()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			helper.CreateRowAndGenerateLocations(data.Whs1, "B", 5);
			helper.CreateRowAndGenerateLocations(data.Whs1, "C", 5);
			helper.CreateRowAndGenerateLocations(data.Whs1, "D", 5);
			helper.CreateRowAndGenerateLocations(data.Whs1, "E", 5);

			webService.Factory.Save();
			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("B-2"),
				data.Whs1.FindLocation("C-3"),
				data.Whs1.FindLocation("D-4"),
				data.Whs1.FindLocation("E-5"),
			};

			sourceLocations[0].Row.WR_PickPathSequence = 5;
			sourceLocations[1].Row.WR_PickPathSequence = 4;
			sourceLocations[2].Row.WR_PickPathSequence = 1;
			sourceLocations[3].Row.WR_PickPathSequence = 2;
			sourceLocations[4].Row.WR_PickPathSequence = 3;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertTransferResponseContainLocationsSortedByPickSequence(new[] { "C-3", "D-4", "E-5", "B-2", "A-1" }, response);
		}

		public void TestGetWhsTransfer_InModifiedRowPathSequence_AllSameSequence()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			helper.CreateRowAndGenerateLocations(data.Whs1, "B", 5);

			webService.Factory.Save();
			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("B-1"),
				data.Whs1.FindLocation("B-2"),
				data.Whs1.FindLocation("B-3"),
				data.Whs1.FindLocation("B-4"),
				data.Whs1.FindLocation("B-5"),
			};

			sourceLocations[0].Row.WR_PickPathSequence = 5;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertTransferResponseContainLocationsSortedByPickSequence(new[] { "B-1", "B-2", "B-3", "B-4", "B-5" }, response);
		}

		public void TestGetWhsTransfer_InPartiallyModifiedRowPathSequence()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			helper.CreateRowAndGenerateLocations(data.Whs1, "B", 5);
			helper.CreateRowAndGenerateLocations(data.Whs1, "C", 5);
			helper.CreateRowAndGenerateLocations(data.Whs1, "D", 5);

			webService.Factory.Save();
			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("B-1"),
				data.Whs1.FindLocation("C-2"),
				data.Whs1.FindLocation("D-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			sourceLocations[0].Row.WR_PickPathSequence = 5;
			sourceLocations[1].Row.WR_PickPathSequence = 4;
			sourceLocations[2].Row.WR_PickPathSequence = 1;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertTransferResponseContainLocationsSortedByPickSequence(new[] { "D-3", "C-2", "B-1", "A-4", "A-5", }, response);
		}

		public void TestGetWhsTransfer_InModifiedPickSequence_AndModifiedRowPathSequence()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			helper.CreateRowAndGenerateLocations(data.Whs1, "B", 5);

			webService.Factory.Save();
			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("B-2"),
				data.Whs1.FindLocation("B-3"),
				data.Whs1.FindLocation("B-4"),
				data.Whs1.FindLocation("A-5"),
			};

			sourceLocations[0].WLV_PickPathSequence = 3;
			sourceLocations[1].WLV_PickPathSequence = 5;
			sourceLocations[2].WLV_PickPathSequence = 1;
			sourceLocations[3].WLV_PickPathSequence = 4;
			sourceLocations[4].WLV_PickPathSequence = 2;

			sourceLocations[0].Row.WR_PickPathSequence = 2;
			sourceLocations[1].Row.WR_PickPathSequence = 3;

			var destinationLocation = data.Whs1.FindLocation("A-6");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertTransferResponseContainLocationsSortedByPickSequence(new[] { "A-5", "A-1", "B-3", "B-4", "B-2" }, response);
		}

		public void TestGetWhsTransfer_InPickSequence_AndPickMethod()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			// Setup Registry
			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			var pickMethod = pickMethods.AddNew();
			pickMethod.Code = "TRB";
			pickMethod.Bool = true;
			pickMethod.Description = (NoResString)"Tractor Beam";

			sourceLocations[0].WLV_PickMethod = "TRB";
			sourceLocations[2].WLV_PickMethod = "TRB";
			sourceLocations[3].WLV_PickMethod = "TRB";

			var destinationLocation = data.Whs1.FindLocation("A-6");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 50m, sourceLocations[3], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 50m, sourceLocations[4], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[4].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[3].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertTransferResponseContainLocationsSortedByPickSequence(new[] { "A-2", "A-5", "A-1", "A-3", "A-4" }, response);
		}

		public void TestGetWhsTransfer_InPickSequence_WithDuplicatedLocations()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 4, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3")
			};

			var destinationLocation = data.Whs1.FindLocation("A-4");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[0].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertTransferResponseContainLocationsSortedByPickSequence(new[] { "A-1", "A-1", "A-2", "A-2", "A-3" }, response);
		}

		public void TestGetWhsTransfer_InPickSequence_WithSomeEmptyDestinationLocation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 4, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
			};

			var destinationLocation = data.Whs1.FindLocation("A-4");

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocation.ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), "");
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), "");

			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertTransferResponseContainLocationsSortedByPickSequence(new[] { "A-1", "A-2", "A-3" }, response);
		}

		public void TestGetWhsTransfer_InPickSequence_DoesNotUseDestinationLocation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
			};

			var destinationLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
				data.Whs1.FindLocation("A-6"),
			};

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[1].ToLocationString(), destinationLocations[0].ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[0].ToLocationString(), destinationLocations[1].ToLocationString());
			helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocations[2].ToLocationString());

			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertTransferResponseContainLocationsSortedByPickSequence(new[] { "A-1", "A-2", "A-3" }, response);
		}

		public void AssertTransferResponseContainLocationsSortedByPickSequence(string[] expected, WhsDocketWebServiceResponse response)
		{
			AssertTransferResponseContainLocationsSortedByPickSequence("", expected, response);
		}

		public void AssertTransferResponseContainLocationsSortedByPickSequence(string message, string[] expected, WhsDocketWebServiceResponse response)
		{
			var transferLines = response.Docket.Lines;
			AssertArrayEqualsByElements(message, expected, transferLines.Select(l => l.Location).ToArray());
		}

		#endregion

		#region TestGetWhsTransfer_InPutawayPathSequence

		public void TestGetWhsTransfer_InPutawayPathSequence()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocation = data.Whs1.FindLocation("A-6");
			var destinationLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			destinationLocations[0].WLV_PutawayPathSequence = 3;
			destinationLocations[1].WLV_PutawayPathSequence = 5;
			destinationLocations[2].WLV_PutawayPathSequence = 1;
			destinationLocations[3].WLV_PutawayPathSequence = 4;
			destinationLocations[4].WLV_PutawayPathSequence = 2;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocation, "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation.ToLocationString(), destinationLocations[0].ToLocationString());
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), destinationLocations[2].ToLocationString());
			var transferLine3 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), destinationLocations[3].ToLocationString());
			var transferLine4 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), destinationLocations[4].ToLocationString());
			var transferLine5 = helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocation.ToLocationString(), destinationLocations[1].ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var now = ZDateTimeOffset.Now;
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			transferLine4.PickedTime = now;
			transferLine5.PickedTime = now;
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, true, null);
			AssertTransferResponseContainLocationsSortedByPutawaySequence(new[] { "A-3", "A-5", "A-1", "A-4", "A-2" }, response);
		}

		public void TestGetWhsTransfer_InPutawayPathSequence_LocationHaveSameSequence()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocation = data.Whs1.FindLocation("A-6");
			var destinationLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			destinationLocations[0].WLV_PutawayPathSequence = 1;
			destinationLocations[1].WLV_PutawayPathSequence = 1;
			destinationLocations[2].WLV_PutawayPathSequence = 1;
			destinationLocations[3].WLV_PutawayPathSequence = 1;
			destinationLocations[4].WLV_PutawayPathSequence = 1;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocation, "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation.ToLocationString(), destinationLocations[0].ToLocationString());
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), destinationLocations[2].ToLocationString());
			var transferLine3 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), destinationLocations[3].ToLocationString());
			var transferLine4 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), destinationLocations[4].ToLocationString());
			var transferLine5 = helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocation.ToLocationString(), destinationLocations[1].ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var now = ZDateTimeOffset.Now;
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			transferLine4.PickedTime = now;
			transferLine5.PickedTime = now;
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, true, null);
			AssertTransferResponseContainLocationsSortedByPutawaySequence(new[] { "A-1", "A-2", "A-3", "A-4", "A-5" }, response);
		}

		public void TestGetWhsTransfer_InPutawayPathSequence_PartialPutaway()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 8, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-6"),
				data.Whs1.FindLocation("A-7"),
				data.Whs1.FindLocation("A-8"),
			};

			sourceLocations[0].WLV_PickPathSequence = 2;
			sourceLocations[1].WLV_PickPathSequence = 5;
			sourceLocations[2].WLV_PickPathSequence = 3;

			var destinationLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
				data.Whs1.FindLocation("A-4"),
				data.Whs1.FindLocation("A-5"),
			};

			destinationLocations[0].WLV_PutawayPathSequence = 1;
			destinationLocations[1].WLV_PutawayPathSequence = 2;
			destinationLocations[2].WLV_PutawayPathSequence = 3;
			destinationLocations[3].WLV_PutawayPathSequence = 4;
			destinationLocations[4].WLV_PutawayPathSequence = 5;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocations[0], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 50m, sourceLocations[1], "");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, sourceLocations[2], "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocations[2].ToLocationString(), destinationLocations[0].ToLocationString());
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[2].ToLocationString(), destinationLocations[1].ToLocationString());
			var transferLine3 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[1].ToLocationString(), destinationLocations[2].ToLocationString());
			var transferLine4 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocations[0].ToLocationString(), destinationLocations[3].ToLocationString());
			var transferLine5 = helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocations[1].ToLocationString(), destinationLocations[4].ToLocationString());
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var now = ZDateTimeOffset.Now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			transferLine5.PickedTime = now;
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, true, null);
			AssertTransferResponseContainLocationsSortedByPutawaySequence(new[] { "A-2", "A-3", "A-5" }, response);
		}

		public void TestGetWhsTransfer_InPutawayPathSequence_MissingLocation()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 6, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = staff.GS_LoginName;
			webService.SecurityHeader.Password = GetEncryptedText(staff.StaffPlainTextPassword);

			var sourceLocation = data.Whs1.FindLocation("A-6");
			var destinationLocations = new WhsLocation[] {
				data.Whs1.FindLocation("A-1"),
				data.Whs1.FindLocation("A-2"),
				data.Whs1.FindLocation("A-3"),
			};

			destinationLocations[0].WLV_PutawayPathSequence = 3;
			destinationLocations[1].WLV_PutawayPathSequence = 5;
			destinationLocations[2].WLV_PutawayPathSequence = 1;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocation, "");

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation.ToLocationString(), destinationLocations[0].ToLocationString());
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), destinationLocations[1].ToLocationString());
			var transferLine3 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), destinationLocations[2].ToLocationString());
			var transferLine4 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation.ToLocationString(), "");
			var transferLine5 = helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocation.ToLocationString(), "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var now = ZDateTimeOffset.Now;
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			transferLine4.PickedTime = now;
			transferLine5.PickedTime = now;
			webService.Factory.Save();

			var response = webService.GetWhsTransfer(transfer.WD_ExternalReference, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, true, null);
			AssertTransferResponseContainLocationsSortedByPutawaySequence(new[] { "A-3", "A-1", "A-2" }, response);
		}

		public void AssertTransferResponseContainLocationsSortedByPutawaySequence(string[] expected, WhsDocketWebServiceResponse response)
		{
			var transferLines = response.Docket.Lines;
			AssertArrayEqualsByElements("", expected, transferLines.Select(l => l.DestLocation).ToArray());
		}

		#endregion

		#region  TestGetWhsTransfer_DoesNotReturnPutawayTransfers

		#region TestGetWhsTransfer_DoesNotReturnDockDoorTransfers_WithClient

		public void TestGetWhsTransfer_DoesNotReturnPutawayTransfers_WithClient()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, "12345", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultOutboundDockDoorLocation, "12345", 20m);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, nonDockDoorLocation, "12345", 10m);
			var transferLineForPart2 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part2, data.Whs1.DefaultOutboundDockDoorLocation, nonDockDoorLocation, "12345", 20m);
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff1);

			AssertNotNull("Precondition: inventory1 has putaway transfer.", ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransfer);
			AssertNotNull("Precondition: inventory2 has putaway transfer.", ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransfer);

			AssertBusinessValidationError(webService, $"Can't find un finalized Transfer with Reference: {transfer.WD_DocketID}.", webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo { PickMethod = "ANY", AreaCode = "ANY", ClientCode = data.Org1.OH_Code }, false, null));
		}

		#endregion

		#region TestGetWhsTransfer_DoesNotReturnDockDoorTransfers_WithPalletID

		public void TestGetWhsTransfer_DoesNotReturnPutawayTransfers_WithPalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, "12345", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultOutboundDockDoorLocation, "12345", 20m);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, nonDockDoorLocation, "12345", 10m);
			var transferLineForPart2 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part2, data.Whs1.DefaultOutboundDockDoorLocation, nonDockDoorLocation, "12345", 20m);
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff1);

			AssertNotNull("Precondition: inventory1 has putaway transfer.", ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransfer);
			AssertNotNull("Precondition: inventory2 has putaway transfer.", ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransfer);

			AssertBusinessValidationError(webService, "Can't find un finalized Transfer with Reference: 12345.", webService.GetWhsTransfer("12345", new SearchFilterCriteriaInfo { PickMethod = "ANY", AreaCode = "ANY" }, false, null));
		}

		#endregion

		#region TestGetWhsTransfer_DoesNotReturnDockDoorTransfers_WithNoReference

		public void TestGetWhsTransfer_DoesNotReturnPutawayTransfers_WithNoReference()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, "12345", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultOutboundDockDoorLocation, "12345", 20m);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, nonDockDoorLocation, "12345", 10m);
			var transferLineForPart2 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part2, data.Whs1.DefaultOutboundDockDoorLocation, nonDockDoorLocation, "12345", 20m);
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff1);

			AssertNotNull("Precondition: inventory1 has putaway transfer.", ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransfer);
			AssertNotNull("Precondition: inventory2 has putaway transfer.", ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransfer);

			AssertBusinessValidationError(webService, "Un-finalized transfer could not be found. Possible mismatch on registered equipment, registered area, registered client or transfers have been assigned to another operator.",
				webService.GetWhsTransfer("", new SearchFilterCriteriaInfo { PickMethod = "ANY", AreaCode = "ANY" }, false, null));
		}

		#endregion

		#endregion

		#region TestGetWhsTransfer_TaskManagementEnabled

		public void TestGetWhsTransfer_TaskManagementEnabled_NoProcessTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			AssertNotEquals(TaskPlanningStatus.Codes.Planned, transfer.WD_TaskPlanningStatus);
			AssertEquals("Precondition", Guid.Empty, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", Guid.Empty, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", 0, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Single();
			AssertEquals(WarehouseTaskFormFlowTypes.TransferJob, transferProcessTask.P9_FormFlowType);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTask.P9_Status);
			AssertEquals("S2", transferProcessTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(transferProcessTask.PK, response.Docket.TaskPK);

			var transferLine1InNewFactory = newFactory.Load<WhsTransferLine>(transferLine1.PK);
			AssertEquals(transferProcessTask.PK, transferLine1InNewFactory.WE_P9_Task);
			var transferLine2InNewFactory = newFactory.Load<WhsTransferLine>(transferLine2.PK);
			AssertEquals(transferProcessTask.PK, transferLine2InNewFactory.WE_P9_Task);

			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);
			AssertEquals(TaskPlanningStatus.Codes.Planned, transferInNewFactory.WD_TaskPlanningStatus);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_HasProcessTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = transferProcessTask.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask.PK, response.Docket.TaskPK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Single();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_HasProcessTask_SomeLinesHasNoTaskAssigned()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = Guid.Empty;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask.PK, response.Docket.TaskPK);
			AssertEquals(transferLine1.PK, response.Docket.Lines.Single().PK);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Single();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_HasProcessTask_LinesHasNoTaskAssigned()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = Guid.Empty;
			transferLine2.WE_P9_Task = Guid.Empty;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertBusinessValidationError(webService, "Un-finalized transfer could not be found for Reference / Pallet ID : W00000002. Possible mismatch on registered equipment, registered area, registered client or transfer / Pallet ID  has been assigned to another operator", response);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Single();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_HasProcessTask_Assigned()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff1 = Helper.CreateGlbStaff("S2", "S2");
			var staff2 = Helper.CreateGlbStaff("S3", "S3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff2);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = transferProcessTask.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertBusinessValidationError(webService, "This transfer is assigned to another user.", response);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_HasProcessTask_Completed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = transferProcessTask.PK;
			Helper.Factory.Save();

			var webServiceToComplete = GetNewWebService(data.Whs1, staff);
			webServiceToComplete.CompleteTask(transferProcessTask.PK.ToGuid());
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertBusinessValidationError(webService, "This transfer is already completed.", response);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Single();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_HasMultipleProcessTasks_MultipleAssignedToUser_TakesSuspendedFirst()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			var webServiceToComplete = GetNewWebService(data.Whs1, staff);
			webServiceToComplete.SuspendTask(transferProcessTask1.PK.ToGuid());
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Suspended, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask1.PK, response.Docket.TaskPK);
			AssertEquals(transferLine1.PK, response.Docket.Lines.Single().PK);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_HasMultipleProcessTasks_MultipleAssignedToUser_TakesAssignedOverOpen()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask1.PK, response.Docket.TaskPK);
			AssertEquals(transferLine1.PK, response.Docket.Lines.Single().PK);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTask1InNewFactory.P9_Status);

			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_HasMultipleProcessTasks_NothingAssigned()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer);
			transferProcessTask1.P9_SystemCreateTimeUtc = DateTime.Now.AddDays(-1);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferProcessTask2.P9_SystemCreateTimeUtc = DateTime.Now;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask1.PK, response.Docket.TaskPK);
			AssertEquals(transferLine1.PK, response.Docket.Lines.Single().PK);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_HasMultipleProcessTasks_AllAssignedToOtherUsers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff1 = Helper.CreateGlbStaff("S3", "S3");
			var otherStaff2 = Helper.CreateGlbStaff("S4", "S4");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff1);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff2);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertBusinessValidationError(webService, "This transfer is assigned to another user.", response);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1InNewFactory.P9_Status);
			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_HasMultipleProcessTasks_OtherLinesWithProcessTaskAssignedToOtherUsers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(transferProcessTask1.PK, response.Docket.TaskPK);
			AssertEquals(transferLine1.PK, response.Docket.Lines.Single().PK);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementNotEnabled_HasProcessTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(transferProcessTask.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementNotEnabled_HasProcessTask_AssignedToOtherUsers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff1 = Helper.CreateGlbStaff("S3", "S3");
			var otherStaff2 = Helper.CreateGlbStaff("S4", "S4");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff1);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff2);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1InNewFactory.P9_Status);
			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementNotEnabled_HasProcessTask_Completed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = transferProcessTask.PK;
			Helper.Factory.Save();

			var webServiceToCompleteTask = GetNewWebService(data.Whs1, staff);
			webServiceToCompleteTask.CompleteTask(transferProcessTask.PK.ToGuid());
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(transferProcessTask.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementNotEnabled_HasProcessTask_LinesWithNoTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = transferProcessTask.PK;
			transferLine2.WE_P9_Task = Guid.Empty;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(transferProcessTask.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementNotEnabled_HasProcessTask_SomeLinesAssignedToOtherUsers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var transferProcessTask1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var transferProcessTask2 = Helper.CreateProcessTaskForTransfer(transfer, otherStaff);
			transferLine1.WE_P9_Task = transferProcessTask1.PK;
			transferLine2.WE_P9_Task = transferProcessTask2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2.P9_Status);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsTransfer(transfer.WD_DocketID, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
			AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
			AssertEquals(2, response.Docket.Lines.Count);
			AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1InNewFactory.P9_Status);
			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(transferProcessTask2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.TransferJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				})
				.Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
				AssertEquals(2, response.Docket.Lines.Count);
				AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_ErrorReturnedFromTaskManagementService()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult("Error Here"));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "Error Here", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_SomeLinesOnAnotherTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var task1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var task2 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = task1.PK;
			transferLine2.WE_P9_Task = task2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", task1.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task2.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			AssertEquals("Precondition", 2, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.TransferJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				})
				.Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
				AssertEquals(data.Part1.PK, response.Docket.Lines.Single().Product.PK);
				AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(task1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, transferProcessTask1InNewFactory.P9_Status);

			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(task2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_NoLinesToTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var task1 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			var task2 = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine1.WE_P9_Task = task2.PK;
			transferLine2.WE_P9_Task = task2.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition", task2.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task2.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			AssertEquals("Precondition", 2, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.TransferJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				})
				.Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "Un-finalized transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTask1InNewFactory = newFactory.Load<ProcessTask>(task1.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask1InNewFactory.P9_Status);

			var transferProcessTask2InNewFactory = newFactory.Load<ProcessTask>(task2.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTask2InNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_NoAssociatedTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(ZGuid.BrettsGuid.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "Un-finalized transfer could not be found.", response);
			}
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_TaskParentIsNotTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var receiveTask = Helper.CreateProcessTaskForReceive(receive, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, receiveTask.P9_Status);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(receiveTask.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "Un-finalized transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var receiveProcessTaskInNewFactory = newFactory.Load<ProcessTask>(receiveTask.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, receiveProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_TaskParentIsNull()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var dummyTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			dummyTask.P9_ParentID = ZGuid.Empty;
			Helper.Factory.Save();

			AssertEquals("Precondition", dummyTask.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", dummyTask.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, dummyTask.P9_Status);
			AssertEquals("Precondition", 0, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(dummyTask.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "Un-finalized transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var dummyTaskInNewFactory = newFactory.Load<ProcessTask>(dummyTask.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, dummyTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_FinalisedTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transferLine2.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine2);

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "Un-finalized transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_DockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "PLT1");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition: Transfer is created.", 1, pick.Transfers.Count);

			var transfer = pick.Transfers.Single();
			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "Un-finalized transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_InvalidProcessFlowType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.LoadJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "Un-finalized transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_InterWhsChild_Source() => TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_InterWhsChild(isSource: true);
		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_InterWhsChild_Dest() => TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_InterWhsChild(isSource: false);

		void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_InterWhsChild(bool isSource)
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var warehouse2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, isSource ? data.Whs1 : warehouse2, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", warehouse2.PK, "A");
			transferLine.RunPreSaveValidation();

			var now = ZDateTimeOffset.Now;
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var childTransfer = transfer.ChildTransfers.First();
			AssertNotNull("Precondition: Created child transfer.", childTransfer);
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(childTransfer, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "Un-finalized transfer could not be found.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_TaskIsClosed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.TransferJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
				})
				.Returns(UpdateTaskStatusResult.TaskStatusIsCompleted);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "The task is already closed and cannot be updated. Please check the task status and try again.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_TaskIsAssignedToAnotherUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, otherStaff);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));
			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.TransferJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
					AssertEquals("S3", task.P9_GS_NKAssignedStaffMember);
				})
				.Returns(UpdateTaskStatusResult.AssignedUserIsDifferent);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "This task is not assigned to the current user. Please perform a different task.", response);
			}

			var newFactory = new BusinessObjectFactory();
			var transferProcessTaskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, transferProcessTaskInNewFactory.P9_Status);
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_ConcurrencyException()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var innerException = new Exception();
				var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)task).Row, Db.Connection);
				webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertBusinessValidationError(webService, "Another user has changed the Transfer Job while you have been working on it. Please restart the operation and try again.", response);
			}
		}

		public void TestGetWhsTransfer_TaskManagementEnabled_EmptyReference_IsInvokedInTemporaryUserContext()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-3");
			transfer.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForTransfer(transfer, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Precondition", task.PK, transferLine2.WE_P9_Task);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			AssertEquals("Precondition", 1, Helper.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, transfer.PK)).Length);

			var glowUserDataManagerMock = new Mock<IGlowUserDataManager>();
			var disposableMock = new Mock<IDisposable>();
			glowUserDataManagerMock.Setup(g => g.IncreaseTempUserCount()).Returns(disposableMock.Object);

			var envBranch = GlbBranch.CurrentBranch.PK.ToGuid();
			var envDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();

			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceManagerMock
				.Setup(t => t.GetNextTask(It.IsAny<BusinessObjectFactory>(), string.Empty, staff.PK.ToGuid(), data.Whs1.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob, string.Empty, Array.Empty<Guid>()))
				.Callback(() =>
				{
					glowUserDataManagerMock.Verify(g => g.IncreaseTempUserCount());

					AssertEquals(staff.PK, GlbStaff.CurrentUser.PK);
					AssertEquals(envBranch, GlbBranch.CurrentBranch.PK);
					AssertEquals(envDepartment, GlbDepartment.CurrentDepartment.PK);
				})
				.Returns(new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.TransferJob));

			taskManagementServiceManagerMock
				.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.TransferJob, "S2"))
				.Callback((IProcessTask task, string formFlowType, string staffCode) =>
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				})
				.Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute("IGlowServiceClientFactory", glowUserDataManagerMock.Object))

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsTransfer("", new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, false, null);
				AssertEquals(transfer.PK.ToGuid(), response.Docket.PK);
				AssertEquals(2, response.Docket.Lines.Count);
				AssertTransferEventsCreated(transfer, 1, 1, 0, 0, 0, 0);
				AssertSuccessfulResponseWithNoErrors(response, webService);

				disposableMock.Verify(d => d.Dispose());
			}
		}

		#endregion

		#region Implementation

		static SearchFilterCriteriaInfo DefaultCriteria => new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" };

		#endregion
	}
}
