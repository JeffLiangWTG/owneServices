using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PreventOverReducingStockViaWhsInventoryTest : TestCase
	{
		// adjustments

		#region TestTrigger_ForAdjustments

		[UseSnapshotProtection]
		public void TestTrigger_ForAdjustments()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(conn);
				var helper = new WhsTestHelperFunctions(factory);
				var data = new TestDataSimpleEnvironment(factory);

				// create 10 inventory
				var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				receive.Lines[0].WE_ClientOrderedUnits = 0m;
				var inventory = receive.Inventory[0];
				factory.Save();

				// create transfer for 7 units
				var adjustment1 = helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
				var adjustment2 = helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A2");
				var adjustmentLine1 = helper.CreateWhsAdjustmentLine(adjustment1, data.Part1, -2m, inventory.Location);
				var adjustmentLine2 = helper.CreateWhsAdjustmentLine(adjustment2, data.Part1, -5m, inventory.Location);
				adjustment2.RunPreSaveValidation(); // commit stock
				adjustment1.FinaliseDocketWithoutUserConfirmation(); // finalise 2 of 7 units
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment1);
				AssertEquals("Precondition: Stock is committed.", 5m, adjustmentLine2.CommittedQuantity);

				factory.Save();

				// change below will create imbalance in 2 places, suspend one check procedure to test other fails.
				WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)", "0");

				inventory.WI_TotalUnits = 5m;
				AssertNoExceptionThrown("Trigger should only ignore finalised picklines.", () => factory.Save());

				inventory.WI_TotalUnits = 1m;
				AssertTriggerPreventsSave(factory); // Save inventory should fail since inventory < Committed units
				AssertDatabaseValueForInventory(inventory.PK, 5m);
			}
		}

		#endregion

		// picks

		#region TestTrigger_ForPicks

		[UseSnapshotProtection]
		public void TestTrigger_ForPicks()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(conn);
				var helper = new WhsTestHelperFunctions(factory);
				var data = new TestDataSimpleEnvironment(factory);

				// create 10 inventory
				var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				receive.Lines[0].WE_ClientOrderedUnits = 0m;
				var inventory = (WhsInventoryView)receive.Inventory.Single();
				factory.Save();

				// pick 7 units for order
				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 7m, WhsPickOption.Codes.Manual);
				var pick = helper.CreatePickNew(order);
				pick.AutoAllocateItemsWithMock();

				factory.Save(); // save the order/pick and create pick lines in DB

				// change below will create imbalance in 2 places, suspend one check procedure to test other fails.
				WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)", "0");

				inventory.WI_TotalUnits = 8;
				AssertNoExceptionThrown(() => factory.Save());

				inventory.WI_TotalUnits = 7;
				AssertNoExceptionThrown(() => factory.Save());

				inventory.WI_TotalUnits = 6;
				AssertTriggerPreventsSave(factory); // inventory is less than committed units
				AssertDatabaseValueForInventory(inventory.PK, 7m);
			}
		}

		#endregion

		#region TestTrigger_ForPicks_DirectSQL

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTrigger_ForPicks_DirectSQL()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection1);
				var helper = new WhsTestHelperFunctions(factory);
				var data = new TestDataSimpleEnvironment(factory);

				// create 10 inventory
				var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				receive.Lines[0].WE_ClientOrderedUnits = 0m;
				var inventory = (WhsInventoryView)receive.Inventory.Single();
				factory.Save();

				// pick 7 units for order
				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 7m, WhsPickOption.Codes.Manual);
				var pick = helper.CreatePickNew(order);
				pick.AutoAllocateItemsWithMock();

				factory.Save(); // save the order/pick and create pick lines in DB

				connection2.BeginTransaction();
				AssertEquals("Precondition: Connection is in Transaction.", true, connection2.IsInTransaction);

				// changes below will trigger multiple triggers, suspending balance checker to test other trigger.
				WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)", "0", connection2);

				// attempt to reduce total units below the committed amount of 7 by setting total units to 6
				NUnit.Framework.Assert.That(() =>
					{
						CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine.UpdateWhere(inventory.WI_WE_InDocketLine.ToGuid()).Set(l => l.WE_StockOnHand, 6).Post(connection2);
					}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), $"TriggerLikelyConcurrencyError: {WhsInventoryView.PreventOverReduceStockViaInventoryLineMessageID}", true), "Trigger should prevent Save.");

				AssertEquals("Transaction should be rolledback.", false, connection2.IsInTransaction);
				AssertDatabaseValueForInventory(inventory.PK, 10m);
			}
		}

		#endregion

		// reserved stock

		#region TestTrigger_ForReservedStock

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTrigger_ForReservedStock()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(conn);
				var helper = new WhsTestHelperFunctions(factory);
				var data = new TestDataSimpleEnvironment(factory);

				// create 10 inventory
				var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				receive.Lines[0].WE_ClientOrderedUnits = 0m;
				var inventory = (WhsInventoryView)receive.Inventory.Single();

				// reserve 3 units
				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);
				var orderLine = order.Lines[0];
				var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
				AssertEquals("Precondition - 3 units should be reserved.", 3m, reservedPickLine.WZ_Units);
				factory.Save();

				inventory.WI_TotalUnits = 1m; // reduce inventory so that Inventory < reserved qty
				NUnit.Framework.Assert.That(factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException)), "Should have thrown an exception.");
				AssertDatabaseValueForInventory(inventory.PK, 10m);
			}
		}

		#endregion

		#region TestTrigger_ConsidersExpectedQuantity

		#region TestTrigger_ConsidersExpectedQuantity_IgnoresFinalisedDockets

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTrigger_ConsidersExpectedQuantity_IgnoresFinalisedDockets()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(conn);
				var helper = new WhsTestHelperFunctions(factory);
				var client = helper.CreateClient("Client");
				var whs = helper.CreateWarehouse("Whs", "A", 4, 4);
				var product = helper.CreateProduct("Product", client);
				var receive = helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 10m, allocateLocations: true, finalise: true);
				var receiveLine = receive.Lines[0];
				var inventory = receive.Inventory[0];
				AssertEquals("receiveLine.WE_TransactionQuantity", 10m, receiveLine.WE_TransactionQuantity);
				AssertEquals("receiveLine.WE_StockOnHand", 10m, receiveLine.WE_StockOnHand);
				AssertEquals("receiveLine.WE_ClientOrderedUnits", 10m, receiveLine.WE_ClientOrderedUnits);

				var order = helper.CreateWhsOrder(client, whs, "Order1");
				var orderLine = helper.CreateWhsOrderLine(order, product, 10m);
				var pickLine = helper.CreateReservePickLine(orderLine, inventory, 10m);
				AssertNoExceptionThrown("Can reserve 10 units for order1 and save without issue.", () => factory.Save());

				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receiveLine);
				AssertEquals("Precondition: receive line's WE_ClientOrderedUnits > 5m.", true, receiveLine.WE_ClientOrderedUnits > 5m);
				NUnit.Framework.Assert.That(() =>
					{
						using (conn.BeginTransactionWithManager())
						{
							conn.ExecuteNonQuery($"EXEC dbo.SuspendTrigger '{WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced}'");
							CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine.UpdateWhere(receiveLine.PK.ToGuid()).Set(l => l.WE_StockOnHand, 5m).Post(conn);
							conn.CommitTransaction();
						}
					}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "TriggerLikelyConcurrencyError: " + WhsInventoryView.PreventOverReduceStockViaInventoryLineMessageID, true), "Should have thrown an exception.");
			}
		}

		#endregion

		#region TestTrigger_ConsidersExpectedQuantity_SingleOrder

		[UseSnapshotProtection]
		public void TestTrigger_ConsidersExpectedQuantity_SingleOrder()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(conn);
				var helper = new WhsTestHelperFunctions(factory);
				var client = helper.CreateClient("Client");
				var whs = helper.CreateWarehouse("Whs");
				var product = helper.CreateProduct("Product", client);
				var receive = helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 15m, whs.DefaultOutboundDockDoorLocation, "", allocateLocations: false, finalise: false);
				var receiveLine = receive.Lines[0];
				var inventory = receive.Inventory[0];
				AssertEquals("receiveLine.WE_TransactionQuantity", 15m, receiveLine.WE_TransactionQuantity);
				AssertEquals("receiveLine.WE_StockOnHand", 15m, receiveLine.WE_StockOnHand);
				AssertEquals("receiveLine.WE_ClientOrderedUnits", 15m, receiveLine.WE_ClientOrderedUnits);

				var order = helper.CreateWhsOrder(client, whs, "Order1");
				var orderLine = helper.CreateWhsOrderLine(order, product, 15m);
				var pickLine = helper.CreateReservePickLine(orderLine, inventory, 15m);
				AssertNoExceptionThrown("Can reserve 15 units for order1 and save without issue.", () => factory.Save());

				receiveLine.WE_TransactionQuantity = 10m;
				AssertEquals("Receive line is not finalised.", false, receiveLine.IsFinalised);
				AssertEquals("Precondition: receive line's WE_StockOnHand > WE_ClientOrderedUnits.", true, receiveLine.WE_ClientOrderedUnits > receiveLine.WE_StockOnHand);
				AssertNoExceptionThrown("Can reserve 15 units for order1 if 15 units are expected.", () => factory.Save());

				receiveLine.WE_ClientOrderedUnits = 10m;
				AssertTriggerPreventsSave(factory);
			}
		}

		#endregion

		#region TestTrigger_ConsidersExpectedQuantity_MultipleOrders

		[UseSnapshotProtection]
		public void TestTrigger_ConsidersExpectedQuantity_MultipleOrders()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(conn);
				var helper = new WhsTestHelperFunctions(factory);
				var client = helper.CreateClient("Client");
				var whs = helper.CreateWarehouse("Whs");
				var product = helper.CreateProduct("Product", client);
				var receive = helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 15m, whs.DefaultOutboundDockDoorLocation, "", allocateLocations: false, finalise: false);
				var receiveLine = receive.Lines[0];
				var inventory = receive.Inventory[0];
				AssertEquals("receiveLine.WE_TransactionQuantity", 15m, receiveLine.WE_TransactionQuantity);
				AssertEquals("receiveLine.WE_StockOnHand", 15m, receiveLine.WE_StockOnHand);
				AssertEquals("receiveLine.WE_ClientOrderedUnits", 15m, receiveLine.WE_ClientOrderedUnits);

				var order1 = helper.CreateWhsOrder(client, whs, "Order1");
				var orderLine1 = helper.CreateWhsOrderLine(order1, product, 5m);
				helper.CreateReservePickLine(orderLine1, inventory, 5m);

				var order2 = helper.CreateWhsOrder(client, whs, "Order2");
				var orderLine2 = helper.CreateWhsOrderLine(order2, product, 5m);
				helper.CreateReservePickLine(orderLine2, inventory, 5m);

				var order3 = helper.CreateWhsOrder(client, whs, "Order3");
				var orderLine3 = helper.CreateWhsOrderLine(order3, product, 5m);
				helper.CreateReservePickLine(orderLine3, inventory, 5m);

				AssertNoExceptionThrown("Can reserve a total of 15 units from receive and save without issue.", () => factory.Save());

				receiveLine.WE_TransactionQuantity = 10m;
				AssertEquals("Receive line is not finalised.", false, receiveLine.IsFinalised);
				AssertEquals("Precondition: receive line's WE_StockOnHand > WE_ClientOrderedUnits.", true, receiveLine.WE_ClientOrderedUnits > receiveLine.WE_StockOnHand);
				AssertNoExceptionThrown("Can reserve a total of 15 units from receive if 15 units are expected.", () => factory.Save());

				receiveLine.WE_ClientOrderedUnits = 10m;
				AssertTriggerPreventsSave(factory);
			}
		}

		#endregion

		#region TestTrigger_ConsidersExpectedQuantity_MultipleReceiveLines

		[UseSnapshotProtection]
		public void TestTrigger_ConsidersExpectedQuantity_MultipleReceiveLines()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(conn);
				var helper = new WhsTestHelperFunctions(factory);
				var client = helper.CreateClient("Client");
				var whs = helper.CreateWarehouse("Whs");
				var product = helper.CreateProduct("Product", client);
				var receive1 = helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 5m, whs.DefaultOutboundDockDoorLocation, "", allocateLocations: false, finalise: false);
				var receive2 = helper.CreateWhsReceiveWithInventory(client, whs, "R2", product, 5m, whs.DefaultOutboundDockDoorLocation, "", allocateLocations: false, finalise: false);
				var receive3 = helper.CreateWhsReceiveWithInventory(client, whs, "R3", product, 5m, whs.DefaultOutboundDockDoorLocation, "", allocateLocations: false, finalise: false);

				CombineAssertions(delegate
				{
					AssertEquals("receiveLine1.Lines[0].WE_TransactionQuantity", 5m, receive1.Lines[0].WE_TransactionQuantity);
					AssertEquals("receiveLine1.Lines[0].WE_TransactionQuantity", 5m, receive1.Lines[0].WE_StockOnHand);
					AssertEquals("receiveLine1.Lines[0].WE_ClientOrderedUnits", 5m, receive1.Lines[0].WE_ClientOrderedUnits);
					AssertEquals("receiveLine2.Lines[0].WE_TransactionQuantity", 5m, receive2.Lines[0].WE_TransactionQuantity);
					AssertEquals("receiveLine2.Lines[0].WE_TransactionQuantity", 5m, receive2.Lines[0].WE_StockOnHand);
					AssertEquals("receiveLine2.Lines[0].WE_ClientOrderedUnits", 5m, receive2.Lines[0].WE_ClientOrderedUnits);
					AssertEquals("receiveLine3.Lines[0].WE_TransactionQuantity", 5m, receive3.Lines[0].WE_TransactionQuantity);
					AssertEquals("receiveLine3.Lines[0].WE_TransactionQuantity", 5m, receive3.Lines[0].WE_StockOnHand);
					AssertEquals("receiveLine3.Lines[0].WE_ClientOrderedUnits", 5m, receive3.Lines[0].WE_ClientOrderedUnits);
				});

				var order1 = helper.CreateWhsOrder(client, whs, "Order1");
				var orderLine1 = helper.CreateWhsOrderLine(order1, product, 15m);
				helper.CreateReservePickLine(orderLine1, receive1.Inventory[0], 5m);
				helper.CreateReservePickLine(orderLine1, receive2.Inventory[0], 5m);
				helper.CreateReservePickLine(orderLine1, receive3.Inventory[0], 5m);
				AssertNoExceptionThrown("Can reserve 15 units between receive inventory and save without issue.", () => factory.Save());

				receive1.Lines[0].WE_TransactionQuantity = 0m;
				receive1.Lines[0].WE_ClientOrderedUnits = 5m;
				AssertNoExceptionThrown("Can reserve 15 units from receives if a total of 15 units are expected.", () => factory.Save());

				receive1.Lines[0].WE_ClientOrderedUnits = 0m;
				AssertTriggerPreventsSave(factory);
			}
		}

		#endregion

		#endregion

		// transfers

		#region TestTrigger_ForTransfers

		[UseSnapshotProtection]
		public void TestTrigger_ForTransfers()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(conn);
				var helper = new WhsTestHelperFunctions(factory);
				var data = new TestDataSimpleEnvironment(factory);

				// create 10 inventory
				var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				receive.Lines[0].WE_ClientOrderedUnits = 0m;
				var inventory = (WhsInventoryView)receive.Inventory.Single();
				factory.Save();

				// create transfer for 7 units
				var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine1 = helper.CreateWhsTransferLine(transfer, data.Part1, 2m, inventory.LocationString, inventory.LocationString);
				var transferLine2 = helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventory.LocationString, inventory.LocationString);
				transfer.RunPreSaveValidation(); // commit stock
				transferLine1.FinaliseDocketLine(); // finalise 2 of 7 units

				factory.Save();

				// change below will create imbalance in 2 places, suspend one check procedure to test other fails.
				WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)", "0");

				inventory.WI_TotalUnits = 5m;
				AssertNoExceptionThrown("Trigger should only ignore finalised picklines.", () => factory.Save());

				inventory.WI_TotalUnits = 1m;
				AssertTriggerPreventsSave(factory); // Save inventory should fail since inventory < Committed units
				AssertDatabaseValueForInventory(inventory.PK, 5m);
			}
		}

		#endregion

		#region TestTrigger_ForPutawayTransfers_DirectSQL

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTrigger_ForPutawayTransfers_DirectSQL()
		{
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			using (var connection2 = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection1);
				var helper = new WhsTestHelperFunctions(factory);
				var data = new TestDataSimpleEnvironment(factory, 2, 1);
				factory.Save(); // need to create dock door location

				var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
				var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

				var receive = helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
				var line = helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLTID", 2m);
				receive.Lines[0].WE_ClientOrderedUnits = 2m;
				factory.Save();

				var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1);
				transfer.WD_IsPutawayTransfer = true;
				var transferLine = helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLTID", 2m);
				transfer.RunPreSaveValidation();
				AssertEquals("Precondition: Stock is Committed.", 2m, transferLine.QtyCommittedIncludingMatchingLines);
				factory.Save();

				connection2.BeginTransaction();
				AssertEquals("Precondition: Connection is in Transaction.", true, connection2.IsInTransaction);

				// to prevent the Constraint which checks that TotalUnits cannot be greater than Transaction Units failing, we set it to zero here so we can test the trigger next
				connection2.ExecuteNonQuery(@"
IF OBJECT_ID('Constraint_ReceiveStockOnHandEqualsTransactionQtyWhenNotFinalised', 'C') IS NOT NULL
	ALTER TABLE dbo.WhsDocketLine DROP CONSTRAINT Constraint_ReceiveStockOnHandEqualsTransactionQtyWhenNotFinalised
"); // To test trigger, we need disable constraint to create bad data.

				// attempt to reduce the SOH below committed value of 2 by setting SOH to 1
				NUnit.Framework.Assert.That(() =>
					{
						CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine.UpdateWhere(line.PK.ToGuid())
						.Set(l => l.WE_StockOnHand, 1)
						.Set(l => l.WE_TransactionQuantity, 1)
						.Set(l => l.WE_ClientOrderedUnits, 1)
						.Post(connection2);
					}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), $"TriggerLikelyConcurrencyError: {WhsInventoryView.PreventOverReduceStockViaInventoryLineMessageID}", true), "Trigger should prevent Save.");

				AssertEquals("Transaction should be rolledback.", false, connection2.IsInTransaction);
			}
		}

		#endregion

		#region Implementation

		void AssertTriggerPreventsSave(BusinessObjectFactory factory)
		{
			var exceptionThrown = false;
			try
			{
				factory.Save();
			}
			catch (Exception e)
			{
				if (e is ZConcurrencyCheckFailureException || e is ZSaveConcurrencyException)
				{
					AssertContains(WhsInventoryView.PreventOverReduceStockViaInventoryLineMessageID, e.Message);
					exceptionThrown = true;
				}
				else
				{
					throw;
				}
			}

			AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
		}

		void AssertDatabaseValueForInventory(ZGuid wE_PK, ZDecimal wI_TotalUnits)
		{
			AssertEquals(wI_TotalUnits, Db.Connection.ExecuteScalar(string.Format("SELECT WE_StockOnHand FROM dbo.WhsDocketLine WHERE WE_PK = '{0}'", wE_PK)));
		}

		#endregion
	}
}
