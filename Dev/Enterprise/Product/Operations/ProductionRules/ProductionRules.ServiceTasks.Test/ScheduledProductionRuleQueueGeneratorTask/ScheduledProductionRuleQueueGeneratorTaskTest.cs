using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Business.Testing;
using Enterprise.Scheduler.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ProductionRules.ServiceTasks.Testing
{
	[TestedType(typeof(ScheduledProductionRuleQueueGeneratorTask))]
	class ScheduledProductionRuleQueueGeneratorTaskTest : ServiceTaskTestCase<ScheduledProductionRuleQueueGeneratorTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(HostedServiceAttribute.Code), "SPR", hostedServiceAttribute.Code);
				AssertEquals(nameof(HostedServiceAttribute.Description), "Scheduled Production Rule Queue Generator", hostedServiceAttribute.Description);
				AssertEquals(nameof(HostedServiceAttribute.Category), "SYS", hostedServiceAttribute.Category);
				AssertEquals(nameof(HostedServiceAttribute.CanRunInAnyBranch), true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals(nameof(HostedServiceAttribute.AllowsMultipleInstances), false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals(nameof(HostedServiceAttribute.IsMandatory), true, hostedServiceAttribute.IsMandatory);
				AssertEquals(nameof(HostedServiceAttribute.MinimumPeriod), "5minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals(nameof(HostedServiceAttribute.DefaultScheduleRunEvery), "5minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
				Assert(nameof(HostedServiceAttribute.ActiveByDefault), hostedServiceAttribute.ActiveByDefault);
			});
		}

		public void TestQueueSize()
		{
			var queue = new QueueResult(42, TimeSpan.FromSeconds(1));
			var scheduledTaskRunnerMock = new Mock<IScheduleTaskRunner>();
			scheduledTaskRunnerMock.Setup(str => str.GetPendingJobsQueue(ProductionRuleSchema.Constants.Prefix)).Returns(queue);

			var task = new ScheduledProductionRuleQueueGeneratorTask();

			using (ObjectFactory.Substitute(scheduledTaskRunnerMock.Object))
			{
				AssertEquals(queue.QueueSize, ((IHostedServiceQueueProvider)task).QueueResult.QueueSize);
				AssertEquals(queue.MaximumItemAge, ((IHostedServiceQueueProvider)task).QueueResult.MaximumItemAge);
			}

			scheduledTaskRunnerMock.Verify(str => str.GetPendingJobsQueue(ProductionRuleSchema.Constants.Prefix));
			scheduledTaskRunnerMock.VerifyNoOtherCalls();
		}

		public void TestRunTask()
		{
			var cancellationToken = new CancellationToken();
			INotifications notificationsMock = null;

			var loggerMock = new Mock<ILogger>();

			var scheduledTaskRunnerMock = new Mock<IScheduleTaskRunner>();
			scheduledTaskRunnerMock
				.Setup(str => str.Process(ProductionRuleSchema.Constants.Prefix, false, It.IsAny<INotifications>(), cancellationToken))
				.Callback<ZString, bool, INotifications, CancellationToken>((prefix, useStm, n, c) => notificationsMock = n);

			var task = new ScheduledProductionRuleQueueGeneratorTask();
			task.ServiceLogger = loggerMock.Object;

			using (ObjectFactory.Substitute(scheduledTaskRunnerMock.Object))
			{
				task.RunTask(cancellationToken);
			}

			scheduledTaskRunnerMock.Verify(str => str.Process(ProductionRuleSchema.Constants.Prefix, false, It.IsAny<INotifications>(), cancellationToken));
			scheduledTaskRunnerMock.VerifyNoOtherCalls();

			var mockedNotification = new Mock<INotification>();
			mockedNotification.Setup(n => n.Type).Returns(CargoWise.ComponentModel.NotificationType.Error);
			mockedNotification.Setup(n => n.Message).Returns("Test message!");

			AssertNotNull(notificationsMock);
			notificationsMock.Notify(mockedNotification.Object);

			loggerMock.Verify(l => l.Log(LogType.Error, "Test message!"));
			loggerMock.VerifyNoOtherCalls();
		}

		[TestDate(2022, 1, 3)]
		[UseSnapshotProtection]
		public void TestRunTask_EndToEnd()
		{
			TestRunTask_EndToEnd(hasWarehouseSet: true);
		}

		[TestDate(2022, 1, 3)]
		[UseSnapshotProtection]
		public void TestRunTask_EndToEnd_NoWarehouse()
		{
			TestRunTask_EndToEnd(hasWarehouseSet: false);
		}

		void TestRunTask_EndToEnd(bool hasWarehouseSet)
		{
			var ruleSets = Factory.Load<ProductionRuleSet>(new ZQuery());

			foreach (var existingRuleSet in ruleSets)
			{
				existingRuleSet.PRS_IsLive = false;
			}

			var warehousePk = ZGuid.Empty;

			if (hasWarehouseSet)
			{
				var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
				warehousePk = warehouse.PK;
			}

			var ruleSet1 = Helper.CreateRuleSet("A", "A", context: "PWP", warehousePK: warehousePk);
			var ruleSet2 = Helper.CreateRuleSet("B", "B", context: "PWP", isLive: false, warehousePK: warehousePk);

			var rule1_1 = Helper.CreateRule(ruleSet1, "Rule 1", "Desc", 42);
			var rule1_2 = Helper.CreateRule(ruleSet1, "Rule 2", "Desc", 1);
			var rule2_1 = Helper.CreateRule(ruleSet2, "Rule Set 2 Rule 1", "Desc", 2);

			Factory.Save();

			var scheduleTask1 = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask1.S5_ParentID = rule1_1.PK;
			scheduleTask1.S5_GB = ZGuid.Empty;

			var scheduleTask2 = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask2.S5_ParentID = rule1_2.PK;
			scheduleTask1.S5_GB = ZGuid.Empty;

			var scheduleTask3 = Factory.New<ProductionRuleScheduleTask>();
			scheduleTask3.S5_ParentID = rule2_1.PK;
			scheduleTask3.S5_GB = ZGuid.Empty;

			Factory.Save();
			TestConnection.CommitTransaction();

			try
			{
				var serviceLogger = new TestServiceLogger();

				using (EnvProxy.Instance.TemporaryServiceTaskContext("SPR", canRunInAnyBranch: true))
				{
					var serviceTask = new ScheduledProductionRuleQueueGeneratorTask { ServiceLogger = serviceLogger };
					serviceTask.RunTask();
				}

				AssertEquals("Should have logged service tasks.",
	@"Information|3 scheduled tasks to run
Information|Running 1 of 3 scheduled tasks: Queue Scheduled Rule, Context: PWP, RuleSet: A, Rule: Rule 2
Information|Task completed. Queue Scheduled Rule, Context: PWP, RuleSet: A, Rule: Rule 2. Time taken: 0 seconds
Information|Running 2 of 3 scheduled tasks: Queue Scheduled Rule, Context: PWP, RuleSet: B, Rule: Rule Set 2 Rule 1
Warning|Production Ruleset is not live: Queue Scheduled Rule, Context: PWP, RuleSet: B, Rule: Rule Set 2 Rule 1
Information|Task completed. Queue Scheduled Rule, Context: PWP, RuleSet: B, Rule: Rule Set 2 Rule 1. Time taken: 0 seconds
Information|Running 3 of 3 scheduled tasks: Queue Scheduled Rule, Context: PWP, RuleSet: A, Rule: Rule 1
Information|Task completed. Queue Scheduled Rule, Context: PWP, RuleSet: A, Rule: Rule 1. Time taken: 0 seconds
", serviceLogger.ToString());

				var queues = Factory.Load<ProductionRuleScheduleQueue>(new ZQuery());
				AssertEquals("Should have populated the queue twice.", 2, queues.Length);
				AssertEquals("Should have populated the queue twice.", true, queues.Any(q => q.PRQ_PRL_Rule == rule1_1.PK));
				AssertEquals("Should have populated the queue twice.", true, queues.Any(q => q.PRQ_PRL_Rule == rule1_2.PK));
			}
			finally
			{
				TestConnection.BeginTransaction();
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		Helper Helper => helper ?? (helper = new Helper(Factory));
		Helper helper;
	}
}
