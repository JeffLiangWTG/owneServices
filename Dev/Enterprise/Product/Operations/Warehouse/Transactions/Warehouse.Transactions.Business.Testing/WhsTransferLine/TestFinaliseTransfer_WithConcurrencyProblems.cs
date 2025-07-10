using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class TestFinaliseTransfer_WithConcurrencyProblems : TestCase
	{
		#region TestTriggerPreventsStatusChangeConcurrencyIssue

		[UseSnapshotProtection]
		public void TestTriggerPreventsStatusChangeConcurrencyIssue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locations[0], "");
			var inventory = receive.Inventory[0];
			var origLinePK = inventory.WI_WE_InDocketLine;

			// Create another receive so TransferLine will have multiple pick lines - for testing the units available in validation message
			// Total units greater than 50 as PickLines are ordered by units in ascending order.
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 55m, locations[0], "");
			Factory.Save();

			// Create Transfer
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 75m, locations[0].ToLocationString(), locations[1].ToLocationString());

			// Split the inventory, 24 of 105 units are now damaged
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var docketLineInOtherFactory = factory2.Load<WhsReceiveLine>(inventory.WI_WE_InDocketLine);
			docketLineInOtherFactory.IsInventoryEditForm = true;
			docketLineInOtherFactory.HeldCodeChangeQuantity = 24;
			docketLineInOtherFactory.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			factory2.Save();

			var list = new WhsInventoryViewCollection(factory2);
			list.Load();
			AssertEquals(3, list.Count);
			var availableInventory = list.Cast<WhsInventoryView>().FirstOrDefault(i => i.WI_InventoryStatus == InventoryStatus.Codes.Available && i.WI_TotalUnits == 26m);
			var damagedInventory = list.Cast<WhsInventoryView>().FirstOrDefault(i => i.WI_InventoryStatus == InventoryStatus.Codes.Held && i.IsDamaged);

			AssertEquals(50m, availableInventory.WI_InDocketLineUnits);
			AssertEquals(origLinePK, availableInventory.WI_WE_InDocketLine);
			AssertEquals(true, availableInventory.WI_IsOriginalReceiptLine);

			AssertEquals(24m, damagedInventory.WI_TotalUnits);
			AssertEquals(0m, damagedInventory.WI_InDocketLineUnits);
			AssertNotEquals(origLinePK, damagedInventory.WI_WE_InDocketLine);
			AssertEquals(origLinePK, damagedInventory.InDocketLine.WE_WE_ParentDocketLine);
			AssertEquals(false, damagedInventory.WI_IsOriginalReceiptLine);

			// Save Transfer Line
			transferLine.WE_TransactionQuantity = 95m;
			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			transferLine.RunPreSaveValidation(); // Commit inventory
			AssertExceptionThrown<ZConcurrencyCheckFailureException>("Trigger to stop over committed pick lines should prevent the factory save.", () => Factory.Save());
			AssertNull("Should have rolled back transaction.", new BusinessObjectFactory().Load<WhsTransferLine>(transferLine.PK));
		}

		#endregion

		#region TestFinaliseTransfer_OverCommittedInventoryBug

		[UseSnapshotProtection]
		public void TestFinaliseTransfer_OverCommittedInventoryBug()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			// Receive 50 Units
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locations[0], "");

			// Create Transfer with line for 50 units
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, locations[0].ToLocationString(), locations[1].ToLocationString());
			transferLine.RunPreSaveValidation(); // Commit inventory
			Factory.Save();

			// HACK: Mock bad existing data that is no longer possible after "PreventOverCommitOfStockViaPickLine" was added
			// Testing that we dont create new stock if the source location could not satisfy the required quantity.
			// Previously stock in source location was not reduced, but new stock was created.
			transferLine.WE_TransactionQuantity = 75m;
			transferLine.PickLines[0].WZ_Units = 75m;
			transferLine.FinaliseDocketLine();
			Assert("Should not be finalised.", !transferLine.IsFinalised);

			AssertHasError(transferLine.QtyToMoveIncludingMatchingLinesInfo, @"Attempted to transfer 75 Units, but only 50 Units are available for transfer out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the transfer line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to transfer.
If you are trying to transfer stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to transfer stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.");

			TestCaseWithFactory.AssertHasRowError("Should have added a RowError.", transferLine, "Error occurred during finalization. Close the form without saving and try again.");
		}

		public static void AssertHasError(ZPropertyInfo info, string notificationExpectedToBeFound)
		{
			// No nice formatting/messages - standard AssertHasError method is in TestCaseWithFactory which subclasses TransactionedTestCase.
			Assert(info.Notifications != null && info.Notifications.GetErrors().Contains(notificationExpectedToBeFound));
		}

		#endregion

		#region TestCanNotAddUnPickedPickLinesToFinalisedDocketLine

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestCanNotAddUnPickedPickLinesToFinalisedDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locationA1, locationA2);
			AssertEquals("Precondition - transfer should not be finalised.", false, transfer.IsFinalised);
			AssertEquals("Precondition - transfer line should not be finalised.", false, transferLine.IsFinalised);

			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: TransferLine is committed.", 5m, transferLine.QtyCommittedIncludingMatchingLines);
			transferLine.FinaliseDocketLine();

			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine);
			AssertEquals("Precondition - Only the transfer line is finalised.", false, transfer.IsFinalised);

			var pickLine = transferLine.PickLines[0];
			AssertNotNull("Precondition.", pickLine);
			AssertEquals("Precondition.", false, pickLine.WZ_PickedDateTime.IsEmpty);

			// Try clearing the existing pick lines picked time
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to save unpicked PickLines that are attached to a Finalised Job.", true), "Should have thrown an exception.");

			pickLine.WZ_PickedDateTime = transferLine.WE_FinalisedDate;
			AssertNoExceptionThrown(Factory.Save);

			// Try adding another Pick line with no picked time set
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var args = new BusinessObjectCloneArgs(new[] { WhsPickLineSchema.Constants.WZ_PickedDateTime }, performRowCopyWithoutTriggeringValidationAndSetter: true);
			var newPickLine = (WhsPickLine)pickLine.Clone(args);
			newPickLine.WZ_WE_InventoryLine = receive2.Lines[0].PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to save unpicked PickLines that are attached to a Finalised Job.", true), "Should have thrown an exception.");

			newPickLine.Delete();
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region Implementation

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory factory;

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		#endregion
	}
}
