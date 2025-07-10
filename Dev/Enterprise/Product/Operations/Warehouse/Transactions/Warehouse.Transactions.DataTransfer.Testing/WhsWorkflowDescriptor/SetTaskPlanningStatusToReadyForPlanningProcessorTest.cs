using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Moq;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class SetTaskPlanningStatusToReadyForPlanningProcessorTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new SetTaskPlanningStatusToReadyForPlanningProcessor(null));
		}

		#endregion

		#region TestProcess_CurrentTaskPlanningStatus

		public void TestProcess_CurrentTaskPlanningStatus_Empty()
		{
			TestProcessCore(string.Empty, string.Empty, "Current Task Planning Status is not 'Not Ready For Planning'.\r\n");
		}

		public void TestProcess_CurrentTaskPlanningStatus_NotReadyForPlanning()
		{
			TestProcessCore(TaskPlanningStatus.Codes.NotReady, TaskPlanningStatus.Codes.Ready, string.Empty);
		}

		public void TestProcess_CurrentTaskPlanningStatus_ReadyForPlanning()
		{
			TestProcessCore(TaskPlanningStatus.Codes.Ready, TaskPlanningStatus.Codes.Ready, "Current Task Planning Status is not 'Not Ready For Planning'.\r\n");
		}

		public void TestProcess_CurrentTaskPlanningStatus_Planned()
		{
			TestProcessCore(TaskPlanningStatus.Codes.Planned, TaskPlanningStatus.Codes.Planned, "Current Task Planning Status is not 'Not Ready For Planning'.\r\n");
		}

		void TestProcessCore(string status, string expectedStatus, string expectedMessage)
		{
			Notify.DefaultResponse = false;
			var job = new Mock<ITaskPlanningJob>();
			job.Setup(s => s.TaskPlanningStatus).Returns(status);
			job.SetupSet(s => s.TaskPlanningStatus = It.IsAny<ZString>())
				.Callback((ZString value) => job.Setup(s => s.TaskPlanningStatus).Returns(value));
			job.Setup(s => s.IsInDatabase).Returns(true);
			job.Setup(s => s.HasChanges).Returns(false);
			job.Setup(s => s.IsFinalisedOrCancelled).Returns(false);

			IProcessor processor = new SetTaskPlanningStatusToReadyForPlanningProcessor(job.Object);
			processor.Process(Notify);
			AssertEquals(expectedMessage, Notify.AsString);
			AssertEquals(expectedStatus, job.Object.TaskPlanningStatus);
		}

		#endregion

		#region TestProcess_CannotUpdateReason

		public void TestProcess_CannotUpdateReason_NotInDatabase()
		{
			TestProcess_CannotUpdateReasonCore(false, false, false, TaskPlanningStatus.Codes.NotReady, "Cannot change Task Planning Status as the mock is not saved.\r\n");
		}

		public void TestProcess_CannotUpdateReason_HasChanges()
		{
			TestProcess_CannotUpdateReasonCore(true, true, false, TaskPlanningStatus.Codes.NotReady, "Cannot change Task Planning Status as the mock is not saved.\r\n");
		}

		public void TestProcess_CannotUpdateReason_IsFinalisedOrCancelled()
		{
			TestProcess_CannotUpdateReasonCore(true, false, true, TaskPlanningStatus.Codes.NotReady, "Cannot change Task Planning Status as the mock is finalized or canceled.\r\n");
		}

		public void TestProcess_CannotUpdateReason_NoError()
		{
			TestProcess_CannotUpdateReasonCore(true, false, false, TaskPlanningStatus.Codes.Ready,String.Empty);
		}

		public void TestProcess_CannotUpdateReasonCore(bool isInDatabase, bool hasChanges, bool isFinalisedOrCancelled, string expectedStatus, string expectedMessage)
		{
			Notify.DefaultResponse = false;
			var job = new Mock<ITaskPlanningJob>();
			job.Setup(s => s.TaskPlanningStatus).Returns(TaskPlanningStatus.Codes.NotReady);
			job.SetupSet(s => s.TaskPlanningStatus = It.IsAny<ZString>())
				.Callback((ZString value) => job.Setup(s => s.TaskPlanningStatus).Returns(value));
			job.Setup(s => s.IsInDatabase).Returns(isInDatabase);
			job.Setup(s => s.HasChanges).Returns(hasChanges);
			job.Setup(s => s.IsFinalisedOrCancelled).Returns(isFinalisedOrCancelled);
			job.Setup(s => s.HumanReadableNameWithoutID).Returns("mock");

			IProcessor processor = new SetTaskPlanningStatusToReadyForPlanningProcessor(job.Object);
			processor.Process(Notify);
			AssertEquals(expectedMessage, Notify.AsString);
			AssertEquals(expectedStatus, job.Object.TaskPlanningStatus);
		}

		#endregion

	}
}
