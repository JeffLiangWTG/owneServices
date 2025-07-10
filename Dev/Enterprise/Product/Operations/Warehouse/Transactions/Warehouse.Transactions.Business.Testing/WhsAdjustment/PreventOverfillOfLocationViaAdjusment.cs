using CargoWise.Data;
using CargoWise.EntityFramework;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PreventOverfillOfLocationViaAdjusment : WhsTestCaseWithFactory
	{
		#region TestSubscribeToAfterSaveInTransactionForOverFillLocationWithUnitsCapacityCheck

		[ExpectNoExceptions]
		public void TestSubscribeToAfterSaveInTransactionForOverFillLocationWithUnitsCapacityCheck()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var loc1 = data.Whs1.FindLocation("A");
			loc1.WLV_MaxQuantity = 100;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, loc1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 40m, loc1, "");
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			using (adjustment.GetValidationSuspender())
			{
				var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, loc1);
				var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 30, loc1);
				adjustment.RunPreSaveValidation(); // to commit inventory and create pickline

				AssertAdjustmentCausesOverfillTrigger(adjustment);
			}
		}

		[ExpectNoExceptions]
		public void TestSubscribeToAfterSaveInTransactionForOverFillLocationWithUnitsCapacityCheck_AdjustmentWithTwoLocations_OneOverflows()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var loc1 = data.Whs1.FindLocation("A-1-1");
			loc1.WLV_MaxQuantity = 100;
			var loc2 = data.Whs1.FindLocation("A-2-1");
			loc2.WLV_MaxQuantity = 100;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, loc1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 40m, loc1, "");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, loc2, "");
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 40m, loc2, "");
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);
			AssertIsFinalisedPrecondition(receive3);
			AssertIsFinalisedPrecondition(receive4);

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			using (adjustment.GetValidationSuspender())
			{
				var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, loc1);
				var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 10, loc1);
				var adjustmentLine3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, loc2);
				var adjustmentLine4 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 30, loc2);
				adjustment.RunPreSaveValidation(); // to commit inventory and create pickline

				AssertAdjustmentCausesOverfillTrigger(adjustment);
			}
		}

		[ExpectNoExceptions]
		public void TestSubscribeToAfterSaveInTransactionForOverFillLocationWithUnitsCapacityCheck_AdjustmentWithTwoLocations_BothOverflow()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var loc1 = data.Whs1.FindLocation("A-1-1");
			loc1.WLV_MaxQuantity = 100;
			var loc2 = data.Whs1.FindLocation("A-2-1");
			loc2.WLV_MaxQuantity = 100;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, loc1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 40m, loc1, "");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 50m, loc2, "");
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 40m, loc2, "");
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);
			AssertIsFinalisedPrecondition(receive3);
			AssertIsFinalisedPrecondition(receive4);

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			using (adjustment.GetValidationSuspender())
			{
				var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, loc1);
				var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 30, loc1);
				var adjustmentLine3 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, loc2);
				var adjustmentLine4 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 30, loc2);
				adjustment.RunPreSaveValidation(); // to commit inventory and create pickline

				AssertAdjustmentCausesOverfillTrigger(adjustment);
			}
		}

		void AssertAdjustmentCausesOverfillTrigger(WhsAdjustment adjustment)
		{
			Factory.Save();

			adjustment.FinaliseDocket();
			AssertIsFinalisedPrecondition(adjustment);

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WhsDocketLine.PreventOverflowLocationQuantityTriggerID}"), "Adjustment should cause location to overflow.");
		}

		public void TestSubscribeToAfterSaveInTransactionForOverFillLocationWithUnitsCapacityCheck_NoOverflow()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var loc1 = data.Whs1.FindLocation("A");
			loc1.WLV_MaxQuantity = 100;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, loc1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 40m, loc1, "");
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			using (adjustment.GetValidationSuspender())
			{
				var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -20, loc1);
				var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 30, loc1);
				adjustment.RunPreSaveValidation(); // to commit inventory and create pickline
				Factory.Save();

				adjustment.FinaliseDocket();
				AssertIsFinalisedPrecondition(adjustment);
				AssertNoExceptionThrown("Adjustment should not cause location to overflow.", Factory.Save);
			}
		}

		[ExpectNoExceptions]
		public void TestSubscribeToAfterSaveInTransactionForOverFillLocationWithUnitsCapacityCheck_SuspendedTriggerReEnabledWhenOverfill()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Factory.Save();

			var loc1 = data.Whs1.FindLocation("A");
			loc1.WLV_MaxQuantity = 100;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, loc1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 40m, loc1, "");
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			using (adjustment.GetValidationSuspender())
			{
				var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, loc1);
				var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 30, loc1);
				adjustment.RunPreSaveValidation(); // to commit inventory and create pickline

				AssertAdjustmentCausesOverfillTrigger(adjustment);

				// Trigger should be re-enabled
				var connection = (IDbConnected)Factory;
				var disabledFlag = (int)connection.Connection.ExecuteScalar("SELECT Result FROM dbo.IsTriggerSuspended('TG_PreventOverfillLocationWithUnitsCapacity')");
				AssertEquals("Trigger should not be disabled.", 0, disabledFlag);
			}
		}

		public void TestSubscribeToAfterSaveInTransactionForOverFillLocationWithUnitsCapacityCheck_SuspendedTriggerReEnabledWhenNoOverfill()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Factory.Save();

			var loc1 = data.Whs1.FindLocation("A");
			loc1.WLV_MaxQuantity = 100;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, loc1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 40m, loc1, "");

			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "A1");
			using (adjustment.GetValidationSuspender())
			{
				var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -20, loc1);
				var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 30, loc1);
				adjustment.RunPreSaveValidation(); // to commit inventory and create pickline
				Factory.Save();

				adjustment.FinaliseDocket();
				AssertIsFinalisedPrecondition(adjustment);
				AssertNoExceptionThrown("Adjustment should not cause location to overflow.", Factory.Save);

				// Trigger should be re-enabled
				var connection = (IDbConnected)Factory;
				var disabledFlag = (int)connection.Connection.ExecuteScalar("SELECT Result FROM dbo.IsTriggerSuspended('TG_PreventOverfillLocationWithUnitsCapacity')");
				AssertEquals("Trigger should not be disabled.", 0, disabledFlag);
			}
		}

		#endregion
	}
}
