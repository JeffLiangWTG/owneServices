using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class TaskManagementHelperTest : WhsSecureServiceTestCase
	{
		public void TestReceiveCanSetUnloadCompleteTime()
			=> TestReceiveCanSetUnloadCompleteTimeCore(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestReceiveCanSetUnloadCompleteTime_NotPlannedTask()
			=> TestReceiveCanSetUnloadCompleteTimeCore(isPlannedReceive: false, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestReceiveCanSetUnloadCompleteTime_NotWorkingTask()
			=> TestReceiveCanSetUnloadCompleteTimeCore(isPlannedReceive: true, isWorkingTask: false, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestReceiveCanSetUnloadCompleteTime_NotUnloadTask()
			=> TestReceiveCanSetUnloadCompleteTimeCore(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: false, isTaskForOtherUser: true);

		public void TestReceiveCanSetUnloadCompleteTime_WorkingTaskAssignedToCurrentUser()
			=> TestReceiveCanSetUnloadCompleteTimeCore(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: false);

		void TestReceiveCanSetUnloadCompleteTimeCore(bool isPlannedReceive, bool isWorkingTask, bool isUnloadTask, bool isTaskForOtherUser)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			if (isPlannedReceive)
			{
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			}
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, isTaskForOtherUser ? otherStaff : staff);
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

			var cannotSetUnloadCompleteTime = isPlannedReceive && isWorkingTask && isUnloadTask && isTaskForOtherUser;
			AssertEquals(!cannotSetUnloadCompleteTime, TaskManagementHelper.PlannedReceiveCanSetUnloadCompleteTime(receive, new BusinessObjectFactory(), staff));
		}

		public void TestGetJobTasksQuery()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var plannedReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			plannedReceive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			Helper.CreateWhsReceiveInventoryLine(plannedReceive, data.Part1, 10m);
			plannedReceive.AllocateLocationsWithMock();

			var unplannedReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(unplannedReceive, data.Part2, 10m);
			unplannedReceive.AllocateLocationsWithMock();
			Helper.Factory.Save();

			var workingTask = Helper.CreateProcessTaskForReceive(plannedReceive, staff);
			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var assignedTask = Helper.CreateProcessTaskForReceive(plannedReceive, staff);
			var openTask = Helper.CreateProcessTaskForReceive(plannedReceive);
			var notUnloadTask = Helper.CreateProcessTaskForReceive(plannedReceive);
			notUnloadTask.P9_FormFlowType = string.Empty;
			var taskForOtherReceive = Helper.CreateProcessTaskForReceive(unplannedReceive, staff);
			Helper.Factory.Save();

			AssertEquals("Precondition", plannedReceive.PK, workingTask.P9_ParentID);
			AssertEquals("Precondition", "S2", workingTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("Precondition", plannedReceive.PK, assignedTask.P9_ParentID);
			AssertEquals("Precondition", "S2", assignedTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("Precondition", plannedReceive.PK, openTask.P9_ParentID);
			AssertEquals("Precondition", string.Empty, openTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("Precondition", plannedReceive.PK, openTask.P9_ParentID);
			AssertEquals("Precondition", string.Empty, notUnloadTask.P9_FormFlowType);
			AssertNotEquals("Precondition", plannedReceive.PK, taskForOtherReceive.P9_ParentID);

			var newFactory = new BusinessObjectFactory();
			var plannedReceiveUnloadJobTaskPKs = newFactory.Load<ProcessTask>(TaskManagementHelper.GetJobTasksQuery(plannedReceive, WarehouseTaskFormFlowTypes.UnloadJob)).Select(t => t.PK).ToArray();
			AssertContainsExactElementsInAnyOrder("Only unload job tasks on the receive is returned.", [workingTask.PK, assignedTask.PK, openTask.PK], plannedReceiveUnloadJobTaskPKs);

			var plannedReceiveUnloadJobTaskPKs_SpecificUser = newFactory.Load<ProcessTask>(TaskManagementHelper.GetJobTasksQuery(plannedReceive, WarehouseTaskFormFlowTypes.UnloadJob, staff: staff)).Select(t => t.PK).ToArray();
			AssertContainsExactElementsInAnyOrder("Only unload job tasks on the receive assigned to the staff is returned.", [workingTask.PK, assignedTask.PK], plannedReceiveUnloadJobTaskPKs_SpecificUser);

			AssertEquals("No tasks are returned for mismatched docket and task form flow type.", 0, newFactory.Load<ProcessTask>(TaskManagementHelper.GetJobTasksQuery(plannedReceive, WarehouseTaskFormFlowTypes.TransferJob)).Length);
		}

		public void TestCloseRelatedTasks()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			var staff1 = Helper.CreateGlbStaff("US1", "User1");
			var task1 = Helper.CreateProcessTaskForReceive(receive, staff1);

			var staff2 = Helper.CreateGlbStaff("US2", "User2");
			var task2 = Helper.CreateProcessTaskForReceive(receive, staff2);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var staff3 = Helper.CreateGlbStaff("US3", "User3");
			var task3 = Helper.CreateProcessTaskForReceive(receive, staff3);
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			var task4 = Helper.CreateProcessTaskForReceive(receive);

			var task5 = Helper.CreateProcessTaskForReceive(receive);
			task5.P9_FormFlowType = "";

			Helper.Factory.Save();

			TaskManagementHelper.CloseRelatedTasks(Helper.Factory, receive.PK.ToGuid(), WarehouseTaskFormFlowTypes.UnloadJob);

			AssertEquals("Task1 is cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			AssertEquals("Task2 is still working", ProcessTaskStatusCodeList.Codes.Working, task2.P9_Status);
			AssertEquals("Task3 is closed", ProcessTaskStatusCodeList.Codes.Closed, task3.P9_Status);
			AssertEquals("Task4 is cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, task4.P9_Status);
			AssertEquals("Task5 is still open", ProcessTaskStatusCodeList.Codes.Open, task5.P9_Status);
		}
	}
}
