using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PickAnotherPalletIDToTransferTest : WhsSecureServiceTestCase
	{
		#region TestPickAnotherPalletIDToTransfer

		public void TestPickAnotherPalletIDToTransfer()
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
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "plt-123", "plt-456");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo = response.Docket.Lines.Single();
			AssertEquals("PLT-456", transferLineInfo.PalletID);
			AssertEquals("", transferLineInfo.DestPalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456" },
				response.Docket.PalletsToTransferCompletely);

			var reloadedTransferLine = new BusinessObjectFactory().Load<WhsTransferLine>(transferLine.PK);
			AssertEquals("New Pallet ID should be saved.", "PLT-456", reloadedTransferLine.WE_TransferFromPalletId);
			AssertEquals("Quantity should be correct.", 10m, reloadedTransferLine.QtyToMoveIncludingMatchingLines);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_FullPalletTransfer

		public void TestPickAnotherPalletIDToTransfer_FullPalletTransfer()
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
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "A-2", "PLT-123");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo = response.Docket.Lines.Single();
			AssertEquals("PLT-456", transferLineInfo.PalletID);
			AssertEquals("PLT-456", transferLineInfo.DestPalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456" },
				response.Docket.PalletsToTransferCompletely);

			var reloadedTransferLine = new BusinessObjectFactory().Load<WhsTransferLine>(transferLine.PK);
			AssertEquals("New Pallet ID should be saved.", "PLT-456", reloadedTransferLine.WE_TransferFromPalletId);
			AssertEquals("New Pallet ID should be saved.", "PLT-456", reloadedTransferLine.WE_PalletID);
			AssertEquals("Quantity should be correct.", 10m, reloadedTransferLine.QtyToMoveIncludingMatchingLines);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_CannotPassInInvalidTransferPK

		public void TestPickAnotherPalletIDToTransfer_CannotPassInInvalidTransferPK()
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

			var response = webService.PickAnotherPalletIDToTransfer(Guid.Empty, "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Could not find Transfer.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_DoesNotAcceptNullOrEmptyStrings

		public void TestPickAnotherPalletIDToTransfer_DoesNotAcceptNullOrEmptyStrings()
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

			var response1 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "", "PLT-456");
			AssertSuccessfulResponse(response1, webService);
			AssertEquals("No Pallet ID provided for allocation.", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			var response2 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), null, "PLT-456");
			AssertSuccessfulResponse(response2, webService);
			AssertEquals("No Pallet ID provided for allocation.", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);

			var response3 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "");
			AssertSuccessfulResponse(response3, webService);
			AssertEquals("No Pallet ID provided for allocation.", response3.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response3.Error);

			var response4 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", null);
			AssertSuccessfulResponse(response4, webService);
			AssertEquals("No Pallet ID provided for allocation.", response4.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response4.Error);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_WithValidationError

		public void TestPickAnotherPalletIDToTransfer_WithValidationError()
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
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			transferLine.WE_TransferFromPalletIdInfo.ValueChanged += (sender, e) => transferLine.AddRowError("TEST ERROR");

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Error - Docket Line: TEST ERROR", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_NewPalletHasGreaterQuantity

		public void TestPickAnotherPalletIDToTransfer_NewPalletHasGreaterQuantity()
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
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 15m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo = response.Docket.Lines.Single();
			AssertEquals("PLT-456", transferLineInfo.PalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
			AssertEquals(10m, transferLineInfo.Qty);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123" },
				response.Docket.PalletsToTransferCompletely);

			var reloadedTransferLine = new BusinessObjectFactory().Load<WhsTransferLine>(transferLine.PK);
			AssertEquals("New Pallet ID should be saved.", "PLT-456", reloadedTransferLine.WE_TransferFromPalletId);
			AssertEquals("Quantity should be correct.", 10m, reloadedTransferLine.QtyToMoveIncludingMatchingLines);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_NewPalletHasLessQuantity

		public void TestPickAnotherPalletIDToTransfer_NewPalletHasLessQuantity()
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
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 9m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-456' as it does not have enough stock.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_NewPalletHasGreaterQuantity_CannotSwapIfTransferringFullPallet

		public void TestPickAnotherPalletIDToTransfer_NewPalletHasGreaterQuantity_CannotSwapIfTransferringFullPallet()
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
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 15m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "A-2", "PLT-123");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertEquals("Unable to transfer Pallet 'PLT-456' as it would result in a partial Pallet Transfer when full Pallet Transfer is required.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_NewPalletHasGreaterQuantity_CanSwapIfTransferringToAnotherPallet

		public void TestPickAnotherPalletIDToTransfer_NewPalletHasGreaterQuantity_CanSwapIfTransferringToAnotherPallet()
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
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 15m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "A-2", "PLT-789");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo = response.Docket.Lines.Single();
			AssertEquals("PLT-456", transferLineInfo.PalletID);
			AssertEquals("PLT-789", transferLineInfo.DestPalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
			AssertEquals(10m, transferLineInfo.Qty);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123" },
				response.Docket.PalletsToTransferCompletely);

			var reloadedTransferLine = new BusinessObjectFactory().Load<WhsTransferLine>(transferLine.PK);
			AssertEquals("New Pallet ID should be saved.", "PLT-456", reloadedTransferLine.WE_TransferFromPalletId);
			AssertEquals("Dest. Pallet ID should be unchanged.", "PLT-789", reloadedTransferLine.WE_PalletID);
			AssertEquals("Quantity should be correct.", 10m, reloadedTransferLine.QtyToMoveIncludingMatchingLines);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails

		public void TestPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails_DifferentProduct()
		{
			AssertPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails((l, data) =>
			{
				l.WE_OP = data.Part2.PK;
				l.WE_WHC_NKOriginalInventoryHeldCode = "COVID";
			});
		}

		public void TestPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails_DifferentHoldCode()
		{
			AssertPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails((l, data) => l.WE_WHC_NKOriginalInventoryHeldCode = "FLU");
		}

		public void TestPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails_DifferentExpiry()
		{
			var year = ZDate.Today.Year;
			AssertPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails((l, data) => l.WE_ExpiryDate = new ZDate(year, 2, 1));
		}

		public void TestPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails_DifferentPacking()
		{
			var year = ZDate.Today.Year;
			AssertPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails((l, data) => l.WE_PackingDate = new ZDate(year, 2, 1));
		}

		public void TestPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails_DifferentPartAttrib1()
		{
			AssertPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails((l, data) => l.WE_PartAttrib1 = "ZZ1");
		}

		public void TestPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails_DifferentPartAttrib2()
		{
			AssertPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails((l, data) => l.WE_PartAttrib2 = "ZZ2");
		}

		public void TestPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails_DifferentPartAttrib3()
		{
			AssertPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails((l, data) => l.WE_PartAttrib3 = "ZZ3");
		}

		public void TestPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails_DifferentDestinationLocation()
		{
			AssertPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails((l, data) =>
			{
				if (l is WhsTransferLine line)
				{
					line.LocationString = "A-1";
				}
			});
		}

		public void TestPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails_DifferentDestPalletID()
		{
			AssertPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails((l, data) =>
			{
				if (l is WhsTransferLine line)
				{
					line.WE_PalletID = "PLT-XXX";
				}
			});
		}

		void AssertPickAnotherPalletIDToTransfer_AllocatedTransferLinesMustHaveSameTransferringDetails(Action<WhsDocketLine, TestDataSimpleEnvironment> setFieldToDifferentValue)
		{
			var year = ZDate.Today.Year;
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: false);
			helper.CreateInventoryHeldCode("COVID", "Corona Virus");
			helper.CreateInventoryHeldCode("FLU", "Influenza");

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
			var pallet1Line1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location1, "PLT-123", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			var pallet1Line2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location1, "PLT-123", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			pallet1Line1.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = "COVID";
			pallet1Line2.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = "COVID";

			var pallet2Line1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location1, "PLT-456", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			var pallet2Line2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location1, "PLT-456", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			pallet2Line1.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = "COVID";
			pallet2Line2.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = "COVID";

			setFieldToDifferentValue(pallet1Line2.InDocketLine, data);
			setFieldToDifferentValue(pallet2Line2.InDocketLine, data);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			helper.CreateWhsTransferLine(transfer, data.Part1.PK, 5m, "A-1", "PLT-123", data.Whs1.PK, "A-2", "PLT-789", ZDateTimeOffset.Empty, new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", heldCode: "COVID");
			var transferLineToChange = helper.CreateWhsTransferLine(transfer, data.Part1.PK, 5m, "A-1", "PLT-123", data.Whs1.PK, "A-2", "PLT-789", ZDateTimeOffset.Empty, new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", heldCode: "COVID");

			setFieldToDifferentValue(transferLineToChange, data);

			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Pallet ID Neutral is not supported for non-uniform Pallets.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_PalletInDifferentLocation

		public void TestPickAnotherPalletIDToTransfer_PalletInDifferentLocation()
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

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-789");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-789' as there is a mismatch on Location, Product, Part Attributes or Hold Code.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_DoesNotAllowSwappingQuantityOfPalletAllocatedToRFGun

		public void TestPickAnotherPalletIDToTransfer_DoesNotAllowSwappingQuantityOfPalletAllocatedToRFGun()
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

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			pickLine.WZ_IsPicking = true;
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			webService.Factory.Save();

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-456' as another Job has specifically allocated this Pallet.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_DoesNotAllowSwappingToInTransitPallet

		public void TestPickAnotherPalletIDToTransfer_DoesNotAllowSwappingToInTransitPallet()
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

			var response = webService.PickAnotherPalletIDToTransfer(transfer1.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Pallet ID 'PLT-456' does not exist or is not Available to Transfer.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_DoesNotAllowingSwappingToPuttingAwayPallets

		public void TestPickAnotherPalletIDToTransfer_DoesNotAllowingSwappingToPuttingAwayPallets()
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

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-789");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Pallet ID 'PLT-789' does not exist or is not Available to Transfer.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_DoesNotAllowSwappingToPalletsOnUnfinalisedReceives

		public void TestPickAnotherPalletIDToTransfer_DoesNotAllowSwappingToPalletsOnUnfinalisedReceives()
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

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-789");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Pallet ID 'PLT-789' does not exist or is not Available to Transfer.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_CannotSwapPalletThatIsPicked

		public void TestPickAnotherPalletIDToTransfer_CannotSwapPalletThatIsPicked()
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
			webService.Factory.Save();

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Current Pallet is already Picked.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_CannotSwapToPalletWithMismatchOnProduct

		public void TestPickAnotherPalletIDToTransfer_CannotSwapToPalletWithMismatchOnProduct()
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

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-456' as there is a mismatch on Location, Product, Part Attributes or Hold Code.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_CannotSwapToPalletWithMismatchOnAttribs

		public void TestPickAnotherPalletIDToTransfer_CannotSwapToPalletWithMismatchOnAttribs()
		{
			var year = ZDateTime.Today.Year;
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
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
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-007", new ZDate(year, 1, 1), new ZDate(year, 1, 2), "Pa1", "pA2", "pa3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, "PLT-001", location2, new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response1 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-001", "PLT-002");
			AssertSuccessfulResponse(response1, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-002' as there is a mismatch on Location, Product, Part Attributes or Hold Code.", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			var response2 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-001", "PLT-003");
			AssertSuccessfulResponse(response2, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-003' as there is a mismatch on Location, Product, Part Attributes or Hold Code.", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);

			var response3 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-001", "PLT-004");
			AssertSuccessfulResponse(response3, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-004' as there is a mismatch on Location, Product, Part Attributes or Hold Code.", response3.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response3.Error);

			var response4 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-001", "PLT-005");
			AssertSuccessfulResponse(response4, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-005' as there is a mismatch on Location, Product, Part Attributes or Hold Code.", response4.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response4.Error);

			var response5 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-001", "PLT-006");
			AssertSuccessfulResponse(response5, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-006' as there is a mismatch on Location, Product, Part Attributes or Hold Code.", response5.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response5.Error);

			var response6 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-001", "PLT-007");
			AssertSuccessfulResponse(response6, webService);
			AssertNull(response6.ErrorMessage);

			var transferLineInfo = response6.Docket.Lines.Single();
			AssertEquals("PLT-007", transferLineInfo.PalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-001", "PLT-007" },
				response6.Docket.PalletsToTransferCompletely);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_CannotSwapToPalletWithMismatchOnHeldCode

		public void TestPickAnotherPalletIDToTransfer_CannotSwapToPalletWithMismatchOnHeldCode()
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
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-001", "PUT", "Covid");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-002", "PUT", "coVid");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT-003", "PUT", "FLU");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-001", "A-2", "", "COVID");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var response1 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-001", "PLT-003");
			AssertSuccessfulResponse(response1, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-003' as there is a mismatch on Location, Product, Part Attributes or Hold Code.", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("Error returned, no lines are set.", 0, response1.Docket.Lines.Count);

			// test that hold code check is case insensitive
			var response2 = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-001", "PLT-002");
			AssertSuccessfulResponse(response2, webService);
			AssertNull(response2.ErrorMessage);

			var transferLineInfo = response2.Docket.Lines.Single();
			AssertEquals("PLT-002", transferLineInfo.PalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_CanSwapWhenSerialsDoNotMatch

		public void TestPickAnotherPalletIDToTransfer_CanSwapWhenSerialsDoNotMatch()
		{
			var year = ZDateTime.Today.Year;
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.Mandatory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
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
			var line1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "PLT-001", ZDate.Empty, ZDate.Empty, "A1", "B1", "C1", "");
			line1.WI_SerialNumber = "SER1";
			var line2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "PLT-001", ZDate.Empty, ZDate.Empty, "A1", "B1", "C1", "");
			line2.WI_SerialNumber = "SER2";
			var line3 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "PLT-002", ZDate.Empty, ZDate.Empty, "A1", "B1", "C1", "");
			line3.WI_SerialNumber = "SER3";
			var line4 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "PLT-002", ZDate.Empty, ZDate.Empty, "A1", "B1", "C1", "");
			line4.WI_SerialNumber = "SER4";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location1, "PLT-001", location2, ZDate.Empty, ZDate.Empty, "A1", "B1", "C1");
			transferLine1.WE_SerialNumber = "SER1";
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location1, "PLT-001", location2, ZDate.Empty, ZDate.Empty, "A1", "B1", "C1");
			transferLine2.WE_SerialNumber = "SER2";
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2m);
			helper.CreatePickNew(order);
			var pickLine1 = order.Lines.Single().PickLines.Single(pl => pl.InventoryLine.WE_SerialNumber == "SER3");
			var pickLine2 = order.Lines.Single().PickLines.Single(pl => pl.InventoryLine.WE_SerialNumber == "SER4");

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-001", "PLT-002");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo1 = response.Docket.Lines.Single(l => l.PK == transferLine1.PK);
			var transferLineInfo2 = response.Docket.Lines.Single(l => l.PK == transferLine2.PK);
			AssertEquals("PLT-002", transferLineInfo1.PalletID);
			AssertEquals("PLT-002", transferLineInfo2.PalletID);
			AssertContainsExactElementsInAnyOrder(new[] { "SER3", "SER4" },
				new[] { transferLineInfo1, transferLineInfo2 }.Select(l => l.SerialNumber));
			AssertContainsExactElementsInAnyOrder(new[] { "SER1", "SER2" },
				new[] { pickLine1.InventoryLine, pickLine2.InventoryLine }.Select(i => (string)i.WE_SerialNumber));
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-001", "PLT-002" },
				response.Docket.PalletsToTransferCompletely);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_CannotSwapQuantityOfPalletCommittedToAdjustment

		public void TestPickAnotherPalletIDToTransfer_CannotSwapQuantityOfPalletCommittedToAdjustment()
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

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-456' as another Job has specifically allocated this Pallet.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_CannotSwapQuantityOfPalletCommittedToAnotherTransfer

		public void TestPickAnotherPalletIDToTransfer_CannotSwapQuantityOfPalletCommittedToAnotherTransfer()
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

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Unable to transfer Pallet 'PLT-456' as another Job has specifically allocated this Pallet.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error returned, no lines are set.", 0, response.Docket.Lines.Count);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_AllowsSwappingToPalletCommittedToOrder

		public void TestPickAnotherPalletIDToTransfer_AllowsSwappingToPalletCommittedToOrder()
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
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
			helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			AssertEquals("Precondition", "PLT-456", pickLine.InventoryLine.WE_PalletID);

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo = response.Docket.Lines.Single();
			AssertEquals("PLT-456", transferLineInfo.PalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
			AssertEquals("Pick Line should have been re-wired.", "PLT-123", pickLine.InventoryLine.WE_PalletID);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456" },
				response.Docket.PalletsToTransferCompletely);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_AllowsSwappingToBiggerPalletCommittedToOrder

		public void TestPickAnotherPalletIDToTransfer_AllowsSwappingToBiggerPalletCommittedToOrder()
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
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 15m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			helper.CreatePickNew(order);
			var originalPickLine = order.Lines.Single().PickLines.Single();
			AssertEquals("Precondition", "PLT-456", originalPickLine.InventoryLine.WE_PalletID);

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo = response.Docket.Lines.Single();
			AssertEquals("PLT-456", transferLineInfo.PalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
			AssertEquals("Original Pick Line should have been split.", 5m, originalPickLine.WZ_Units);
			AssertEquals("Original Pick Line should point to its Original Pallet.", "PLT-456", originalPickLine.InventoryLine.WE_PalletID);

			var newPickLine = order.Lines.Single().PickLines.Single(pl => pl.PK != originalPickLine.PK);
			AssertEquals("Split Pick Line should have quantity swapped.", 10m, newPickLine.WZ_Units);
			AssertEquals("Split Pick Line should point to the Pallet the Transfer used to commit.", "PLT-123", newPickLine.InventoryLine.WE_PalletID);

			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123" },
				response.Docket.PalletsToTransferCompletely);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_SwappingPalletAcrossMultipleInventoriesCommittedToOrder

		public void TestPickAnotherPalletIDToTransfer_SwappingPalletAcrossMultipleInventoriesToPalletCommittedToOrder()
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
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, location1, "PLT-123");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, location1, "PLT-123");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			helper.CreatePickNew(order);
			var originalPickLine = order.Lines.Single().PickLines.Single();
			AssertEquals("Precondition", "PLT-456", originalPickLine.InventoryLine.WE_PalletID);

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo = response.Docket.Lines.Single();
			AssertEquals("PLT-456", transferLineInfo.PalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
			AssertEquals("Original Pick Line should have been split.", 4m, originalPickLine.WZ_Units);
			AssertEquals("Original Pick Line should point to the Pallet the Transfer used to commit.", "PLT-123", originalPickLine.InventoryLine.WE_PalletID);

			var newPickLine = order.Lines.Single().PickLines.Single(pl => pl.PK != originalPickLine.PK);
			AssertEquals("Split Pick Line should have quantity swapped.", 6m, newPickLine.WZ_Units);
			AssertEquals("Split Pick Line should point to the Pallet the Transfer used to commit.", "PLT-123", newPickLine.InventoryLine.WE_PalletID);

			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456" },
				response.Docket.PalletsToTransferCompletely);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_SwappingToPalletAcrossMultipleInventoriesAndCommittedToOrder

		public void TestPickAnotherPalletIDToTransfer_SwappingToPalletAcrossMultipleInventoriesAndCommittedToOrder()
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
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, location1, "PLT-123");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, location1, "PLT-123");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "PLT-456");
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 10m, location2, "PLT-789");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-456", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			helper.CreatePickNew(order);
			var pickLine1 = order.Lines.Single().PickLines.Single(pl => pl.WZ_Units == 6m);
			var pickLine2 = order.Lines.Single().PickLines.Single(pl => pl.WZ_Units == 4m);
			AssertEquals("Precondition", "PLT-123", pickLine1.InventoryLine.WE_PalletID);
			AssertEquals("Precondition", "PLT-123", pickLine1.InventoryLine.WE_PalletID);

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-456", "PLT-123");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo = response.Docket.Lines.Single();
			AssertEquals("PLT-123", transferLineInfo.PalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
			AssertEquals("Original Pick Line should point to the Pallet the Transfer used to commit.", "PLT-456", pickLine1.InventoryLine.WE_PalletID);
			AssertEquals("Original Pick Line should point to the Pallet the Transfer used to commit.", "PLT-456", pickLine2.InventoryLine.WE_PalletID);

			AssertContainsExactElementsInAnyOrder(new[] { "PLT-456", "PLT-123" },
				response.Docket.PalletsToTransferCompletely);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_SwappingWhenPalletIsAcrossMultipleTransferLines

		public void TestPickAnotherPalletIDToTransfer_SwappingWhenPalletIsAcrossMultipleTransferLines()
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
			var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "PLT-123", "", "");
			var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			helper.CreatePickNew(order);
			var originalPickLine = order.Lines.Single().PickLines.Single();
			AssertEquals("Precondition", "PLT-456", originalPickLine.InventoryLine.WE_PalletID);

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo1 = response.Docket.Lines.Single(l => l.PK == transferLine1.PK);
			var transferLineInfo2 = response.Docket.Lines.Single(l => l.PK == transferLine2.PK);
			AssertEquals("PLT-456", transferLineInfo1.PalletID);
			AssertEquals("PLT-456", transferLineInfo2.PalletID);
			AssertEquals("Original Pick Line Units should be unchanged.", 10m, originalPickLine.WZ_Units);
			AssertEquals("Original Pick Line should point to Pallet from Transfer.", "PLT-123", originalPickLine.InventoryLine.WE_PalletID);

			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123", "PLT-456" },
				response.Docket.PalletsToTransferCompletely);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_DoesNotSwapCommittedPickLinesIfAvailableStockIsEnough

		public void TestPickAnotherPalletIDToTransfer_DoesNotSwapCommittedPickLinesIfAvailableStockIsEnough()
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
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, location1, "PLT-456");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			helper.CreatePickNew(order);
			var originalPickLine = order.Lines.Single().PickLines.Single();
			AssertEquals("Precondition", "PLT-456", originalPickLine.InventoryLine.WE_PalletID);

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo = response.Docket.Lines.Single();
			AssertEquals("PLT-456", transferLineInfo.PalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
			AssertEquals("Original Pick Line should be unchanged.", 10m, originalPickLine.WZ_Units);
			AssertEquals("Original Pick Line should be unchanged.", "PLT-456", originalPickLine.InventoryLine.WE_PalletID);

			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123" },
				response.Docket.PalletsToTransferCompletely);
		}

		#endregion

		#region TestPickAnotherPalletIDToTransfer_DoesNotSwapPickLinesCommittedToNonOrder

		public void TestPickAnotherPalletIDToTransfer_DoesNotSwapPickLinesCommittedToNonOrder()
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
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, location1, "PLT-456");
			webService.Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "PLT-123", "", "");
			transfer.RunPreSaveValidation();
			webService.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			helper.CreatePickNew(order);
			var originalPickLine = order.Lines.Single().PickLines.Single();
			AssertEquals("Precondition", "PLT-456", originalPickLine.InventoryLine.WE_PalletID);

			var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, "A-1", "PLT-456");
			adjustment.RunPreSaveValidation();
			webService.Factory.Save();

			var response = webService.PickAnotherPalletIDToTransfer(transfer.PK.ToGuid(), "PLT-123", "PLT-456");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);

			var transferLineInfo = response.Docket.Lines.Single();
			AssertEquals("PLT-456", transferLineInfo.PalletID);
			AssertEquals(transferLine.PK, transferLineInfo.PK);
			AssertEquals("Original Pick Line should have same quantity.", 10m, originalPickLine.WZ_Units);
			AssertEquals("Original Pick Line point to the Pallet the Transfer was pointing to.", "PLT-123", originalPickLine.InventoryLine.WE_PalletID);

			AssertContainsExactElementsInAnyOrder(new[] { "PLT-123" },
				response.Docket.PalletsToTransferCompletely);
		}

		#endregion
	}
}
