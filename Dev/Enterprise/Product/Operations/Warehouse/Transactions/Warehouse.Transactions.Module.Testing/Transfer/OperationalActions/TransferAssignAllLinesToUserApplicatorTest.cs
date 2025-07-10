using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(TransferAssignAllLinesToUserApplicator))]
	class TransferAssignAllLinesToUserApplicatorTest : LinesAssignerActionMethodApplicatorTest<TransferAssignAllLinesToUserApplicator, WhsTransfer>
	{
		#region Test Operational Action

		#region TestOperationalAction_PickOnly

		[TestDate(2012, 06, 04)]
		public void TestOperationalAction_PickOnly()
		{
			option = AssignLineOptions.PickOnly;

			// setup test data

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("AA", "Antman");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 7m);
			Factory.Save();
			var location = receive.Inventory[0].Location;

			var enteredtransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var enteredTransfersWithNoUnAssignedLines = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var finalizedTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var cancelledTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			enteredtransfer.WD_DocketStatus = DocketStatus.Codes.Entered;
			enteredTransfersWithNoUnAssignedLines.WD_DocketStatus = DocketStatus.Codes.Entered;
			cancelledTransfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;

			var lineInFinalizedTransfer = Helper.CreateWhsTransferLine(finalizedTransfer, data.Part1, 1m, location, location);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				finalizedTransfer.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(finalizedTransfer);
				AssertEquals("Picker will be set to finaliser.", "AA", lineInFinalizedTransfer.GS_NKPickedBy);
				AssertEquals("Putaway-er will be set to finaliser.", "AA", lineInFinalizedTransfer.WE_GS_NKPutawayBy);
			}

			var lineWithEmptyPickedAndPutAwayDate = Helper.CreateWhsTransferLine(enteredtransfer, data.Part1, 1m, location.ToLocationString(), "");
			var lineWithEmptyPickedAndNonEmptyPutAwayDate = Helper.CreateWhsTransferLine(enteredtransfer, data.Part1, 1m, location.ToLocationString(), "");
			lineWithEmptyPickedAndNonEmptyPutAwayDate.WE_WL = data.Whs1.FindLocation("A-2").PK;
			lineWithEmptyPickedAndPutAwayDate.RunPreSaveValidation(); // to commit inventory
			lineWithEmptyPickedAndNonEmptyPutAwayDate.RunPreSaveValidation(); // to commit inventory

			var lineWithNonEmptyPickedAndEmptyPutAwayDate = Helper.CreateWhsTransferLine(enteredtransfer, data.Part1, 1m, location.ToLocationString(), "");
			lineWithNonEmptyPickedAndEmptyPutAwayDate.RunPreSaveValidation(); // to commit inventory
			lineWithNonEmptyPickedAndEmptyPutAwayDate.PickedTime = ZDateTimeOffset.Now;
			var lineWithNonEmptyPickedAndNonEmptyPutAwayDate = Helper.CreateWhsTransferLine(enteredtransfer, data.Part1, 1m, location.ToLocationString(), "");
			lineWithNonEmptyPickedAndNonEmptyPutAwayDate.RunPreSaveValidation(); // to commit inventory
			lineWithNonEmptyPickedAndNonEmptyPutAwayDate.PickedTime = ZDateTimeOffset.Now;
			lineWithNonEmptyPickedAndNonEmptyPutAwayDate.WE_WL = data.Whs1.FindLocation("A-2").PK;

			var assignedLine = Helper.CreateWhsTransferLine(enteredTransfersWithNoUnAssignedLines, data.Part1, 1m, location.ToLocationString(), "");
			assignedLine.RunPreSaveValidation(); // to commit inventory
			assignedLine.PickedTime = ZDateTimeOffset.Now;
			assignedLine.WE_WL = data.Whs1.FindLocation("A-2").PK;

			Factory.Save();

			lineWithEmptyPickedAndNonEmptyPutAwayDate.WE_GS_NKPutawayBy = ""; // Clear default
			lineWithNonEmptyPickedAndNonEmptyPutAwayDate.WE_GS_NKPutawayBy = ""; // Clear default
			assignedLine.WE_GS_NKPutawayBy = ""; // Clear default

			// Test Operational action

			var errorMessage = string.Format("INFO: Transfer [HL {0}] - All lines have been assigned successfully.\n\r" +
														 "INFO: Transfer [HL {1}] - No Lines were assigned because there are no unassigned lines, or Picking has already commenced.\n\r" +
														 "WARNING: Transfer [HL {2}] - No Lines were assigned because the Transfer is Finalized.\n\r" +
														 "WARNING: Transfer [HL {3}] - No Lines were assigned because the Transfer is Canceled.", enteredtransfer.WD_DocketID, enteredTransfersWithNoUnAssignedLines.WD_DocketID, finalizedTransfer.WD_DocketID, cancelledTransfer.WD_DocketID);

			ApplyApplicator(new WhsTransfer[] { enteredtransfer, enteredTransfersWithNoUnAssignedLines, finalizedTransfer, cancelledTransfer }, errorMessage);

			AssertEquals(SelectedUser.GS_Code, lineWithEmptyPickedAndPutAwayDate.GS_NKPickedBy);
			AssertEquals("", lineWithEmptyPickedAndPutAwayDate.WE_GS_NKPutawayBy);
			AssertEquals(SelectedUser.GS_Code, lineWithEmptyPickedAndNonEmptyPutAwayDate.GS_NKPickedBy);
			AssertEquals("", lineWithEmptyPickedAndNonEmptyPutAwayDate.WE_GS_NKPutawayBy);
			AssertEquals("E", lineWithNonEmptyPickedAndEmptyPutAwayDate.GS_NKPickedBy); // automatically set  by default
			AssertEquals("", lineWithNonEmptyPickedAndEmptyPutAwayDate.WE_GS_NKPutawayBy);
			AssertEquals("E", lineWithNonEmptyPickedAndNonEmptyPutAwayDate.GS_NKPickedBy); // automatically set  by default
			AssertEquals("", lineWithNonEmptyPickedAndNonEmptyPutAwayDate.WE_GS_NKPutawayBy);
			AssertEquals("Picked By should not have been Updated by Assign User Operational Action.", "AA", lineInFinalizedTransfer.GS_NKPickedBy);
			AssertEquals("AA", lineInFinalizedTransfer.WE_GS_NKPutawayBy);
		}

		#endregion

		#region TestOperationalAction_PickOnly_CannotChangeIfIsPicking

		[TestDate(2012, 06, 04)]
		public void TestOperationalAction_PickOnly_CannotChangeIfIsPicking()
		{
			option = AssignLineOptions.PickOnly;

			// setup test data
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var ben = Helper.CreateGlbStaff("Ben", "Ben");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, loc1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.Option = AssignLineOptions.PickOnly;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 6m, loc1, loc2);
			var lineAssigner = (ILineStaffAssigner)transferLine;
			lineAssigner.AssignLine(ben);
			transfer.RunPreSaveValidation(); // to commit inventory.
			Factory.Save();

			transferLine.PickLines[0].WZ_IsPicking = true;

			var errorMessage = string.Format("INFO: Transfer [HL {0}] - No Lines were assigned because there are no unassigned lines, or Picking has already commenced.\n\r", transfer.WD_DocketID);

			ApplyApplicator(new WhsTransfer[] { transfer }, errorMessage);

			AssertEquals(ben.GS_Code, transferLine.GS_NKPickedBy);

			transferLine.PickLines[0].WZ_IsPicking = false;

			errorMessage = string.Format("INFO: Transfer [HL {0}] - All lines have been assigned successfully.\n\r", transfer.WD_DocketID);

			AssertNotEquals("Precondition. selected user is not ben.", ben.GS_Code, SelectedUser.GS_Code);
			ApplyApplicator(new WhsTransfer[] { transfer }, errorMessage);

			AssertNotEquals("Should not be assigned to ben anymore.", ben.GS_Code, transferLine.GS_NKPickedBy);
		}

		#endregion

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var applicator = (TransferAssignAllLinesToUserApplicator)base.GetNewBusinessObject();
			applicator.Option = option;
			return applicator;
		}

		protected override TransferAssignAllLinesToUserApplicator GetNewApplicator()
		{
			return new TransferAssignAllLinesToUserApplicator(Factory);
		}

		protected override Type ExpectedApplicatorValidationType => typeof(AssignAllLinesApplicatorValidation<WhsTransfer>);

		#endregion

		AssignLineOptions option;
	}
}
