using System;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Moq;
using NUnit.Framework;
using WTG.NUnit;
using WhsDocketDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocket;
using WhsDocketLineDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketLineTriggerTestCase : WhsTestCaseWithFactory
	{
		#region TestStatusChangeAtLineLevelWillBlowUpIfNotMatchedToDocket

		public void TestStatusChangeAtLineLevelWillBlowUpIfNotMatchedToDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AdditionalTestDataSetupForDocket(data);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-2"), "");

			var line = GetNewDocketLineForNewNonFinalisedDocket(data);
			line.RunPreSaveValidation(); // to commit inventory for transfer and adjustment lines
			Factory.Save();
			AnonymousMethod updateStatusCommand = () => UpdateStatusAndDateCommand_WithoutCheckingSOH(DocketLineStatus.Codes.Finalised, ZDateTimeOffset.Now, line.PK.ToGuid());
			if (line.Docket.PropagatesFinalisedDateAndStatusToLines)
			{
				AssertExceptionThrown<SqlException>(updateStatusCommand);
			}
			else
			{
				AssertNoExceptionThrown(updateStatusCommand);
			}
		}

		#endregion

		#region TestEmptyDocketLineStatusForFinalisedDocketShouldBlowUp

		public void TestEmptyDocketLineStatusForFinalisedDocketShouldBlowUp()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AdditionalTestDataSetupForDocket(data);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-2"), "");

			var line = GetNewDocketLineForNewNonFinalisedDocket(data);
			line.RunPreSaveValidation(); // to commit inventory for transfer and adjustment lines
			CreatePick(data.Whs1, line.Docket);
			Factory.Save();

			line.Docket.FinaliseDocket();
			Factory.Save();
			AssertExceptionThrown<SqlException>(() => UpdateStatusAndDateCommand_WithoutCheckingSOH("", null, line.PK.ToGuid()));
		}

		protected virtual WhsPick CreatePick(WhsWarehouse warehouse, WhsDocket docket)
		{
			return null;
		}

		#endregion

		#region TestWrongDocketLineFinalisedDateForFinalisedDocketShouldBlowUp

		public void TestWrongDocketLineFinalisedDateForFinalisedDocketShouldBlowUp()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AdditionalTestDataSetupForDocket(data);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-2"), "");

			var line = GetNewDocketLineForNewNonFinalisedDocket(data);
			line.RunPreSaveValidation(); // to commit inventory for transfer and adjustment lines
			var docket = line.Docket;
			CreatePick(data.Whs1, docket);
			Factory.Save();

			docket.FinaliseDocket();
			Factory.Save();

			if (docket.PropagatesFinalisedDateAndStatusToLines)
			{
				AssertExceptionThrown<SqlException>(() => UpdateStatusAndDateCommand_WithoutCheckingSOH("FIN", docket.WD_FinalisedDate.AddHours(-1), line.PK.ToGuid()));
			}
			else
			{
				AssertNoExceptionThrown(() => UpdateStatusAndDateCommand_WithoutCheckingSOH("FIN", docket.WD_FinalisedDate.AddHours(-1), line.PK.ToGuid()));
			}
		}

		#endregion

		#region TestFinalisedDocketWithUnfinalisedLineShouldBlowUp

		[ExpectNoExceptions]
		public void TestFinalisedDocketWithUnfinalisedLineShouldBlowUp()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AdditionalTestDataSetupForDocket(data);
			var line1 = GetNewDocketLineForNewNonFinalisedDocket(data);
			var docket = line1.Docket;
			CreatePick(data.Whs1, line1.Docket);
			Factory.Save();

			docket.FinaliseDocket();

			var line2 = CreateUnfinalisedDocketLineOnFinalisedDocket(line1);

			AssertIsFinalisedPrecondition(docket);
			AssertIsFinalisedPrecondition(line1);
			AssertEquals("Precondition: Docket Line 2 is not finalised", false, line2.IsFinalised);

			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to set incorrect WE_DocketLineStatus"), "Insert a new un-finalised line to a finalised docket should throw exception");
		}

		protected virtual WhsDocketLine CreateUnfinalisedDocketLineOnFinalisedDocket(WhsDocketLine finalisedLine)
		{
			var line = finalisedLine.Docket.Lines.AddNew();
			line.WE_OP = finalisedLine.WE_OP;
			line.WE_WL = finalisedLine.WE_WL;
			line.WE_TransactionQuantity = 1m;
			return line;
		}

		#endregion

		#region TestCancelDocketWithUncancelledLineShouldBlowUp

		[ExpectNoExceptions]
		public void TestCancelDocketWithUncancelledLineShouldBlowUp()
		{
			if (!CanBeCancelled)
			{
				Assert("The docket can't be cancelled.", true);
			}
			else
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				AdditionalTestDataSetupForDocket(data);
				var line = GetNewDocketLineForNewNonFinalisedDocket(data);
				var docket = line.Docket;

				SetupDocketLine_CancelDocketWithUncancelledLineShouldBlowUp(line);
				docket.CancelReactivateDocket();
				Factory.Save();

				var connection = ((IDbConnected)Factory).Connection;
				connection.ExecuteNonQuery(@"
IF OBJECT_ID('Constraint_ReceiveStockOnHandEqualsTransactionQtyWhenNotFinalised', 'C') IS NOT NULL
	ALTER TABLE dbo.WhsDocketLine DROP CONSTRAINT Constraint_ReceiveStockOnHandEqualsTransactionQtyWhenNotFinalised
");
				line.WE_DocketLineStatus = ZString.Empty;

				AssertEquals("Precondition: Docket is cancelled", "CAN", docket.WD_DocketStatus);
				AssertNotEquals("Precondition: Docket Line is not cancelled", "CAN", line.WE_DocketLineStatus);

				NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to set incorrect WE_DocketLineStatus"), "Insert a new un-cancelled line to a cancelled docket should throw exception");
			}
		}

		protected virtual void SetupDocketLine_CancelDocketWithUncancelledLineShouldBlowUp(WhsDocketLine line) { }

		#endregion

		#region TestUncancelDocketWithCancelledLineShouldBlowUp

		[ExpectNoExceptions]
		public void TestUncancelDocketWithCancelledLineShouldBlowUp()
		{
			if (!CanBeCancelled)
			{
				Assert("The docket can be cancelled.", true);
			}
			else
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				AdditionalTestDataSetupForDocket(data);
				var line = GetNewDocketLineForNewNonFinalisedDocket(data);
				var docket = line.Docket;
				Factory.Save();

				line.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
				line.WE_StockOnHand = 0m;

				AssertNotEquals("Precondition: Docket is not cancelled", "CAN", docket.WD_DocketStatus);
				AssertEquals("Precondition: Docket Line is cancelled", "CAN", line.WE_DocketLineStatus);

				NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to set incorrect WE_DocketLineStatus"), "Insert a new cancelled line to an-uncancelled docket should throw exception");
			}
		}

		#endregion

		#region TestTG_PreventOverfillLocationWithUnitsCapacity

		#region TestOverfillTrigger for WE_TransactionQuantity

		#region TestOverfillTriggerBlowsUpWhenWeTryToPutTooMuchStockIn

		public void TestOverfillTriggerBlowsUpWhenWeTryToPutTooMuchStockIn()
		{
			if (DoesCreateNewStock)
			{
				OverFlowTriggerTestForWE_TransactionQuantity(3m, shouldBlowup: true);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestOverfillTriggerNotBlowsUpWhenWePutJustEnoughStockIn

		public void TestOverfillTriggerNotBlowsUpWhenWePutJustEnoughStockIn()
		{
			if (DoesCreateNewStock)
			{
				OverFlowTriggerTestForWE_TransactionQuantity(2m, shouldBlowup: false);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		void OverFlowTriggerTestForWE_TransactionQuantity(ZDecimal targetQty, bool shouldBlowup)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AdditionalTestDataSetupForDocket(data);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			locA1.WLV_MaxQuantity = 10m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, locA2, "");

			var pendingTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var pendingTransferLine = Helper.CreateWhsTransferLine(pendingTransfer, data.Part1, 2m, locA2, locA1);
			pendingTransferLine.RunPreSaveValidation(); // to commit inventory
														// At this point we have 6 units in stock and 2 units pending to the location which capacity is 10 units.
			Factory.Save();

			var docketLine = GetNewDocketLineForNewNonFinalisedDocket(data);
			docketLine.WE_WL = locA1.PK;
			docketLine.WE_TransactionQuantity = 1m;
			Factory.Save();

			OverFlowLocationCapacityTriggerTestForSetWE_TransactionQuantityCore(docketLine, targetQty);
			if (shouldBlowup)
			{
				try
				{
					Factory.Save();
					Assert("Trigger should prevent saving this.", false);
				}
				catch (ZSaveConcurrencyException ex)
				{
					AssertEquals("Exception message should be correct.", true, ex.Message.Contains("Attempt to overflow the location unit capacity."));
				}
			}
			else
			{
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		protected virtual void OverFlowLocationCapacityTriggerTestForSetWE_TransactionQuantityCore(WhsDocketLine docketLine, ZDecimal targetQty)
		{
			docketLine.WE_TransactionQuantity = targetQty;
			docketLine.RunPreSaveValidation(); // to commit inventory by transfers and adjustments
		}

		#endregion

		#region TestTransitInventory_WhsLocationUnitsConsumed

		public void TestTransitInventory_WhsLocationUnitsConsumed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var sourceLocation = data.Whs1.FindLocation("A-1");
			sourceLocation.WLV_MaxQuantity = 10m;
			var destLocation = data.Whs1.FindLocation("A-2");
			destLocation.WLV_MaxQuantity = 10m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation.ToLocationString(), destLocation.ToLocationString());
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			sourceLocation.WLV_MaxQuantity = 6;
			destLocation.WLV_MaxQuantity = 6;
			AssertNoErrors("There are currently 10 units consuming capacity for this location, you cannot reduce the Max Capacity below this amount.", sourceLocation.WLV_MaxQuantityInfo);
			AssertHasError(destLocation.WLV_MaxQuantityInfo, "There are currently 10 units consuming capacity for this location, you cannot reduce the Max Capacity below this amount.");
		}

		#endregion

		#region TestTransitInventory_PreventOverfillLocationWithUnitsCapacity_SourceDest

		public void TestTransitInventory_PreventOverfillLocationWithUnitsCapacity_SourceDest()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destLocation = data.Whs1.FindLocation("A-2");
			sourceLocation.WLV_MaxQuantity = 10m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation, destLocation);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, sourceLocation, "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation, destLocation);
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			AssertNoExceptionThrown("No exception thrown as source location is empty.", Factory.Save);
		}

		#endregion

		#region TestTransitInventory_PreventOverfillLocationWithUnitsCapacity_DestLocation

		[ExpectNoExceptions]
		public void TestTransitInventory_PreventOverfillLocationWithUnitsCapacity_DestLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destLocation = data.Whs1.FindLocation("A-2");
			sourceLocation.WLV_MaxQuantity = 10m;
			destLocation.WLV_MaxQuantity = 10m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation, destLocation);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.WE_TransactionQuantity = 22m;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsDocketLine.PreventOverflowLocationQuantityTriggerID), "Location to overflow.");
		}

		#endregion

		#region TestOverfillTrigger for WE_WL

		#region TestOverfillTriggerBlowsUpWhenChangeTheLocationCauseOverfill

		public void TestOverfillTriggerBlowsUpWhenChangeTheLocationCauseOverfill()
		{
			if (DoesCreateNewStock)
			{
				OverFlowTriggerTestForWE_WL(3m, shouldBlowup: true);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestOverfillTriggerNotBlowsUpWhenChangeTheLocationDoesNotCauseOverfill

		public void TestOverfillTriggerNotBlowsUpWhenChangeTheLocationDoesNotCauseOverfill()
		{
			if (DoesCreateNewStock)
			{
				OverFlowTriggerTestForWE_WL(2m, shouldBlowup: false);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		void OverFlowTriggerTestForWE_WL(ZDecimal targetQty, bool shouldBlowup)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AdditionalTestDataSetupForDocket(data);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			locA1.WLV_MaxQuantity = 10m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, locA2, "");
			var pendingTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var pendingTransferLine = Helper.CreateWhsTransferLine(pendingTransfer, data.Part1, 2m, locA2, locA1);
			pendingTransferLine.RunPreSaveValidation(); // to commit inventory
														// At this point we have 6 units in stock and 2 units pending to the location which capacity is 10 units.
			Factory.Save();

			// This new line is pointing to the different location
			var docketLine = GetNewDocketLineForNewNonFinalisedDocket(data);
			docketLine.WE_TransactionQuantity = targetQty;
			docketLine.WE_WL = data.Whs1.FindLocation("A-2").PK;
			docketLine.RunPreSaveValidation(); // to commit inventory for transfer and adjustment lines
			Factory.Save();

			// When we change the location - trigger should blow up if there is an overflow
			OverFlowTriggerTestForSetWE_WLCore(docketLine, pendingTransferLine);
			if (shouldBlowup)
			{
				try
				{
					Factory.Save();
					Assert("Trigger should prevent saving this.", false);
				}
				catch (ZSaveConcurrencyException ex)
				{
					Assert(ex.Message.Contains("Attempt to overflow the location unit capacity."));
				}
			}
			else
			{
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}
		protected virtual void OverFlowTriggerTestForSetWE_WLCore(WhsDocketLine docketLine, WhsTransferLine pendingTransferLine)
		{
			docketLine.WE_WL = pendingTransferLine.WE_WL;
		}

		#endregion

		#region TestOverfillTrigger for WE_StockOnHand

		#region TestOverfillTriggerBlowsUpWhenChangeToTotalUnitsCauseOverfill

		public void TestOverfillTriggerBlowsUpWhenChangeToTotalUnitsCauseOverfill()
		{
			if (DoesCreateNewStock)
			{
				OverFlowTriggerTestForWE_StockOnHand(3m, shouldBlowup: true);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestOverfillTriggerNotBlowsUpWhenChangeToTotalUnitsNotCauseingOverfill

		public void TestOverfillTriggerNotBlowsUpWhenChangeToTotalUnitsNotCauseingOverfill()
		{
			if (DoesCreateNewStock)
			{
				OverFlowTriggerTestForWE_StockOnHand(2m, shouldBlowup: false);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		void OverFlowTriggerTestForWE_StockOnHand(ZDecimal targetQty, bool shouldBlowup)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AdditionalTestDataSetupForDocket(data);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m, locA2, "");
			var pendingTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var pendingTransferLine = Helper.CreateWhsTransferLine(pendingTransfer, data.Part1, 2m, locA2, locA1);
			pendingTransferLine.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			// changes below will create imbalance in 2 places, suspend one check procedure to test other fails.
			WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced_ForInsert} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)", "0");
			WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)", "0");

			var docketLine = GetNewDocketLineForNewFinalisedDocket(data.Whs1, data.Org1, data.Part1, 10m);
			docketLine.WE_StockOnHand = 1m;
			Factory.Save();

			// At this point we have 6+1 units in stock and 2 units pending to the location A-1 which capacity is 10 units.
			locA1.WLV_MaxQuantity = 10m;
			Factory.Save();

			// When we change the total units - trigger should blow up if there is an overflow
			if (shouldBlowup)
			{
				try
				{
					RawWE_StockOnHandUpdate(docketLine.PK, targetQty);
					Assert("Trigger should prevent saving this.", false);
				}
				catch (SqlException ex)
				{
					Assert(ex.Message.Contains("Attempt to overflow the location unit capacity."));
				}
			}
			else
			{
				AssertNoExceptionThrown(() => RawWE_StockOnHandUpdate(docketLine.PK, targetQty));
			}
		}

		void RawWE_StockOnHandUpdate(ZGuid docketLinePK, decimal newWE_StockOnHand) => CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine.UpdateWhere(docketLinePK.ToGuid()).Set(l => l.WE_StockOnHand, newWE_StockOnHand).Post(TestConnection);

		#endregion

		#region TestOverfillTriggerSavingManyRows

		public void TestOverfillTriggerSavingManyRows()
		{
			if (DoesCreateNewStock)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				AdditionalTestDataSetupForDocket(data);
				data.Whs1.FindLocation("A-1").WLV_MaxQuantity = 7m;
				Factory.Save();

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, data.Whs1.FindLocation("A-1"), "");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m, data.Whs1.FindLocation("A-1"), "");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, data.Whs1.FindLocation("A-2"), ""); // to be able to create transfer in a subclass
				var pendingAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ-1");
				var pendingAdjustmentLine = Helper.CreateWhsAdjustmentLine(pendingAdjustment, data.Part1, 2m, data.Whs1.FindLocation("A-1"));
				pendingAdjustmentLine.RunPreSaveValidation(); // to commit inventory
															  // At this point we have 2 units in stock and 2 units pending to the location with capacity of 7 units.
				Factory.Save();

				var docketLine1 = GetNewDocketLineForNewNonFinalisedDocket(data);
				var docket = docketLine1.Docket;
				var docketLine2 = GetNewDocketLineForDocket(docket, data.Part1);
				var docketLine3 = GetNewDocketLineForDocket(docket, data.Part1);
				AssertEquals("Precondition: ", 1m, docketLine1.WE_TransactionQuantity);
				AssertEquals("Precondition: ", 1m, docketLine2.WE_TransactionQuantity);
				AssertEquals("Precondition: ", 1m, docketLine3.WE_TransactionQuantity);

				// the idea is that we are trying to add 3 docketlines with 1 unit each, so there should be no capacity overfill
				docket.RunPreSaveValidation(); // to commit inventory for transfer and adjustment lines
				AssertNoExceptionThrown(() => Factory.Save());
				// data should be saved
				Assert(!docketLine1.HasChanges);
				Assert(!docketLine2.HasChanges);
				Assert(!docketLine3.HasChanges);
				data.Whs1.FindLocation("A-1").WLV_MaxQuantity = 10m;
				Factory.Save();

				var newFactory = new BusinessObjectFactory(); // have to use other factory as available capacity quantity is cached on docket and there is no way to clear that cache
				var docketInAnotherFactory = newFactory.Load<WhsDocket>(docket.PK);
				docketInAnotherFactory.Lines[0].WE_TransactionQuantity = 2;
				docketInAnotherFactory.Lines[1].WE_TransactionQuantity = 2;
				docketInAnotherFactory.Lines[2].WE_TransactionQuantity = 2;
				docketInAnotherFactory.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(docketInAnotherFactory);
				AssertEquals("Precondition: ", 2m, docketInAnotherFactory.Lines[0].WE_StockOnHand);
				AssertEquals("Precondition: ", 2m, docketInAnotherFactory.Lines[1].WE_StockOnHand);
				AssertEquals("Precondition: ", 2m, docketInAnotherFactory.Lines[2].WE_StockOnHand);
				// now we are updating 3 docketlines
				AssertNoExceptionThrown(() => newFactory.Save());
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		protected abstract bool DoesCreateNewStock { get; }

		#region TestTG_PreventOverfillLocationWithUnitsCapacity_PutawayTransfer

		public void TestTG_PreventOverfillLocationWithUnitsCapacity_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var normalLocation = data.Whs1.DefaultLocation;
			dockDoorLocation.WLV_MaxQuantity = 10m;
			normalLocation.WLV_MaxQuantity = 10m;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, dockDoorLocation, "PLT-1");
			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.CreateWhsTransferLine(putawayTransfer, data.Part1, 7m, dockDoorLocation.ToLocationString(), "PLT-1", normalLocation.ToLocationString(), "PLT-1");
			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			transferLine.RunPreSaveValidation(); // to commit inventory
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition", DocketLineStatus.Codes.PickedForUnload, receiveLine.WE_DocketLineStatus);
			AssertEquals("Precondition", 7m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, dockDoorLocation, "PLT-2");

			// for DDL
			AssertNoExceptionThrown("We can add 10 more qty to Dock Door location, as first receive was already picked for unload.", Factory.Save);

			inventory2.WI_InDocketLineUnits = 3m;
			AssertNoExceptionThrown("We can add 10 more qty to Dock Door location, as first receive was already picked for unload.", Factory.Save);

			inventory2.WI_InDocketLineUnits = 10m;
			AssertNoExceptionThrown("We can add 10 more qty to Dock Door location, as first receive was already picked for unload.", Factory.Save);

			// for normal location
			inventory2.WI_WL = normalLocation.PK;
			inventory2.WI_InDocketLineUnits = 1m;
			AssertNoExceptionThrown("We can add only 3 more qty to normal location, as putaway tranfers in-route to add 7 qty.", Factory.Save);

			inventory2.WI_InDocketLineUnits = 3m;
			AssertNoExceptionThrown("We can add only 3 more qty to normal location, as putaway tranfers in-route to add 7 qty", Factory.Save);

			inventory2.WI_InDocketLineUnits = 10m;
			AssertExceptionThrown<ZSaveException>("We can add only 3 more qty to normal location, as putaway tranfers in-route to add 7 qty", Factory.Save);
		}

		#endregion

		#endregion

		#region TestUpdatingWE_DocketLineTypeFails

		public void TestUpdatingWE_DocketLineTypeFails()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AdditionalTestDataSetupForDocket(data);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, data.Whs1.FindLocation("A-2"), "");

			var line = GetNewDocketLineForNewNonFinalisedDocket(data);
			line.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var originalDocketLineType = line.WE_DocketLineType;
			line.WE_DocketLineType = new DocketType().Cast<ICodeDescription>().First(c => c.Code != line.WE_DocketLineType).Code;
			AssertNotEquals("Precondition: Docket line must have a different docket type now.", originalDocketLineType, line.WE_DocketLineType);
			AssertEquals("Precondition: Line must have changes.", true, line.HasChanges);
			AssertExceptionThrown<ZSaveException>("Update trigger must fire and prevent updating docket line type.", () => Factory.Save());
		}

		#endregion

		#region TestInsertDocketLineTypeDifferentToDocketType

		public void TestInsertDocketLineTypeDifferentToDocketType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AdditionalTestDataSetupForDocket(data);
			var referenceLine = GetNewDocketLineForNewNonFinalisedDocket(data);
			var referenceDocket = referenceLine.Docket;

			var sql = new StringBuilder();

			var docketDO = new WhsDocketDO(data.Org1.PK.ToGuid(), data.Whs1.PK.ToGuid(), referenceDocket.WD_DocketType, referenceDocket.WD_DocketSubType, "ENT", "D1").AppendInsertAndReturnObject(sql);

			var docketMock = new Mock<CargoWise.Database.TestFramework.ObjectModel.IWhsDocketSQL>();
			docketMock.Setup(d => d.PK).Returns(docketDO.PK);

			var newType = referenceLine.Docket.WD_DocketType != DocketType.Codes.Order // Switch Docket to either Receive or Adjustment for simplicity
				? DocketType.Codes.Order
				: DocketType.Codes.Receive;
			docketMock.Setup(d => d.WD_DocketType).Returns(newType);

			var docketAsDataObjectMock = docketMock.As<CargoWise.Database.TestFramework.ObjectModel.ISQLDataObject>();
			docketAsDataObjectMock.Setup(d => d.PK).Returns(docketDO.PK);

			var lineDO = new WhsDocketLineDO(docketMock.Object, data.Part1.PK.ToGuid(), 0m);

			if (newType == DocketType.Codes.Receive)
			{
				lineDO.WE_OriginalInventoryStatus = "REC";
				lineDO.WE_CurrentInventoryStatus = "REC";
				lineDO.WE_UnloadedTime = DateTimeOffset.Now;
				lineDO.WE_GS_NKUnloadedBy = GlbStaff.CurrentUser.GS_Code;
				lineDO.WE_WL = data.Whs1.FindLocation("A-1").PK.ToGuid();
				lineDO.WE_AdjustmentArrivalDate = DateTime.Now;
			}

			lineDO.AppendInsertAndReturnObject(sql);

			var exception = AssertExceptionThrown<SqlException>("Insert trigger must fire and prevent saving of docket line that does not match with docket type.",
				() => TestConnection.ExecuteNonQuery(sql.ToString()));
			AssertEquals("Attempt to add unmatching docket line type.\r\nThe transaction ended in the trigger. The batch has been aborted.", exception.Message);
		}

		#endregion

		#region TestInsertingLineWithLocationFromAnotherWarehouse

		[ExpectNoExceptions]
		public void TestInsertingLineWithLocationFromAnotherWarehouse()
		{
			if (DoesCreateNewStock)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				AdditionalTestDataSetupForDocket(data);
				var otherWhs = Helper.CreateWarehouse("W2", "A", 1, 1);

				var line1 = GetNewDocketLineForNewNonFinalisedDocket(data);
				var line2 = GetNewDocketLineForDocket(line1.Docket, data.Part1);
				line2.WE_WL = otherWhs.DefaultLocation.PK;

				NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Locations should be in the warehouse specified on the parent Docket."), "Trigger should prevent saving a location for a warehouse different to the warehouse on the docket.");
			}
			else
			{
				Assert(true); // Can't have location
			}
		}

		#endregion

		#region TestUpdatingLineWithLocationFromAnotherWarehouse

		[ExpectNoExceptions]
		public void TestUpdatingLineWithLocationFromAnotherWarehouse()
		{
			if (DoesCreateNewStock)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				AdditionalTestDataSetupForDocket(data);
				var otherWhs = Helper.CreateWarehouse("W2", "A", 1, 1);
				var line = GetNewDocketLineForNewNonFinalisedDocket(data);
				Factory.Save();

				line.WE_WL = otherWhs.DefaultLocation.PK;
				NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Locations should be in the warehouse specified on the parent Docket."), "Trigger should prevent saving a location for a warehouse different to the warehouse on the docket.");
			}
			else
			{
				Assert(true); // Can't have location
			}
		}

		#endregion

		// this helper methods are a disaster, need to be removed and most of the test to be moved to Database solution. If you reading this feel free to do it.
		#region Helper methods

		void UpdateStatusAndDateCommand_WithoutCheckingSOH(string status, ZDateTimeOffset? finaliseDate, Guid pk)
		{
			// We sometimes expect this method to pass and sometimes to throw exception.
			// When it throws exception and we try to enable trigger back or do check procedure in dispose of using, then original exception will be overriden with:
			// "Transaction has been rolled back in the server" therefore we have to suspend it and not re-enable.
			WhsTestHelperFunctions.SuspendDatabaseProcForTest($"{WhsValidationHelper.WhsCheckStockOnHandIsBalanced} (@InventoryPKsToCheck dbo.TVP_uniqueidentifier READONLY)", "0");

			var command = GetUpdateStatusAndDateCommandCore(status, finaliseDate, pk);
			command.ExecuteNonQuery();
		}

		protected virtual IDbCommand GetUpdateStatusAndDateCommandCore(string status, ZDateTimeOffset? finaliseDate, Guid pk)
		{
			var updateStatusSql = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine
					.UpdateWhere(l => l.PK == pk)
					.Set(l => l.WE_DocketLineStatus, status)
					.Set(l => l.WE_FinalisedDate, finaliseDate?.ToDateTimeOffset()).AsSQL();
			var updateStatusCommand = TestConnection.Command(updateStatusSql);
			return updateStatusCommand;
		}

		#endregion

		#region GetNewDocketLine

		protected abstract WhsDocketLine GetNewDocketLineForNewNonFinalisedDocket(TestDataSimpleEnvironment data);

		protected abstract WhsDocketLine GetNewDocketLineForNewFinalisedDocket(WhsWarehouse whs, OrgHeader client, OrgSupplierPart product, ZDecimal qty);

		protected abstract WhsDocketLine GetNewDocketLineForDocket(WhsDocket docket, OrgSupplierPart product);

		protected virtual void AdditionalTestDataSetupForDocket(TestDataSimpleEnvironment data) { }

		#endregion

		protected virtual bool CanBeCancelled => true;
	}
}
