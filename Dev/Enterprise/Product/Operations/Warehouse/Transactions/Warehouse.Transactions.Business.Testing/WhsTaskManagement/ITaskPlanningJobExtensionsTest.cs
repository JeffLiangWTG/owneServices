using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class ITaskPlanningJobExtensionsTest : WhsTestCaseWithFactory
	{
		#region IsUnplanned

		public void TestIsTaskPlanningStatusEmptyOrNotReady_Empty()
		{
			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.TaskPlanningStatus).Returns(string.Empty);
			AssertEquals(true, mock.Object.IsUnplanned());
		}

		public void TestIsTaskPlanningStatusEmptyOrNotReady_NotReady()
		{
			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.TaskPlanningStatus).Returns(TaskPlanningStatus.Codes.NotReady);
			AssertEquals(true, mock.Object.IsUnplanned());
		}

		public void TestIsTaskPlanningStatusEmptyOrNotReady_Ready()
		{
			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.TaskPlanningStatus).Returns(TaskPlanningStatus.Codes.Ready);
			AssertEquals(false, mock.Object.IsUnplanned());
		}

		public void TestIsTaskPlanningStatusEmptyOrNotReady_Planned()
		{
			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.TaskPlanningStatus).Returns(TaskPlanningStatus.Codes.Planned);
			AssertEquals(false, mock.Object.IsUnplanned());
		}

		#endregion

		#region IsTaskManagementEnabled

		public void TestIsTaskManagementEnabled()
		{
			var whs = Helper.CreateWarehouse("WH1", "A", 1, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			Factory.Save();

			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.Factory).Returns(Factory);
			AssertEquals(false, mock.Object.IsTaskManagementEnabled());

			mock.Setup(s => s.WarehousePK).Returns(whs.PK);
			AssertEquals(false, mock.Object.IsTaskManagementEnabled());

			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			AssertEquals(true, mock.Object.IsTaskManagementEnabled());
		}

		#endregion

		#region GetCannotUpdateTaskPlanningStatusReason

		public void TestGetCannotUpdateTaskPlanningStatusReason_NotInDatabase()
		{
			TestGetCannotUpdateTaskPlanningStatusReasonCore(false, false, false, "Cannot change Task Planning Status as the mock is not saved.");
		}

		public void TestGetCannotUpdateTaskPlanningStatusReason_HasChanges()
		{
			TestGetCannotUpdateTaskPlanningStatusReasonCore(true, true, false, "Cannot change Task Planning Status as the mock is not saved.");
		}

		public void TestGetCannotUpdateTaskPlanningStatusReason_IsFinalisedOrCancelled()
		{
			TestGetCannotUpdateTaskPlanningStatusReasonCore(true, false, true, "Cannot change Task Planning Status as the mock is finalized or canceled.");
		}

		public void TestGetCannotUpdateTaskPlanningStatusReason_NoError()
		{
			TestGetCannotUpdateTaskPlanningStatusReasonCore(true, false, false, string.Empty);
		}

		void TestGetCannotUpdateTaskPlanningStatusReasonCore(bool isInDatabase, bool hasChanges, bool isFinalisedOrCancelled, string expectedMessage)
		{
			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.Factory).Returns(Factory);
			mock.Setup(s => s.IsInDatabase).Returns(isInDatabase);
			mock.Setup(s => s.HasChanges).Returns(hasChanges);
			mock.Setup(s => s.IsFinalisedOrCancelled).Returns(isFinalisedOrCancelled);
			mock.Setup(s => s.HumanReadableNameWithoutID).Returns("mock");

			if (string.IsNullOrEmpty(expectedMessage))
			{
				AssertNullOrEmpty(mock.Object.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
				AssertNullOrEmpty(mock.Object.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
			}
			else
			{
				AssertEquals(expectedMessage, mock.Object.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
				AssertEquals(expectedMessage, mock.Object.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
			}
		}

		public void TestGetCannotUpdateTaskPlanningStatusReason_SpecialError()
		{
			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.SpecialCannotUpdateTaskPlanningStatusReason).Returns("Cannot change Task Planning Status for special reason.");

			AssertEquals("Cannot change Task Planning Status for special reason.", mock.Object.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals("Cannot change Task Planning Status for special reason.", mock.Object.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestGetCannotUpdateTaskPlanningStatusReason_ContainsCompletedProcessTask()
		{
			var jobPk = ZGuid.NewZGuid();

			var task1 = Factory.New<ProcessTask>();
			task1.P9_ParentID = jobPk;
			task1.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task1.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task1.P9_CompletedTime = ZDateTimeOffset.Now;

			var task2 = Factory.New<ProcessTask>();
			task2.P9_ParentID = jobPk;
			task2.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task2.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task2.P9_ActualDateOffset = ZDateTimeOffset.Now.AddHours(-20);

			var task3 = Factory.New<ProcessTask>();
			task3.P9_ParentID = jobPk;
			task3.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task3.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task3.P9_SuspendedAt = ZDateTime.Now.AddHours(-10);

			var task4 = Factory.New<ProcessTask>();
			task4.P9_ParentID = jobPk;
			task4.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task4.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			Factory.Save();

			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.PK).Returns(jobPk);
			mock.Setup(s => s.Factory).Returns(Factory);
			mock.Setup(s => s.IsInDatabase).Returns(true);
			mock.Setup(s => s.HasChanges).Returns(false);
			mock.Setup(s => s.IsFinalisedOrCancelled).Returns(false);

			AssertEquals("Cannot change Task Planning Status to Not Ready for job with completed task(s).", mock.Object.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestGetCannotUpdateTaskPlanningStatusReason_ProcessTaskCompletedNotInTheFirst()
		{
			var jobPk = ZGuid.NewZGuid();

			var task1 = Factory.New<ProcessTask>();
			task1.P9_ParentID = jobPk;
			task1.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task1.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddHours(-20);

			var task2 = Factory.New<ProcessTask>();
			task2.P9_ParentID = jobPk;
			task2.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task2.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task2.P9_SuspendedAt = ZDateTime.Now.AddHours(-10);

			var task3 = Factory.New<ProcessTask>();
			task3.P9_ParentID = jobPk;
			task3.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task3.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;

			var task4 = Factory.New<ProcessTask>();
			task4.P9_ParentID = jobPk;
			task4.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task4.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task4.P9_CompletedTime = ZDateTimeOffset.Now;
			Factory.Save();

			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.Factory).Returns(Factory);
			mock.Setup(s => s.PK).Returns(jobPk);
			mock.Setup(s => s.IsInDatabase).Returns(true);
			mock.Setup(s => s.HasChanges).Returns(false);
			mock.Setup(s => s.IsFinalisedOrCancelled).Returns(false);

			AssertEquals("Cannot change Task Planning Status to Not Ready for job with completed task(s).", mock.Object.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestGetCannotUpdateTaskPlanningStatusReason_ContainsTaskStartedNoMoreThanOneDay()
		{
			var jobPk = ZGuid.NewZGuid();

			var task1 = Factory.New<ProcessTask>();
			task1.P9_ParentID = jobPk;
			task1.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task1.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddHours(-20);

			var task2 = Factory.New<ProcessTask>();
			task2.P9_ParentID = jobPk;
			task2.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task2.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task2.P9_ActualDateOffset = ZDateTimeOffset.Now.AddHours(-26);
			Factory.Save();

			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.Factory).Returns(Factory);
			mock.Setup(s => s.PK).Returns(jobPk);
			mock.Setup(s => s.IsInDatabase).Returns(true);
			mock.Setup(s => s.HasChanges).Returns(false);
			mock.Setup(s => s.IsFinalisedOrCancelled).Returns(false);

			AssertEquals("Cannot change Task Planning Status to Not Ready for job with task(s) started/suspended within the last 24 hours.", mock.Object.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestGetCannotUpdateTaskPlanningStatusReason_ContainsTaskSuspendedNoMoreThanOneDay()
		{
			var jobPk = ZGuid.NewZGuid();

			var task1 = Factory.New<ProcessTask>();
			task1.P9_ParentID = jobPk;
			task1.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task1.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task1.P9_SuspendedAt = ZDateTime.Now.AddHours(-20);

			var task2 = Factory.New<ProcessTask>();
			task2.P9_ParentID = jobPk;
			task2.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task2.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task2.P9_SuspendedAt = ZDateTime.Now.AddHours(-26);
			Factory.Save();

			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.Factory).Returns(Factory);
			mock.Setup(s => s.PK).Returns(jobPk);
			mock.Setup(s => s.IsInDatabase).Returns(true);
			mock.Setup(s => s.HasChanges).Returns(false);
			mock.Setup(s => s.IsFinalisedOrCancelled).Returns(false);

			AssertEquals("Cannot change Task Planning Status to Not Ready for job with task(s) started/suspended within the last 24 hours.", mock.Object.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestGetCannotUpdateTaskPlanningStatusReason_AllTasksStartedOrSuspendedMoreThanOneDay()
		{
			var jobPk = ZGuid.NewZGuid();

			var task1 = Factory.New<ProcessTask>();
			task1.P9_ParentID = jobPk;
			task1.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task1.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddHours(-25);

			var task2 = Factory.New<ProcessTask>();
			task2.P9_ParentID = jobPk;
			task2.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task2.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task2.P9_SuspendedAt = ZDateTime.Now.AddHours(-26);
			Factory.Save();

			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.Factory).Returns(Factory);
			mock.Setup(s => s.PK).Returns(jobPk);
			mock.Setup(s => s.IsInDatabase).Returns(true);
			mock.Setup(s => s.HasChanges).Returns(false);
			mock.Setup(s => s.IsFinalisedOrCancelled).Returns(false);

			AssertNullOrEmpty(mock.Object.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		#endregion

		#region GetRelatedProcessTasks

		public void TestGetRelatedProcessTasks()
		{
			var jobPk = ZGuid.NewZGuid();

			var task1 = Factory.New<ProcessTask>();
			task1.P9_ParentID = jobPk;
			task1.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task1.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task1.P9_CompletedTime = ZDateTimeOffset.Now;

			var task2 = Factory.New<ProcessTask>();
			task2.P9_ParentID = jobPk;
			task2.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task2.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task2.P9_ActualDateForBinding = ZDateTimeOffset.Now.AddHours(-20);

			var task3 = Factory.New<ProcessTask>();
			task3.P9_ParentID = jobPk;
			task3.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task3.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			task3.P9_SuspendedAtForBinding = ZDateTimeOffset.Now.AddHours(-10);

			var task4 = Factory.New<ProcessTask>();
			task4.P9_ParentID = jobPk;
			task4.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task4.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			Factory.Save();

			var mock = new Mock<ITaskPlanningJob>();
			mock.Setup(s => s.Factory).Returns(Factory);
			mock.Setup(s => s.PK).Returns(jobPk);

			AssertContainsExactElementsInAnyOrder([task1, task2, task3, task4], mock.Object.GetRelatedProcessTasksOffJob());
		}

		public void TestGetRelatedProcessTasks_JobWithExternalTasks()
		{
			var jobPk = ZGuid.NewZGuid();

			var task1 = Factory.New<ProcessTask>();
			task1.P9_ParentID = jobPk;
			task1.P9_ParentTableCode = WhsCycleCountWaveSchema.Constants.Prefix;
			task1.P9_FormFlowType = WarehouseTaskFormFlowTypes.CycleCountJob;
			task1.P9_CompletedTime = ZDateTimeOffset.Now;

			var task2 = Factory.New<ProcessTask>();
			task2.P9_ParentID = jobPk;
			task2.P9_ParentTableCode = WhsCycleCountWaveSchema.Constants.Prefix;
			task2.P9_FormFlowType = WarehouseTaskFormFlowTypes.CycleCountJob;
			task2.P9_ActualDateForBinding = ZDateTimeOffset.Now.AddHours(-20);

			var task3 = Factory.New<ProcessTask>();
			task3.P9_ParentID = jobPk;
			task3.P9_ParentTableCode = WhsCycleCountWaveSchema.Constants.Prefix;
			task3.P9_FormFlowType = WarehouseTaskFormFlowTypes.CycleCountJob;
			task3.P9_SuspendedAtForBinding = ZDateTimeOffset.Now.AddHours(-10);

			var task4 = Factory.New<ProcessTask>();
			task4.P9_ParentID = jobPk;
			task4.P9_ParentTableCode = WhsCycleCountWaveSchema.Constants.Prefix;
			task4.P9_FormFlowType = WarehouseTaskFormFlowTypes.CycleCountJob;
			Factory.Save();

			var mock = new Mock<ITaskPlanningJobWithExternalTasks>();
			mock.Setup(s => s.Factory).Returns(Factory);
			mock.Setup(s => s.PK).Returns(jobPk);
			mock.Setup(s => s.GetRelatedProcessTasks()).Returns([task1, task2]);

			AssertContainsExactElementsInAnyOrder([task1, task2], mock.Object.GetRelatedProcessTasks());
		}

		#endregion
	}
}
