using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Moq;
using Moq.Language;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class BeginRFTaskHelperTest : TransactionedTestCase
	{
		public void TestBeginRFTaskHelper_BeginRFTask_NullTask()
		{
			TestBeginRFTaskHelper_NullTaskCore((response, task, staff) => BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.UnloadJob, staff));
		}

		public void TestBeginRFTaskHelper_SuspendRFTask_NullTask()
		{
			TestBeginRFTaskHelper_NullTaskCore(BeginRFTaskHelper.SuspendRFTask);
		}

		public void TestBeginRFTaskHelper_CloseRFTask_NullTask()
		{
			TestBeginRFTaskHelper_NullTaskCore(BeginRFTaskHelper.CloseRFTask);
		}

		void TestBeginRFTaskHelper_NullTaskCore(Func<WebServiceResponse, ProcessTask, GlbStaff, bool> actionToTest)
		{
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var response = new WebServiceResponse();
			var shouldSave = BeginRFTaskHelper.BeginRFTask(response, null, WarehouseTaskFormFlowTypes.UnloadJob, staff);

			AssertEquals("Should save returns false", false, shouldSave);
			AssertEquals("Task could not be found.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestBeginRFTaskHelper_BeginRFTask_Success()
		{
			TestBeginRFTaskHelper_SuccessCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.UnloadJob, "US1")),
				(response, task, staff) => BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.UnloadJob, staff));
		}

		public void TestBeginRFTaskHelper_SuspendRFTask_Success()
		{
			TestBeginRFTaskHelper_SuccessCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToSuspendedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.SuspendRFTask);
		}

		public void TestBeginRFTaskHelper_CloseRFTask_Success()
		{
			TestBeginRFTaskHelper_SuccessCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToCompletedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.CloseRFTask);
		}

		void TestBeginRFTaskHelper_SuccessCore(
			Func<Mock<IWhsTaskManagementService>, IReturns<IWhsTaskManagementService, UpdateTaskStatusResult>> setupMockMethod,
			Func<WebServiceResponse, ProcessTask, GlbStaff, bool> actionToTest)
		{
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var response = new WebServiceResponse();
			var task = Factory.New<ProcessTask>();
			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			setupMockMethod(taskManagementServiceManagerMock).Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var result = actionToTest(response, task, staff);

				AssertEquals("Should save returns true", true, result);
				AssertEquals(null, response.ErrorMessage);
				AssertEquals(ErrorTypes.None, response.Error);
			}
		}

		public void TestBeginRFTaskHelper_CloseRFTask_SuccessWithNoChanges()
		{
			TestBeginRFTaskHelper_SuccessWithNoChangesCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToCompletedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.CloseRFTask);
		}

		public void TestBeginRFTaskHelper_SuspendRFTask_SuccessWithNoChanges()
		{
			TestBeginRFTaskHelper_SuccessWithNoChangesCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToSuspendedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.SuspendRFTask);
		}

		void TestBeginRFTaskHelper_SuccessWithNoChangesCore(
			Func<Mock<IWhsTaskManagementService>, IReturns<IWhsTaskManagementService, UpdateTaskStatusResult>> setupMockMethod,
			Func<WebServiceResponse, ProcessTask, GlbStaff, bool> actionToTest)
		{
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var response = new WebServiceResponse();
			var task = Factory.New<ProcessTask>();
			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			setupMockMethod(taskManagementServiceManagerMock).Returns(UpdateTaskStatusResult.SuccessWithNoChanges);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var result = actionToTest(response, task, staff);

				AssertEquals("Should save returns false", false, result);
				AssertEquals(null, response.ErrorMessage);
				AssertEquals(ErrorTypes.None, response.Error);
			}
		}

		public void TestBeginRFTaskHelper_BeginRFTask_TaskStatusIsOpen()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.UnloadJob, "US1")),
				(response, task, staff) => BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.UnloadJob, staff),
				UpdateTaskStatusResult.TaskStatusIsOpen,
				"The task is currently unassigned to any user and cannot be updated. Please check the task status and try again.");
		}

		public void TestBeginRFTaskHelper_SuspendRFTask_TaskStatusIsOpen()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToSuspendedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.SuspendRFTask,
				UpdateTaskStatusResult.TaskStatusIsOpen,
				"The task is currently unassigned to any user and cannot be updated. Please check the task status and try again.");
		}

		public void TestBeginRFTaskHelper_CloseRFTask_TaskStatusIsOpen()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToCompletedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.CloseRFTask,
				UpdateTaskStatusResult.TaskStatusIsOpen,
				"The task is currently unassigned to any user and cannot be updated. Please check the task status and try again.");
		}

		public void TestBeginRFTaskHelper_SuspendRFTask_TaskStatusIsCompleted()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToSuspendedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.SuspendRFTask,
				UpdateTaskStatusResult.TaskStatusIsCompleted,
				"The task is already closed and cannot be updated. Please check the task status and try again.");
		}

		public void TestBeginRFTaskHelper_CloseRFTask_TaskStatusIsCompleted()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToCompletedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.CloseRFTask,
				UpdateTaskStatusResult.TaskStatusIsCompleted,
				"The task is already closed and cannot be updated. Please check the task status and try again.");
		}

		public void TestBeginRFTaskHelper_BeginRFTask_TaskStatusIsCancelled()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.UnloadJob, "US1")),
				(response, task, staff) => BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.UnloadJob, staff),
				UpdateTaskStatusResult.TaskStatusIsCancelled,
				"The task is already canceled and cannot be updated. Please check the task status and try again.");
		}

		public void TestBeginRFTaskHelper_SuspendRFTask_TaskStatusIsCancelled()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToSuspendedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.SuspendRFTask,
				UpdateTaskStatusResult.TaskStatusIsCancelled,
				"The task is already canceled and cannot be updated. Please check the task status and try again.");
		}

		public void TestBeginRFTaskHelper_CloseRFTask_TaskStatusIsCancelled()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToCompletedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.CloseRFTask,
				UpdateTaskStatusResult.TaskStatusIsCancelled,
				"The task is already canceled and cannot be updated. Please check the task status and try again.");
		}

		public void TestBeginRFTaskHelper_BeginRFTask_TaskIsNotValidWarehouseJob()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.UnloadJob, "US1")),
				(response, task, staff) => BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.UnloadJob, staff),
				UpdateTaskStatusResult.TaskIsNotValidWarehouseJob,
				"This task is not a valid warehouse job. Please perform a different task.");
		}

		public void TestBeginRFTaskHelper_SuspendRFTask_TaskIsNotValidWarehouseJob()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToSuspendedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.SuspendRFTask,
				UpdateTaskStatusResult.TaskIsNotValidWarehouseJob,
				"This task is not a valid warehouse job. Please perform a different task.");
		}

		public void TestBeginRFTaskHelper_CloseRFTask_TaskIsNotValidWarehouseJob()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToCompletedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.CloseRFTask,
				UpdateTaskStatusResult.TaskIsNotValidWarehouseJob,
				"This task is not a valid warehouse job. Please perform a different task.");
		}

		public void TestBeginRFTaskHelper_BeginRFTask_TaskIsWrongFormFlowType()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.UnloadJob, "US1")),
				(response, task, staff) => BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.UnloadJob, staff),
				UpdateTaskStatusResult.TaskIsWrongFormFlowType,
				"This task is not valid for the current operation.");
		}

		public void TestBeginRFTaskHelper_SuspendRFTask_TaskIsWrongFormFlowType()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToSuspendedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.SuspendRFTask,
				UpdateTaskStatusResult.TaskIsWrongFormFlowType,
				"This task is not valid for the current operation.");
		}

		public void TestBeginRFTaskHelper_CloseRFTask_TaskIsWrongFormFlowType()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToCompletedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.CloseRFTask,
				UpdateTaskStatusResult.TaskIsWrongFormFlowType,
				"This task is not valid for the current operation.");
		}

		public void TestBeginRFTaskHelper_BeginRFTask_AssignedUserIsDifferent()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToPlayIfValid(It.IsAny<ProcessTask>(), WarehouseTaskFormFlowTypes.UnloadJob, "US1")),
				(response, task, staff) => BeginRFTaskHelper.BeginRFTask(response, task, WarehouseTaskFormFlowTypes.UnloadJob, staff),
				UpdateTaskStatusResult.AssignedUserIsDifferent,
				"This task is not assigned to the current user. Please perform a different task.");
		}

		public void TestBeginRFTaskHelper_SuspendRFTask_AssignedUserIsDifferent()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToSuspendedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.SuspendRFTask,
				UpdateTaskStatusResult.AssignedUserIsDifferent,
				"This task is not assigned to the current user. Please perform a different task.");
		}

		public void TestBeginRFTaskHelper_CloseRFTask_AssignedUserIsDifferent()
		{
			TestBeginRFTaskHelper_ErrorCore(
				(taskManagementServiceManagerMock) => taskManagementServiceManagerMock.Setup(t => t.SetTaskToCompletedIfValid(It.IsAny<ProcessTask>(), "US1")),
				BeginRFTaskHelper.CloseRFTask,
				UpdateTaskStatusResult.AssignedUserIsDifferent,
				"This task is not assigned to the current user. Please perform a different task.");
		}

		void TestBeginRFTaskHelper_ErrorCore(
			Func<Mock<IWhsTaskManagementService>, IReturns<IWhsTaskManagementService, UpdateTaskStatusResult>> setupMockMethod,
			Func<WebServiceResponse, ProcessTask, GlbStaff, bool> actionToTest,
			UpdateTaskStatusResult expectedTaskStatusResult,
			string expectedErrorMessage)
		{
			var staff = Helper.CreateGlbStaff("US1", "User");
			Factory.Save();

			var response = new WebServiceResponse();
			var task = Factory.New<ProcessTask>();
			var taskManagementServiceManagerMock = new Mock<IWhsTaskManagementService>();
			setupMockMethod(taskManagementServiceManagerMock).Returns(expectedTaskStatusResult);

			using (ObjectFactory.Substitute(taskManagementServiceManagerMock.Object))
			{
				var shouldSave = actionToTest(response, task, staff);

				AssertEquals("Should save returns false", false, shouldSave);
				AssertEquals(expectedErrorMessage, response.ErrorMessage);
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			}
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;
	}
}
