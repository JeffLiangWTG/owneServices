using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;
using WhsDocketLineDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsTransferLine))]
	public class WhsTransferLineTest : WhsDocketLineTestCase<WhsTransferLine, WhsTransfer>
	{
		#region Related Business Objects

		#region TestPickLines

		public void TestPickLines()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();

			var pickLine1 = Factory.New<WhsPickLine>();
			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine1.WZ_WE_TransactionLine = transferLine.PK;
			pickLine2.WZ_WE_TransactionLine = transferLine.PK;

			AssertEquals(2, transferLine.PickLines.Count);
			AssertCollectionContains(pickLine1, transferLine.PickLines);
			AssertCollectionContains(pickLine2, transferLine.PickLines);
		}

		#endregion

		#region TestChildTransferLine

		public void TestChildTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transferLine1 = Factory.New<WhsTransferLine>();
			var transferLine2 = Factory.New<WhsTransferLine>();
			var transferLine3 = Factory.New<WhsTransferLine>();
			AssertNull("Precondition - transfer should have no child.", transferLine1.ChildTransferLine);
			AssertNull("Precondition - transfer should have no child.", transferLine2.ChildTransferLine);
			AssertNull("Precondition - transfer should have no child.", transferLine3.ChildTransferLine);

			transferLine2.WE_WE_ParentDocketLine = transferLine1.PK;
			transferLine3.WE_WE_ParentDocketLine = transferLine2.PK;
			AssertEquals(transferLine2, transferLine1.ChildTransferLine);
			AssertEquals(transferLine3, transferLine2.ChildTransferLine);
			AssertNull(transferLine3.ChildTransferLine);
		}

		public void TestChildTransferLine_DoesNotReturnStatusChangedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			AssertEquals("Precondition", 10m, transferLine.WE_StockOnHand);
			AssertNull("Precondition", transferLine.ChildTransferLine);

			transferLine.HeldCodeChangeQuantity = 3m;
			transferLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			transferLine.ChangeInventoryHeldCode(true);
			AssertEquals("Precondition - ensure status change ocured.", 7m, transferLine.WE_StockOnHand);
			AssertNull("Status changed inventory should *NOT* be identified as child transfer lines.", transferLine.ChildTransferLine);
		}

		#endregion

		#region TestMatchingLines

		public void TestMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2m);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", "");
			Helper.CreateWhsPickLine(line, receive1.Inventory[0], 10m);
			AssertEquals(0, line.MatchingLines.Count);
			AssertEquals(true, line.IsRegisteredEditableChildObject(line.MatchingLines));

			var line2 = Helper.CreateMatchingLine(line, 2m);
			Helper.CreateWhsPickLine(line2, receive2.Inventory[0], 2m);
			AssertEquals(line2, line.MatchingLines.Single());

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var transferLineInOtherFacotry = otherFactory.Load<WhsTransferLine>(line.PK);
			AssertEquals("Matching lines should be loaded automatically.", 1, transferLineInOtherFacotry.MatchingLines.Count);
		}

		#endregion

		#region TestMatchingParentLine

		public void TestMatchingParentLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "", "");
			var matchingLine1 = Helper.CreateMatchingLine(transferLine, 5m);
			var matchingLine2 = Helper.CreateMatchingLine(transferLine, 5m);
			AssertNull("Main line should not have Matching Parent.", transferLine.MatchingParentLine);
			AssertEquals("All matching lines should point to the same Matching Parent.", transferLine, matchingLine1.MatchingParentLine);
			AssertEquals("All matching lines should point to the same Matching Parent.", transferLine, matchingLine2.MatchingParentLine);
		}

		#endregion

		public void TestInventoryUpdatesWE_WL_TransferFrom()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 12m, "", "");

			AssertEquals("Transfer line source location has not been set up.", ZGuid.Empty, transferLine.WE_WL_TransferFrom);

			SetInventoryDataForUseChosenInventoryRowMethod(data.Line111);
			transferLine.UseChosenInventoryRowFindAttributes(data.Line111);

			AssertEquals("Transfer line source location gets updated.", data.Line111.Location.PK, transferLine.WE_WL_TransferFrom);
		}

		public void TestCustomsData_ReadOnly()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			AssertEquals("Non Outbound Dock Door Transfer should not have ReadOnly Customs Data.", false, transferLine.CustomsData.ReadOnly);

			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			transferLine.CustomsData.Delete();
			AssertEquals("Outbound Dock Door Transfer should have ReadOnly Customs Data.", true, transferLine.CustomsData.ReadOnly);
		}

		#endregion

		#region Business Object Overrides

		#region TestSetDefaultValues

		protected override string ExpectedDefaultInventoryStatus
		{
			get { return InventoryStatus.Codes.Available; }
		}

		protected override void TestSetDefaultValuesCore(WhsDocketLine docketLine)
		{
			AssertEquals("WE_DocketLineStatus", DocketLineStatus.Codes.Entered, docketLine.WE_DocketLineStatus);
		}

		#endregion

		#region TestIsInventoryLine

		protected override void TestIsInventoryLineCore()
		{
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			AssertEquals(true, docketLine.IsInventoryLine);

			docketLine.Docket.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			AssertEquals("Only interwarehouse source transfer do not create stock.", false, docketLine.IsInventoryLine);

			docketLine.Docket.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			AssertEquals(true, docketLine.IsInventoryLine);

			docketLine.Docket.WD_DocketSubType = TransferType.Codes.Internal;
			AssertEquals(true, docketLine.IsInventoryLine);
		}

		#endregion

		#region TestSaveAndDeleteBusinessObject

		public override void TestSaveAndDeleteBusinessObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10, inventory.Location, inventory.Location);
			transferLine.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			transferLine.Delete();
			Factory.Save();
		}

		#endregion

		#endregion

		#region Validation

		protected override Type GetExpectedValidationType()
		{
			return typeof(WhsTransferLineValidation);
		}
		#region TestValidationAfterFinalisation

		#region TestValidationAfterFinalisation_ValidationEnabledCorrectly

		public void TestValidationAfterFinalisation_ValidationEnabledCorrectly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation, destinationLocation);

			AssertEquals("Precondition: Property is NOT readonly", false, transferLine.WE_LineCommentInfo.ReadOnly);
			AssertEquals("Validation for NOT readonly property should be enabled", true, transferLine.IsValidationEnabled(transferLine.WE_LineCommentInfo));

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			CombineAssertions(() =>
			{
				foreach (var propertyInfo in transferLine.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(info => info.ReadOnly))
				{
					AssertEquals(string.Format(CultureInfo.InvariantCulture, "Precondition: Property is readonly - {0}", propertyInfo.Description), true, propertyInfo.ReadOnly);
					AssertEquals(string.Format(CultureInfo.InvariantCulture, "Validation for readonly property should be disabled - {0}", propertyInfo.Description), false, transferLine.IsValidationEnabled(propertyInfo));
				}
			});
		}

		#endregion

		#region TestValidationAfterFinalisation_FinalisedInvalidValuesShouldNotCauseErrors

		public void TestValidationAfterFinalisation_FinalisedInvalidValuesShouldNotCauseErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation, destinationLocation);

			// check for error before finalisation
			transferLine.WE_TransactionQuantity = 2400m;
			var expectedError =
@"Attempted to transfer 2400 Units, but only 20 Units are available for transfer out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the transfer line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to transfer.
If you are trying to transfer stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to transfer stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.";

			transferLine.RunPreSaveValidation();
			AssertHasError("TransferLine should have an error.", transferLine.QtyToMoveIncludingMatchingLinesInfo, expectedError);

			// reset value to valid amount
			transferLine.WE_TransactionQuantity = 5m;
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);

			transferLine.WE_TransactionQuantity = 2400m;
			transferLine.RunPreSaveValidation();
			AssertNoErrors(string.Format(CultureInfo.InvariantCulture, "TransferLine - {0} - should have no errors.", transferLine.QtyToMoveIncludingMatchingLinesInfo.Description), transferLine.QtyToMoveIncludingMatchingLinesInfo);
		}

		#endregion

		#endregion

		#region TestRunPreSaveValidation

		#region TestRunPreSaveValidation_ValidateStockAvailability

		public void TestRunPreSaveValidation_ValidateStockAvailability()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[0]);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, locations[0], "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 12m, locations[0].ToLocationString(), "");
			AssertNoErrors("Precondition - transferLine should have no errors.", transferLine.WE_TransactionQuantityInfo);

			var expectedErrorMessage =
@"Attempted to transfer 12 Units, but only 10 Units are available for transfer out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the transfer line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to transfer.
If you are trying to transfer stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to transfer stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.";

			transferLine.RunPreSaveValidation();
			AssertHasError(transferLine.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.RunPreSaveValidation();
			AssertNoErrors(transferLine.QtyToMoveIncludingMatchingLinesInfo);
		}

		#endregion

		#region TestRunPreSaveValidation_CommitsStock

		public void TestRunPreSaveValidation_CommitsStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locations[0], "");
			var inventory = receive.Inventory[0];

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locations[0].ToLocationString(), "");

			AssertEquals("Precondition - ensure no stock is not committed.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is not committed.", 0m, transferLine.GetQtyCommittedToThisLine());

			var expectedErrorMessage =
@"Attempted to transfer 100 Units, but only 50 Units are available for transfer out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the transfer line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to transfer.
If you are trying to transfer stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to transfer stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.";

			transferLine.RunPreSaveValidation();
			AssertEquals("When transfer is saved it should commit stock.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("When transfer is saved it should commit stock.", 10m, transferLine.GetQtyCommittedToThisLine());
			AssertNoError("Transfer line should have no error if there are enought stock to transfer.", transferLine.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);

			transferLine.WE_TransactionQuantity = 100m;
			transferLine.RunPreSaveValidation();
			AssertEquals("When transfer is saved it should commit stock.", 50m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("When transfer is saved it should commit stock.", 50m, transferLine.GetQtyCommittedToThisLine());
			AssertHasError("Transfer line should have error if there are not enought stock to transfer.", transferLine.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
		}

		#endregion

		#region TestRunPreSaveValidation_ValidateLocationIsOnDocketWarehouse

		public void TestRunPreSaveValidation_ValidateLocationIsOnDocketWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs2, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "", "");
			transferLine.WE_WL = data.Whs1.FindLocation("A-1").PK;

			transferLine.RunPreSaveValidation();
			AssertHasErrors(transferLine.LocationStringInfo);
		}

		#endregion

		#endregion

		#endregion

		#region Lookups

		protected override Type GetExpectedLookupsType()
		{
			return typeof(WhsTransferLineLookups);
		}

		public void TestGetNewLookupsUS()
		{
			var helper = new US.Testing.WhsTestHelperFunctionsUS(Factory);
			var whs = helper.CreateWarehouse("WHS");
			var docket = helper.CreateWhsTransfer(helper.CreateClient(), whs);
			var docketLine = GetNewBusinessObject();
			docketLine.WE_WD = docket.PK;
			AssertEquals(typeof(WhsTransferLineValidationUS), docketLine.Validation.GetType());
		}

		#endregion

		#region Cloning

		public void TestSupportsCloneCore()
		{
			Assert(DocketLine.SupportsClone());
		}

		protected override void TestClonePicked(WhsDocketLine docketLine)
		{
			base.TestClonePicked(docketLine);

			var transferLine = docketLine;
			transferLine.PickLines.AddNew().WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Picked Time should not be cloned.", ZDateTimeOffset.Empty, transferLine.Clone<WhsTransferLine>().PickedTime);
			var clone = (WhsTransferLine)docketLine.Clone();
			AssertEquals(DocketLineStatus.Codes.Entered, clone.WE_DocketLineStatus);
		}

		protected override void TestClonePutaway(WhsDocketLine docketLine)
		{
			// do not call base
			var user = Helper.CreateGlbStaff("AAA", "AAA");
			docketLine.WE_GS_NKPutawayBy = user.GS_Code;
			using (new SemaphoreManager(((WhsTransferLine)docketLine).FinaliseDocketLineSemaphore))
			{
				docketLine.WE_PutawayTime = ZDateTimeOffset.Today;
			}
			var clone = (WhsDocketLine)docketLine.Clone();
			AssertEquals("", clone.WE_GS_NKPutawayBy);
			AssertEquals(ZDateTimeOffset.Empty, clone.WE_PutawayTime);
		}

		#region TestClone_QuantitiesIncludingMatchingLines

		public void TestClone_QuantitiesIncludingMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, "A-1", "");
			var matchingLine1 = Helper.CreateMatchingLine(transferLine, 5m);
			var matchingLine2 = Helper.CreateMatchingLine(transferLine, 3m);
			AssertEquals("Precondition", 18m, transferLine.QtyToMoveIncludingMatchingLines);

			var clone = (WhsTransferLine)transferLine.Clone();
			AssertEquals("QtyToMoveIncludingMatchingLines should be the same as original line.", 18m, clone.QtyToMoveIncludingMatchingLines);
			AssertEquals("No matching lines should be created for cloned line.", 0, clone.MatchingLines.Count);
		}

		protected override void TestCloneWE_DocketLineStatus(WhsDocketLine docketLine)
		{
			docketLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			var clone = (WhsDocketLine)docketLine.Clone();
			AssertEquals("Cloned WE_FinalisedDate should be ENT", DocketLineStatus.Codes.Entered, clone.WE_DocketLineStatus);
		}

		#endregion

		#region TestCloneWE_WE_OriginalDocketLineForRating

		public void TestCloneWE_WE_OriginalDocketLineForRating()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var transferNormal = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR1");
			var transferLineNormal = Helper.CreateWhsTransferLine(transferNormal, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			var clonedTransferLineNormal = (WhsTransferLine)transferLineNormal.Clone();
			transferNormal.Lines.Add(clonedTransferLineNormal);
			transferLineNormal.FinaliseDocketLine();
			clonedTransferLineNormal.FinaliseDocketLine();
			AssertEquals("For transfers WE_WE_OriginalDocketLineForRating should be cloned as well.", clonedTransferLineNormal.WE_WE_OriginalDocketLineForRating, transferLineNormal.WE_WE_OriginalDocketLineForRating);
			AssertNotEquals("WE_WE_OriginalDocketLineForRating should point to original stock.", clonedTransferLineNormal.WE_WE_OriginalDocketLineForRating, clonedTransferLineNormal.PK);

			var whs2 = Helper.CreateWarehouse("whs2", "B", 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", data.Part1, 100m);
			Factory.Save();
			var transformIWD = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR-IWD", null, TransferType.Codes.InterWhsDest);
			var transferLineIWD = Helper.CreateWhsTransferLine(transformIWD, data.Part1, 5m, whs2.FindLocation("B-1-1"), data.Whs1.FindLocation("A-1"));
			transferLineIWD.TransferFromWarehousePK = whs2.PK;
			transferLineIWD.TransferFromLocationString = "B-1-1";
			var clonedTransferLineIwsDest = (WhsTransferLine)transferLineIWD.Clone();
			transformIWD.Lines.Add(clonedTransferLineIwsDest);

			transferLineIWD.FinaliseDocketLine();
			clonedTransferLineIwsDest.FinaliseDocketLine();
			AssertNotEquals("For inter-warehouse destination transfers WE_WE_OriginalDocketLineForRating should NOT be cloned.", clonedTransferLineIwsDest.WE_WE_OriginalDocketLineForRating, transferLineIWD.WE_WE_OriginalDocketLineForRating);
			AssertEquals("WE_WE_OriginalDocketLineForRating should equal to PK as inter-warehouse destination transfers create stock.", clonedTransferLineIwsDest.WE_WE_OriginalDocketLineForRating, clonedTransferLineIwsDest.PK);
		}

		#endregion

		#region TestClone_DoesNotCreateExtraInventory

		public void TestCloneFinalisedLine_DoesNotCreateExtraInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, sourceLocation, "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, sourceLocation.WLV_LocationString, destLocation.WLV_LocationString);
			transferLine.FinaliseDocketLine();

			AssertEquals("Pre-condition: TransferLine finalised.", DocketLineStatus.Codes.Finalised, transferLine.WE_DocketLineStatus);
			AssertEquals("Pre-condition: Transfer not finalised.", DocketStatus.Codes.New, transfer.WD_DocketStatus);

			var clonedLine = (WhsTransferLine)transferLine.Clone();
			clonedLine.WE_WD = transferLine.WE_WD;
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertNotEquals("Cloned Line should not be finalised.", DocketLineStatus.Codes.Finalised, clonedLine.WE_DocketLineStatus);
			AssertEquals("Cloned Line should not have available quantity to be picked.", 0m, clonedLine.WE_StockOnHand);
			AssertEquals("Inventory available to pick quantity should be reduced.", 0m, inventory.WI_AvailableToPickQuantity);
		}

		#endregion

		#region TestCloneInTransitLine_DoesNotCreateExtraInventory

		public void TestCloneInTransitLine_DoesNotCreateExtraInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destLocation = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocation, "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, sourceLocation.WLV_LocationString, destLocation.WLV_LocationString);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock should be committed.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition: Stock should be committed.", 20m, transferLine.GetQtyCommittedToThisLine());
			transferLine.PickedTime = ZDateTimeOffset.Today;
			Factory.Save();

			AssertEquals("Precondition: Should have changed Inventory Status.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);

			var clonedLine = (WhsTransferLine)transferLine.Clone();
			clonedLine.WE_WD = transferLine.WE_WD;
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Cloned Line should be Available Status.", InventoryStatus.Codes.Available, clonedLine.WE_OriginalInventoryStatus);
			AssertEquals("Cloned Line should not have available quantity to be picked.", 0m, clonedLine.WE_StockOnHand);
			AssertEquals("Available inventory quantity should be reduced.", 10m, inventory.WI_AvailableToPickQuantity);
		}

		#endregion

		#region TestClone_DoesNotCreateExtraInventory_MatchingLine

		public void TestClone_DoesNotCreateExtraInventory_MatchingLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1-1"), "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1-1"), "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1-1"), "");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1-1"), "");
			receive.FinaliseDocketWithoutUserConfirmation();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 2m, "A-1-1", "A-1-2");

			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Precondition", 2m, transferLine.QtyToMoveIncludingMatchingLines);
			AssertEquals("Precondition", 1, transferLine.MatchingLines.Count);

			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition: TransferLine finalised.", DocketLineStatus.Codes.Finalised, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition: Transfer not finalised.", DocketStatus.Codes.Entered, transfer.WD_DocketStatus);

			var clonedLine = (WhsTransferLine)transferLine.Clone();
			clonedLine.WE_WD = transferLine.WE_WD;
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Cloned Line should be entered.", DocketLineStatus.Codes.Entered, clonedLine.WE_DocketLineStatus);
			AssertEquals("Cloned Line should copy the WE_TransactionQuantity.", 1m, clonedLine.WE_TransactionQuantity);
			AssertEquals("Cloned line should have quantity unit 2m.", 2m, clonedLine.QtyToMoveIncludingMatchingLines);
			AssertEquals("Cloned Line should not have available quantity to be picked.", 0m, clonedLine.WE_StockOnHand);
			AssertEquals("Cloned line should have 1 matching line.", 1, clonedLine.MatchingLines.Count);

			var clonedMatchingLine = clonedLine.MatchingLines[0];
			AssertEquals("Matching line of cloned line should have WE_TransactionQuantity of 1m.", 1m, clonedMatchingLine.WE_TransactionQuantity);
			AssertEquals("Matching line of cloned line should have WE_StockOnHand of 0m.", 0m, clonedMatchingLine.WE_StockOnHand);

			AssertEquals("Available inventory quantity should be reduced.", 0m, inventory1.WI_AvailableToPickQuantity);
			AssertEquals("Available inventory quantity should be reduced.", 0m, inventory2.WI_AvailableToPickQuantity);
			AssertEquals("Available inventory quantity should be reduced.", 0m, inventory3.WI_AvailableToPickQuantity);
			AssertEquals("Available inventory quantity should be reduced.", 0m, inventory4.WI_AvailableToPickQuantity);
		}

		#endregion

		#region TestCloneWE_StockOnHand

		public void TestCloneWE_StockOnHand()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			transferLine.WE_StockOnHand = 3m;
			var clone = (WhsTransferLine)transferLine.Clone();
			AssertEquals("Cloned WE_FinalisedDate should be empty", 0m, clone.WE_StockOnHand);
		}

		#endregion

		#region TestCloneWE_P9_Task

		public void TestCloneWE_P9_Task()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			transferLine.WE_P9_Task = ZGuid.BrettsGuid;

			var clone = (WhsTransferLine)transferLine.Clone();
			AssertEquals("Cloned WE_P9_Task should be empty", ZGuid.Empty, clone.WE_P9_Task);
		}

		#endregion

		#endregion

		#region Find Attributes

		protected override void SetInventoryDataForUseChosenInventoryRowMethod(WhsInventoryView inventory)
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;

			base.SetInventoryDataForUseChosenInventoryRowMethod(inventory);

			inventory.WI_WL = locations[1].PK;
			inventory.WI_PalletID = "PALLETID_1";
			inventory.WI_ArrivalDate = ZDateTimeOffset.Today;
		}

		protected override void AssertUseChosenInventoryRowHasSetProperties(WhsInventoryView expected, WhsDocketLine actual)
		{
			base.AssertUseChosenInventoryRowHasSetProperties(expected, actual);
			AssertEquals("Location: ", expected.WI_WL, actual.WE_WL_TransferFrom);
			AssertEquals("Pallet ID: ", expected.WI_PalletID, actual.WE_PalletID);
			AssertEquals("Arrival Date: ", expected.WI_ArrivalDate, actual.WE_AdjustmentArrivalDate);
		}

		#endregion

		#region Properties

		#region Flags

		#region TestCanUpdateFromInventory_FinalisedTransferLine

		public void TestCanUpdateFromInventory_FinalisedTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			AssertEquals("Can Update Transfer line from inventory if it is not Finalised.", true, transferLine1.CanUpdateFromInventory);

			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			AssertEquals("Cannot Update Transfer line from inventory if it is Finalised.", false, transferLine1.CanUpdateFromInventory);
		}

		#endregion

		#region TestIsFinalised

		protected override void TestIsFinalisedCore(TestDataSimpleEnvironment data)
		{
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, "A-1", "");
			AssertEquals("Precondition", false, transferLine.IsFinalised);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			transferLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals("If main transfer line is finalised and there are no matching lines, result should be true.", true, transferLine.IsFinalised);

			var matchingLine = Helper.CreateMatchingLine(transferLine, 5m);
			AssertEquals("If main transfer line is finalised and there are unfinalised matching lines, result should be false.", false, transferLine.IsFinalised);
			AssertEquals("Matching lines result should be depending on its status.", false, matchingLine.IsFinalised);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Entered; // clean up
			transferLine.WE_FinalisedDate = ZDateTimeOffset.Empty;
			matchingLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			matchingLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals("If main transfer line is not finalised and matching lines are finalised, result should be false.", false, transferLine.IsFinalised);
			AssertEquals("Matching lines result should be depending on its status.", true, matchingLine.IsFinalised);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			transferLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals("If main and matching transfer lines are finalised, result should be true.", true, transferLine.IsFinalised);
			AssertEquals("Matching lines result should be depending on its status.", true, matchingLine.IsFinalised);
		}

		#endregion

		#region TestIsFinalising

		public void TestIsFinalising()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();

			AssertEquals("Precondition", false, transferLine.IsFinalising);

			using (new SemaphoreManager(transfer.FinaliseDocketSemaphore)) // sets Transfers IsFinalising = true;
			{
				AssertEquals(true, transferLine.IsFinalising);
			}
			using (new SemaphoreManager(transfer.FinaliseDocketLineSemaphore)) // sets Transfers IsFinalisingLines = true;
			{
				AssertEquals(true, transferLine.IsFinalising);
			}
			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore)) // sets Transfer Line IsFinalising = true;
			{
				AssertEquals(true, transferLine.IsFinalising);
			}

			AssertEquals(false, transferLine.IsFinalising);

			transferLine.WE_WD = ZGuid.Empty;
			AssertEquals(false, transferLine.IsFinalising);
		}

		#endregion

		#region TestIsChildTransferLine_InterWhsChild

		#region TestIsChildTransferLine_InterWhsChild_Source

		public void TestIsChildTransferLine_InterWhsChild_Source()
		{
			TestIsChildTransferLine_InterWhsChild_Core(parentType: TransferType.Codes.InterWhsDest); // Parent = Dest
		}

		#endregion

		#region TestIsChildTransferLine_InterWhsChild_Destination

		public void TestIsChildTransferLine_InterWhsChild_Destination()
		{
			TestIsChildTransferLine_InterWhsChild_Core(parentType: TransferType.Codes.InterWhsSource); // Parent = Source
		}

		#endregion

		#region TestIsChildTransferLine_InterWhsChild_Core

		void TestIsChildTransferLine_InterWhsChild_Core(string parentType)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, parentType == TransferType.Codes.InterWhsDest ? whs2 : data.Whs1);
			transfer.DocketSubType = parentType;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", parentType == TransferType.Codes.InterWhsDest ? data.Whs1.PK : whs2.PK, "A");
			AssertEquals("Master Transfer Line should *not* be a child line.", false, transferLine.IsChildTransferLine);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertNotNull("Precondition: Child Transfer created.", transferLine.ChildTransferLine);

			var childTransferLine = transferLine.ChildTransferLine;
			AssertEquals("Child Transfer Line IsChildTransferLine.", true, childTransferLine.IsChildTransferLine);

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(childTransferLine);
			AssertEquals("Child Transfer Line IsChildTransferLine.", true, childTransferLine.IsChildTransferLine);
		}

		#endregion

		#endregion

		#region TestIsChildTransferLine_HoldCodeChange

		public void TestIsChildTransferLine_HoldCodeChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "A-2");
			transfer.FinaliseDocket();

			AssertIsFinalisedPrecondition(transferLine);
			AssertEquals("Master Transfer Line should *not* be a child line.", false, transferLine.IsChildTransferLine);
			AssertEquals("Matching Transfer Line should *not* be a child line.", false, transferLine.MatchingLines[0].IsChildTransferLine);

			transferLine.IsInventoryEditForm = true;
			transferLine.HeldCodeChangeQuantity = 5m;
			transferLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			transferLine.ChangeInventoryHeldCode(true);

			var statusChangeLine = Factory.LoadTop1<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, false));
			AssertNotNull("Precondition: HoldCode Changed.", statusChangeLine);
			AssertEquals("Status Change Line should *not* be a child line.", false, statusChangeLine.IsChildTransferLine);
		}

		#endregion

		#region TestIsPicked

		public void TestIsPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventory.LocationString, "");
			AssertEquals("Precondition: Transfer Line should not be picked.", false, transferLine.IsPicked);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Transfer Line should be considered Picked when Picked Time is set.", true, transferLine.IsPicked);
		}

		#endregion

		#region TestIsPutaway

		public void TestIsPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventory.LocationString, "");
			AssertEquals("Precondition: Transfer Line should *not* be putaway.", false, transferLine.IsPutaway);

			var now = ZDateTimeOffset.Now;
			transferLine.PickedTime = now;
			AssertEquals("Precondition: Transfer Line should *not* be putaway.", false, transferLine.IsPutaway);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = now;
			}
			AssertEquals("Precondition: Transfer Line should be putaway.", true, transferLine.IsPutaway);
		}

		#endregion

		#region TestIsValidationEnabled_OnlyForMainTransferLine

		public void TestIsValidationEnabled_OnlyForMainTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: stock is committed.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			AssertEquals("Validation should be enabled for the main transfer line.", true, transferLine.IsValidationEnabled(transferLine.QtyToMoveIncludingMatchingLinesInfo));

			var matchingTransferLine = transferLine.MatchingLines[0];
			AssertEquals("Validation should *not* be enabled for the matching transfer lines.", false, matchingTransferLine.IsValidationEnabled(matchingTransferLine.QtyToMoveIncludingMatchingLinesInfo));
		}

		#endregion

		#region TestIsHeldForTransfer

		public void TestIsHeldForTransfer()
		{
			var transferLine = DocketLine;
			AssertEquals("Precondition", false, transferLine.IsHeldForTransfer);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.HeldForTransfer;
			AssertEquals(true, transferLine.IsHeldForTransfer);
		}

		#endregion

		#region TestIsCrossDockPutaway

		public void TestIsCrossDockPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "A", 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(inventory, 1m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory);
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;

			inventory.ReservedPickLines.ToArray().ForEach(pickLine => pickLine.WZ_WE_InventoryLine = transferLine.PK);

			Assert("Precondition: transferline has reserved pick lines", transferLine.ReservedPickLines.Count > 0);
			Assert("Precondition: transferline is a putaway transfer line", transferLine.IsPutawayTransferLine);

			Assert("Transferline is a crossdock putaway.", transferLine.IsCrossDockPutaway);
		}

		public void TestIsCrossDockPutaway_NotPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var nonPutawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = nonPutawayTransfer.Lines.AddNew();

			Assert("Precondition: transferline is not a putaway transfer line", !transferLine.IsPutawayTransferLine);
			Assert("Transferline is not a crossdock putaway.", !transferLine.IsCrossDockPutaway);
		}

		public void TestIsCrossDockPutaway_NoReserveLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "A", 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(inventory, 1m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory);
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;

			Assert("Precondition: transferline has no reserved pick lines", transferLine.ReservedPickLines.Count == 0);
			Assert("Transferline is not a crossdock putaway.", !transferLine.IsCrossDockPutaway);
		}

		#endregion

		#endregion

		#region TestTransferFromWarehouse

		public void TestTransferFromWarehouse()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var docket = GetNewWhsDocket();
			var line = GetNewBusinessObject(docket);
			docket.WD_WW_Whs = whs.PK;
			AssertEquals(whs, line.TransferFromWarehouse);
		}

		#endregion

		#region TestTransferFromWarehousePK

		public void TestTransferFromWarehousePK()
		{
			var client = Helper.CreateClient();
			var whs1 = Helper.CreateWarehouse("WH1", "A");
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var docket = GetNewWhsDocket();
			var line = GetNewBusinessObject(docket, false);
			AssertEquals(ZGuid.Empty, line.TransferFromWarehousePK);

			docket.WD_WW_Whs = whs1.PK;
			AssertEquals(whs1.PK, line.TransferFromWarehousePK);

			docket.WD_OH_Client = client.PK;

			var part = Helper.CreateProduct(docket.Client, "P1");
			Helper.CreateWhsReceiveWithInventory(docket.Client, whs1, "R1", part, 10m, whs1.FindLocation("A"), "");

			line.WE_TransactionQuantity = 1m;

			// for source inter-whs transfer
			docket.WD_DocketType = DocketType.Codes.Transfer;
			docket.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			AssertEquals(ZGuid.Empty, line.TransferFromWarehousePK);

			line.TransferFromWarehousePK = whs1.PK;
			AssertEquals(whs1.PK, line.TransferFromWarehousePK);

			// modify location
			line.WE_WL_TransferFrom = whs1.DefaultLocation.PK;
			line.TransferFromWarehousePK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, line.WE_WL_TransferFrom);
			AssertEquals(ZGuid.Empty, line.TransferFromWarehousePK);

			// for internal transfer
			docket.WD_DocketSubType = TransferType.Codes.Internal;
			docket.WD_WW_Whs = whs2.PK;
			AssertEquals(whs2.PK, line.TransferFromWarehousePK);

			//load properly for Inter Whs Dest transfer
			docket.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			line.WE_OP = part.PK;
			line.TransferFromWarehousePK = whs1.PK;
			line.TransferFromLocationString = "A";
			line.DestinationWarehousePK = whs2.PK;
			line.LocationString = "B";
			line.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var lineInOtherFactory = otherFactory.Load<WhsTransferLine>(line.PK);
			AssertEquals("Source warehouse should be loaded from location.", whs1.PK, lineInOtherFactory.TransferFromWarehousePK);

			lineInOtherFactory.WE_WL_TransferFrom = ZGuid.Empty;
			AssertEquals("Source warehouse should stay even if location is cleared.", whs1.PK, lineInOtherFactory.TransferFromWarehousePK);
		}

		#endregion

		#region TestTransferFromLocation

		public void TestTransferFromLocation()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var docketLine = GetNewBusinessObject();
			docketLine.WE_WL_TransferFrom = whs.DefaultLocation.PK;
			AssertEquals(whs.DefaultLocation, docketLine.TransferFromLocation);
		}

		#endregion

		#region TestWE_WL_TransferFrom

		public void TestWE_WL_TransferFrom()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var docketLine = GetNewBusinessObject();
			var docketLineInterface = (IWhsTransferLine)docketLine;
			docketLine.WE_WL_TransferFrom = whs.DefaultLocation.PK;
			AssertEquals(whs.DefaultLocation.PK, docketLine.WE_WL_TransferFrom);
			AssertEquals(whs.DefaultLocation.PK, docketLineInterface.WE_WL_TransferFrom);
		}

		#endregion

		#region TestTransferFromLocationString

		public void TestTransferFromLocationString()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 1);

			var docketLine = GetNewBusinessObject();
			var docket = GetNewWhsDocket(docketLine);
			docket.WD_WW_Whs = whs1.PK;
			docketLine.TransferFromLocationString = "A-1";
			AssertEquals(whs1.PK, docketLine.WarehousePK);
			AssertEquals("A-1", docketLine.TransferFromLocationString);

			docketLine.TransferFromLocationString = "";
			AssertEquals("", docketLine.TransferFromLocationString);
			AssertEquals(null, docketLine.TransferFromLocation);
			AssertHasErrors(docketLine.TransferFromLocationStringInfo);
		}

		public void TestTransferFromLocationString_FixedWidthLocation()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("WHS", 3, 3, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "A00200201");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "A00100101");
			Factory.Save();

			var docketLine = GetNewBusinessObject();
			var docket = GetNewWhsDocket(docketLine);
			docket.WD_WW_Whs = warehouse.PK;
			AssertEquals(warehouse.PK, docketLine.WarehousePK);

			docketLine.TransferFromLocationString = "A00200201";
			AssertEquals("A-002-002-01", docketLine.TransferFromLocationString);
			AssertEquals(location1, docketLine.TransferFromLocation);
			AssertNoErrors(docketLine.TransferFromLocationStringInfo);

			docketLine.TransferFromLocationString = "A00100101";
			AssertEquals("A-001-001-01", docketLine.TransferFromLocationString);
			AssertEquals(location2, docketLine.TransferFromLocation);
			AssertNoErrors(docketLine.TransferFromLocationStringInfo);

			docketLine.TransferFromLocationString = "BLA";
			AssertEquals("BLA", docketLine.TransferFromLocationString);
			AssertEquals(null, docketLine.TransferFromLocation);
			AssertHasErrors(docketLine.TransferFromLocationStringInfo);

			docketLine.TransferFromLocationString = "A-001-001-01";
			AssertEquals("A-001-001-01", docketLine.TransferFromLocationString);
			AssertEquals(location2, docketLine.TransferFromLocation);
			AssertNoErrors(docketLine.TransferFromLocationStringInfo);
		}

		#endregion

		#region TestTransferFromLocationString_MaxLength

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestTransferFromLocationString_MaxLength()
		{
			var docketLine = GetNewBusinessObject();
			try
			{
				docketLine.TransferFromLocationString = ZString.Replicate('A', docketLine.TransferFromLocationStringInfo.MaxLength + 1);
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		#endregion

		#region TestTransferFromLocationStringInfo

		public void TestTransferFromLocationStringInfo()
		{
			var docketLine = GetNewBusinessObject();
			AssertEquals("TransferFromLocationString", docketLine.TransferFromLocationStringInfo.Name);
			AssertEquals(36, docketLine.TransferFromLocationStringInfo.MaxLength);

			TestStandardReadOnly(d => d.TransferFromWarehousePKInfo);
		}

		#endregion

		#region TestDestinationWarehousePK

		public void TestDestinationWarehousePK()
		{
			var client = Helper.CreateClient();
			var whs1 = Helper.CreateWarehouse("WH1", "A");
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(client, whs1, "R1", part, 10m, whs1.FindLocation("A"), "");

			var transfer = Helper.CreateWhsTransfer(client, whs1);
			var line = GetNewBusinessObject(transfer);
			line.WE_TransactionQuantity = 1m;

			// for destination inter-whs transfer
			transfer.WD_DocketType = DocketType.Codes.Transfer;
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			AssertEquals(ZGuid.Empty, line.DestinationWarehousePK);

			line.DestinationWarehousePK = whs1.PK;
			AssertEquals(whs1.PK, line.DestinationWarehousePK);

			// modify location
			line.WE_WL = whs1.DefaultLocation.PK;
			line.DestinationWarehousePK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, line.WE_WL);
			AssertEquals(ZGuid.Empty, line.DestinationWarehousePK);

			// for internal transfer
			transfer.WD_DocketSubType = TransferType.Codes.Internal;
			transfer.WD_WW_Whs = whs2.PK;
			AssertEquals(whs2.PK, line.DestinationWarehousePK);

			//load properly for Inter Whs Source transfer
			transfer.WD_WW_Whs = whs1.PK;
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			line.WE_OP = part.PK;
			line.TransferFromWarehousePK = whs1.PK;
			line.TransferFromLocationString = "A";
			line.DestinationWarehousePK = whs2.PK;
			line.LocationString = "B";
			line.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var lineInOtherFactory = otherFactory.Load<WhsTransferLine>(line.PK);
			AssertEquals("Dest warehouse should be loaded from location.", whs2.PK, lineInOtherFactory.DestinationWarehousePK);

			lineInOtherFactory.WE_WL = ZGuid.Empty;
			AssertEquals("Dest warehouse should stay even if location is cleared.", whs2.PK, lineInOtherFactory.DestinationWarehousePK);
		}

		#endregion

		#region TestWarehousePK_InterWarehouseDestination

		public void TestWarehousePK_InterWarehouseDestination()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("Warehouse1", "A", 3, 3);
			Factory.Save();

			var sourceLocation = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var interWhsDestTransfer = Helper.CreateWhsTransfer(data.Org1, whs2, "TR2", Notify, TransferType.Codes.InterWhsDest);
			var interWhsDestTransferLine = Helper.CreateWhsTransferLine(interWhsDestTransfer, data.Part1.PK, 10m, "A", data.Whs1.PK, "");
			interWhsDestTransferLine.PickedTime = ZDateTimeOffset.Now;

			var childLine = interWhsDestTransferLine.ChildTransferLine;
			AssertNotNull("Precondition: Create Child Line.", childLine);
			AssertEquals("Precondition: Child Line has no dest location.", ZGuid.Empty, childLine.WE_WL);
			AssertEquals("WarehousePK is correct.", whs2.PK, childLine.WarehousePK);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var childLine_InFactory2 = factory2.Load<WhsTransferLine>(childLine.PK);
			AssertEquals("WarehousePK is correct when loaded in a new factory.", whs2.PK, childLine_InFactory2.WarehousePK);
		}

		#endregion

		#region TestArrivalDateForBindingIsEmptyWhen_DatesDifferentForMatchingLines

		public void TestArrivalDateForBindingIsEmptyWhen_DatesDifferentForMatchingLines()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 08), data.Part1, 2m, sourceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 16), data.Part1, 3m, sourceLocation, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation, destinationLocation);
			transferLine.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			transferLine.FinaliseDocketLine();
			AssertEquals(1, transferLine.MatchingLines.Count);
			var matchingLine = transferLine.MatchingLines[0];
			AssertNotEquals(transferLine.WE_AdjustmentArrivalDate, matchingLine.WE_AdjustmentArrivalDate);
			AssertEquals(false, transferLine.WE_AdjustmentArrivalDate.IsEmpty);
			AssertEquals(false, matchingLine.WE_AdjustmentArrivalDate.IsEmpty);
			AssertEquals(true, transferLine.ArrivalDateForBinding.IsEmpty);
		}

		#endregion

		#region TestArrivalDateForBindingIsNotEmptyWhen_DatesAreSameForMatchingLines

		public void TestArrivalDateForBindingIsNotEmptyWhen_DatesAreSameForMatchingLines()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			var arrivalDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 08);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", arrivalDate, data.Part1, 2m, sourceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", arrivalDate, data.Part1, 3m, sourceLocation, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation, destinationLocation);
			transferLine.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			transferLine.FinaliseDocketLine();
			AssertEquals(1, transferLine.MatchingLines.Count);
			var matchingLine = transferLine.MatchingLines[0];
			AssertEquals(transferLine.WE_AdjustmentArrivalDate, matchingLine.WE_AdjustmentArrivalDate);
			AssertEquals(false, transferLine.ArrivalDateForBinding.IsEmpty);
			AssertEquals(arrivalDate, transferLine.ArrivalDateForBinding);
			AssertEquals(arrivalDate, transferLine.WE_AdjustmentArrivalDate);
			AssertEquals(arrivalDate, matchingLine.WE_AdjustmentArrivalDate);
		}

		#endregion

		#region TestAdjustmentDateForBindingInfo

		public void TestAdjustmentDateForBindingInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation, destinationLocation);
			transferLine.RunPreSaveValidation(); // to commit inventory
			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(transferLine.ArrivalDateForBindingInfo, data);
			transfer.WD_DocketStatus = DocketStatus.Codes.Entered; //Revert back the changes
		}

		#endregion

		#region TestAdjustmentDateForBindingInfoVASOrder

		public void TestAdjustmentDateForBindingInfoVASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);

			Factory.Save();

			var vasOrderTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition", vasOrderTransfer);
			var line = (WhsTransferLine)vasOrderTransfer.Lines.Single();
			AssertEquals(true, line.ArrivalDateForBindingInfo.ReadOnly);
			line.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(line);
			AssertEquals(true, line.ArrivalDateForBindingInfo.ReadOnly);

			var outOfServiceAreaTransfer = Factory.New<WhsTransfer>();
			vasOrder.WVO_WD_TransferOutOfServiceArea = outOfServiceAreaTransfer.PK;
			AssertEquals(true, outOfServiceAreaTransfer.Lines.AddNew().ArrivalDateForBindingInfo.ReadOnly);
		}

		#endregion

		#region TestCurrentLocation_InTransit

		public void TestCurrentLocation_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "A-2");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: stock should be committed.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition: stock should be committed.", 20m, transferLine.GetQtyCommittedToThisLine());

			AssertEquals("Precondition.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - Has *NOT* created inventory yet.", 0m, transferLine.WE_StockOnHand);
			AssertCurrentLocationProperties(transferLine, location2); // No inventory created yet so it is fine to fall back to WE_WL as this line isn't shown in the inventory module yet

			transferLine.PickedTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition: Should have changed Inventory Status.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - Created inventory.", 20m, transferLine.WE_StockOnHand);
			AssertCurrentLocationProperties(transferLine, null); // Should have no location

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			AssertEquals("Precondition: Finalised.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - Created inventory.", 20m, transferLine.WE_StockOnHand);
			AssertCurrentLocationProperties(transferLine, location2); // No inventory created yet so it is fine to fall back to WE_WL as this line isn't shown in the inventory module yet
		}

		#endregion

		#region TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume

		protected override bool SettingWE_TransactionQuantityUpdatesTotalWeightAndVolume => false;

		#endregion

		#region TestWE_CommittedQuantity

		public void TestWE_CommittedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locations[0]);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, locations[0]);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "OR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locations[0].ToLocationString(), "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 100m, locations[0].ToLocationString(), "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locations[0].ToLocationString(), "");
			transferLine3.WE_TransferFromPalletId = "PLT-1";

			transfer.RunPreSaveValidation();

			AssertEquals("Quantity Committed should be same as Quantity To Transfer.", 10m, transferLine1.GetQtyCommittedToThisLine());
			AssertEquals("Quantity Committed should not be bigger that available quantity.", 50m, transferLine2.GetQtyCommittedToThisLine());
			AssertEquals("There should be no Committed quantity if available quantity is 0.", 0m, transferLine3.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestWE_WL_ClearsPalletIdForPickFaceLocation

		public void TestWE_WL_ClearsPalletIdForPickFaceLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var pickFace = product.PickFaces.AddNew();
			var pickFaceLocation = locations[1];
			pickFace.WF_WL = pickFaceLocation.PK;
			var normalLocation = locations[2];
			var stockLocation = locations[0];

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, stockLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "OR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, stockLocation.ToLocationString(), "");
			transferLine.WE_PalletID = "ABC";
			transferLine.WE_WL = normalLocation.PK;
			AssertEquals("Setting non-pickface destination location should not affect pallet id", "ABC", transferLine.WE_PalletID);

			transferLine.WE_WL = pickFaceLocation.PK;
			AssertEquals("Setting pick destination location should clear pallet id", "", transferLine.WE_PalletID);
		}

		public void TestWE_WL_ClearsPalletIdForPickFaceLocation_DifferentClientPickFaces()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var differentClient = Helper.CreateClient("C2", "C2");
			Helper.CreateProductClientRelationShip(differentClient, data.Part1);
			var pickFaceLocation = locations[0];
			Helper.CreateProductPickFace(data.Part1, differentClient, pickFaceLocation);
			var transferToLocation = locations[1];
			var transferFromLocation = locations[2];

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, transferFromLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "OR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, transferFromLocation.ToLocationString(), "");
			transferLine.WE_PalletID = "ABC";
			transferLine.WE_WL = transferToLocation.PK;
			AssertEquals("Setting non-pickface destination location should not affect pallet id", "ABC", transferLine.WE_PalletID);

			transferLine.WE_WL = pickFaceLocation.PK;
			AssertEquals("Pallet Id should not be cleared since pickface location belongs to a different client.", "ABC", transferLine.WE_PalletID);
		}

		public void TestWE_WL_ClearsPalletIdForPickFaceLocation_DoesNotClearWhenRetainPalletIDsFlagIsTrue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var pickFace = product.PickFaces.AddNew();
			var pickFaceLocation = locations[1];
			pickFaceLocation.LocationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			pickFace.WF_WL = pickFaceLocation.PK;
			var normalLocation = locations[2];
			var stockLocation = locations[0];

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, stockLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "OR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, stockLocation.ToLocationString(), "");
			transferLine.WE_PalletID = "ABC";
			transferLine.WE_WL = normalLocation.PK;
			AssertEquals("Setting non-pickface destination location should not affect pallet id", "ABC", transferLine.WE_PalletID);

			transferLine.WE_WL = pickFaceLocation.PK;
			AssertEquals("Pallet Id should not be cleared since the Retain Pallet IDs flag is true.", "ABC", transferLine.WE_PalletID);
		}

		#endregion

		#region TestWE_F3_NKPackType_SetsAllMatchingLines

		public void TestWE_F3_NKPackType_SetsAllMatchingLines()
		{
			AssertSetPropertySetsPropertyOnAllMatchingLines(WhsTransferLine.Schema.WE_F3_NKPackType, new ZString("UNT"), new ZString("PLT"));
		}

		#endregion

		#region TestIsPutawayTransferLine

		public void TestIsPutawayTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var nonPutawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Assert(putawayTransfer.Lines.AddNew().IsPutawayTransferLine);
			Assert(!nonPutawayTransfer.Lines.AddNew().IsPutawayTransferLine);
			Assert(!Factory.New<WhsTransferLine>().IsPutawayTransferLine);
		}

		#endregion

		#region TestAssociatedReceiveLineOfPutawayTransfer

		public void TestAssociatedReceiveLineOfPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLT");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT", 10m);
			transfer.RunPreSaveValidation();
			AssertEquals("Transfer line is putaway transferline", true, putawayTransferLine.IsPutawayTransferLine);
			AssertNotNull("Putaway transferlines should return a receiveline", putawayTransferLine.AssociatedReceiveLineOfPutawayTransfer);
			AssertEquals("PK is same as expected", receiveLine.PK, putawayTransferLine.AssociatedReceiveLineOfPutawayTransfer.PK);

			var nonPutawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			AssertNull("Non putaway transferlines should return null", nonPutawayTransfer.Lines.AddNew().AssociatedReceiveLineOfPutawayTransfer);
		}

		#endregion

		#region PickedTime

		#region TestPickedTime

		public void TestPickedTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventory.LocationString, "");
			AssertEquals("Precondition", ZDateTimeOffset.Empty, transferLine.PickedTime);
			AssertEquals("Precondition", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition", 0m, transferLine.QtyCommittedIncludingMatchingLines);

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			transferLine.PickedTime = now;
			AssertEquals("Stock should be committed when Picking if not yet Committed.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			AssertEquals(DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals(now, transferLine.PickedTime);

			transferLine.PickedTime = ZDateTimeOffset.Empty;
			AssertEquals(DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			AssertEquals(DocketLineStatus.Codes.Finalised, transferLine.WE_DocketLineStatus);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.PickedTime = now;
				AssertEquals(DocketLineStatus.Codes.Finalised, transferLine.WE_DocketLineStatus);
			}
		}

		#endregion

		#region TestPickedTime_SetsCorrectTimeOffset

		public void TestPickedTime_SetsCorrectTimeOffset()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, inventoryLocation);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventoryLocation.WLV_LocationString, "");
			Helper.CreateWhsPickLine(transferLine, receiveLine1.Inventory[0], 5m);
			Helper.CreateWhsPickLine(transferLine, receiveLine2.Inventory[0], 5m);
			Factory.Save();

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.UtcNow);
			var warehouseLocalTime = data.Whs1.GetWarehouseBranchDateTimeOffset(now.ToUtcDateTime());

			transferLine.PickedTime = now;

			var matchingLine = (WhsTransferLine)transferLine.MatchingLines.Single();
			AssertEquals("Local Warehouse Time should be used to set PickedTime.", warehouseLocalTime, transferLine.PickedTime);
			AssertEquals("Local Warehouse Time should be used to set PickedTime.", warehouseLocalTime, matchingLine.PickedTime);
			AssertEquals("Local Warehouse Time should be used to set PickedTime.", warehouseLocalTime, transferLine.PickLines[0].WZ_PickedDateTime);
			AssertEquals("Local Warehouse Time should be used to set PickedTime.", warehouseLocalTime, matchingLine.PickLines[0].WZ_PickedDateTime);
		}

		#endregion

		#region TestPickedTime_SetDocketLineStatus

		public void TestPickedTime_SetDocketLineStatus()
		{
			var staff = Helper.CreateGlbStaff("ST1", "TestUser");
			Helper.Factory.Save();

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventory.LocationString, "");
			AssertEquals("Precondition", ZDateTimeOffset.Empty, transferLine.PickedTime);
			AssertEquals("Precondition", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			Assert("Precondition", !transferLine.IsPutawayTransferLine);
			Assert("Precondition", GlbStaff.CurrentUser.IsSupportUser);

			var now = ZDateTimeOffset.Now;
			transferLine.PickedTime = now;
			AssertEquals("WE_DocketLineStatus should be HFT if not picked.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			transferLine.Docket.WD_IsPutawayTransfer = true;
			transferLine.PickedTime = now;
			Assert("WE_DocketLineStatus should be PFU if picked.", transferLine.PickLines.All(p => p.Inventory.InDocketLine.WE_DocketLineStatus == DocketLineStatus.Codes.PickedForUnload));
			Assert("WE_StockOnHand should be 0 if picked.", transferLine.PickLines.All(p => p.Inventory.InDocketLine.WE_StockOnHand == 0));

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				transferLine.Docket.WD_IsPutawayTransfer = true;
				transferLine.PickedTime = now;
				AssertEquals("WE_DocketLineStatus should be HFT if the current staff is not support user even it is picked.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			}
		}

		#endregion

		#region TestPickedTime_CommitsStock

		public void TestPickedTime_CommitsStock()
		{
			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", now, data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", now, data.Part1, 5m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", "A");
			AssertEquals("Precondition: No Stock is Committed.", 0m, transferLine.QtyCommittedIncludingMatchingLines);

			var matchingLine = Helper.CreateMatchingLine(transferLine, 5m);
			AssertEquals("Precondition: No Stock is Committed.", 0m, transferLine.QtyCommittedIncludingMatchingLines);

			matchingLine.PickedTime = now;
			AssertEquals("No Stock should be Committed when setting Matching Line.", 0m, transferLine.QtyCommittedIncludingMatchingLines);
			matchingLine.Delete(); // clean up

			transferLine.PickedTime = now;
			var newMatchingLine = (WhsTransferLine)transferLine.MatchingLines.Single();
			AssertEquals("Stock should be Committed when picking.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			AssertEquals("Stock should be Committed when picking.", 5m, newMatchingLine.GetQtyCommittedToThisLine());
			AssertEquals("Picked Time should be set.", now, transferLine.PickedTime);
			AssertEquals("Picked Time should be set.", now, newMatchingLine.PickedTime);
			AssertEquals("Picked Time should be set.", true, transferLine.PickLines.All(pl => pl.WZ_PickedDateTime == now));
			AssertEquals("Picked Time should be set.", true, newMatchingLine.PickLines.All(pl => pl.WZ_PickedDateTime == now));

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);

			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot change Picked Time of Finalised Transfer Line.", () => transferLine.PickedTime = now.AddDays(1));
			AssertEquals("Cannot change Picked Time of Finalised Transfer Line.", now, transferLine.PickedTime);

			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot change Picked Time of Finalised Transfer Line.", () => transferLine.PickedTime = ZDateTimeOffset.Empty);
			AssertEquals("Cannot change Picked Time of Finalised Transfer Line.", now, transferLine.PickedTime);
		}

		#endregion

		#region TestPickedTime_DataRefresh

		[TestDate(2014, 01, 03)]
		public void TestPickedTime_DataRefresh()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation, destinationLocation);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, line.PickedTime);

			line.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Picklines should not be created if there's nothing to commit.", 0, line.PickLines.Count);
			AssertEquals("Picked time should be empty if there are no picklines available.", ZDateTimeOffset.Empty, line.PickedTime);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, sourceLocation, "");
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 1m, line.QtyCommittedIncludingMatchingLines);

			var pickLine = line.PickLines.Single();
			line.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Transfer line Picked time should now be set.", ZDateTimeOffset.Now, line.PickedTime);
			AssertEquals("Pick line Picked time should now be set.", ZDateTimeOffset.Now, pickLine.WZ_PickedDateTime);

			line.PickedTime = ZDateTimeOffset.Now.AddDays(2);
			AssertEquals(ZDateTimeOffset.Now.AddDays(2), line.PickedTime);
			AssertEquals(ZDateTimeOffset.Now.AddDays(2), pickLine.WZ_PickedDateTime);

			line.PickedTime = ZDateTimeOffset.Empty;
			AssertEquals(ZDateTimeOffset.Empty, line.PickedTime);
			AssertEquals(ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);

			line.PickedTime = ZDateTimeOffset.Now.AddDays(2);
			AssertEquals(ZDateTimeOffset.Now.AddDays(2), line.PickedTime);
			AssertEquals(ZDateTimeOffset.Now.AddDays(2), pickLine.WZ_PickedDateTime);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);
			var transferLineInNewFactory = (WhsTransferLine)transferInNewFactory.Lines.Single();
			var pickLineInAnotherFactory = transferLineInNewFactory.PickLines.Single();
			AssertEquals("Transfer line Picked time in new factory should be same as in old factory.", ZDateTimeOffset.Now.AddDays(2), transferLineInNewFactory.PickedTime);
			AssertEquals("Pick line Picked time in new factory should be same as in old factory.", ZDateTimeOffset.Now.AddDays(2), pickLineInAnotherFactory.WZ_PickedDateTime);
		}

		#endregion

		#region TestPickedTime_OnChildTransferLine

		public void TestPickedTime_OnChildTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", "");
			transferLine.DestinationWarehousePK = warehouse2.PK;
			transferLine.LocationString = warehouse2.DefaultLocation.WLV_LocationString;

			var childTransfer = (WhsTransfer)transfer.Clone();
			var childTransferLine = transferLine.Clone<WhsTransferLine>();
			childTransferLine.WE_WD = childTransfer.PK;
			childTransferLine.WE_WE_ParentDocketLine = transferLine.PK;
			AssertEquals("Precondition: Picked Time should be empty.", ZDateTimeOffset.Empty, childTransferLine.PickedTime);

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot set Picked Time on Child Transfer Line.", () => childTransferLine.PickedTime = now);
			AssertEquals("Picked Time should be empty when trying to set Child Transfer Line.", ZDateTimeOffset.Empty, childTransferLine.PickedTime);
			AssertEquals("No stock should be Committed when trying to set Child Transfer Line.", 0m, childTransferLine.GetQtyCommittedToThisLine());

			transferLine.PickedTime = now;
			AssertEquals("Picked Time on Child Transfer Line should be the same as the Master Transfer Line.", now, childTransferLine.PickedTime);

			childTransferLine.WE_WE_ParentDocketLine = ZGuid.Empty;
			AssertEquals("Picked Time on Child Transfer Line should empty if Master Transfer Line does not exist.", ZDateTimeOffset.Empty, childTransferLine.PickedTime);
		}

		#endregion

		#region TestPickedTime_ReducesStockWhenSet

		public void TestPickedTime_ReducesStockWhenSet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 3m, sourceLocation, "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation, destinationLocation);
			line.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is Committed.", 1m, line.QtyCommittedIncludingMatchingLines);
			AssertEquals("Precondition: Stock is unchanged.", 3m, inventory.WI_TotalUnits);

			line.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Picking Transfer Line reduces Stock.", 2m, inventory.WI_TotalUnits);
		}

		#endregion

		#region TestPickedTime_SetsPickerToCurrentUserIfPicked

		[TestDate(2014, 01, 03)]
		public void TestPickedTime_SetsPickerToCurrentUserIfPicked()
		{
			var staff = Helper.CreateGlbStaff("BRS", "BRS");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, sourceLocation, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation, destinationLocation);
			AssertEquals("Precondition", "", line.GS_NKPickedBy);

			transfer.RunPreSaveValidation();
			Factory.Save();
			var pickLine = line.PickLines.Single();
			var refreshBindingCount = 0;
			line.GS_NKPickedByInfo.ValueChanged += (s, e) => refreshBindingCount++;

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Preconditon.", 0, refreshBindingCount);
				line.PickedTime = ZDateTimeOffset.Now.AddDays(2);
			}

			AssertEquals("Precondition.", ZDateTimeOffset.Now.AddDays(2), line.PickedTime);
			AssertEquals("Precondition.", ZDateTimeOffset.Now.AddDays(2), pickLine.WZ_PickedDateTime);
			AssertEquals("Should have set PickedBy to current user", "BRS", line.GS_NKPickedBy);
			AssertEquals("Should have set PickedBy to current user", "BRS", pickLine.WZ_GS_NKAssignedTo);
			AssertEquals("Should have refreshed binding.", 1, refreshBindingCount);

			AssertEquals("Preconditon.", 1, refreshBindingCount);
			line.PickedTime = ZDateTimeOffset.Empty;
			AssertEquals("Precondition.", ZDateTimeOffset.Empty, line.PickedTime);
			AssertEquals("Precondition.", ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);
			AssertEquals("Should *not* have cleared PickedBy.", "BRS", line.GS_NKPickedBy);
			AssertEquals("Should *not* have cleared PickedBy.", "BRS", pickLine.WZ_GS_NKAssignedTo);
			AssertEquals("Should have refreshed binding.", 2, refreshBindingCount);

			line.GS_NKPickedBy = "E";
			AssertEquals("Precondition.", "E", line.GS_NKPickedBy);
			AssertEquals("Precondition.", "E", pickLine.WZ_GS_NKAssignedTo);
			AssertEquals("Should have refreshed binding.", 3, refreshBindingCount);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Preconditon.", 3, refreshBindingCount);
				line.PickedTime = ZDateTimeOffset.Now;
			}

			AssertEquals("Precondition.", ZDateTimeOffset.Now, line.PickedTime);
			AssertEquals("Precondition.", ZDateTimeOffset.Now, pickLine.WZ_PickedDateTime);
			AssertEquals("Should *not* have overridden PickedBy.", "E", line.GS_NKPickedBy);
			AssertEquals("Should *not* have overridden PickedBy.", "E", pickLine.WZ_GS_NKAssignedTo);
			AssertEquals("Should have refreshed binding.", 4, refreshBindingCount);

			line.GS_NKPickedBy = "";
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Preconditon.", 5, refreshBindingCount);
				line.PickedTime = ZDateTimeOffset.Empty;
			}

			AssertEquals("Clearing PickedTime should *not* set PickedBy.", "", line.GS_NKPickedBy);
			AssertEquals("Clearing PickedTime should *not* set PickedBy.", "", pickLine.WZ_GS_NKAssignedTo);
			AssertEquals("Should have refreshed binding.", 6, refreshBindingCount);
		}

		#endregion

		#region TestPickedTime_SetsAllMatchingLines

		public void TestPickedTime_SetsAllMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var now = ZDateTimeOffset.Now;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", now, data.Part1, 10m, data.Whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", now, data.Part1, 5m, data.Whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", now, data.Part1, 5m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var whs2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			Factory.Save();
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, data.Whs1.DefaultLocation.WLV_LocationString, "");
			line.DestinationWarehousePK = whs2.PK;
			line.LocationString = whs2.DefaultLocation.WLV_LocationString;
			line.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is Committed.", 20m, line.QtyCommittedIncludingMatchingLines);
			AssertEquals("Precondition: Matching Lines are created.", 2, line.MatchingLines.Count);

			var matchingLine1 = line.MatchingLines[0];
			var matchingLine2 = line.MatchingLines[1];
			AssertEquals("Precondition - Property to test should be empty.", ZDateTimeOffset.Empty, line.PickedTime);

			var value1 = ZDateTimeOffset.Today.AddDays(5);
			matchingLine1.PickedTime = value1;
			AssertEquals("Modifying PickedTime on matching line should *not* modify it on main line.", ZDateTimeOffset.Empty, line.PickedTime);
			AssertEquals("Modifying PickedTime on matching line should stay on the matching line.", value1, matchingLine1.PickedTime);
			AssertEquals("Modifying PickedTime on matching line should not modify other matching lines.", ZDateTimeOffset.Empty, matchingLine2.PickedTime);

			var value2 = ZDateTimeOffset.Today.AddDays(10);
			line.PickedTime = value2;
			AssertEquals("Modifying PickedTime on main line should stay on the main line.", value2, line.PickedTime);
			AssertEquals("Modifying PickedTime on main line should modify it on matching line 1.", value2, matchingLine1.PickedTime);
			AssertEquals("Modifying PickedTime on main line should modify it on matching line 2.", value2, matchingLine2.PickedTime);
		}

		#endregion

		#region TestPickedTime_CreatesInTransitInventory

		#region TestPickedTime_CreatesInTransitInventory

		public void TestPickedTime_CreatesInTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, location.ToLocationString(), "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, location.ToLocationString(), "");
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			AssertEquals("Precondition", 30m, transferLine1.GetQtyCommittedToThisLine());
			AssertEquals("Precondition", DocketLineStatus.Codes.Entered, transferLine1.WE_DocketLineStatus);
			AssertEquals("Precondition.", InventoryStatus.Codes.Available, transferLine1.WE_OriginalInventoryStatus);
			AssertEquals("Precondition.", InventoryStatus.Codes.Available, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition.", 0m, transferLine1.WE_StockOnHand);

			transferLine2.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition.", 20m, transferLine2.GetQtyCommittedToThisLine());
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine2.WE_DocketLineStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine2.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Total units and Committed quantity should have the same value.", 20m, transferLine2.WE_StockOnHand);
			AssertEquals("Total units and Committed quantity should have the same value.", 20m, transferLine2.PickLines.GetQtyCommitted());
			AssertNoExceptionThrown("Should have a valid datashape.", Factory.Save);
		}

		#endregion

		#region TestPickedTime_CreatesInTransitInventory_ArrivalDate

		[TestDate(2017, 1, 7)]
		public void TestPickedTime_CreatesInTransitInventory_ArrivalDate()
		{
			TestPickedTime_CreatesInTransitInventory_ArrivalDate(withArrivalDateAlreadySet: false);
		}

		[TestDate(2017, 1, 7)]
		public void TestPickedTime_CreatesInTransitInventory_ArrivalDate_WithExistingArrivalDate()
		{
			TestPickedTime_CreatesInTransitInventory_ArrivalDate(withArrivalDateAlreadySet: true);
		}

		void TestPickedTime_CreatesInTransitInventory_ArrivalDate(bool withArrivalDateAlreadySet)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, location, "", finalise: false);
			receive.WD_ArrivalDate = new ZDateTimeOffset(2017, 1, 1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, location.ToLocationString(), "");
			if (withArrivalDateAlreadySet)
			{
				transferLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(2017, 1, 1);
			}
			transferLine.RunPreSaveValidation(); // to commit inventory
			Factory.Save();
			AssertEquals("Precondition.", !withArrivalDateAlreadySet, transferLine.WE_AdjustmentArrivalDate.IsEmpty);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", 30m, transferLine.GetQtyCommittedToThisLine());
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("ArrivalDate Should Be Set To Inventory arrival date - not today.", new ZDateTimeOffset(2017, 1, 1), transferLine.WE_AdjustmentArrivalDate);
			AssertNoExceptionThrown("Should have a valid datashape.", Factory.Save);
		}

		#endregion

		#region TestPickedTime_CreatesInTransitInventory_WhenAttachedToVASOrder

		public void TestPickedTime_CreatesInTransitInventory_WhenAttachedToVASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			var transferLine = (WhsTransferLine)intoServiceAreaTransfer.Lines.Single();
			AssertEquals("Precondition", 10m, transferLine.GetQtyCommittedToThisLine());
			AssertEquals("Precondition", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			Factory.Save();

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Total units and Committed quantity should have the same value.", 10m, transferLine.WE_StockOnHand);
			AssertEquals("Total units and Committed quantity should have the same value.", 10m, transferLine.PickLines.GetQtyCommitted());
			Factory.Save();

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);

			AssertEquals("The units should not change", 10m, transferLine.WE_StockOnHand);
			AssertEquals("Transfer lines for VAS Orders should *not* have their Original Status changed.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Transfer lines for VAS Orders should have their Current Status changed to Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory on Transfers for VAS Orders should have their Current Status changed to Staged.", InventoryStatus.Codes.Staged, transferLine.Inventory[0].WI_InventoryStatus);

			AssertNoExceptionThrown(() => Factory.Save()); // ensure data is correct

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}
			AssertNotNull("Precondition: Return transfer successfully created.", returnTransfer);

			var transferLineOnReturnTransfer = (WhsTransferLine)returnTransfer.Lines.Single();
			transferLineOnReturnTransfer.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLineOnReturnTransfer.WE_DocketLineStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLineOnReturnTransfer.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLineOnReturnTransfer.WE_CurrentInventoryStatus);
			AssertEquals("Total units and Committed quantity should have the same value.", 10m, transferLineOnReturnTransfer.WE_StockOnHand);
			AssertEquals("Total units and Committed quantity should have the same value.", 10m, transferLineOnReturnTransfer.PickLines.GetQtyCommitted());
			Factory.Save();

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(returnTransfer);

			AssertEquals("The units should not change", 10m, transferLineOnReturnTransfer.WE_StockOnHand);
			AssertEquals("The Original Status for Transfer lines on Return VAS Order Transfers should be Available.", InventoryStatus.Codes.Available, transferLineOnReturnTransfer.WE_OriginalInventoryStatus);
			AssertEquals("Return Transfer Lines should have their Current Status be Available.", InventoryStatus.Codes.Available, transferLineOnReturnTransfer.WE_CurrentInventoryStatus);
			AssertEquals("Inventory on Return Transfers for VAS Orders should have their Current Status changed to Available.", InventoryStatus.Codes.Available, transferLineOnReturnTransfer.Inventory[0].WI_InventoryStatus);

			AssertNoExceptionThrown(() => Factory.Save()); // ensure data is correct
		}

		#endregion

		#region TestPickedTime_CreatesInTransitInventory_MatchingLines

		public void TestPickedTime_CreatesInTransitInventory_MatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 7m, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m, location, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 13m, location.ToLocationString(), "");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition", 13m, transferLine.GetQtyCommittedIncludingMatchingLines());

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var matchingLine = transferLine.MatchingLines[0];
			AssertEquals("Inventory status change should propogate from master.", InventoryStatus.Codes.InTransit, matchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status change should propogate from master.", InventoryStatus.Codes.InTransit, matchingLine.WE_CurrentInventoryStatus);

			AssertEquals("Total units should sum to 13.", 13m, transferLine.WE_StockOnHand + matchingLine.WE_StockOnHand);
			AssertEquals("Total units and Committed quantity should have the same value.", transferLine.PickLines.GetQtyCommitted(), transferLine.WE_StockOnHand);
			AssertEquals("Total units and Committed quantity should have the same value.", matchingLine.PickLines.GetQtyCommitted(), matchingLine.WE_StockOnHand);
			AssertNoExceptionThrown("Should have a valid datashape.", Factory.Save);
		}

		#endregion

		#region TestPickedTime_CreatesInTransitInventory_InterWarehouse_MasterSource

		public void TestPickedTime_CreatesInTransitInventory_InterWarehouse_MasterSource()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var whs3 = Helper.CreateWarehouse("WH3", "C");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 50m, whs1.DefaultLocation, "");

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine10 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			var transferLine15 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A", whs3.PK, "C");
			AssertEquals("Precondition - no related Jobs should exist.", 0, transfer.RelatedJobs.Count);

			transfer.RunPreSaveValidation();
			AssertEquals("Precondition", 10m, transferLine10.GetQtyCommittedToThisLine());
			AssertEquals("Precondition", 15m, transferLine15.GetQtyCommittedToThisLine());

			transferLine15.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine15.WE_DocketLineStatus);

			AssertEquals("Inventory status should change on source transfer line", InventoryStatus.Codes.InTransit, transferLine15.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should change on source transfer line", InventoryStatus.Codes.InTransit, transferLine15.WE_CurrentInventoryStatus);
			AssertEquals("Stock on hand should NOT change on source transfer line", 0m, transferLine15.WE_StockOnHand);

			var matchingTransfers = transfer.ChildTransfers;
			AssertEquals("Should only create 1 destination child transfer.", 1, matchingTransfers.Count());

			var matchingTransfer = transfer.ChildTransfers.Single();
			AssertEquals(TransferType.Codes.InterWhsDest, matchingTransfer.WD_DocketSubType);
			AssertEquals(whs3.PK, matchingTransfer.WD_WW_Whs);
			AssertEquals(1, matchingTransfer.Lines.Count);

			var matchingTransferLine = (WhsTransferLine)matchingTransfer.Lines.Single();
			AssertEquals("Should be held for transfer.", DocketLineStatus.Codes.HeldForTransfer, matchingTransferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should change on destination line", InventoryStatus.Codes.InTransit, matchingTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should change on destination line", InventoryStatus.Codes.InTransit, matchingTransferLine.WE_CurrentInventoryStatus);
			AssertEquals("Stock on hand should change on destination line", 15m, matchingTransferLine.WE_StockOnHand);
			AssertNoExceptionThrown("Should have a valid datashape.", Factory.Save);
		}

		#endregion

		#region TestPickedTime_CreatesInTransitInventory_InterWarehouse_MasterSource_MatchingLines

		public void TestPickedTime_CreatesInTransitInventory_InterWarehouse_MasterSource_MatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 26m, whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part1, 24m, whs1.DefaultLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 40m, "A", whs2.PK, "B");
			transferLine.RunPreSaveValidation();
			var matchingLine = transferLine.MatchingLines[0];
			AssertEquals("Precondition", 40m, transferLine.GetQtyCommittedIncludingMatchingLines());

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, matchingLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should change on source transfer line", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should change on source transfer line", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Stock on hand should NOT change on source transfer line", 0m, transferLine.WE_StockOnHand);

			var childTransfers = transfer.ChildTransfers;
			AssertEquals("Should only create 1 destination child transfer.", 1, childTransfers.Count());

			var childTransfer = childTransfers.Single();
			AssertEquals(TransferType.Codes.InterWhsDest, childTransfer.WD_DocketSubType);
			AssertEquals(whs2.PK, childTransfer.WD_WW_Whs);
			AssertEquals(1, childTransfer.Lines.Count);

			var childTransferLine = (WhsTransferLine)childTransfer.Lines.Single();
			AssertEquals("Should be held for transfer.", DocketLineStatus.Codes.HeldForTransfer, childTransferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should change on destination line", InventoryStatus.Codes.InTransit, childTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should change on destination line", InventoryStatus.Codes.InTransit, childTransferLine.WE_CurrentInventoryStatus);

			var childTransferMatchingLine = (WhsTransferLine)childTransferLine.MatchingLines.Single();
			AssertEquals("Should be held for transfer.", DocketLineStatus.Codes.HeldForTransfer, childTransferMatchingLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should change on destination line", InventoryStatus.Codes.InTransit, childTransferMatchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should change on destination line", InventoryStatus.Codes.InTransit, childTransferMatchingLine.WE_CurrentInventoryStatus);

			AssertEquals("Total units should sum to 40.", 40m, childTransferLine.WE_StockOnHand + childTransferMatchingLine.WE_StockOnHand);
			AssertEquals("Total units on child and Committed quantity on parent should have the same value.", transferLine.PickLines.GetQtyCommitted(), childTransferLine.WE_StockOnHand);
			AssertEquals("Total units on child and Committed quantity on parent should have the same value.", matchingLine.PickLines.GetQtyCommitted(), childTransferMatchingLine.WE_StockOnHand);
			AssertNoExceptionThrown("Should have a valid datashape.", Factory.Save);
		}

		#endregion

		#region TestPickedTime_CreatesInTransitInventory_InterWarehouse_MasterDestination

		public void TestPickedTime_CreatesInTransitInventory_InterWarehouse_MasterDestination()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var whs3 = Helper.CreateWarehouse("WH3", "C");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", data.Part1, 50m, whs2.DefaultLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, whs3, "R3", data.Part1, 50m, whs3.DefaultLocation, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "C", whs3.PK, "A");
			AssertEquals("Precondition - no related Jobs should exist.", 0, transfer.RelatedJobs.Count);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should change on destination transfer line", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should change on destination transfer line", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Stock on hand should change on destination transfer line", 15m, transferLine.WE_StockOnHand);

			var matchingTransfers = transfer.ChildTransfers;
			AssertEquals(1, matchingTransfers.Count());

			var matchingTransfer = transfer.ChildTransfers.Single();
			AssertEquals(TransferType.Codes.InterWhsSource, matchingTransfer.WD_DocketSubType);
			AssertEquals(whs3.PK, matchingTransfer.WD_WW_Whs);
			AssertEquals(1, matchingTransfer.Lines.Count);

			var matchingTransferLine = (WhsTransferLine)matchingTransfer.Lines.Single();
			AssertEquals("Should be held for transfer.", DocketLineStatus.Codes.HeldForTransfer, matchingTransferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should change on source line", InventoryStatus.Codes.InTransit, matchingTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should change on source line", InventoryStatus.Codes.InTransit, matchingTransferLine.WE_CurrentInventoryStatus);
			AssertEquals("Stock on hand should NOT change on source line", 0m, matchingTransferLine.WE_StockOnHand);
			AssertNoExceptionThrown("Should have a valid datashape.", Factory.Save);
		}

		#endregion

		#region TestPickedTime_CreatesInTransitInventory_InterWarehouse_MasterDestination_MatchingLines

		public void TestPickedTime_CreatesInTransitInventory_InterWarehouse_MasterDestination_MatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", data.Part1, 26m, whs2.DefaultLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R3", data.Part1, 24m, whs2.DefaultLocation, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 40m, "B", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			var matchingLine = transferLine.MatchingLines[0];
			AssertEquals("Precondition", 40m, transferLine.GetQtyCommittedIncludingMatchingLines());

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, matchingLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should change on destination transfer line", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should change on destination transfer line", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory status should change on destination matching transfer line", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should change on destination matching transfer line", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Total units should sum to 40.", 40m, transferLine.WE_StockOnHand + matchingLine.WE_StockOnHand);
			AssertEquals("Total units and Committed quantity should have the same value.", transferLine.PickLines.GetQtyCommitted(), transferLine.WE_StockOnHand);
			AssertEquals("Total units and Committed quantity should have the same value.", matchingLine.PickLines.GetQtyCommitted(), matchingLine.WE_StockOnHand);

			var childTransfers = transfer.ChildTransfers;
			AssertEquals("Should only create 1 source child transfer.", 1, childTransfers.Count());

			var childTransfer = childTransfers.Single();
			AssertEquals("Should create a child source transfer.", TransferType.Codes.InterWhsSource, childTransfer.WD_DocketSubType);
			AssertEquals("Should create a child source transfer for whs2.", whs2.PK, childTransfer.WD_WW_Whs);
			AssertEquals("Should have a single line.", 1, childTransfer.Lines.Count);

			var childTransferLine = (WhsTransferLine)childTransfer.Lines.Single();
			AssertEquals("Should be held for transfer.", DocketLineStatus.Codes.HeldForTransfer, childTransferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should change on source line", InventoryStatus.Codes.InTransit, childTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should change on source line", InventoryStatus.Codes.InTransit, childTransferLine.WE_CurrentInventoryStatus);
			AssertEquals("Stock on hand should *not* change on source line", 0m, childTransferLine.WE_StockOnHand);

			var childTransferMatchingLine = childTransferLine.MatchingLines.Single();
			AssertEquals("Should be held for transfer.", DocketLineStatus.Codes.HeldForTransfer, childTransferMatchingLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should change on source line", InventoryStatus.Codes.InTransit, childTransferMatchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should change on source line", InventoryStatus.Codes.InTransit, childTransferMatchingLine.WE_CurrentInventoryStatus);
			AssertEquals("Stock on hand should *not* change on source line", 0m, childTransferMatchingLine.WE_StockOnHand);
			AssertNoExceptionThrown("Should have a valid datashape.", Factory.Save);
		}

		#endregion

		#endregion

		#region TestPickedTime_RecreatesInTransitInventoryWhenSettingPickedTime

		public void TestPickedTime_RecreatesInTransitInventoryWhenSettingPickedTime()
		{
			TestPickedTime_RecreatesInTransitInventoryWhenSettingPickedTime_Core(wasSaved: false);
		}

		public void TestPickedTime_RecreatesInTransitInventoryWhenSettingPickedTime_Saved()
		{
			TestPickedTime_RecreatesInTransitInventoryWhenSettingPickedTime_Core(wasSaved: true);
		}

		void TestPickedTime_RecreatesInTransitInventoryWhenSettingPickedTime_Core(bool wasSaved)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventory.LocationString, "");
			AssertEquals("Precondition", ZDateTimeOffset.Empty, transferLine.PickedTime);
			AssertEquals("Precondition", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock should be committed.", 5m, transferLine.QtyCommittedIncludingMatchingLines);

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition.", now, transferLine.PickedTime);
			AssertEquals("Precondition: Created In-Transit Inventory.", 5m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			if (wasSaved)
			{
				Factory.Save();

				// Try changing picked time
				AssertExceptionThrown("Should not allow repicking if already saved.", typeof(InvalidOperationException), "You cannot change the Picked Time of a PickLine that is already Picked in the DB.", () => transferLine.PickedTime = now.AddDays(-1));
				// NOTE: If this ever becomes valid, we must consider the possibility of inventory in the database being deleted/recreated (see Work Item WI00169232)

				// Keep picked time constant
				AssertExceptionThrown("Should not allow repicking if already saved.", typeof(InvalidOperationException), "You cannot change the Picked Time of a PickLine that is already Picked in the DB.", () => transferLine.PickedTime = transferLine.PickedTime);
				// NOTE: If this ever becomes valid, we must consider the possibility of inventory in the database being deleted/recreated (see Work Item WI00169232)
			}
			else
			{
				transferLine.WE_StockOnHand = 1m; // hack
				transferLine.PickedTime = now.AddDays(-1);
				AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
				AssertEquals("Should have recreated In-Transit Inventory.", 5m, transferLine.WE_StockOnHand);
				AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
				AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			}
		}

		#endregion

		#region TestPickedTime_RecreatesInTransitInventoryWhenSettingPickedTime_InterWhsSource

		public void TestPickedTime_RecreatesInTransitInventoryWhenSettingPickedTime_InterWhsSource()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", warehouse2.PK, "A");
			transferLine.RunPreSaveValidation();

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition.", now, transferLine.PickedTime);
			AssertEquals("Precondition.", 0m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition: Should have set inventory status on the parent.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Should have set inventory status on the parent.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var childLine = transferLine.ChildTransferLine;
			AssertEquals("Precondition: Created In-Transit Inventory.", 5m, childLine.WE_StockOnHand);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, childLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, childLine.WE_CurrentInventoryStatus);

			// It's important that we "recreate" In-Transit inventory as PickedTime, this is done by RF for splitting transfer lines for example and many tests
			transferLine.WE_TransactionQuantity = 1m; // hack
			transferLine.PickLines[0].Delete(); // Recommit pick lines on picking
			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available; // Hack to allow recommiting

			transferLine.PickedTime = now.AddDays(-1);
			AssertEquals("Data on parent should be unchanged.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Data on parent should be unchanged.", 0m, transferLine.WE_StockOnHand);
			AssertEquals("Data on parent should be unchanged.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Data on parent should be unchanged.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have resynchronised child line.", 1m, childLine.WE_TransactionQuantity);
			AssertEquals("Should have recreated In-Transit Inventory.", 1m, childLine.WE_StockOnHand);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, childLine.WE_OriginalInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, childLine.WE_CurrentInventoryStatus);
			AssertNoExceptionThrown("Should have a valid datashape.", Factory.Save);
		}

		#endregion

		#region TestPickedTime_RecreatesInTransitInventoryWhenSettingPickedTime_InterWhsDest

		public void TestPickedTime_RecreatesInTransitInventoryWhenSettingPickedTime_InterWhsDest()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, warehouse2);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", data.Whs1.PK, "A");
			transferLine.RunPreSaveValidation();

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition.", now, transferLine.PickedTime);
			AssertEquals("Precondition: Created Inventory.", 5m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition: Should have set inventory status on the parent.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Should have set inventory status on the parent.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var childLine = transferLine.ChildTransferLine;
			AssertEquals("Precondition: Child should be In-Transit, but not created inventory.", 0m, childLine.WE_StockOnHand);
			AssertEquals("Precondition: Child should be In-Transit.", InventoryStatus.Codes.InTransit, childLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Child should be In-Transit.", InventoryStatus.Codes.InTransit, childLine.WE_CurrentInventoryStatus);

			// It's important that we "recreate" In-Transit inventory as PickedTime, this is done by RF for splitting transfer lines for example and many tests
			transferLine.WE_TransactionQuantity = 1m; // hack
			transferLine.PickLines[0].Delete(); // Recommit pick lines on picking
			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available; // Hack to allow recommiting

			transferLine.PickedTime = now.AddDays(-1);
			AssertEquals("Should have recreated In-Transit Inventory.", 1m, transferLine.WE_StockOnHand);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have resynchronised child line.", 1m, childLine.WE_TransactionQuantity);
			AssertEquals("Inventory Data on child should be unchanged.", 0m, childLine.WE_StockOnHand);
			AssertEquals("Inventory Data on child should be unchanged.", InventoryStatus.Codes.InTransit, childLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory Data on child should be unchanged.", InventoryStatus.Codes.InTransit, childLine.WE_CurrentInventoryStatus);
			AssertNoExceptionThrown("Should have a valid datashape.", Factory.Save);
		}

		#endregion

		#region TestPickedTime_RemovesInTransitInventoryWhenUnpicking

		public void TestPickedTime_RemovesInTransitInventoryWhenUnpicking()
		{
			TestPickedTime_RemovesInTransitInventoryWhenUnpicking(wasSaved: false);
		}

		public void TestPickedTime_RemovesInTransitInventoryWhenUnpicking_Saved()
		{
			TestPickedTime_RemovesInTransitInventoryWhenUnpicking(wasSaved: true);
		}

		void TestPickedTime_RemovesInTransitInventoryWhenUnpicking(bool wasSaved)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventory.LocationString, "");
			AssertEquals("Precondition", ZDateTimeOffset.Empty, transferLine.PickedTime);
			AssertEquals("Precondition", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock should be committed.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition.", now, transferLine.PickedTime);
			AssertEquals("Precondition: Created In-Transit Inventory.", 10m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			if (wasSaved)
			{
				Factory.Save();

				AssertExceptionThrown("Should not allow unpicking if already saved.", typeof(InvalidOperationException), "You cannot change the Picked Time of a PickLine that is already Picked in the DB.", () => transferLine.PickedTime = ZDateTimeOffset.Empty);
				// NOTE: If this ever becomes valid, we must consider the possibility of inventory in the database being deleted/recreated (see Work Item WI00169232)
			}
			else
			{
				transferLine.PickedTime = ZDateTimeOffset.Empty;
				AssertEquals("Precondition.", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
				AssertEquals("Unpicking should have removed In-Transit Inventory.", 0m, transferLine.WE_StockOnHand);
				AssertEquals("Unpicking should have removed In-Transit Inventory.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
				AssertEquals("Unpicking should have removed In-Transit Inventory.", InventoryStatus.Codes.Available, transferLine.WE_CurrentInventoryStatus);
			}
		}

		#endregion

		#region TestPickedTime_RemovesInTransitInventoryWhenUnpicking_InterWhsSource

		public void TestPickedTime_RemovesInTransitInventoryWhenUnpicking_InterWhsSource()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", warehouse2.PK, "A");
			transferLine.RunPreSaveValidation();

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition.", now, transferLine.PickedTime);
			AssertEquals("Precondition.", 0m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition: Should have set inventory status on the parent.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Should have set inventory status on the parent.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var childLine = transferLine.ChildTransferLine;
			AssertEquals("Precondition: Created In-Transit Inventory.", 5m, childLine.WE_StockOnHand);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, childLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, childLine.WE_CurrentInventoryStatus);

			transferLine.PickedTime = ZDateTimeOffset.Empty;
			AssertEquals("Should have changed docket line status.", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			AssertEquals("Unpicking should have removed In-Transit Inventory Status.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Unpicking should have removed In-Transit Inventory Status.", InventoryStatus.Codes.Available, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have deleted the child line.", true, childLine.IsDeleted);
		}

		#endregion

		#region TestPickedTime_RemovesInTransitInventoryWhenUnpicking_InterWhsDest

		public void TestPickedTime_RemovesInTransitInventoryWhenUnpicking_InterWhsDest()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, warehouse2);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", data.Whs1.PK, "A");
			transferLine.RunPreSaveValidation();

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition.", now, transferLine.PickedTime);
			AssertEquals("Precondition: Created Inventory.", 5m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition: Should have set inventory status on the parent.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Should have set inventory status on the parent.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var childLine = transferLine.ChildTransferLine;
			AssertEquals("Precondition: Child should be In-Transit, but not created inventory.", 0m, childLine.WE_StockOnHand);
			AssertEquals("Precondition: Child should be In-Transit.", InventoryStatus.Codes.InTransit, childLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Child should be In-Transit.", InventoryStatus.Codes.InTransit, childLine.WE_CurrentInventoryStatus);

			transferLine.PickedTime = ZDateTimeOffset.Empty;
			AssertEquals("Should have changed docket line status.", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			AssertEquals("Unpicking should have removed In-Transit Inventory.", 0m, transferLine.WE_StockOnHand);
			AssertEquals("Unpicking should have removed In-Transit Inventory.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Unpicking should have removed In-Transit Inventory.", InventoryStatus.Codes.Available, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have deleted the child line.", true, childLine.IsDeleted);
		}

		#endregion

		#region TestPickedTime_RemovesInTransitInventoryWhenUnpicking_Held

		public void TestPickedTime_RemovesInTransitInventoryWhenUnpicking_Held()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var inventoryLine = receive.Lines[0];
			inventoryLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			inventoryLine.ChangeInventoryHeldCode(true);
			Factory.Save();
			AssertEquals("Precondition.", InventoryStatus.Codes.Held, inventoryLine.WE_CurrentInventoryStatus);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventoryLine.LocationString, "", InventoryHoldCodes.Codes.Held);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, transferLine.PickedTime);
			AssertEquals("Precondition", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Held, transferLine.WE_OriginalInventoryStatus);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock should be committed.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition.", now, transferLine.PickedTime);
			AssertEquals("Precondition: Created In-Transit Inventory.", 10m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			transferLine.PickedTime = ZDateTimeOffset.Empty;
			AssertEquals("Precondition.", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			AssertEquals("Unpicking should have removed In-Transit Inventory.", 0m, transferLine.WE_StockOnHand);
			AssertEquals("Unpicking should have removed In-Transit Inventory.", InventoryStatus.Codes.Held, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Unpicking should have removed In-Transit Inventory.", InventoryStatus.Codes.Held, transferLine.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestPickedTime_PutawayTransferLineInventoryStatusUpdatedToPuttingAway

		public void TestPickedTime_PutawayTransferLineInventoryStatusUpdatedToPuttingAway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, dockDoorLocation, "12345");
			inventory1.WI_ArrivalDate = ZDateTimeOffset.Now.AddDays(-2);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "12345", 10m);
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Precondition", ZDateTimeOffset.Empty, transferLine.PickedTime);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, transferLine.WE_CurrentInventoryStatus);
			Assert("Precondition", transferLine.IsPutawayTransferLine);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("WE_OriginalInventoryStatus is PUTTING AWAY.", InventoryStatus.Codes.PuttingAway, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("WE_CurrentInventoryStatus is PUTTING AWAY.", InventoryStatus.Codes.PuttingAway, transferLine.WE_CurrentInventoryStatus);
		}

		#endregion

		#endregion

		#region TestPickedBy

		public void TestPickedBy()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, sourceLocation, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation, destinationLocation);
			AssertEquals("Precondition", ZString.Empty, line.GS_NKPickedBy);

			line.GS_NKPickedBy = "AAA";
			AssertEquals("Precondition : Picklines should not be created.", 0, line.PickLines.Count);
			AssertEquals("It should return picked by eventhough code is invalid.", "AAA", line.GS_NKPickedBy);

			line.GS_NKPickedBy = staff1.GS_Code;
			AssertEquals("Precondition : Picklines should not be created.", 0, line.PickLines.Count);
			AssertEquals("It should return picked by eventhough picklines are not avaialable.", staff1.GS_Code, line.GS_NKPickedBy);

			transfer.RunPreSaveValidation();
			Factory.Save();
			var pickLine = line.PickLines.Single();
			AssertEquals("Transfer line Picked By should not be changed.", staff1.GS_Code, line.GS_NKPickedBy);
			AssertEquals("Pick line Picked By should not be changed.", staff1.GS_Code, pickLine.WZ_GS_NKAssignedTo);
			AssertEquals(staff1, pickLine.AssignedTo);

			line.GS_NKPickedBy = "BBB";
			AssertEquals("BBB", line.GS_NKPickedBy);
			AssertEquals("BBB", pickLine.WZ_GS_NKAssignedTo);
			AssertNull(pickLine.AssignedTo);

			line.GS_NKPickedBy = staff2.GS_Code;
			AssertEquals(staff2.GS_Code, line.GS_NKPickedBy);
			AssertEquals(staff2.GS_Code, pickLine.WZ_GS_NKAssignedTo);
			AssertEquals(staff2, pickLine.AssignedTo);

			line.GS_NKPickedBy = "";
			AssertEquals("", line.GS_NKPickedBy);
			AssertEquals("", pickLine.WZ_GS_NKAssignedTo);
			AssertNull(pickLine.AssignedTo);

			line.GS_NKPickedBy = staff2.GS_Code;
			AssertEquals(staff2.GS_Code, line.GS_NKPickedBy);
			AssertEquals(staff2.GS_Code, pickLine.WZ_GS_NKAssignedTo);
			AssertEquals(staff2, pickLine.AssignedTo);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);
			var transferLineInNewFactory = (WhsTransferLine)transferInNewFactory.Lines.Single();
			var pickLineInAnotherFactory = transferLineInNewFactory.PickLines.Single();
			AssertEquals("Transfer line Picked By in new factory should be same as in old factory.", staff2.GS_Code, transferLineInNewFactory.GS_NKPickedBy);
			AssertEquals("Pick line Picked By in new factory should be same as in old factory.", staff2.GS_Code, pickLineInAnotherFactory.WZ_GS_NKAssignedTo);

			transferLineInNewFactory.GS_NKPickedBy = staff1.GS_Code;
			AssertEquals(staff1.GS_Code, transferLineInNewFactory.GS_NKPickedBy);
			AssertEquals(staff1.GS_Code, pickLineInAnotherFactory.WZ_GS_NKAssignedTo);

			transferLineInNewFactory.GS_NKPickedBy = "CCC";
			AssertEquals("CCC", transferLineInNewFactory.GS_NKPickedBy);
			AssertEquals("CCC", pickLineInAnotherFactory.WZ_GS_NKAssignedTo);
			AssertNull(pickLineInAnotherFactory.AssignedTo);
		}

		public void TestPickedBy_OnChildTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("WH2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			transferLine.DestinationWarehousePK = warehouse2.PK;
			var childTransferLine = transferLine.Clone<WhsTransferLine>();
			childTransferLine.WE_WE_ParentDocketLine = transferLine.PK;
			AssertEquals("Precondition: Picked By should be empty.", "", childTransferLine.GS_NKPickedBy);

			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot set Picked By on Child Transfer Line.", () => childTransferLine.GS_NKPickedBy = "AA");
			AssertEquals("Picked By should be empty when trying to set Child Transfer Line.", "", childTransferLine.GS_NKPickedBy);

			transferLine.GS_NKPickedBy = "AA";
			AssertEquals("Picked By on Child Transfer Line should be the same as the Master Transfer Line.", "AA", childTransferLine.GS_NKPickedBy);

			childTransferLine.WE_WE_ParentDocketLine = ZGuid.Empty;
			AssertEquals("Picked By on Child Transfer Line should empty if Master Transfer Line does not exist.", "", childTransferLine.GS_NKPickedBy);
		}

		#endregion

		#region TestPickedByInfo

		public void TestPickedByInfo()
		{
			TestReadOnly(dl => dl.GS_NKPickedByInfo, false, false, true, true, (_, n, d, dl) => additionalAssertions(n, d, dl));

			void additionalAssertions(string name, WhsTransfer docket, WhsTransferLine docketLine)
			{
				docket.WD_DocketStatus = DocketStatus.Codes.Held;
				AssertEquals(false, docketLine.GS_NKPickedByInfo.ReadOnly);
			}
		}

		#endregion

		#region TestIsPickerPicking

		public void TestIsPickerPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, receive.Inventory[0].LocationString, "");
			transfer.RunPreSaveValidation(); // to commit inventory.
			Factory.Save();
			AssertEquals("Precondition", false, transferLine.IsPickerPicking);
			AssertEquals("Precondition", false, transferLine.PickLines[0].WZ_IsPicking);

			transferLine.PickLines[0].WZ_IsPicking = true;
			AssertEquals(true, transferLine.IsPickerPicking);
			AssertEquals(true, transferLine.PickLines[0].WZ_IsPicking);
		}

		#endregion

		#region TestDestinationWarehousePK_SetsAllMatchingLines

		public void TestDestinationWarehousePK_SetsAllMatchingLines()
		{
			var whs1 = Helper.CreateWarehouse("WH1");
			var whs2 = Helper.CreateWarehouse("WH2");
			AssertSetPropertySetsPropertyOnAllMatchingLines(WhsTransferLine.Schema.DestinationWarehousePK, whs1.PK, whs2.PK);
		}

		#endregion

		#region TestWE_PalletID

		public void TestWE_PalletID_SetMatchingLines()
		{
			AssertSetPropertySetsPropertyOnAllMatchingLines(WhsTransferLine.Schema.WE_PalletID, new ZString("PLT-1"), new ZString("PLT-2"));
		}

		#endregion

		#region TestWE_TransferFromPalletId

		public void TestWE_TransferFromPalletId()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var docketLine = GetNewBusinessObject();
			var docketLineInterface = (IWhsTransferLine)docketLine;
			docketLine.WE_TransferFromPalletId = "TEST";
			AssertEquals("TEST", docketLine.WE_TransferFromPalletId);
			AssertEquals("TEST", docketLineInterface.WE_TransferFromPalletId);
		}

		#endregion

		#region TestWE_PutawayTime

		public void TestWE_PutawayTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventory.LocationString, "");
			AssertEquals("Precondition", ZDateTimeOffset.Empty, transferLine.WE_PutawayTime);
			AssertEquals("Precondition", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);

			transferLine.WE_PutawayTime = ZDateTimeOffset.Empty;
			AssertEquals(DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			transferLine.WE_PutawayTime = ZDateTimeOffset.Empty;
			AssertEquals(DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			AssertEquals(DocketLineStatus.Codes.Finalised, transferLine.WE_DocketLineStatus);

			// Don't have to handle below case assuming an exception is thrown
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot set PutawayTime if not finalising the Transfer Line.", () => transferLine.WE_PutawayTime = ZDateTimeOffset.Now);
		}

		[TestDate(2018, 1, 1, 16, 10, 55)]
		public void TestWE_PutawayTime_HasGotSeconds()
		{
			// TestDate attribute will be used as a WE_PutawayTime for transferLine
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 14m, locationA1, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 14m, "A-1", "A-2");
			transferLine.FinaliseDocketLine();
			Factory.Save();

			// load transferLine from a new factory
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transferLineInOtherFactory = newFactory.Load<WhsTransferLine>(transferLine.PK);

			var dateTime = new ZDateTimeOffset(2018, 1, 1, 16, 10, 55);
			AssertEquals("WE_PutawayTime is incorrect.", dateTime, transferLineInOtherFactory.WE_PutawayTime);
		}

		#endregion

		#region TestWE_PutawayTime_SetsPutawayBy

		[TestDate(2016, 1, 2)]
		public void TestWE_PutawayTime_SetsPutawayBy()
		{
			var staff = Helper.CreateGlbStaff("AA", "Antman");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, locationA1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, locationA1, locationA2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: Should be a matching line.", 1, transferLine1.MatchingLines.Count);

			AssertPutawayTimeAndPutawayBy(transferLine1, ZDateTimeOffset.Empty, ""); // Precondition
			AssertPutawayTimeAndPutawayBy(transferLine2, ZDateTimeOffset.Empty, ""); // Precondition

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				// Set putaway time with no putaway by entered, should default putaway
				using (new SemaphoreManager(transferLine1.FinaliseDocketLineSemaphore))
				{
					transferLine1.WE_PutawayTime = ZDateTimeOffset.Now;
				}
				AssertPutawayTimeAndPutawayBy(transferLine1, transferLine1.WE_PutawayTime, "AA");

				// Set putway time with putaway by entered, should *not* change putaway by
				transferLine2.WE_GS_NKPutawayBy = "~BP";
				AssertPutawayTimeAndPutawayBy(transferLine2, ZDateTimeOffset.Empty, "~BP"); // Precondition

				using (new SemaphoreManager(transferLine2.FinaliseDocketLineSemaphore))
				{
					transferLine2.WE_PutawayTime = ZDateTimeOffset.Now.AddDays(1);
				}
				AssertPutawayTimeAndPutawayBy(transferLine2, transferLine2.WE_PutawayTime, "~BP");

				// This is not possible on the GUI currently, but I feel it should logically be handled
				// Clearing PutawayTime should not set PutawayBy
				transferLine2.WE_GS_NKPutawayBy = "";
				AssertPutawayTimeAndPutawayBy(transferLine2, transferLine2.WE_PutawayTime, ""); // Precondition

				using (new SemaphoreManager(transferLine2.FinaliseDocketLineSemaphore))
				{
					transferLine2.WE_PutawayTime = ZDateTimeOffset.Empty;
				}
				AssertPutawayTimeAndPutawayBy(transferLine2, ZDateTimeOffset.Empty, "");
			}
		}

		#endregion

		#region TestWE_PutawayTime_SetsAllMatchingLines

		public void TestWE_PutawayTime_SetsAllMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 8m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 4m, locationA1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 14m, "A-1", "A-2");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 14m, transferLine.QtyCommittedIncludingMatchingLines);

			var matchingLine1 = transferLine.MatchingLines[0];
			var matchingLine2 = transferLine.MatchingLines[1];

			transferLine.FinaliseDocketLine();

			AssertEquals("Main transfer line and matching line should have the same putaway time.", transferLine.WE_PutawayTime, matchingLine1.WE_PutawayTime);
			AssertEquals("Main transfer line and matching line should have the same putaway time.", transferLine.WE_PutawayTime, matchingLine2.WE_PutawayTime);
		}

		#endregion

		#region TestWE_PutawayTime_CannotBeSetOnFinalisedLines

		[TestDate(2016, 10, 20)]
		public void TestWE_PutawayTime_CannotBeSetOnFinalisedLines()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", now, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", now, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			var matchingLine = (WhsTransferLine)transferLine.MatchingLines.Single();
			AssertNotNull("Precondition: Should have a matching line.", matchingLine);

			AssertEquals("Precondition - PutawayTime must not be set so it is defaulted during Finalisation.", false, transferLine.IsPutaway);
			AssertEquals("Precondition - PutawayTime must not be set so it is defaulted during Finalisation.", false, matchingLine.IsPutaway);
			AssertNoExceptionThrown("Should not throw exceptions during finalisation.", transferLine.FinaliseDocketLine);
			AssertEquals("Should have set PutawayTime.", true, transferLine.IsPutaway);
			AssertEquals("Should have set PutawayTime.", true, matchingLine.IsPutaway);

			// Master Line
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot set PutawayTime if not finalising the Transfer Line.", () => transferLine.WE_PutawayTime = now.AddDays(1));
			AssertEquals("Cannot change PutwayTime of Finalised Transfer Line.", now, transferLine.WE_PutawayTime);

			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot set PutawayTime if not finalising the Transfer Line.", () => transferLine.WE_PutawayTime = ZDateTimeOffset.Empty);
			AssertEquals("Cannot change PutwayTime of Finalised Transfer Line.", now, transferLine.WE_PutawayTime);

			// Matching Line
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot set PutawayTime if not finalising the Transfer Line.", () => matchingLine.WE_PutawayTime = now.AddDays(1));
			AssertEquals("Cannot change PutwayTime of Finalised Transfer Line.", now, matchingLine.WE_PutawayTime);

			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot set PutawayTime if not finalising the Transfer Line.", () => matchingLine.WE_PutawayTime = ZDateTimeOffset.Empty);
			AssertEquals("Cannot change PutwayTime of Finalised Transfer Line.", now, matchingLine.WE_PutawayTime);
		}

		#endregion

		#region TestLocalisedPutawayTime

		public void TestLocalisedPutawayTime()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchAUSYD = company.Branches.AddNew();
			branchAUSYD.GB_Code = "AUS";
			branchAUSYD.GB_RL_NKHomePort = "AUSYD";

			var branchUSNYC = company.Branches.AddNew();
			branchUSNYC.GB_Code = "NYC";
			branchUSNYC.GB_RL_NKHomePort = "USNYC";
			Factory.Save();

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 14m, locationA1, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 14m, "A-1", "A-2");
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var putawayTime = transferLine.WE_PutawayTime;

			ZDateTime putawayTimeAUSYD;
			ZDateTime putawayTimeUSNYC;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchAUSYD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				putawayTimeAUSYD = putawayTime.ToLocalZDateTime();
				AssertEquals(putawayTimeAUSYD, transferLine.LocalisedPutawayTime);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchUSNYC.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				putawayTimeUSNYC = putawayTime.ToLocalZDateTime();
				AssertEquals(putawayTimeUSNYC, transferLine.LocalisedPutawayTime);
			}

			AssertNotEquals("Localised times should represent their respective branches timezone.", putawayTimeAUSYD, putawayTimeUSNYC);
		}

		#endregion

		#region TestGS_NKPickedBy

		public void TestGS_NKPickedBy()
		{
			var staff1 = Helper.CreateGlbStaff("AAA", "A.A");
			var staff2 = Helper.CreateGlbStaff("BBB", "B.B");
			AssertSetPropertySetsPropertyOnAllMatchingLines(WhsTransferLine.Schema.GS_NKPickedBy, new ZString("AAA"), new ZString("BBB"));
		}

		#endregion

		#region TestWE_GS_NKPutawayBy

		public void TestWE_GS_NKPutawayBy()
		{
			var staff1 = Helper.CreateGlbStaff("AAA", "A.A");
			var staff2 = Helper.CreateGlbStaff("BBB", "B.B");
			AssertSetPropertySetsPropertyOnAllMatchingLines(WhsTransferLine.Schema.WE_GS_NKPutawayBy, new ZString("AAA"), new ZString("BBB"));
		}

		#endregion

		#region TestLineComment

		public void TestWE_LineComment()
		{
			AssertSetPropertySetsPropertyOnAllMatchingLines(WhsTransferLine.Schema.WE_LineComment, new ZString("Comment 1"), new ZString("Comment 2"));
		}

		#endregion

		#region TestWE_WL_SetsAllMatchingLines

		public void TestWE_WL_SetsAllMatchingLines()
		{
			var warehouse = Helper.CreateWarehouse("Warehouse", "B", 2, 1);
			Factory.Save();
			AssertSetPropertySetsPropertyOnAllMatchingLines(WhsDocketLineSchema.Constants.WE_WL, warehouse.FindLocation("B-1").PK, warehouse.FindLocation("B-2").PK);
		}

		#endregion

		#region TestWE_CurrentInventoryStatus && TestWE_OriginalInventoryStatus

		#region TestInventoryStatus_Constraint_InTransitInventoryAllowedForTransfers

		[UseSnapshotProtection]
		public void TestInventoryStatus_Constraint_InTransitInventoryAllowedForTransfers()
		{
			// Need to ensure a tighter snapshot/rollback mechanism than what is
			// provided by the static Factory object
			using (var secondConnection = Db.NewExtraConnectionToMainDb())
			{
				var localFactory = new BusinessObjectFactory(secondConnection);
				var localHelper = new WhsTestHelperFunctions(localFactory);
				var data = new TestDataSimpleEnvironment(localFactory, 2, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				var locationA2 = data.Whs1.FindLocation("A-2");

				var receive = localHelper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				var inventoryLine = localHelper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1).InDocketLine;

				localFactory.Save();

				inventoryLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.InTransit; // will also change WE_CurrentInventoryStatus
				AssertExceptionThrown("In-Transit inventory status is only applicable to not finalised transfer lines, therefore should fail if applied to receive lines.", typeof(ZSaveException), () => localFactory.Save());

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				localFactory.Save();

				var transfer = localHelper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
				var transferLine = localHelper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
				transferLine.RunPreSaveValidation(); // to commit inventory
				transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.InTransit; // will also change WE_CurrentInventoryStatus
				AssertNoExceptionThrown("Not finalised transfer lines can have in-transit status.", () => localFactory.Save());
			}
		}

		#endregion

		#region TestInventoryStatus_Constraint_InTransitInventoryIsNotAllowedForFinalisedTransferLines

		public void TestWE_CurrentInventoryStatus_Constraint_InTransitInventoryIsNotAllowedForFinalisedTransferLines()
		{
			TestInventoryStatus_Constraint_InTransitInventoryIsNotAllowedForFinalisedTransferLines((transferLinePK) => WhsDocketLineDO.UpdateWhere(transferLinePK.ToGuid()).Set(l => l.WE_CurrentInventoryStatus, InventoryStatus.Codes.InTransit).Post(TestConnection));
		}

		public void TestWE_OriginalInventoryStatus_Constraint_InTransitInventoryIsNotAllowedForFinalisedTransferLines()
		{
			TestInventoryStatus_Constraint_InTransitInventoryIsNotAllowedForFinalisedTransferLines((transferLinePK) => WhsDocketLineDO.UpdateWhere(transferLinePK.ToGuid()).Set(l => l.WE_OriginalInventoryStatus, InventoryStatus.Codes.InTransit).Post(TestConnection));
		}

		void TestInventoryStatus_Constraint_InTransitInventoryIsNotAllowedForFinalisedTransferLines(Action<ZGuid> insertData)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.FinaliseDocketLine();
			Factory.Save();
			AssertIsFinalisedPrecondition(transferLine);

			AssertExceptionThrown("In-Transit inventory status is only applicable to NOT finalised transfer lines.", typeof(SqlException),
				() => insertData(transferLine.PK));
		}

		#endregion

		#endregion

		#region TestQtyToMoveIncludingMatchingLines

		public void TestQtyToMoveIncludingMatchingLines()
		{
			AssertQtyIncludingMatchingLines(WhsTransferLine.Schema.QtyToMoveIncludingMatchingLines, WhsTransferLine.Schema.WE_TransactionQuantity);
		}

		void AssertQtyIncludingMatchingLines(ZString groupPropertyName, ZString linePropertyName)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 0m, "", "");
			var matchingLine1 = Helper.CreateMatchingLine(line, 0m);
			var matchingLine2 = Helper.CreateMatchingLine(line, 0m);
			var lineQtyIncludingMatchingLineInfo = line.ZPropertyInfoHash[groupPropertyName];
			var lineQtyInfo = line.ZPropertyInfoHash[linePropertyName];
			var matchingLine1Info = matchingLine1.ZPropertyInfoHash[linePropertyName];
			var matchingLine2Info = matchingLine2.ZPropertyInfoHash[linePropertyName];

			AssertEquals("Precondition", 0m, lineQtyIncludingMatchingLineInfo.Value);

			// test getter
			lineQtyInfo.Value = (ZDecimal)2m;
			AssertEquals(string.Format("{0} for main line should be coming from Main Transfer Line and matching lines.", groupPropertyName), 2m, lineQtyIncludingMatchingLineInfo.Value);
			AssertEquals(string.Format("Modifying {0} on main line should not modify matching lines.", linePropertyName), 0m, matchingLine1Info.Value);
			AssertEquals(string.Format("Modifying {0} on main line should not modify matching lines.", linePropertyName), 0m, matchingLine2Info.Value);

			matchingLine1Info.Value = (ZDecimal)3m;
			matchingLine2Info.Value = (ZDecimal)4m;
			AssertEquals(string.Format("{0} for main line should be coming from Main Transfer Line and matching lines.", groupPropertyName), 9m, lineQtyIncludingMatchingLineInfo.Value);
			AssertEquals(string.Format("Setting {0} should not modify other matching lines.", linePropertyName), 2m, lineQtyInfo.Value);
			AssertEquals(string.Format("Setting {0} should not modify other matching lines.", linePropertyName), 3m, matchingLine1Info.Value);
			AssertEquals(string.Format("Setting {0} should not modify other matching lines.", linePropertyName), 4m, matchingLine2Info.Value);

			// test setter
			lineQtyIncludingMatchingLineInfo.Value = (ZDecimal)9m;
			AssertEquals(string.Format("Setting {0} to same value for main line should *NOT* change the quantity.", groupPropertyName), 9m, lineQtyIncludingMatchingLineInfo.Value);
			AssertEquals(string.Format("Setting {0} for main line that is equal to previous quantity should not modify main line.", groupPropertyName), 2m, lineQtyInfo.Value);
			AssertEquals(string.Format("Setting {0} for main line that is equal to previous quantity should not modify main line.", groupPropertyName), 3m, matchingLine1Info.Value);
			AssertEquals(string.Format("Setting {0} for main line that is equal to previous quantity should not modify main line.", groupPropertyName), 4m, matchingLine2Info.Value);

			lineQtyIncludingMatchingLineInfo.Value = (ZDecimal)12m;
			AssertEquals(string.Format("Increasing {0} to different value for main line should properly modify the field.", groupPropertyName), 12m, lineQtyIncludingMatchingLineInfo.Value);
			AssertEquals(string.Format("Increasing {0} should increate {1} on main line only.", groupPropertyName, linePropertyName), 5m, lineQtyInfo.Value);
			AssertEquals(string.Format("Increasing {0} should increate {1} on main line only.", groupPropertyName, linePropertyName), 3m, matchingLine1Info.Value);
			AssertEquals(string.Format("Increasing {0} should increate {1} on main line only.", groupPropertyName, linePropertyName), 4m, matchingLine2Info.Value);

			lineQtyIncludingMatchingLineInfo.Value = (ZDecimal)10m;
			AssertEquals(string.Format("Decreasing {0} to different value for main line should work.", groupPropertyName), 10m, lineQtyIncludingMatchingLineInfo.Value);
			AssertEquals(string.Format("Decreasing {0} should decrease qty from matching lines first.", groupPropertyName), 5m, lineQtyInfo.Value);
			AssertEquals(string.Format("Decreasing {0} should decrease qty from matching lines first.", groupPropertyName), 5m, (ZDecimal)matchingLine1Info.Value + (ZDecimal)matchingLine2Info.Value); // could be either line

			lineQtyIncludingMatchingLineInfo.Value = (ZDecimal)(-5m);
			AssertEquals(string.Format("Decreasing {0} below amount of main line should delete matching lines and reduce main line {1}.", groupPropertyName, linePropertyName), -5m, lineQtyIncludingMatchingLineInfo.Value);
			AssertEquals(string.Format("Decreasing {0} below amount of main line should delete matching lines and reduce main line {1}.", groupPropertyName, linePropertyName), -5m, lineQtyInfo.Value);
			AssertEquals(string.Format("Decreasing {0} below amount of main line should delete matching lines and reduce main line {1}.", groupPropertyName, linePropertyName), true, matchingLine1.IsDeleted);
			AssertEquals(string.Format("Decreasing {0} below amount of main line should delete matching lines and reduce main line {1}.", groupPropertyName, linePropertyName), true, matchingLine2.IsDeleted);
		}

		#endregion

		#region TestPackQtyIncludingMatchingLines

		public void TestPackQtyIncludingMatchingLines()
		{
			AssertQtyIncludingMatchingLines(WhsTransferLine.Schema.PackQtyIncludingMatchingLines, WhsTransferLine.Schema.WE_PackQuantity);
		}

		#endregion

		#region TestQtyCommittedIncludingMatchingLines

		public void TestQtyCommittedIncludingMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, data.Whs1.FindLocation("A"), data.Whs1.FindLocation("A"));
			var line2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, data.Whs1.FindLocation("A"), data.Whs1.FindLocation("A"));
			var line3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 4m, data.Whs1.FindLocation("A"), data.Whs1.FindLocation("A"));
			line1.MatchingLines.Add(line2);
			line1.MatchingLines.Add(line3);
			AssertEquals("Precondition", 0m, line1.QtyCommittedIncludingMatchingLines);

			Helper.CreateWhsPickLine(line1, inventory, 1m);
			Helper.CreateWhsPickLine(line1, inventory, 2m);
			AssertEquals("Committed Qty should be the sum of picklines from the main transfer line & matching transfer lines.", 3m, line1.QtyCommittedIncludingMatchingLines);

			Helper.CreateWhsPickLine(line2, inventory, 3m);
			Helper.CreateWhsPickLine(line3, inventory, 4m);
			AssertEquals("Committed Qty should be the sum of picklines from the main transfer line & matching transfer lines.", 10m, line1.QtyCommittedIncludingMatchingLines);
		}

		#endregion

		#region TestCustomAttributes

		public void TestCustomAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			var line3 = transfer.Lines.AddNew();
			line1.MatchingLines.Add(line2);
			line1.MatchingLines.Add(line3);
			AssertCustomAttributesAreEmpty(line1);

			var date = ZDateTime.Today;
			line1.WE_CustomAttrib1 = "CA1";
			line1.WE_CustomAttrib2 = "CA2";
			line1.WE_CustomAttrib3 = "CA3";
			line1.WE_CustomAttrib4 = "CA4";
			line1.WE_CustomAttrib5 = "CA5";
			line1.WE_CustomAttrib6 = "CA6";
			line1.WE_CustomDate1 = date.AddDays(1);
			line1.WE_CustomDate2 = date.AddDays(2);
			line1.WE_CustomDate3 = date.AddDays(3);
			line1.WE_CustomDate4 = date.AddDays(4);
			line1.WE_CustomDate5 = date.AddDays(5);
			line1.WE_CustomDecimal1 = 1m;
			line1.WE_CustomDecimal2 = 2m;
			line1.WE_CustomDecimal3 = 3m;
			line1.WE_CustomDecimal4 = 4m;
			line1.WE_CustomDecimal5 = 5m;
			line1.WE_CustomFlag1 = true;
			line1.WE_CustomFlag2 = true;
			line1.WE_CustomFlag3 = true;
			line1.WE_CustomFlag4 = true;
			line1.WE_CustomFlag5 = true;
			line1.WE_CustomTextBlob1 = "BLOB-1";
			AssertCustomAttributesHaveValues(line1, date);
			AssertCustomAttributesHaveValues(line2, date);
			AssertCustomAttributesHaveValues(line3, date);
		}

		void AssertCustomAttributesAreEmpty(WhsTransferLine transferLine)
		{
			AssertEquals("Modifying CustomAttrib1 on main line should not modify matching lines.", "", transferLine.WE_CustomAttrib1);
			AssertEquals("Modifying CustomAttrib2 on main line should not modify matching lines.", "", transferLine.WE_CustomAttrib2);
			AssertEquals("Modifying CustomAttrib3 on main line should not modify matching lines.", "", transferLine.WE_CustomAttrib3);
			AssertEquals("Modifying CustomAttrib4 on main line should not modify matching lines.", "", transferLine.WE_CustomAttrib4);
			AssertEquals("Modifying CustomAttrib5 on main line should not modify matching lines.", "", transferLine.WE_CustomAttrib5);
			AssertEquals("Modifying CustomAttrib6 on main line should not modify matching lines.", "", transferLine.WE_CustomAttrib6);
			AssertEquals("Modifying CustomDate1 on main line should not modify matching lines.", ZDateTime.Empty, transferLine.WE_CustomDate1);
			AssertEquals("Modifying CustomDate2 on main line should not modify matching lines.", ZDateTime.Empty, transferLine.WE_CustomDate2);
			AssertEquals("Modifying CustomDate3 on main line should not modify matching lines.", ZDateTime.Empty, transferLine.WE_CustomDate3);
			AssertEquals("Modifying CustomDate4 on main line should not modify matching lines.", ZDateTime.Empty, transferLine.WE_CustomDate4);
			AssertEquals("Modifying CustomDate5 on main line should not modify matching lines.", ZDateTime.Empty, transferLine.WE_CustomDate5);
			AssertEquals("Modifying CustomDecimal1 on main line should not modify matching lines.", 0m, transferLine.WE_CustomDecimal1);
			AssertEquals("Modifying CustomDecimal2 on main line should not modify matching lines.", 0m, transferLine.WE_CustomDecimal2);
			AssertEquals("Modifying CustomDecimal3 on main line should not modify matching lines.", 0m, transferLine.WE_CustomDecimal3);
			AssertEquals("Modifying CustomDecimal4 on main line should not modify matching lines.", 0m, transferLine.WE_CustomDecimal4);
			AssertEquals("Modifying CustomDecimal5 on main line should not modify matching lines.", 0m, transferLine.WE_CustomDecimal5);
			AssertEquals("Modifying CustomFlag1 on main line should not modify matching lines.", false, transferLine.WE_CustomFlag1);
			AssertEquals("Modifying CustomFlag2 on main line should not modify matching lines.", false, transferLine.WE_CustomFlag2);
			AssertEquals("Modifying CustomFlag3 on main line should not modify matching lines.", false, transferLine.WE_CustomFlag3);
			AssertEquals("Modifying CustomFlag4 on main line should not modify matching lines.", false, transferLine.WE_CustomFlag4);
			AssertEquals("Modifying CustomFlag5 on main line should not modify matching lines.", false, transferLine.WE_CustomFlag5);
			AssertEquals("Modifying CustomTextBlob1 on main line should not modify matching lines.", "", transferLine.WE_CustomTextBlob1);
		}

		void AssertCustomAttributesHaveValues(WhsTransferLine transferLine, ZDateTime date)
		{
			AssertEquals("Modifying CustomAttrib1 on group should modify all matching transfer lines.", "CA1", transferLine.WE_CustomAttrib1);
			AssertEquals("Modifying CustomAttrib2 on group should modify all matching transfer lines.", "CA2", transferLine.WE_CustomAttrib2);
			AssertEquals("Modifying CustomAttrib3 on group should modify all matching transfer lines.", "CA3", transferLine.WE_CustomAttrib3);
			AssertEquals("Modifying CustomAttrib4 on group should modify all matching transfer lines.", "CA4", transferLine.WE_CustomAttrib4);
			AssertEquals("Modifying CustomAttrib5 on group should modify all matching transfer lines.", "CA5", transferLine.WE_CustomAttrib5);
			AssertEquals("Modifying CustomAttrib6 on group should modify all matching transfer lines.", "CA6", transferLine.WE_CustomAttrib6);
			AssertEquals("Modifying CustomDate1 on group should modify all matching transfer lines.", date.AddDays(1), transferLine.WE_CustomDate1);
			AssertEquals("Modifying CustomDate2 on group should modify all matching transfer lines.", date.AddDays(2), transferLine.WE_CustomDate2);
			AssertEquals("Modifying CustomDate3 on group should modify all matching transfer lines.", date.AddDays(3), transferLine.WE_CustomDate3);
			AssertEquals("Modifying CustomDate4 on group should modify all matching transfer lines.", date.AddDays(4), transferLine.WE_CustomDate4);
			AssertEquals("Modifying CustomDate5 on group should modify all matching transfer lines.", date.AddDays(5), transferLine.WE_CustomDate5);
			AssertEquals("Modifying CustomDecimal1 on group should modify all matching transfer lines.", 1m, transferLine.WE_CustomDecimal1);
			AssertEquals("Modifying CustomDecimal2 on group should modify all matching transfer lines.", 2m, transferLine.WE_CustomDecimal2);
			AssertEquals("Modifying CustomDecimal3 on group should modify all matching transfer lines.", 3m, transferLine.WE_CustomDecimal3);
			AssertEquals("Modifying CustomDecimal4 on group should modify all matching transfer lines.", 4m, transferLine.WE_CustomDecimal4);
			AssertEquals("Modifying CustomDecimal5 on group should modify all matching transfer lines.", 5m, transferLine.WE_CustomDecimal5);
			AssertEquals("Modifying CustomFlag1 on group should modify all matching transfer lines.", true, transferLine.WE_CustomFlag1);
			AssertEquals("Modifying CustomFlag2 on group should modify all matching transfer lines.", true, transferLine.WE_CustomFlag2);
			AssertEquals("Modifying CustomFlag3 on group should modify all matching transfer lines.", true, transferLine.WE_CustomFlag3);
			AssertEquals("Modifying CustomFlag4 on group should modify all matching transfer lines.", true, transferLine.WE_CustomFlag4);
			AssertEquals("Modifying CustomFlag5 on group should modify all matching transfer lines.", true, transferLine.WE_CustomFlag5);
			AssertEquals("Modifying CustomTextBlob1 on group should modify all matching transfer lines.", "BLOB-1", transferLine.WE_CustomTextBlob1);
		}

		#endregion

		#region TestReadOnly_TaskManagementEnable

		public void TestGS_NKPickedBy_ReadOnly_TaskManagementEnable()
		{
			TestReadOnly_TaskManagementEnableCore(t => t.GS_NKPickedByInfo);
		}

		public void TestWE_GS_NKPutawayBy_ReadOnly_TaskManagementEnable()
		{
			TestReadOnly_TaskManagementEnableCore(t => t.WE_GS_NKPutawayByInfo);
		}

		void TestReadOnly_TaskManagementEnableCore(Func<WhsTransferLine, ZPropertyInfo> getInfo)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = transfer.Lines.AddNew();
			var info = getInfo(transferLine);
			AssertEquals(false, info.ReadOnly);

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var warehouse = Helper.CreateWarehouse("WH1", "A", 2, 2);
			warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, warehouse);
			var transferLine2 = transfer2.Lines.AddNew();
			var info2 = getInfo(transferLine2);
			AssertEquals(true, info2.ReadOnly);
		}

		#endregion

		#region AssertSetPropertySetsPropertyOnAllMatchingLines

		void AssertSetPropertySetsPropertyOnAllMatchingLines(string propertyName, IZType value1, IZType value2)
		{
			var emptyValue = value1.Default;

			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "", "");
			ClearPreSetValues(line); // clear to check setup later.
			var matchingLine1 = Helper.CreateMatchingLine(line, 5m);
			var matchingLine2 = Helper.CreateMatchingLine(line, 5m);
			ClearPreSetValues(matchingLine1); // clear to check setup later.
			ClearPreSetValues(matchingLine2); // clear to check setup later.
			AssertEquals("Precondition - Property to test should be empty.", emptyValue, line[propertyName]);

			matchingLine1[propertyName] = value1;
			AssertEquals(string.Format("Modifying {0} on matching line should *not* modify it on main line.", propertyName), emptyValue, line[propertyName]);
			AssertEquals(string.Format("Modifying {0} on matching line should stay on the matching line.", propertyName), value1, matchingLine1[propertyName]);
			AssertEquals(string.Format("Modifying {0} on matching line should not modify other matching lines.", propertyName), emptyValue, matchingLine2[propertyName]);

			line[propertyName] = value2;
			AssertEquals(string.Format("Modifying {0} on main line should stay on the main line.", propertyName), value2, line[propertyName]);
			AssertEquals(string.Format("Modifying {0} on main line should modify it on matching line 1.", propertyName), value2, matchingLine1[propertyName]);
			AssertEquals(string.Format("Modifying {0} on main line should modify it on matching line 2.", propertyName), value2, matchingLine2[propertyName]);
		}

		void ClearPreSetValues(WhsTransferLine line)
		{
			line.WE_F3_NKPackType = "";
			line.DestinationWarehousePK = ZGuid.Empty;
		}

		#endregion

		#region TestDeleteMatchingLines

		public void TestDeleteMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			TestDeleteMatchingLinesCore(data, WhsDocketLineSchema.WE_OP.Name, data.Part2.PK, part3.PK);
			TestDeleteMatchingLinesCore(data, WhsDocketLineSchema.WE_OriginalInventoryStatus.Name, new ZString("HLD"), new ZString("DAM"));
			TestDeleteMatchingLinesCore(data, WhsDocketLineSchema.WE_PartAttrib1.Name, new ZString("PA1"), new ZString("PA2"));
			TestDeleteMatchingLinesCore(data, WhsDocketLineSchema.WE_PartAttrib2.Name, new ZString("PA1"), new ZString("PA2"));
			TestDeleteMatchingLinesCore(data, WhsDocketLineSchema.WE_PartAttrib3.Name, new ZString("PA1"), new ZString("PA2"));
			TestDeleteMatchingLinesCore(data, WhsDocketLineSchema.WE_SerialNumber.Name, new ZString("PA1"), new ZString("PA2"));
			TestDeleteMatchingLinesCore(data, WhsDocketLineSchema.WE_PackingDate.Name, ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(10));
			TestDeleteMatchingLinesCore(data, WhsDocketLineSchema.WE_ExpiryDate.Name, ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(10));
			TestDeleteMatchingLinesCore(data, WhsTransferLine.Schema.ArrivalDateForBinding, ZDateTime.Today.AddDays(5), ZDateTime.Today.AddDays(10));
			TestDeleteMatchingLinesCore(data, WhsDocketLineSchema.WE_TransferFromPalletId.Name, new ZString("PLT-1"), new ZString("PLT-2"));
			TestDeleteMatchingLinesCore(data, WhsTransferLine.Schema.TransferFromLocationString, new ZString("A-2"), new ZString("A-3"));
			TestDeleteMatchingLinesCore(data, WhsDocketLineSchema.WE_BondedEntryKey.Name, new ZString("123-1"), new ZString("123-2"));
			TestDeleteMatchingLinesCore(data, WhsDocketLineSchema.WE_PackageGroupId.Name, new ZString("ABC"), new ZString("DEF"));
		}

		void TestDeleteMatchingLinesCore(TestDataSimpleEnvironment data, string testColumnName, IZType value1, IZType value2)
		{
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-1");
			var matchingLine1 = Helper.CreateMatchingLine(line, 5m);
			var matchingLine2 = Helper.CreateMatchingLine(line, 5m);

			// modifying matching lines property should not trigger event and so delete of matching lines
			matchingLine1[testColumnName] = value1;
			AssertEquals(false, line.IsDeleted);
			AssertEquals(false, matchingLine1.IsDeleted);
			AssertEquals(false, matchingLine2.IsDeleted);

			// modifying main line property to match matching line should not delete matching lines.
			line[testColumnName] = value1;
			AssertEquals(false, line.IsDeleted);
			AssertEquals(false, matchingLine1.IsDeleted);
			AssertEquals(true, matchingLine2.IsDeleted);
			AssertEquals("Quantity from deleted matching line should be moved to the main line.", 15m, line.WE_TransactionQuantity);

			// modifying main line property to a different value, should trigger event and delete all matching lines
			line[testColumnName] = value2;
			AssertEquals(false, line.IsDeleted);
			AssertEquals(true, matchingLine1.IsDeleted);
			AssertEquals("Quantity from deleted matching line should be moved to the main line.", 20m, line.WE_TransactionQuantity);
		}

		#endregion

		#region ReadOnly

		#region TestWE_DocketLineStatusInfo

		public void TestWE_DocketLineStatusInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestReadOnly(data, WhsDocketLineSchema.WE_DocketLineStatus.Name, true, true, true, true);
		}

		#endregion

		#region TestWE_TransactionQuantityInfo

		protected override void TestWE_TransactionQuantityInfoCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");

			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(transferLine.WE_TransactionQuantityInfo, data);
			TestReadOnly(data, WhsDocketLineSchema.WE_TransactionQuantity.Name, false, true, true, true);
		}

		#endregion

		#region TestWE_OPInfo

		protected override void TestWE_OPInfoCore(TestDataSimpleEnvironment data)
		{
			TestHeldForTransferOrTransferredReadOnly(data, WhsDocketLineSchema.WE_OP.Name);
		}

		#endregion

		#region TestReadonlyPropertiesForVasOrderTransfers

		public void TestReadonlyPropertiesForVasOrderTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, data.Whs1.FindLocation("A-1"), "");

			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			var vasOrderTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition", vasOrderTransfer);

			var vasOrderTransferLine = (WhsTransferLine)vasOrderTransfer.Lines.Single();
			ReadonlyPropertiesForVasOrderTransfers(vasOrderTransferLine, true);

			var outOfServiceAreaTransfer = Factory.New<WhsTransfer>();
			vasOrder.WVO_WD_TransferOutOfServiceArea = outOfServiceAreaTransfer.PK;
			ReadonlyPropertiesForVasOrderTransfers(outOfServiceAreaTransfer.Lines.AddNew(), false);
		}

		static void ReadonlyPropertiesForVasOrderTransfers(WhsTransferLine line, bool isIntoServiceAreaTransfer)
		{
			AssertEquals(true, line.WE_OPInfo.ReadOnly);
			AssertEquals(true, line.WE_PartAttrib1Info.ReadOnly);
			AssertEquals(true, line.WE_PartAttrib2Info.ReadOnly);
			AssertEquals(true, line.WE_PartAttrib3Info.ReadOnly);
			AssertEquals(true, line.WE_SerialNumberInfo.ReadOnly);
			AssertEquals(true, line.WE_ExpiryDateInfo.ReadOnly);
			AssertEquals(true, line.WE_PackingDateInfo.ReadOnly);
			AssertEquals(true, line.PackQtyIncludingMatchingLinesInfo.ReadOnly);
			AssertEquals(true, line.QtyToMoveIncludingMatchingLinesInfo.ReadOnly);
			AssertEquals(true, line.WE_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals(true, line.WE_CurrentInventoryStatusInfo.ReadOnly);
			AssertEquals(false, line.WE_PalletIDInfo.ReadOnly);
			AssertEquals(!isIntoServiceAreaTransfer, line.WE_TransferFromPalletIdInfo.ReadOnly);
			AssertEquals(!isIntoServiceAreaTransfer, line.WE_WL_TransferFromInfo.ReadOnly);
			AssertEquals(isIntoServiceAreaTransfer, line.WE_WLInfo.ReadOnly);
		}

		#endregion

		#region TestWE_PerPackageQtyInfo

		protected override void TestWE_PerPackageQtyInfoCore(WhsDocketLine docketLine, TestDataSimpleEnvironment data)
		{
			AssertEquals(true, docketLine.WE_PerPackageQtyInfo.ReadOnly);
		}

		#endregion

		#region TestWE_PackQuantityInfo

		protected override void TestWE_PackQuantityInfoCore(TestDataSimpleEnvironment data)
		{
			TestHeldForTransferOrTransferredReadOnly(data, WhsDocketLine.Schema.WE_PackQuantity);
		}

		#endregion

		#region TestWE_F3_NKPackTypeInfoCore

		protected override void TestWE_F3_NKPackTypeInfoCore(TestDataSimpleEnvironment data)
		{
			TestHeldForTransferOrTransferredReadOnly(data, WhsDocketLineSchema.WE_F3_NKPackType.Name);
		}

		#endregion

		#region TestWE_AdjustmentArrivalDateInfo

		protected override void TestWE_AdjustmentArrivalDateInfoCore(TestDataSimpleEnvironment data)
		{
			TestHeldForTransferOrTransferredReadOnly(data, WhsDocketLineSchema.WE_AdjustmentArrivalDate.Name);
		}

		#endregion

		#region TestWE_TransferFromPalletIdInfo

		public void TestWE_TransferFromPalletIDInfo()
		{
			TestDataSimpleEnvironment data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestStandardReadOnly(d => d.WE_TransferFromPalletIdInfo);
			TestReadOnly(data, WhsDocketLineSchema.WE_TransferFromPalletId.Name, false, true, true, true);

			var interWhsSourceTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var interWhsSourceTransferLine = Helper.CreateWhsTransferLine(interWhsSourceTransfer, data.Part1, 10m, "A-1", "A-2");
			TestReadOnly(interWhsSourceTransferLine, WhsDocketLineSchema.WE_TransferFromPalletId.Name, false, true, true, true);

			var interWhsDestTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsDest);
			var interWhsDestTransferLine = Helper.CreateWhsTransferLine(interWhsDestTransfer, data.Part1, 10m, "A-2", "A-1");
			TestReadOnly(interWhsDestTransferLine, WhsDocketLineSchema.WE_TransferFromPalletId.Name, false, true, true, true);
		}

		#endregion

		#region TestWE_PalletIDInfo

		protected override void TestWE_PalletIDInfoCore(TestDataSimpleEnvironment data)
		{
			TestReadOnly(data, WhsDocketLineSchema.WE_PalletID.Name, false, false, true, true);

			var interWhsSourceTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var interWhsSourceTransferLine = Helper.CreateWhsTransferLine(interWhsSourceTransfer, data.Part1, 10m, "A-1", "A-2");
			TestReadOnly(interWhsSourceTransferLine, WhsDocketLineSchema.WE_PalletID.Name, false, false, true, true);

			var interWhsDestTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsDest);
			var interWhsDestTransferLine = Helper.CreateWhsTransferLine(interWhsDestTransfer, data.Part1, 10m, "A-2", "A-1");
			TestReadOnly(interWhsDestTransferLine, WhsDocketLineSchema.WE_PalletID.Name, false, false, true, true);
		}

		#endregion

		#region TestTransferFromWarehousePKInfo_ReadOnly

		public void TestTransferFromWarehousePKInfo_ReadOnly()
		{
			TestStandardReadOnly(d => d.TransferFromWarehousePKInfo);
		}

		#endregion

		#region TestWE_WL_TransferFromInfo

		public void TestWE_WL_TransferFromInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestStandardReadOnly(d => d.WE_WL_TransferFromInfo);
			TestReadOnly(data, WhsDocketLineSchema.WE_WL_TransferFrom.Name, false, true, true, true);

			var interWhsSourceTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var interWhsSourceTransferLine = Helper.CreateWhsTransferLine(interWhsSourceTransfer, data.Part1, 10m, "A-1", "A-2");
			TestReadOnly(interWhsSourceTransferLine, WhsDocketLineSchema.WE_WL_TransferFrom.Name, false, true, true, true);

			var interWhsDestTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsDest);
			var interWhsDestTransferLine = Helper.CreateWhsTransferLine(interWhsDestTransfer, data.Part1, 10m, "A-2", "A-1");
			TestReadOnly(interWhsDestTransferLine, WhsDocketLineSchema.WE_WL_TransferFrom.Name, false, true, true, true);
		}

		#endregion

		#region TestWE_WLInfo

		protected override void TestWE_WLInfoCore(TestDataSimpleEnvironment data, ZPropertyInfo info)
		{
			TestReadOnly(data, WhsDocketLineSchema.WE_WL.Name, false, false, true, true);

			var interWhsSourceTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var interWhsSourceTransferLine = Helper.CreateWhsTransferLine(interWhsSourceTransfer, data.Part1, 10m, "A-1", "A-2");
			TestReadOnly(interWhsSourceTransferLine, WhsDocketLineSchema.WE_WL.Name, false, false, true, true);

			var interWhsDestTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsDest);
			var interWhsDestTransferLine = Helper.CreateWhsTransferLine(interWhsDestTransfer, data.Part1, 10m, "A-2", "A-1");
			TestReadOnly(interWhsDestTransferLine, WhsDocketLineSchema.WE_WL.Name, false, false, true, true);
		}

		#endregion

		#region TestWE_GS_NKPutawayByInfo

		protected override void TestWE_GS_NKPutawayByInfoCore(TestDataSimpleEnvironment data)
		{
			TestTransferredReadOnly(data, WhsDocketLineSchema.WE_GS_NKPutawayBy.Name);
		}

		#endregion

		#region TestTransferFromWarehousePKInfo

		public void TestTransferFromWarehousePKInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var interWhsSourceTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var interWhsSourceTransferLine = Helper.CreateWhsTransferLine(interWhsSourceTransfer, data.Part1, 10m, "A-1", "A-2");
			TestReadOnly(interWhsSourceTransferLine, WhsTransferLine.Schema.TransferFromWarehousePK, false, true, true, true);

			var interWhsDestTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsDest);
			var interWhsDestTransferLine = Helper.CreateWhsTransferLine(interWhsDestTransfer, data.Part1, 10m, "A-2", "A-1");
			TestReadOnly(interWhsDestTransferLine, WhsTransferLine.Schema.TransferFromWarehousePK, false, true, true, true);
		}

		#endregion

		#region TestDestinationWarehousePKInfo

		public void TestDestinationWarehousePKInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestStandardReadOnly(DocketLine.DestinationWarehousePKInfo);

			var interWhsSourceTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var interWhsSourceTransferLine = Helper.CreateWhsTransferLine(interWhsSourceTransfer, data.Part1, 10m, "A-1", "A-2");
			TestReadOnly(interWhsSourceTransferLine, WhsTransferLine.Schema.DestinationWarehousePK, false, false, true, true);

			var interWhsDestTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsDest);
			var interWhsDestTransferLine = Helper.CreateWhsTransferLine(interWhsDestTransfer, data.Part1, 10m, "A-2", "A-1");
			TestReadOnly(interWhsDestTransferLine, WhsTransferLine.Schema.DestinationWarehousePK, false, false, true, true);
		}

		#endregion

		#region TestWE_PutawayTimeInfo

		public void TestWE_PutawayTimeInfo_ReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestReadOnly(d => d.WE_PutawayTimeInfo, true, true, true, true);
			TestReadOnly(data, WhsDocketLineSchema.WE_PutawayTime.Name, true, true, true, true);
		}

		#endregion

		#region TestQtyToMoveIncludingMatchingLinesInfo

		public void TestQtyToMoveIncludingMatchingLinesInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestHeldForTransferOrTransferredReadOnly(data, WhsTransferLine.Schema.QtyToMoveIncludingMatchingLines);
		}

		#endregion

		#region TestPackQtyIncludingMatchingLinesInfo

		public void TestPackQtyIncludingMatchingLinesInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestHeldForTransferOrTransferredReadOnly(data, WhsTransferLine.Schema.PackQtyIncludingMatchingLines);
		}

		#endregion

		#region TestHoldCodeChangeReadOnly_InterWhsChild_Dest

		public void TestHoldCodeChangeReadOnly_InterWhsChild_Dest()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsSource;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertNotNull("Precondition: Child Transfer created.", transferLine.ChildTransferLine);

			var childTransferLine = transferLine.ChildTransferLine;
			AssertEquals("ChildTransferLines should *not* be deletable.", false, childTransferLine.CanDelete);

			childTransferLine.IsInventoryEditForm = true;
			AssertEquals("HeldCodeChangeQuantity on ChildTransferLines should be ReadOnly when unfinalised.", true, childTransferLine.HeldCodeChangeQuantityInfo.ReadOnly);
			AssertEquals("HeldCodeToChangeTo on ChildTransferLines should be ReadOnly when unfinalised.", true, childTransferLine.HeldCodeToChangeToInfo.ReadOnly);

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(childTransferLine);
			AssertEquals("HeldCodeChangeQuantity on ChildTransferLines should *not* be ReadOnly when finalised.", false, childTransferLine.HeldCodeChangeQuantityInfo.ReadOnly);
			AssertEquals("HeldCodeToChangeTo on ChildTransferLines should *not* be ReadOnly when finalised.", false, childTransferLine.HeldCodeToChangeToInfo.ReadOnly);
		}

		#endregion

		#region TestSynchroniseNonReadOnlyFieldsOnInterWarehouseChild

		public void TestSynchroniseNonReadOnlyFieldsOnInterWarehouseChild()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("Warehouse1", "A", 3, 3);
			Factory.Save();
			var sourceLocation = data.Whs1.FindLocation("A");
			var destinationLocation = whs2.FindLocation("A-2-1");
			var newDestinationLocation = whs2.FindLocation("A-3-1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var interWhsSourceTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var interWhsSourceTransferLine = Helper.CreateWhsTransferLine(interWhsSourceTransfer, data.Part1, 10m, sourceLocation, destinationLocation);

			var user1 = Helper.CreateGlbStaff("T1", "T1");
			interWhsSourceTransferLine.WE_GS_NKPutawayBy = user1.GS_Code;
			interWhsSourceTransferLine.WE_PalletID = "ABC";
			interWhsSourceTransferLine.PickedTime = ZDateTimeOffset.Now;

			var childTransferLine = interWhsSourceTransferLine.ChildTransferLine;
			AssertEquals("pre-condition: A child transfer line exists.", true, childTransferLine != null);
			AssertEquals("Current WE_WL on child line is not correct.", destinationLocation.PK, childTransferLine.WE_WL);
			AssertEquals("Current WE_GS_NKPutawayBy on child line is not correct.", "T1", childTransferLine.WE_GS_NKPutawayBy);
			AssertEquals("Current WE_PalletID on child line is not correct.", "ABC", childTransferLine.WE_PalletID);

			interWhsSourceTransferLine.WE_WL = newDestinationLocation.PK;
			interWhsSourceTransferLine.WE_GS_NKPutawayBy = GlbStaff.CurrentUser.GS_Code;
			interWhsSourceTransferLine.WE_PalletID = "DEF";
			AssertEquals(newDestinationLocation.PK, childTransferLine.WE_WL);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, childTransferLine.WE_GS_NKPutawayBy);
			AssertEquals("DEF", childTransferLine.WE_PalletID);

			interWhsSourceTransferLine.WE_WL = destinationLocation.PK;
			AssertEquals("Precondition.", destinationLocation.PK, childTransferLine.WE_WL);

			interWhsSourceTransferLine.LocationString = "A-3-1";
			AssertEquals("Should have synchronised location.", newDestinationLocation.WLV_LocationString, childTransferLine.LocationString);
			AssertEquals("Should have synchronised location.", newDestinationLocation.PK, childTransferLine.WE_WL);

			AssertEquals("Prediction: putaway time should be empty before finalisation.", ZDateTimeOffset.Empty, childTransferLine.WE_PutawayTime);
			interWhsSourceTransferLine.FinaliseDocketLine();
			AssertEquals("Putaway time should be synchronised with the parent transferLine.", interWhsSourceTransferLine.WE_PutawayTime, childTransferLine.WE_PutawayTime);
		}

		#endregion

		#region TestSynchroniseNonReadOnlyFieldsOnInterWarehouseChild_InterWarehouseDestination_Location

		public void TestSynchroniseNonReadOnlyFieldsOnInterWarehouseChild_InterWarehouseDestination_Location()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("Warehouse1", "A", 3, 3);
			Factory.Save();

			var sourceLocation = data.Whs1.FindLocation("A");
			var destinationLocation = whs2.FindLocation("A-2-1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var interWhsDestTransfer = Helper.CreateWhsTransfer(data.Org1, whs2, "TR2", Notify, TransferType.Codes.InterWhsDest);
			var interWhsDestTransferLine = Helper.CreateWhsTransferLine(interWhsDestTransfer, data.Part1.PK, 10m, "A", data.Whs1.PK, "");
			interWhsDestTransferLine.PickedTime = ZDateTimeOffset.Now;

			var childLine = interWhsDestTransferLine.ChildTransferLine;
			AssertNotNull("Precondition: Create Child Line.", childLine);
			AssertEquals("Precondition: Child Line has no dest location.", ZGuid.Empty, childLine.WE_WL);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var interWhsDestTransfer_InFactory2 = factory2.Load<WhsTransferLine>(interWhsDestTransferLine.PK);
			var childLine_InFactory2 = factory2.Load<WhsTransferLine>(childLine.PK);

			interWhsDestTransfer_InFactory2.LocationString = "A-2-1";
			AssertEquals("Should have synchronised location.", "A-2-1", childLine_InFactory2.LocationString);
			AssertEquals("Should have synchronised location.", destinationLocation.PK, childLine_InFactory2.WE_WL);
		}

		#endregion

		#region ReadOnly

		#region TestPartAttributesReadOnly

		protected override void TestPartAttributesReadOnly(ZPropertyInfo info, TestDataSimpleEnvironment data)
		{
			base.TestPartAttributesReadOnly(info, data);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			TestHeldForTransferOrTransferredReadOnly(transferLine, info.Name);
		}

		#endregion

		#region TestHeldForTransferOrTransferredReadOnly

		void TestHeldForTransferOrTransferredReadOnly(TestDataSimpleEnvironment data, ZString propertyName)
		{
			TestReadOnly(data, propertyName, false, true, true, true);
		}

		void TestHeldForTransferOrTransferredReadOnly(WhsTransferLine transferLine, ZString propertyName)
		{
			TestReadOnly(transferLine, propertyName, false, true, true, true);
		}

		#endregion

		#region TestIsPickedReadOnly

		public void TestPickedTimeInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			TestIsPickedReadOnly(data, DocketLine.PickedTimeInfo);
		}

		void TestIsPickedReadOnly(TestDataSimpleEnvironment data, ZPropertyInfo info)
		{
			var line = (WhsTransferLine)info.BizObj;
			var pickLine = line.PickLines.AddNew();
			AssertEquals("Having pick line is not the same as being picked.", false, line.ZPropertyInfoHash[info.Name].ReadOnly);

			using (new SemaphoreManager(line.FinaliseDocketLineSemaphore))
			{
				line.WE_PutawayTime = ZDateTimeOffset.Now;
			}
			AssertEquals("Transfer line will be picked before being putaway, therefore putaway should also be readonly.", true, line.ZPropertyInfoHash[info.Name].ReadOnly);
			using (new SemaphoreManager(line.FinaliseDocketLineSemaphore))
			{
				line.WE_PutawayTime = ZDateTimeOffset.Empty; // clean up
			}
			TestReadOnly(data, info.Name, false, true, true, true); // Transferline specific tests
			TestReadOnly(info, false, false, true, true); // DocketLine tests
		}

		#endregion

		#region TestTransferredReadOnly

		void TestTransferredReadOnly(TestDataSimpleEnvironment data, ZString propertyName)
		{
			TestReadOnly(data, propertyName, false, false, true, true);
		}

		#endregion

		#region TestReadOnly

		void TestReadOnly(TestDataSimpleEnvironment data, ZString propertyName, bool isEntered, bool isHeldForTransfer, bool isTransferred, bool isFinalised)
		{
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			TestReadOnly(transferLine, propertyName, isEntered, isHeldForTransfer, isTransferred, isFinalised);
		}

		void TestReadOnly(WhsTransferLine transferLine, ZString propertyName, bool isEntered, bool isHeldForTransfer, bool isTransferred, bool isFinalised)
		{
			var propertyInfoToTest = transferLine.FindPropertyInfo(propertyName);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Entered;
			AssertEquals(string.Format("When transfer line is entered, {0} should not be readonly.", propertyName), isEntered, propertyInfoToTest.ReadOnly);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.HeldForTransfer;
			AssertEquals(string.Format("When transfer line is held for transfer, {0} should not be readonly.", propertyName), isHeldForTransfer, propertyInfoToTest.ReadOnly);

			transferLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			AssertEquals(string.Format("When transfer line is finalised, {0} should not be readonly.", propertyName), isFinalised, propertyInfoToTest.ReadOnly);

			TestChildLineReadOnly(transferLine, propertyInfoToTest.Name);
		}

		protected override void TestStandardReadOnly(ZPropertyInfo info)
		{
			base.TestStandardReadOnly(info); // Base tests primarily Finalised/Cancelled read only at Job level

			TestChildLineReadOnly((WhsTransferLine)info.BizObj, info.Name);
		}

		void TestChildLineReadOnly(WhsTransferLine transferLine, string propertyToTest)
		{
			var childTransferLine = (WhsTransferLine)transferLine.Clone();
			childTransferLine.WE_WD = transferLine.WE_WD;

			var defaultReadOnly = childTransferLine.FindPropertyInfo(propertyToTest).ReadOnly;
			childTransferLine.WE_WE_ParentDocketLine = transferLine.PK;
			AssertEquals("Precondition: Child Transfer Line.", true, childTransferLine.IsChildTransferLine);
			AssertEquals("Child Transfer Lines should always be readonly.", true, childTransferLine.FindPropertyInfo(propertyToTest).ReadOnly);

			// Change to Hold Code Change Line
			childTransferLine.WE_IsOriginalInventory = false;
			AssertEquals("Precondition: No longer a Child Transfer Line.", false, childTransferLine.IsChildTransferLine);
			AssertEquals("Should not change ReadOnly if not a Child Transfer Line.", defaultReadOnly, childTransferLine.FindPropertyInfo(propertyToTest).ReadOnly);

			childTransferLine.Delete(); // Cleanup
		}

		protected override void TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(ZPropertyInfo info, TestDataSimpleEnvironment data)
		{
			base.TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(info, data);

			TestHeldForTransferOrTransferredReadOnly(data, info.Name);
		}

		#region TestPalletIDReadOnly_InTransitOnInventoryForm 

		public void TestPalletIDReadOnly_InTransitOnInventoryForm()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, sourceLocation);
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation, destinationLocation);
			transferLine.PickedTime = ZDateTimeOffset.Now;

			AssertEquals("Pallet ID In-Transit Transfer Lines should not be readonly if not in InventoryEditForm", false, transferLine.WE_PalletIDInfo.ReadOnly);

			transferLine.IsInventoryEditForm = true;
			AssertEquals("Pallet ID In-Transit Transfer Lines should be readonly if in InventoryEditForm", true, transferLine.WE_PalletIDInfo.ReadOnly);
		}

		#endregion

		#region TestTransferLineReadOnly

		public void TestTransferLineReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", whs2.PK, "B-1");
			AssertEquals("Precondition", false, transfer.IsReadyForPlanningOrPlanned);
			AssertEquals("Precondition", false, transferLine.ReadOnly);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals("The property should be readonly when ready for planning.", true, transferLine.ReadOnly);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertEquals("The property should not be readonly when not ready for planning.", false, transferLine.ReadOnly);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			AssertEquals("The property should be readonly when planned.", true, transferLine.ReadOnly);

			transfer.WD_TaskPlanningStatus = "";
			AssertEquals("The property should not be readonly when task planning status is empty.", false, transferLine.ReadOnly);
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region TestPackTypeDefaultedFromProduct

		public void TestPackTypeDefaultedFromProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket();

			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			var docketLine = GetNewBusinessObject(docket);

			var product = WhsProduct.GetWhsProduct(data.Part1);
			var productParams = product.ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_F3_NKReleasedPackType = "BAG";
			productParams.W3_F3_NKReceivedPackType = "BOX";

			docketLine.WE_OP = data.Part2.PK;
			AssertEquals("UNT", docketLine.WE_F3_NKPackType);

			docketLine.WE_OP = data.Part1.PK;
			AssertEquals("Pack Type should be defaulted from W3_F3_NKReleasedPackType.", "BAG", docketLine.WE_F3_NKPackType);

			docketLine.WE_OP = ZGuid.Empty;

			productParams.W3_F3_NKReleasedPackType = "";
			docketLine.WE_OP = data.Part1.PK;
			AssertEquals("W3_F3_NKReceivedPackType was empty thus Pack Type should be defaulted from W3_F3_NKReceivedPackType.", "BOX", docketLine.WE_F3_NKPackType);
		}

		#endregion

		#region TestHumanReadableShortcutName

		protected override void TestHumanReadableShortcutNameCore()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destLocation = data.Whs1.FindLocation("A-2");

			var org = helper.CreateClient("ABC");
			var product = helper.CreateProduct("TESTPROD", org);
			var receive = helper.CreateWhsReceiveWithInventory(org, data.Whs1, "R1", product, 10m, sourceLocation, "");
			receive.Lines[0].WE_AdjustmentArrivalDate = new ZDateTimeOffset(2022, 12, 12);
			Factory.Save();

			var transfer = helper.CreateWhsTransfer(org, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, product, 10m, sourceLocation, destLocation);
			helper.CreateWhsPickLine(transferLine, receive.Inventory[0], 10m);

			transferLine.FinaliseDocketLine();
			AssertEquals(true, transferLine.IsFinalised);

			AssertEquals("ABC - TESTPROD - 12-Dec-22", transferLine.HumanReadableShortcutName);
		}

		public void TestHumanReadableShortcutName_NotInventoryLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var transferWhs = data.Whs1;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferWhs, "TR1");
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", whs2.PK, "A");
			AssertEquals(false, transferLine.IsInventoryLine);
			AssertEquals("Docket Line", transferLine.HumanReadableShortcutName);
		}

		#endregion

		#endregion

		#region TestConstraints

		#region TestConstraint_WE_CurrentInventoryStatus

		[ExpectNoExceptions]
		public void TestConstraint_WE_CurrentInventoryStatus_Finalized()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destLocation = data.Whs1.FindLocation("A-2");
			var expectedExceptionMsg = "The UPDATE statement conflicted with the CHECK constraint \"Constraint_WE_CurrentInventoryStatus";

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "");
			Factory.Save();

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation, destLocation);
			helper.CreateWhsPickLine(transferLine, receive.Inventory[0], 10m);

			// AVL
			AssertEquals("Expected 'AVL' current inventory status.", InventoryStatus.Codes.Available, transferLine.WE_CurrentInventoryStatus);
			transfer.FinaliseDocket();
			Factory.Save();
			AssertEquals("Docket line must be finalized.", true, transferLine.IsFinalised);

			// HEL
			transferLine.WE_WHC_NKCurrentInventoryHeldCode = "COOL";
			transferLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			Factory.Save();
			AssertEquals("Expected 'HEL' current inventory status.", InventoryStatus.Codes.Held, transferLine.WE_CurrentInventoryStatus);

			// PUT
			transferLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Putaway;
			Factory.Save();
			AssertEquals("Expected 'PUT' current inventory status.", InventoryStatus.Codes.Putaway, transferLine.WE_CurrentInventoryStatus);

			// INT: Not allowed
			transferLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.InTransit;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMsg, true), "Exception expected from Constraint_WE_CurrentInventoryStatus");
		}

		public void TestConstraint_WE_CurrentInventoryStatus_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation, destLocation);
			Helper.CreateWhsPickLine(transferLine, receive.Inventory[0], 10m);

			Factory.Save();

			transfer.FinaliseDocket();
			transferLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Staged;
			Factory.Save();

			AssertEquals("Docket line must be finalized.", true, transferLine.IsFinalised);
			AssertEquals("Expected 'STA' current inventory status.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
		}

		protected override WhsTransferLine SetupDocketLineForCurrentInventoryStatusConstraintTest()
		{
			var docketLine = base.SetupDocketLineForCurrentInventoryStatusConstraintTest();
			docketLine.WE_WL_TransferFrom = docketLine.Docket.Warehouse.DefaultLocation.PK;

			return docketLine;
		}

		#endregion

		#endregion

		#region TestUpdateLastInventoryChangeDate

		[TestDate(2019, 3, 26, 13, 15, 10)]
		public void TestUpdateLastInventoryChangeDate()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "");
			Factory.Save();

			TestDateAttribute.Date = now.AddDays(1).ToDateTime();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation, destLocation);
			Helper.CreateWhsPickLine(transferLine, receive.Inventory[0], 10m);
			Factory.Save();

			AssertEquals("Create receive in source location should update.", now, sourceLocation.WLV_LastInventoryChangeDate);
			AssertEquals("not transfer to destination yet.", ZDateTimeOffset.Empty, destLocation.WLV_LastInventoryChangeDate);

			transferLine.FinaliseDocketLine(); // finalise  line will update pick line pick time 
			AssertEquals("Source and destination location should be updated.", now.AddDays(1), sourceLocation.WLV_LastInventoryChangeDate);
			AssertEquals("Source and destination location should be updated.", now.AddDays(1), destLocation.WLV_LastInventoryChangeDate);
		}

		#endregion

		#region Locations

		#region TestLocationString_WithInvalidLocationStringAndSuspendedValidation

		public void TestLocationString_WithInvalidLocationStringAndSuspendedValidation()
		{
			var whs = Helper.CreateWarehouse("1", "A", 2, 1);
			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;
			var transfer = Helper.CreateWhsTransfer(Helper.CreateClient(), whs);
			Factory.Save();

			var location1 = locations[0];
			var location2 = locations[1];

			// Set up a Whsdocket and a line

			var docketLine = GetNewBusinessObject(transfer);
			docketLine.TransferFromLocationString = "A-1";
			docketLine.LocationString = "A-2";

			AssertEquals(location1.PK, docketLine.WE_WL_TransferFrom);
			AssertEquals(location2.PK, docketLine.WE_WL);

			Factory.SuspendValidation(); // Stop the validation for the business object

			docketLine.WE_WL_TransferFrom = ZGuid.Empty; // To reset locations 
			docketLine.WE_WL = ZGuid.Empty; // To reset locations 
			docketLine.TransferFromLocationString = ""; // To reset locations
			docketLine.LocationString = ""; // To reset locations

			docketLine.TransferFromLocationString = "A-1";
			docketLine.LocationString = "A-2";

			AssertEquals(location1.PK, docketLine.WE_WL_TransferFrom);
			AssertEquals(location2.PK, docketLine.WE_WL);
		}

		#endregion

		#region TestLocationString_ClearmatchingLines

		public void TestLocationString_ClearmatchingLines()
		{
			AssertSetPropertySetsPropertyOnAllMatchingLines(WhsTransferLine.Schema.LocationString, new ZString("A-1"), new ZString("A-2"));
		}

		#endregion

		#endregion

		#region TestUncommitOverPickedOrNonMatchingInventory

		#region TestUncommitOverPickedOrNonMatchingInventory

		public void TestUncommitOverPickedOrNonMatchingInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locations[0], "");
			var inventory = receive.Inventory[0];

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, locations[0].ToLocationString(), "");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition - ensure stock is committed.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure stock is committed.", 20m, transferLine.GetQtyCommittedToThisLine());

			transferLine.WE_TransactionQuantity = 30m;
			transferLine.UncommitOverPickedOrNotMatchingInventory();
			AssertEquals("Uncommit inventory should not commit additional stock.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Uncommit inventory should not commit additional stock.", 20m, transferLine.GetQtyCommittedToThisLine());

			transferLine.WE_TransactionQuantity = 10m;
			transferLine.UncommitOverPickedOrNotMatchingInventory();
			AssertEquals("Uncommit inventory should uncommit stock if committed more that required.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Uncommit inventory should uncommit stock if committed more that required.", 10m, transferLine.GetQtyCommittedToThisLine());

			transferLine.WE_OP = data.Part2.PK;
			transferLine.UncommitOverPickedOrNotMatchingInventory();
			AssertEquals("Uncommit inventory should uncommit all stock if one of parameters doesn't much anymore.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Uncommit inventory should uncommit all stock if one of parameters doesn't much anymore.", 0m, transferLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestUncommitOverPickedOrNotMatchingInventory_ForFinalisedOrNonMasterTransferLine

		public void TestUncommitOverPickedOrNotMatchingInventory_ForFinalisedOrNonMasterTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition - ensure stock is committed.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure stock is committed.", 20m, transferLine.GetQtyCommittedToThisLine());

			transferLine.WE_TransactionQuantity = 10m;
			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			transferLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			transferLine.UncommitOverPickedOrNotMatchingInventory();
			AssertEquals("Uncommit inventory for finalised line should not uncommit stock.", 20m, transferLine.GetQtyCommittedToThisLine());
			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Entered; // clear up

			var masterTransfer = Helper.CreateWhsTransfer(data.Org1, Helper.CreateWarehouse("WH2"), "TR2", Notify);
			var masterTransferLine = Helper.CreateWhsTransferLine(masterTransfer, data.Part1, 10m, "A-1", "");
			transfer.WD_WD_ParentDocket = masterTransfer.PK;
			transferLine.WE_WE_ParentDocketLine = masterTransferLine.PK;
			transferLine.UncommitOverPickedOrNotMatchingInventory();
			AssertEquals("Child transfer line should not uncommit stock.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Child transfer line should not uncommit stock.", 20m, transferLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestUncommitOverPickedOrNotMatchingInventory_InTransitInventory

		public void TestUncommitOverPickedOrNotMatchingInventory_InTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: stock should be committed.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition: stock should be committed.", 20m, transferLine.GetQtyCommittedToThisLine());

			AssertEquals("Precondition.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition.", ZDateTimeOffset.Empty, transferLine.WE_AdjustmentArrivalDate);
			transferLine.UncommitOverPickedOrNotMatchingInventory();
			AssertEquals("Precondition: should not uncommit stock, because inventory matches transaction line (and is not in error).",
				20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition: should not uncommit stock, because inventory matches transaction line (and is not in error).",
				20m, transferLine.GetQtyCommittedToThisLine());

			transferLine.PickedTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition: Should have changed Inventory Status.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertNotEquals("Precondition: Should have changed ArrivalDate.", ZDateTimeOffset.Empty, transferLine.WE_AdjustmentArrivalDate);

			Factory.Save(); // necessary condition for AssertNoExceptionThrown().
			AssertNoExceptionThrown("Should not try to uncommit picked lines for In-Transit inventory.", transferLine.UncommitOverPickedOrNotMatchingInventory);
			AssertEquals("No units should have been unpicked for In-Transit Inventory.", 20m, transferLine.PickLines.Where(pl => pl.IsPicked).Sum(pl => pl.WZ_Units));
		}

		#endregion

		#endregion

		#region TestCreateInTransitInventory

		#region TestCreateInTransitInventory

		public void TestCreateInTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventory.LocationString, "");
			AssertEquals("Precondition", ZDateTimeOffset.Empty, transferLine.PickedTime);
			AssertEquals("Precondition", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock should be committed.", 5m, transferLine.QtyCommittedIncludingMatchingLines);

			AssertExceptionThrown(typeof(NotSupportedException), "Attempt to create In-Transit Inventory for unpicked transfer line", transferLine.CreateInTransitInventory);

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition.", now, transferLine.PickedTime);
			AssertEquals("Precondition: Created In-Transit Inventory.", 5m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			transferLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Empty;
			transferLine.PickLines[0].WZ_Units = 3m;
			transferLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;

			transferLine.CreateInTransitInventory();
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", 3m, transferLine.WE_StockOnHand);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			transferLine.PickedTime = ZDateTimeOffset.Empty;
			AssertEquals("Precondition: Removed In-Transit inventory.", 0m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition: Removed In-Transit inventory.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Removed In-Transit inventory.", InventoryStatus.Codes.Available, transferLine.WE_CurrentInventoryStatus);

			AssertExceptionThrown(typeof(NotSupportedException), "Attempt to create In-Transit Inventory for unpicked transfer line", transferLine.CreateInTransitInventory);
			AssertEquals("Should *not* be In-Transit.", 0m, transferLine.WE_StockOnHand);
			AssertEquals("Should *not* be In-Transit.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Should *not* be In-Transit.", InventoryStatus.Codes.Available, transferLine.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestCreateInTransitInventory_Finalised

		public void TestCreateInTransitInventory_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventory.LocationString, "A-2");
			AssertEquals("Precondition", ZDateTimeOffset.Empty, transferLine.PickedTime);
			AssertEquals("Precondition", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock should be committed.", 5m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);

			AssertExceptionThrown(typeof(NotSupportedException), "Attempt to create In-Transit Inventory for finalised transfer!", transferLine.CreateInTransitInventory);
		}

		#endregion

		#region TestCreateInTransitInventory_HoldReasonIsCopied

		public void TestCreateInTransitInventory_HoldReasonIsCopied()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			var inventory1 = receive1.Lines[0];
			var inventory2 = receive2.Lines[0];
			Factory.Save();

			inventory1.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			inventory1.HoldReasonToChangeTo = "Whatever";
			inventory1.ChangeInventoryHeldCode(true);

			inventory2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			inventory2.HoldReasonToChangeTo = "Whenever";
			inventory2.ChangeInventoryHeldCode(true);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, inventory1.Location.WLV_LocationString, "", inventory1.Location.WLV_LocationString, "PLT-1", InventoryHoldCodes.Codes.Held);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 15m, transferLine.QtyCommittedIncludingMatchingLines);
			AssertEquals("Hold Reason not set.", "", transferLine.WE_CurrentHoldReason);
			AssertEquals("Hold Reason not set.", "", transferLine.MatchingLines.Single().WE_CurrentHoldReason);
			Factory.Save();

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertContainsExactElementsInAnyOrder(new[] { "Whatever", "Whenever" }, new[] { transferLine.WE_CurrentHoldReason, transferLine.MatchingLines.Single().WE_CurrentHoldReason });

			transferLine.PickedTime = ZDateTimeOffset.Empty;
			AssertEquals("Hold Reason should be cleared.", "", transferLine.WE_CurrentHoldReason);
			AssertEquals("Hold Reason should be cleared.", "", transferLine.MatchingLines.Single().WE_CurrentHoldReason);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertContainsExactElementsInAnyOrder(new[] { "Whatever", "Whenever" }, new[] { transferLine.WE_CurrentHoldReason, transferLine.MatchingLines.Single().WE_CurrentHoldReason });
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region TestCreateInTransitInventory_MatchingLines

		public void TestCreateInTransitInventory_MatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, data.Whs1.DefaultLocation.WLV_LocationString, "");
			AssertEquals("Precondition", ZDateTimeOffset.Empty, transferLine.PickedTime);
			AssertEquals("Precondition", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock should be committed.", 20m, transferLine.QtyCommittedIncludingMatchingLines);

			var matchingLine = (WhsTransferLine)transferLine.MatchingLines.SingleOrDefault();
			AssertNotNull("Precondition: Created matching line.", matchingLine);

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			transferLine.PickedTime = now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition.", now, transferLine.PickedTime);
			AssertEquals("Precondition: Created In-Transit Inventory.", 10m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Created In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			AssertEquals("Precondition: Created In-Transit Inventory on matching line.", 10m, matchingLine.WE_StockOnHand);
			AssertEquals("Precondition: Created In-Transit Inventory on matching line.", InventoryStatus.Codes.InTransit, matchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Created In-Transit Inventory on matching line.", InventoryStatus.Codes.InTransit, matchingLine.WE_CurrentInventoryStatus);

			matchingLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			matchingLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Empty;
			matchingLine.PickLines[0].WZ_Units = 2m;
			matchingLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;

			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			transferLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Empty;
			transferLine.PickLines[0].WZ_Units = 3m;
			transferLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;

			transferLine.CreateInTransitInventory();
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", 3m, transferLine.WE_StockOnHand);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			AssertEquals("Should have recreated In-Transit Inventory on matching line.", 2m, matchingLine.WE_StockOnHand);
			AssertEquals("Should have recreated In-Transit Inventory on matching line.", InventoryStatus.Codes.InTransit, matchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory on matching line.", InventoryStatus.Codes.InTransit, matchingLine.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestCreateInTransitInventory_InterWhs

		#region TestCreateInTransitInventory_InterWhs_Dest

		public void TestCreateInTransitInventory_InterWhs_Dest()
		{
			TestCreateInTransitInventory_InterWhsCore(isSource: false);
		}

		#endregion

		#region TestCreateInTransitInventory_InterWhs_Source

		public void TestCreateInTransitInventory_InterWhs_Source()
		{
			TestCreateInTransitInventory_InterWhsCore(isSource: true);
		}

		#endregion

		#region TestCreateInTransitInventory_InterWhsCore

		void TestCreateInTransitInventory_InterWhsCore(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var transferWhs = isSource ? data.Whs1 : whs2;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferWhs, "TR1");
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", isSource ? whs2.PK : data.Whs1.PK, "A");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = ZDateTimeOffset.Now;

			var childTransfer = transfer.ChildTransfers.First();
			var childLine = transferLine.ChildTransferLine;
			AssertNotNull("Precondition: Created child line.", childLine);

			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			childLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			transferLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Empty;
			transferLine.PickLines[0].WZ_Units = 3m;
			transferLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;

			transferLine.CreateInTransitInventory();
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory on child line.", InventoryStatus.Codes.InTransit, childLine.WE_OriginalInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory on child line.", InventoryStatus.Codes.InTransit, childLine.WE_CurrentInventoryStatus);

			var destLine = isSource ? childLine : transferLine;
			var sourceLine = isSource ? transferLine : childLine;
			AssertEquals("Should have recreated In-Transit Inventory.", 3m, destLine.WE_StockOnHand);
			AssertEquals("Source line should have no stock.", 0m, sourceLine.WE_StockOnHand);
		}

		#endregion

		#endregion

		#region TestCreateInTransitInventory_InterWhs_MatchingLines

		#region TestCreateInTransitInventory_InterWhs_MatchingLines_Dest

		public void TestCreateInTransitInventory_MatchingLines_InterWhs_Dest()
		{
			TestCreateInTransitInventory_InterWhs_MatchingLinesCore(isSource: false);
		}

		#endregion

		#region TestCreateInTransitInventory_InterWhs_MatchingLines_Source

		public void TestCreateInTransitInventory_InterWhs_MatchingLines_Source()
		{
			TestCreateInTransitInventory_InterWhs_MatchingLinesCore(isSource: true);
		}

		#endregion

		#region TestCreateInTransitInventory_InterWhs_MatchingLinesCore

		void TestCreateInTransitInventory_InterWhs_MatchingLinesCore(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var transferWhs = isSource ? data.Whs1 : whs2;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferWhs, "TR1");
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", isSource ? whs2.PK : data.Whs1.PK, "A");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = ZDateTimeOffset.Now;

			var matchingLine = (WhsTransferLine)transferLine.MatchingLines.SingleOrDefault();
			var childTransfer = transfer.ChildTransfers.First();
			var childLine = transferLine.ChildTransferLine;
			AssertNotNull("Precondition: Created child line.", childLine);
			AssertNotNull("Precondition: Should have created a matching line.", childLine);

			var childMatchingLine = matchingLine.ChildTransferLine;
			AssertNotNull("Precondition: Should have created a matching line.", childLine);

			matchingLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			childMatchingLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			childLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;

			matchingLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Empty;
			matchingLine.PickLines[0].WZ_Units = 2m;
			transferLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Empty;
			transferLine.PickLines[0].WZ_Units = 3m;
			transferLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;
			matchingLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Today;

			transferLine.CreateInTransitInventory();
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, matchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, matchingLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, childLine.WE_OriginalInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, childLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, childMatchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Should have recreated In-Transit Inventory.", InventoryStatus.Codes.InTransit, childMatchingLine.WE_CurrentInventoryStatus);

			var destLine = isSource ? childLine : transferLine;
			var destMatchingLine = isSource ? childMatchingLine : matchingLine;
			var sourceLine = isSource ? transferLine : childLine;
			var sourceMatchingLine = isSource ? matchingLine : childMatchingLine;
			AssertEquals("Should have recreated In-Transit Inventory.", 2m, destMatchingLine.WE_StockOnHand);
			AssertEquals("Should have recreated In-Transit Inventory.", 3m, destLine.WE_StockOnHand);
			AssertEquals("Source line should have no stock.", 0m, sourceLine.WE_StockOnHand);
			AssertEquals("Source line should have no stock.", 0m, sourceMatchingLine.WE_StockOnHand);
		}

		#endregion

		#endregion

		#endregion

		#region TestCommitInventory

		#region TestCommitInventory

		public void TestCommitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locations[0], "");
			var inventory = receive.Inventory[0];

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locations[0].ToLocationString(), "");
			AssertEquals("Precondition - ensure no stock is committed.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, transferLine.GetQtyCommittedToThisLine());

			transferLine.RunPreSaveValidation();
			AssertEquals("CommitInventory should commit stock.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 10m, transferLine.GetQtyCommittedToThisLine());

			transferLine.WE_TransactionQuantity = 100m;
			transferLine.RunPreSaveValidation();
			AssertEquals("CommitInventory should commit stock.", 50m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 50m, transferLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestPutawayTransfers

		public void TestPutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-2");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inv1InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation1, "A", 1m);
			var inv2InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation1, "A", 2m);
			var inv3InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation2, "B", 3m);
			var inv4InRec1ForPart2 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part2, dockDoorLocation1, "D", 1m);
			var invWithHeldCode = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part2, dockDoorLocation1, "C", 4m);
			invWithHeldCode.OriginalInventoryHeldCode = "HEL";
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "A", 3m);
			var transferLineForPart2 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part2, dockDoorLocation1, nonDockDoorLocation, "D", 1m);
			var transferLineWithHeldCode = (WhsTransferLine)transfer.CreateDocketLineFromInventory(invWithHeldCode);
			transferLineWithHeldCode.WE_WL = nonDockDoorLocation.PK;
			transferLineWithHeldCode.WE_PalletID = "C";
			AssertEquals(InventoryStatus.Codes.Held, transferLineWithHeldCode.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals(InventoryStatus.Codes.Received, transferLineWithHeldCode.WE_OriginalInventoryStatus);

			transfer.RunPreSaveValidation();
			transferLineForPart1.PickedTime = ZDateTimeOffset.Now;
			transferLineWithHeldCode.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Transfer line inventory status should be changed to Putting Away.", InventoryStatus.Codes.PuttingAway, transferLineForPart1.WE_OriginalInventoryStatus);
			AssertEquals("Transfer line's matching line's inventory status should be changed to Putting Away.", InventoryStatus.Codes.PuttingAway, transferLineForPart1.MatchingLines.Single().WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv2InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv2InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv3InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv4InRec1ForPart2.WI_InventoryStatus);
			AssertEquals("CommitInventory should commit stock.", 3m, transferLineForPart1.QtyCommittedIncludingMatchingLines);
			AssertEquals(InventoryStatus.Codes.Received, inv4InRec1ForPart2.WI_InventoryStatus);
			AssertEquals("CommitInventory should commit stock.", 1m, transferLineForPart2.QtyCommittedIncludingMatchingLines);
			AssertEquals(InventoryStatus.Codes.PuttingAway, transferLineWithHeldCode.WE_OriginalInventoryStatus);
			AssertEquals("CommitInventory should commit stock.", 4m, transferLineWithHeldCode.QtyCommittedIncludingMatchingLines);

			transferLineForPart1.FinaliseDocketLine();
			AssertEquals("Putaway Transfer should be finalised.", true, transferLineForPart1.IsFinalised);
			AssertEquals("Transfer line inventory status should be changed to Putaway.", InventoryStatus.Codes.Putaway, transferLineForPart1.WE_OriginalInventoryStatus);
			AssertEquals("Other Transfer line inventory status should not be changed.", InventoryStatus.Codes.Received, transferLineForPart2.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart1.WI_InventoryStatus);
			AssertEquals("Inventory Total Units should be reduced due to the picked putaway transfer.", 0m, inv1InRec1ForPart1.WI_TotalUnits);

			AssertEquals(InventoryStatus.Codes.Received, inv3InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv4InRec1ForPart2.WI_InventoryStatus);
			AssertEquals(dockDoorLocation1.PK, inv1InRec1ForPart1.WI_WL);
			AssertEquals(dockDoorLocation1.PK, inv2InRec1ForPart1.WI_WL);
			AssertEquals(dockDoorLocation2.PK, inv3InRec1ForPart1.WI_WL);
			AssertEquals(dockDoorLocation1.PK, inv4InRec1ForPart2.WI_WL);

			transferLineForPart2.FinaliseDocketLine();
			AssertEquals("Putaway Transfer should be finalised.", true, transferLineForPart2.IsFinalised);
			AssertEquals("Transfer line inventory status should be changed to Putaway.", InventoryStatus.Codes.Putaway, transferLineForPart2.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv3InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv4InRec1ForPart2.WI_InventoryStatus);
			AssertEquals("Inventory Total Units should be reduced due to the picked putaway transfer.", 0m, inv4InRec1ForPart2.WI_TotalUnits);

			AssertEquals(dockDoorLocation1.PK, inv1InRec1ForPart1.WI_WL);
			AssertEquals(dockDoorLocation1.PK, inv2InRec1ForPart1.WI_WL);
			AssertEquals(dockDoorLocation2.PK, inv3InRec1ForPart1.WI_WL);
			AssertEquals(dockDoorLocation1.PK, inv4InRec1ForPart2.WI_WL);

			transferLineWithHeldCode.FinaliseDocketLine();
			AssertEquals("Putaway Transfer line with held code should be finalised.", true, transferLineWithHeldCode.IsFinalised);
			AssertEquals("TPutaway Transfer line with held code inventory status should be changed to Putaway.", InventoryStatus.Codes.Putaway, transferLineWithHeldCode.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, invWithHeldCode.WI_InventoryStatus);
			AssertEquals("Inventory Total Units should be reduced due to the picked putaway transfer.", 0m, invWithHeldCode.WI_TotalUnits);
			AssertEquals(dockDoorLocation1.PK, invWithHeldCode.WI_WL);

			AssertEquals("A", inv1InRec1ForPart1.WI_PalletID);
			AssertEquals("A", inv2InRec1ForPart1.WI_PalletID);
			AssertEquals("B", inv3InRec1ForPart1.WI_PalletID);
			AssertEquals("D", inv4InRec1ForPart2.WI_PalletID);
			AssertEquals("C", invWithHeldCode.WI_PalletID);
		}

		public void TestPutawayTransfers_Delete()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-2");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inv1InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation1, "A", 1m);
			var inv2InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation1, "A", 2m);
			var inv3InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation2, "B", 3m);
			var inv1InRec1ForPart2 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part2, dockDoorLocation1, "A", 1m);

			var receive2 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", ZDateTimeOffset.Empty);
			var inv1InRec2ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, dockDoorLocation1, "A", 4m);
			var inv2InRec2ForPart2 = Helper.CreateInventoryForDockDoorLocation(receive2, data.Part2, dockDoorLocation1, "A", 2m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "A", 7m);
			var transferLineForPart2 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part2, dockDoorLocation1, nonDockDoorLocation, "A", 3m);

			transfer.RunPreSaveValidation();
			AssertEquals("Transfer line inventory status is Received until PickedTime is set.", InventoryStatus.Codes.Received, transferLineForPart1.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv2InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv3InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart2.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec2ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv2InRec2ForPart2.WI_InventoryStatus);
			AssertEquals("CommitInventory should commit stock.", 7m, transferLineForPart1.QtyCommittedIncludingMatchingLines);
			AssertEquals("CommitInventory should commit stock.", 3m, transferLineForPart2.QtyCommittedIncludingMatchingLines);

			transferLineForPart1.Delete();
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv2InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv3InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart2.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec2ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv2InRec2ForPart2.WI_InventoryStatus);
			AssertNoExceptionThrown(() => Factory.Save());

			transferLineForPart2.Delete();
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv2InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv3InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart2.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec2ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv2InRec2ForPart2.WI_InventoryStatus);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestPutawayTransfers_ReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-2");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inv1InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation1, "A", 1m);
			var inv2InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation1, "A", 2m);
			var inv3InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation2, "B", 3m);
			var inv4InRec1ForPart2 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part2, dockDoorLocation1, "C", 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(inv1InRec1ForPart1, 1m);
			orderLine.ReserveStockIfAbleTo(inv2InRec1ForPart1, 1m);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "A", 3m);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();

			AssertEquals("Transfer line inventory status should be changed to Putting Away.", InventoryStatus.Codes.PuttingAway, transferLine.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv2InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv3InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv4InRec1ForPart2.WI_InventoryStatus);
			AssertEquals("CommitInventory should commit stock.", 3m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.FinaliseDocketLine();
			AssertEquals("Putaway Transfer should be finalised.", true, transferLine.IsFinalised);
			AssertEquals("Transfer line inventory status should be changed to Putaway.", InventoryStatus.Codes.Putaway, transferLine.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv2InRec1ForPart1.WI_InventoryStatus);
			AssertEquals("Inventory Total Units should be reduced for Putaway Transfers.", 0m, inv1InRec1ForPart1.WI_TotalUnits);
			AssertEquals("Inventory Total Units should be reduced for Putaway Transfers.", 0m, inv2InRec1ForPart1.WI_TotalUnits);

			AssertEquals(InventoryStatus.Codes.Received, inv3InRec1ForPart1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv4InRec1ForPart2.WI_InventoryStatus);
			AssertEquals("CommitInventory should commit stock.", 3m, transferLine.QtyCommittedIncludingMatchingLines);
		}

		public void TestPutawayTransfers_RollbackChangesWhenFinalisationFailed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-2");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inv1InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation1, "A", 1m).InDocketLine;
			var inv2InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation1, "A", 2m).InDocketLine;
			var inv3InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation2, "B", 3m).InDocketLine;
			var inv4InRec1ForPart2 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part2, dockDoorLocation1, "A", 1m).InDocketLine;
			var invWithHeldCode = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part2, dockDoorLocation1, "C", 4m);
			invWithHeldCode.OriginalInventoryHeldCode = "HEL";
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "A", 3m);
			var transferLineForPart2 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part2, dockDoorLocation1, nonDockDoorLocation, "A", 1m);
			var transferLineWithHeldCode = (WhsTransferLine)transfer.CreateDocketLineFromInventory(invWithHeldCode);
			transferLineWithHeldCode.WE_WL = nonDockDoorLocation.PK;
			transferLineWithHeldCode.WE_PalletID = "C";
			AssertEquals(InventoryStatus.Codes.Held, transferLineWithHeldCode.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals(InventoryStatus.Codes.Received, transferLineWithHeldCode.WE_OriginalInventoryStatus);
			transferLineForPart1.PickedTime = ZDateTimeOffset.Now;
			transferLineForPart2.PickedTime = ZDateTimeOffset.Now;

			transfer.RunPreSaveValidation();

			AssertEquals("Transfer line inventory status should be changed to Putting Away.", InventoryStatus.Codes.PuttingAway, transferLineForPart1.WE_OriginalInventoryStatus);
			AssertEquals("Transfer line's matching line's inventory status should be changed to Putting Away.", InventoryStatus.Codes.PuttingAway, transferLineForPart1.MatchingLines.Single().WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv1InRec1ForPart1.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv2InRec1ForPart1.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv3InRec1ForPart1.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, inv4InRec1ForPart2.WE_OriginalInventoryStatus);
			AssertEquals("CommitInventory should commit stock.", 3m, transferLineForPart1.QtyCommittedIncludingMatchingLines);
			AssertEquals("CommitInventory should commit stock.", 1m, transferLineForPart2.QtyCommittedIncludingMatchingLines);
			AssertEquals(InventoryStatus.Codes.Received, transferLineWithHeldCode.WE_OriginalInventoryStatus);
			AssertEquals("CommitInventory should commit stock.", 4m, transferLineWithHeldCode.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			transferLineForPart1.WE_OriginalInventoryStatusInfo.ValueChanged += delegate
			{
				transferLineForPart1.AddRowError("Test");
			};
			transferLineForPart1.FinaliseDocketLine();
			AssertEquals("Putaway Transfer line finalisation failed.", false, transferLineForPart1.IsFinalised);
			AssertHasRowError("Should have added a RowError.", transferLineForPart1, "Error occurred during finalization. Close the form without saving and try again.");
		}

		public void TestPutawayTransfers_SetReceiveDocketStatusToPutawayOnFinalise()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var location = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoor, "A");
			Factory.Save();
			AssertEquals("WD_DocketStatus is Entered", DocketStatus.Codes.Entered, receive.WD_DocketStatus);

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoor, location, "A", 10m);
			AssertEquals("WD_DocketStatus is Entered", DocketStatus.Codes.Entered, receive.WD_DocketStatus);

			putawayTransfer.RunPreSaveValidation();
			AssertEquals("WD_DocketStatus is Entered", DocketStatus.Codes.Entered, receive.WD_DocketStatus);

			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("WD_DocketStatus is Putaway", DocketStatus.Codes.Putaway, receive.WD_DocketStatus);
		}

		#endregion

		#region TestCommitInventory_ForFinalisedOrNotMasterTransferLine

		public void TestCommitInventory_ForFinalisedOrNotMasterTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory[0];

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			AssertEquals("Precondition - ensure no stock is committed.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, transferLine.GetQtyCommittedToThisLine());

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			transferLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			transferLine.RunPreSaveValidation();
			AssertEquals("CommitInventory for finalised line should not commit stock.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory for finalised line should not commit stock.", 0m, transferLine.GetQtyCommittedToThisLine());
			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Entered; // clear up
			transferLine.WE_FinalisedDate = ZDateTimeOffset.Empty;
			transferLine.CommitInventory();
			AssertEquals("CommitInventory for not finalised line should commit inventory.", 10m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory for not finalised line should commit inventory.", 10m, transferLine.GetQtyCommittedToThisLine());
			transferLine.PickLines.DeleteAll(); // clean up

			var masterTransfer = Helper.CreateWhsTransfer(data.Org1, Helper.CreateWarehouse("WH2"), "TR2", Notify);
			var masterTransferLine = Helper.CreateWhsTransferLine(masterTransfer, data.Part1, 10m, "A-1", "");
			transfer.WD_WD_ParentDocket = masterTransfer.PK;
			transferLine.WE_WE_ParentDocketLine = masterTransferLine.PK;
			transferLine.RunPreSaveValidation();
			AssertEquals("CommitInventory for child transfer should not commit stock.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory for child transfer should not commit stock.", 0m, transferLine.GetQtyCommittedToThisLine());

			var matchingLine = Helper.CreateMatchingLine(transferLine, 5m);
			matchingLine.RunPreSaveValidation();
			AssertEquals("CommitInventory for matching transfer line should not commit stock.", 0m, matchingLine.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestCommitInventory_InTransitInventory

		public void TestCommitInventory_InTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock should be committed.", 20m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition: Stock should be committed.", 20m, transferLine.GetQtyCommittedToThisLine());
			AssertEquals("Precondition.", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition.", ZDateTimeOffset.Empty, transferLine.WE_AdjustmentArrivalDate);

			transferLine.PickedTime = ZDateTimeOffset.Today;
			AssertEquals("Should have changed Inventory Status.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertNotEquals("Should have changed ArrivalDate.", ZDateTimeOffset.Empty, transferLine.WE_AdjustmentArrivalDate);

			AssertNoExceptionThrown("Should not try to uncommit picked lines for In-Transit inventory.", transferLine.RunPreSaveValidation); // CommitInventory is run on PreSaveValidation
			AssertEquals("No units should have been unpicked for In-Transit Inventory.", 20m, transferLine.PickLines.Where(pl => pl.IsPicked).Sum(pl => pl.WZ_Units));
		}

		#endregion

		#endregion

		#region TestChangeInventoryHeldCode_StagedInventoryReturnedToStock

		public void TestChangeInventoryHeldCode_StagedInventoryReturnedToStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var inventoryLine = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var stagedInventoryLine = Helper.PickAndMakeInTransitTransfer(order.Lines[0].PickLines.Single(), ZDateTimeOffset.Now);

			var ddlTransfer = pick.Transfers.Single();
			ddlTransfer.FinaliseDocketWithoutUserConfirmation();

			AssertEquals("Precondition: Should be staged.", InventoryStatus.Codes.Staged, stagedInventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Should be available.", InventoryStatus.Codes.Available, stagedInventoryLine.WE_OriginalInventoryStatus);

			var iTransfer = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Returned);
			iTransfer.LinkToDocket(order);
			iTransfer.AdjustOutInventory(stagedInventoryLine.Inventory[0], inventoryLine.PK, 10m);

			var transfer = (WhsTransfer)iTransfer;
			transfer.FinaliseDocketWithoutUserConfirmation();

			// End to end process handles this part usually
			orderLine.PickLines.ForEach(pl => pl.WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty); // Avoid overpick trigger
			orderLine.PickLines.DeleteAll(); // Avoid overpick trigger

			var transferLine = transfer.Lines[0];
			AssertIsFinalisedPrecondition(transfer);
			AssertEquals("Precondition: Should be staged.", InventoryStatus.Codes.Available, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Should be staged.", InventoryStatus.Codes.Staged, transferLine.WE_OriginalInventoryStatus);
			Factory.Save();

			transferLine.IsInventoryEditForm = true;
			transferLine.HeldCodeChangeQuantity = 10m;
			transferLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			transferLine.ChangeInventoryHeldCode(true);
			AssertNoExceptionThrown("Should not throw constraint exception.", Factory.Save);
		}

		#endregion

		#region TestWhyIsThisInventoryCommitted_DockDoorTransfer

		public void TestWhyIsThisInventoryCommitted_DockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			AssertEquals(string.Format("This inventory item has units committed to:\r\nPick(s): {0}.", pick.WP_PickNo), transferLine.WhyIsThisInventoryCommitted());
		}

		#endregion

		#region TestClone

		public void TestTransferLineClone()
		{
			var whs = Helper.CreateWarehouse("Whs", "A");
			var client = Helper.CreateClient("C", "client");
			var product = Helper.CreateProduct(client, "Product");
			Factory.Save();
			var location = whs.FindLocation("A");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 1m, location, "");
			var transfer = Helper.CreateWhsTransfer(client, whs);
			var transferLine = Helper.CreateWhsTransferLine(transfer, product, 1m, location.ToLocationString(), "");
			transferLine.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			AssertCloneStatusCore(message: "Cloned InTransit line Inventory Status should be based on the master InTransit line original inventory hold code.",
				transferLine: transferLine,
				inventoryHoldCode: InventoryHoldCodes.Codes.Damaged,
				transferLineOriginalInventoryStatus: InventoryStatus.Codes.InTransit,
				transferlineCurrentInventoryStatus: InventoryStatus.Codes.InTransit,
				expectedCloneOriginalInventoryStatus: InventoryStatus.Codes.Held,
				expectedCloneCurrentInventoryStatus: InventoryStatus.Codes.Held);

			AssertCloneStatusCore(message: "Cloned InTransit line Inventory Status should be based on the master InTransit line original inventory hold code.",
				transferLine: transferLine,
				inventoryHoldCode: InventoryHoldCodes.Codes.Damaged,
				transferLineOriginalInventoryStatus: InventoryStatus.Codes.InTransit,
				transferlineCurrentInventoryStatus: InventoryStatus.Codes.Received, // Make different current inventory status not change the result.
				expectedCloneOriginalInventoryStatus: InventoryStatus.Codes.Held,
				expectedCloneCurrentInventoryStatus: InventoryStatus.Codes.Held);

			AssertCloneStatusCore(message: "Cloned line Inventory Status should be based on the master InTransit line original inventory hold code.",
				transferLine: transferLine,
				inventoryHoldCode: "",
				transferLineOriginalInventoryStatus: InventoryStatus.Codes.InTransit,
				transferlineCurrentInventoryStatus: InventoryStatus.Codes.InTransit,
				expectedCloneOriginalInventoryStatus: InventoryStatus.Codes.Available,
				expectedCloneCurrentInventoryStatus: InventoryStatus.Codes.Available);

			AssertCloneStatusCore(message: "Cloned line Inventory Status should be based on the master line original inventory status.",
				transferLine: transferLine,
				inventoryHoldCode: "",
				transferLineOriginalInventoryStatus: InventoryStatus.Codes.Pending,
				transferlineCurrentInventoryStatus: InventoryStatus.Codes.Pending,
				expectedCloneOriginalInventoryStatus: InventoryStatus.Codes.Pending,
				expectedCloneCurrentInventoryStatus: InventoryStatus.Codes.Pending);

			AssertCloneStatusCore(message: "Cloned line Inventory Status should be based on the master line original inventory status.",
				transferLine: transferLine,
				inventoryHoldCode: InventoryHoldCodes.Codes.Damaged,
				transferLineOriginalInventoryStatus: InventoryStatus.Codes.Pending,
				transferlineCurrentInventoryStatus: InventoryStatus.Codes.Pending,
				expectedCloneOriginalInventoryStatus: InventoryStatus.Codes.Held,
				expectedCloneCurrentInventoryStatus: InventoryStatus.Codes.Held);
		}

		void AssertCloneStatusCore(string message, WhsTransferLine transferLine, string inventoryHoldCode, string transferLineOriginalInventoryStatus, string transferlineCurrentInventoryStatus, string expectedCloneOriginalInventoryStatus, string expectedCloneCurrentInventoryStatus)
		{
			transferLine.WE_WHC_NKOriginalInventoryHeldCode = inventoryHoldCode;
			transferLine.WE_OriginalInventoryStatus = transferLineOriginalInventoryStatus;
			transferLine.WE_CurrentInventoryStatus = transferlineCurrentInventoryStatus;
			AssertEquals("Precondition", transferLineOriginalInventoryStatus, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", transferlineCurrentInventoryStatus, transferLine.WE_CurrentInventoryStatus);

			var cloneTransferLine = (WhsTransferLine)transferLine.Clone();

			AssertEquals("CloneOriginalInventoryStatus: " + message, expectedCloneOriginalInventoryStatus, cloneTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("CloneCurrentInventoryStatus: " + message, expectedCloneCurrentInventoryStatus, cloneTransferLine.WE_CurrentInventoryStatus);
		}

		public void TestClone_InTransitInventory()
		{
			var whs = Helper.CreateWarehouse("Whs", "A");
			var client = Helper.CreateClient("C", "client");
			var product = Helper.CreateProduct(client, "Product");
			Factory.Save();
			var location = whs.FindLocation("A");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 1m, location, "");
			var transfer = Helper.CreateWhsTransfer(client, whs);
			var transferLine = Helper.CreateWhsTransferLine(transfer, product, 1m, location.ToLocationString(), "");
			transferLine.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var cloneTransferLine = (WhsTransferLine)transferLine.Clone();
			AssertEquals("Inventory status should be Available.", InventoryStatus.Codes.Available, cloneTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should be Available.", InventoryStatus.Codes.Available, cloneTransferLine.WE_CurrentInventoryStatus);
		}

		public void TestClone_InTransitInventory_HeldInventory()
		{
			var whs = Helper.CreateWarehouse("Whs", "A");
			var client = Helper.CreateClient("C", "client");
			var product = Helper.CreateProduct(client, "Product");
			Factory.Save();
			var location = whs.FindLocation("A");
			var receive = Helper.CreateWhsReceive(client, whs, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, product, 1m, location);
			inventory.OriginalInventoryHeldCode = InventoryStatus.Codes.Held;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(client, whs);
			var transferLine = Helper.CreateWhsTransferLine(transfer, product, 1m, location.ToLocationString(), "", InventoryHoldCodes.Codes.Held);
			transferLine.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var cloneTransferLine = (WhsTransferLine)transferLine.Clone();
			AssertEquals("Inventory status in clone should set to Held based on InventoryHeldCode.", InventoryStatus.Codes.Held, cloneTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status in clone should set to Held based on InventoryHeldCode.", InventoryStatus.Codes.Held, cloneTransferLine.WE_CurrentInventoryStatus);
		}

		#region TestClone_MatchingLines

		public void TestClone_MatchingLines()
		{
			var whs = Helper.CreateWarehouse("Whs", "A");
			var client = Helper.CreateClient("C", "client");
			var product = Helper.CreateProduct(client, "Product");
			Factory.Save();
			var location = whs.FindLocation("A");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 1m, location, "");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product, 1m, location, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(client, whs);
			var transferLine = Helper.CreateWhsTransferLine(transfer, product, 2m, location.ToLocationString(), "");
			transferLine.RunPreSaveValidation();
			var matchingLine = transferLine.MatchingLines[0];
			AssertEquals("Precondition", false, matchingLine.IsMainTransactionLine());

			foreach (var inventoryStatusCode in new string[] { InventoryStatus.Codes.Available, InventoryStatus.Codes.Held, InventoryStatus.Codes.InTransit })
			{
				AssertCloneStatusCore(message: "Cloned line Inventory Status should be based on the matching line original inventory status (copy without change).",
					transferLine: matchingLine,
					inventoryHoldCode: "",
					transferLineOriginalInventoryStatus: inventoryStatusCode,
					transferlineCurrentInventoryStatus: inventoryStatusCode,
					expectedCloneOriginalInventoryStatus: inventoryStatusCode,
					expectedCloneCurrentInventoryStatus: inventoryStatusCode);

				AssertCloneStatusCore(message: "Cloned line Inventory Status should be based on the matching line original inventory hold code.",
					transferLine: matchingLine,
					inventoryHoldCode: InventoryHoldCodes.Codes.Damaged,
					transferLineOriginalInventoryStatus: inventoryStatusCode,
					transferlineCurrentInventoryStatus: inventoryStatusCode,
					expectedCloneOriginalInventoryStatus: InventoryStatus.Codes.Held,
					expectedCloneCurrentInventoryStatus: InventoryStatus.Codes.Held);
			}
		}

		public void TestClone_MatchingLines_InTransitInventory()
		{
			var whs = Helper.CreateWarehouse("Whs", "A");
			var client = Helper.CreateClient("C", "client");
			var product = Helper.CreateProduct(client, "Product");
			Factory.Save();
			var location = whs.FindLocation("A");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 1m, location, "");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product, 1m, location, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(client, whs);
			var transferLine = Helper.CreateWhsTransferLine(transfer, product, 2m, location.ToLocationString(), "");
			transferLine.RunPreSaveValidation();
			var matchingLine = transferLine.MatchingLines[0];
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			AssertEquals("Inventory status change should propogate from master.", InventoryStatus.Codes.InTransit, matchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status change should propogate from master.", InventoryStatus.Codes.InTransit, matchingLine.WE_CurrentInventoryStatus);

			var cloneTransferLine = (WhsTransferLine)transferLine.Clone();
			transfer.Lines.Add(cloneTransferLine);
			transferLine.Delete();
			cloneTransferLine.RunPreSaveValidation();
			var cloneMatchingLine = cloneTransferLine.MatchingLines[0];
			AssertEquals("Inventory status should be Available.", InventoryStatus.Codes.Available, cloneTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should be Available.", InventoryStatus.Codes.Available, cloneTransferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory status should propogate from master.", InventoryStatus.Codes.Available, cloneMatchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should propogate from master.", InventoryStatus.Codes.Available, cloneMatchingLine.WE_CurrentInventoryStatus);
		}

		#endregion

		#endregion

		#region TestOnSave_FinalisedTransferLineDontSetLocationsLastAllocatedOrChangedDate

		[TestDate(2018, 12, 12)]
		public void TestOnSave_FinalisedTransferLineDontSetLocationsLastAllocatedOrChangedDate()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(now.Year, 1, 1), data.Part1, 10m, locations[0], "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locations[0].PK, locations[1].PK);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Change to a WE_WL should update new locations WLV_LastAllocatedOrChangedDateUtc.", now.Date, locations[1].WLV_LastAllocatedOrChangedDateUtc);

			TestDateAttribute.Date = now.AddDays(1).ToDateTime();
			transferLine.FinaliseDocketLine();
			transferLine.WE_CustomDate1 = now.ToLocalZDateTime();
			Factory.Save();

			AssertEquals("Change to a WE_WL when finalised should not update a locations WLV_LastAllocatedOrChangedDateUtc.", now.Date, locations[1].WLV_LastAllocatedOrChangedDateUtc);
		}

		#endregion

		#region FinaliseInTransitInventory

		public void TestFinaliseDocketLine_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory20 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, locationA1);
			var inventory15 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, locationA1);
			inventory15.OriginalInventoryHeldCode = InventoryStatus.Codes.Held;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine20 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 20m, "A-1", "A-2");
			var transferLine15 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 15m, "A-1", "A-2", InventoryHoldCodes.Codes.Held);
			transferLine20.RunPreSaveValidation(); // to commit inventory
			transferLine15.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			transferLine20.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine20.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine20.IsFinalised);

			transferLine15.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine15.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine15.IsFinalised);

			transferLine20.FinaliseDocketLine();
			AssertEquals("Transfer line should be finalised.", true, transferLine20.IsFinalised);
			AssertEquals("Make sure the total units have not changed", 20m, transferLine20.WE_StockOnHand);

			transferLine15.FinaliseDocketLine();
			AssertEquals("Transfer line should be finalised.", true, transferLine15.IsFinalised);
			AssertEquals("Make sure the total units have not changed", 15m, transferLine15.WE_StockOnHand);

			var inventoryA1 = Helper.LoadInventory(locationA1);
			var inventoryA2 = Helper.LoadInventory(locationA2);
			AssertEquals("No stock should be in A1.", 0m, inventoryA1.UnitsTotal);
			AssertEquals("All stock should be in A2.", 35m, inventoryA2.UnitsTotal);
			AssertEquals("0 inventory records should be in A1.", 0, inventoryA1.Inventory.Count());
			AssertEquals("2 inventory records should be in A2.", 2, inventoryA2.Inventory.Count());
			inventoryA2.Inventory.Single(i => i.WI_InventoryStatus == InventoryStatus.Codes.Available && i.WI_TotalUnits == 20m);
			inventoryA2.Inventory.Single(i => i.WI_InventoryStatus == InventoryStatus.Codes.Held && i.WI_TotalUnits == 15m);
		}

		public void TestFinaliseDocketLine_InTransit_ParentPickForTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Available (original Inventory's status).", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
		}

		public void TestFinaliseDocketLine_InTransit_ParentPickForTransfer_PackingStation()
		{
			TestFinaliseDocketLine_InTransit_ParentPickForTransfer_PackingLocationCore(LocationClasses.Codes.PST);
		}

		public void TestFinaliseDocketLine_InTransit_ParentPickForTransfer_PackingConsolidation()
		{
			TestFinaliseDocketLine_InTransit_ParentPickForTransfer_PackingLocationCore(LocationClasses.Codes.CON);
		}

		void TestFinaliseDocketLine_InTransit_ParentPickForTransfer_PackingLocationCore(string locationClass)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, locationClass);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Available (original Inventory's status).", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be ReadyToPack.", InventoryStatus.Codes.ReadyToPack, transferLine.WE_CurrentInventoryStatus);
		}

		public void TestFinaliseDocketLine_InTransit_PickByBOMComponents_PackingStation()
		{
			TestFinaliseDocketLine_InTransit_PickByBOMComponents_Core(LocationClasses.Codes.PST);
		}

		public void TestFinaliseDocketLine_InTransit_PickByBOMComponents_PackingConsolidation()
		{
			TestFinaliseDocketLine_InTransit_PickByBOMComponents_Core(LocationClasses.Codes.CON);
		}

		public void TestFinaliseDocketLine_InTransit_PickByBOMComponents_DockDoor()
		{
			TestFinaliseDocketLine_InTransit_PickByBOMComponents_Core(LocationClasses.Codes.DDL);
		}

		void TestFinaliseDocketLine_InTransit_PickByBOMComponents_Core(string locationClass)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var date1 = new ZDateTime(2024, 4, 1, 7, 0, 0);
			var dateOffset1 = data.Whs1.GetWarehouseBranchLocalDateTimeOffset(date1);
			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, locationClass);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, location1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			pick.ClearInventoryCache();

			// mock picking component lines and putaway, the WE_WL of component transfer line will be set to an inventory location
			var componentPickLine = orderLine.ChildComponentLines.Single().PickLines.Single();
			var componentTransferLine = Helper.PickAndMakeInTransitTransfer(componentPickLine, ZDateTimeOffset.Now);
			componentTransferLine.WE_WL = location1.PK;

			var kitPickLine = orderLine.PickLines.Single();
			var kitReceiveLine = kitPickLine.InventoryLine;

			kitReceiveLine.WE_WL = location1.PK;
			kitReceiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway;
			kitReceiveLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Putaway;
			kitReceiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.PickedForUnload;
			kitReceiveLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			kitReceiveLine.WE_UnloadedTime = ZDateTimeOffset.Now;
			kitReceiveLine.WE_GS_NKUnloadedBy = GlbStaff.CurrentUser.GS_Code;

			var kitTransferLine = Helper.PickAndMakeInTransitTransfer(kitPickLine, ZDateTimeOffset.Now);
			kitTransferLine.WE_WL = packingLocation.PK;

			var expectedStatus = (locationClass == LocationClasses.Codes.PST || locationClass == LocationClasses.Codes.CON)
				? InventoryStatus.Codes.ReadyToPack
				: InventoryStatus.Codes.Staged;

			AssertEquals("Precondition", InventoryStatus.Codes.InTransit, componentTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.InTransit, componentTransferLine.WE_CurrentInventoryStatus);
			componentTransferLine.FinaliseDocketLine();
			AssertEquals(InventoryStatus.Codes.Available, componentTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("This is intentional as the Component Lines are staged to be assembled from the Order's point of view", InventoryStatus.Codes.Staged, componentTransferLine.WE_CurrentInventoryStatus);

			AssertEquals("Precondition", InventoryStatus.Codes.InTransit, kitTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.InTransit, kitTransferLine.WE_CurrentInventoryStatus);
			kitTransferLine.FinaliseDocketLine();
			AssertEquals(InventoryStatus.Codes.Available, kitTransferLine.WE_OriginalInventoryStatus);
			AssertEquals(expectedStatus, kitTransferLine.WE_CurrentInventoryStatus);
		}

		public void TestFinaliseDocketLine_InterWhsSourceTransfer_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var locationA = whs1.DefaultLocation;
			var locationB = whs2.DefaultLocation;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, locationA, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			transferLine.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var inventoryA = Helper.LoadInventory(locationA);
			var inventoryB = Helper.LoadInventory(locationB);
			AssertEquals("Precondition - all stock should be in A.", 10m, inventoryA.UnitsTotal);
			AssertEquals("Precondition - no stock should be in B.", 0m, inventoryB.UnitsTotal);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine.IsFinalised);
			AssertEquals("Precondition - Total Units in Source transfer should be 0.", 0m, transferLine.WE_StockOnHand);

			var childLine = transferLine.ChildTransferLine;
			AssertNotNull("Should have created child transfer line.", childLine);
			AssertEquals("Precondition - child should be InTransit", InventoryStatus.Codes.InTransit, childLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - child should have total units.", 10m, childLine.WE_StockOnHand);
			Factory.Save();

			transferLine.FinaliseDocketLine();
			AssertEquals("Child line should be resynchronised.", 10m, childLine.WE_StockOnHand);
			AssertEquals("Child line should be resynchronised.", data.Part1.PK, childLine.WE_OP);

			inventoryA = Helper.LoadInventory(locationA);
			inventoryB = Helper.LoadInventory(locationB);
			AssertEquals("Transfer line should be finalised.", true, transferLine.IsFinalised);
			AssertEquals("No stock should be in A.", 0m, inventoryA.UnitsTotal);
			AssertEquals("All stock should be in B.", 10m, inventoryB.UnitsTotal);
			AssertNotEquals("Inventory created from Inter Warehouse Transfers should not keep link to original receive.", receive.Lines[0].PK, inventoryB.Inventory.First().WI_WE_OriginalInDocketLineForRating);
		}

		public void TestFinaliseDocketLine_InterWhsDestinationTransfer_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var whs3 = Helper.CreateWarehouse("WH3", "C");
			Factory.Save();

			var location1 = whs1.DefaultLocation;
			var location2 = whs2.DefaultLocation;
			var location3 = whs3.DefaultLocation;
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", data.Part1, 50m, whs2.DefaultLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, whs3, "R3", data.Part1, 50m, whs3.DefaultLocation, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine10 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "B", whs2.PK, "A");
			var transferLine15 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "C", whs3.PK, "A");
			transferLine10.RunPreSaveValidation(); // to commit inventory
			transferLine15.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var inventory1 = Helper.LoadInventory(location1);
			var inventory2 = Helper.LoadInventory(location2);
			var inventory3 = Helper.LoadInventory(location3);
			AssertEquals("Precondition - no stock should be in L1.", 0m, inventory1.UnitsTotal);
			AssertEquals("Precondition - stock should be in L2.", 50m, inventory2.UnitsTotal);
			AssertEquals("Precondition - stock should be in L3.", 50m, inventory3.UnitsTotal);

			transferLine10.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine10.WE_DocketLineStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine10.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine10.IsFinalised);

			var childLine = transferLine10.ChildTransferLine;
			AssertNotNull("Should have created child transfer line.", childLine);
			AssertEquals("Precondition - child should be InTransit", InventoryStatus.Codes.InTransit, childLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - child should *not* have total units.", 0m, childLine.WE_StockOnHand);

			transferLine15.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine15.WE_DocketLineStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine15.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine15.IsFinalised);
			Factory.Save();

			transferLine10.FinaliseDocketLine();
			AssertEquals("Precondition - check total units.", 10m, transferLine10.WE_StockOnHand);
			AssertEquals("Child line should have no total units.", 0m, childLine.WE_StockOnHand);

			transferLine15.FinaliseDocketLine();
			AssertEquals("Precondition - check total units.", 15m, transferLine15.WE_StockOnHand);

			inventory1 = Helper.LoadInventory(location1);
			inventory2 = Helper.LoadInventory(location2);
			inventory3 = Helper.LoadInventory(location3);

			AssertEquals("Transfer line should be finalised.", true, transferLine10.IsFinalised);
			AssertEquals("Transfer line should be finalised.", true, transferLine15.IsFinalised);

			AssertEquals("Precondition - moved stock should be in L1.", 25m, inventory1.UnitsTotal);
			AssertEquals("Precondition - remaining stock should be in L2.", 40m, inventory2.UnitsTotal);
			AssertEquals("Precondition - remaining stock should be in L3.", 35m, inventory3.UnitsTotal);
			AssertNotEquals("Inventory created from Inter Warehouse Transfers should not keep link to original receive.", receive1.Lines[0].PK, inventory1.Inventory.First().WI_WE_OriginalInDocketLineForRating);
			AssertNotEquals("Inventory created from Inter Warehouse Transfers should not keep link to original receive.", receive2.Lines[0].PK, inventory1.Inventory.First().WI_WE_OriginalInDocketLineForRating);
		}

		public void TestFinaliseDocketLine_MatchingLines_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 7m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m, locationA1, "");

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 13m, locationA1.ToLocationString(), locationA2.ToLocationString());
			transferLine.RunPreSaveValidation();
			var matchingLine = transferLine.MatchingLines[0];

			AssertEquals("Precondition: Stock is committed.", 6m, transferLine.GetQtyCommittedToThisLine());
			AssertEquals("Precondition: Stock is committed.", 13m, transferLine.QtyCommittedIncludingMatchingLines);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Total units and Committed quantity should have the same value.", 6m, transferLine.WE_StockOnHand);
			AssertEquals("Total units and Committed quantity should have the same value.", 6m, transferLine.PickLines.GetQtyCommitted());

			AssertEquals("Inventory status change should propogate from master.", InventoryStatus.Codes.InTransit, matchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Inventory status change should propogate from master.", InventoryStatus.Codes.InTransit, matchingLine.WE_CurrentInventoryStatus);
			AssertEquals("Stock on hand change should propogate from master.", 7m, matchingLine.WE_StockOnHand);
			AssertEquals("Stock on hand change should propogate from master.", 7m, matchingLine.PickLines.GetQtyCommitted());

			transferLine.FinaliseDocketLine();
			AssertEquals("Transfer line should be finalised.", true, transferLine.IsFinalised);
			AssertEquals("Make sure the total units have not changed.", 6m, transferLine.WE_StockOnHand);

			AssertEquals("Matching line should be finalised.", true, matchingLine.IsFinalised);
			AssertEquals("Make sure the total units on matching line have not changed.", 7m, matchingLine.WE_StockOnHand);

			var inventoryA1 = Helper.LoadInventory(locationA1);
			var inventoryA2 = Helper.LoadInventory(locationA2);
			AssertEquals("No stock should remain in source location.", 0m, inventoryA1.UnitsTotal);
			AssertEquals("All stock should be in destination location.", 13m, inventoryA2.UnitsTotal);
		}

		public void TestFinaliseDocketLine_MatchingLines_InTransit_ParentPickForTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WE_MatchingLine = transferLine1.PK; // Hack so I can test this datashape which may eventually be valid

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine1.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine2.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, transferLine2.WE_CurrentInventoryStatus);
			transferLine1.FinaliseDocketLine();
			AssertEquals("Precondition - should be Available (original Inventory's status).", InventoryStatus.Codes.Available, transferLine1.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be Available (original Inventory's status).", InventoryStatus.Codes.Available, transferLine2.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, transferLine2.WE_CurrentInventoryStatus);
		}

		#region TestFinaliseDocketLine_MatchingLines_InterWhs_InTransit

		public void TestFinaliseDocketLine_MatchingLines_InterWhs_InTransit_Dest()
		{
			TestFinaliseDocketLine_MatchingLines_InterWhs_InTransit(isSource: false);
		}

		public void TestFinaliseDocketLine_MatchingLines_InterWhs_InTransit_Source()
		{
			TestFinaliseDocketLine_MatchingLines_InterWhs_InTransit(isSource: true);
		}

		void TestFinaliseDocketLine_MatchingLines_InterWhs_InTransit(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var transferWhs = isSource ? data.Whs1 : whs2;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferWhs, "TR1");
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A", isSource ? whs2.PK : data.Whs1.PK, "A");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = ZDateTimeOffset.Now;
			var childTransfer = transfer.ChildTransfers.First();
			var childLine = transferLine.ChildTransferLine;
			AssertNotNull("Precondition: Created child line.", childLine);
			Factory.Save();

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);

			var inventoryInWhs1 = Helper.LoadInventory(data.Whs1.FindLocation("A"));
			var inventoryInWhs2 = Helper.LoadInventory(whs2.FindLocation("A"));
			AssertEquals("No stock should be in Whs1.", 0m, inventoryInWhs1.UnitsTotal);
			AssertEquals("All stock should be in Whs2.", 50m, inventoryInWhs2.UnitsTotal);
		}

		#endregion

		#endregion

		#region Finalisation

		#region TestTruncateSecondsInFinaliseDate

		[TestDate(2012, 3, 13, 5, 5, 5)]
		public void TestSecondsInFinaliseDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine1.FinaliseDocketLine();
			AssertEquals("Transfer line should be finalised.", true, transferLine1.IsFinalised);
			AssertEquals("Transfer line SHOULD NOT truncate FinalizedDate seconds.", 5, transferLine1.WE_FinalisedDate.Second);
		}

		#endregion

		#region TestFinaliseDocketLine_WithOverridenFinalisedTime

		[TestDate(2017, 1, 1)]
		public void TestFinaliseDocketLine_WithOverridenFinalisedTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();
			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;

			var provider = new Provider { FinaliseTimeUTC = new ZDateTimeOffset(2017, 2, 2) };
			using (transfer.SetFinalisedDateProvider(provider))
			{
				transfer.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(transferLine);
				AssertEquals("Finalised Date is Overriden with the Correct value.", data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2017, 2, 2)), transferLine.WE_FinalisedDate);
			}
		}

		class Provider : IFinalisedDateProvider
		{
			ZDateTimeOffset IFinalisedDateProvider.GetFinalisationTimeOffset() => FinaliseTimeUTC;

			public ZDateTimeOffset FinaliseTimeUTC { get; set; }
		}

		#endregion

		#region TestFinaliseDocketLine

		[TestDate(2012, 3, 13, 5, 5, 5)]
		public void TestFinaliseDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var finalisedDate = ZDateTime.Now;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, locationA1, locationA2);
			var inventoryA1 = Helper.LoadInventory(locationA1);
			var inventoryA2 = Helper.LoadInventory(locationA2);
			AssertEquals("Precondition - transfer should not be finalised.", false, transfer.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine1.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine2.IsFinalised);
			AssertEquals("Precondition - all stock should be in A1.", 50m, inventoryA1.UnitsAvailable);
			AssertEquals("Precondition - no stock should be in A2.", 0m, inventoryA2.UnitsAvailable);

			transferLine1.FinaliseDocketLine();
			inventoryA1 = Helper.LoadInventory(locationA1);
			inventoryA2 = Helper.LoadInventory(locationA2);
			AssertEquals("Transfer should not be finalised.", false, transfer.IsFinalised);
			AssertEquals("Transfer line should be finalised.", true, transferLine1.IsFinalised);
			AssertEquals("Transfer line should not be finalised.", false, transferLine2.IsFinalised);
			AssertEquals("Transfer line finalize date is incorrect.", data.Whs1.GetWarehouseBranchDateTimeOffset(finalisedDate), transferLine1.WE_FinalisedDate);
			AssertEquals("Transfer line finalize date is incorrect.", ZDateTimeOffset.Empty, transferLine2.WE_FinalisedDate);
			AssertEquals("Some stock should be moved out of A1.", 40m, inventoryA1.UnitsTotal);
			AssertEquals("Some stock should be moved into A2.", 10m, inventoryA2.UnitsTotal);
			AssertEquals("Inventory created from Inner Warehouse Transfers should keep link to the original receive.", receive.Lines[0].PK, transferLine1.Inventory[0].WI_WE_OriginalInDocketLineForRating);

			transferLine2.FinaliseDocketLine();
			inventoryA1 = Helper.LoadInventory(locationA1);
			inventoryA2 = Helper.LoadInventory(locationA2);
			AssertEquals("Transfer should not be finalised.", false, transfer.IsFinalised);
			AssertEquals("Transfer line should be finalised.", true, transferLine1.IsFinalised);
			AssertEquals("Transfer line should be finalised.", true, transferLine2.IsFinalised);
			AssertEquals("Transfer line finalize date is incorrect.", data.Whs1.GetWarehouseBranchDateTimeOffset(finalisedDate), transferLine1.WE_FinalisedDate);
			AssertEquals("Transfer line finalize date is incorrect.", data.Whs1.GetWarehouseBranchDateTimeOffset(finalisedDate), transferLine2.WE_FinalisedDate);
			AssertEquals("Some stock should be moved out of A1.", 25m, inventoryA1.UnitsTotal);
			AssertEquals("Some stock should be moved into A2.", 25m, inventoryA2.UnitsTotal);
			AssertEquals("Inventory created from Inner Warehouse Transfers should keep link to the original receive.", receive.Lines[0].PK, transferLine2.Inventory[0].WI_WE_OriginalInDocketLineForRating);
		}

		#endregion

		#region TestFinaliseDocketLine_SetsPickedTime

		[TestDate(2016, 1, 2)]
		public void TestFinaliseDocketLine_SetsPickedTime()
		{
			var staff = Helper.CreateGlbStaff("AA", "Antman");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 30m, locationA1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 35m, locationA1, locationA2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locationA1, locationA2);
			AssertEquals("Precondition - transfer should not be finalised.", false, transfer.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine1.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine2.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine3.IsFinalised);

			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: TransferLine is committed.", 35m, transferLine1.QtyCommittedIncludingMatchingLines);
			AssertEquals("Precondition: TransferLine is committed.", 10m, transferLine2.QtyCommittedIncludingMatchingLines);
			AssertEquals("Precondition: TransferLine is committed.", 5m, transferLine3.QtyCommittedIncludingMatchingLines);

			transferLine1.GS_NKPickedBy = "ZZ"; // assign Transfer line but don't pick.

			transferLine2.GS_NKPickedBy = "E";
			var today = ZDateTimeOffset.Today;
			var transferLinePickTime = today.AddDays(-2);
			transferLine2.PickedTime = transferLinePickTime; // pick transfer line
			AssertEquals("All Picked Times should be Picked Time on TransferLine.", true, GetAllPickLines(transferLine2).All(pl => pl.WZ_PickedDateTime == transferLinePickTime));

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				transferLine1.FinaliseDocketLine();
				transferLine2.FinaliseDocketLine();
				transferLine3.FinaliseDocketLine();
			}

			AssertEquals("Precondition: Finalised Date is set.", true, !transferLine1.WE_FinalisedDate.IsEmpty);
			AssertEquals("Precondition: Finalised Date is set.", true, !transferLine2.WE_FinalisedDate.IsEmpty);
			AssertEquals("Precondition: Finalised Date is set.", true, !transferLine3.WE_FinalisedDate.IsEmpty);
			AssertIsFinalisedPrecondition(transferLine1);
			AssertIsFinalisedPrecondition(transferLine2);
			AssertIsFinalisedPrecondition(transferLine3);
			AssertEquals("All Picked Times should be set to finalisation time.", true, GetAllPickLines(transferLine1).All(pl => pl.WZ_PickedDateTime == today && pl.WZ_GS_NKAssignedTo == "ZZ")); // don't override picker
			AssertEquals("Picked Times set before finalisation should not be changed.", true, GetAllPickLines(transferLine2).All(pl => pl.WZ_PickedDateTime == transferLinePickTime && pl.WZ_GS_NKAssignedTo == "E"));
			AssertEquals("All Picked Times should be set to finalisation time.", true, GetAllPickLines(transferLine3).All(pl => pl.WZ_PickedDateTime == today && pl.WZ_GS_NKAssignedTo == "AA"));
		}

		IEnumerable<WhsPickLine> GetAllPickLines(WhsTransferLine transferLine)
		{
			foreach (var pickLine in transferLine.PickLines)
			{
				yield return pickLine;
			}

			foreach (WhsTransferLine matchingLine in transferLine.MatchingLines)
			{
				foreach (var pickLine in GetAllPickLines(matchingLine))
				{
					yield return pickLine;
				}
			}
		}

		#endregion

		#region TestFinaliseDocketLine_SetsPutawayTimeAndPutawayBy

		[TestDate(2016, 1, 2)]
		public void TestFinaliseDocketLine_SetsPutawayTimeAndPutawayBy()
		{
			var staff = Helper.CreateGlbStaff("AA", "Antman");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var today = ZDate.Today;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 35m, locationA1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_WithPutawayBySet = Helper.CreateWhsTransferLine(transfer, data.Part1, 35m, locationA1, locationA2);
			var transferLine_WithNoPutawayFieldsSet = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locationA1, locationA2);
			AssertEquals("Precondition - transfer should not be finalised.", false, transfer.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine_WithPutawayBySet.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine_WithNoPutawayFieldsSet.IsFinalised);

			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: TransferLine is committed.", 35m, transferLine_WithPutawayBySet.QtyCommittedIncludingMatchingLines);
			AssertEquals("Precondition: TransferLine is committed.", 5m, transferLine_WithNoPutawayFieldsSet.QtyCommittedIncludingMatchingLines);

			transferLine_WithPutawayBySet.WE_GS_NKPutawayBy = "ZZ"; // assign Transfer line to be putaway but don't putaway.

			var transferLinePickTime = today.AddDays(-2);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				transferLine_WithPutawayBySet.FinaliseDocketLine();
				transferLine_WithNoPutawayFieldsSet.FinaliseDocketLine();
			}
			AssertIsFinalisedPrecondition(transferLine_WithPutawayBySet);
			AssertIsFinalisedPrecondition(transferLine_WithNoPutawayFieldsSet);
			AssertPutawayTimeAndPutawayBy(transferLine_WithPutawayBySet, ZDateTimeOffset.Now, "ZZ");
			AssertPutawayTimeAndPutawayBy(transferLine_WithNoPutawayFieldsSet, ZDateTimeOffset.Now, "AA");
		}

		[TestDate(2016, 1, 2)]
		public void TestFinaliseDocketLine_SetsPutawayTimeAndPutawayBy_InterWhsTransfer()
		{
			var staff = Helper.CreateGlbStaff("AA", "Antman");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var locationA1 = data.Whs1.FindLocation("A-1");
			var today = ZDate.Today;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, locationA1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 35m, "A-1", whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: TransferLine is committed.", 35m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				transferLine.FinaliseDocketLine();
			}

			var matchingTransferLine = (WhsTransferLine)transferLine.MatchingLines.Single();
			AssertIsFinalisedPrecondition(transferLine);
			AssertIsFinalisedPrecondition(matchingTransferLine);

			var childTransfer = transfer.ChildTransfers.Single();
			var childTransferLine = (WhsTransferLine)childTransfer.Lines.Single();
			var childTransferMatchingLine = (WhsTransferLine)childTransferLine.MatchingLines.Single();
			AssertIsFinalisedPrecondition(childTransferLine);
			AssertIsFinalisedPrecondition(childTransferMatchingLine);

			AssertPutawayTimeAndPutawayBy(transferLine, ZDateTimeOffset.Now, "AA");
			AssertPutawayTimeAndPutawayBy(matchingTransferLine, ZDateTimeOffset.Now, "AA");
			AssertPutawayTimeAndPutawayBy(childTransferLine, ZDateTimeOffset.Now, "AA");
			AssertPutawayTimeAndPutawayBy(childTransferMatchingLine, ZDateTimeOffset.Now, "AA");
		}

		[TestDate(2023, 12, 12)]
		public void TestFinaliseDocketLine_SetsPutawayTimeAndPutawayBy_WithInvalidBranchHomePort()
		{
			var staff = Helper.CreateGlbStaff("AA", "Antman");
			var dummyBranch = Helper.CreateGlbBranch("ABC");
			dummyBranch.GB_RL_NKHomePort = "XXYZZ";

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_GB_RelatedCompanyBranch = dummyBranch.PK;
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 35m, locationA1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_WithPutawayBySet = Helper.CreateWhsTransferLine(transfer, data.Part1, 35m, locationA1, locationA2);
			var transferLine_WithNoPutawayFieldsSet = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locationA1, locationA2);
			AssertEquals("Precondition - transfer should not be finalised.", false, transfer.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine_WithPutawayBySet.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine_WithNoPutawayFieldsSet.IsFinalised);

			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: TransferLine is committed.", 35m, transferLine_WithPutawayBySet.QtyCommittedIncludingMatchingLines);
			AssertEquals("Precondition: TransferLine is committed.", 5m, transferLine_WithNoPutawayFieldsSet.QtyCommittedIncludingMatchingLines);

			transferLine_WithPutawayBySet.WE_GS_NKPutawayBy = "ZZ"; // assign Transfer line to be putaway but don't putaway.
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertNoExceptionThrown(() => transferLine_WithPutawayBySet.FinaliseDocketLine());
				AssertNoExceptionThrown(() => transferLine_WithNoPutawayFieldsSet.FinaliseDocketLine());
			}
			AssertIsFinalisedPrecondition(transferLine_WithPutawayBySet);
			AssertPutawayTimeAndPutawayBy(transferLine_WithPutawayBySet, ZDateTimeOffset.Now, "ZZ");

			AssertIsFinalisedPrecondition(transferLine_WithNoPutawayFieldsSet);			
			AssertPutawayTimeAndPutawayBy(transferLine_WithNoPutawayFieldsSet, ZDateTimeOffset.Now, "AA");
		}

		void AssertPutawayTimeAndPutawayBy(WhsTransferLine transferLine, ZDateTimeOffset putawayTime, ZString putawayBy)
		{
			foreach (var line in transferLine.MatchingLines.Concat(new[] { transferLine }))
			{
				AssertEquals("PutawayTime", putawayTime, line.WE_PutawayTime);
				AssertEquals("PutawayBy", putawayBy, line.WE_GS_NKPutawayBy);
			}
		}

		#endregion

		#region TestFinaliseDocketLineWithReceiveParams

		[TestDate(2012, 3, 13, 5, 5, 5)]
		public void TestFinaliseDocketLineWithReceiveParams()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Part1.OP_CountDecimalPlaces = 2;
			data.Part1.PartUnits.GetUnitConversion(Constants.PkgUnit.Carton, Constants.PkgUnit.Unit).OF_QuantityInParent = 10m;
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1).W3_F3_NKReceivedPackType = Constants.PkgUnit.Carton;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, sourceLocation, "", false);
			AssertEquals("Precondition", Constants.PkgUnit.Carton, receive.Lines.Single().WE_F3_NKPackType);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation, destinationLocation);
			line.WE_F3_NKPackType = Constants.PkgUnit.Unit;
			AssertEquals("Precondition", Constants.PkgUnit.Unit, line.WE_F3_NKPackType);

			line.FinaliseDocketLine();
			AssertEquals(true, line.IsFinalised);
			AssertEquals(Constants.PkgUnit.Unit, line.WE_F3_NKPackType);
			AssertEquals(Constants.PkgUnit.Unit, line.Inventory.Cast<WhsInventoryView>().Single().WI_F3_NKPackType);
		}

		#endregion

		#region TestFinaliseDocketLine_WithoutDestLocation

		public void TestFinaliseDocketLine_WithoutDestLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			transferLine.FinaliseDocketLine();
			AssertEquals("Transfer line should not be finalised without Destination location.", false, transferLine.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocketLine_KeepsOriginalInventoryStatus

		public void TestFinaliseDocketLine_KeepsOriginalInventoryStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryAvailable = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			var inventoryHeld = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, locationA1);
			var inventoryDamaged = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, locationA1);
			inventoryHeld.OriginalInventoryHeldCode = InventoryStatus.Codes.Held;
			inventoryDamaged.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLineAvailable = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLineHeld = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 15m, "A-1", "A-2", InventoryHoldCodes.Codes.Held);
			var transferLineDamaged = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 20m, "A-1", "A-2", InventoryHoldCodes.Codes.Damaged);
			var inventoryA1 = Helper.LoadInventory(locationA1);
			var inventoryA2 = Helper.LoadInventory(locationA2);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLineAvailable.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLineHeld.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLineDamaged.IsFinalised);
			AssertEquals("Precondition - all stock should be in A1.", 45m, inventoryA1.UnitsTotal);
			AssertEquals("Precondition - no stock should be in A2.", 0m, inventoryA2.UnitsTotal);
			AssertEquals("Precondition - 3 inventory records should be in A1.", 3, inventoryA1.Inventory.Count());
			AssertEquals("Precondition - no inventory records should be in A2.", 0, inventoryA2.Inventory.Count());

			transferLineAvailable.FinaliseDocketLine();
			transferLineHeld.FinaliseDocketLine();
			transferLineDamaged.FinaliseDocketLine();
			inventoryA1 = Helper.LoadInventory(locationA1);
			inventoryA2 = Helper.LoadInventory(locationA2);
			AssertEquals("Transfer line should be finalised.", true, transferLineAvailable.IsFinalised);
			AssertEquals("Transfer line should be finalised.", true, transferLineHeld.IsFinalised);
			AssertEquals("Transfer line should be finalised.", true, transferLineDamaged.IsFinalised);
			AssertEquals("No stock should be in A1.", 0m, inventoryA1.UnitsTotal);
			AssertEquals("All stock should be in A2.", 45m, inventoryA2.UnitsTotal);
			AssertEquals("3 inventory records should be in A1.", 0, inventoryA1.Inventory.Count());
			AssertEquals("3 inventory records should be in A2.", 3, inventoryA2.Inventory.Count());
			inventoryA2.Inventory.Single(i => i.WI_InventoryStatus == InventoryStatus.Codes.Available && i.WI_TotalUnits == 10m);
			inventoryA2.Inventory.Single(i => i.WI_InventoryStatus == InventoryStatus.Codes.Held && i.WI_TotalUnits == 15m);
			inventoryA2.Inventory.Single(i => i.WI_InventoryStatus == InventoryStatus.Codes.Held && i.IsDamaged && i.WI_TotalUnits == 20m);
		}

		#endregion

		#region TestFinaliseDocketLine_KeepsOriginalArrivalDate

		public void TestFinaliseDocketLine_KeepsOriginalArrivalDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var today = ZDateTimeOffset.Today;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, locationA1);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, locationA1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			inventory1.WI_ArrivalDate = today.AddDays(-1);
			inventory2.WI_ArrivalDate = today.AddDays(-2);
			inventory3.WI_ArrivalDate = today.AddDays(-3);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 45m, locationA1, locationA2);
			var inventoryA1 = Helper.LoadInventory(locationA1);
			var inventoryA2 = Helper.LoadInventory(locationA2);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine.IsFinalised);
			AssertEquals("Precondition - all stock should be in A1.", 45m, inventoryA1.UnitsTotal);
			AssertEquals("Precondition - no stock should be in A2.", 0m, inventoryA2.UnitsTotal);
			AssertEquals("Precondition - 3 inventory records should be in A1.", 3, inventoryA1.Inventory.Count());
			AssertEquals("Precondition - no inventory records should be in A2.", 0, inventoryA2.Inventory.Count());

			transferLine.FinaliseDocketLine();
			inventoryA1 = Helper.LoadInventory(locationA1);
			inventoryA2 = Helper.LoadInventory(locationA2);
			AssertEquals("Transfer line should be finalised.", true, transferLine.IsFinalised);
			AssertEquals("No stock should be in A1.", 0m, inventoryA1.UnitsTotal);
			AssertEquals("All stock should be in A2.", 45m, inventoryA2.UnitsTotal);
			AssertEquals("3 inventory records should be in A1.", 0, inventoryA1.Inventory.Count());
			AssertEquals("3 inventory records should be in A2.", 3, inventoryA2.Inventory.Count());
			inventoryA2.Inventory.Single(i => i.WI_ArrivalDate == today.AddDays(-1) && i.WI_TotalUnits == 10m);
			inventoryA2.Inventory.Single(i => i.WI_ArrivalDate == today.AddDays(-2) && i.WI_TotalUnits == 15m);
			inventoryA2.Inventory.Single(i => i.WI_ArrivalDate == today.AddDays(-3) && i.WI_TotalUnits == 20m);
		}

		#endregion

		#region TestFinaliseDocketLine_CommitAndValidate

		public void TestFinaliseDocketLine_CommitAndValidate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, data.Whs1.FindLocation("A-1"), "");

			Factory.Save();

			var expectedErrorMessage = @"Attempted to transfer 50 Units, but only 20 Units are available for transfer out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the transfer line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to transfer.
If you are trying to transfer stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to transfer stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.";

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			AssertEquals("Precondition - transfer should not be finalised.", false, transfer.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine.IsFinalised);
			AssertEquals("Precondition - no stock should be committed.", 0m, transferLine.GetQtyCommittedToThisLine());
			AssertEquals("Precondition - no stock should be committed.", 0m, receive.Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertNoError(transferLine.WE_TransactionQuantityInfo, expectedErrorMessage);

			transferLine.FinaliseDocketLine();
			AssertEquals("Transfer should not be finalised.", false, transfer.IsFinalised);
			AssertEquals("Transfer line should not be finalised.", false, transferLine.IsFinalised);
			AssertEquals("Stock should be committed.", 20m, transferLine.GetQtyCommittedToThisLine());
			AssertEquals("Stock should be committed.", 20m, receive.Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt);
			AssertHasError(transferLine.QtyToMoveIncludingMatchingLinesInfo, expectedErrorMessage);
		}

		#endregion

		#region TestFinaliseDocketLine_AbortsAndNotifiesUserIfLineIsAlreadyFinalised

		public void TestFinaliseDocketLine_AbortsAndNotifiesUserIfLineIsAlreadyFinalised()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			transferLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertIsFinalisedPrecondition(transferLine);

			transferLine.FinaliseDocketLine();

			var notify = (NotificationBuffer)transfer.NotificationManager.Peek;
			AssertEquals(true, notify.ContainsNotificationType(WhsErrorTypes.LineIsFinalised));
		}

		#endregion

		#region TestFinaliseDocketLine_InterWhsSourceTransfer

		public void TestFinaliseDocketLine_InterWhsSourceTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var locationA = data.Whs1.DefaultLocation;
			var locationB = whs2.DefaultLocation;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			var inventoryA = Helper.LoadInventory(locationA);
			var inventoryB = Helper.LoadInventory(locationB);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine.IsFinalised);
			AssertEquals("Precondition - all stock should be in A.", 10m, inventoryA.UnitsTotal);
			AssertEquals("Precondition - no stock should be in B.", 0m, inventoryB.UnitsTotal);

			transferLine.FinaliseDocketLine();
			inventoryA = Helper.LoadInventory(locationA);
			inventoryB = Helper.LoadInventory(locationB);
			AssertEquals("Transfer line should be finalised.", true, transferLine.IsFinalised);
			AssertEquals("No stock should be in A.", 0m, inventoryA.UnitsTotal);
			AssertEquals("All stock should be in B.", 10m, inventoryB.UnitsTotal);
			AssertNotEquals("Inventory created from Inter Warehouse Transfers should not keep link to original receive.", receive.Lines[0].PK, inventoryB.Inventory.First().WI_WE_OriginalInDocketLineForRating);
		}

		#endregion

		#region TestFinaliseDocketLine_InterWhsSourceTransfer_WithCustomAttributes

		public void TestFinaliseDocketLine_InterWhsSourceTransfer_WithCustomAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var locationA = data.Whs1.DefaultLocation;
			var locationB = whs2.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA, "");

			var docketLabels = new WhsDocket.CustomLabelsProvider(receive).GetCustomFields(data.Org1, Factory);
			var docketLineLabels = new WhsInventoryView.CustomLabelsProvider(inventory).GetCustomFields(data.Org1, Factory);
			Helper.SetAllCustomLabels(data.Org1, docketLabels, true);
			Helper.SetAllCustomLabels(data.Org1, docketLineLabels, true);
			Helper.SetDocketCustomAttributes(receive, "CA1", "CA2", "CA3", "CA4", "CA5", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), 1m, 2m, 3m, 4m, 5m, true, true, true, true, true);
			Helper.SetInventoryCustomAttributes(inventory, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1m, 2m, 3m, 4m, 5m, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), true, true, true, true, true, "Blob1");

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			Helper.SetDocketCustomAttributes(transfer, "CA1", "CA2", "CA3", "CA4", "CA5", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), 1m, 2m, 3m, 4m, 5m, true, true, true, true, true);
			Helper.SetDocketLineCustomAttributes(transferLine, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1m, 2m, 3m, 4m, 5m, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), true, true, true, true, true, "Blob1");

			var inventoryA = Helper.LoadInventory(locationA);
			var inventoryB = Helper.LoadInventory(locationB);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine.IsFinalised);
			AssertEquals("Precondition - all stock should be in A.", 10m, inventoryA.UnitsTotal);
			AssertEquals("Precondition - no stock should be in B.", 0m, inventoryB.UnitsTotal);

			transferLine.FinaliseDocketLine();
			inventoryA = Helper.LoadInventory(locationA);
			inventoryB = Helper.LoadInventory(locationB);
			AssertEquals("Transfer line should be finalised.", true, transferLine.IsFinalised);
			AssertEquals("No stock should be in A.", 0m, inventoryA.UnitsTotal);
			AssertEquals("All stock should be in B.", 10m, inventoryB.UnitsTotal);
			AssertEquals("Child transfer line should be finalised.", true, transferLine.ChildTransferLine.IsFinalised);
			Helper.AssertDocketCustomAttributes(transfer.ChildTransfers.ElementAt(0), "CA1", "CA2", "CA3", "CA4", "CA5", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), 1m, 2m, 3m, 4m, 5m, true, true, true, true, true);
			Helper.AssertDocketLineCustomAttributes(transferLine.ChildTransferLine, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1m, 2m, 3m, 4m, 5m, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), true, true, true, true, true, "Blob1");
		}

		#endregion

		#region TestFinaliseDocketLine_InterWhsDestTransfer

		public void TestFinaliseDocketLine_InterWhsDestTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var locationA = data.Whs1.DefaultLocation;
			var locationB = whs2.DefaultLocation;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs2, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", data.Whs1.PK, "B");
			var inventoryA = Helper.LoadInventory(locationA);
			var inventoryB = Helper.LoadInventory(locationB);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine.IsFinalised);
			AssertEquals("Precondition - all stock should be in A.", 10m, inventoryA.UnitsTotal);
			AssertEquals("Precondition - no stock should be in B.", 0m, inventoryB.UnitsTotal);

			transferLine.FinaliseDocketLine();
			inventoryA = Helper.LoadInventory(locationA);
			inventoryB = Helper.LoadInventory(locationB);
			AssertEquals("Transfer line should be finalised.", true, transferLine.IsFinalised);
			AssertEquals("No stock should be in A.", 0m, inventoryA.UnitsTotal);
			AssertEquals("All stock should be in B.", 10m, inventoryB.UnitsTotal);
			AssertNotEquals("Inventory created from Inter Warehouse Transfers should not keep link to original receive.", receive.Lines[0].PK, inventoryB.Inventory.First().WI_WE_OriginalInDocketLineForRating);
		}

		#endregion

		#region TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord

		[TestDate(2012, 3, 13, 5, 5, 5)]
		public void TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var staff1 = Helper.CreateGlbStaff("A.A", "AAA");
			var staff2 = Helper.CreateGlbStaff("B.B", "BBB");
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var locationA = data.Whs1.DefaultLocation;
			var locationB = whs2.DefaultLocation;
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA);
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, locationA, "PLT-1", today.AddDays(20), today.AddDays(-20), "PA1", "PA2", "PA3", "ENTRY");
			inv2.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 15m, "A", whs2.PK, "B", InventoryHoldCodes.Codes.Damaged);
			transferLine2.WE_F3_NKPackType = Constants.PkgUnit.Carton;
			transferLine2.WE_TransactionQuantity = 15m;
			transferLine2.WE_TransferFromPalletId = "PLT-1";
			transferLine2.WE_BondedEntryKey = "ENTRY";
			transferLine2.WE_PartAttrib1 = "PA1";
			transferLine2.WE_PartAttrib2 = "PA2";
			transferLine2.WE_PartAttrib3 = "PA3";
			transferLine2.WE_ExpiryDate = today.AddDays(20);
			transferLine2.WE_PackingDate = today.AddDays(-20);
			transferLine2.GS_NKPickedBy = staff1.GS_Code;
			transferLine2.WE_GS_NKPutawayBy = staff2.GS_Code;
			AssertEquals("Precondition - no related transfers should exist.", 0, transfer.ChildTransfers.Count());

			transferLine1.FinaliseDocketLine();
			var matchingTransfers = transfer.ChildTransfers;
			AssertEquals("Transfer Line should be Finalised.", true, transferLine1.IsFinalised);
			AssertEquals("Finalizing Inter Whs Transfer Line should create matching Transfer.", 1, matchingTransfers.Count());
			AssertEquals("Matching Transfer should not be finalised.", false, matchingTransfers.ElementAt(0).IsFinalised);
			AssertEquals("Matching Transfer should have only 1 line.", 1, matchingTransfers.ElementAt(0).Lines.Count);
			var matchingTransferLine1 = (WhsTransferLine)matchingTransfers.ElementAt(0).Lines.Single(l => l.WE_TransactionQuantity == 10m);
			AssertInterWhsTransferLinesAreMatching(transferLine1, matchingTransferLine1);

			transferLine2.FinaliseDocketLine();
			AssertEquals("Transfer Line should be Finalised.", true, transferLine2.IsFinalised);
			AssertEquals("Finalizing Inter Whs Transfer Line should update existing not finalized related transfer.", 1, matchingTransfers.Count());
			AssertEquals("Matching Transfer should not be finalised.", false, matchingTransfers.ElementAt(0).IsFinalised);
			AssertEquals("Matching Transfer should have 2 lines.", 2, matchingTransfers.ElementAt(0).Lines.Count);
			var matchingTransferLine2 = (WhsTransferLine)matchingTransfers.ElementAt(0).Lines.Single(l => l.WE_TransactionQuantity == 15m);
			AssertInterWhsTransferLinesAreMatching(transferLine2, matchingTransferLine2);
		}

		[TestDate(2012, 3, 13, 5, 5, 5)]
		public void TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var locationA = data.Whs1.DefaultLocation;
			var locationB = whs2.DefaultLocation;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true, false);

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA);
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, locationA, "PLT-1");
			inv2.WI_SerialNumber = "SER";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 1m, "A", whs2.PK, "B");
			transferLine2.WE_TransactionQuantity = 1m;
			transferLine2.WE_TransferFromPalletId = "PLT-1";
			transferLine2.WE_SerialNumber = "SER";
			AssertEquals("Precondition - no related transfers should exist.", 0, transfer.ChildTransfers.Count());

			transferLine1.FinaliseDocketLine();
			var matchingTransfers = transfer.ChildTransfers;
			AssertEquals("Transfer Line should be Finalised.", true, transferLine1.IsFinalised);
			AssertEquals("Finalizing Inter Whs Transfer Line should create matching Transfer.", 1, matchingTransfers.Count());
			AssertEquals("Matching Transfer should not be finalised.", false, matchingTransfers.ElementAt(0).IsFinalised);
			AssertEquals("Matching Transfer should have only 1 line.", 1, matchingTransfers.ElementAt(0).Lines.Count);
			var matchingTransferLine1 = (WhsTransferLine)matchingTransfers.ElementAt(0).Lines.Single(l => l.WE_TransactionQuantity == 10m);
			AssertInterWhsTransferLinesAreMatching(transferLine1, matchingTransferLine1);

			transferLine2.FinaliseDocketLine();
			AssertEquals("Transfer Line should be Finalised.", true, transferLine2.IsFinalised);
			AssertEquals("Finalizing Inter Whs Transfer Line should update existing not finalized related transfer.", 1, matchingTransfers.Count());
			AssertEquals("Matching Transfer should not be finalised.", false, matchingTransfers.ElementAt(0).IsFinalised);
			AssertEquals("Matching Transfer should have 2 lines.", 2, matchingTransfers.ElementAt(0).Lines.Count);
			var matchingTransferLine2 = (WhsTransferLine)matchingTransfers.ElementAt(0).Lines.Single(l => l.WE_TransactionQuantity == 1m);
			AssertInterWhsTransferLinesAreMatching(transferLine2, matchingTransferLine2);
		}

		[TestDate(2021, 8, 20)]
		public void TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var staff1 = Helper.CreateGlbStaff("A.A", "AAA");
			var staff2 = Helper.CreateGlbStaff("B.B", "BBB");
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var locationA = data.Whs1.DefaultLocation;
			var locationB = whs2.DefaultLocation;
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 1m, locationA.PK, "PLT-1", today.AddDays(20), today.AddDays(-20), "PA1", "PA2", "PA3", "SN1", "ENTRY").OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part2, 1m, "A", whs2.PK, "B", InventoryHoldCodes.Codes.Damaged);
			transferLine.WE_F3_NKPackType = Constants.PkgUnit.Carton;
			transferLine.WE_TransactionQuantity = 1m;
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.WE_BondedEntryKey = "ENTRY";
			transferLine.WE_PartAttrib1 = "PA1";
			transferLine.WE_PartAttrib2 = "PA2";
			transferLine.WE_PartAttrib3 = "PA3";
			transferLine.WE_SerialNumber = "SN1";
			transferLine.WE_ExpiryDate = today.AddDays(20);
			transferLine.WE_PackingDate = today.AddDays(-20);
			transferLine.GS_NKPickedBy = staff1.GS_Code;
			transferLine.WE_GS_NKPutawayBy = staff2.GS_Code;
			AssertEquals("Precondition - no related transfers should exist.", 0, transfer.ChildTransfers.Count());

			transferLine.FinaliseDocketLine();
			var matchingTransfer = transfer.ChildTransfers.Single();
			AssertEquals("Transfer Line should be Finalised.", true, transferLine.IsFinalised);
			AssertNotNull("Matching transfer created.", matchingTransfer);
			AssertEquals("Matching Transfer should not be finalised.", false, matchingTransfer.IsFinalised);
			AssertEquals("Matching Transfer should have 1 line.", 1, matchingTransfer.Lines.Count);
			var matchingTransferLine = (WhsTransferLine)matchingTransfer.Lines.Single();
			AssertInterWhsTransferLinesAreMatching(transferLine, matchingTransferLine);
		}

		#endregion

		#region TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_MultipleWarehouses

		public void TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_MultipleWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var whs3 = Helper.CreateWarehouse("WH3", "C");
			Factory.Save();

			var locationA = data.Whs1.DefaultLocation;
			var locationB = whs2.DefaultLocation;
			var locationC = whs3.DefaultLocation;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A", whs3.PK, "C");
			AssertEquals("Precondition - no related Jobs should exist.", 0, transfer.RelatedJobs.Count);

			transferLine1.FinaliseDocketLine();
			var matchingTransfers1 = transfer.ChildTransfers;
			AssertEquals("Transfer Line should be Finalised.", true, transferLine1.IsFinalised);
			AssertEquals("Finalizing Inter Whs Transfer Line should create matching Transfer.", 1, matchingTransfers1.Count());
			AssertEquals("Matching Transfer should not be finalised.", false, matchingTransfers1.ElementAt(0).IsFinalised);
			AssertEquals("Matching Transfer should have only 1 line.", 1, matchingTransfers1.ElementAt(0).Lines.Count);
			var matchingTransferLine1 = (WhsTransferLine)matchingTransfers1.ElementAt(0).Lines.Single(l => l.WE_TransactionQuantity == 10m);
			AssertInterWhsTransferLinesAreMatching(transferLine1, matchingTransferLine1);

			transferLine2.FinaliseDocketLine();
			var matchingTransfers2 = transfer.ChildTransfers;
			AssertEquals("Transfer Line should be Finalised.", true, transferLine2.IsFinalised);
			AssertEquals("Finalizing Inter Whs Transfer Line should create another matching.", 2, matchingTransfers2.Count());
			var matchingTransfer2 = matchingTransfers2.Single(t => t.WD_WW_Whs == whs3.PK);
			AssertEquals("Matching Transfer should not be finalised.", false, matchingTransfer2.IsFinalised);
			AssertEquals("Matching Transfer should have 1 line.", 1, matchingTransfer2.Lines.Count);
			var matchingTransferLine2 = (WhsTransferLine)matchingTransfer2.Lines.Single(l => l.WE_TransactionQuantity == 15m);
			AssertInterWhsTransferLinesAreMatching(transferLine2, matchingTransferLine2);
		}

		#endregion

		#region TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_ErrorsPassedToParent

		public void TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_ErrorsPassedToParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WH2", "B");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", whs2.PK, "B");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", whs2.PK, "B");
			transferLine1.FinaliseDocketLine();
			transferLine2.RunPreSaveValidation(); // to commit inventory
			AssertIsFinalisedPrecondition(transferLine1);

			Factory.Save();

			// hack to add error to a child transfer line during finalise.
			EventHandler action = null;
			action = (collection, e) =>
			{
				var line = ((WhsTransferLineCollection)collection).Single(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Entered);
				line.AddRowError("TEST ERROR");
				transfer.ChildTransfers.ElementAt(0).Lines.CountChanged -= action;
			};
			transfer.ChildTransfers.ElementAt(0).Lines.CountChanged += action;
			var poke = transfer.ChildTransfers.ElementAt(0).Lines.Count; // need this to make count changed fire

			transferLine2.FinaliseDocketLine();
			AssertEquals("Transfer line should not be finalised when its matching transfer line have error during finalise.", false, transferLine2.IsFinalised);
			AssertEquals("Original transfer line should have an error passed from its child line.", true, transferLine2.GetErrors().Any());
			AssertHasRowError("Should have added a RowError from the child line.", transferLine2, "Transfer line does not match between warehouses and cannot be finalized.\r\nError - Docket Line: TEST ERROR");
			AssertHasRowError("Should have added a RowError.", transferLine2, "Error occurred during finalization. Close the form without saving and try again.");
		}

		#endregion

		#region TestFinaliseDocketLine_WithMatchingLines

		public void TestFinaliseDocketLine_WithMatchingLines()
		{
			TestFinaliseDocketLine_WithMatchingLines(wasAlreadyPicked: false);
		}

		public void TestFinaliseDocketLine_WithMatchingLines_InTransit()
		{
			TestFinaliseDocketLine_WithMatchingLines(wasAlreadyPicked: true);
		}

		void TestFinaliseDocketLine_WithMatchingLines(bool wasAlreadyPicked)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 8m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 4m, locationA1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 14m, "A-1", "A-2");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 14m, transferLine.QtyCommittedIncludingMatchingLines);

			var pickedTime = wasAlreadyPicked ? ZDateTimeOffset.Now : ZDateTimeOffset.Empty;
			if (wasAlreadyPicked)
			{
				transferLine.PickedTime = pickedTime;
				Factory.Save();
			}

			var matchingLine1 = transferLine.MatchingLines[0];
			var matchingLine2 = transferLine.MatchingLines[1];

			if (!wasAlreadyPicked)
			{
				matchingLine2.Inventory.CountChanged += delegate
				{
					matchingLine2.AddRowError("Test");
				};
			}
			else
			{
				matchingLine2.WE_CurrentInventoryStatusInfo.ValueChanged += delegate
				{
					matchingLine2.AddRowError("Test");
				};
			}

			transferLine.FinaliseDocketLine();
			CombineAssertions(delegate
			{
				AssertEquals("Transfer Line should not be finalised.", false, transferLine.IsFinalised);
				AssertHasRowError("Should have added a RowError.", transferLine, "Error occurred during finalization. Close the form without saving and try again.");
			});
		}

		#endregion

		#region TestFinaliseDocketLine_WithNegativeCommittedStock

		public void TestFinaliseDocketLine_WithNegativeCommittedStock()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 2m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 26m, data.Whs1.FindLocation("A-1"), "");
			var inventory1 = receive1.Inventory[0];
			var inventory2 = receive2.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 8m, locations[0].PK, locations[1].PK);
			AssertFinaliseFailureForNegativeCommittedStock(transferLine, inventory1, inventory2);
		}

		#endregion

		#region TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_WithNegativeCommittedStock

		public void TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_WithNegativeCommittedStock()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 2m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 26m, data.Whs1.FindLocation("A-1"), "");
			var inventory1 = receive1.Inventory[0];
			var inventory2 = receive2.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 8m, "A-1", whs2.PK, "B");
			AssertFinaliseFailureForNegativeCommittedStock(transferLine, inventory1, inventory2);
		}

		#endregion

		#region TestFinaliseDocketLine_SplitIntoMultipleTransferLines

		public void TestFinaliseDocketLine_SplitIntoMultipleTransferLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A-1"));
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3m, data.Whs1.FindLocation("A-1"));
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-1"));
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 5m, "A-1", "A-2");
			transfer.FinaliseDocket();
			AssertEquals("Transfer should be finalised", true, transfer.IsFinalised);
			AssertEquals("Main transfer line should be finalised.", true, transferLine1.IsFinalised);
			AssertEquals("Main transfer line should be finalised.", true, transferLine2.IsFinalised);
			AssertEquals("Matching transfer line should be finalised.", true, transferLine2.MatchingLines[0].IsFinalised);
			AssertEquals("Matching transfer line should be finalised.", true, transferLine2.MatchingLines[1].IsFinalised);

			var allLines = new List<WhsTransferLine>();
			allLines.Add(transferLine2);
			allLines.AddRange(transferLine2.MatchingLines.Cast<WhsTransferLine>());
			var line1 = allLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			var line2 = allLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory3.WI_WE_InDocketLine);
			var line3 = allLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory4.WI_WE_InDocketLine);

			// 1 inventory record should be created for each transfer line with qty matching picked qty.
			AssertEquals("New inventory should be linked to main transfer line.", 1, transferLine1.Inventory.Count);
			AssertEquals("New inventory should be linked to the transfer line.", 1, line1.Inventory.Count);
			AssertEquals("New inventory should be linked to the transfer line.", 1, line2.Inventory.Count);
			AssertEquals("New inventory should be linked to the transfer line.", 1, line3.Inventory.Count);
			transferLine1.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 2m);
			line1.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 3m);
			line2.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 1m);
			line3.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 1m);
			AssertEquals("Inventory should be picked from original inventory.", 0m, inventory1.WI_TotalUnits);
			AssertEquals("Inventory should be picked from original inventory.", 0m, inventory2.WI_TotalUnits);
			AssertEquals("Inventory should be picked from original inventory.", 0m, inventory3.WI_TotalUnits);
			AssertEquals("Inventory should be picked from original inventory.", 0m, inventory4.WI_TotalUnits);
		}

		#endregion

		#region TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLines

		public void TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLines_Source()
		{
			TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLinesCore(TransferType.Codes.InterWhsSource);
		}

		public void TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLines_Dest()
		{
			TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLinesCore(TransferType.Codes.InterWhsDest);
		}

		void TestFinaliseDocketLine_CreateMatchingInterWarehouseRecord_SplitIntoMultipleTransferLinesCore(ZString transferSubType)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			Factory.Save();

			var jobWhs = (transferSubType == TransferType.Codes.InterWhsSource) ? data.Whs1 : whs2;
			var lineWhs = (transferSubType != TransferType.Codes.InterWhsSource) ? data.Whs1 : whs2;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A"));
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3m, data.Whs1.FindLocation("A"));
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A"));
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A"));
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, jobWhs, "TR1", Notify, transferSubType);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, "A", lineWhs.PK, "B");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 5m, "A", lineWhs.PK, "B");
			transfer.FinaliseDocket();
			AssertEquals("Transfer should be finalised", true, transfer.IsFinalised);
			AssertEquals("Main transfer line should be finalised.", true, transferLine1.IsFinalised);
			AssertEquals("Main transfer line should be finalised.", true, transferLine2.IsFinalised);
			AssertEquals("Matching transfer line should be finalised.", true, transferLine2.MatchingLines[0].IsFinalised);
			AssertEquals("Matching transfer line should be finalised.", true, transferLine2.MatchingLines[1].IsFinalised);

			var allTransferLines = new List<WhsTransferLine>();
			allTransferLines.Add(transferLine2);
			allTransferLines.AddRange(transferLine2.MatchingLines.Cast<WhsTransferLine>());
			var line1 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			var line2 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory3.WI_WE_InDocketLine);
			var line3 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory4.WI_WE_InDocketLine);
			if (transferSubType == TransferType.Codes.InterWhsSource)
			{
				AssertEquals("No new inventory should be linked to source main tranfser line.", 0, transferLine1.Inventory.Count);
				AssertEquals("No new inventory should be linked to source main tranfser line.", 0, transferLine2.Inventory.Count);
				AssertEquals("No new inventory should be linked to source matching tranfser line.", 0, transferLine2.MatchingLines[0].Inventory.Count);
				AssertEquals("No new inventory should be linked to source matching tranfser line.", 0, transferLine2.MatchingLines[1].Inventory.Count);
			}
			else
			{
				// 1 inventory record should be created for each transfer line with qty matching picked qty.
				AssertEquals("New inventory should be linked to dest main tranfser line.", 1, transferLine1.Inventory.Count);
				AssertEquals("New inventory should be linked to dest main tranfser line.", 1, line1.Inventory.Count);
				AssertEquals("New inventory should be linked to dest main tranfser line.", 1, line2.Inventory.Count);
				AssertEquals("New inventory should be linked to dest main tranfser line.", 1, line3.Inventory.Count);
				transferLine1.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 2m);
				line1.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 3m);
				line2.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 1m);
				line3.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 1m);
			}

			var childTransfer = transfer.ChildTransfers.Single();
			AssertEquals("Child transfer should have same amount of lines as main transfer.", 2, childTransfer.Lines.Count);
			var childTransferLine1 = transferLine1.ChildTransferLine;
			var childTransferLine2 = transferLine2.ChildTransferLine;
			var childMatchingTransferLine1 = transferLine2.MatchingLines[0].ChildTransferLine;
			var childMatchingTransferLine2 = transferLine2.MatchingLines[1].ChildTransferLine;
			// check everything was finalised
			AssertEquals("Child Transfer should be finalised.", true, childTransfer.IsFinalised);
			AssertEquals("Child Main transfer line should be finalised.", true, childTransferLine1.IsFinalised);
			AssertEquals("Child Main transfer line should be finalised.", true, childTransferLine2.IsFinalised);
			AssertEquals("Child Matching transfer line should be finalised.", true, childMatchingTransferLine1.IsFinalised);
			AssertEquals("Child Matching transfer line should be finalised.", true, childMatchingTransferLine2.IsFinalised);
			// check pick lines and inventories are linked propertly
			AssertEquals("No matching lines should be linked to the child transfer line.", 0, childTransferLine1.MatchingLines.Count);
			AssertEquals("2 matching line should be linked to the child transfer line.", 2, childTransferLine2.MatchingLines.Count);
			AssertEquals("No matching lines should be linked to the child transfer line.", 0, childMatchingTransferLine1.MatchingLines.Count);
			AssertEquals("No matching lines should be linked to the child transfer line.", 0, childMatchingTransferLine2.MatchingLines.Count);
			AssertEquals("No pick lines should be linked to child transfer lines.", 0, childTransferLine1.PickLines.Count);
			AssertEquals("No pick lines should be linked to child transfer lines.", 0, childTransferLine2.PickLines.Count);
			AssertEquals("No pick lines should be linked to child transfer lines.", 0, childMatchingTransferLine1.PickLines.Count);
			AssertEquals("No pick lines should be linked to child transfer lines.", 0, childMatchingTransferLine2.PickLines.Count);
			if (transferSubType == TransferType.Codes.InterWhsSource)
			{
				// 1 inventory record should be created for each transfer line with qty matching picked qty.
				AssertEquals("New inventory should be linked to dest main tranfser line.", 1, childTransferLine1.Inventory.Count);
				var childLine1 = line1.ChildTransferLine;
				var childLine2 = line2.ChildTransferLine;
				var childLine3 = line3.ChildTransferLine;
				AssertEquals("New inventory should be linked to the tranfser line.", 1, childTransferLine1.Inventory.Count);
				AssertEquals("New inventory should be linked to the tranfser line.", 1, childLine1.Inventory.Count);
				AssertEquals("New inventory should be linked to the tranfser line.", 1, childLine2.Inventory.Count);
				AssertEquals("New inventory should be linked to the tranfser line.", 1, childLine3.Inventory.Count);
				childTransferLine1.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 2m);
				childLine1.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 3m);
				childLine2.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 1m);
				childLine3.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_TotalUnits == 1m);
			}
			else
			{
				AssertEquals("No new inventory should be linked to source main tranfser line.", 0, childTransferLine1.Inventory.Count);
				AssertEquals("No new inventory should be linked to source main tranfser line.", 0, childTransferLine2.Inventory.Count);
				AssertEquals("No new inventory should be linked to source matching tranfser line.", 0, childTransferLine2.MatchingLines[0].Inventory.Count);
				AssertEquals("No new inventory should be linked to source matching tranfser line.", 0, childTransferLine2.MatchingLines[1].Inventory.Count);
			}
		}

		#endregion

		#region TestFinaliseDocketLine_WhenAttachedToVASOrder

		public void TestFinaliseDocketLine_WhenAttachedToVASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull(intoServiceAreaTransfer);

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);

			var topLevelLine = (WhsTransferLine)intoServiceAreaTransfer.Lines.Single();
			AssertEquals("Transfer lines for VAS Orders should *not* have their Original Status changed.", InventoryStatus.Codes.Available, topLevelLine.WE_OriginalInventoryStatus);
			AssertEquals("Transfer lines for VAS Orders should have their Current Status changed to Staged.", InventoryStatus.Codes.Staged, topLevelLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory on Transfers for VAS Orders should have their Current Status changed to Staged.", InventoryStatus.Codes.Staged, topLevelLine.Inventory[0].WI_InventoryStatus);

			var matchingLine = (WhsTransferLine)topLevelLine.MatchingLines.Single();
			AssertEquals("Transfer lines for VAS Orders should *not* have their Original Status changed.", InventoryStatus.Codes.Available, matchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Transfer lines for VAS Orders should have their Current Status changed to Staged.", InventoryStatus.Codes.Staged, matchingLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory on Transfers for VAS Orders should have their Current Status changed to Staged.", InventoryStatus.Codes.Staged, matchingLine.Inventory[0].WI_InventoryStatus);
			AssertNoExceptionThrown(() => Factory.Save()); // ensure data is correct

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}
			AssertNotNull("Precondition: Return transfer successfully created.", returnTransfer);

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(returnTransfer);

			var topLevelLineOnReturnTransfer = (WhsTransferLine)returnTransfer.Lines.Single();
			AssertEquals("The Original Status for Transfer lines on Return VAS Order Transfers should be Available.", InventoryStatus.Codes.Available, topLevelLineOnReturnTransfer.WE_OriginalInventoryStatus);
			AssertEquals("Return Transfer Lines should have their Current Status changed to Available.", InventoryStatus.Codes.Available, topLevelLineOnReturnTransfer.WE_CurrentInventoryStatus);
			AssertEquals("Inventory on Return Transfers for VAS Orders should have their Current Status changed to Available.",
				InventoryStatus.Codes.Available, topLevelLineOnReturnTransfer.Inventory[0].WI_InventoryStatus);

			var matchingLineOnReturnTransfer = (WhsTransferLine)topLevelLineOnReturnTransfer.MatchingLines.Single();
			AssertEquals("The Original Status for Transfer lines on Return VAS Order Transfers should be Available.", InventoryStatus.Codes.Available, matchingLineOnReturnTransfer.WE_OriginalInventoryStatus);
			AssertEquals("Return Transfer Lines should have their Current Status changed to Available.", InventoryStatus.Codes.Available, matchingLineOnReturnTransfer.WE_CurrentInventoryStatus);
			AssertEquals("Inventory on Return Transfers for VAS Orders should have their Current Status changed to Available.",
				InventoryStatus.Codes.Available, matchingLineOnReturnTransfer.Inventory[0].WI_InventoryStatus);
			AssertEquals("", matchingLineOnReturnTransfer.WE_WHC_NKCurrentInventoryHeldCode);
			AssertNoExceptionThrown(() => Factory.Save()); // ensure data is correct
		}

		#endregion

		#region TestFinaliseDocketLine_RollBack_InTransit

		public void TestFinaliseDocketLine_RollBack_InTransit()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m, locations[0], "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locations[0].PK, locations[1].PK);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: committed stock.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			// Hack validation error during finalisation
			transferLine.WE_CurrentInventoryStatusInfo.ValueChanged += (b, args) =>
			{
				transferLine.AddRowError("TEST");
			};

			transferLine.FinaliseDocketLine();
			AssertEquals(false, transferLine.IsFinalised);
			AssertHasRowError("Should have added a RowError.", transferLine, "Error occurred during finalization. Close the form without saving and try again.");
			AssertNoExceptionThrown(() => Factory.Load<WhsInventoryView>(new ZDBOnlyQuery(typeof(WhsInventoryView))));
		}

		#endregion

		#region TestFinaliseDocketLine_Rollback_InterWhs

		public void TestFinaliseDocketLine_Rollback_InterWhs_Dest()
		{
			TestFinaliseDocketLine_Rollback_InterWhs(isSource: false);
		}

		public void TestFinaliseDocketLine_Rollback_InterWhs_Source()
		{
			TestFinaliseDocketLine_Rollback_InterWhs(isSource: true);
		}

		void TestFinaliseDocketLine_Rollback_InterWhs(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var transferWhs = isSource ? data.Whs1 : whs2;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferWhs, "TR1");
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, "A", isSource ? whs2.PK : data.Whs1.PK, "A");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = ZDateTimeOffset.Now;
			var childTransfer = transfer.ChildTransfers.First();
			var childLine = transferLine.ChildTransferLine;
			AssertNotNull("Precondition: Created child line.", childLine);
			Factory.Save();

			// Hack validation error during finalisation (Changing In-Transit status to Available)
			childLine.WE_CurrentInventoryStatusInfo.ValueChanged += (b, args) =>
			{
				childLine.AddRowError("TEST");
			};

			transferLine.FinaliseDocketLine();
			AssertEquals("Transfer line should not be finalised.", false, transferLine.IsFinalised);
			AssertEquals("Child Transfer line should not be deleted.", false, childLine.IsDeleted);
			AssertEquals("Child Transfer line should not be finalised.", false, childLine.IsFinalised);
			AssertHasRowError("Should have added a RowError.", childLine, "Error occurred during finalization. Close the form without saving and try again.");
		}

		#endregion

		#region TestFinaliseDocketLine_Rollback_InterWhs_WithMatchingLines

		public void TestFinaliseDocketLine_Rollback_InterWhs_WithMatchingLines_Dest()
		{
			TestFinaliseDocketLine_Rollback_InterWhs_WithMatchingLines(isSource: false);
		}

		public void TestFinaliseDocketLine_Rollback_InterWhs_WithMatchingLines_Source()
		{
			TestFinaliseDocketLine_Rollback_InterWhs_WithMatchingLines(isSource: true);
		}

		void TestFinaliseDocketLine_Rollback_InterWhs_WithMatchingLines(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");
			var transferWhs = isSource ? data.Whs1 : whs2;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, data.Whs1.FindLocation("A-1"), "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, transferWhs, "TR1");
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A", isSource ? whs2.PK : data.Whs1.PK, "A");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = ZDateTimeOffset.Now;
			var matchingLine = transferLine.MatchingLines[0];
			var childTransfer = transfer.ChildTransfers.First();
			var childLine = transferLine.ChildTransferLine;
			var childMatchingLine = childLine.MatchingLines[0];
			AssertNotNull("Precondition: Created child line.", childLine);
			Factory.Save();

			// Hack validation error during finalisation (Changing In-Transit status to Available)
			childMatchingLine.WE_CurrentInventoryStatusInfo.ValueChanged += (b, args) =>
			{
				childMatchingLine.AddRowError("TEST");
			};

			transferLine.FinaliseDocketLine();
			AssertEquals("Transfer line should not be finalised.", false, transferLine.IsFinalised);
			AssertEquals("Matching line should not be finalised.", false, matchingLine.IsFinalised);
			AssertEquals("Child Transfer line should not be deleted.", false, childLine.IsDeleted);
			AssertEquals("Child Transfer line should not be finalised.", false, childLine.IsFinalised);
			AssertEquals("Matching Child Transfer line should not be deleted.", false, childMatchingLine.IsDeleted);
			AssertEquals("Matching Child Transfer line should not be finalised.", false, childMatchingLine.IsFinalised);
			AssertHasRowError("Should have added a RowError.", transferLine, "Error occurred during finalization. Close the form without saving and try again.");
		}

		#endregion

		#region TestFinaliseTransferWithSpecifiedReceivedPackType

		public void TestFinaliseTransferWithSpecifiedReceivedPackType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, "PLT", 100);

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_F3_NKReceivedPackType = "PLT"; // this is what causing a problem

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1")).InDocketLine;
			receiveLine.WE_F3_NKPackType = "UNT";
			receiveLine.WE_TransactionQuantity = 100m;
			AssertEquals("UNT", receiveLine.WE_F3_NKPackType);
			AssertEquals(100m, receiveLine.WE_TransactionQuantity);
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 51, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine.WE_F3_NKPackType = "UNT";
			transferLine.WE_TransactionQuantity = 51;
			transferLine.RunPreSaveValidation(); // to commit inventory
			AssertEquals("UNT", transferLine.WE_F3_NKPackType);
			AssertEquals(51m, transferLine.WE_TransactionQuantity);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var transferInAnotherFactory = anotherFactory.Load<WhsTransfer>(transfer.PK);

			transferInAnotherFactory.FinaliseDocketWithoutUserConfirmation();
			Assert(transferInAnotherFactory.Lines[0].IsFinalised);
			AssertEquals("WE_TransactionQuantity must not be changed when stock gets created.", 51m, transferInAnotherFactory.Lines[0].WE_TransactionQuantity);
		}

		#endregion

		#region TestFinaliseTransferWithSpecifiedReleasedPackType

		public void TestFinaliseTransferWithSpecifiedReleasedPackType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, "PLT", 120);

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_F3_NKReceivedPackType = "";
			productParams.W3_F3_NKReleasedPackType = "PLT"; // this is what causing a problem

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 120m, data.Whs1.FindLocation("A-1")).InDocketLine;
			AssertEquals(120m, receiveLine.WE_TransactionQuantity);
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 120m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine.RunPreSaveValidation(); // to commit inventory
			AssertEquals("PLT", transferLine.WE_F3_NKPackType);
			AssertEquals(1m, transferLine.PackQtyIncludingMatchingLines);
			AssertEquals(120m, transferLine.WE_TransactionQuantity);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var transferInAnotherFactory = anotherFactory.Load<WhsTransfer>(transfer.PK);
			transferInAnotherFactory.FinaliseDocketWithoutUserConfirmation();
			Assert(transferInAnotherFactory.Lines[0].IsFinalised);
			AssertEquals("WE_TransactionQuantity must not be changed when stock gets created.", 120m, transferInAnotherFactory.Lines[0].WE_TransactionQuantity);
		}

		#endregion

		#region AssertFinaliseFailureForNegativeCommittedStock

		void AssertFinaliseFailureForNegativeCommittedStock(WhsTransferLine transferLine, WhsInventoryView inventory1, WhsInventoryView inventory2)
		{
			AssertEquals("Precondition - ensure no stock is committed.", 0m, inventory1.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition - ensure no stock is committed.", 0m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.RunPreSaveValidation();
			AssertEquals("CommitInventory should commit stock.", 2m, inventory1.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 6m, inventory2.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("CommitInventory should commit stock.", 8m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.QtyToMoveIncludingMatchingLines = 4m;
			transferLine.RunPreSaveValidation();
			AssertEquals(4m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			// Hack Negative Units
			CollectionCountChangedEventHandler countChangedEvent = (b, args) =>
			{
				var newInv = (WhsInventoryView)args.BizObject;
				EventHandler setNegativeUnits = null;
				setNegativeUnits = (i, ar) =>
				{
					var inventoryView = Factory.Load<WhsInventoryView>(((WhsInventoryView)i).PK);
					newInv.InventoryCreatedFromTransferLine_ForTest -= setNegativeUnits;
					inventoryView.WI_TotalUnits = -2;
				};

				newInv.InventoryCreatedFromTransferLine_ForTest += setNegativeUnits;
			};

			transferLine.Inventory.CountChanged += countChangedEvent;
			transferLine.ChildTransferLineChanged_ForTest += (l, a) =>
			{
				var childLine = (WhsTransferLine)l;
				childLine.Inventory.CountChanged += countChangedEvent;
			};

			transferLine.FinaliseDocketLine();
			AssertEquals(false, transferLine.IsFinalised);
			AssertHasRowError("Should have added a RowError.", transferLine, "Error occurred during finalization. Close the form without saving and try again.");
		}

		#endregion

		#region AssertInterWhsTransferLinesAreMatching

		void AssertInterWhsTransferLinesAreMatching(WhsTransferLine transferLine, WhsTransferLine matchingTransferLine)
		{
			AssertEquals("Client", transferLine.Docket.WD_OH_Client, matchingTransferLine.Docket.WD_OH_Client);
			AssertEquals("WE_OP", transferLine.WE_OP, matchingTransferLine.WE_OP);
			AssertEquals("WE_F3_NKPackType", transferLine.WE_F3_NKPackType, matchingTransferLine.WE_F3_NKPackType);
			AssertEquals("TransferFromWarehousePK", transferLine.TransferFromWarehousePK, matchingTransferLine.TransferFromWarehousePK);
			AssertEquals("WE_WL_TransferFrom", transferLine.WE_WL_TransferFrom, matchingTransferLine.WE_WL_TransferFrom);
			AssertEquals("WE_TransferFromPalletId", transferLine.WE_TransferFromPalletId, matchingTransferLine.WE_TransferFromPalletId);
			AssertEquals("DestinationWarehousePK", transferLine.DestinationWarehousePK, matchingTransferLine.DestinationWarehousePK);
			AssertEquals("WE_WL", transferLine.WE_WL, matchingTransferLine.WE_WL);
			AssertEquals("WE_PalletID", transferLine.WE_PalletID, matchingTransferLine.WE_PalletID);
			AssertEquals("WE_BondedEntryKey", transferLine.WE_BondedEntryKey, matchingTransferLine.WE_BondedEntryKey);
			AssertEquals("WE_ExpiryDate", transferLine.WE_ExpiryDate, matchingTransferLine.WE_ExpiryDate);
			AssertEquals("WE_PackingDate", transferLine.WE_PackingDate, matchingTransferLine.WE_PackingDate);
			AssertEquals("WE_PartAttrib1", transferLine.WE_PartAttrib1, matchingTransferLine.WE_PartAttrib1);
			AssertEquals("WE_PartAttrib2", transferLine.WE_PartAttrib2, matchingTransferLine.WE_PartAttrib2);
			AssertEquals("WE_PartAttrib3", transferLine.WE_PartAttrib3, matchingTransferLine.WE_PartAttrib3);
			AssertEquals("WE_SerialNumber", transferLine.WE_SerialNumber, matchingTransferLine.WE_SerialNumber);
			AssertNotEquals("Source transfer must have ArrivalDateForBinding empty and destination transfer must have a value", transferLine.ArrivalDateForBinding, matchingTransferLine.ArrivalDateForBinding);
			AssertEquals("WE_TransactionQuantity", transferLine.WE_TransactionQuantity, matchingTransferLine.WE_TransactionQuantity);
			AssertEquals("WE_DocketLineStatus", transferLine.WE_DocketLineStatus, matchingTransferLine.WE_DocketLineStatus);
			AssertEquals("PickedBy", transferLine.GS_NKPickedBy, matchingTransferLine.GS_NKPickedBy);
			AssertEquals("PickedTime", transferLine.PickedTime, matchingTransferLine.PickedTime);
			AssertEquals("WE_GS_NKPutawayBy", transferLine.WE_GS_NKPutawayBy, matchingTransferLine.WE_GS_NKPutawayBy);
			AssertEquals("WE_PutawayByTime", transferLine.WE_PutawayTime, matchingTransferLine.WE_PutawayTime);
			AssertEquals("WE_FinalisedDate", transferLine.WE_FinalisedDate, matchingTransferLine.WE_FinalisedDate);
			AssertEquals("WE_OriginalInventoryStatus", transferLine.WE_OriginalInventoryStatus, matchingTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("WE_CurrentInventoryStatus", transferLine.WE_CurrentInventoryStatus, matchingTransferLine.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestClearingPutawayTimeOnFinalisedTransferLine

		public void TestClearingPutawayTimeOnFinalisedTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.FinaliseDocket();

			AssertIsFinalisedPrecondition(transferLine);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = ZDateTimeOffset.Empty;
			}
			Helper.AssertZCannotSaveExceptionThrown("This transfer line cannot be finalized without putaway time because it is Original Inventory.", Factory.Save);
		}

		#endregion

		#region TestSetPutawayTimeOnFinalisedTransferLine_HoldCodeChange

		public void TestSetPutawayTimeOnFinalisedTransferLine_HoldCodeChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.FinaliseDocket();

			AssertIsFinalisedPrecondition(transferLine);

			transferLine.IsInventoryEditForm = true;
			transferLine.HeldCodeChangeQuantity = 5m;
			transferLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			transferLine.ChangeInventoryHeldCode(true);

			var statusChangeLine = Factory.LoadTop1<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, false));
			AssertNotNull("Precondition: HoldCode Changed.", statusChangeLine);
			AssertEquals("Precondition: IsOriginalInventory.", false, statusChangeLine.WE_IsOriginalInventory);

			using (new SemaphoreManager(statusChangeLine.FinaliseDocketLineSemaphore))
			{
				statusChangeLine.WE_PutawayTime = ZDateTimeOffset.Today;
			}
			Helper.AssertZCannotSaveExceptionThrown("This transfer line cannot be finalized with putaway time because it is not Original Inventory.", Factory.Save);
		}

		#endregion

		#region TestFinaliseDocketLine_PutawayLineIsPuttingAwayRemoved

		public void TestFinaliseDocketLine_PutawayLineIsPuttingAwayRemoved()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");

			var dockdoor = data.Whs1.DefaultInboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, dockdoor, "PLT-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoor, "PLT-2");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoor, "PLT3");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine1.WE_GS_NKPutawayBy = staff1.GS_Code;
			transferLine1.RunPreSaveValidation();
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-1"), "PLT-2", 10m);
			transferLine2.WE_GS_NKPutawayBy = staff1.GS_Code;
			transferLine2.RunPreSaveValidation();
			var transferLine3 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-1"), "PLT3", 10m);
			transferLine3.WE_GS_NKPutawayBy = staff2.GS_Code;
			transferLine3.RunPreSaveValidation();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", isPuttingAway: true);
			var putawayLine2 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-2", isPuttingAway: true);

			var putawayJob2 = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			var putawayLine3 = Helper.CreateWhsPutawayLine(putawayJob2, "PLT3", isPuttingAway: true);
			Helper.Factory.Save();

			AssertEquals("Precondition: PutawayLine1 IsPuttingAway == 1", true, putawayLine1.WPL_IsPuttingAway);
			AssertEquals("Precondition: PutawayLine2 IsPuttingAway == 1", true, putawayLine2.WPL_IsPuttingAway);
			AssertEquals("Precondition: PutawayLine3 IsPuttingAway == 1", true, putawayLine3.WPL_IsPuttingAway);

			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			AssertEquals("TransferLine1 WE_WPL_PutawayLine correct", putawayLine1.PK, transferLine1.WE_WPL_PutawayLine);
			AssertEquals("PutawayLine1 IsPuttingAway == 0", false, putawayLine1.WPL_IsPuttingAway);

			transferLine2.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine2);
			AssertEquals("TransferLine2 WE_WPL_PutawayLine correct", putawayLine2.PK, transferLine2.WE_WPL_PutawayLine);
			AssertEquals("PutawayLine2 IsPuttingAway == 0", false, putawayLine2.WPL_IsPuttingAway);

			transferLine3.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine3);
			AssertEquals("TransferLine3 WE_WPL_PutawayLine correct", putawayLine3.PK, transferLine3.WE_WPL_PutawayLine);
			AssertEquals("PutawayLine3 IsPuttingAway == 0", false, putawayLine3.WPL_IsPuttingAway);
		}

		public void TestFinaliseDocketLine_PutawayLineIsPuttingAwayRemoved_ConsolidatedPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var dockdoor = data.Whs1.DefaultInboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, dockdoor, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine1.RunPreSaveValidation();
			transferLine1.WE_GS_NKPutawayBy = staff1.GS_Code;
			transferLine1.WE_PalletID = "PLT-2";
			transferLine1.PickedTime = ZDateTimeOffset.Now;

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", isPuttingAway: true);
			Helper.Factory.Save();

			AssertEquals("Precondition: PutawayLine1 IsPuttingAway == 1", true, putawayLine1.WPL_IsPuttingAway);

			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			AssertEquals("TransferLine1 WE_WPL_PutawayLine correct", putawayLine1.PK, transferLine1.WE_WPL_PutawayLine);
			AssertEquals("PutawayLine1 IsPuttingAway == 0", false, putawayLine1.WPL_IsPuttingAway);
		}

		#endregion

		#endregion

		#region TestAttemptToModifyFinalisedLine

		public void TestAttemptToModifyFinalisedLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			Factory.Save();

			transferLine.WE_TransactionQuantity = 2;
			Helper.AssertZCannotSaveExceptionThrown("Cannot save as a Finalized Transfer Line has been modified.", Factory.Save);
		}

		#endregion

		#region TestAttemptToModifyFinalisedLine_Exemption

		public void TestAttemptToModifyFinalisedLine_Exemption()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, locationA1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -4m, locationA2);
			Helper.CreateWhsPickLine(adjustmentLine, transferLine.Inventory[0], 2m); // hack to ensure correct inventory lines are picked, do not reuse
			Helper.CreateWhsPickLine(adjustmentLine, transferLine.MatchingLines[0].Inventory[0], 2m);
			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);
			AssertEquals("Precondition", 3m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition", 3m, transferLine.MatchingLines[0].WE_StockOnHand);
			AssertNoExceptionThrown("WE_StockOnHand is in exclusion list, so should not trigger exception.", Factory.Save);
		}

		#endregion

		#region TestDelete

		#region TestCanDelete

		public void TestCanDelete()
		{
			var transferLine = Factory.New<WhsTransferLine>();
			AssertEquals(true, transferLine.CanDelete);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			transferLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals(false, transferLine.CanDelete);
		}

		public void TestCanDelete_VASOrderTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull(intoServiceAreaTransfer);
			var line = (WhsTransferLine)intoServiceAreaTransfer.Lines.Single();
			AssertEquals(false, line.CanDelete);
			AssertEquals("You cannot delete VAS Order Transfer Lines.", line.ReasonForNotAbleToDelete);

			line.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(line);
			AssertEquals(false, line.CanDelete);
			AssertEquals("You cannot delete finalized Transfer Lines.", line.ReasonForNotAbleToDelete);

			var outOfServiceAreaTransfer = Factory.New<WhsTransfer>();
			vasOrder.WVO_WD_TransferOutOfServiceArea = outOfServiceAreaTransfer.PK;
			var lineForOutOfServiceAreaTransfer = outOfServiceAreaTransfer.Lines.AddNew();
			AssertEquals(false, lineForOutOfServiceAreaTransfer.CanDelete);
			AssertEquals("You cannot delete VAS Order Transfer Lines.", lineForOutOfServiceAreaTransfer.ReasonForNotAbleToDelete);
		}

		public void TestCanDelete_OutboundDockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
			var inventory = receive.Inventory[0];

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
			AssertEquals(true, transferLine.CanDelete);

			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			AssertEquals(false, transferLine.CanDelete);
			AssertEquals("You cannot delete Outbound Dock Door Transfer Lines.", transferLine.ReasonForNotAbleToDelete);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is Picked.", true, transferLine.IsPicked);
			AssertEquals(false, transferLine.CanDelete);
			AssertEquals("You cannot delete Outbound Dock Door Transfer Lines.", transferLine.ReasonForNotAbleToDelete);
		}

		public void TestCanDelete_PickedOrPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10, inventory.Location, inventory.Location);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			AssertEquals("Committed transfer lines can be deleted.", true, transferLine.CanDelete);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Picked Transer Lines cannot be deleted.", false, transferLine.CanDelete);

			// putaway_time only gets set on finalisation
			transferLine.FinaliseDocketLine();
			AssertEquals("Putaway Transer Lines cannot be deleted.", false, transferLine.CanDelete);
		}

		#endregion

		#region TestReasonForNotAbleToDelete

		public void TestReasonForNotAbleToDelete()
		{
			var transferLine = Factory.New<WhsTransferLine>();
			AssertEquals("", transferLine.ReasonForNotAbleToDelete);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			transferLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals("You cannot delete finalized Transfer Lines.", transferLine.ReasonForNotAbleToDelete);
		}

		public void TestReasonForNotAbleToDelete_PickedOrFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10, inventory.Location, inventory.Location);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			AssertEquals("Committed transfer lines can be deleted.", "", transferLine.ReasonForNotAbleToDelete);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("You cannot delete Transfer Lines that have been partially or fully picked.", transferLine.ReasonForNotAbleToDelete);
			transferLine.PickedTime = ZDateTimeOffset.Empty; // clean up

			transferLine.FinaliseDocketLine();
			AssertEquals("You cannot delete finalized Transfer Lines.", transferLine.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestDelete_MatchingLines

		public void TestDelete_MatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, "A-1", "");
			var matchingLine1 = Helper.CreateMatchingLine(transferLine, 5m);
			var matchingLine2 = Helper.CreateMatchingLine(transferLine, 3m);
			matchingLine2.Delete();
			AssertEquals("When deleting matching transfer line, the main line and other matching lines should not be deleted.", false, transferLine.IsDeleted);
			AssertEquals("When deleting matching transfer line, the main line and other matching lines should not be deleted.", false, matchingLine1.IsDeleted);
			AssertEquals("When deleting matching transfer line, the main line and other matching lines should not be deleted.", true, matchingLine2.IsDeleted);

			transferLine.Delete();
			AssertEquals("When deleting main transfer line, the matching lines should also be deleted.", true, transferLine.IsDeleted);
			AssertEquals("When deleting main transfer line, the matching lines should also be deleted.", true, matchingLine1.IsDeleted);
			AssertEquals("When deleting main transfer line, the matching lines should also be deleted.", true, matchingLine2.IsDeleted);
		}

		#endregion

		#region TestDelete_ChildTransferLine

		public void TestDelete_ChildTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs2);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", data.Whs1.PK, "A");
			AssertEquals("Master Transfer Line should *not* be a child line.", false, transferLine.IsChildTransferLine);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			var childTransferLine = transferLine.ChildTransferLine;
			AssertNotNull("Precondition: Child Transfer created.", childTransferLine);

			transferLine.Delete();
			AssertEquals("Precondition: Master Transfer Line Deleted.", true, transferLine.IsDeleted);
			AssertEquals("When deleting main transfer line, the child line should also be deleted.", true, childTransferLine.IsDeleted);
		}

		#endregion

		#endregion

		#region TestPropertiesModifiedInVASOrderTransferLine

		public void TestPropertiesModifiedInVASOrderTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateArea(data.Whs1, "A2");
			var inv = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			var vasOrderTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			vasOrderTransfer.RunPreSaveValidation(); // to commit inventory
			AssertNotNull(vasOrderTransfer);
			Factory.Save();

			PropertiesModifiedInVASOrderTransferLine((WhsTransferLine)vasOrderTransfer.Lines.Single(), data.Whs1, data.Part2, true);

			vasOrderTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(vasOrderTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var outOfServiceAreaTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T2");
			var outOfServiceAreaTransferLine = Helper.CreateWhsTransferLine(outOfServiceAreaTransfer, data.Part1, data.Whs1.FindLocation("A-1"), "");
			outOfServiceAreaTransferLine.RunPreSaveValidation(); // to commit inventory
			vasOrder.WVO_WD_TransferOutOfServiceArea = outOfServiceAreaTransfer.PK;
			Factory.Save();
			PropertiesModifiedInVASOrderTransferLine((WhsTransferLine)outOfServiceAreaTransfer.Lines.Single(), data.Whs1, data.Part2, false);
		}

		void PropertiesModifiedInVASOrderTransferLine(WhsTransferLine transferLine, WhsWarehouse warehouse, OrgSupplierPart part, bool isIntoVASOrderTransfer)
		{
			var exceptionMessage = "Cannot save as fields are modified in VAS Order Transfer Line.";
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.WE_OP.Name, part.PK);
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.WE_PartAttrib1.Name, (ZString)"P1");
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.WE_PartAttrib2.Name, (ZString)"P2");
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.WE_PartAttrib3.Name, (ZString)"P3");
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.WE_SerialNumber.Name, (ZString)"SN1");
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.WE_PackingDate.Name, ZDateTime.Now.AddDays(-2));
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.WE_ExpiryDate.Name, ZDateTime.Now.AddDays(2));
			if (isIntoVASOrderTransfer)
			{
				AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.WE_WL.Name, warehouse.FindLocation("A-3").PK);
			}
			else
			{
				AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.WE_TransferFromPalletId.Name, (ZString)"PLT1");
				AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.WE_WL_TransferFrom.Name, warehouse.FindLocation("A-3").PK);
			}
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.WE_CurrentInventoryStatus.Name, (ZString)InventoryStatus.Codes.Held);
			AssertChangedProperties(exceptionMessage, transferLine, WhsTransferLine.Schema.QtyToMoveIncludingMatchingLines, (ZDecimal)2);
			AssertChangedProperties(exceptionMessage, transferLine, WhsTransferLine.Schema.ArrivalDateForBinding, ZDateTime.Now.AddDays(-5));
		}

		void AssertChangedProperties(string expectedExceptionMessage, WhsTransferLine transferLine, string column, IZType differentValue)
		{
			var originalValue = transferLine[column];
			transferLine[column] = differentValue;
			Assert(transferLine.HasChanges);
			Helper.AssertZCannotSaveExceptionThrown(expectedExceptionMessage, Factory.Save);
			transferLine[column] = originalValue;
			transferLine.HasChanges = false;
			Assert(!transferLine.HasChanges);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestPropertiesModifiedInOutboundDockDoorTransfer

		public void TestPropertiesModifiedInOutboundDockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
			transferLine.WE_TransferFromPalletId = "PLT1";
			transferLine.RunPreSaveValidation(); // to commit inventory
			AssertEquals("Precondition: Transfer Line is Committed.", 10m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			var exceptionMessage = "Cannot save as an Outbound Dock Door Transfer Line has been modified.";
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_OP, data.Part2.PK);
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_PartAttrib1, (ZString)"P1");
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_PartAttrib2, (ZString)"P2");
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_PartAttrib3, (ZString)"P3");
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_PackingDate, ZDateTime.Now.AddDays(-2));
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_ExpiryDate, ZDateTime.Now.AddDays(-3));
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_BondedEntryKey, (ZString)"BEK");
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode, (ZString)"HEL");
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_TransferFromPalletId, (ZString)"PLT2");
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_WL_TransferFrom, data.Whs1.FindLocation("A-3").PK);
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_AdjustmentArrivalDate, ZDateTime.Now.AddDays(-5));
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_PackageGroupId, (ZString)"ABC");
			AssertChangedProperties(exceptionMessage, transferLine, WhsDocketLineSchema.Constants.WE_PerPackageQty, (ZDecimal)1.1m);
		}

		#endregion

		#region TestReadonlyPropertiesForPutawayTransfers

		public void TestReadonlyPropertiesForPutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, dockDoorLocation, "A", false, false);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, dockDoorLocation, data.Whs1.FindLocation("A-2"), "");

			ReadonlyPropertiesForPutawayTransfers(transferLine);
		}

		static void ReadonlyPropertiesForPutawayTransfers(WhsTransferLine line)
		{
			AssertEquals(true, line.WE_OPInfo.ReadOnly);
			AssertEquals(true, line.WE_PartAttrib1Info.ReadOnly);
			AssertEquals(true, line.WE_PartAttrib2Info.ReadOnly);
			AssertEquals(true, line.WE_PartAttrib3Info.ReadOnly);
			AssertEquals("line.WE_SerialNumberInfo.ReadOnly", true, line.WE_SerialNumberInfo.ReadOnly);
			AssertEquals(true, line.WE_ExpiryDateInfo.ReadOnly);
			AssertEquals(true, line.WE_PackingDateInfo.ReadOnly);
			AssertEquals(true, line.WE_WHC_NKOriginalInventoryHeldCodeInfo.ReadOnly);
			AssertEquals(true, line.WE_WHC_NKCurrentInventoryHeldCodeInfo.ReadOnly);
			AssertEquals(true, line.PackQtyIncludingMatchingLinesInfo.ReadOnly);
			AssertEquals(true, line.QtyToMoveIncludingMatchingLinesInfo.ReadOnly);
			AssertEquals(true, line.WE_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals(true, line.WE_CurrentInventoryStatusInfo.ReadOnly);
			AssertEquals(true, line.WE_TransferFromPalletIdInfo.ReadOnly);
			AssertEquals(true, line.WE_PalletIDInfo.ReadOnly);
			AssertEquals(true, line.WE_WL_TransferFromInfo.ReadOnly);
			AssertEquals(true, line.WE_WPL_PutawayLineInfo.ReadOnly);
			AssertEquals(false, line.WE_WLInfo.ReadOnly);
			AssertEquals(false, line.GS_NKPickedByInfo.ReadOnly);
		}

		#endregion

		#region TestReadOnlyPropertiesForOutboundDockDoorTransfers

		public void TestReadOnlyPropertiesForOutboundDockDoorTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location1, location2);
			ReadOnlyPropertiesForOutboundDockDoorTransfers(transferLine);
		}

		static void ReadOnlyPropertiesForOutboundDockDoorTransfers(WhsTransferLine line)
		{
			var propertiesToExclude = new HashSet<string>(new[]
			{
				WhsDocketLineSchema.Constants.WE_ClientOrderedUnits,
				WhsDocketLineSchema.Constants.WE_DocketLineType,
				WhsDocketLineSchema.Constants.WE_ExtendedLinePrice,
				WhsDocketLineSchema.Constants.WE_FinalisedDate,
				WhsDocketLineSchema.Constants.WE_IsOriginalInventory,
				WhsDocketLineSchema.Constants.WE_PickGroup,
				WhsDocketLineSchema.Constants.WE_ReasonCode,
				WhsDocketLineSchema.Constants.WE_ReceiveCrossDockOrderNo,
				WhsDocketLineSchema.Constants.WE_RecommendedUnitPrice,
				WhsDocketLineSchema.Constants.WE_RequiredByDate,
				WhsDocketLineSchema.Constants.WE_RX_NKUnitPriceCurrency,
				WhsDocketLineSchema.Constants.WE_UnitDiscountAmount,
				WhsDocketLineSchema.Constants.WE_UnitDiscountPercent,
				WhsDocketLineSchema.Constants.WE_UnitPriceAfterDiscount,
				WhsDocketLineSchema.Constants.WE_WD,
				WhsDocketLineSchema.Constants.WE_WE_MatchingLine,
				WhsDocketLineSchema.Constants.WE_WE_OriginalDocketLineForRating,
				WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine,
				WhsDocketLineSchema.Constants.WE_UnloadedTime,
				WhsDocketLineSchema.Constants.WE_GS_NKUnloadedBy,
				WhsDocketLineSchema.Constants.WE_SystemCreateTimeUtc, // System column
				WhsDocketLineSchema.Constants.WE_SystemCreateUser, // System column
				WhsDocketLineSchema.Constants.WE_SystemLastEditTimeUtc, // System column
				WhsDocketLineSchema.Constants.WE_SystemLastEditUser, // System column
				WhsDocketLineSchema.Constants.WE_WB_CustomsData,
				WhsDocketLineSchema.Constants.WE_P9_Task, // Not shown on the UI
				nameof(WhsTransferLine.PickedTime), // Picked Time will be Read Only when set
				nameof(WhsTransferLine.ProductCode), // Temporary Products are not used on TransferLine
			});

			CombineAssertions(() =>
			{
				foreach (ZPropertyInfo info in line.ZPropertyInfoHash)
				{
					if (!propertiesToExclude.Contains(info.Name))
					{
						AssertEquals($"{info.Name} should be Read Only.", true, info.ReadOnly);
					}
				}
			});
		}

		#endregion

		#region TestReadonlyDestinationWarehouseOnInterWhsSourceTransfer

		public void TestReadonlyDestinationWarehouseOnInterWhsSourceTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("Warehouse2", "A", 2, 2);
			Factory.Save();
			var sourceLocation = data.Whs1.FindLocation("A");
			var destinationLocation = whs2.FindLocation("A-2-1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var interWhsSourceTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var interWhsSourceTransferLine = Helper.CreateWhsTransferLine(interWhsSourceTransfer, data.Part1, 10m, sourceLocation, destinationLocation);

			interWhsSourceTransferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Prediction: Destination Warehouse on an InterWhs Source must be read only. ", true, interWhsSourceTransferLine.DestinationWarehousePKInfo.ReadOnly);
		}

		#endregion

		#region ILineAssigner Members

		#region TestILineAssigner_AssignLine

		#region TestILineAssigner_AssignLine_PickOnly

		public void TestILineAssigner_AssignLine_PickOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var user = Helper.CreateGlbStaff("T1", "T1");
			var sourceLocation = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.Option = AssignLineOptions.PickOnly;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation.WLV_LocationString, "A-2");
			transferLine.RunPreSaveValidation(); // Commit inventory
			Factory.Save();

			AssertEquals(ZString.Empty, transferLine.GS_NKPickedBy);
			AssertEquals(ZString.Empty, transferLine.WE_GS_NKPutawayBy);
			Assert(!transfer.HasChanges);

			var lineAssigner = (ILineStaffAssigner)transferLine;
			lineAssigner.AssignLine(user);

			AssertEquals(user.GS_Code, transferLine.GS_NKPickedBy);
			AssertEquals(ZString.Empty, transferLine.WE_GS_NKPutawayBy);
			Assert(transfer.HasChanges);
		}

		#endregion

		#endregion

		#region TestILineAssigner_UnAssignLine

		public void TestILineAssigner_UnAssignLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var user = Helper.CreateGlbStaff("T1", "T1");
			var sourceLocation = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.Option = AssignLineOptions.PickOnly;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation.WLV_LocationString, "A-2");
			transferLine.RunPreSaveValidation(); // Commit inventory
			AssertEquals(ZString.Empty, transferLine.GS_NKPickedBy);

			var lineAssigner = (ILineStaffAssigner)transferLine;
			lineAssigner.AssignLine(user);
			Factory.Save();

			AssertEquals(user.GS_Code, transferLine.GS_NKPickedBy);
			Assert(!transfer.HasChanges);

			lineAssigner.UnAssignLine(user);
			AssertEquals(ZString.Empty, transferLine.GS_NKPickedBy);
			Assert(transfer.HasChanges);
		}

		public void TestILineAssigner_UnAssignLine_NullUser()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var user = Helper.CreateGlbStaff("T1", "T1");
			var sourceLocation = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.Option = AssignLineOptions.PickOnly;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation.WLV_LocationString, "A-2");
			transferLine.RunPreSaveValidation(); // Commit inventory
			AssertEquals(ZString.Empty, transferLine.GS_NKPickedBy);

			var lineAssigner = (ILineStaffAssigner)transferLine;
			lineAssigner.AssignLine(user);
			Factory.Save();

			AssertEquals(user.GS_Code, transferLine.GS_NKPickedBy);
			Assert(!transfer.HasChanges);

			lineAssigner.UnAssignLine(null);
			AssertEquals(ZString.Empty, transferLine.GS_NKPickedBy);
			Assert(transfer.HasChanges);
		}

		public void TestILineAssigner_UnAssignLine_DifferentUser()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var user1 = Helper.CreateGlbStaff("T1", "T1");
			var user2 = Helper.CreateGlbStaff("T2", "T2");
			var sourceLocation = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.Option = AssignLineOptions.PickOnly;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation.WLV_LocationString, "A-2");
			transferLine.RunPreSaveValidation(); // Commit inventory
			AssertEquals(ZString.Empty, transferLine.GS_NKPickedBy);

			var lineAssigner = (ILineStaffAssigner)transferLine;
			lineAssigner.AssignLine(user1);
			Factory.Save();

			AssertEquals(user1.GS_Code, transferLine.GS_NKPickedBy);
			Assert(!transfer.HasChanges);

			lineAssigner.UnAssignLine(user2);
			AssertEquals(user1.GS_Code, transferLine.GS_NKPickedBy);
			Assert(!transfer.HasChanges);
		}

		#endregion

		#region TestILineAssigner_CanAssignLine

		#region TestILineAssigner_CanAssignLine_PickOnly

		public void TestILineAssigner_CanAssignLine_PickOnly()
		{
			var user = Helper.CreateGlbStaff("T1", "T1");
			var transfer = Factory.New<WhsTransfer>();
			transfer.Option = AssignLineOptions.PickOnly;

			var lineWithEmptyPick = transfer.Lines.AddNew();
			var lineWithNonEmptyPick = transfer.Lines.AddNew();

			lineWithNonEmptyPick.PickLines.AddNew().WZ_PickedDateTime = ZDateTimeOffset.Now;

			AssertEquals(true, ((ILineStaffAssigner)lineWithEmptyPick).CanAssignOrUnAssignLine());
			AssertEquals(false, ((ILineStaffAssigner)lineWithNonEmptyPick).CanAssignOrUnAssignLine());
		}

		#endregion

		#region TestILineAssigner_CanAssignLine_FinalisedLines

		public void TestILineAssigner_CanAssignLine_FinalisedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var user = Helper.CreateGlbStaff("A.A", "A.A");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.Option = AssignLineOptions.PickOnly;
			var transferLine_NotFinalised = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine_Finalised = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine_Finalised.FinaliseDocketLine();
			AssertEquals("Precondition - ensure Transfer line is not Finalised.", false, transferLine_NotFinalised.IsFinalised);
			AssertIsFinalisedPrecondition(transferLine_Finalised);

			AssertEquals(true, ((ILineStaffAssigner)transferLine_NotFinalised).CanAssignOrUnAssignLine());
			AssertEquals(false, ((ILineStaffAssigner)transferLine_Finalised).CanAssignOrUnAssignLine());
		}

		#endregion

		#endregion

		#endregion

		#region ICustomsDataParent Members

		public void TestIsOutwardTypeUsed()
		{
			ICustomsDataParent customsDataParent = Factory.New<WhsTransferLine>();
			AssertEquals("customsDataParent.IsOutwardTypeUsed", false, customsDataParent.IsOutwardTypeRequired);
		}

		public void TestIsCustomsDataReadOnly()
		{
			var transferLine = Factory.New<WhsTransferLine>();
			ICustomsDataParent customsDataParent = transferLine;
			AssertEquals("Non Outbound Dock Door Transfers are not Read Only.", false, customsDataParent.IsCustomsDataReadOnly);

			var transfer = Factory.New<WhsTransfer>();
			transferLine.WE_WD = transfer.PK;
			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			AssertEquals("Outbound Dock Door Transfers are Read Only.", true, customsDataParent.IsCustomsDataReadOnly);
		}

		public void TestIsMainCustomsDataPropertiesReadOnly()
		{
			var transferLine = Factory.New<WhsTransferLine>();
			ICustomsDataParent customsDataParent = transferLine;
			AssertEquals("When Transfer Line is not Picked, the Bonded Entry Key etc should be editable.", false, customsDataParent.IsMainCustomsDataPropertiesReadOnly);

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.HeldForTransfer;
			AssertEquals("When Transfer Line is Picked, the Bonded Entry Key etc should not be editable.", true, customsDataParent.IsMainCustomsDataPropertiesReadOnly);
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		public void TestConfigOrg_DisabledForAutoCreatedTransfers_TransferForOrder()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			var client = Helper.CreateClient("C1");

			transfer.WD_OH_Client = client.PK;
			AssertNotNull("Precondition.", ((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrg);

			transfer.WD_WP_ParentPickForTransfer = ZGuid.NewZGuid();
			AssertNull("Should *not* support custom labels for auto created transfers.", ((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrg);
		}

		public void TestConfigOrg_DisabledForAutoCreatedTransfers_PutawayTransfer()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			var client = Helper.CreateClient("C1");

			transfer.WD_OH_Client = client.PK;
			AssertNotNull("Precondition.", ((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrg);

			transfer.WD_IsPutawayTransfer = true;
			AssertNull("Should *not* support custom labels for auto created transfers.", ((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrg);
		}

		public void TestConfigOrg_DisabledForAutoCreatedTransfers_VASOrder()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			var vasOrder = Factory.New<WhsVASOrder>();
			var client = Helper.CreateClient("C1");

			transfer.WD_OH_Client = client.PK;
			AssertNotNull("Precondition.", ((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrg);

			vasOrder.WVO_WD_TransferIntoServiceArea = transfer.PK;
			AssertNull("Should *not* support custom labels for auto created transfers.", ((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrg);

			vasOrder.WVO_WD_TransferIntoServiceArea = ZGuid.Empty;
			AssertNotNull("Precondition.", ((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrg);

			vasOrder.WVO_WD_TransferOutOfServiceArea = transfer.PK;
			AssertNull("Should *not* support custom labels for auto created transfers.", ((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrg);
		}

		public void TestConfigOrg_DisabledForAutoCreatedTransfers_PickFaceReplenishment()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			var client = Helper.CreateClient("C1");

			transfer.WD_OH_Client = client.PK;
			AssertNotNull("Precondition.", ((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrg);

			transfer.WD_IsPickFaceReplenishment = true;
			AssertNull("Should *not* support custom labels for auto created transfers.", ((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrg);
		}

		public void TestConfigOrgChanged_DisabledForAutoCreatedTransfers_TransferForOrder()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			var configOrgChangedCalled = false;
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");

			transfer.WD_OH_Client = client1.PK;
			transfer.WD_WP_ParentPickForTransfer = ZGuid.NewZGuid();

			((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;
			transfer.WD_OH_Client = client2.PK;
			AssertEquals("Should *not* support custom labels for auto created transfers.", false, configOrgChangedCalled);
		}

		public void TestConfigOrgChanged_DisabledForAutoCreatedTransfers_PutawayTransfer()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			var configOrgChangedCalled = false;
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");

			transfer.WD_OH_Client = client1.PK;
			transfer.WD_IsPutawayTransfer = true;

			((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;
			transfer.WD_OH_Client = client2.PK;
			AssertEquals("Should *not* support custom labels for auto created transfers.", false, configOrgChangedCalled);
		}

		public void TestConfigOrgChanged_DisabledForAutoCreatedTransfers_VASOrder()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			var vasOrder = Factory.New<WhsVASOrder>();
			var configOrgChangedCalled = false;
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");

			transfer.WD_OH_Client = client1.PK;
			vasOrder.WVO_WD_TransferIntoServiceArea = transfer.PK;

			((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;
			transfer.WD_OH_Client = client2.PK;
			AssertEquals("Should *not* support custom labels for auto created transfers.", false, configOrgChangedCalled);

			vasOrder.WVO_WD_TransferIntoServiceArea = ZGuid.Empty;
			vasOrder.WVO_WD_TransferOutOfServiceArea = transfer.PK;
			((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;
		}

		public void TestConfigOrgChanged_DisabledForAutoCreatedTransfers_PickFaceReplenishment()
		{
			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			var configOrgChangedCalled = false;
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");

			transfer.WD_OH_Client = client1.PK;
			transfer.WD_IsPickFaceReplenishment = true;

			((ICustomLabelsConfigOrgProvider)transferLine).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;
			transfer.WD_OH_Client = client2.PK;
			AssertEquals("Should *not* support custom labels for auto created transfers.", false, configOrgChangedCalled);
		}

		#endregion

		#region ILineWithMatchingLines Members

		public void TestILineWithMatchingLines()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, "PLT", 5m);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(year, 1, 1);
			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			transferLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			transferLine.WE_F3_NKPackType = "PLT";
			transferLine.WE_LineNo = 2;
			transferLine.WE_PackageGroupId = "123";
			transferLine.WE_PalletID = "PLT-123";
			transferLine.WE_TransferFromPalletId = "PLT-456";
			transferLine.WE_TransactionQuantity = 5m;

			// properties
			ILineWithMatchingLines<WhsTransferLine> iLineWithCommittedPickLines = transferLine;
			AssertEquals(new ZDateTimeOffset(year, 1, 1), iLineWithCommittedPickLines.ArrivalDate);
			AssertEquals(typeof(WhsInventoryCommitterWithMatchingLines<WhsTransferLine>), iLineWithCommittedPickLines.CommittedStrategy.GetType());
			AssertEquals(transferLine.Inventory, iLineWithCommittedPickLines.Inventory);
			AssertEquals(new ZShort(2), iLineWithCommittedPickLines.LineNo);
			AssertEquals(data.Whs1.FindLocation("A-1"), iLineWithCommittedPickLines.LocationToCommit);
			AssertEquals(ZGuid.Empty, iLineWithCommittedPickLines.MatchingLinePK);
			AssertEquals("transfer", iLineWithCommittedPickLines.Noun);
			AssertEquals("123", iLineWithCommittedPickLines.PackageGroupID);
			AssertEquals("PLT", iLineWithCommittedPickLines.PackType);
			AssertEquals("PLT-456", iLineWithCommittedPickLines.PalletIDToCommit);
			AssertEquals(transfer, iLineWithCommittedPickLines.ParentDocket);
			AssertEquals(transfer.PK, iLineWithCommittedPickLines.ParentDocketPK);
			AssertEquals(transferLine.PickLines, iLineWithCommittedPickLines.PickLines);
			AssertEquals(transferLine.Product, iLineWithCommittedPickLines.Product);
			AssertEquals(data.Part1.PK, iLineWithCommittedPickLines.ProductPK);
			AssertEquals(false, iLineWithCommittedPickLines.IsFinalising);
			AssertEquals(InventoryStatus.Codes.Held, iLineWithCommittedPickLines.TransactionInventoryStatus);
			AssertEquals(InventoryHoldCodes.Codes.Damaged, iLineWithCommittedPickLines.TransactionInventoryHeldCode);
			AssertEquals(data.Whs1.FindLocation("A-2").PK, iLineWithCommittedPickLines.TransactionLocation);
			AssertEquals("PLT-123", iLineWithCommittedPickLines.TransactionPalletID);
			AssertEquals(5m, iLineWithCommittedPickLines.TransactionQty);
			AssertEquals("transfer", iLineWithCommittedPickLines.Verb);

			AssertEquals("Should *not* be able to create inventory.", false, iLineWithCommittedPickLines.CanCreateInventory);

			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				AssertEquals("IsFinalising should be true when transfer line is finalising.", true, iLineWithCommittedPickLines.IsFinalising);
				AssertEquals("Should be able to create inventory.", true, iLineWithCommittedPickLines.CanCreateInventory);

				AssertNoErrors("Precondition", transferLine.QtyToMoveIncludingMatchingLinesInfo);

				transferLine.Validation.ValidateQtyToMoveIncludingMatchingLines();
				AssertHasError(transferLine.QtyToMoveIncludingMatchingLinesInfo, @"Attempted to transfer 5 Units, but no Units are available for transfer out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the transfer line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to transfer.
If you are trying to transfer stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to transfer stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.");
			}

			// matching lines
			var matchingLine1 = transferLine.MatchingLines.AddNew();
			var matchingLine2 = transferLine.MatchingLines.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { matchingLine1, matchingLine2 }, iLineWithCommittedPickLines.MatchingLines);
			AssertEquals(transferLine.PK, ((ILineWithMatchingLines<WhsTransferLine>)matchingLine1).MatchingLinePK);
			AssertEquals(transferLine.PK, ((ILineWithMatchingLines<WhsTransferLine>)matchingLine2).MatchingLinePK);

			// setters
			AssertEquals("Precondition: Per Package Qty not set.", 0m, transferLine.WE_PerPackageQty);
			((ILineWithCommittedPickLines)iLineWithCommittedPickLines).PerPackageQty = 2m;
			AssertEquals(2m, transferLine.WE_PerPackageQty);
			AssertEquals(2m, iLineWithCommittedPickLines.PerPackageQty);

			((ILineWithMatchingLines<WhsTransferLine>)matchingLine1).MatchingLinePK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, matchingLine1.WE_WE_MatchingLine);

			matchingLine1.WE_WE_MatchingLine = transferLine.PK;
			AssertEquals(transferLine.PK, ((ILineWithMatchingLines<WhsTransferLine>)matchingLine1).MatchingLinePK);

			iLineWithCommittedPickLines.TransactionQty = 10m;
			AssertEquals(10m, transferLine.WE_TransactionQuantity);

			transferLine.WE_TransactionQuantity = 5m;
			AssertEquals(5m, iLineWithCommittedPickLines.TransactionQty);

			iLineWithCommittedPickLines.ParentDocketPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, transferLine.WE_WD);

			transferLine.WE_WD = transfer.PK;
			AssertEquals(transfer.PK, iLineWithCommittedPickLines.ParentDocketPK);
		}

		public void TestILineWithMatchingLines_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", "A");
			transferLine.RunPreSaveValidation();

			ILineWithMatchingLines<WhsTransferLine> iLineWithCommittedPickLines = transferLine;
			AssertEquals("Should *not* be In-Transit.", false, iLineWithCommittedPickLines.IsInTransit);
			AssertEquals("Should *not* be able to create inventory.", false, iLineWithCommittedPickLines.CanCreateInventory);

			CollectionCountChangedEventHandler assertionHook = null;
			assertionHook += (b, args) =>
			{
				AssertEquals("Should be In-Transit.", true, iLineWithCommittedPickLines.IsInTransit);
				AssertEquals("Should be able to create inventory, this is controlled by a semaphore.", true, iLineWithCommittedPickLines.CanCreateInventory);

				transferLine.Inventory.CountChanged -= assertionHook;
			};

			transferLine.Inventory.CountChanged += assertionHook;

			var now = ZDateTimeOffset.Now;
			transferLine.PickedTime = now;
			AssertEquals("Precondition: Should have set inventory status.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Should have set inventory status.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Should be In-Transit.", true, iLineWithCommittedPickLines.IsInTransit);

			transferLine.Inventory.DeleteAll();

			AssertEquals("Should *not* be able to create inventory, this is controlled by a semaphore.", false, iLineWithCommittedPickLines.CanCreateInventory);
			AssertExceptionThrown(typeof(InvalidOperationException), "Should not create Inventory if CanCreateInventory flag is not set.", () => transferLine.IncreaseStockInLocation(ZGuid.Empty, 5m));
		}

		public void TestILineWithMatchingLines_InTransitIf_PuttingAway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, dockDoorLocation, "PLT1");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT1", 15m);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();

			AssertEquals($"InventoryStatus of Precondition: Transfer Line should be {InventoryStatus.Codes.PuttingAway}.", InventoryStatus.Codes.PuttingAway, transferLine.WE_CurrentInventoryStatus);

			ILineWithMatchingLines<WhsTransferLine> iLineWithCommittedPickLines = transferLine;
			AssertEquals("Should be In-Transit.", true, iLineWithCommittedPickLines.IsInTransit);
		}

		#endregion

		#region TestILineTransactionInventoryStatus

		public void TestILineTransactionInventoryStatus()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var recieve = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, data.Whs1.FindLocation("A-1"), "");
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition", initialTransfer);
			AssertEquals("Precondition", InventoryStatus.Codes.Available , initialTransfer.Lines[0].WE_OriginalInventoryStatus);
			Factory.Save();

			ILineWithInventory initialTransferILine = initialTransfer.Lines[0];
			AssertEquals("TransferIn should return the original inventory status", initialTransfer.Lines[0].WE_OriginalInventoryStatus, initialTransferILine.TransactionInventoryStatus);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			ILineWithInventory initialTransferILineFinalised = initialTransfer.Lines[0];
			AssertEquals("TransferIn should still return the original inventory status", initialTransfer.Lines[0].WE_OriginalInventoryStatus, initialTransferILineFinalised.TransactionInventoryStatus);

			vasOrder.MarkVASOrderAsCompleted(Notify);
			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
				AssertNotNull("Precondition", returnTransfer);
				AssertEquals("Precondition", InventoryStatus.Codes.Available, returnTransfer.Lines[0].WE_OriginalInventoryStatus);
			}
			Factory.Save();

			ILineWithInventory returnTransferILine = returnTransfer.Lines[0];
			AssertEquals("Non-Finalised TransferOut should return a Staged inventory status", InventoryStatus.Codes.Staged, returnTransferILine.TransactionInventoryStatus);

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(returnTransfer);
			Factory.Save();

			ILineWithInventory returnTransferILineFinalised = returnTransfer.Lines[0];
			AssertEquals("Finalised TransferOut should return the original inventory status", returnTransfer.Lines[0].WE_OriginalInventoryStatus, returnTransferILineFinalised.TransactionInventoryStatus);
		}

		#endregion

		#region TestDestWarehouseReadOnly

		protected override void TestDestWarehouseReadOnly(Func<WhsDocketLine, ZPropertyInfo> getInfo)
		{
			TestReadOnly(getInfo, true, true, true, true);
		}

		#endregion

		#region TestSetLocationDataFromInventory_PutawayTransfer_WE_WL_TransferFromCorrect

		public void TestSetLocationDataFromInventory_PutawayTransfer_WE_WL_TransferFromCorrect()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 10m, dockDoorLocation, "B", false, false);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, locationA2, "EE");
			AssertEquals("Precondition: TransferLine WE_WLTransferFrom is A-2", locationA2.PK, transferLine.WE_WL_TransferFrom);

			var inventory = receive.Inventory[0];
			transferLine.SetDocketLineFromInventory(inventory.InDocketLine, ExcludeFromCopy.None);
			AssertEquals("TransferLine WE_WLTransferFrom is DOCKDOOR", dockDoorLocation.PK, transferLine.WE_WL_TransferFrom);
		}

		#endregion

		#region TestOnSave_OnlyVasOrderTransferOutCanUpdateVasOrderTransferInInventory

		public void TestOnSave_OnlyVasOrderTransferOutCanUpdateVasOrderTransferInInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);

			var transferInInventory = intoServiceAreaTransfer.Lines.Single();
			AssertEquals("Precondition: inventory status is Staged.", InventoryStatus.Codes.Staged, transferInInventory.WE_CurrentInventoryStatus);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var transferInInventoryInNewFactory = newFactory.Load<WhsTransferLine>(transferInInventory.PK);

			transferInInventoryInNewFactory.HeldCodeToChangeTo = string.Empty;
			transferInInventoryInNewFactory.HeldCodeChangeQuantity = 5m;
			transferInInventoryInNewFactory.IsInventoryEditForm = true;

			Helper.AssertZCannotSaveExceptionThrown("You cannot modify VAS Order Transfer In inventory outside of its VAS Order Transfer Out.", newFactory.Save);
		}

		public void TestOnSave_OnlyVasOrderTransferOutCanUpdateVasOrderTransferInInventory_FinalizeVasOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);

			var transferInInventory = intoServiceAreaTransfer.Lines.Single();
			AssertEquals("Precondition: inventory status is staged.", InventoryStatus.Codes.Staged, transferInInventory.WE_CurrentInventoryStatus);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var vasOrderInNewFactory = newFactory.Load<WhsVASOrder>(vasOrder.PK);

			vasOrderInNewFactory.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order completed.", true, vasOrderInNewFactory.WVO_WorkCompletedTimeUtc.IsValid);
			newFactory.Save();

			vasOrderInNewFactory.FinaliseVASOrder(Notify, false);
			AssertNoExceptionThrown("No you cannot modify VAS order transfer in inventory error.", () => newFactory.Save());

			var transferInInventoryInNewFactory = newFactory.Load<WhsTransferLine>(transferInInventory.PK);
			AssertEquals("Transfer in inventory status is updated to available.", InventoryStatus.Codes.Available, transferInInventoryInNewFactory.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestVasOrderTransferInInventoryCanOnlyBePickedByVasOrderTransferOut

		public void TestVasOrderTransferInInventoryCanOnlyBePickedByVasOrderTransferOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);

			var transferInInventory = intoServiceAreaTransfer.Lines.Single();
			AssertEquals("Precondition: inventory status is staged.", InventoryStatus.Codes.Staged, transferInInventory.WE_CurrentInventoryStatus);
			Factory.Save();

			var destinationLocation = data.Whs1.FindLocation("A-2");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, transferInInventory.Location, destinationLocation);
			transferLine.SetDocketLineFromInventory(transferInInventory, ExcludeFromCopy.None);
			transferLine.RunPreSaveValidation();

			AssertHasErrorContaining(transferLine.QtyToMoveIncludingMatchingLinesInfo, "Attempted to transfer 5 Units, but no Units are available for transfer out of this location.");
		}

		public void TestVasOrderTransferInInventoryCanOnlyBePickedByVasOrderTransferOut_FinalisedVasOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			var transferInLine = (WhsTransferLine)intoServiceAreaTransfer.Lines.Single();
			transferInLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("VAS order is completed", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			vasOrder.FinaliseVASOrder(Notify, false);
			AssertEquals("VAS order is finalised.", true, vasOrder.IsFinalised);
			Factory.Save();

			var transferInInventory = intoServiceAreaTransfer.Lines.Single();
			AssertEquals("Precondition: inventory status is available.", InventoryStatus.Codes.Available, transferInInventory.WE_CurrentInventoryStatus);
			Factory.Save();

			var destinationLocation = data.Whs1.FindLocation("A-2");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, transferInInventory.Location, destinationLocation);
			transferLine.SetDocketLineFromInventory(transferInInventory, ExcludeFromCopy.None);
			transferLine.RunPreSaveValidation();
			transfer.FinaliseDocketWithoutUserConfirmation();

			AssertEquals("Transfer is finalised.", true, transfer.IsFinalised);
		}

		#endregion

		#region TestIsIntoServiceAreaTransferForVASOrder

		public void TestIsIntoServiceAreaTransferForVASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Transfer In is created.", intoServiceAreaTransfer);

			var transferInInventory = (WhsTransferLine)intoServiceAreaTransfer.Lines.Single();
			AssertEquals("IsIntoServiceAreaTransferForVASOrder", true, transferInInventory.IsIntoServiceAreaTransferForVASOrder);
		}

		public void TestIsIntoServiceAreaTransferForVASOrder_NotVasOrderTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			AssertEquals("IsIntoServiceAreaTransferForVASOrder", false, transferLine.IsIntoServiceAreaTransferForVASOrder);
		}

		#endregion

		#region TestIsOutOfServiceAreaTransferForVASOrder

		public void TestIsOutOfServiceAreaTransferForVASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			var transferLine = (WhsTransferLine)intoServiceAreaTransfer.Lines.Single();

			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			intoServiceAreaTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(intoServiceAreaTransfer);
			AssertNoExceptionThrown(() => Factory.Save()); // ensure data is correct

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}
			AssertNotNull("Precondition: Return transfer successfully created.", returnTransfer);

			var transferLineOnReturnTransfer = (WhsTransferLine)returnTransfer.Lines.Single();
			AssertEquals("IsOutOfServiceAreaTransferForVASOrder", true, transferLineOnReturnTransfer.IsOutOfServiceAreaTransferForVASOrder);
		}

		public void TestIsOutOfServiceAreaTransferForVASOrder_NotVasOrderTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			AssertEquals("IsOutOfServiceAreaTransferForVASOrder", false, transferLine.IsOutOfServiceAreaTransferForVASOrder);
		}

		#endregion

		#region Implementation

		protected override FinalisableDocketHelper<WhsTransfer> GetNewDocketHelper(BusinessObjectFactory factory)
		{
			return new FinalisableTransferHelper(factory);
		}

		protected override Type FetchStrategyType => typeof(WhsTransferLineFetchStrategy);

		#region NeedDocketLine_TransferFrom

		protected override bool NeedDocketLine_TransferFrom
		{
			get { return true; }
		}

		protected override bool SupportsCustomsSubType => false;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var transferLine = (WhsTransferLine)base.GetNewBusinessObjectForDeleteTest(factory);
			transferLine.Docket.WD_OH_Client = data.Org1.PK;
			transferLine.Docket.WD_WW_Whs = data.Whs1.PK;
			transferLine.WE_OP = data.Part1.PK;
			transferLine.WE_TransactionQuantity = 1m;
			transferLine.WE_WL_TransferFrom = data.Whs1.DefaultLocation.PK;
			transferLine.RunPreSaveValidation();
			return transferLine;
		}

		#endregion

		#endregion
	}
}
