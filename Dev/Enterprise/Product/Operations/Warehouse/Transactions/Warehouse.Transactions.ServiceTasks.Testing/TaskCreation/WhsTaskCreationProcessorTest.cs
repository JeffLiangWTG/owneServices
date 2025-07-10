using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Utils;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ProductionRules.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class WhsTaskCreationProcessorTest : WhsTestCaseWithFactory
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<WhsTaskCreationProcessor>(ObjectFactory.Get<IWhsTaskCreationProcessor>());
		}

		public void TestConstructor()
		{
			var jobLoader = Mock.Of<IReadyForPlanningJobLoader>();
			var jobStrategyFactory = Mock.Of<ITaskCreationJobStrategyFactory>();
			var taskFactory = Mock.Of<IWhsTaskFactory>();
			AssertExceptionThrown<ArgumentNullException>(() => new WhsTaskCreationProcessor(null, jobStrategyFactory, taskFactory));
			AssertExceptionThrown<ArgumentNullException>(() => new WhsTaskCreationProcessor(jobLoader, null, taskFactory));
			AssertExceptionThrown<ArgumentNullException>(() => new WhsTaskCreationProcessor(jobLoader, jobStrategyFactory, null));
		}

		public void TestProcessQueue_NullNotifications()
		{
			var jobLoader = Mock.Of<IReadyForPlanningJobLoader>();
			var jobStrategyFactory = Mock.Of<ITaskCreationJobStrategyFactory>();
			var taskFactory = Mock.Of<IWhsTaskFactory>();
			var processor = new WhsTaskCreationProcessor(jobLoader, jobStrategyFactory, taskFactory);
			AssertExceptionThrown<ArgumentNullException>(() => processor.ProcessQueue(null, CancellationToken.None));
		}

		public void TestProcessQueue_NoQueuedJobs()
		{
			var notificationsMock = new Mock<INotifications>();
			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			var taskFactory = new Mock<IWhsTaskFactory>();
			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);

			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns((AppLockedItem<WhsReadyForPlanningJobsView>)null);

			processor.ProcessQueue(notificationsMock.Object, CancellationToken.None);
			jobLoader.Verify(srl => srl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>()), Times.Once);
			jobStrategyFactory.VerifyNoOtherCalls();
			taskFactory.VerifyNoOtherCalls();
			Assert(true);
		}

		public void TestProcessQueue()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationToken = new CancellationToken();

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock = new Mock<ISqlApplicationLock>();
			var jobToLoad = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad.WRV_JobNo = "J1";
			jobToLoad.WRV_JobType = "PIC";
			var jobToLoadWithLock = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad, sqlLock.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");

			var releaseGroupPK = ZGuid.NewZGuid();
			var capabilityCode = "PWH";
			var task = otherFactory.New<WhsReceiveProcessTasks>();
			taskFactory.Setup(t => t.CreateTask(workflowProvider.Object, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST")).Returns(task);

			var taskToCreate = new TaskToCreate(ZGuid.BrettsGuid, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST");
			var tasksToCreate = new[] { taskToCreate };
			var link = new Mock<ITaskManagementGroupingFact>();
			link.Setup(l => l.PK).Returns(jobToLoad.PK.ToGuid());
			link.Setup(l => l.AssignedTask).Returns(ZGuid.BrettsGuid.ToGuid());
			var lineFact = new Mock<ITaskManagementLineFact>();
			lineFact.Setup(l => l.PK).Returns(Guid.NewGuid());
			lineFact.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(link.Object));
			var lines = new[] { lineFact.Object };
			var tasksToCreateResult = new TasksToCreateResult(tasksToCreate, lines);

			var workflowInfo = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy = new Mock<ITaskCreationJobStrategy>();
			mockStrategy.Setup(p => p.GetWorkflowInfo(jobToLoad)).Returns(workflowInfo);
			mockStrategy.Setup(p => p.GetTasksToCreate(jobToLoad, workflowInfo, cancellationToken)).Returns(tasksToCreateResult);

			Action assertRunInTemporaryContext = () =>
			{
				AssertEquals("Should be run in a temporary user context.", newBranch.PK, Env.CurrentBranchPK);
			};

			IReadOnlyDictionary<ZGuid, ProcessTask> taskDictionary = null;
			mockStrategy.Setup(p => p.LinkTasks(jobToLoad, It.IsAny<IReadOnlyDictionary<ZGuid, ProcessTask>>(), lines))
				.Callback<WhsReadyForPlanningJobsView, IReadOnlyDictionary<ZGuid, ProcessTask>, IEnumerable<ITaskManagementLineFact>>(
					(j, td, l) =>
					{
						assertRunInTemporaryContext();

						taskDictionary = td;

						var warehouseInResultFactory = j.Factory.Load<WhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged";
					});
			mockStrategy.Setup(p => p.SetJobPlanningStatus(jobToLoad.Factory, jobToLoad, TaskPlanningStatus.Codes.Planned));

			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock, null }).Dequeue);

			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy.Object);

			var notificationsMock = new Mock<INotifications>();
			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: PICK J1."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 1 task(s)."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: PICK J1."))));

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			AssertNotNull("Should have set task dictionary.", taskDictionary);
			AssertEquals("Should setup the task dictionary properly.", task, taskDictionary[taskToCreate.ID]);

			jobLoader.Verify(srl => srl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			jobLoader.VerifyNoOtherCalls();

			mockStrategy.VerifyAll();
			mockStrategy.VerifyNoOtherCalls();

			jobStrategyFactory.VerifyAll();
			jobStrategyFactory.VerifyNoOtherCalls();

			taskFactory.VerifyAll();
			taskFactory.VerifyNoOtherCalls();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock.Verify(l => l.Dispose(), Times.Once);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should have been saved.", "NameChanged", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_BufferManagementNotEnabled()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WIN", true); // Different workflow type to the job

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationToken = new CancellationToken();

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock = new Mock<ISqlApplicationLock>();
			var jobToLoad = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad.WRV_JobNo = "J1";
			jobToLoad.WRV_JobType = "PIC";
			var jobToLoadWithLock = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad, sqlLock.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");

			var workflowInfo = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy = new Mock<ITaskCreationJobStrategy>();
			mockStrategy.Setup(p => p.GetWorkflowInfo(jobToLoad)).Returns(workflowInfo);
			mockStrategy.Setup(p => p.SetJobPlanningStatus(It.IsAny<BusinessObjectFactory>(), jobToLoad, string.Empty))
				.Callback<BusinessObjectFactory, WhsReadyForPlanningJobsView, string>(
					(f, j, s) =>
					{
						var warehouseInResultFactory = f.Load<WhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged";
					});
			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock, null }).Dequeue);

			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy.Object);

			var notificationsMock = new Mock<INotifications>();
			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: PICK J1."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Warning && x.Message.StartsWith("Buffer Management is not enabled for the Workflow Type: WOU, clearing the Task Planning Status for Job: PICK J1."))));

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			jobLoader.Verify(srl => srl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			jobLoader.VerifyNoOtherCalls();

			mockStrategy.VerifyAll();
			mockStrategy.VerifyNoOtherCalls();

			jobStrategyFactory.VerifyAll();
			jobStrategyFactory.VerifyNoOtherCalls();

			taskFactory.VerifyAll();
			taskFactory.VerifyNoOtherCalls();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock.Verify(l => l.Dispose(), Times.Once);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should have been saved.", "NameChanged", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_TaskBreakdownHalted()
			=> TestProcessQueue_TaskBreakdownHalted(fromCancellationToken: false);

		public void TestProcessQueue_TaskBreakdownHalted_CancellationToken()
			=> TestProcessQueue_TaskBreakdownHalted(fromCancellationToken: true);

		void TestProcessQueue_TaskBreakdownHalted(bool fromCancellationToken)
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock = new Mock<ISqlApplicationLock>();
			var jobToLoad = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad.WRV_JobNo = "J1";
			jobToLoad.WRV_JobType = "PIC";
			var jobToLoadWithLock = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad, sqlLock.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");

			var workflowInfo = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy = new Mock<ITaskCreationJobStrategy>();
			mockStrategy.Setup(p => p.GetWorkflowInfo(jobToLoad)).Returns(workflowInfo);
			mockStrategy.Setup(p => p.GetTasksToCreate(jobToLoad, workflowInfo, cancellationToken))
				.Returns<WhsReadyForPlanningJobsView, TaskCreationWorkflowInfo, CancellationToken>(
				(j, w, t) =>
				{
					if (fromCancellationToken)
					{
						cancellationTokenSource.Cancel();
					}

					var warehouseInResultFactory = j.Factory.Load<WhsWarehouse>(warehouse.PK);
					warehouseInResultFactory.WW_WarehouseName = "NameChanged";
					return !fromCancellationToken ? TasksToCreateResult.Halted : new TasksToCreateResult(Enumerable.Empty<TaskToCreate>(), Enumerable.Empty<ITaskManagementLineFact>());
				});

			Action assertRunInTemporaryContext = () =>
			{
				AssertEquals("Should be run in a temporary user context.", newBranch.PK, Env.CurrentBranchPK);
			};

			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock, null }).Dequeue);

			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy.Object);

			var notificationsMock = new Mock<INotifications>();
			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: PICK J1."))));

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			jobLoader.Verify(srl => srl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(fromCancellationToken ? 1 : 2));
			jobLoader.VerifyNoOtherCalls();

			mockStrategy.VerifyAll();
			mockStrategy.VerifyNoOtherCalls();

			jobStrategyFactory.VerifyAll();
			jobStrategyFactory.VerifyNoOtherCalls();

			taskFactory.VerifyAll();
			taskFactory.VerifyNoOtherCalls();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock.Verify(l => l.Dispose(), Times.Once);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertNotEquals("Factory should *not* have been saved.", "NameChanged", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_TaskBreakdownError()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationToken = new CancellationToken();

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock = new Mock<ISqlApplicationLock>();
			var jobToLoad = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad.WRV_JobNo = "J1";
			jobToLoad.WRV_JobType = "PIC";
			var jobToLoadWithLock = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad, sqlLock.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");

			var workflowInfo = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy = new Mock<ITaskCreationJobStrategy>();
			mockStrategy.Setup(p => p.GetWorkflowInfo(jobToLoad)).Returns(workflowInfo);
			mockStrategy.Setup(p => p.GetTasksToCreate(jobToLoad, workflowInfo, cancellationToken))
				.Returns<WhsReadyForPlanningJobsView, TaskCreationWorkflowInfo, CancellationToken>(
				(j, w, t) =>
				{
					var warehouseInResultFactory = j.Factory.Load<WhsWarehouse>(warehouse.PK);
					warehouseInResultFactory.WW_WarehouseName = "NameChanged";
					return new TasksToCreateResult("Rules did not work");
				});
			mockStrategy.Setup(p => p.SetJobPlanningStatus(jobToLoad.Factory, jobToLoad, TaskPlanningStatus.Codes.Error));

			Action assertRunInTemporaryContext = () =>
			{
				AssertEquals("Should be run in a temporary user context.", newBranch.PK, Env.CurrentBranchPK);
			};

			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock, null }).Dequeue);

			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy.Object);

			var notificationsMock = new Mock<INotifications>();
			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: PICK J1."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type.IsFatal && x.Message == "Error Processing Job: PICK J1, Message: Rules did not work")));

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			jobLoader.Verify(srl => srl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			jobLoader.VerifyNoOtherCalls();

			mockStrategy.VerifyAll();
			mockStrategy.VerifyNoOtherCalls();

			jobStrategyFactory.VerifyAll();
			jobStrategyFactory.VerifyNoOtherCalls();

			taskFactory.VerifyAll();
			taskFactory.VerifyNoOtherCalls();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock.Verify(l => l.Dispose(), Times.Once);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should have been saved after error status set.", "NameChanged", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_FactorySaveFails_ConcurrencyError()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationToken = new CancellationToken();

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock = new Mock<ISqlApplicationLock>();
			var jobToLoad = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad.WRV_JobNo = "J1";
			jobToLoad.WRV_JobType = "PIC";
			var jobToLoadWithLock = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad, sqlLock.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");
			var releaseGroupPK = ZGuid.NewZGuid();
			var capabilityCode = "PWH";
			var task = otherFactory.New<WhsReceiveProcessTasks>();
			taskFactory.Setup(t => t.CreateTask(workflowProvider.Object, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST")).Returns(task);

			var taskToCreate = new TaskToCreate(ZGuid.BrettsGuid, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST");
			var tasksToCreate = new[] { taskToCreate };
			var link = new Mock<ITaskManagementGroupingFact>();
			link.Setup(l => l.PK).Returns(jobToLoad.PK.ToGuid());
			link.Setup(l => l.AssignedTask).Returns(ZGuid.BrettsGuid.ToGuid());
			var lineFact = new Mock<ITaskManagementLineFact>();
			lineFact.Setup(l => l.PK).Returns(Guid.NewGuid());
			lineFact.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(link.Object));
			var lines = new[] { lineFact.Object };
			var tasksToCreateResult = new TasksToCreateResult(tasksToCreate, lines);

			var workflowInfo = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy = new Mock<ITaskCreationJobStrategy>();
			mockStrategy.Setup(p => p.GetWorkflowInfo(jobToLoad)).Returns(workflowInfo);
			mockStrategy.Setup(p => p.GetTasksToCreate(jobToLoad, workflowInfo, cancellationToken)).Returns(tasksToCreateResult);
			mockStrategy.Setup(p => p.LinkTasks(jobToLoad, It.IsAny<IReadOnlyDictionary<ZGuid, ProcessTask>>(), lines))
				.Callback<WhsReadyForPlanningJobsView, IReadOnlyDictionary<ZGuid, ProcessTask>, IEnumerable<ITaskManagementLineFact>>(
					(j, td, l) =>
					{
						var warehouseInResultFactory = j.Factory.Load<WhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged2";

						var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
						var queueInfactory2 = factory2.Load<WhsWarehouse>(warehouse.PK);
						queueInfactory2.WW_WarehouseName = $"NameChanged1";
						factory2.Save();
					});
			mockStrategy.Setup(p => p.SetJobPlanningStatus(jobToLoad.Factory, jobToLoad, TaskPlanningStatus.Codes.Planned));

			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock, null }).Dequeue);

			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy.Object);

			var notificationsMock = new Mock<INotifications>();
			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: PICK J1."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 1 task(s)."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type.IsFatal && x.Message == "Concurrency error occurred while saving the results of the processed job.")));

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			jobLoader.Verify(srl => srl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			jobLoader.VerifyNoOtherCalls();

			mockStrategy.VerifyAll();
			mockStrategy.VerifyNoOtherCalls();

			jobStrategyFactory.VerifyAll();
			jobStrategyFactory.VerifyNoOtherCalls();

			taskFactory.VerifyAll();
			taskFactory.VerifyNoOtherCalls();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock.Verify(l => l.Dispose(), Times.Once);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should *not* have been saved.", "NameChanged1", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_FactorySaveFails_OtherError()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationToken = new CancellationToken();

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock = new Mock<ISqlApplicationLock>();
			var jobToLoad = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad.WRV_JobNo = "J1";
			jobToLoad.WRV_JobType = "PIC";
			var jobToLoadWithLock = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad, sqlLock.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");
			var capabilityCode = "PWH";
			var releaseGroupPK = ZGuid.NewZGuid();
			var task = otherFactory.New<WhsReceiveProcessTasks>();
			taskFactory.Setup(t => t.CreateTask(workflowProvider.Object, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST")).Returns(task);

			var taskToCreate = new TaskToCreate(ZGuid.BrettsGuid, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST");
			var tasksToCreate = new[] { taskToCreate };
			var link = new Mock<ITaskManagementGroupingFact>();
			link.Setup(l => l.PK).Returns(jobToLoad.PK.ToGuid());
			link.Setup(l => l.AssignedTask).Returns(ZGuid.BrettsGuid.ToGuid());
			var lineFact = new Mock<ITaskManagementLineFact>();
			lineFact.Setup(l => l.PK).Returns(Guid.NewGuid());
			lineFact.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(link.Object));
			var lines = new[] { lineFact.Object };
			var tasksToCreateResult = new TasksToCreateResult(tasksToCreate, lines);

			var workflowInfo = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy = new Mock<ITaskCreationJobStrategy>();
			mockStrategy.Setup(p => p.GetWorkflowInfo(jobToLoad)).Returns(workflowInfo);
			mockStrategy.Setup(p => p.GetTasksToCreate(jobToLoad, workflowInfo, cancellationToken)).Returns(tasksToCreateResult);

			mockStrategy.Setup(p => p.LinkTasks(jobToLoad, It.IsAny<IReadOnlyDictionary<ZGuid, ProcessTask>>(), lines))
				.Callback<WhsReadyForPlanningJobsView, IReadOnlyDictionary<ZGuid, ProcessTask>, IEnumerable<ITaskManagementLineFact>>(
					(j, td, l) =>
					{
						var warehouseInResultFactory = j.Factory.Load<WhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged2";

						j.Factory.Saving += _ => throw new Exception("OTHER!");
					});

			var mockSequence = new MockSequence();
			mockStrategy.InSequence(mockSequence).Setup(p => p.SetJobPlanningStatus(jobToLoad.Factory, jobToLoad, TaskPlanningStatus.Codes.Planned));
			mockStrategy.InSequence(mockSequence).Setup(p => p.SetJobPlanningStatus(It.IsAny<BusinessObjectFactory>(), jobToLoad, TaskPlanningStatus.Codes.Error))
				.Callback<BusinessObjectFactory, WhsReadyForPlanningJobsView, string>(
					(f, j, s) =>
					{
						var warehouseInResultFactory = f.Load<WhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged3";
					});

			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock, null }).Dequeue);

			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy.Object);

			var notificationsMock = new Mock<INotifications>();
			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: PICK J1."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 1 task(s)."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type.IsFatal && x.Message == "Unexpected error occurred: OTHER!")));

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			AssertEquals("OTHER!", ErrorReporter.LastExceptionReported.Message);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			jobLoader.Verify(srl => srl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			jobLoader.VerifyNoOtherCalls();

			mockStrategy.VerifyAll();
			mockStrategy.VerifyNoOtherCalls();

			jobStrategyFactory.VerifyAll();
			jobStrategyFactory.VerifyNoOtherCalls();

			taskFactory.VerifyAll();
			taskFactory.VerifyNoOtherCalls();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock.Verify(l => l.Dispose(), Times.Once);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should have been saved after error status set.", "NameChanged3", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_ExceptionThrown()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationToken = new CancellationToken();

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock = new Mock<ISqlApplicationLock>();
			var jobToLoad = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad.WRV_JobNo = "J1";
			jobToLoad.WRV_JobType = "PIC";
			var jobToLoadWithLock = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad, sqlLock.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");
			var capabilityCode = "PWH";
			var releaseGroupPK = ZGuid.NewZGuid();
			var task = otherFactory.New<WhsReceiveProcessTasks>();
			taskFactory.Setup(t => t.CreateTask(workflowProvider.Object, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST")).Returns(task);

			var taskToCreate = new TaskToCreate(ZGuid.BrettsGuid, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST");
			var tasksToCreate = new[] { taskToCreate };
			var link = new Mock<ITaskManagementGroupingFact>();
			link.Setup(l => l.PK).Returns(jobToLoad.PK.ToGuid());
			link.Setup(l => l.AssignedTask).Returns(ZGuid.BrettsGuid.ToGuid());
			var lineFact = new Mock<ITaskManagementLineFact>();
			lineFact.Setup(l => l.PK).Returns(Guid.NewGuid());
			lineFact.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(link.Object));
			var lines = new[] { lineFact.Object };
			var tasksToCreateResult = new TasksToCreateResult(tasksToCreate, lines);

			var workflowInfo = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy = new Mock<ITaskCreationJobStrategy>();
			mockStrategy.Setup(p => p.GetWorkflowInfo(jobToLoad)).Returns(workflowInfo);
			mockStrategy.Setup(p => p.GetTasksToCreate(jobToLoad, workflowInfo, cancellationToken)).Returns(tasksToCreateResult);

			mockStrategy.Setup(p => p.LinkTasks(jobToLoad, It.IsAny<IReadOnlyDictionary<ZGuid, ProcessTask>>(), lines))
				.Callback<WhsReadyForPlanningJobsView, IReadOnlyDictionary<ZGuid, ProcessTask>, IEnumerable<ITaskManagementLineFact>>(
					(j, td, l) =>
					{
						var warehouseInResultFactory = j.Factory.Load<WhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged2";

						throw new Exception("OTHER!");
					});

			mockStrategy.Setup(p => p.SetJobPlanningStatus(It.IsAny<BusinessObjectFactory>(), jobToLoad, TaskPlanningStatus.Codes.Error))
				.Callback<BusinessObjectFactory, WhsReadyForPlanningJobsView, string>(
					(f, j, s) =>
					{
						var warehouseInResultFactory = f.Load<WhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged3";
					});

			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock, null }).Dequeue);

			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy.Object);

			var notificationsMock = new Mock<INotifications>();
			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: PICK J1."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 1 task(s)."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type.IsFatal && x.Message == "Unexpected error occurred: OTHER!")));

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			AssertEquals("OTHER!", ErrorReporter.LastExceptionReported.Message);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			jobLoader.Verify(srl => srl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			jobLoader.VerifyNoOtherCalls();

			mockStrategy.VerifyAll();
			mockStrategy.VerifyNoOtherCalls();

			jobStrategyFactory.VerifyAll();
			jobStrategyFactory.VerifyNoOtherCalls();

			taskFactory.VerifyAll();
			taskFactory.VerifyNoOtherCalls();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock.Verify(l => l.Dispose(), Times.Once);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should have been saved after error status set.", "NameChanged3", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_UnmatchedStrategy()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationToken = new CancellationToken();
			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock = new Mock<ISqlApplicationLock>();
			var jobToLoad = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad.WRV_JobNo = "J1";
			jobToLoad.WRV_JobType = "LOL";

			var jobToLoadWithLock = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad, sqlLock.Object);
			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock, null }).Dequeue);

			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();

			var notificationsMock = new Mock<INotifications>();
			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			AssertEquals("Unexpected job type 'LOL'.", ErrorReporter.LastExceptionReported.Message);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestProcessQueue_GetTasksToCreate_CancellationExceptionThrown_ThisToken()
			=> TestProcessQueue_GetTasksToCreate_CancellationExceptionThrown(forCurrentCancellationToken: true);

		public void TestProcessQueue_GetTasksToCreate_CancellationExceptionThrown_OtherToken()
			=> TestProcessQueue_GetTasksToCreate_CancellationExceptionThrown(forCurrentCancellationToken: false);

		void TestProcessQueue_GetTasksToCreate_CancellationExceptionThrown(bool forCurrentCancellationToken)
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationToken = new CancellationTokenSource().Token;

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock = new Mock<ISqlApplicationLock>();
			var jobToLoad = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad.WRV_JobNo = "J1";
			jobToLoad.WRV_JobType = "PIC";
			var jobToLoadWithLock = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad, sqlLock.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");
			var capabilityCode = "PWH";
			var releaseGroupPK = ZGuid.NewZGuid();
			var task = otherFactory.New<WhsReceiveProcessTasks>();
			taskFactory.Setup(t => t.CreateTask(workflowProvider.Object, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST")).Returns(task);

			var taskToCreate = new TaskToCreate(ZGuid.BrettsGuid, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST");
			var tasksToCreate = new[] { taskToCreate };
			var link = new Mock<ITaskManagementGroupingFact>();
			link.Setup(l => l.PK).Returns(jobToLoad.PK.ToGuid());
			link.Setup(l => l.AssignedTask).Returns(ZGuid.BrettsGuid.ToGuid());
			var lineFact = new Mock<ITaskManagementLineFact>();
			lineFact.Setup(l => l.PK).Returns(Guid.NewGuid());
			lineFact.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(link.Object));
			var lines = new[] { lineFact.Object };
			var tasksToCreateResult = new TasksToCreateResult(tasksToCreate, lines);

			var exceptionToThrow = forCurrentCancellationToken ? new OperationCanceledException(cancellationToken) : new OperationCanceledException();
			var workflowInfo = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy = new Mock<ITaskCreationJobStrategy>();
			mockStrategy.Setup(p => p.GetWorkflowInfo(jobToLoad)).Returns(workflowInfo);
			mockStrategy.Setup(p => p.GetTasksToCreate(jobToLoad, workflowInfo, cancellationToken))
					.Callback<WhsReadyForPlanningJobsView, TaskCreationWorkflowInfo, CancellationToken>((j, w, c) => throw exceptionToThrow);

			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock, null }).Dequeue);

			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy.Object);

			var notificationsMock = new Mock<INotifications>();
			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: PICK J1."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 1 task(s)."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: PICK J1."))));

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);

			if (forCurrentCancellationToken)
			{
				AssertExceptionThrown<OperationCanceledException>(() => processor.ProcessQueue(notificationsMock.Object, cancellationToken));
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
			else
			{
				processor.ProcessQueue(notificationsMock.Object, cancellationToken);
				AssertEquals(exceptionToThrow, ErrorReporter.LastExceptionReported);
				AssertEquals(1, ErrorReporter.TotalErrorCount);
			}

			ErrorReporter.Clear();
		}

		public void TestProcessQueue_LinkTasks_CancellationExceptionThrown_ThisToken()
			=> TestProcessQueue_LinkTasks_CancellationExceptionThrown(forCurrentCancellationToken: true);

		public void TestProcessQueue_LinkTasks_CancellationExceptionThrown_OtherToken()
			=> TestProcessQueue_LinkTasks_CancellationExceptionThrown(forCurrentCancellationToken: false);

		void TestProcessQueue_LinkTasks_CancellationExceptionThrown(bool forCurrentCancellationToken)
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationToken = new CancellationTokenSource().Token;

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock = new Mock<ISqlApplicationLock>();
			var jobToLoad = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad.WRV_JobNo = "J1";
			jobToLoad.WRV_JobType = "PIC";
			var jobToLoadWithLock = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad, sqlLock.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");
			var capabilityCode = "PWH";
			var releaseGroupPK = ZGuid.NewZGuid();
			var task = otherFactory.New<WhsReceiveProcessTasks>();
			taskFactory.Setup(t => t.CreateTask(workflowProvider.Object, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST")).Returns(task);

			var taskToCreate = new TaskToCreate(ZGuid.BrettsGuid, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST");
			var tasksToCreate = new[] { taskToCreate };
			var link = new Mock<ITaskManagementGroupingFact>();
			link.Setup(l => l.PK).Returns(jobToLoad.PK.ToGuid());
			link.Setup(l => l.AssignedTask).Returns(ZGuid.BrettsGuid.ToGuid());
			var lineFact = new Mock<ITaskManagementLineFact>();
			lineFact.Setup(l => l.PK).Returns(Guid.NewGuid());
			lineFact.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(link.Object));
			var lines = new[] { lineFact.Object };
			var tasksToCreateResult = new TasksToCreateResult(tasksToCreate, lines);

			var exceptionToThrow = forCurrentCancellationToken ? new OperationCanceledException(cancellationToken) : new OperationCanceledException();
			var workflowInfo = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy = new Mock<ITaskCreationJobStrategy>();
			mockStrategy.Setup(p => p.GetWorkflowInfo(jobToLoad)).Returns(workflowInfo);
			mockStrategy.Setup(p => p.GetTasksToCreate(jobToLoad, workflowInfo, cancellationToken)).Returns(tasksToCreateResult);
			mockStrategy.Setup(p => p.LinkTasks(jobToLoad, It.IsAny<IReadOnlyDictionary<ZGuid, ProcessTask>>(), lines))
				.Callback<WhsReadyForPlanningJobsView, IReadOnlyDictionary<ZGuid, ProcessTask>, IEnumerable<ITaskManagementLineFact>>(
					(j, td, l) => throw exceptionToThrow);

			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock, null }).Dequeue);

			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy.Object);

			var notificationsMock = new Mock<INotifications>();
			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: PICK J1."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 1 task(s)."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: PICK J1."))));

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);

			if (forCurrentCancellationToken)
			{
				AssertExceptionThrown<OperationCanceledException>(() => processor.ProcessQueue(notificationsMock.Object, cancellationToken));
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
			else
			{
				processor.ProcessQueue(notificationsMock.Object, cancellationToken);
				AssertEquals(exceptionToThrow, ErrorReporter.LastExceptionReported);
				AssertEquals(1, ErrorReporter.TotalErrorCount);
			}

			ErrorReporter.Clear();
		}

		public void TestProcessQueue_MultipleJobs()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationToken = new CancellationToken();

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock1 = new Mock<ISqlApplicationLock>();
			var jobToLoad1 = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad1.WRV_JobNo = "J1";
			jobToLoad1.WRV_JobType = "PIC";
			var jobToLoadWithLock1 = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad1, sqlLock1.Object);

			var sqlLock2 = new Mock<ISqlApplicationLock>();
			var jobToLoad2 = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad2.WRV_JobNo = "J2";
			jobToLoad2.WRV_JobType = "INW";
			var jobToLoadWithLock2 = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad2, sqlLock2.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");
			var capabilityCode = "PWH";
			var releaseGroupPK = ZGuid.NewZGuid();
			var task = otherFactory.New<WhsReceiveProcessTasks>();
			taskFactory.Setup(t => t.CreateTask(workflowProvider.Object, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST")).Returns(task);
			taskFactory.Setup(t => t.CreateTask(workflowProvider.Object, "TES", "WNAME2", "TNAME2", "BRS", 42, capabilityCode, releaseGroupPK, "TST")).Returns(task);
			taskFactory.Setup(t => t.CreateTask(workflowProvider.Object, "TES", "WNAME3", "TNAME3", "MMC", 42, capabilityCode, releaseGroupPK, "TST")).Returns(task);

			var taskToCreate1 = new TaskToCreate(ZGuid.BrettsGuid, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST");
			var tasksToCreate1 = new[] { taskToCreate1 };
			var link1 = new Mock<ITaskManagementGroupingFact>();
			link1.Setup(l => l.PK).Returns(jobToLoad1.PK.ToGuid());
			link1.Setup(l => l.AssignedTask).Returns(ZGuid.BrettsGuid.ToGuid());
			var line1Fact = new Mock<ITaskManagementLineFact>();
			line1Fact.Setup(l => l.PK).Returns(Guid.NewGuid());
			line1Fact.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(link1.Object));
			var lines1 = new[] { line1Fact.Object };
			var tasksToCreateResult1 = new TasksToCreateResult(tasksToCreate1, lines1);

			var workflowInfo1 = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy1 = new Mock<ITaskCreationJobStrategy>();
			mockStrategy1.Setup(p => p.GetWorkflowInfo(jobToLoad1)).Returns(workflowInfo1);
			mockStrategy1.Setup(p => p.GetTasksToCreate(jobToLoad1, workflowInfo1, cancellationToken)).Returns(tasksToCreateResult1);
			mockStrategy1.Setup(p => p.LinkTasks(jobToLoad1, It.IsAny<IReadOnlyDictionary<ZGuid, ProcessTask>>(), lines1))
				.Callback<WhsReadyForPlanningJobsView, IReadOnlyDictionary<ZGuid, ProcessTask>, IEnumerable<ITaskManagementLineFact>>(
					(j, td, l) =>
					{
						var warehouseInResultFactory = j.Factory.Load<WhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged1";
					});
			mockStrategy1.Setup(p => p.SetJobPlanningStatus(jobToLoad1.Factory, jobToLoad1, TaskPlanningStatus.Codes.Planned));

			var taskToCreate2 = new TaskToCreate(ZGuid.BrettsGuid, "TES", "WNAME2", "TNAME2", "BRS", 42, capabilityCode, releaseGroupPK, "TST");
			var taskToCreate3 = new TaskToCreate(ZGuid.NewZGuid(), "TES", "WNAME3", "TNAME3", "MMC", 42, capabilityCode, releaseGroupPK, "TST");
			var tasksToCreate2 = new[] { taskToCreate2, taskToCreate3 };
			var link2 = new Mock<ITaskManagementGroupingFact>();
			link2.Setup(l => l.PK).Returns(jobToLoad2.PK.ToGuid());
			link2.Setup(l => l.AssignedTask).Returns(ZGuid.BrettsGuid.ToGuid());
			var line2Fact = new Mock<ITaskManagementLineFact>();
			line2Fact.Setup(l => l.PK).Returns(Guid.NewGuid());
			line2Fact.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(link2.Object));
			var lines2 = new[] { line2Fact.Object };
			var tasksToCreateResult2 = new TasksToCreateResult(tasksToCreate2, lines2);

			var workflowInfo2 = new TaskCreationWorkflowInfo("UNLOAD J2", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy2 = new Mock<ITaskCreationJobStrategy>();
			mockStrategy2.Setup(p => p.GetWorkflowInfo(jobToLoad2)).Returns(workflowInfo2);
			mockStrategy2.Setup(p => p.GetTasksToCreate(jobToLoad2, workflowInfo2, cancellationToken)).Returns(tasksToCreateResult2);
			mockStrategy2.Setup(p => p.LinkTasks(jobToLoad2, It.IsAny<IReadOnlyDictionary<ZGuid, ProcessTask>>(), lines2))
				.Callback<WhsReadyForPlanningJobsView, IReadOnlyDictionary<ZGuid, ProcessTask>, IEnumerable<ITaskManagementLineFact>>(
					(j, td, l) =>
					{
						var warehouseInResultFactory = j.Factory.Load<WhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged2";
					});
			mockStrategy2.Setup(p => p.SetJobPlanningStatus(jobToLoad2.Factory, jobToLoad2, TaskPlanningStatus.Codes.Planned));

			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock1, jobToLoadWithLock2, null }).Dequeue);

			var strategySequence = new MockSequence();
			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.InSequence(strategySequence).Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy1.Object);
			jobStrategyFactory.InSequence(strategySequence).Setup(jsf => jsf.GetJobStrategy("INW")).Returns(mockStrategy2.Object);

			var notificationsMock = new Mock<INotifications>();
			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: PICK J1."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 1 task(s)."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: PICK J1."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: UNLOAD J2."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 2 task(s)."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: UNLOAD J2."))));

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			jobLoader.Verify(srl => srl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(3));
			jobLoader.VerifyNoOtherCalls();

			jobStrategyFactory.VerifyAll();
			jobStrategyFactory.VerifyNoOtherCalls();

			mockStrategy1.VerifyAll();
			mockStrategy1.VerifyNoOtherCalls();

			taskFactory.VerifyAll();
			taskFactory.VerifyNoOtherCalls();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock1.Verify(l => l.Dispose(), Times.Once);
			sqlLock2.Verify(l => l.Dispose(), Times.Once);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should have been saved.", "NameChanged2", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_MultipleJobs_Cancelled()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock1 = new Mock<ISqlApplicationLock>();
			var jobToLoad1 = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad1.WRV_JobNo = "J1";
			jobToLoad1.WRV_JobType = "PIC";
			var jobToLoadWithLock1 = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad1, sqlLock1.Object);

			var sqlLock2 = new Mock<ISqlApplicationLock>();
			var jobToLoad2 = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad2.WRV_JobNo = "J2";
			jobToLoad2.WRV_JobType = "INW";
			var jobToLoadWithLock2 = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad2, sqlLock2.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");
			var capabilityCode = "PWH";
			var releaseGroupPK = ZGuid.NewZGuid();
			var task = otherFactory.New<WhsReceiveProcessTasks>();
			taskFactory.Setup(t => t.CreateTask(workflowProvider.Object, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST")).Returns(task);

			var taskToCreate = new TaskToCreate(ZGuid.BrettsGuid, "TES", "WNAME", "TNAME", "BRS", 42, capabilityCode, releaseGroupPK, "TST");
			var tasksToCreate = new[] { taskToCreate };
			var link = new Mock<ITaskManagementGroupingFact>();
			link.Setup(l => l.PK).Returns(jobToLoad1.PK.ToGuid());
			link.Setup(l => l.AssignedTask).Returns(ZGuid.BrettsGuid.ToGuid());
			var lineFact = new Mock<ITaskManagementLineFact>();
			lineFact.Setup(l => l.PK).Returns(Guid.NewGuid());
			lineFact.Setup(l => l.Grouping).Returns(new FactJoin<ITaskManagementGroupingFact>(link.Object));
			var lines = new[] { lineFact.Object };
			var tasksToCreateResult = new TasksToCreateResult(tasksToCreate, lines);

			var workflowInfo = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy1 = new Mock<ITaskCreationJobStrategy>();
			mockStrategy1.Setup(p => p.GetWorkflowInfo(jobToLoad1)).Returns(workflowInfo);
			mockStrategy1.Setup(p => p.GetTasksToCreate(jobToLoad1, workflowInfo, cancellationToken)).Returns(tasksToCreateResult);
			mockStrategy1.Setup(p => p.LinkTasks(jobToLoad1, It.IsAny<IReadOnlyDictionary<ZGuid, ProcessTask>>(), lines))
				.Callback<WhsReadyForPlanningJobsView, IReadOnlyDictionary<ZGuid, ProcessTask>, IEnumerable<ITaskManagementLineFact>>(
					(j, td, l) =>
					{
						var warehouseInResultFactory = j.Factory.Load<WhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged";

						cancellationTokenSource.Cancel();
					});
			mockStrategy1.Setup(p => p.SetJobPlanningStatus(jobToLoad1.Factory, jobToLoad1, TaskPlanningStatus.Codes.Planned));

			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock1, jobToLoadWithLock2, null }).Dequeue);

			var strategySequence = new MockSequence();
			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.InSequence(strategySequence).Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy1.Object);

			var notificationsMock = new Mock<INotifications>();

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			jobLoader.Verify(srl => srl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(1));
			jobLoader.VerifyNoOtherCalls();

			mockStrategy1.VerifyAll();
			mockStrategy1.VerifyNoOtherCalls();

			jobStrategyFactory.VerifyAll();
			jobStrategyFactory.VerifyNoOtherCalls();

			taskFactory.VerifyAll();
			taskFactory.VerifyNoOtherCalls();

			sqlLock1.Verify(l => l.Dispose(), Times.Once);
			sqlLock2.Verify(l => l.Dispose(), Times.Never);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should have been saved.", "NameChanged", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_NoTasks()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WOU", true);

			var warehouse = Helper.CreateWarehouse("W1");
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var testCurrentBranch = Env.Instance.CurrentBranch;
			var cancellationToken = new CancellationToken();

			var taskFactory = new Mock<IWhsTaskFactory>();

			var otherFactory = new BusinessObjectFactory();
			var sqlLock = new Mock<ISqlApplicationLock>();
			var jobToLoad = otherFactory.New<WhsReadyForPlanningJobsView>();
			jobToLoad.WRV_JobNo = "J1";
			jobToLoad.WRV_JobType = "PIC";
			var jobToLoadWithLock = new AppLockedItem<WhsReadyForPlanningJobsView>(jobToLoad, sqlLock.Object);

			var workflowProvider = new Mock<IWorkflowProvider>();
			workflowProvider.Setup(wf => wf.WorkflowType).Returns("WOU");
			var tasksToCreateResult = new TasksToCreateResult(Enumerable.Empty<TaskToCreate>(), Enumerable.Empty<ITaskManagementLineFact>());
			var workflowInfo = new TaskCreationWorkflowInfo("PICK J1", newBranch.PK, warehouse.PK, ZGuid.BrettsGuid, workflowProvider.Object);
			var mockStrategy = new Mock<ITaskCreationJobStrategy>();
			mockStrategy.Setup(p => p.GetWorkflowInfo(jobToLoad)).Returns(workflowInfo);
			mockStrategy.Setup(p => p.GetTasksToCreate(jobToLoad, workflowInfo, cancellationToken)).Returns(tasksToCreateResult);

			mockStrategy.Setup(p => p.LinkTasks(jobToLoad, It.IsAny<IReadOnlyDictionary<ZGuid, ProcessTask>>(), It.IsAny<IEnumerable<ITaskManagementLineFact>>()))
				.Callback<WhsReadyForPlanningJobsView, IReadOnlyDictionary<ZGuid, ProcessTask>, IEnumerable<ITaskManagementLineFact>>(
					(j, td, l) =>
					{
						var warehouseInResultFactory = j.Factory.Load<WhsWarehouse>(warehouse.PK);
						warehouseInResultFactory.WW_WarehouseName = "NameChanged";
					});
			mockStrategy.Setup(p => p.SetJobPlanningStatus(jobToLoad.Factory, jobToLoad, TaskPlanningStatus.Codes.Planned));

			var jobLoader = new Mock<IReadyForPlanningJobLoader>();
			jobLoader.Setup(jl => jl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>())).Returns(new Queue<AppLockedItem<WhsReadyForPlanningJobsView>>(new[] { jobToLoadWithLock, null }).Dequeue);

			var jobStrategyFactory = new Mock<ITaskCreationJobStrategyFactory>();
			jobStrategyFactory.Setup(jsf => jsf.GetJobStrategy("PIC")).Returns(mockStrategy.Object);

			var notificationsMock = new Mock<INotifications>();
			var notificationSequence = new MockSequence();
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: PICK J1."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 0 task(s)."))));
			notificationsMock.InSequence(notificationSequence).Setup(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: PICK J1."))));

			var processor = new WhsTaskCreationProcessor(jobLoader.Object, jobStrategyFactory.Object, taskFactory.Object);
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			jobLoader.Verify(srl => srl.GetNextJobToProcess(It.IsAny<BusinessObjectFactory>()), Times.Exactly(2));
			jobLoader.VerifyNoOtherCalls();

			mockStrategy.VerifyAll();
			mockStrategy.VerifyNoOtherCalls();

			jobStrategyFactory.VerifyAll();
			jobStrategyFactory.VerifyNoOtherCalls();

			taskFactory.VerifyNoOtherCalls();

			notificationsMock.VerifyAll();
			notificationsMock.VerifyNoOtherCalls();

			sqlLock.Verify(l => l.Dispose(), Times.Once);
			AssertEquals("CurrentBranch was restored", testCurrentBranch, Env.Instance.CurrentBranch);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var warehouse_InNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertEquals("Factory should have been saved.", "NameChanged", warehouse_InNewFactory.WW_WarehouseName);
		}

		public void TestProcessQueue_EndToEnd_Unload() => TestProcessQueue_EndToEnd_Unload(warehouseOnRuleSet: false);
		public void TestProcessQueue_EndToEnd_Unload_WarehouseOnRuleSet() => TestProcessQueue_EndToEnd_Unload(warehouseOnRuleSet: true);

		void TestProcessQueue_EndToEnd_Unload(bool warehouseOnRuleSet)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WIN", true);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "PWH";
			capability.G4_Description = "Product Warehouse";
			Factory.Save();

			var ruleHelper = new Helper(Factory);
			var ruleSet = ruleHelper.CreateRuleSet("Breakdown", "Breakdown", context: "PWB", contextSubType: "ULD", warehousePK: warehouseOnRuleSet ? data.Whs1.PK : null);
			var rule = ruleHelper.CreateRule(ruleSet, "Rule 1", "Rule 1");
			rule.PRL_RuleDefinition = $@"
{{
	""conditions"": [
	],
	""action"":	{{
		""$type"": ""BreakdownUnloadTaskActionState"",
		""sortByCriteria"": [
		],
		""groupByCriteria"": [
		],
		""capability"": ""PWH"",
		""maxLinesPerTask"": 2
	}}
}}".TrimStart();
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, allocateLocations: false, finalise: false);
			var receiveLine1 = receive.Lines[0];
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals("Precondition: ReadyForPlanning", TaskPlanningStatus.Codes.Ready, receive.WD_TaskPlanningStatus);
			Factory.Save();

			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();
			var processor = ObjectFactory.Get<IWhsTaskCreationProcessor>();
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: Warehouse Receipt W00000001."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 2 task(s)."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: Warehouse Receipt W00000001."))));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, receiveInNewFactory.WD_TaskPlanningStatus);

			var tasks = receiveInNewFactory.WorkflowItems.Where(t => !t.P9_FormFlowType.IsEmpty).OrderBy(t => t.P9_Sequence).ToArray();
			AssertEquals("Should have created two tasks.", 2, tasks.Length);

			var sequence = 100;
			foreach (var task in tasks)
			{
				AssertEquals("Should have set formflow type.", WarehouseTaskFormFlowTypes.UnloadJob, task.P9_FormFlowType);
				AssertEquals("Should have set capability.", capability.PK, task.P9_G4_RequiredCapability);
				AssertEquals("Should have set nudge.", "100", task.EffectiveTaskNudge);
				AssertEquals("Should have set sequence.", sequence, task.P9_Sequence);
				AssertEquals("Should have set task name.", "Unload", task.P9_Description);

				sequence += 100;
			}
		}

		public void TestProcessQueue_EndToEnd_Load() => TestProcessQueue_EndToEnd_Load(warehouseOnRuleSet: false);
		public void TestProcessQueue_EndToEnd_Load_WarehouseOnRuleSet() => TestProcessQueue_EndToEnd_Load(warehouseOnRuleSet: true);

		void TestProcessQueue_EndToEnd_Load(bool warehouseOnRuleSet)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;

			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WLO", true);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "PWH";
			capability.G4_Description = "Product Warehouse";
			Factory.Save();

			var ruleHelper = new Helper(Factory);
			var ruleSet = ruleHelper.CreateRuleSet("Breakdown", "Breakdown", context: "PWB", contextSubType: "LOA", warehousePK: warehouseOnRuleSet ? data.Whs1.PK : null);
			var rule = ruleHelper.CreateRule(ruleSet, "Rule 1", "Rule 1");
			rule.PRL_RuleDefinition = $@"
{{
	""conditions"": [
	],
	""action"":	{{
		""$type"": ""BreakdownLoadTaskActionState"",
		""sortByCriteria"": [
		],
		""groupByCriteria"": [
		],
		""capability"": ""PWH"",
		""maxPackagesPerTask"": 2
	}}
}}".TrimStart();
			Factory.Save();

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", carrierServiceLevel: "RD");
			load.WLO_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.ConsigneePK = data.Org1.PK;
			order.WD_WLO_PlannedLoad = load.PK;

			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			var package2 = packingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			var package3 = packingHelper.CreatePackage(packageJob, "PKG3", 1, PkgUnit.Box);
			package1.Pack(order.Lines[0].ReleaseLines[0], 3m);
			package2.Pack(order.Lines[0].ReleaseLines[0], 3m);
			package3.Pack(order.Lines[0].ReleaseLines[0], 4m);
			Factory.Save();

			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();
			var processor = ObjectFactory.Get<IWhsTaskCreationProcessor>();
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: Load Planning L1."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 2 task(s)."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: Load Planning L1."))));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadInNewFactory = newFactory.Load<WhsLoad>(load.PK);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, loadInNewFactory.WLO_TaskPlanningStatus);

			var tasks = loadInNewFactory.WorkflowItems.Where(t => !t.P9_FormFlowType.IsEmpty).OrderBy(t => t.P9_Sequence).ToArray();
			AssertEquals("Should have created two tasks.", 2, tasks.Length);

			var sequence = 100;
			foreach (var task in tasks)
			{
				AssertEquals("Should have set formflow type.", WarehouseTaskFormFlowTypes.LoadJob, task.P9_FormFlowType);
				AssertEquals("Should have set capability.", capability.PK, task.P9_G4_RequiredCapability);
				AssertEquals("Should have set nudge.", "200", task.EffectiveTaskNudge);
				AssertEquals("Should have set sequence.", sequence, task.P9_Sequence);
				AssertEquals("Should have set task name.", "Load Packages", task.P9_Description);

				sequence += 100;
			}
		}

		public void TestProcessQueue_EndToEnd_CycleCountLocation() => TestProcessQueue_EndToEnd_CycleCountLocation(warehouseOnRuleSet: false);
		public void TestProcessQueue_EndToEnd_CycleCountLocation_WarehouseOnRuleSet() => TestProcessQueue_EndToEnd_CycleCountLocation(warehouseOnRuleSet: true);

		void TestProcessQueue_EndToEnd_CycleCountLocation(bool warehouseOnRuleSet)
		{
			var data = new TestDataSimpleEnvironment(Factory, 8, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;

			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, WorkflowDescriptors.WhsCycleCountWaveWorkflowDescriptorCode, true);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "PWH";
			capability.G4_Description = "Product Warehouse";
			Factory.Save();

			var ruleHelper = new Helper(Factory);
			var ruleSet = ruleHelper.CreateRuleSet("Breakdown", "Breakdown", context: "PWB", contextSubType: "CC", warehousePK: warehouseOnRuleSet ? data.Whs1.PK : null);
			var rule = ruleHelper.CreateRule(ruleSet, "Rule 1", "Rule 1");
			rule.PRL_RuleDefinition = $@"
{{
	""conditions"": [
	],
	""action"":	{{
		""$type"": ""BreakdownCycleCountTaskActionState"",
		""sortByCriteria"": [
          {{
            ""propertyPath"": ""Priority"",
            ""direction"": ""ascending""
          }}       
		],
		""groupByCriteria"": [
		],
		""capability"": ""PWH"",
		""maxLinesPerTask"": 0
	}}
}}".TrimStart();
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var location4 = data.Whs1.FindLocation("A-4");
			var location5 = data.Whs1.FindLocation("A-5");
			var location6 = data.Whs1.FindLocation("A-6");
			var location7 = data.Whs1.FindLocation("A-7");
			var location8 = data.Whs1.FindLocation("A-8");
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount3 = Helper.CreateWhsCycleCountLocation(location3, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount4 = Helper.CreateWhsCycleCountLocation(location4, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount5 = Helper.CreateWhsCycleCountLocation(location5, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount6 = Helper.CreateWhsCycleCountLocation(location6, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount7 = Helper.CreateWhsCycleCountLocation(location7, CycleCountGranularity.Codes.ProductWithAttributes);
			var cycleCount8 = Helper.CreateWhsCycleCountLocation(location8, CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount1.WCL_Priority = 19; // Task 2
			cycleCount2.WCL_Priority = 5; // Task 1
			cycleCount3.WCL_Priority = 4; // Task 1
			cycleCount4.WCL_Priority = 1; // Task 1
			cycleCount5.WCL_Priority = 2; // Task 1
			cycleCount6.WCL_Priority = 0; // Task 2
			cycleCount7.WCL_Priority = 3; // Task 1
			cycleCount1.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount2.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount3.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount4.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount5.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount6.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount7.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			cycleCount8.WCL_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			Factory.Save();

			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();
			var processor = ObjectFactory.Get<IWhsTaskCreationProcessor>();
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: Cycle Count Tasks For Warehouse 1."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 2 task(s)."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: Cycle Count Tasks For Warehouse 1."))));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var cycleCount1InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount1.PK);
			var cycleCount2InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount2.PK);
			var cycleCount3InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount3.PK);
			var cycleCount4InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount4.PK);
			var cycleCount5InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount5.PK);
			var cycleCount6InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount6.PK);
			var cycleCount7InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount7.PK);
			var cycleCount8InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount8.PK);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, cycleCount1InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, cycleCount2InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, cycleCount3InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, cycleCount4InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, cycleCount5InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, cycleCount6InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, cycleCount7InNewFactory.WCL_TaskPlanningStatus);
			AssertEquals("Should *not* be planned", TaskPlanningStatus.Codes.NotReady, cycleCount8InNewFactory.WCL_TaskPlanningStatus);

			var cycleCountWaveInNewFactory = Factory.LoadTop1<WhsCycleCountWave>(new ZQuery());
			var tasks = cycleCountWaveInNewFactory.WorkflowItems.Where(t => !t.P9_FormFlowType.IsEmpty).OrderBy(t => t.P9_Sequence).ToArray();
			AssertEquals("Should have created two tasks.", 2, tasks.Length);

			var sequence = 100;
			foreach (var task in tasks)
			{
				AssertEquals("Should have set formflow type.", WarehouseTaskFormFlowTypes.CycleCountJob, task.P9_FormFlowType);
				AssertEquals("Should have set capability.", capability.PK, task.P9_G4_RequiredCapability);
				AssertEquals("Should have set nudge.", "0", task.EffectiveTaskNudge);
				AssertEquals("Should have set sequence.", sequence, task.P9_Sequence);
				AssertEquals("Should have set task name.", "Count Locations", task.P9_Description);

				sequence += 100;
			}

			AssertEquals("Should be linked.", tasks[1].PK, cycleCount1InNewFactory.WCL_P9_Task);
			AssertEquals("Should be linked.", tasks[0].PK, cycleCount2InNewFactory.WCL_P9_Task);
			AssertEquals("Should be linked.", tasks[0].PK, cycleCount3InNewFactory.WCL_P9_Task);
			AssertEquals("Should be linked.", tasks[0].PK, cycleCount4InNewFactory.WCL_P9_Task);
			AssertEquals("Should be linked.", tasks[0].PK, cycleCount5InNewFactory.WCL_P9_Task);
			AssertEquals("Should be linked.", tasks[1].PK, cycleCount6InNewFactory.WCL_P9_Task);
			AssertEquals("Should be linked.", tasks[0].PK, cycleCount7InNewFactory.WCL_P9_Task);
		}

		public void TestProcessQueue_EndToEnd_Transfer() => TestProcessQueue_EndToEnd_Transfer(warehouseOnRuleSet: false, replenishment: false);
		public void TestProcessQueue_EndToEnd_Transfer_WarehouseOnRuleSet() => TestProcessQueue_EndToEnd_Transfer(warehouseOnRuleSet: true, replenishment: false);

		public void TestProcessQueue_EndToEnd_Replenishment() => TestProcessQueue_EndToEnd_Transfer(warehouseOnRuleSet: false, replenishment: true);
		public void TestProcessQueue_EndToEnd_Replenishment_WarehouseOnRuleSet() => TestProcessQueue_EndToEnd_Transfer(warehouseOnRuleSet: true, replenishment: true);

		void TestProcessQueue_EndToEnd_Transfer(bool warehouseOnRuleSet, bool replenishment)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;

			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, WorkflowDescriptors.WhsTransferWorkflowDescriptorCode, true);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "PWH";
			capability.G4_Description = "Product Warehouse";
			Factory.Save();

			var ruleHelper = new Helper(Factory);
			var ruleSet = ruleHelper.CreateRuleSet("Breakdown", "Breakdown", context: "PWB", contextSubType: "TFR", warehousePK: warehouseOnRuleSet ? data.Whs1.PK : null);
			var rule = ruleHelper.CreateRule(ruleSet, "Rule 1", "Rule 1");
			rule.PRL_RuleDefinition = $@"
{{
	""conditions"": [
	],
	""action"":	{{
		""$type"": ""BreakdownTransferTaskActionState"",
		""sortByCriteria"": [
          {{
            ""propertyPath"": ""ToPalletID"",
            ""direction"": ""ascending""
          }}       
		],
		""groupByCriteria"": [
		],
		""capability"": ""PWH"",
		""maxLinesPerTask"": 1
	}}
}}".TrimStart();
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				allocateLocations: false, finalise: false);
			var receiveLine1 = receive.Lines[0];
			receiveLine1.WE_WL = location1.PK;

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			transfer.WD_IsPickFaceReplenishment = replenishment;

			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, string.Empty, location2.WLV_LocationString, "456");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.WLV_LocationString, string.Empty, location2.WLV_LocationString, "123");
			transferLine1.RunPreSaveValidation();
			transferLine2.RunPreSaveValidation();
			Factory.Save();

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();
			var processor = ObjectFactory.Get<IWhsTaskCreationProcessor>();
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: Warehouse Transfer W00000002."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 2 task(s)."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: Warehouse Transfer W00000002."))));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);
			var transferLine1InNewFactory = newFactory.Load<WhsTransferLine>(transferLine1.PK);
			var transferLine2InNewFactory = newFactory.Load<WhsTransferLine>(transferLine2.PK);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, transferInNewFactory.WD_TaskPlanningStatus);

			var tasks = transferInNewFactory.WorkflowItems.Where(t => !t.P9_FormFlowType.IsEmpty).OrderBy(t => t.P9_Sequence).ToArray();
			AssertEquals("Should have created two tasks.", 2, tasks.Length);

			var sequence = 100;
			foreach (var task in tasks)
			{
				AssertEquals("Should have set formflow type.", replenishment ? WarehouseTaskFormFlowTypes.ReplenishmentJob : WarehouseTaskFormFlowTypes.TransferJob, task.P9_FormFlowType);
				AssertEquals("Should have set capability.", capability.PK, task.P9_G4_RequiredCapability);
				AssertEquals("Should have set nudge.", (replenishment ? 200 : 100).ToString(), task.EffectiveTaskNudge);
				AssertEquals("Should have set sequence.", sequence, task.P9_Sequence);
				AssertEquals("Should have set task name.", replenishment ? "Replenishment" : "Transfer", task.P9_Description);

				sequence += 100;
			}

			AssertEquals("Should be linked.", tasks[0].PK, transferLine2InNewFactory.WE_P9_Task);
			AssertEquals("Should be linked.", tasks[1].PK, transferLine1InNewFactory.WE_P9_Task);
		}

		public void TestProcessQueue_EndToEnd_Pick() => TestProcessQueue_EndToEnd_Pick(warehouseOnRuleSet: false);
		public void TestProcessQueue_EndToEnd_Pick_WarehouseOnRuleSet() => TestProcessQueue_EndToEnd_Pick(warehouseOnRuleSet: true);

		void TestProcessQueue_EndToEnd_Pick(bool warehouseOnRuleSet)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_NumberOfCycleCountLocationsToAutoAssign = 5;

			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, WorkflowDescriptors.WhsPickWorkflowDescriptorCode, true);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "PWH";
			capability.G4_Description = "Product Warehouse";
			Factory.Save();

			var ruleHelper = new Helper(Factory);
			var ruleSet = ruleHelper.CreateRuleSet("Breakdown", "Breakdown", context: "PWB", contextSubType: "PIC", warehousePK: warehouseOnRuleSet ? data.Whs1.PK : null);
			var rule = ruleHelper.CreateRule(ruleSet, "Rule 1", "Rule 1");
			rule.PRL_RuleDefinition = $@"
{{
	""conditions"": [
	],
	""action"":	{{
		""$type"": ""BreakdownPickTaskActionState"",
		""sortByCriteria"": [
		],
		""groupByCriteria"": [
		],
		""capability"": ""PWH"",
		""maxUnitsPerTask"": 6,
		""maxWeightPerTaskUQ"": ""KG"",
		""maxVolumePerTaskUQ"": ""M3""
	}}
}}".TrimStart();
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var orderedInv = pick.OrderedInventories[0];
			var availableInventory1 = orderedInv.AvailableInventories[0];
			var availableInventorySplit1 = availableInventory1.AvailableInventoriesSplit.Single();
			AssertEquals("Precondition: Single pick line.", 1, availableInventory1.PickLines.Count());

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);
			Factory.Save();

			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();
			var processor = ObjectFactory.Get<IWhsTaskCreationProcessor>();
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: Pick P00000001."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 2 task(s)."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: Pick P00000001."))));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, pickInNewFactory.WP_TaskPlanningStatus);

			var tasks = pickInNewFactory.WorkflowItems.Where(t => !t.P9_FormFlowType.IsEmpty).OrderBy(t => t.P9_Sequence).ToArray();
			AssertEquals("Should have created two tasks.", 2, tasks.Length);

			var sequence = 100;
			foreach (var task in tasks)
			{
				AssertEquals("Should have set formflow type.", WarehouseTaskFormFlowTypes.PickJob, task.P9_FormFlowType);
				AssertEquals("Should have set capability.", capability.PK, task.P9_G4_RequiredCapability);
				AssertEquals("Should have set nudge.", "200", task.EffectiveTaskNudge);
				AssertEquals("Should have set sequence.", sequence, task.P9_Sequence);
				AssertEquals("Should have set task name.", "Pick Lines", task.P9_Description);

				sequence += 100;
			}

			var pickLines = pickInNewFactory.GetAllPickLines().ToArray();
			AssertEquals("Should have two pick lines.", 2, pickLines.Length);
			AssertContainsExactElementsInAnyOrder("Should have linked pick lines.", [tasks[0].PK, tasks[1].PK], pickLines.Select(pl => pl.WZ_P9_Task));
			AssertContainsExactElementsInAnyOrder("Should have split pick lines.", [6m, 4m], pickLines.Select(pl => pl.WZ_Units));
		}

		public void TestProcessQueue_EndToEnd_Pick_PickByUOM() => TestProcessQueue_EndToEnd_Pick_PickByUOM(cartoniseSplitCase: false, pickPalletsByLabel: false, pickCasesByLabel: false);
		public void TestProcessQueue_EndToEnd_Pick_PickByUOM_UsingCartonization() => TestProcessQueue_EndToEnd_Pick_PickByUOM(cartoniseSplitCase: true, pickPalletsByLabel: false, pickCasesByLabel: false);
		public void TestProcessQueue_EndToEnd_Pick_PickByUOM_UsingPickPalletsByLabel() => TestProcessQueue_EndToEnd_Pick_PickByUOM(cartoniseSplitCase: false, pickPalletsByLabel: true, pickCasesByLabel: false);
		public void TestProcessQueue_EndToEnd_Pick_PickByUOM_UsingPickCasesByLabel() => TestProcessQueue_EndToEnd_Pick_PickByUOM(cartoniseSplitCase: false, pickPalletsByLabel: false, pickCasesByLabel: true);
		public void TestProcessQueue_EndToEnd_Pick_PickByUOM_UsingCartonizationAndPickByLabel() => TestProcessQueue_EndToEnd_Pick_PickByUOM(cartoniseSplitCase: true, pickPalletsByLabel: true, pickCasesByLabel: true);

		void TestProcessQueue_EndToEnd_Pick_PickByUOM(bool cartoniseSplitCase, bool pickPalletsByLabel, bool pickCasesByLabel)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Pallet)).F3_UOMType = UOMPackTypesList.Codes.Pallet;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Unit)).F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 10m);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 3m);

			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, WorkflowDescriptors.WhsPickWorkflowDescriptorCode, true);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "PWH";
			capability.G4_Description = "Product Warehouse";
			Factory.Save();

			var ruleHelper = new Helper(Factory);
			var ruleSet = ruleHelper.CreateRuleSet("Breakdown", "Breakdown", context: "PWB", contextSubType: "PIC");
			var rule = ruleHelper.CreateRule(ruleSet, "Rule 1", "Rule 1");
			rule.PRL_RuleDefinition = $@"
{{
	""conditions"": [
	],
	""action"":	{{
		""$type"": ""BreakdownPickTaskActionState"",
		""sortByCriteria"": [
		],
		""groupByCriteria"": [
		],
		""capability"": ""PWH"",
		""maxWeightPerTaskUQ"": ""KG"",
		""maxVolumePerTaskUQ"": ""M3""
	}}
}}".TrimStart();
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 37m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = cartoniseSplitCase;
			pick.WP_PickPalletsByLabel = pickPalletsByLabel;
			pick.WP_PickCasesByLabel = pickCasesByLabel;
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var availableInventorySplitByUOM = pick.OrderedInventories[0].AvailableInventories[0].AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitByUOM>().ToArray();
			AssertEquals("Should have created 3 splits.", 3, availableInventorySplitByUOM.Length);

			var pltSplit = availableInventorySplitByUOM.Single(s => s.UOMType == UOMPackTypesList.Codes.Pallet);
			var casSplit = availableInventorySplitByUOM.Single(s => s.UOMType == UOMPackTypesList.Codes.Case);
			var spcSplit = availableInventorySplitByUOM.Single(s => s.UOMType == UOMPackTypesList.Codes.SplitCase);
			AssertEquals("Pallet split should have 3 packs.", 3m, pltSplit.PackQuantity);
			AssertEquals("Case split should have 3 packs.", 2m, casSplit.PackQuantity);
			AssertEquals("Pallet split should have 3 packs.", 1m, spcSplit.PackQuantity);

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);
			Factory.Save();

			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();
			var processor = ObjectFactory.Get<IWhsTaskCreationProcessor>();
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, pickInNewFactory.WP_TaskPlanningStatus);

			if (cartoniseSplitCase && pickPalletsByLabel && pickCasesByLabel)
			{
				notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: Pick P00000001."))));
				notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 0 task(s)."))));
				notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: Pick P00000001."))));

				AssertEquals("Should not have created any tasks.", 0, pickInNewFactory.WorkflowItems.Count);
			}
			else
			{
				notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: Pick P00000001."))));
				notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 1 task(s)."))));
				notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: Pick P00000001."))));

				var tasks = pickInNewFactory.WorkflowItems.Where(t => !t.P9_FormFlowType.IsEmpty).OrderBy(t => t.P9_Sequence).ToArray();
				AssertEquals("Should have created 1 task.", 1, tasks.Length);

				var task = tasks.Single();
				AssertEquals("Should have set formflow type.", WarehouseTaskFormFlowTypes.PickJob, task.P9_FormFlowType);
				AssertEquals("Should have set capability.", capability.PK, task.P9_G4_RequiredCapability);
				AssertEquals("Should have set nudge.", "200", task.EffectiveTaskNudge);
				AssertEquals("Should have set sequence.", 100, task.P9_Sequence);
				AssertEquals("Should have set task name.", "Pick Lines", task.P9_Description);

				var pickLines = pickInNewFactory.GetAllPickLines().ToArray();
				AssertEquals("Should have three pick lines.", 3, pickLines.Length);
				var spcPickLine = pickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Unit);
				var casPickLine = pickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Box);
				var pltPickLine = pickLines.Single(pl => pl.WZ_F3_NKAllocatedPackType == Constants.PkgUnit.Pallet);

				AssertEquals(pickPalletsByLabel ? ZGuid.Empty : task.PK, pltPickLine.WZ_P9_Task);
				AssertEquals(pickCasesByLabel ? ZGuid.Empty : task.PK, casPickLine.WZ_P9_Task);
				AssertEquals(cartoniseSplitCase ? ZGuid.Empty : task.PK, spcPickLine.WZ_P9_Task);
			}
		}

		public void TestProcessQueue_EndToEnd_Pick_SplitByWeight() => TestProcessQueue_EndToEnd_Pick_Split(useWeightLimit: true, useVolumeLimit: false);
		public void TestProcessQueue_EndToEnd_Pick_SplitByVolume() => TestProcessQueue_EndToEnd_Pick_Split(useWeightLimit: false, useVolumeLimit: true);

		void TestProcessQueue_EndToEnd_Pick_Split(bool useWeightLimit, bool useVolumeLimit)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			data.Part1.OP_Weight = 5000m;
			data.Part1.OP_Cubic = 500000m;
			data.Part1.OP_WeightUQ = "G";
			data.Part1.OP_CubicUQ = "CC";

			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, WorkflowDescriptors.WhsPickWorkflowDescriptorCode, true);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "PWH";
			capability.G4_Description = "Product Warehouse";
			Factory.Save();

			var ruleHelper = new Helper(Factory);
			var ruleSet = ruleHelper.CreateRuleSet("Breakdown", "Breakdown", context: "PWB", contextSubType: "PIC");
			var rule = ruleHelper.CreateRule(ruleSet, "Rule 1", "Rule 1");
			rule.PRL_RuleDefinition = $@"
{{
	""conditions"": [
	],
	""action"":	{{
		""$type"": ""BreakdownPickTaskActionState"",
		""sortByCriteria"": [
		],
		""groupByCriteria"": [
		],
		""capability"": ""PWH"",
		""maxWeightPerTask"": ""{(useWeightLimit ? 30m : 0m)}"",
		""maxVolumePerTask"": ""{(useVolumeLimit ? 3m : 0m)}"",
		""maxWeightPerTaskUQ"": ""KG"",
		""maxVolumePerTaskUQ"": ""M3""
	}}
}}".TrimStart();
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);
			Factory.Save();

			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();
			var processor = ObjectFactory.Get<IWhsTaskCreationProcessor>();
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, pickInNewFactory.WP_TaskPlanningStatus);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: Pick P00000001."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 2 task(s)."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: Pick P00000001."))));

			var tasks = pickInNewFactory.WorkflowItems.Where(t => !t.P9_FormFlowType.IsEmpty).OrderBy(t => t.P9_Sequence).ToArray();
			AssertEquals("Should have created 2 task.", 2, tasks.Length);

			var sequence = 100;
			foreach (var task in tasks)
			{
				AssertEquals("Should have set formflow type.", WarehouseTaskFormFlowTypes.PickJob, task.P9_FormFlowType);
				AssertEquals("Should have set capability.", capability.PK, task.P9_G4_RequiredCapability);
				AssertEquals("Should have set nudge.", "200", task.EffectiveTaskNudge);
				AssertEquals("Should have set sequence.", sequence, task.P9_Sequence);
				AssertEquals("Should have set task name.", "Pick Lines", task.P9_Description);

				sequence += 100;
			}

			var pickLines = pickInNewFactory.GetAllPickLines().ToArray();
			AssertEquals("Should have two pick lines.", 2, pickLines.Length);
			AssertContainsExactElementsInAnyOrder("Should have linked pick lines.", [tasks[0].PK, tasks[1].PK], pickLines.Select(pl => pl.WZ_P9_Task));
			AssertContainsExactElementsInAnyOrder("Should have split pick lines.", [6m, 4m], pickLines.Select(pl => pl.WZ_Units));
		}

		public void TestProcessQueue_EndToEnd_Pick_WorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, WorkflowDescriptors.WhsPickWorkflowDescriptorCode, true);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "PWH";
			capability.G4_Description = "Product Warehouse";
			Factory.Save();

			var ruleHelper = new Helper(Factory);
			var ruleSet = ruleHelper.CreateRuleSet("Breakdown", "Breakdown", context: "PWB", contextSubType: "PIC");
			var rule = ruleHelper.CreateRule(ruleSet, "Rule 1", "Rule 1");
			rule.PRL_RuleDefinition = $@"
{{
	""conditions"": [
	],
	""action"":	{{
		""$type"": ""BreakdownPickTaskActionState"",
		""sortByCriteria"": [
		],
		""groupByCriteria"": [
		],
		""capability"": ""PWH"",
		""maxWeightPerTaskUQ"": ""KG"",
		""maxVolumePerTaskUQ"": ""M3""
	}}
}}".TrimStart();
			Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, wheel.OP_StockKeepingUnit);
			Helper.CreateProductBOM(bike, frame, 1m, frame.OP_StockKeepingUnit);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", frame, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, bike, 10m);
			var pick = Helper.CreatePickNew(workOrder);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var jobReadyForPlanning = Factory.Load<WhsReadyForPlanningJobsView>(pick.PK);
			AssertNotNull("Precondition.", jobReadyForPlanning);
			Factory.Save();

			var cancellationToken = new CancellationToken();
			var notificationsMock = new Mock<INotifications>();
			var processor = ObjectFactory.Get<IWhsTaskCreationProcessor>();
			processor.ProcessQueue(notificationsMock.Object, cancellationToken);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Should be planned", TaskPlanningStatus.Codes.Planned, pickInNewFactory.WP_TaskPlanningStatus);

			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Processing Job: Pick P00000001."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Creating 1 task(s)."))));
			notificationsMock.Verify(n => n.Add(It.Is<INotification>(x => x.Type == NotificationType.Information && x.Message.StartsWith("Succesfully processed and saved Job: Pick P00000001."))));

			var tasks = pickInNewFactory.WorkflowItems.Where(t => !t.P9_FormFlowType.IsEmpty).OrderBy(t => t.P9_Sequence).ToArray();
			var task = tasks.Single();
			AssertEquals("Should have set formflow type.", WarehouseTaskFormFlowTypes.PickJob, task.P9_FormFlowType);
			AssertEquals("Should have set capability.", capability.PK, task.P9_G4_RequiredCapability);
			AssertEquals("Should have set nudge.", "200", task.EffectiveTaskNudge);
			AssertEquals("Should have set sequence.", 100, task.P9_Sequence);
			AssertEquals("Should have set task name.", "Pick Lines", task.P9_Description);

			var pickLines = pickInNewFactory.GetAllPickLines().ToArray();
			AssertEquals("Should have two pick lines.", 2, pickLines.Length);
			AssertEquals("Should have linked pick lines.", true, pickLines.All(pl => pl.WZ_P9_Task == task.PK));
		}
	}
}
