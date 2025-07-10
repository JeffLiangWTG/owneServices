using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ProductionRules.ServiceTasks.Testing
{
	[TestedType(typeof(ScheduledProductionRuleQueueConsumerTask))]
	class ScheduledProductionRuleQueueConsumerTaskTest : ServiceTaskTestCase<ScheduledProductionRuleQueueConsumerTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(HostedServiceAttribute.Code), "SPC", hostedServiceAttribute.Code);
				AssertEquals(nameof(HostedServiceAttribute.Description), "Scheduled Production Rule Queue Consumer", hostedServiceAttribute.Description);
				AssertEquals(nameof(HostedServiceAttribute.Category), "SYS", hostedServiceAttribute.Category);
				AssertEquals(nameof(HostedServiceAttribute.CanRunInAnyBranch), true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals(nameof(HostedServiceAttribute.AllowsMultipleInstances), true, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals(nameof(HostedServiceAttribute.IsMandatory), true, hostedServiceAttribute.IsMandatory);
				AssertEquals(nameof(HostedServiceAttribute.MinimumPeriod), "15minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals(nameof(HostedServiceAttribute.DefaultScheduleRunEvery), "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
				Assert(nameof(HostedServiceAttribute.ActiveByDefault), hostedServiceAttribute.ActiveByDefault);
			});
		}

		public void TestRunTask()
		{
			var cancellationToken = new CancellationToken();

			var queueConsumerMock = new Mock<IScheduledProductionRuleQueueConsumer>();
			var task = new ScheduledProductionRuleQueueConsumerTask { ServiceLogger = new TestServiceLogger() };

			using (ObjectFactory.Substitute(queueConsumerMock.Object))
			{
				task.RunTask(cancellationToken);
			}

			queueConsumerMock.Verify(qc => qc.ProcessQueue(It.IsAny<INotifications>(), cancellationToken));
			queueConsumerMock.VerifyNoOtherCalls();
			Assert(true); // Prevent empty test failure
		}

		public void TestRunTask_NotificationSubscriber()
		{
			var cancellationToken = new CancellationToken();

			INotifications notifications = null;
			var queueConsumerMock = new Mock<IScheduledProductionRuleQueueConsumer>();
			queueConsumerMock
				.Setup(qc => qc.ProcessQueue(It.IsAny<INotifications>(), cancellationToken))
				.Callback<INotifications, CancellationToken>((n, _) => notifications = n);

			var serviceLogger = new TestServiceLogger();
			var task = new ScheduledProductionRuleQueueConsumerTask { ServiceLogger = serviceLogger };

			using (ObjectFactory.Substitute(queueConsumerMock.Object))
			{
				task.RunTask(cancellationToken);
			}

			queueConsumerMock.Verify(qc => qc.ProcessQueue(It.IsAny<INotifications>(), cancellationToken));
			queueConsumerMock.VerifyNoOtherCalls();

			AssertNotNull(notifications);
			AssertEquals("Precondition.", 0, serviceLogger.Count);

			notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "TEST"));
			AssertEquals("Should have a notification.", 1, serviceLogger.Count);
			AssertEquals("Should have a notification.", "Error|TEST", serviceLogger[0]);
		}

		[UseSnapshotProtection]
		[TestDate(2023, 11, 1)]
		public void TestQueueSize()
		{
			var ruleSets = Factory.Load<ProductionRuleSet>(new ZQuery());

			foreach (var existingRuleSet in ruleSets)
			{
				existingRuleSet.PRS_IsLive = false;
			}

			var someOtherDate = new ZDateTime(2020, 10, 1);
			var ruleSet1 = Helper.CreateRuleSet("A", "A", context: "PWP", lastEditTime: someOtherDate);
			var ruleSet2 = Helper.CreateRuleSet("B", "B", context: "PWP", isLive: false, lastEditTime: someOtherDate);
			var ruleSet3 = Helper.CreateRuleSet("C", "C", context: "PWA", lastEditTime: someOtherDate);
			var ruleSet4 = Helper.CreateRuleSet("D", "D", context: "TWP", lastEditTime: someOtherDate);

			var rule1_1 = Helper.CreateRule(ruleSet1, "Rule 1", "Desc", 42);
			var rule1_2 = Helper.CreateRule(ruleSet1, "Rule 2", "Desc", 1);
			var rule2_1 = Helper.CreateRule(ruleSet2, "Rule Set 2 Rule 1", "Desc", 2);
			var rule3_1 = Helper.CreateRule(ruleSet3, "Rule Set 3 Rule 1", "Desc", 2);
			var rule4_1 = Helper.CreateRule(ruleSet4, "Rule Set 4 Rule 1", "Desc", 2);

			Factory.Save();

			var rule1_1_Queue = Factory.New<ProductionRuleScheduleQueue>();
			rule1_1_Queue.PRQ_PRL_Rule = rule1_1.PK;

			var rule1_2_Queue = Factory.New<ProductionRuleScheduleQueue>();
			rule1_2_Queue.PRQ_PRL_Rule = rule1_2.PK;

			var rule2_1_Queue = Factory.New<ProductionRuleScheduleQueue>();
			rule2_1_Queue.PRQ_PRL_Rule = rule2_1.PK;

			var rule3_1_Queue = Factory.New<ProductionRuleScheduleQueue>();
			rule3_1_Queue.PRQ_PRL_Rule = rule3_1.PK;
			Factory.Save();

			var editDate = new ZDateTime(2023, 10, 1);
			rule1_1_Queue.PRQ_SystemCreateTimeUtc = editDate.AddDays(10);
			rule1_2_Queue.PRQ_SystemCreateTimeUtc = editDate.AddDays(20);
			rule2_1_Queue.PRQ_SystemCreateTimeUtc = editDate;
			rule3_1_Queue.PRQ_SystemCreateTimeUtc = editDate.AddDays(30);
			Factory.Save();

			IHostedServiceQueueProvider task = new ScheduledProductionRuleQueueConsumerTask();
			var result = task.QueueResult;
			AssertEquals(2, result.QueueSize); // RuleSet1 + RuleSet3
			var expectedAge = DateTime.UtcNow - editDate;
			AssertCloseEnough((int)expectedAge.TotalSeconds, (int)result.MaximumItemAge.TotalSeconds, 60);
		}

		[UseSnapshotProtection]
		[TestDate(2023, 11, 1)]
		public void TestQueueSize_Empty()
		{
			IHostedServiceQueueProvider task = new ScheduledProductionRuleQueueConsumerTask();
			var result = task.QueueResult;
			AssertEquals(0, result.QueueSize);
			AssertEquals(0, (int)result.MaximumItemAge.TotalSeconds);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(ProductionRuleScheduleQueueSchema.Constants.TableName, "Scheduled Rules"),
				};
			}
		}

		Helper Helper => helper ?? (helper = new Helper(Factory));
		Helper helper;
	}
}
