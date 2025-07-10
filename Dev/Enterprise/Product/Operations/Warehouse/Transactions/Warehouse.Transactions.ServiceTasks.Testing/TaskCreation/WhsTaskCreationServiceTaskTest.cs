using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	[TestedType(typeof(WhsTaskCreationServiceTask))]
	class WhsTaskCreationServiceTaskTest : ServiceTaskTestCase<WhsTaskCreationServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(HostedServiceAttribute.Code), "WTC", hostedServiceAttribute.Code);
				AssertEquals(nameof(HostedServiceAttribute.Description), "Product Warehouse Task Creation", hostedServiceAttribute.Description);
				AssertEquals(nameof(HostedServiceAttribute.Category), "SYS", hostedServiceAttribute.Category);
				AssertEquals(nameof(HostedServiceAttribute.CanRunInAnyBranch), true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals(nameof(HostedServiceAttribute.AllowsMultipleInstances), true, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals(nameof(HostedServiceAttribute.IsMandatory), true, hostedServiceAttribute.IsMandatory);
				AssertEquals(nameof(HostedServiceAttribute.MinimumPeriod), "15minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals(nameof(HostedServiceAttribute.DefaultScheduleRunEvery), "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
				Assert(nameof(HostedServiceAttribute.ActiveByDefault), hostedServiceAttribute.ActiveByDefault);
			});
		}

		public void TestCheckTaskManagementEnabled()
		{
			AssertEquals("The registry setting 'Warehouse -> Task Management -> Expose Warehouse Task Management' requires a value equal to 'True'.", WhsTaskCreationServiceTask.CheckTaskManagementEnabled());

			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(string.Empty, WhsTaskCreationServiceTask.CheckTaskManagementEnabled());
			}

			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("The registry setting 'Warehouse -> Task Management -> Expose Warehouse Task Management' requires a value equal to 'True'.", WhsTaskCreationServiceTask.CheckTaskManagementEnabled());
			}
		}

		public void TestHostedServiceRequirementAttribute()
		{
			var method = typeof(WhsTaskCreationServiceTask).GetMethods().SingleOrDefault(x => x.Name == nameof(WhsTaskCreationServiceTask.CheckTaskManagementEnabled));
			AssertNotNull("Service Task must have HostedServiceRequirement function.", method?.GetCustomAttributes(typeof(HostedServiceRequirementAttribute), false).SingleOrDefault());
		}

		public void TestRunTask()
		{
			var cancellationToken = new CancellationToken();

			var processorMock = new Mock<IWhsTaskCreationProcessor>();
			var task = new WhsTaskCreationServiceTask { ServiceLogger = new TestServiceLogger() };

			using (ObjectFactory.Substitute(processorMock.Object))
			{
				task.RunTask(cancellationToken);
			}

			processorMock.Verify(p => p.ProcessQueue(It.IsAny<INotifications>(), cancellationToken));
			processorMock.VerifyNoOtherCalls();
			Assert(true); // Prevent empty test failure
		}

		public void TestRunTask_NotificationSubscriber()
		{
			var cancellationToken = new CancellationToken();

			INotifications notifications = null;
			var processorMock = new Mock<IWhsTaskCreationProcessor>();
			processorMock
				.Setup(qc => qc.ProcessQueue(It.IsAny<INotifications>(), cancellationToken))
				.Callback<INotifications, CancellationToken>((n, _) => notifications = n);

			var serviceLogger = new TestServiceLogger();
			var task = new WhsTaskCreationServiceTask { ServiceLogger = serviceLogger };

			using (ObjectFactory.Substitute(processorMock.Object))
			{
				task.RunTask(cancellationToken);
			}

			processorMock.Verify(qc => qc.ProcessQueue(It.IsAny<INotifications>(), cancellationToken));
			processorMock.VerifyNoOtherCalls();

			AssertNotNull(notifications);
			AssertEquals("Precondition.", 0, serviceLogger.Count);

			notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "TEST"));
			AssertEquals("Should have a notification.", 1, serviceLogger.Count);
			AssertEquals("Should have a notification.", "Error|TEST", serviceLogger[0]);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(WhsPickSchema.Constants.TableName, "Picks", WhsPickSchema.Constants.WP_TaskPlanningStatus + "=" + TaskPlanningStatus.Codes.Ready),
					new TaskNudgeInformationForTest(WhsDocketSchema.Constants.TableName, "Dockets", WhsDocketSchema.Constants.WD_TaskPlanningStatus + "=" + TaskPlanningStatus.Codes.Ready),
					new TaskNudgeInformationForTest(WhsCycleCountLocationSchema.Constants.TableName, "Cycle Count Locations", WhsCycleCountLocationSchema.Constants.WCL_TaskPlanningStatus + "=" + TaskPlanningStatus.Codes.Ready),
					new TaskNudgeInformationForTest(WhsLoadSchema.Constants.TableName, "Loads", WhsLoadSchema.Constants.WLO_TaskPlanningStatus + "=" + TaskPlanningStatus.Codes.Ready),
				};
			}
		}

		WhsTestHelperFunctions Helper => helper ??= new WhsTestHelperFunctions(Factory);
		WhsTestHelperFunctions helper;
	}
}
