using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	class FirstMeaningfulApprovalTasksEmailAddressTest : TestCaseWithFactory
	{
		public void TestNoApprovalTaskReturnsEmpty()
		{
			TestCore((staff, holiday) =>
			{
				AddNewTask(holiday, NonApprovalTaskCode);
			}, Array.Empty<string>());
		}

		public void TestUnassignedApprovalTaskReturnsEmpty()
		{
			TestCore((staff, holiday) =>
			{
				AddNewTask(holiday, ApprovalTaskCode);
			}, Array.Empty<string>());
		}

		public void TestSingleAssignedApprovalTaskReturnsEmail() => SingleApprovalTaskTest(ProcessTaskStatusCodeList.Codes.Assigned, new string[] { "1@1.com" });
		public void TestSingleClosedApprovalTaskReturnsEmail() => SingleApprovalTaskTest(ProcessTaskStatusCodeList.Codes.Closed, new string[] { "1@1.com" });
		public void TestSingleOpenApprovalTaskReturnsEmail() => SingleApprovalTaskTest(ProcessTaskStatusCodeList.Codes.Cancelled, new string[] { "1@1.com" });
		public void TestSingleSuspendedApprovalTaskReturnsEmail() => SingleApprovalTaskTest(ProcessTaskStatusCodeList.Codes.Suspended, Array.Empty<string>());
		public void TestSingleWorkingApprovalTaskReturnsEmail() => SingleApprovalTaskTest(ProcessTaskStatusCodeList.Codes.Working, Array.Empty<string>());

		public void SingleApprovalTaskTest(string status, string[] expected)
		{
			TestCore((staff, holiday) =>
			{
				var assignedStaff = CreateStaff();
				AddNewTask(holiday, ApprovalTaskCode, status: status, assignedStaff: assignedStaff);
			}, expected);
		}

		public void TestAllAssignedReportsAll() => RunMultipleTaskTest(new Tuple<string, string>[]
		{
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Assigned),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Assigned),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Assigned),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Assigned),
		}, new string[] { "1@1.com", "2@2.com", "3@3.com", "4@4.com" });

		public void TestAllCloseReportsAll() => RunMultipleTaskTest(new Tuple<string, string>[]
		{
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Closed),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Closed),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Closed),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Closed),
		}, new string[] { "1@1.com", "2@2.com", "3@3.com", "4@4.com" });

		public void TestAllCancelledReportsAll() => RunMultipleTaskTest(new Tuple<string, string>[]
		{
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Cancelled),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Cancelled),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Cancelled),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Cancelled),
		}, new string[] { "1@1.com", "2@2.com", "3@3.com", "4@4.com" });

		public void TestAllSuspendedReportsNone() => RunMultipleTaskTest(new Tuple<string, string>[]
		{
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Suspended),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Suspended),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Suspended),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Suspended),
		}, Array.Empty<string>());

		public void TestAllWorkingReportsNone() => RunMultipleTaskTest(new Tuple<string, string>[]
		{
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Working),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Working),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Working),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Working),
		}, Array.Empty<string>());

		public void TestMixApprovalStatusReportsOnlyValid() => RunMultipleTaskTest(new Tuple<string, string>[]
		{
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Assigned),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Closed),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Cancelled),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Suspended),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Working),
		}, new string[] { "1@1.com", "2@2.com", "3@3.com" });

		public void TestMixNonApprovalStatusReportsNone() => RunMultipleTaskTest(new Tuple<string, string>[]
		{
			new Tuple<string, string>(NonApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Assigned),
			new Tuple<string, string>(NonApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Closed),
			new Tuple<string, string>(NonApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Cancelled),
			new Tuple<string, string>(NonApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Suspended),
			new Tuple<string, string>(NonApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Working),
		}, Array.Empty<string>());

		public void TestMixNonApprovalAndApprovalStatusReportsOnlyValidApprovals() => RunMultipleTaskTest(new Tuple<string, string>[]
		{
			new Tuple<string, string>(NonApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Assigned),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Closed),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Cancelled),
			new Tuple<string, string>(NonApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Suspended),
			new Tuple<string, string>(ApprovalTaskCode, ProcessTaskStatusCodeList.Codes.Working),
		}, new string[] { "2@2.com", "3@3.com" });

		void RunMultipleTaskTest(Tuple<string, string>[] taskSettings, string[] expected)
		{
			TestCore((staff, holiday) =>
			{
				foreach (var taskSetting in taskSettings)
				{
					AddNewTask(holiday, taskSetting.Item1, assignedStaff: CreateStaff(), status: taskSetting.Item2);
				}
			}, expected);
		}

		GlbStaffHolidayProcessTask AddNewTask(GlbStaffHoliday holiday, string taskType, string status = ProcessTaskStatusCodeList.Codes.Open, GlbStaff assignedStaff = null)
		{
			var task = holiday.WorkflowItems.AddNew();
			task.P9_Type = taskType;
			task.P9_Sequence = seqCounter++;
			task.P9_Status = status;
			if (assignedStaff != null)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				task.P9_GS_NKAssignedStaffMember = assignedStaff.GS_Code;
				if (status != ProcessTaskStatusCodeList.Codes.Open)
				{
					task.P9_Status = status;
				}
			}
			return task;
		}

		GlbStaff CreateStaff()
		{
			staffCounter++;
			var assignedStaff = Factory.NewWithValidTestData<GlbStaff>();
			assignedStaff.GS_EmailAddress = $"{staffCounter}@{staffCounter}.com";
			return assignedStaff;
		}

		void TestCore(Action<GlbStaff, GlbStaffHoliday> setup, string[] expectedEmail)
		{
			SetupRegistryItems();
			seqCounter = 0;
			staffCounter = 0;
			var descriptor = new GlbStaffHolidayWorkflowDescriptor();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var holiday = staff.Holidays.AddNew();

			setup(staff, holiday);

			var actualEmail = FirstMeaningfulApprovalTasksEmailAddress.GetEmail(holiday, descriptor, Factory);
			AssertArrayEqualsByElements(expectedEmail.OrderBy(s => s).ToArray(), actualEmail.OrderBy(s => s).ToArray());
		}

		void SetupRegistryItems()
		{
			var registryItem = new WorkflowTaskTypesRegistryItem("ProcessManagerTaskTypes",
				RawDataRegistry.Categories.WorkflowManager,
				null,
				null,
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
			var collection = registryItem.Value;
			var workflowTaskTypes = collection.GetTaskTypesFromWorkflowCode(GlbStaffHolidayWorkflowDescriptor.WorkflowDescriptorGlbStaffHolidayDescriptorCode);

			var someTask = workflowTaskTypes.AddNew();
			someTask.Code = NonApprovalTaskCode;

			var approvalTask = workflowTaskTypes.AddNew();
			approvalTask.Code = ApprovalTaskCode;
			approvalTask.IsApprovalTask = true;

			var someOtherTask = workflowTaskTypes.AddNew();
			someOtherTask.Code = SomeOtherTaskCode;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}
		int seqCounter;
		int staffCounter;
		const string ApprovalTaskCode = "APT";
		const string NonApprovalTaskCode = "SOT";
		const string SomeOtherTaskCode = "STT";
	}
}
