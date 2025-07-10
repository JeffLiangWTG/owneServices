using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class WhsItemCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategyTest : TestCaseWithFactory
	{
		public void TestRunType()
		{
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IWhsItemCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy));
			AssertEquals(strategy.RunType, TriggerRunType.InsertOrUpdate);
		}

		public void TestShouldDeferTriggerWhenRejectAndRecreate()
		{
			var now = DateTimeOffset.Now;
			var data = new TransitTestDataSimpleEnvironment(Factory, 2, 1);
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IWhsItemCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy));
			var originalCycleCount = Helper.CreateCycleCountLocation(data.Whs1.FindLocation("A-1"), CycleCountLocationStatuses.Codes.Completed, now, now.AddHours(1), now.AddHours(1), "WWW");
			var variance = Helper.CreateCycleCountLocationVariance(originalCycleCount, packageNotInWhsID: "P1");
			AssertEquals("Trigger should be deferred.", true, strategy.ShouldDeferTrigger(originalCycleCount));
			Factory.Save();

			variance.WIV_Status = CycleCountVarianceStatuses.Codes.Rejected;
			var recreatedCycleCount = Helper.CreateCycleCountLocation(data.Whs1.FindLocation("A-1"), CycleCountLocationStatuses.Codes.Completed, now, now.AddHours(1), now.AddHours(1), "WWW");
			AssertEquals("Trigger should not be deferred.", false, strategy.ShouldDeferTrigger(originalCycleCount));
			AssertEquals("Trigger should be deferred.", true, strategy.ShouldDeferTrigger(recreatedCycleCount));
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
