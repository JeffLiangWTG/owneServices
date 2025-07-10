using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[UseSnapshotProtection]
	class PreventInventoryTotalUnitsFromBeingReducedIncorrectlyTest : TestCase
	{
		#region TestFinaliseDocket_MultiplePickLinesOnSameInventory

		public void TestFinaliseDocket_MultiplePickLinesOnSameInventory()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var helper = new WhsTestHelperFunctions(factory);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory.Cast<WhsInventoryView>().Single();
			factory.Save();

			var newFactory1 = new BusinessObjectFactory();
			var newFactory2 = new BusinessObjectFactory();

			var helper1InFactory1 = new WhsTestHelperFunctions(newFactory1);
			var helper2InFactory2 = new WhsTestHelperFunctions(newFactory2);

			var order1 = helper1InFactory1.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper1InFactory1.CreateWhsOrderLine(order1, data.Part1, 100m);
			var pick1 = helper1InFactory1.CreatePickNew(order1);
			newFactory1.Save();

			var order2 = helper2InFactory2.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = helper2InFactory2.CreateWhsOrderLine(order2, data.Part1, 100m);
			var pick2 = helper2InFactory2.CreatePickNew(order2);
			newFactory2.Save();

			pick1.FinaliseAllOrders();
			pick1.FinalisePick();

			pick2.FinaliseAllOrders();
			pick2.FinalisePick();

			AssertNoExceptionThrown(newFactory1.Save);
			AssertHasRowError(orderLine2, "Units for Inventory that was committed have been changed by another user. Reload the current job to redo changes.");
			helper.AssertZCannotSaveExceptionThrown("Units for Inventory that was committed have been changed by another user. Reload the current job to redo changes.", newFactory2.Save);
		}

		#endregion

		#region TestAddRowErrorToPicklinesWhenUnitsOfCommittedInventoryHasBeenChanged

		public void TestAddRowErrorToPicklinesWhenUnitsOfCommittedInventoryHasBeenChanged()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var helper = new WhsTestHelperFunctions(factory);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 300m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory.Cast<WhsInventoryView>().Single();
			factory.Save();

			var newFactory1 = new BusinessObjectFactory();
			var newFactory2 = new BusinessObjectFactory();
			var newFactory3 = new BusinessObjectFactory();

			var helper1InFactory1 = new WhsTestHelperFunctions(newFactory1);
			var helper2InFactory2 = new WhsTestHelperFunctions(newFactory2);
			var helper3InFactory3 = new WhsTestHelperFunctions(newFactory2);

			var order1 = helper1InFactory1.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper1InFactory1.CreateWhsOrderLine(order1, data.Part1, 100m);
			var pick1 = helper1InFactory1.CreatePickNew(order1);
			newFactory1.Save();

			var order2 = helper2InFactory2.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = helper2InFactory2.CreateWhsOrderLine(order2, data.Part1, 100m);
			var pick2 = helper2InFactory2.CreatePickNew(order2);
			newFactory2.Save();

			var order3 = helper3InFactory3.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine3 = helper3InFactory3.CreateWhsOrderLine(order3, data.Part1, 100m);
			var pick3 = helper3InFactory3.CreatePickNew(order3);
			newFactory3.Save();

			pick1.FinaliseAllOrders();
			pick1.FinalisePick();

			pick2.FinaliseAllOrders();
			pick2.FinalisePick();

			pick3.FinaliseAllOrders();
			pick3.FinalisePick();

			AssertNoExceptionThrown(newFactory1.Save);
			AssertHasRowError(orderLine2, "Units for Inventory that was committed have been changed by another user. Reload the current job to redo changes.");
			AssertHasRowError(orderLine3, "Units for Inventory that was committed have been changed by another user. Reload the current job to redo changes.");
		}

		#endregion

		#region TestAddRowErrorToPicklinesWhenUnitsOfCommittedInventoryHasBeenChanged_WithoutFinalise

		public void TestAddRowErrorToPicklinesWhenUnitsOfCommittedInventoryHasBeenChanged_WithoutFinalise()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var helper = new WhsTestHelperFunctions(factory);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory.Cast<WhsInventoryView>().Single();
			factory.Save();

			var newFactory1 = new BusinessObjectFactory();
			var newFactory2 = new BusinessObjectFactory();

			var helper1InFactory1 = new WhsTestHelperFunctions(newFactory1);
			var helper2InFactory2 = new WhsTestHelperFunctions(newFactory2);

			var order1 = helper1InFactory1.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper1InFactory1.CreateWhsOrderLine(order1, data.Part1, 100m);
			var pick1 = helper1InFactory1.CreatePickNew(order1);
			newFactory1.Save();

			var order2 = helper2InFactory2.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = helper2InFactory2.CreateWhsOrderLine(order2, data.Part1, 100m);
			var pick2 = helper2InFactory2.CreatePickNew(order2);
			newFactory2.Save();

			orderLine1.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			orderLine2.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;

			AssertNoExceptionThrown(newFactory1.Save);
			AssertHasRowError(orderLine2, "Units for Inventory that was committed have been changed by another user. Reload the current job to redo changes.");
		}

		#endregion

		#region TestFinaliseDocket_PickAndTransferPickingSameInventory

		public void TestFinaliseDocket_PickAndTransferPickingSameInventory()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var helper = new WhsTestHelperFunctions(factory);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory.Cast<WhsInventoryView>().Single();
			factory.Save();

			var newFactory1 = new BusinessObjectFactory();
			var newFactory2 = new BusinessObjectFactory();

			var helper1InFactory1 = new WhsTestHelperFunctions(newFactory1);
			var helper2InFactory2 = new WhsTestHelperFunctions(newFactory2);

			var order = helper1InFactory1.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = helper1InFactory1.CreateWhsOrderLine(order, data.Part1, 100m);

			var pick = helper1InFactory1.CreatePickNew(order);
			newFactory1.Save();

			var transfer = helper2InFactory2.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = helper2InFactory2.CreateWhsTransferLine(transfer, data.Part1, 100m, "A-1", "A-2");
			transfer.RunPreSaveValidation();
			newFactory2.Save();
			AssertEquals("Precondition - inventory should be committed.", 100m, transferLine.QtyCommittedIncludingMatchingLines);

			pick.FinaliseAllOrders();
			pick.FinalisePick();

			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

			transfer.FinaliseDocket();
			newFactory1.Save();
			AssertHasRowError(transferLine, "Units for Inventory that was committed have been changed by another user. Reload the current job to redo changes.");
			helper.AssertZCannotSaveExceptionThrown("Units for Inventory that was committed have been changed by another user. Reload the current job to redo changes.", newFactory2.Save);

			var otherFactory = new BusinessObjectFactory();
			var transferInOtherFactory = otherFactory.Load<WhsTransfer>(transfer.PK);
			var transferLineInOtherFactory = otherFactory.Load<WhsTransferLine>(transferLine.PK);

			AssertEquals("Transfer should not be finalised.", false, transferInOtherFactory.IsFinalised);
			AssertEquals("Transfer line should not be finalised.", false, transferLineInOtherFactory.IsFinalised);
			AssertEquals("Precondition - inventory should be committed.", 100m, transferLineInOtherFactory.QtyCommittedIncludingMatchingLines);

			transferInOtherFactory.FinaliseDocketWithoutUserConfirmation();
			AssertNoExceptionThrown(otherFactory.Save);
			AssertEquals("Transfer should be finalised.", true, transferLineInOtherFactory.IsFinalised);
			AssertEquals("Transfer line should be finalised.", true, transferLineInOtherFactory.IsFinalised);

			var sourceInventory = otherFactory.Load<WhsInventoryView>(inventory.PK);
			var destinationInventory = otherFactory.Load<WhsInventoryView>(transferLineInOtherFactory.Inventory.Cast<WhsInventoryView>().Single().PK);

			AssertEquals("Inventory should be taken from source location.", 0m, sourceInventory.WI_TotalUnits);
			AssertEquals("Inventory should be transferred to destlocation.", 100m, destinationInventory.WI_TotalUnits);
		}

		#endregion

		#region TestFinalise_MultipleTransferLinesOnSameInventory

		public void TestFinalise_MultipleTransferLinesOnSameInventory()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var helper = new WhsTestHelperFunctions(factory);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory.Cast<WhsInventoryView>().Single();
			factory.Save();

			var newFactory1 = new BusinessObjectFactory();
			var newFactory2 = new BusinessObjectFactory();

			var helper1InFactory1 = new WhsTestHelperFunctions(newFactory1);
			var helper2InFactory2 = new WhsTestHelperFunctions(newFactory2);

			var transfer1 = helper1InFactory1.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine1 = helper1InFactory1.CreateWhsTransferLine(transfer1, data.Part1, 1m, "A-1", "A-2");
			var transfer2 = helper2InFactory2.CreateWhsTransfer(data.Org1, data.Whs1, "TR2");
			var transferLine2 = helper2InFactory2.CreateWhsTransferLine(transfer2, data.Part1, 2m, "A-1", "A-2");

			transfer1.RunPreSaveValidation();
			transfer2.RunPreSaveValidation();
			newFactory1.Save();
			newFactory2.Save();

			AssertEquals("Precondition - inventory should be committed.", 1m, transferLine1.QtyCommittedIncludingMatchingLines);
			AssertEquals("Precondition - inventory should be committed.", 2m, transferLine2.QtyCommittedIncludingMatchingLines);

			transfer1.FinaliseDocket();
			transfer2.FinaliseDocket();

			AssertNoExceptionThrown(newFactory1.Save);
			AssertHasRowError(transferLine2, "Units for Inventory that was committed have been changed by another user. Reload the current job to redo changes.");
			helper.AssertZCannotSaveExceptionThrown("Units for Inventory that was committed have been changed by another user. Reload the current job to redo changes.", newFactory2.Save);
		}

		#endregion

		#region Implementation

		void AssertHasRowError(WhsDocketLine line, ZString message)
		{
			AssertEquals(message, line.RowErrors.ToUniqueMessageListString());
		}

		#endregion
	}
}
