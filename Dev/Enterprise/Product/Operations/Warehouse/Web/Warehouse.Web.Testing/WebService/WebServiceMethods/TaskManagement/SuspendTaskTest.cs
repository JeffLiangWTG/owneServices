using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Moq;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class SuspendTaskTest : WhsSecureServiceTestCase
	{
		public void TestSuspendTask_BadTaskPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.SuspendTask(Guid.NewGuid());

			AssertBusinessValidationError(webService, "Task could not be found.", response);
		}

		public void TestSuspendTask_InvalidTaskFormFlowType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var task = Helper.Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.SuspendTask(task.PK.ToGuid());

			AssertBusinessValidationError(webService, "This task is not a valid warehouse job. Please perform a different task.", response);
		}

		public void TestSuspendTask_SuspendsValidTasks()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.SuspendTask(task.PK.ToGuid());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task.P9_Status);
			AssertSuccessfulResponseWithNoErrors(response, webService);
		}

		public void TestSuspendTask_SuccessfulIfServiceSuccess()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
			Helper.Factory.Save();

			var taskManagementServiceMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceMock.Setup(t => t.SetTaskToSuspendedIfValid(It.IsAny<IProcessTask>(), staff.GS_Code)).Returns(UpdateTaskStatusResult.Success);

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.SuspendTask(task.PK.ToGuid());

				AssertSuccessfulResponseWithNoErrors(response, webService);
				taskManagementServiceMock.Verify(t => t.SetTaskToSuspendedIfValid(It.Is<IProcessTask>(tk => tk.PK == task.PK), staff.GS_Code));
				taskManagementServiceMock.VerifyNoOtherCalls();
			}
		}

		public void TestSuspendTask_AlreadySuspended()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var staff = Helper.CreateGlbStaff("US1", "User");

			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Helper.Factory.Save();

			var taskManagementServiceMock = new Mock<IWhsTaskManagementService>();
			taskManagementServiceMock.Setup(t => t.SetTaskToSuspendedIfValid(It.IsAny<IProcessTask>(), staff.GS_Code)).Returns(UpdateTaskStatusResult.SuccessWithNoChanges);

			using (ObjectFactory.Substitute(taskManagementServiceMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var saveCount = 0;
				webService.Factory.Saving += f => saveCount++;

				var response = webService.SuspendTask(task.PK.ToGuid());
				AssertSuccessfulResponseWithNoErrors(response, webService);

				AssertEquals("Factory Save should not be called.", 0, saveCount);
				taskManagementServiceMock.Verify(t => t.SetTaskToSuspendedIfValid(It.Is<IProcessTask>(tk => tk.PK == task.PK), staff.GS_Code));
				taskManagementServiceMock.VerifyNoOtherCalls();
			}
		}
	}
}
