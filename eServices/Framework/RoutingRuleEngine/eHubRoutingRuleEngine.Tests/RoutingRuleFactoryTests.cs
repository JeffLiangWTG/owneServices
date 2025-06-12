using System;
using System.Linq;
using System.Threading.Tasks;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;
using Rhino.Mocks;

namespace eServices.eHubRoutingRuleEngine.Tests
{	
	[TestFixture]
	[Parallelizable(ParallelScope.Children)]
	public class RoutingRuleFactoryTests
	{
		private static eHubTransactionsContext mockContext;
		private static TestDbSet<eHubClient> testEHubClients;
		private ConsoleOutLogger logger;
		private RoutingRuleFactory ruleFactory;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var utcNow = DateTime.UtcNow;
			testEHubClients = new TestDbSet<eHubClient>
			{
				new eHubClient
				{
					CC_ID = "RULE_CACHED",
					CC_RR = Guid.NewGuid(),
					eHubRoutingRule = new eHubRoutingRule { RR_LastUpdateUTC = utcNow }
				},
				new eHubClient
				{
					CC_ID = "RULE_REFRESHED",
					CC_RR = Guid.NewGuid(),
					eHubRoutingRule = new eHubRoutingRule { RR_LastUpdateUTC = new DateTime(2020,1,1) }
				}
			};
			mockContext.Stub(x => x.eHubClients).Return(testEHubClients);
			mockContext.Stub(x => x.SqlQuery<DateTime>(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything)).Return(new[] { utcNow });
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
        {
			mockContext?.Dispose();
        }

        [SetUp]
		public void SetUp()
		{
			logger = new ConsoleOutLogger(nameof(RoutingRuleFactoryTests), LogLevel.All, true, false, false, null);
			ruleFactory = new RoutingRuleFactory(mockContext, logger);
		}

		[Test]
		public void GetForReading_Get_NoRule()
		{
			// Cache entry added from check of client ID with no rule
			var rule = ruleFactory.GetForReading("NORULE", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1));
			RoutingRuleFactory.ruleCache.TryGetValue("NORULE", out var cachedFirst);
			Assert.That(rule, Is.Null);
			Assert.That(cachedFirst.Rule, Is.Null);
			Assert.That(cachedFirst.Expiry, Is.Not.EqualTo(default(DateTime)));

			// Cache hit for check of client ID with no rule
			rule = ruleFactory.GetForReading("NORULE", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1));
			RoutingRuleFactory.ruleCache.TryGetValue("NORULE", out var cachedSecond);
			Assert.That(rule, Is.Null);
			Assert.That(cachedSecond.Expiry, Is.EqualTo(cachedFirst.Expiry));

			// Wait for cache expiry so new cache entry added from check of client ID with no rule
			Task.Delay(TimeSpan.FromSeconds(1)).Wait();
			rule = ruleFactory.GetForReading("NORULE", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1));
			RoutingRuleFactory.ruleCache.TryGetValue("NORULE", out var cachedThird);
			Assert.That(rule, Is.Null);
			Assert.That(cachedThird.Expiry, Is.GreaterThan(cachedFirst.Expiry));
		}

		[Test]
		public void GetForReading_Get_RuleCached()
		{
			// Cache entries added from check of client ID with rule
			var rule = ruleFactory.GetForReading("RULE_CACHED", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
			RoutingRuleFactory.ruleCache.TryGetValue("RULE_CACHED", out var cachedFirst);
			Assert.That(rule, Is.Not.Null.And.SameAs(cachedFirst.Rule));
			Assert.That(cachedFirst.Expiry, Is.Not.EqualTo(default(DateTime)));

			// Cache hit for check of client ID with rule
			rule = ruleFactory.GetForReading("RULE_CACHED", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
			RoutingRuleFactory.ruleCache.TryGetValue("RULE_CACHED", out var cachedSecond);
			Assert.That(rule, Is.Not.Null.And.SameAs(cachedFirst.Rule).And.SameAs(cachedSecond.Rule));
			Assert.That(cachedSecond.Expiry, Is.EqualTo(cachedFirst.Expiry));

			// Wait for cache expiry for check of client ID with rule but rule not changed so cached rule returned
			Task.Delay(TimeSpan.FromSeconds(1)).Wait();
			rule = ruleFactory.GetForReading("RULE_CACHED", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
			RoutingRuleFactory.ruleCache.TryGetValue("RULE_CACHED", out var cachedThird);
			Assert.That(rule, Is.Not.Null.And.SameAs(cachedFirst.Rule));
			Assert.That(cachedThird.Expiry, Is.GreaterThan(cachedFirst.Expiry));
		}

		[Test]
		public void GetForReading_Get_RuleRefreshed()
		{
			// Cache entries added from check of client ID with rule
			var rule = ruleFactory.GetForReading("RULE_REFRESHED", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
			RoutingRuleFactory.ruleCache.TryGetValue("RULE_REFRESHED", out var cachedFirst);
			Assert.That(rule, Is.Not.Null.And.SameAs(cachedFirst.Rule));
			Assert.That(cachedFirst.Expiry, Is.Not.EqualTo(default(DateTime)));

			// Update the rule and wait for cache expiry for check of client ID
			mockContext.eHubClients.First(c => c.CC_ID == "RULE_REFRESHED").eHubRoutingRule.RR_LastUpdateUTC += TimeSpan.FromHours(1);
			Task.Delay(TimeSpan.FromSeconds(1)).Wait();

			// Multiple requests for updated rule should refresh rule on only one thread. Second thread should wait then return the already refreshed rule.
			IRule ruleCheckTask() => ruleFactory.GetForReading("RULE_REFRESHED", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
			var tasks = new[] { Task.Run(() => ruleCheckTask()), Task.Run(() => ruleCheckTask()) };
			Task.WaitAll(tasks);

			RoutingRuleFactory.ruleCache.TryGetValue("RULE_REFRESHED", out var cachedSecond);
			Assert.That(tasks[0].Result, Is.Not.Null);
			Assert.That(tasks[0].Result, Is.SameAs(tasks[1].Result));
			Assert.That(tasks[0].Result, Is.SameAs(cachedSecond.Rule));
			Assert.That(tasks[0].Result, Is.Not.SameAs(cachedFirst.Rule));
			Assert.That(cachedSecond.Expiry, Is.GreaterThan(cachedFirst.Expiry));
		}
	}
}
