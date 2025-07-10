using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class WorkflowWithExtraTasksFilterStripsHelperTest : TestCaseWithFactory
	{
		#region Tasks

		public void TestTaskAssignedTo_WithCurrentFlag()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1_1 = dummy1.WorkflowItems.AddNew();
			task1_1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task2_1 = dummy2.WorkflowItems.AddNew();
			task2_1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2_2 = dummy2.WorkflowItems.AddNew();
			task2_2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");
			((ModuleNkFilter)filterStrip["Task Assigned To"]).Property = staff1.GS_Code;
			((ModuleNkFilter)filterStrip["Task Assigned To"]).IsActive = true;

			((ModuleFlagsFilter)filterStrip["Current Task Only"]).Property0 = true;
			((ModuleFlagsFilter)filterStrip["Current Task Only"]).IsActive = true;

			var bizos = Factory.Load<DummyWithWorkflow>(filterStrip.Filter);
			AssertArrayEqualsByElements("Should only contain the bizo that is current and assigned to staff1.", new[] { dummy1 }, bizos);
		}

		public void TestTaskAssignedTo_WithCurrentFlag_AndBlockedWorkflow()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1_1 = dummy1.WorkflowItems.AddNew();
			task1_1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var jobHeader1 = helper.GetJobHeaderForParent(dummy1, Factory, false);
			var workflow = helper.CreateWorkflow(jobHeader1, "workflow");
			task1_1.P9_FH_ProcessHeader = workflow.PK;

			var dummy1blocked = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1_1blocked = dummy1blocked.WorkflowItems.AddNew();
			task1_1blocked.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task1_1blocked.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var jobHeader2 = helper.GetJobHeaderForParent(dummy1blocked, Factory, false);
			var blockedWorkflow = helper.CreateWorkflow(jobHeader2, "blocked");
			task1_1blocked.P9_FH_ProcessHeader = blockedWorkflow.PK;

			helper.CreateLink(workflow, blockedWorkflow);

			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task2_1 = dummy2.WorkflowItems.AddNew();
			task2_1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2_2 = dummy2.WorkflowItems.AddNew();
			task2_2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			AssertEquals("OPN", workflow.FH_Status);
			AssertEquals("BLK", blockedWorkflow.FH_Status);

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");
			((ModuleNkFilter)filterStrip["Task Assigned To"]).Property = staff1.GS_Code;
			((ModuleNkFilter)filterStrip["Task Assigned To"]).IsActive = true;

			((ModuleFlagsFilter)filterStrip["Current Task Only"]).Property0 = true;
			((ModuleFlagsFilter)filterStrip["Current Task Only"]).IsActive = true;

			var bizos = Factory.Load<DummyWithWorkflow>(filterStrip.Filter);
			AssertArrayEqualsByElements("Should only contain the bizo that is current and assigned to staff1 and not contain the bizo that has the blocked workflow.", new[] { dummy1 }, bizos);
		}

		public void TestTaskGroup_WithCurrentFlag()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1_1 = dummy1.WorkflowItems.AddNew();
			task1_1.P9_GG_AssignedGroup = group1.PK;
			task1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task2_1 = dummy2.WorkflowItems.AddNew();
			task2_1.P9_GG_AssignedGroup = group1.PK;
			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2_2 = dummy2.WorkflowItems.AddNew();
			task2_2.P9_GG_AssignedGroup = group2.PK;
			task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");
			((ModuleGuidFilter)filterStrip["Task Assigned Group"]).Property = group1.PK;
			((ModuleGuidFilter)filterStrip["Task Assigned Group"]).IsActive = true;

			((ModuleFlagsFilter)filterStrip["Current Task Only"]).Property0 = true;
			((ModuleFlagsFilter)filterStrip["Current Task Only"]).IsActive = true;

			var bizos = Factory.Load<DummyWithWorkflow>(filterStrip.Filter);
			AssertArrayEqualsByElements("Should only contain the bizo that is current and assigned to staff1.", new[] { dummy1 }, bizos);
		}

		public void TestTaskGroup_WithCurrentFlag_AndBlockedWorkflow()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1_1 = dummy1.WorkflowItems.AddNew();
			task1_1.P9_GG_AssignedGroup = group1.PK;
			task1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var jobHeader1 = helper.GetJobHeaderForParent(dummy1, Factory, false);
			var workflow = helper.CreateWorkflow(jobHeader1, "workflow");
			task1_1.P9_FH_ProcessHeader = workflow.PK;

			var dummy1blocked = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1_1blocked = dummy1blocked.WorkflowItems.AddNew();
			task1_1blocked.P9_GG_AssignedGroup = group1.PK;
			task1_1blocked.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var jobHeader2 = helper.GetJobHeaderForParent(dummy1blocked, Factory, false);
			var blockedWorkflow = helper.CreateWorkflow(jobHeader2, "blocked");
			task1_1blocked.P9_FH_ProcessHeader = blockedWorkflow.PK;

			helper.CreateLink(workflow, blockedWorkflow);

			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task2_1 = dummy2.WorkflowItems.AddNew();
			task2_1.P9_GG_AssignedGroup = group1.PK;
			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2_2 = dummy2.WorkflowItems.AddNew();
			task2_2.P9_GG_AssignedGroup = group2.PK;
			task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			AssertEquals("OPN", workflow.FH_Status);
			AssertEquals("BLK", blockedWorkflow.FH_Status);

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");
			((ModuleGuidFilter)filterStrip["Task Assigned Group"]).Property = group1.PK;
			((ModuleGuidFilter)filterStrip["Task Assigned Group"]).IsActive = true;

			((ModuleFlagsFilter)filterStrip["Current Task Only"]).Property0 = true;
			((ModuleFlagsFilter)filterStrip["Current Task Only"]).IsActive = true;

			var bizos = Factory.Load<DummyWithWorkflow>(filterStrip.Filter);
			AssertArrayEqualsByElements("Should only contain the bizo that is current and assigned to staff1 and not contain the bizo that has the blocked workflow.", new[] { dummy1 }, bizos);
		}

		public void TestTaskAssignedToFilter()
		{
			GlbStaff person1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff person2 = Factory.NewWithValidTestData<GlbStaff>();

			var parentAssignedBoth = Factory.NewWithValidTestData<DummyWithWorkflow>();
			ProcessTask task1 = parentAssignedBoth.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = person1.GS_Code;
			ProcessTask task2 = parentAssignedBoth.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = person2.GS_Code;

			DummyWithWorkflow parentWithClosedCancelledAndSuspendedTasks = Factory.NewWithValidTestData<DummyWithWorkflow>();
			ProcessTask closedTask = parentWithClosedCancelledAndSuspendedTasks.WorkflowItems.AddNew();
			closedTask.P9_GS_NKAssignedStaffMember = person2.GS_Code;
			closedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask cancelledTask = parentWithClosedCancelledAndSuspendedTasks.WorkflowItems.AddNew();
			cancelledTask.P9_GS_NKAssignedStaffMember = person2.GS_Code;
			cancelledTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			ProcessTask suspendedTask1 = parentWithClosedCancelledAndSuspendedTasks.WorkflowItems.AddNew();
			suspendedTask1.P9_GS_NKAssignedStaffMember = person1.GS_Code;
			suspendedTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			DummyWithWorkflow parentAssigned2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			ProcessTask assignedTask = parentAssigned2.WorkflowItems.AddNew();
			assignedTask.P9_GS_NKAssignedStaffMember = person2.GS_Code;

			DummyWithWorkflow parentMilestoneAssigned1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var mileStone = parentMilestoneAssigned1.WorkflowItems.Milestones.AddNew();
			mileStone.P9_GS_NKAssignedStaffMember = person1.GS_Code;

			Factory.Save();

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");
			((ModuleNkFilter)filterStrip["Task Assigned To"]).Property = person1.GS_Code;
			((ModuleNkFilter)filterStrip["Task Assigned To"]).IsActive = true;

			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();

			Assert(dummyCollection.Contains(parentAssignedBoth));
			Assert(dummyCollection.Contains(parentWithClosedCancelledAndSuspendedTasks));
			Assert(!dummyCollection.Contains(parentAssigned2));
			Assert(!dummyCollection.Contains(parentMilestoneAssigned1));
			AssertEquals(2, dummyCollection.Count);

			((ModuleNkFilter)filterStrip["Task Assigned To"]).Property = person2.GS_Code;
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			Assert(dummyCollection.Contains(parentAssignedBoth));
			Assert(dummyCollection.Contains(parentWithClosedCancelledAndSuspendedTasks));
			Assert(dummyCollection.Contains(parentAssigned2));
			Assert(!dummyCollection.Contains(parentMilestoneAssigned1));
			AssertEquals(3, dummyCollection.Count);
		}

		public void TestTaskAssignedToFilter_Multi()
		{
			GlbStaff person1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff person2 = Factory.NewWithValidTestData<GlbStaff>();

			var parentAssignedBoth = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1_1 = CreateTask(parentAssignedBoth, person1.GS_Code);
			var task1_2 = CreateTask(parentAssignedBoth, person2.GS_Code);

			var parentAssignedSingle = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task2_1 = CreateTask(parentAssignedSingle, person1.GS_Code);

			Factory.Save();

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");
			var strip1 = new FilterStrip(filterStrip.ModuleFilters);
			strip1.FilterDescription = "Task Assigned To";
			var strip2 = new FilterStrip(filterStrip.ModuleFilters);
			strip2.FilterDescription = "Task Assigned To";

			strip1.CurrentModuleFilter.IsActive = true;
			((ModuleNkFilter)strip1.CurrentModuleFilter).Property = person1.GS_Code;
			strip2.CurrentModuleFilter.IsActive = true;
			((ModuleNkFilter)strip2.CurrentModuleFilter).Property = person2.GS_Code;

			AssertContainsExactElementsInAnyOrder(new[] { parentAssignedBoth }, Factory.Load<DummyWithWorkflow>(filterStrip.Filter));
		}

		ProcessTask CreateTask(IWorkflowProvider provider, string staffCode = "")
		{
			var task = provider.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = staffCode;
			return task;
		}

		public void TestTaskStatusFilter()
		{
			var openParent = Factory.NewWithValidTestData<DummyWithWorkflow>();
			openParent.WorkflowItems.AddNew().P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var workingParent = Factory.NewWithValidTestData<DummyWithWorkflow>();
			workingParent.WorkflowItems.AddNew().P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var closedParent = Factory.NewWithValidTestData<DummyWithWorkflow>();
			closedParent.WorkflowItems.AddNew().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");

			((ModuleTextFilter)filterStrip["Task Status"]).Property = ProcessTaskStatusCodeList.Codes.Open;
			((ModuleTextFilter)filterStrip["Task Status"]).IsActive = true;
			var dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(1, dummyCollection.Count);
			Assert(dummyCollection.Contains(openParent));

			((ModuleTextFilter)filterStrip["Task Status"]).Property = ProcessTaskStatusCodeList.Codes.Closed;
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load(filterStrip.Filter);
			AssertEquals(1, dummyCollection.Count);
			Assert(dummyCollection.Contains(closedParent));

			((ModuleTextFilter)filterStrip["Task Status"]).Property = "NCM";
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load(filterStrip.Filter);
			AssertEquals(2, dummyCollection.Count);
			Assert(dummyCollection.Contains(openParent));
			Assert(dummyCollection.Contains(workingParent));

			((ModuleTextFilter)filterStrip["Task Status"]).Property = "";
			dummyCollection.Load(filterStrip.Filter);
			AssertEquals(3, dummyCollection.Count);
		}

		public void TestTaskStatusFilter_WithCurrentFlag()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var openParent = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = openParent.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var workingParent = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task2 = workingParent.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var closedParent = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task3 = closedParent.WorkflowItems.AddNew();
			task3.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");

			((ModuleTextFilter)filterStrip["Task Status"]).Property = ProcessTaskStatusCodeList.Codes.Open;
			((ModuleTextFilter)filterStrip["Task Status"]).IsActive = true;
			((ModuleFlagsFilter)filterStrip["Current Task Only"]).Property0 = true;
			((ModuleFlagsFilter)filterStrip["Current Task Only"]).IsActive = true;
			var dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(1, dummyCollection.Count);

			((ModuleTextFilter)filterStrip["Task Status"]).Property = ProcessTaskStatusCodeList.Codes.Closed;
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load(filterStrip.Filter);
			AssertEquals("Closed tasks are not startable", 0, dummyCollection.Count);

			((ModuleTextFilter)filterStrip["Task Status"]).Property = "NCM";
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load(filterStrip.Filter);
			AssertEquals(2, dummyCollection.Count);
			Assert(dummyCollection.Contains(openParent));
			Assert(dummyCollection.Contains(workingParent));

			((ModuleTextFilter)filterStrip["Task Status"]).Property = "";
			dummyCollection.Load(filterStrip.Filter);
			AssertEquals(2, dummyCollection.Count);
			Assert(dummyCollection.Contains(openParent));
			Assert(dummyCollection.Contains(workingParent));
		}

		public void TestTaskStatusFilter_WithCurrentFlag_AndBlockedWorkflow()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var openJob = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = openJob.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var jobHeader = helper.GetJobHeaderForParent(openJob, Factory, false);
			var workflow = helper.CreateWorkflow(jobHeader, "workflow");
			task1.P9_FH_ProcessHeader = workflow.PK;

			var blockedJob = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task2 = blockedJob.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var jobHeader2 = helper.GetJobHeaderForParent(blockedJob, Factory, false);
			var blockedWorkflow = helper.CreateWorkflow(jobHeader2, "blocked");
			task2.P9_FH_ProcessHeader = blockedWorkflow.PK;

			helper.CreateLink(workflow, blockedWorkflow);

			Factory.Save();

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");

			((ModuleTextFilter)filterStrip["Task Status"]).Property = ProcessTaskStatusCodeList.Codes.Open;
			((ModuleTextFilter)filterStrip["Task Status"]).IsActive = true;
			((ModuleFlagsFilter)filterStrip["Current Task Only"]).Property0 = true;
			((ModuleFlagsFilter)filterStrip["Current Task Only"]).IsActive = true;
			var dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(1, dummyCollection.Count);
			Assert(dummyCollection.Contains(openJob));
		}

		public void TestTaskTypeFilter()
		{
			var parent1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = parent1.WorkflowItems.Tasks.AddNew();
			task1.P9_Type = "AAA";

			var parent2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task2a = parent2.WorkflowItems.Tasks.AddNew();
			task2a.P9_Type = "BBB";
			var task2b = parent2.WorkflowItems.Tasks.AddNew();
			task2b.P9_Type = "CC1";

			var parent3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task3a = parent3.WorkflowItems.Tasks.AddNew();
			task3a.P9_Type = "CC2";
			var task3b = parent3.WorkflowItems.Tasks.AddNew();
			task3b.P9_Type = "AAA";

			Factory.Save();

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");
			var filter = (ModuleTextFilter)filterStrip["Task Type"];
			filter.Property = "AAA";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(2, dummyCollection.Count);
			Assert(dummyCollection.Contains(parent1));
			Assert(dummyCollection.Contains(parent3));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(2, dummyCollection.Count);
			Assert(dummyCollection.Contains(parent2));
			Assert(dummyCollection.Contains(parent3));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "CC";
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(2, dummyCollection.Count);
			Assert(dummyCollection.Contains(parent2));
			Assert(dummyCollection.Contains(parent3));

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(3, dummyCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "1";
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(1, dummyCollection.Count);
			Assert(dummyCollection.Contains(parent2));
		}

		public void TestTaskTypeFilter_WithCurrentFlag()
		{
			var parent1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = parent1.WorkflowItems.Tasks.AddNew();
			task1.P9_Type = "AAA";

			var parent2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task2a = parent2.WorkflowItems.Tasks.AddNew();
			task2a.P9_Type = "BBB";
			task2a.P9_Sequence = 1;
			var task2b = parent2.WorkflowItems.Tasks.AddNew();
			task2b.P9_Type = "CC1";
			task2b.P9_Sequence = 2;

			var parent3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task3a = parent3.WorkflowItems.Tasks.AddNew();
			task3a.P9_Type = "CC2";
			task3a.P9_Sequence = 1;
			var task3b = parent3.WorkflowItems.Tasks.AddNew();
			task3b.P9_Type = "AAA";
			task3b.P9_Sequence = 2;

			Factory.Save();

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");
			var filter = (ModuleTextFilter)filterStrip["Task Type"];
			filter.Property = "AAA";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			((ModuleFlagsFilter)filterStrip["Current Task Only"]).Property0 = true;
			((ModuleFlagsFilter)filterStrip["Current Task Only"]).IsActive = true;

			var dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(1, dummyCollection.Count);
			AssertEquals(true, dummyCollection.Contains(parent1));
			AssertEquals(false, dummyCollection.Contains(parent3));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(2, dummyCollection.Count);
			AssertEquals(true, dummyCollection.Contains(parent2));
			AssertEquals(true, dummyCollection.Contains(parent3));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "CC";
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(1, dummyCollection.Count);
			AssertEquals(false, dummyCollection.Contains(parent2));
			AssertEquals(true, dummyCollection.Contains(parent3));

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(2, dummyCollection.Count);
			AssertEquals(true, dummyCollection.Contains(parent1));
			AssertEquals(true, dummyCollection.Contains(parent2));
			AssertEquals(false, dummyCollection.Contains(parent3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "1";
			dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(0, dummyCollection.Count);
		}

		public void TestTaskTypeFilter_WithCurrentFlag_AndBlockedWorkflow()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var openJob = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = openJob.WorkflowItems.AddNew();
			task1.P9_Type = "AAA";
			var jobHeader = helper.GetJobHeaderForParent(openJob, Factory, false);
			var workflow = helper.CreateWorkflow(jobHeader, "workflow");
			task1.P9_FH_ProcessHeader = workflow.PK;

			var blockedJob = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task2 = blockedJob.WorkflowItems.AddNew();
			task2.P9_Type = "AAA";
			var jobHeader2 = helper.GetJobHeaderForParent(blockedJob, Factory, false);
			var blockedWorkflow = helper.CreateWorkflow(jobHeader2, "blocked");
			task2.P9_FH_ProcessHeader = blockedWorkflow.PK;

			helper.CreateLink(workflow, blockedWorkflow);

			Factory.Save();

			var filterStrip = new DummyFilterStripBizOWithTaskFilters("");
			((ModuleTextFilter)filterStrip["Task Type"]).Property = "AAA";
			((ModuleTextFilter)filterStrip["Task Type"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			((ModuleTextFilter)filterStrip["Task Type"]).IsActive = true;
			((ModuleFlagsFilter)filterStrip["Current Task Only"]).Property0 = true;
			((ModuleFlagsFilter)filterStrip["Current Task Only"]).IsActive = true;

			var dummyCollection = new DummyBusinessObjectCollection(Factory, filterStrip.Filter);
			dummyCollection.Load();
			AssertEquals(1, dummyCollection.Count);
			Assert(dummyCollection.Contains(openJob));
		}

		#endregion
	}
}
