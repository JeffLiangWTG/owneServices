using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsTransferLineValidationUSTest : WhsTransferLineValidationTest
	{
		#region TestCheckLocationStringLocationIsNotTSAKnownError

		public void TestCheckLocationStringLocationIsNotKnownByTSAError()
		{
			string errorMessage = "This location is not known by TSA.";
			AssertNotNull("Precondition: Docket should be set", DocketLine.Docket);
			AssertNotNull("Precondition: Client should be set", DocketLine.Docket.Client);
			AssertNotNull("Precondition: Warehouse should be set", DocketLine.Docket.Warehouse);
			WhsRow row = Helper.CreateRow(DocketLine.Docket.Warehouse, "ROW");
			WhsLocation location = row.Locations.AddNew();
			DocketLine.WE_WL = location.PK;
			Helper.SetWarehouseTSAStatus(DocketLine.Docket.Warehouse, Environment.CodeLists.US.TSAStatus.Codes.Known);
			Helper.SetOrgAddressTSAStatus(DocketLine.Docket.Client.MainAddress, Environment.CodeLists.US.TSAStatus.Codes.Known);
			location.WLV_ApprovedKnownLocation = Environment.CodeLists.US.TSAStatus.Codes.Unknown;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.LocationStringInfo, errorMessage);

			location.WLV_ApprovedKnownLocation = Environment.CodeLists.US.TSAStatus.Codes.Known;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			location.WLV_ApprovedKnownLocation = Environment.CodeLists.US.TSAStatus.Codes.Unknown;
			Helper.SetOrgAddressTSAStatus(DocketLine.Docket.Client.MainAddress, Environment.CodeLists.US.TSAStatus.Codes.Unknown);
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			Helper.SetOrgAddressTSAStatus(DocketLine.Docket.Client.MainAddress, Environment.CodeLists.US.TSAStatus.Codes.Known);
			Helper.SetWarehouseTSAStatus(DocketLine.Docket.Warehouse, Environment.CodeLists.US.TSAStatus.Codes.Unknown);
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			DocketLine.Docket.WD_WW_Whs = ZGuid.Empty;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			DocketLine.Docket.WD_WW_Whs = row.Warehouse.PK;
			DocketLine.WE_WL = ZGuid.Empty;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			DocketLine.WE_WL = location.PK;
			DocketLine.WE_TransactionQuantity = 0m;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			DocketLine.WE_TransactionQuantity = -10m;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			DocketLine.WE_TransactionQuantity = 10m;
			DocketLine.WE_WD = ZGuid.Empty;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);
		}

		#endregion

		#region TestCheckLocationStringLocationIsNotTSAUnknownError

		public void TestCheckLocationStringLocationIsKnownByTSAError()
		{
			string errorMessage = "This location is known by TSA.";
			AssertNotNull("Precondition: Docket should be set", DocketLine.Docket);
			AssertNotNull("Precondition: Client should be set", DocketLine.Docket.Client);
			AssertNotNull("Precondition: Warehouse should be set", DocketLine.Docket.Warehouse);
			WhsRow row = Helper.CreateRow(DocketLine.Docket.Warehouse, "ROW");
			WhsLocation location = row.Locations.AddNew();
			DocketLine.WE_TransactionQuantity = 10m;
			DocketLine.WE_WL = location.PK;
			Helper.SetWarehouseTSAStatus(DocketLine.Docket.Warehouse, Environment.CodeLists.US.TSAStatus.Codes.Known);
			Helper.SetOrgAddressTSAStatus(DocketLine.Docket.Client.MainAddress, Environment.CodeLists.US.TSAStatus.Codes.Unknown);
			location.WLV_ApprovedKnownLocation = Environment.CodeLists.US.TSAStatus.Codes.Known;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.LocationStringInfo, errorMessage);

			location.WLV_ApprovedKnownLocation = Environment.CodeLists.US.TSAStatus.Codes.Unknown;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			location.WLV_ApprovedKnownLocation = Environment.CodeLists.US.TSAStatus.Codes.Known;
			Helper.SetOrgAddressTSAStatus(DocketLine.Docket.Client.MainAddress, Environment.CodeLists.US.TSAStatus.Codes.Known);
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			Helper.SetOrgAddressTSAStatus(DocketLine.Docket.Client.MainAddress, Environment.CodeLists.US.TSAStatus.Codes.Unknown);
			Helper.SetWarehouseTSAStatus(DocketLine.Docket.Warehouse, Environment.CodeLists.US.TSAStatus.Codes.Unknown);
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			DocketLine.Docket.WD_WW_Whs = ZGuid.Empty;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			DocketLine.Docket.WD_WW_Whs = row.Warehouse.PK;
			DocketLine.WE_WL = ZGuid.Empty;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			DocketLine.WE_WL = location.PK;
			DocketLine.WE_TransactionQuantity = 0m;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			DocketLine.WE_TransactionQuantity = -10m;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);

			DocketLine.WE_TransactionQuantity = 10m;
			DocketLine.WE_WD = ZGuid.Empty;
			DocketLine.Validation.ValidateAll();
			AssertNoError(DocketLine.LocationStringInfo, errorMessage);
		}

		#endregion

		#region TestCheckWE_PerPackageQty_MustBeEnteredIfPackageGroupIDIsEntered

		public void TestCheckWE_PerPackageQty_MustBeEnteredIfPackageGroupIDIsEntered()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var expectedErrorMessage = "Per Group Quantity must be specified if Package Group ID is specified.";

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"), "", "ABC");
			AssertHasError(transferLine1.WE_PerPackageQtyInfo, expectedErrorMessage);

			transferLine1.WE_PerPackageQty = 5m;
			AssertNoError(transferLine1.WE_PerPackageQtyInfo, expectedErrorMessage);

			transferLine1.WE_PerPackageQty = 0m;
			AssertHasError(transferLine1.WE_PerPackageQtyInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckQtyToMoveIncludingMatchingLines

		#region TestCheckQtyToMoveIncludingMatchingLines_WhenUsingPackageGroupID

		public void TestCheckQtyToMoveIncludingMatchingLines_WhenUsingPackageGroupID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var location = data.Whs1.FindLocation("A-1-1");
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive1.WD_DocketSubType = "CUS";
			receive2.WD_DocketSubType = "CUS";

			var expectedDevisableErrorMessage = "Units to transfer must be divisible by Per Group Quantity.";

			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location.PK, "123-1", "ABC", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 20m, location.PK, "123-2", "ABC", 10m);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 5m, location.PK, "", "123-1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location.PK, "123-1", "XYZ", 2m);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 40m, location.PK, "456-1", "DEF", 10m);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 12m, location.PK, "456-2", "DEF", 3m);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2.PK, 15m, location.PK, "", "456-3");

			receive1.FinaliseDocketWithoutUserConfirmation();
			receive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 4m, location, data.Whs1.FindLocation("A-1-2"), "123-1", "XYZ");
			AssertEquals("Precondition", 0m, transferLine1.WE_PerPackageQty);

			transferLine1.RunPreSaveValidation(); // commits inventory
			AssertEquals(2m, transferLine1.WE_PerPackageQty); // committing inventory should set Per Package Qty
			AssertEquals(4m, transferLine1.GetQtyCommittedToThisLine());

			transferLine1.WE_PackageGroupId = "ABC";
			AssertEquals("Precondition - Should not change value until we commit", 2m, transferLine1.WE_PerPackageQty);

			transferLine1.RunPreSaveValidation(); // commits inventory
			AssertEquals(5m, transferLine1.WE_PerPackageQty); // committing inventory should set Per Package Qty
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedDevisableErrorMessage);
			AssertEquals(4m, transferLine1.GetQtyCommittedToThisLine());

			transferLine1.QtyToMoveIncludingMatchingLines = 5m;
			AssertNoError(transferLine1.WE_TransactionQuantityInfo, expectedDevisableErrorMessage);

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 6m, location, data.Whs1.FindLocation("A-2-1"), "123-1");
			transferLine2.RunPreSaveValidation(); // commits inventory
			AssertHasError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, @"Attempted to transfer 6 Units, but only 5 Units are available for transfer out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the transfer line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to transfer.
If you are trying to transfer stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to transfer stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.
If you are trying to transfer stock with a Package Group ID you must enter it exactly. Blank Package Group IDs will only match to inventory with blank Package Group IDs.");

			transferLine2.QtyToMoveIncludingMatchingLines = 5m;
			AssertNoErrors(transferLine2.QtyToMoveIncludingMatchingLinesInfo);

			transferLine2.RunPreSaveValidation(); // re-run the validation with commit
			AssertNoErrors(transferLine2.QtyToMoveIncludingMatchingLinesInfo);

			var expectedFullPackagesPickedErrorMessage = "Only full packages (by Package Group ID) can be picked from source location.";
			transfer.ValidateAndFinaliseDocketLines(new[] { transferLine1 }, confirmFinalise: false);
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedFullPackagesPickedErrorMessage);

			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, 20m, location, data.Whs1.FindLocation("A-1-2"), "123-2", "ABC");
			transferLine3.RunPreSaveValidation(); // commits inventory
			AssertHasError(transferLine3.QtyToMoveIncludingMatchingLinesInfo, expectedFullPackagesPickedErrorMessage);
			AssertEquals(20m, transferLine3.GetQtyCommittedToThisLine());

			transfer.ValidateAndFinaliseDocketLines(new[] { transferLine1, transferLine3 }, confirmFinalise: false);
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedFullPackagesPickedErrorMessage);
			AssertHasError(transferLine3.QtyToMoveIncludingMatchingLinesInfo, expectedFullPackagesPickedErrorMessage);

			// Uncommit all inventory
			transferLine1.PickLines.DeleteAll();
			transferLine1.WE_PerPackageQty = 0m;
			transferLine3.PickLines.DeleteAll();
			transferLine3.WE_PerPackageQty = 0m;

			// finalising the transfer should fail gracefully
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedFullPackagesPickedErrorMessage);
			AssertHasError(transferLine3.QtyToMoveIncludingMatchingLinesInfo, expectedFullPackagesPickedErrorMessage);

			transferLine3.QtyToMoveIncludingMatchingLines = 10m;
			AssertNoErrors(transferLine3.QtyToMoveIncludingMatchingLinesInfo);

			var expectedFullPackagesPutawayErrorMessage = "Only full packages (by Package Group ID) can be putaway into destination location.";
			transferLine3.WE_WL = data.Whs1.FindLocation("A-2-2").PK;
			transfer.ValidateAndFinaliseDocketLines(new[] { transferLine1, transferLine3 }, confirmFinalise: false);
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedFullPackagesPutawayErrorMessage);
			AssertHasError(transferLine3.QtyToMoveIncludingMatchingLinesInfo, expectedFullPackagesPutawayErrorMessage);
			AssertEquals(10m, transferLine3.GetQtyCommittedToThisLine());

			transferLine3.WE_WL = data.Whs1.FindLocation("A-1-2").PK;
			var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location, data.Whs1.FindLocation("A-2-2"), "123-1", "ABC");
			var transferLine5 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, location, data.Whs1.FindLocation("A-2-2"), "123-2", "ABC");
			transferLine4.RunPreSaveValidation(); // to committ items and set PerPackageQty.
			transferLine5.RunPreSaveValidation(); // to committ items and set PerPackageQty.
			Factory.Save();

			// Do this in another factory so I can test more things later on
			var transferInOtherFactory1 = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			var transferLine1InOtherFactory1 = transferInOtherFactory1.Factory.Load<WhsTransferLine>(transferLine1.PK);
			var transferLine3InOtherFactory1 = transferInOtherFactory1.Factory.Load<WhsTransferLine>(transferLine3.PK);

			transferInOtherFactory1.ValidateAndFinaliseDocketLines(new[] { transferLine1InOtherFactory1, transferLine3InOtherFactory1 }, confirmFinalise: false);
			AssertEquals(true, transferLine1InOtherFactory1.IsFinalised);
			AssertEquals(true, transferLine3InOtherFactory1.IsFinalised);

			transferInOtherFactory1.RunPreSaveValidation();
			AssertNoErrors(transferInOtherFactory1);

			// Do this in another factory so I can test more things later on
			var transferInOtherFactory2 = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			var transferLine1InOtherFactory2 = transferInOtherFactory2.Factory.Load<WhsTransferLine>(transferLine1.PK);
			var transferLine3InOtherFactory2 = transferInOtherFactory2.Factory.Load<WhsTransferLine>(transferLine3.PK);
			var transferLine4InOtherFactory2 = transferInOtherFactory2.Factory.Load<WhsTransferLine>(transferLine4.PK);

			transferInOtherFactory2.ValidateAndFinaliseDocketLines(new[] { transferLine1InOtherFactory2, transferLine3InOtherFactory2, transferLine4InOtherFactory2 }, confirmFinalise: false);
			AssertHasError(transferLine1InOtherFactory2.QtyToMoveIncludingMatchingLinesInfo, expectedFullPackagesPickedErrorMessage);
			AssertHasError(transferLine3InOtherFactory2.QtyToMoveIncludingMatchingLinesInfo, expectedFullPackagesPickedErrorMessage);
			AssertHasError(transferLine4InOtherFactory2.QtyToMoveIncludingMatchingLinesInfo, expectedFullPackagesPickedErrorMessage);
			AssertEquals(false, transferLine1InOtherFactory2.IsFinalised);
			AssertEquals(false, transferLine3InOtherFactory2.IsFinalised);
			AssertEquals(false, transferLine4InOtherFactory2.IsFinalised);

			// Transfer all packages at once
			transfer.ValidateAndFinaliseDocketLines(new[] { transferLine1, transferLine3, transferLine4, transferLine5 }, confirmFinalise: false);
			AssertEquals(true, transferLine1.IsFinalised);
			AssertEquals(true, transferLine3.IsFinalised);
			AssertEquals(true, transferLine4.IsFinalised);
			AssertEquals(true, transferLine5.IsFinalised);

			transfer.RunPreSaveValidation();
			AssertNoErrors(transfer);

			// Do this in another factory so I can test more things later on
			var transferInOtherFactory3 = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			transferInOtherFactory3.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Make sure finalising the whole docket works.", true, transferInOtherFactory3.IsFinalised);

			transferInOtherFactory3.RunPreSaveValidation();
			AssertNoErrors(transferInOtherFactory3);
		}

		#endregion

		#region TestCheckQtyToMoveIncludingMatchingLines_DoesNotDivideByZero

		public void TestCheckQtyToMoveIncludingMatchingLines_DoesNotDivideByZero()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var location = data.Whs1.FindLocation("A-1");
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = "CUS";
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location.PK, "123-1", "ABC", 1m);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location, data.Whs1.FindLocation("A-2"), "123-1", "ABC");
			transferLine1.RunPreSaveValidation(); // commit inventory
			AssertEquals("Precondition", 1m, transferLine1.WE_PerPackageQty);
			AssertEquals("Precondition", 5m, transferLine1.GetQtyCommittedToThisLine());

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location, data.Whs1.FindLocation("A-2"), "123-1", "ABC");
			transferLine2.RunPreSaveValidation(); // commit inventory
			AssertEquals("Precondition", 1m, transferLine2.WE_PerPackageQty);
			AssertEquals("Precondition", 5m, transferLine2.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePicked

		public void TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			locationA1.WLV_WA_PutawayArea = bondedArea.PK;
			locationA2.WLV_WA_PutawayArea = bondedArea.PK;
			locationA3.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var expectedErrorMessage = "Only full packages (by Package Group ID) can be picked from source location.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1.PK, "KEY-1", "123", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1.PK, "KEY-1", "123", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA2.PK, "KEY-1", "123", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA2.PK, "KEY-1", "123", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA2.PK, "KEY-1", "123", 5m).WI_PalletID = "PLT-1";
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA2.PK, "KEY-1", "123", 5m).WI_PalletID = "PLT-1";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locationA1, locationA3, "KEY-1", "123");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 5m, locationA1, locationA3, "KEY-1", "123");
			transferLine1.WE_PerPackageQty = 5m;
			transferLine2.WE_PerPackageQty = 5m;
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertNoError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertNoError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			// single location
			transferLine1.QtyToMoveIncludingMatchingLines = 10m;
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertHasError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			transferLine2.QtyToMoveIncludingMatchingLines = 10m;
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertNoError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertNoError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			// multiple locations
			transferLine1.WE_WL_TransferFrom = locationA2.PK;
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertHasError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			transferLine2.WE_WL_TransferFrom = locationA2.PK;
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertNoError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertNoError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			transferLine1.QtyToMoveIncludingMatchingLines = 5m;
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertHasError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			transferLine2.QtyToMoveIncludingMatchingLines = 5m;
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertNoError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertNoError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			// with Pallet ID
			transferLine1.WE_TransferFromPalletId = "PLT-1";
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertHasError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			transferLine2.WE_TransferFromPalletId = "PLT-1";
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertNoError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertNoError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			transferLine1.QtyToMoveIncludingMatchingLines = 10m;
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertHasError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			transferLine2.QtyToMoveIncludingMatchingLines = 10m;
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertNoError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertNoError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
		}

		public void TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePicked_WithPartAttributes_Attrib1()
		{
			TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePicked_WithPartAttributesCore(AttributeNumber.One, (docketLine, attrValue) => docketLine.WE_PartAttrib1 = attrValue);
		}

		public void TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePicked_WithPartAttributes_Attrib2()
		{
			TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePicked_WithPartAttributesCore(AttributeNumber.Two, (docketLine, attrValue) => docketLine.WE_PartAttrib2 = attrValue);
		}

		public void TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePicked_WithPartAttributes_Attrib3()
		{
			TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePicked_WithPartAttributesCore(AttributeNumber.Three, (docketLine, attrValue) => docketLine.WE_PartAttrib3 = attrValue);
		}

		public void TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePicked_WithPartAttributes_SerialNumber()
		{
			TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePicked_WithPartAttributesCore(AttributeNumber.Serial, (docketLine, attrValue) => docketLine.WE_SerialNumber = attrValue);
		}

		void TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePicked_WithPartAttributesCore(AttributeNumber attributeNumber, Action<WhsDocketLine, string> attributeValueSetter)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			locationA1.WLV_WA_PutawayArea = bondedArea.PK;
			locationA2.WLV_WA_PutawayArea = bondedArea.PK;
			Helper.SetClientAttributeType(data.Org1, attributeNumber, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			Factory.Save();

			var expectedErrorMessage = "Only full packages (by Package Group ID) can be picked from source location.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1.PK, "KEY-1", "123", 1m);
			attributeValueSetter(inventory.InDocketLine, "VAL1");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, locationA1, locationA2, "KEY-1", "123");
			transferLine.WE_PerPackageQty = 1m;
			attributeValueSetter(transferLine, "VAL1");
			transferLine.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertNoError(transferLine.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			attributeValueSetter(transferLine, "VAL2");
			transferLine.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertHasError(transferLine.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePutaway

		public void TestCheckQtyToMoveIncludingMatchingLines_CheckOnlyFullPackagesArePutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			locationA1.WLV_WA_PutawayArea = bondedArea.PK;
			locationA2.WLV_WA_PutawayArea = bondedArea.PK;
			locationA3.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var expectedErrorMessage = "Only full packages (by Package Group ID) can be putaway into destination location.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1.PK, "KEY-1", "123", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1.PK, "KEY-1", "123", 5m);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locationA1, locationA2, "KEY-1", "123");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 5m, locationA1, locationA2, "KEY-1", "123");
			transferLine1.WE_PerPackageQty = 5m;
			transferLine2.WE_PerPackageQty = 5m;
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertNoError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertNoError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			// multiple locations
			transferLine1.WE_WL = locationA3.PK;
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertHasError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			transferLine2.WE_WL = locationA3.PK;
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertNoError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertNoError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			// with Pallet ID
			transferLine1.WE_PalletID = "PLT-2";
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertHasError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertHasError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			transferLine2.WE_PalletID = "PLT-2";
			transferLine1.Validation.ValidateQtyToMoveIncludingMatchingLines();
			transferLine2.Validation.ValidateQtyToMoveIncludingMatchingLines();
			AssertNoError(transferLine1.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
			AssertNoError(transferLine2.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
		}

		#endregion

		#endregion

		#region TestCheckHeldCodeToChangeTo_CannotChangeStatusIfInventoryIsInPackageGroup

		public void TestCheckHeldCodeToChangeTo_CannotChangeStatusIfInventoryIsInPackageGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A-1-1");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1-1").PK, "123-1", "ABC", 2m);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.FindLocation("A-1-1"), data.Whs1.FindLocation("A-2-1"), "123-1", "ABC");
			transferLine.WE_PerPackageQty = 2m;
			transfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(transfer);
			AssertNoErrors("Precondition", transferLine.HeldCodeToChangeToInfo);

			transferLine.IsInventoryEditForm = true;
			transferLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			AssertHasError(transferLine.HeldCodeToChangeToInfo, "Cannot change the Hold Code of Inventory in a Package Group.");
		}

		#endregion

		#region TestCheckHeldCodeChangeQuantity_ChangeQuantityMustBeDivisibleByPerPackageQty

		public void TestCheckHeldCodeChangeQuantity_ChangeQuantityMustBeDivisibleByPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A-1-1");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1-1").PK, "123-1", "ABC", 2m);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.FindLocation("A-1-1"), data.Whs1.FindLocation("A-2-1"), "123-1", "ABC");
			transferLine.WE_PerPackageQty = 2m;
			transfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(transfer);
			AssertNoErrors("Precondition", transferLine.HeldCodeToChangeToInfo);

			transferLine.IsInventoryEditForm = true;
			transferLine.HeldCodeChangeQuantity = 3m;
			AssertHasError(transferLine.HeldCodeChangeQuantityInfo, "Quantity must be divisible by Per Group Quantity.");

			transferLine.HeldCodeChangeQuantity = 4m;
			AssertNoErrors(transferLine.HeldCodeChangeQuantityInfo);
		}

		#endregion

		#region Implementation

		protected override Environment.Business.Testing.WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new US.Testing.WhsTestHelperFunctionsUS(Factory);
		}

		protected override WhsTransfer GetNewDocket()
		{
			return Helper.CreateWhsTransfer(Helper.CreateClient(), Helper.CreateWarehouse("WHSUS"));
		}

		protected new US.Testing.WhsTestHelperFunctionsUS Helper
		{
			get { return (US.Testing.WhsTestHelperFunctionsUS)base.Helper; }
		}

		#endregion
	}
}
