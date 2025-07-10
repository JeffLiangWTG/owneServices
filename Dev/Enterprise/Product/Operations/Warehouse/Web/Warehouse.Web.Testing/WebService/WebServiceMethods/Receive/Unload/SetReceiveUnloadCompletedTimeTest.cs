using System;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class SetReceiveUnloadCompletedTimeTest : WhsSecureServiceTestCase
	{
		public void TestSetReceiveUnloadCompletedTime()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.Factory.Save();

			AssertEquals("Precondition: WD_UnloadCompleteTime is empty", ZDateTimeOffset.Empty, receive.WD_UnloadCompletedTime);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.SetReceiveUnloadCompletedTime(receive.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);

			AssertNotEquals("WD_UnloadCompleteTime is set", ZDateTimeOffset.Empty, receive.WD_UnloadCompletedTime);
		}

		public void TestSetReceiveUnloadCompletedTime_InvalidPK()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.Factory.Save();

			AssertEquals("Precondition: WD_UnloadCompleteTime is empty", ZDateTimeOffset.Empty, receive.WD_UnloadCompletedTime);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.SetReceiveUnloadCompletedTime(Guid.NewGuid());
			AssertBusinessValidationError(webService, "Receive not found.", response);

			AssertEquals("WD_UnloadCompleteTime is still not set", ZDateTimeOffset.Empty, receive.WD_UnloadCompletedTime);
		}

		public void TestSetReceiveUnloadCompletedTime_AlreadySet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var time = ZDateTimeOffset.Now;
			receive.WD_UnloadCompletedTime = time;
			Helper.Factory.Save();

			AssertEquals("Precondition: WD_UnloadCompleteTime is set", time, receive.WD_UnloadCompletedTime);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.SetReceiveUnloadCompletedTime(receive.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);

			AssertEquals("WD_UnloadCompleteTime is still set to same value", time, receive.WD_UnloadCompletedTime);
		}

		public void TestSetReceiveUnloadCompletedTime_Concurrency()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);

			var staff1 = helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(helper.Factory);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			helper.Factory.Save();

			AssertEquals("Precondition: WD_UnloadCompleteTime is empty", ZDateTimeOffset.Empty, receive.WD_UnloadCompletedTime);

			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)receive).Row, Db.Connection);
			helper.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, helper.Factory);

			SetupSecurityHeader(webService, data.Whs1, staff1);
			var response = webService.SetReceiveUnloadCompletedTime(receive.PK.ToGuid());
			AssertBusinessValidationError(webService, "Unable to save unload completed time to Receive. Another user made changes. Please refresh and try again.", response);

			var receiveInNewFactory = new BusinessObjectFactory() { RefreshEnabled = false }.Load<WhsReceive>(receive.PK);
			AssertEquals("WD_UnloadCompleteTime is still not set", ZDateTimeOffset.Empty, receiveInNewFactory.WD_UnloadCompletedTime);
		}

		public void TestSetReceiveUnloadCompletedTime_PlannedReceiveWithTask_WorkingTask()
			=> TestSetReceiveUnloadCompletedTime_WithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskAssignedToOtherUser: true);

		public void TestSetReceiveUnloadCompletedTime_PlannedReceiveWithTask_WorkingTask_NotUnloadTask()
			=> TestSetReceiveUnloadCompletedTime_WithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: false, isTaskAssignedToOtherUser: true);

		public void TestSetReceiveUnloadCompletedTime_PlannedReceiveWithTask_NotWorkingTask()
			=> TestSetReceiveUnloadCompletedTime_WithTaskManagement(isPlannedReceive: true, isWorkingTask: false, isUnloadTask: true, isTaskAssignedToOtherUser: true);

		public void TestSetReceiveUnloadCompletedTime_NotPlannedReceiveWithTask_WorkingTask()
			=> TestSetReceiveUnloadCompletedTime_WithTaskManagement(isPlannedReceive: false, isWorkingTask: true, isUnloadTask: true, isTaskAssignedToOtherUser: true);

		public void TestSetReceiveUnloadCompletedTime_PlannedReceiveWithTask_WorkingTask_AssignedToOtherUser()
			=> TestSetReceiveUnloadCompletedTime_WithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskAssignedToOtherUser: false);

		void TestSetReceiveUnloadCompletedTime_WithTaskManagement(bool isPlannedReceive, bool isWorkingTask, bool isUnloadTask, bool isTaskAssignedToOtherUser)
		{
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");
			Helper.Factory.Save();

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			if (isPlannedReceive)
			{
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			}
			Helper.Factory.Save();

			var openTask = Helper.CreateProcessTaskForReceive(receive);
			var task = Helper.CreateProcessTaskForReceive(receive, isTaskAssignedToOtherUser ? otherStaff : staff);
			if (isWorkingTask)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			}
			if (!isUnloadTask)
			{
				task.P9_FormFlowType = string.Empty;
			}
			Helper.Factory.Save();

			AssertEquals("Precondition", isPlannedReceive, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", isUnloadTask, task.P9_FormFlowType.EqualsIgnoringCase(WarehouseTaskFormFlowTypes.UnloadJob));
			AssertEquals("Precondition", isWorkingTask, task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working));

			AssertEquals("Precondition: WD_UnloadCompleteTime is empty", ZDateTimeOffset.Empty, receive.WD_UnloadCompletedTime);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.SetReceiveUnloadCompletedTime(receive.PK.ToGuid());

			var cannotSetUnloadCompleteTime = isPlannedReceive && isWorkingTask && isUnloadTask && isTaskAssignedToOtherUser;
			if (!cannotSetUnloadCompleteTime)
			{
				AssertSuccessfulResponse(response, webService);
				AssertNotEquals("WD_UnloadCompleteTime is set", ZDateTimeOffset.Empty, receive.WD_UnloadCompletedTime);
				AssertEquals("current task is still working", isWorkingTask, task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working));
				AssertEquals("related open task is cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, openTask.P9_Status);
			}
			else
			{
				AssertEquals(ErrorTypes.Information, response.Error);
				AssertEquals("Attempted to complete unload but another user is still working on it.", response.ErrorMessage);
				AssertEquals("WD_UnloadCompleteTime is not set", ZDateTimeOffset.Empty, receive.WD_UnloadCompletedTime);
				AssertEquals("current task is still working", isWorkingTask, task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working));
				AssertEquals("related open task is still open", ProcessTaskStatusCodeList.Codes.Open, openTask.P9_Status);
			}
		}
	}
}
