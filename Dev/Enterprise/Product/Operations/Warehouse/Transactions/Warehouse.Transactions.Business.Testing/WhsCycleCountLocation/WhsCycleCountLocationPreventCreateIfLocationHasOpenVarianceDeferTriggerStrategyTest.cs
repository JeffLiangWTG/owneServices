using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategyTest : WhsTestCaseWithFactory
	{
		public void TestRunType()
		{
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(
				nameof(IWhsCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy));
			AssertEquals(strategy.RunType, TriggerRunType.InsertOrUpdate);
		}

		public void TestShouldDeferTriggerWhenRejectAndRecreate()
		{
			var now = DateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(
				nameof(IWhsCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy));
			var originalCycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.FindLocation("A-1"),
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "WWW");
			var variance = Helper.CreateWhsCycleCountLocationVariance(originalCycleCount,
				CycleCountVarianceStatus.Codes.Open, palletID: "PLT1", client: data.Org1, part: data.Part1,
				varianceQty: 2);
			AssertEquals("Trigger should be deferred.", true, strategy.ShouldDeferTrigger(originalCycleCount));
			Factory.Save();
			variance.WCC_Status = CycleCountVarianceStatus.Codes.Rejected;
			var recreatedCycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.FindLocation("A-1"),
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "WWW");
			AssertEquals("Trigger should be deferred.", false, strategy.ShouldDeferTrigger(originalCycleCount));
			AssertEquals("Trigger should be deferred.", true, strategy.ShouldDeferTrigger(recreatedCycleCount));
		}
	}
}
