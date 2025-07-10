using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Business.Testing;
using Enterprise.ProductionRules.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.ProductionRules.ServiceTasks.Testing
{
	[UseSnapshotProtection]
	class ScheduledRuleLoaderTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ScheduledRuleLoader(null));
		}

		public void TestObjectFactoryConfiguration()
		{
			AssertType<ScheduledRuleLoader>(ObjectFactory.Get<IScheduledRuleLoader>());
		}

		public void TestGetNextRuleSetToProcess_NullFactory_Throws()
		{
			var scheduledRuleLoader = new ScheduledRuleLoader(Mock.Of<IScheduledRuleProcessorFactory>());
			AssertExceptionThrown<ArgumentNullException>(() => scheduledRuleLoader.GetNextRuleSetToProcess(null));
		}

		public void TestGetNextRuleSetToProcess_NoRules() => TestGetNextRuleSetToProcess_NoRules(scheduled: true);
		public void TestGetNextRuleSetToProcess_NoRules_NonScheduled() => TestGetNextRuleSetToProcess_NoRules(scheduled: false);

		void TestGetNextRuleSetToProcess_NoRules(bool scheduled)
		{
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", isLive: false);
			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");

			if (scheduled)
			{
				CreateScheduledRuleTask(rule1_1);
			}

			Factory.Save();

			var scheduledRuleLoader = new ScheduledRuleLoader(Mock.Of<IScheduledRuleProcessorFactory>());
			var result = scheduledRuleLoader.GetNextRuleSetToProcess(Factory);
			AssertNull("Should have returned no result.", result);
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess()
		{
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", isLive: false);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", isLive: false);

			var rule1_1 = Helper.CreateRule(ruleSet1, "11", "11");
			var rule1_2 = Helper.CreateRule(ruleSet1, "12", "12");
			var rule2_1 = Helper.CreateRule(ruleSet2, "21", "21");

			CreateScheduledRuleTask(rule1_1);
			CreateScheduledRuleTask(rule1_2);
			CreateScheduledRuleTask(rule2_1);

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var factoryMock = CreateProcessorFactoryMock();
			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			using (var result = scheduledRuleLoader.GetNextRuleSetToProcess(Factory))
			{
				AssertNotNull("Should have returned a result.", result);
				AssertEquals("Should have returned rule set 1.", ruleSet1, result.RuleSetWithLock.Item);
				AssertContainsExactElementsInAnyOrder("Should have returned the first rule wrapper.", new[] { "11" }, result.Rules.Select(rule => rule.Name));
				AssertContainsExactElementsInAnyOrder("Should have returned the first queued rule.", new[] { queue1 }, result.QueueEntries);

				var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
				var queue1_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue1.PK);
				var queue2_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue2.PK);
				AssertEquals("Should have bumped queue 1 retry count.", (byte)1, queue1_InFactory2.PRQ_RetryCount);
				AssertEquals("Should *not* have bumped queue 2 retry count.", (byte)0, queue2_InFactory2.PRQ_RetryCount);
			}
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_NullRuleProcessor()
		{
			var ruleSet = Helper.CreateRuleSet("TEST1", "TEST1", isLive: false);
			var rule = Helper.CreateRule(ruleSet, "11", "11");
			CreateScheduledRuleTask(rule);

			var queue = Helper.CreateScheduledRuleQueue(rule, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var scheduledRuleLoader = new ScheduledRuleLoader(Mock.Of<IScheduledRuleProcessorFactory>());
			AssertExceptionThrown<InvalidOperationException>(
				"Null Rule Processor should throw.",
				"Could not load rule processor for Context: InventoryPutaway, Code: PWP.",
				() => _ = scheduledRuleLoader.GetNextRuleSetToProcess(Factory));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var queue_InNewFactory = newFactory.Load<ProductionRuleScheduleQueue>(queue.PK);
			AssertNotNull("Should *not* have deleted queue.", queue_InNewFactory);
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_SavedOutOfOrder()
		{
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", isLive: false);
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", isLive: false);

			var rule2_1 = Helper.CreateRule(ruleSet2, "21", "21");
			var rule1_1 = Helper.CreateRule(ruleSet1, "11", "11");
			var rule1_2 = Helper.CreateRule(ruleSet1, "12", "12");

			CreateScheduledRuleTask(rule2_1);
			CreateScheduledRuleTask(rule1_1);
			CreateScheduledRuleTask(rule1_2);

			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-1));
			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var factoryMock = CreateProcessorFactoryMock();
			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			using (var result = scheduledRuleLoader.GetNextRuleSetToProcess(Factory))
			{
				AssertNotNull("Should have returned a result.", result);
				AssertEquals("Should have returned rule set 1.", ruleSet1, result.RuleSetWithLock.Item);
				AssertContainsExactElementsInAnyOrder("Should have returned the first rule wrapper.", new[] { "11" }, result.Rules.Select(rule => rule.Name));
				AssertContainsExactElementsInAnyOrder("Should have returned the first queued rule.", new[] { queue1 }, result.QueueEntries);

				var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
				var queue1_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue1.PK);
				var queue2_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue2.PK);
				AssertEquals("Should have bumped queue 1 retry count.", (byte)1, queue1_InFactory2.PRQ_RetryCount);
				AssertEquals("Should *not* have bumped queue 2 retry count.", (byte)0, queue2_InFactory2.PRQ_RetryCount);
			}
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_SavedOutOfOrder_Retries()
		{
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", isLive: false);
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", isLive: false);

			var rule2_1 = Helper.CreateRule(ruleSet2, "21", "21");
			var rule1_1 = Helper.CreateRule(ruleSet1, "11", "11");
			var rule1_2 = Helper.CreateRule(ruleSet1, "12", "12");

			CreateScheduledRuleTask(rule2_1);
			CreateScheduledRuleTask(rule1_1);
			CreateScheduledRuleTask(rule1_2);

			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-2));
			queue2.PRQ_RetryCount = 2;

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var factoryMock = CreateProcessorFactoryMock();
			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			using (var result = scheduledRuleLoader.GetNextRuleSetToProcess(Factory))
			{
				AssertNotNull("Should have returned a result.", result);
				AssertEquals("Should have returned rule set 1.", ruleSet1, result.RuleSetWithLock.Item);
				AssertContainsExactElementsInAnyOrder("Should have returned the first rule wrapper.", new[] { "11" }, result.Rules.Select(rule => rule.Name));
				AssertContainsExactElementsInAnyOrder("Should have returned the first queued rule.", new[] { queue1 }, result.QueueEntries);

				var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
				var queue1_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue1.PK);
				var queue2_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue2.PK);
				AssertEquals("Should have bumped queue 1 retry count.", (byte)1, queue1_InFactory2.PRQ_RetryCount);
				AssertEquals("Should *not* have bumped queue 2 retry count.", (byte)2, queue2_InFactory2.PRQ_RetryCount);
			}
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_MultipleQueuedRules()
		{
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", isLive: false);

			var rule1_1 = Helper.CreateRule(ruleSet1, "11", "11");
			var rule1_2 = Helper.CreateRule(ruleSet1, "12", "12");
			var rule1_3 = Helper.CreateRule(ruleSet1, "13", "13"); // Scheduled, not queued
			var rule1_4 = Helper.CreateRule(ruleSet1, "14", "14"); // Non scheduled

			CreateScheduledRuleTask(rule1_1);
			CreateScheduledRuleTask(rule1_2);
			CreateScheduledRuleTask(rule1_3);

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			var queue2 = Helper.CreateScheduledRuleQueue(rule1_2, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var factoryMock = CreateProcessorFactoryMock();
			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			using (var result = scheduledRuleLoader.GetNextRuleSetToProcess(Factory))
			{
				AssertNotNull("Should have returned a result.", result);
				AssertEquals("Should have returned rule set 1.", ruleSet1, result.RuleSetWithLock.Item);
				AssertContainsExactElementsInAnyOrder("Should have returned the rule wrappers.", new[] { "11", "12", "14" }, result.Rules.Select(rule => rule.Name));
				AssertContainsExactElementsInAnyOrder("Should have returned the queued rules.", new[] { queue1, queue2 }, result.QueueEntries);

				var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
				var queue1_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue1.PK);
				var queue2_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue2.PK);
				AssertEquals("Should have bumped queue 1 retry count.", (byte)1, queue1_InFactory2.PRQ_RetryCount);
				AssertEquals("Should have bumped queue 2 retry count.", (byte)1, queue2_InFactory2.PRQ_RetryCount);
			}
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_ReadsPast()
		{
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", isLive: false);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", isLive: false);

			var rule1_1 = Helper.CreateRule(ruleSet1, "11", "11");
			var rule1_2 = Helper.CreateRule(ruleSet1, "12", "12");
			var rule2_1 = Helper.CreateRule(ruleSet2, "21", "21");

			CreateScheduledRuleTask(rule1_1);
			CreateScheduledRuleTask(rule1_2);
			CreateScheduledRuleTask(rule2_1);

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var factoryMock = CreateProcessorFactoryMock();
			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			using (var result1 = scheduledRuleLoader.GetNextRuleSetToProcess(Factory))
			{
				AssertNotNull("Should have returned a result.", result1);
				AssertEquals("Should have returned rule set 1.", ruleSet1, result1.RuleSetWithLock.Item);
				AssertContainsExactElementsInAnyOrder("Should have returned the first rule wrapper.", new[] { "11" }, result1.Rules.Select(rule => rule.Name));
				AssertContainsExactElementsInAnyOrder("Should have returned the first queued rule.", new[] { queue1 }, result1.QueueEntries);

				using (var secondConnection = Db.NewExtraConnectionToMainDb())
				using (var result2 = scheduledRuleLoader.GetNextRuleSetToProcess(new BusinessObjectFactory(secondConnection) { RefreshEnabled = false }))
				{
					AssertNotNull("Should have returned a result.", result2);
					AssertEquals("Should have returned rule set 2.", ruleSet2.PK, result2.RuleSetWithLock.Item.PK);
					AssertContainsExactElementsInAnyOrder("Should have returned the second ruleset's rule.", new[] { "21" }, result2.Rules.Select(r => r.Name));
					AssertContainsExactElementsInAnyOrder("Should have returned the second ruleset's queued rule.", new[] { queue2.PK }, result2.QueueEntries.Select(q => q.PK));
				}
			}

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var queue1_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue1.PK);
			var queue2_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue2.PK);
			AssertEquals("Should have bumped queue 1 retry count.", (byte)1, queue1_InFactory2.PRQ_RetryCount);
			AssertEquals("Should have bumped queue 2 retry count.", (byte)1, queue2_InFactory2.PRQ_RetryCount);
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_ReadsPast_SecondRuleBecomesAvailable()
		{
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", isLive: false);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", isLive: false);

			var rule1_1 = Helper.CreateRule(ruleSet1, "11", "11");
			var rule1_2 = Helper.CreateRule(ruleSet1, "12", "12");
			var rule2_1 = Helper.CreateRule(ruleSet2, "21", "21");

			CreateScheduledRuleTask(rule1_1);
			CreateScheduledRuleTask(rule1_2);
			CreateScheduledRuleTask(rule2_1);

			ProductionRuleScheduleQueue queue1_2;
			var queue1_1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			var queue2 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var factoryMock = CreateProcessorFactoryMock();
			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			using (var result1 = scheduledRuleLoader.GetNextRuleSetToProcess(Factory))
			{
				AssertNotNull("Should have returned a result.", result1);
				AssertEquals("Should have returned rule set 1.", ruleSet1, result1.RuleSetWithLock.Item);
				AssertContainsExactElementsInAnyOrder("Should have returned the first rule wrapper.", new[] { "11" }, result1.Rules.Select(rule => rule.Name));
				AssertContainsExactElementsInAnyOrder("Should have returned the first queued rule.", new[] { queue1_1 }, result1.QueueEntries);

				queue1_2 = Helper.CreateScheduledRuleQueue(rule1_2, ZDateTime.Today.AddDays(-2));
				Factory.Save();

				using (var secondConnection = Db.NewExtraConnectionToMainDb())
				using (var result2 = scheduledRuleLoader.GetNextRuleSetToProcess(new BusinessObjectFactory(secondConnection) { RefreshEnabled = false }))
				{
					AssertNotNull("Should have returned a result.", result2);
					AssertEquals("Should have returned rule set 2.", ruleSet2.PK, result2.RuleSetWithLock.Item.PK);
					AssertContainsExactElementsInAnyOrder("Should have returned the second ruleset's rule.", new[] { "21" }, result2.Rules.Select(r => r.Name));
					AssertContainsExactElementsInAnyOrder("Should have returned the second ruleset's queued rule.", new[] { queue2.PK }, result2.QueueEntries.Select(q => q.PK));
				}
			}

			using (var thirdConnection = Db.NewExtraConnectionToMainDb())
			using (var result3 = scheduledRuleLoader.GetNextRuleSetToProcess(new BusinessObjectFactory(thirdConnection) { RefreshEnabled = false }))
			{
				AssertNotNull("Should have returned a result.", result3);
				AssertEquals("Should have returned rule set 1.", ruleSet1.PK, result3.RuleSetWithLock.Item.PK);
				AssertContainsExactElementsInAnyOrder("Should have returned the first rule.", new[] { "11", "12" }, result3.Rules.Select(r => r.Name));
				AssertContainsExactElementsInAnyOrder("Should have returned the first queued rule.", new[] { queue1_1.PK, queue1_2.PK }, result3.QueueEntries.Select(q => q.PK));
			}
		}

		#region TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold() => TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold(ruleIsActive: true);

		public void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_InactiveRule() => TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold(ruleIsActive: false);

		void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold(bool ruleIsActive)
		{
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", isLive: false);
			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1");
			var rule1Task = CreateScheduledRuleTask(rule1_1);
			rule1Task.S5_IsActive = ruleIsActive;

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-2));
			queue1.PRQ_RetryCount = 10;
			Factory.Save();

			AssertEquals("Prerequisite", 10, SystemDataRegistry.Instance.MaxNumberOfAttemptsForRuleProcessing.Value);

			var factoryMock = CreateProcessorFactoryMock();
			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			var postMasterPK = Groups.PostMastersGroupPK;
			using (WarehouseDataRegistry.Instance.WaveCreationRulesFailureNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, postMasterPK))
			using (var result = scheduledRuleLoader.GetNextRuleSetToProcess(Factory))
			{
				AssertNull("Should not have returned a result.", result);

				Assert("Should have deleted queue1.", queue1.IsDeleted);
				Assert("Should have disabled production rule task.", !rule1Task.S5_IsActive);
			}
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_ReturnsRulesThatAreStillActive()
		{
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", isLive: false);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", isLive: false);

			var rule1_1 = Helper.CreateRule(ruleSet1, "11", "11");
			var rule1_2 = Helper.CreateRule(ruleSet1, "12", "12");
			var rule1_3 = Helper.CreateRule(ruleSet1, "13", "13");
			var rule2_1 = Helper.CreateRule(ruleSet2, "21", "21");

			var rule1_1Task = CreateScheduledRuleTask(rule1_1);
			var rule1_2Task = CreateScheduledRuleTask(rule1_2);
			var rule1_3Task = CreateScheduledRuleTask(rule1_2);
			var rule2_1Task = CreateScheduledRuleTask(rule2_1);

			rule1_3Task.S5_IsActive = false; // Pretend this task was manually scheduled

			var queue1_1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddDays(-3));
			var queue1_2 = Helper.CreateScheduledRuleQueue(rule1_2, ZDateTime.Today.AddDays(-2));
			var queue1_3 = Helper.CreateScheduledRuleQueue(rule1_3, ZDateTime.Today.AddDays(-1));
			var queue2_1 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today);

			queue1_1.PRQ_RetryCount = 10;
			queue1_2.PRQ_RetryCount = 0;
			queue1_3.PRQ_RetryCount = 0;
			queue2_1.PRQ_RetryCount = 0;
			Factory.Save();

			AssertEquals("Prerequisite", 10, SystemDataRegistry.Instance.MaxNumberOfAttemptsForRuleProcessing.Value);

			var factoryMock = CreateProcessorFactoryMock();
			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			using (var result = scheduledRuleLoader.GetNextRuleSetToProcess(Factory))
			{
				AssertNotNull("Should have returned a result.", result);
				AssertEquals("Should have returned rule set 1.", ruleSet1, result.RuleSetWithLock.Item);
				AssertContainsExactElementsInAnyOrder("Should have returned the second and third rule wrapper.", new[] { "12", "13" }, result.Rules.Select(rule => rule.Name));
				AssertContainsExactElementsInAnyOrder("Should have returned the second and third queued rule.", new[] { queue1_2, queue1_3 }, result.QueueEntries);

				Assert("Should have deleted queue1.", queue1_1.IsDeleted);

				var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
				var queue1_2_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue1_2.PK);
				var queue1_3_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue1_3.PK);
				var queue2_1_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue2_1.PK);
				AssertEquals("Should have bumped queue 2 retry count.", (byte)1, queue1_2_InFactory2.PRQ_RetryCount);
				AssertEquals("Should have bumped queue 3 retry count.", (byte)1, queue1_3_InFactory2.PRQ_RetryCount);
				AssertEquals("Should not have bumped queue 4 retry count.", (byte)0, queue2_1_InFactory2.PRQ_RetryCount);

				var rule1_1Task_InFactory2 = factory2.Load<ProductionRuleScheduleTask>(rule1_1Task.PK);
				var rule1_2Task_InFactory2 = factory2.Load<ProductionRuleScheduleTask>(rule1_2Task.PK);
				var rule1_3Task_InFactory2 = factory2.Load<ProductionRuleScheduleTask>(rule1_3Task.PK);
				var rule2_1Task_InFactory2 = factory2.Load<ProductionRuleScheduleTask>(rule2_1Task.PK);
				Assert("Should have disabled production rule task.", !rule1_1Task_InFactory2.S5_IsActive);
				Assert("Should not have disabled production rule task.", rule1_2Task_InFactory2.S5_IsActive);
				Assert("Should have left production rule task disabled.", !rule1_3Task_InFactory2.S5_IsActive);
				Assert("Should not have disabled production rule task.", rule2_1Task_InFactory2.S5_IsActive);
			}
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_MovesToNextRuleSet()
		{
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", isLive: false);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", isLive: false);

			var rule1_1 = Helper.CreateRule(ruleSet1, "11", "11");
			var rule1_2 = Helper.CreateRule(ruleSet1, "12", "12");
			var rule2_1 = Helper.CreateRule(ruleSet2, "21", "21");

			var rule1Task = CreateScheduledRuleTask(rule1_1);
			var rule2Task = CreateScheduledRuleTask(rule1_2);
			var rule3Task = CreateScheduledRuleTask(rule2_1);

			var queue1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddYears(-10)); // Slight hack to ensure ruleSet1 loads first
			var queue2 = Helper.CreateScheduledRuleQueue(rule1_2, ZDateTime.Today.AddYears(-5));
			var queue3 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today);

			queue1.PRQ_RetryCount = 2;
			queue2.PRQ_RetryCount = 2;
			queue3.PRQ_RetryCount = 0;
			Factory.Save();

			var factoryMock = CreateProcessorFactoryMock();
			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			using (SystemDataRegistry.Instance.MaxNumberOfAttemptsForRuleProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (var result = scheduledRuleLoader.GetNextRuleSetToProcess(Factory))
			{
				AssertNotNull("Should have returned a result.", result);
				AssertEquals("Should have returned rule set 2.", ruleSet2, result.RuleSetWithLock.Item);
				AssertContainsExactElementsInAnyOrder("Should have returned the second rule wrapper.", new[] { "21" }, result.Rules.Select(rule => rule.Name));
				AssertContainsExactElementsInAnyOrder("Should have returned the second queued rule.", new[] { queue3 }, result.QueueEntries);

				Assert("Should have deleted queue1.", queue1.IsDeleted);
				Assert("Should have deleted queue2.", queue2.IsDeleted);

				var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
				var queue3_InFactory2 = factory2.Load<ProductionRuleScheduleQueue>(queue3.PK);
				AssertEquals("Should have bumped queue 3 retry count.", (byte)1, queue3_InFactory2.PRQ_RetryCount);

				var rule1Task_InFactory2 = factory2.Load<ProductionRuleScheduleTask>(rule1Task.PK);
				var rule2Task_InFactory2 = factory2.Load<ProductionRuleScheduleTask>(rule2Task.PK);
				var rule3Task_InFactory2 = factory2.Load<ProductionRuleScheduleTask>(rule3Task.PK);
				Assert("Should have disabled production rule task.", !rule1Task_InFactory2.S5_IsActive);
				Assert("Should have disabled production rule task.", !rule2Task_InFactory2.S5_IsActive);
				Assert("Should not have disabled production rule task.", rule3Task_InFactory2.S5_IsActive);
			}
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmails_All()
		{
			var recipient = Factory.NewWithValidTestData<GlbStaff>();
			recipient.GS_EmailAddress = "testingEmail@email.com.au";
			recipient.GS_Code = "ABC";

			TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmailsCore(Groups.AllPK, new[] { "testingEmail@email.com.au" }, fallback: false);
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmails_Postmaster()
		{
			var postMasterGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, Groups.PostMastersGroupPK));

			var recipient1 = postMasterGroup.Staff.AddNew();
			recipient1.GS_EmailAddress = "postmaster1@email.com.au";
			recipient1.GS_Code = "ABC";
			recipient1.GS_LoginName = "ABC";

			var recipient2 = postMasterGroup.Staff.AddNew();
			recipient2.GS_EmailAddress = "postmaster2@email.com.au";
			recipient2.GS_Code = "DEF";
			recipient2.GS_LoginName = "DEF";

			var nonRecipient = Factory.NewWithValidTestData<GlbStaff>();
			nonRecipient.GS_EmailAddress = "noemails@email.com.au";
			nonRecipient.GS_Code = "GHE";
			nonRecipient.GS_LoginName = "GHE";

			TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmailsCore(Groups.PostMastersGroupPK, new[] { "postmaster1@email.com.au", "postmaster2@email.com.au" }, fallback: false);
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmails_EmptyPostmasterFallsBackToAll()
		{
			var recipient1 = Factory.NewWithValidTestData<GlbStaff>();
			recipient1.GS_EmailAddress = "recipient1@email.com.au";
			recipient1.GS_Code = "ABC";
			recipient1.GS_LoginName = "ABC";

			var recipient2 = Factory.NewWithValidTestData<GlbStaff>();
			recipient2.GS_EmailAddress = "recipient2@email.com.au";
			recipient2.GS_Code = "DEF";
			recipient2.GS_LoginName = "DEF";

			var recipient3 = Factory.NewWithValidTestData<GlbStaff>();
			recipient3.GS_EmailAddress = "recipient3@email.com.au";
			recipient3.GS_Code = "GHE";
			recipient3.GS_LoginName = "GHE";

			TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmailsCore(Groups.PostMastersGroupPK, new[] { "recipient1@email.com.au", "recipient2@email.com.au", "recipient3@email.com.au" }, fallback: true);
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmails_CustomGroup()
		{
			var newEmailGroup = Factory.New<GlbGroup>();
			newEmailGroup.GG_Code = "NEW";

			var recipient1 = newEmailGroup.Staff.AddNew();
			recipient1.GS_EmailAddress = "recipient1@email.com.au";
			recipient1.GS_Code = "ABC";
			recipient1.GS_LoginName = "ABC";

			var recipient2 = newEmailGroup.Staff.AddNew();
			recipient2.GS_EmailAddress = "recipient2@email.com.au";
			recipient2.GS_Code = "DEF";
			recipient2.GS_LoginName = "DEF";

			var nonRecipient = Factory.NewWithValidTestData<GlbStaff>();
			nonRecipient.GS_EmailAddress = "noemails@email.com.au";
			nonRecipient.GS_Code = "GHE";
			nonRecipient.GS_LoginName = "GHE";

			TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmailsCore(newEmailGroup.PK.ToGuid(), new[] { "recipient1@email.com.au", "recipient2@email.com.au" }, fallback: false);
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmails_EmptyCustomGroup()
		{
			var emptyEmailGroup = Factory.New<GlbGroup>();
			emptyEmailGroup.GG_Code = "NEW";

			var recipient1 = Factory.NewWithValidTestData<GlbStaff>();
			recipient1.GS_EmailAddress = "recipient1@email.com.au";
			recipient1.GS_Code = "ABC";
			recipient1.GS_LoginName = "ABC";

			var recipient2 = Factory.NewWithValidTestData<GlbStaff>();
			recipient2.GS_EmailAddress = "recipient2@email.com.au";
			recipient2.GS_Code = "DEF";
			recipient2.GS_LoginName = "DEF";

			var recipient3 = Factory.NewWithValidTestData<GlbStaff>();
			recipient3.GS_EmailAddress = "recipient3@email.com.au";
			recipient3.GS_Code = "GHE";
			recipient3.GS_LoginName = "GHE";

			TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmailsCore(emptyEmailGroup.PK.ToGuid(), new[] { "recipient1@email.com.au", "recipient2@email.com.au", "recipient3@email.com.au" }, fallback: true);
		}

		[TestDate(2022, 10, 21)]
		public void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmails_EmptyRegistryValue()
		{
			var recipient1 = Factory.NewWithValidTestData<GlbStaff>();
			recipient1.GS_EmailAddress = "recipient1@email.com.au";
			recipient1.GS_Code = "ABC";
			recipient1.GS_LoginName = "ABC";

			var recipient2 = Factory.NewWithValidTestData<GlbStaff>();
			recipient2.GS_EmailAddress = "recipient2@email.com.au";
			recipient2.GS_Code = "DEF";
			recipient2.GS_LoginName = "DEF";

			var recipient3 = Factory.NewWithValidTestData<GlbStaff>();
			recipient3.GS_EmailAddress = "recipient3@email.com.au";
			recipient3.GS_Code = "GHE";
			recipient3.GS_LoginName = "GHE";

			TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmailsCore(Guid.Empty, Array.Empty<string>(), fallback: true);
		}

		void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmailsCore(Guid groupPK, string[] expectedRecipients, bool fallback)
		{
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWP", isLive: false);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", context: "PWA", isLive: false);

			var rule1_1 = Helper.CreateRule(ruleSet1, "RULE1_1", "1");
			var rule1_2 = Helper.CreateRule(ruleSet1, "RULE1_2", "2");
			var rule1_3 = Helper.CreateRule(ruleSet1, "RULE1_3", "3");
			var rule2_1 = Helper.CreateRule(ruleSet2, "RULE2_1", "3");
			var rule2_2 = Helper.CreateRule(ruleSet2, "RULE2_2", "4");

			var rule1_1Task = CreateScheduledRuleTask(rule1_1);
			var rule1_2Task = CreateScheduledRuleTask(rule1_2);
			var rule1_3Task = CreateScheduledRuleTask(rule1_3);
			var rule2_1Task = CreateScheduledRuleTask(rule2_1);
			var rule2_2Task = CreateScheduledRuleTask(rule2_2);

			rule1_3Task.S5_IsActive = false; // Pretend manually scheduled, should not send an email

			var queue1_1 = Helper.CreateScheduledRuleQueue(rule1_1, ZDateTime.Today.AddYears(-11));
			var queue1_2 = Helper.CreateScheduledRuleQueue(rule1_2, ZDateTime.Today.AddYears(-11));
			var queue1_3 = Helper.CreateScheduledRuleQueue(rule1_3, ZDateTime.Today.AddYears(-11));
			var queue2_1 = Helper.CreateScheduledRuleQueue(rule2_1, ZDateTime.Today);
			var queue2_2 = Helper.CreateScheduledRuleQueue(rule2_2, ZDateTime.Today);

			queue1_1.PRQ_RetryCount = 10;
			queue1_2.PRQ_RetryCount = 10;
			queue1_3.PRQ_RetryCount = 10;
			queue2_1.PRQ_RetryCount = 10;
			queue2_2.PRQ_RetryCount = 0;
			Factory.Save();

			var errorContactGroupRegistryItem1 = new GuidRegistryItem(
				"TestRegistryItem1",
				(NoResString)"RegistryItem1",
				(NoResString)"TestRegistryItem",
				(NoResString)"",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				defaultValue: groupPK);

			var errorContactGroupRegistryItem2 = new GuidRegistryItem(
				"TestRegistryItem2",
				(NoResString)"RegistryItem2",
				(NoResString)"TestRegistryItem",
				(NoResString)"",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				defaultValue: groupPK);

			var factoryMock = new Mock<IScheduledRuleProcessorFactory>();
			var processorMock = new Mock<IScheduledRuleProcessor>();
			factoryMock.Setup(f => f.GetRuleProcessor(It.IsAny<RulesContextType>())).Returns(processorMock.Object);
			processorMock.SetupSequence(p => p.ErrorContactGroupRegistryItem)
				.Returns(errorContactGroupRegistryItem1)
				.Returns(errorContactGroupRegistryItem2);
			processorMock.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), It.IsAny<ProductionRuleSet>())).Returns(Env.CurrentBranchPK);

			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			using (var result = scheduledRuleLoader.GetNextRuleSetToProcess(Factory))
			{
				AssertNotNull("Should have returned a result.", result);

				Assert("Should have deleted queue1_1.", queue1_1.IsDeleted);
				Assert("Should have deleted queue1_2.", queue1_2.IsDeleted);
				Assert("Should have deleted queue1_2.", queue1_3.IsDeleted);
				Assert("Should have deleted queue2_1.", queue2_1.IsDeleted);
				Assert("Should not have deleted queue2_2.", !queue2_2.IsDeleted);

				if (expectedRecipients.Any())
				{
					AssertEquals("An email should be sent per RuleSet.", 2, Env.OutgoingMailManager.EmailsCreated.Count);

					var errorEmail1 = Env.OutgoingMailManager.EmailsCreated[0];
					AssertContainsExactElementsInAnyOrder(expectedRecipients, errorEmail1.Recipients.ToStringCollection());
					AssertEquals("TEST1 Rule Set Failure", errorEmail1.Subject);
					AssertEquals($@"Production Rule Set TEST1 for Context: InventoryPutaway disabled the following rule(s) due to repeated failure to process:
RULE1_1
RULE1_2

Error Logs can be viewed on the Scheduled Production Rule Consumer (SPC) service task.

To include disabled rules in future runs, re-enable the rules via the Production Rules Management Portal.


{ExpectedReason(1)}", errorEmail1.Body);

					var errorEmail2 = Env.OutgoingMailManager.EmailsCreated[1];
					AssertContainsExactElementsInAnyOrder(expectedRecipients, errorEmail2.Recipients.ToStringCollection());
					AssertEquals("TEST2 Rule Set Failure", errorEmail2.Subject);
					AssertEquals($@"Production Rule Set TEST2 for Context: ProductWarehouseAllocation disabled the following rule(s) due to repeated failure to process:
RULE2_1

Error Logs can be viewed on the Scheduled Production Rule Consumer (SPC) service task.

To include disabled rules in future runs, re-enable the rules via the Production Rules Management Portal.


{ExpectedReason(2)}", errorEmail2.Body);

					string ExpectedReason(int invocation)
						=> fallback
							? $"This email is sent to the group defined at System Registry: RegistryItem{invocation} -> TestRegistryItem. Since there are no valid email addresses set up in this group this email has been sent to all users."
							: $"You have received this email because you are a member of the staff group defined at System Registry: RegistryItem{invocation} -> TestRegistryItem.";
				}
				else
				{
					AssertEquals("No emails should be sent if user defined registry item is empty.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				}
			}

			processorMock.Verify(p => p.ErrorContactGroupRegistryItem);
		}

		public void TestGetNextRuleSetToProcess_RetryAttemptMeetsThreshold_SendsEmails_NoRecipients()
		{
			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWP", isLive: false);
			var ruleSet2 = Helper.CreateRuleSet("TEST2", "TEST2", context: "PWA", isLive: false);

			var rule = Helper.CreateRule(ruleSet1, "RULE1_1", "1");
			var rule1Task = CreateScheduledRuleTask(rule);

			var queue = Helper.CreateScheduledRuleQueue(rule, ZDateTime.Today.AddYears(-11));
			queue.PRQ_RetryCount = 10;
			Factory.Save();

			var errorContactGroupRegistryItem1 = new GuidRegistryItem(
				"TestRegistryItem1",
				(NoResString)"RegistryItem1",
				(NoResString)"TestRegistryItem",
				(NoResString)"",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				defaultValue: Groups.AllPK);

			var factoryMock = new Mock<IScheduledRuleProcessorFactory>();
			var processorMock = new Mock<IScheduledRuleProcessor>();
			factoryMock.Setup(f => f.GetRuleProcessor(It.IsAny<RulesContextType>())).Returns(processorMock.Object);
			processorMock.SetupSequence(p => p.ErrorContactGroupRegistryItem)
			.Returns(errorContactGroupRegistryItem1);
			processorMock.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), It.IsAny<ProductionRuleSet>())).Returns(Env.CurrentBranchPK);

			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			ScheduledRuleLoaderResult result = null;
			AssertNoExceptionThrown(() => result = scheduledRuleLoader.GetNextRuleSetToProcess(Factory));
			using (result)
			{
				AssertNull("Should have returned nothing.", result);
				Assert("Should have deleted queue.", queue.IsDeleted);
				AssertEquals("No emails sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			processorMock.Verify(p => p.ErrorContactGroupRegistryItem);
		}

		#endregion

		#region TestBranchChanges_BasedOnRegistryValue

		public void TestErrorEmail_BasedOnRegistryValue_CompanyLevel() => TestErrorEmail_BasedOnRegistryValueCore(true, "recipient3@email.com.au");

		public void TestErrorEmail_BasedOnRegistryValue_BranchLevel() => TestErrorEmail_BasedOnRegistryValueCore(false, "recipient2@email.com.au");

		void TestErrorEmail_BasedOnRegistryValueCore(bool registryFromCompany, string expectedReceipent)
		{
			var newEmailGroup1 = Factory.New<GlbGroup>();
			newEmailGroup1.GG_Code = "NEW";
			var recipient1 = newEmailGroup1.Staff.AddNew();
			recipient1.GS_EmailAddress = "recipient1@email.com.au";
			recipient1.GS_Code = "ABC";
			recipient1.GS_LoginName = "ABC";
			var newEmailGroup2 = Factory.New<GlbGroup>();
			newEmailGroup1.GG_Code = "NE1";

			var recipient2 = newEmailGroup2.Staff.AddNew();
			recipient2.GS_EmailAddress = "recipient2@email.com.au";
			recipient2.GS_Code = "AB2";
			recipient2.GS_LoginName = "AB2";

			var newEmailGroup3 = Factory.New<GlbGroup>();
			newEmailGroup3.GG_Code = "NE3";

			var recipient3 = newEmailGroup3.Staff.AddNew();
			recipient3.GS_EmailAddress = "recipient3@email.com.au";
			recipient3.GS_Code = "AB3";
			recipient3.GS_LoginName = "AB3";

			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse1 = (IWhsWarehouse)whsHelper.CreateWarehouse("WHS", "A");
			var whsBranch1 = Factory.Load<GlbBranch>(warehouse1.WW_GB_RelatedCompanyBranch);

			var warehouse2 = (IWhsWarehouse)whsHelper.CreateWarehouse("WH2", "A");
			var whsBranch2 = Factory.Load<GlbBranch>(warehouse2.WW_GB_RelatedCompanyBranch);

			var ruleSet1 = Helper.CreateRuleSet("TEST1", "TEST1", context: "PWP", isLive: false, warehousePK: registryFromCompany ? warehouse2.PK : warehouse1.PK);
			var rule = Helper.CreateRule(ruleSet1, "RULE1_1", "1");
			var rule1Task = CreateScheduledRuleTask(rule);

			var queue = Helper.CreateScheduledRuleQueue(rule, ZDateTime.Today.AddYears(-11));
			queue.PRQ_RetryCount = 10;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var errorContactGroupRegistryItem = new GuidRegistryItem(
				"TestRegistryItem",
				(NoResString)"RegistryItem",
				(NoResString)"TestRegistryItem",
				(NoResString)"",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				defaultValue: newEmailGroup1.PK.ToGuid());

			var factoryMock = new Mock<IScheduledRuleProcessorFactory>();
			var processorMock = new Mock<IScheduledRuleProcessor>();
			factoryMock.Setup(f => f.GetRuleProcessor(It.IsAny<RulesContextType>())).Returns(processorMock.Object);
			processorMock.Setup(p => p.ErrorContactGroupRegistryItem).Returns(errorContactGroupRegistryItem);
			processorMock.Setup(p => p.GetBranchToRunRulesAgainst(Factory.GetCachedReadOnlyFactory(), ruleSet1)).Returns(registryFromCompany ? warehouse2.WW_GB_RelatedCompanyBranch : warehouse1.WW_GB_RelatedCompanyBranch);

			var scheduledRuleLoader = new ScheduledRuleLoader(factoryMock.Object);
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (errorContactGroupRegistryItem.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, newEmailGroup1.PK.ToGuid()))
			using (errorContactGroupRegistryItem.SetTemporaryValue(Guid.Empty, warehouse1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, newEmailGroup2.PK.ToGuid()))
			using (errorContactGroupRegistryItem.SetTemporaryValue(whsBranch2.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, newEmailGroup3.PK.ToGuid()))
			using (var result = scheduledRuleLoader.GetNextRuleSetToProcess(Factory))
			{
				AssertNull("Should have returned null result.", result);
				Assert("Should have deleted queue.", queue.IsDeleted);

				AssertEquals("An email should be sent for RuleSet.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var errorEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertContainsExactElementsInAnyOrder(new[] { expectedReceipent }, errorEmail.Recipients.ToStringCollection());
				AssertEquals("TEST1 Rule Set Failure", errorEmail.Subject);
				AssertEquals($@"Production Rule Set TEST1 for Context: InventoryPutaway disabled the following rule(s) due to repeated failure to process:
RULE1_1

Error Logs can be viewed on the Scheduled Production Rule Consumer (SPC) service task.

To include disabled rules in future runs, re-enable the rules via the Production Rules Management Portal.


You have received this email because you are a member of the staff group defined at System Registry: RegistryItem -> TestRegistryItem.", errorEmail.Body);
			}
		}

		#endregion

		#region Implementation

		static Mock<IScheduledRuleProcessorFactory> CreateProcessorFactoryMock()
		{
			var processorMock = new Mock<IScheduledRuleProcessor>();
			processorMock.Setup(p => p.GetBranchToRunRulesAgainst(It.IsAny<ReadOnlyBusinessObjectFactory>(), It.IsAny<ProductionRuleSet>())).Returns(Env.CurrentBranchPK);

			var factoryMock = new Mock<IScheduledRuleProcessorFactory>();
			factoryMock.Setup(f => f.GetRuleProcessor(It.IsAny<RulesContextType>())).Returns(processorMock.Object);

			return factoryMock;
		}

		static ProductionRuleScheduleTask CreateScheduledRuleTask(ProductionRule rule)
		{
			var scheduleTask = rule.Factory.New<ProductionRuleScheduleTask>();
			scheduleTask.S5_ScheduleDescription = rule.PRL_Name;
			scheduleTask.S5_DayList = "NNYYYYN";
			scheduleTask.S5_ScheduleType = "W";
			scheduleTask.S5_DailyStartTime = ZDateTime.Today;
			scheduleTask.S5_ParentID = rule.PK;
			return scheduleTask;
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		Helper Helper => helper ?? (helper = new Helper(Factory));
		Helper helper;

		#endregion
	}
}
