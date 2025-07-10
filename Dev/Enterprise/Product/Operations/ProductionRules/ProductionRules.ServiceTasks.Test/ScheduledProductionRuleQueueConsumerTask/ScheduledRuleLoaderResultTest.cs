using System;
using System.Linq;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Integration;
using Moq;

namespace Enterprise.ProductionRules.ServiceTasks.Testing
{
	class ScheduledRuleLoaderResultTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var ruleSet = Factory.New<ProductionRuleSet>();
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet, null);
			var rule = Factory.New<ProductionRule>();
			rule.PRL_Name = "ABC";
			var rules = new[] { rule };
			var queues = new[] { Factory.New<ProductionRuleScheduleQueue>() };
			var rulesProcessor = Mock.Of<IScheduledRuleProcessor>();

			AssertExceptionThrown<ArgumentNullException>(() => new ScheduledRuleLoaderResult(null, rules, queues, rulesProcessor));
			AssertExceptionThrown<ArgumentNullException>(() => new ScheduledRuleLoaderResult(ruleSetWithLock, null, queues, rulesProcessor));
			AssertExceptionThrown<ArgumentNullException>(() => new ScheduledRuleLoaderResult(ruleSetWithLock, rules, null, rulesProcessor));
			AssertExceptionThrown<ArgumentNullException>(() => new ScheduledRuleLoaderResult(ruleSetWithLock, rules, queues, null));

			var result = new ScheduledRuleLoaderResult(ruleSetWithLock, rules, queues, rulesProcessor);
			AssertEquals(nameof(ScheduledRuleLoaderResult.RuleSetWithLock), ruleSetWithLock, result.RuleSetWithLock);
			AssertContainsExactElementsInAnyOrder(nameof(ScheduledRuleLoaderResult.Rules), new[] { "ABC" }, result.Rules.Select(r => r.Name));
			AssertEquals(nameof(ScheduledRuleLoaderResult.QueueEntries), queues, result.QueueEntries);
			AssertEquals(nameof(ScheduledRuleLoaderResult.RuleProcessor), rulesProcessor, result.RuleProcessor);
		}

		public void TestDispose()
		{
			var sqlLock = new Mock<ISqlApplicationLock>();

			var ruleSet = Factory.New<ProductionRuleSet>();
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet, sqlLock.Object);
			var rules = new[] { Factory.New<ProductionRule>() };
			var queues = new[] { Factory.New<ProductionRuleScheduleQueue>() };
			var rulesProcessor = Mock.Of<IScheduledRuleProcessor>();

			using (var result = new ScheduledRuleLoaderResult(ruleSetWithLock, rules, queues, rulesProcessor))
			{
				sqlLock.Verify(l => l.Dispose(), Times.Never);
			}

			sqlLock.Verify(l => l.Dispose(), Times.Once);
			Assert(true);
		}

		public void TestDispose_NullLock()
		{
			var ruleSet = Factory.New<ProductionRuleSet>();
			var ruleSetWithLock = new AppLockedItem<ProductionRuleSet>(ruleSet, null);
			var rules = new[] { Factory.New<ProductionRule>() };
			var queues = new[] { Factory.New<ProductionRuleScheduleQueue>() };
			var rulesProcessor = Mock.Of<IScheduledRuleProcessor>();

			var result = new ScheduledRuleLoaderResult(ruleSetWithLock, rules, queues, rulesProcessor);
			AssertNoExceptionThrown(() => ((IDisposable)result).Dispose());
		}
	}
}
