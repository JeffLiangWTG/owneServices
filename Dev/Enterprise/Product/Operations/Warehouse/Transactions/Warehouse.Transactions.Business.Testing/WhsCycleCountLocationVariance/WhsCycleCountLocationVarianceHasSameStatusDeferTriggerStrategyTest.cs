using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsCycleCountLocationVarianceHasSameStatusDeferTriggerStrategyTest : WhsTestCaseWithFactory
	{
		public void TestRunType()
		{
			var strategy =
				ObjectFactory.Get<IDeferTriggerConditionStrategy>(
					nameof(IWhsCycleCountLocationVarianceHasSameStatusDeferTriggerStrategy));
			AssertEquals(strategy.RunType, TriggerRunType.InsertOrUpdate);
		}

		public void TestShouldDeferTriggerWhenInsert()
		{
			var now = DateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.FindLocation("A-1"),
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "WWW");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 2);
			var strategy =
				ObjectFactory.Get<IDeferTriggerConditionStrategy>(
					nameof(IWhsCycleCountLocationVarianceHasSameStatusDeferTriggerStrategy));
			AssertEquals("Trigger shouled be deferred.", true, strategy.ShouldDeferTrigger(variance));
		}

		public void TestShouldDeferTriggerWhenUpdateWithChanges()
		{
			var now = DateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.FindLocation("A-1"),
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "WWW");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 2);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT2", client: data.Org1, part: data.Part1, varianceQty: 2);
			var strategy =
				ObjectFactory.Get<IDeferTriggerConditionStrategy>(
					nameof(IWhsCycleCountLocationVarianceHasSameStatusDeferTriggerStrategy));
			AssertEquals("Trigger shouled be deferred.", true, strategy.ShouldDeferTrigger(variance1));
			AssertEquals("Trigger shouled be deferred.", true, strategy.ShouldDeferTrigger(variance2));
			Factory.Save();

			variance1.WCC_Status = CycleCountVarianceStatus.Codes.Approved;
			variance2.WCC_Status = CycleCountVarianceStatus.Codes.Rejected;
			AssertEquals("Trigger shouled be deferred.", true, strategy.ShouldDeferTrigger(variance1));
			AssertEquals("Trigger shouled be deferred.", true, strategy.ShouldDeferTrigger(variance2));
		}

		public void TestShouldDeferTriggerWhenUpdateWithoutChanges()
		{
			var now = DateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.FindLocation("A-1"),
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "WWW");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 2);
			var strategy =
				ObjectFactory.Get<IDeferTriggerConditionStrategy>(
					nameof(IWhsCycleCountLocationVarianceHasSameStatusDeferTriggerStrategy));
			AssertEquals("Trigger shouled be deferred.", true, strategy.ShouldDeferTrigger(variance));
			Factory.Save();

			variance.WCC_Status = CycleCountVarianceStatus.Codes.Open;
			AssertEquals("Trigger shouled be deferred.", false, strategy.ShouldDeferTrigger(variance));
		}
	}
}
