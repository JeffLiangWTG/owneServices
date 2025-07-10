using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustmentLine))]
	class DeferrableTriggers_WhsAdjustmentLineTest : DeferrableTriggerTestCase<WhsAdjustmentLine>
	{
		public void TestShouldDeferTrigger()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var line = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m).Lines[0];
			Factory.Save();

			var adjustment = helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentIn = helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, line.WE_WL);
			var adjustmentOut = helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, line.WE_WL);
			adjustment.RunPreSaveValidation();

			var strategy = (IDeferTriggerConditionStrategy)ObjectFactory.Get<IWhsCheckTransactionAndPickQtyIsCorrectOnUpdateWE_TransactionQuantityWhsAdjustmentLine_DeferTriggerStrategy>();
			AssertEquals("Adjustment Line is not in the DB, Should not defer trigger.", false, strategy.ShouldDeferTrigger(adjustmentIn));
			AssertEquals("Adjustment Line is not in the DB, Should not defer trigger.", false, strategy.ShouldDeferTrigger(adjustmentOut));

			Factory.Save();
			adjustmentIn.WE_TransactionQuantity = 11;
			adjustmentOut.WE_TransactionQuantity = -11;
			AssertEquals("Adjustment In should *not* defer trigger.", false, strategy.ShouldDeferTrigger(adjustmentIn));
			AssertEquals("Adjustment Out should defer trigger.", true, strategy.ShouldDeferTrigger(adjustmentOut));
		}
	}
}
