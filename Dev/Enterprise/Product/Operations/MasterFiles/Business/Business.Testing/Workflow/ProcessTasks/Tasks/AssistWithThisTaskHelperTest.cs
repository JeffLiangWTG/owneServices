using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AssistWithThisTaskHelperTest : TestCaseWithFactory
	{
		public void TestCreateAssistTask_ShouldGetValuesFromRegistry()
		{
			var currentUser = Env.CurrentUser.Initials;
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			var orgHeader = Factory.New<OrgHeader>();
			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;
			task.P9_EstDuration = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1, 1, 0, 0);
			task.P9_EstimateVariationFactor = 4;
			task.P9_Sequence = 5;
			AssertNull("Precondition: Pave isn't enabled for this test so the ProcessHeader should be null.", task.ProcessHeader);

			Assert(AssistWithThisTaskHelper.TryCreateAssistTaskOrGetExistingOne(task, out IProcessTask resultingTask));
			var assistTask = (ProcessTask)resultingTask;
			AssertEquals(orgHeader, assistTask.Parent);
			AssertEquals("OH", assistTask.P9_ParentTableCode);
			AssertEquals("AST", assistTask.P9_Type);
			AssertEquals(15, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertEquals(3m, assistTask.P9_EstimateVariationFactor);
			AssertEquals(5, assistTask.P9_Sequence);
			AssertEquals(currentUser, assistTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assistTask.P9_Status);
			AssertEquals("Assist", assistTask.P9_Description);
			AssertNull(assistTask.ProcessHeader);
		}

		public void TestCreateAssistTask_ForTaskWithProcessHeader()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "ORG");

			var currentUser = Env.CurrentUser.Initials;
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			var orgHeader = Factory.New<OrgHeader>();
			var jobLevelWorkflow = helper.GetJobHeaderForParent(orgHeader, Factory);
			var workflow = jobLevelWorkflow.ProcessHeaders[0];

			var task = helper.CreateTask(workflow, otherUser, 60, "INV", sequence: 4);

			Assert(AssistWithThisTaskHelper.TryCreateAssistTaskOrGetExistingOne(task, out IProcessTask resultingTask));
			var assistTask = (ProcessTask)resultingTask;
			AssertEquals(orgHeader, assistTask.Parent);
			AssertEquals("OH", assistTask.P9_ParentTableCode);
			AssertEquals("AST", assistTask.P9_Type);
			AssertEquals(15, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertEquals(3m, assistTask.P9_EstimateVariationFactor);
			AssertEquals(4, assistTask.P9_Sequence);
			AssertEquals(currentUser, assistTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assistTask.P9_Status);
			AssertEquals("Assist", assistTask.P9_Description);
			AssertEquals(workflow, assistTask.ProcessHeader);
		}

		public void TestCreateAssistTask_ShouldNotCopyP9_TaskCannotBeDeletedFromTargetTask()
		{
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			var orgHeader = Factory.New<OrgHeader>();
			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_TaskCannotBeDeleted = true;
			task.P9_GS_NKAssignedStaffMember = otherUser;

			Assert(AssistWithThisTaskHelper.TryCreateAssistTaskOrGetExistingOne(task, out IProcessTask resultingTask));
			var assistTask = (ProcessTask)resultingTask;
			AssertEquals("Should not copy P9_TaskCannotBeDeleted", false, assistTask.P9_TaskCannotBeDeleted);
		}

		public void TestCreateAssistTask_WhenNotConfiguredInRegistry_ShouldThrowException()
		{
			MasterFilesTestHelper.AddTaskTypesToRegistry("DUM", "INV", "AST");
			var registryItem = WorkflowDataRegistry.Instance.AssistWithThisTask.Value;
			registryItem.Remove(registryItem.GetTaskDetailsForWorkflowType("DUM"));
			WorkflowDataRegistry.Instance.AssistWithThisTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem);

			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "AST";
			task.P9_GS_NKAssignedStaffMember = otherUser;

			var exception = AssertExceptionThrown<InvalidOperationException>("This functionality should never be available for task types that haven't been set up in the registry yet.",
				"Assist With This Task / Add Assistance Task For has not been set up in the registry. This functionality should not be available. See exception Data for workflow and task type.",
				() => AssistWithThisTaskHelper.TryCreateAssistTaskOrGetExistingOne(task, out IProcessTask resultingTask));

			AssertEquals("DUM", exception.Data["WorkflowType"]);
			AssertEquals("AST", exception.Data["TaskType"]);
		}

		public void TestCreateAssistTask_WhenExistingTaskAssignedToCurrentUser_ShouldThrowException()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;

			var exception = AssertExceptionThrown<InvalidOperationException>("This functionality should never be available for tasks assigned to the current user.",
				"Assist With This Task / Add Assistance Task For is only valid on tasks assigned to someone other than the currently assigned user. See exception Data for task id.",
				() => AssistWithThisTaskHelper.TryCreateAssistTaskOrGetExistingOne(task, out IProcessTask resultingTask));

			AssertEquals(task.P9_TaskID, exception.Data["TaskID"]);
		}

		public void TestCreateAssistTask_ForStandaloneTask_ShouldThrowException()
		{
			var task = Factory.New<ProcessTask>();
			AssertExceptionThrown<InvalidOperationException>("This feature isn't supported for standalone tasks at this time.", "Assist With This Task is not available for standalone tasks.",
				() => AssistWithThisTaskHelper.TryCreateAssistTaskOrGetExistingOne(task, out IProcessTask resultingTask));
		}

		public void TestCreateAssistTask_ShouldNotSaveFactory()
		{
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			var orgHeader = Factory.New<OrgHeader>();
			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;

			Assert(AssistWithThisTaskHelper.TryCreateAssistTaskOrGetExistingOne(task, out IProcessTask resultingTask));
			var assistTask = (ProcessTask)resultingTask;
			AssertNotNull(assistTask);
			AssertEquals("The helper should not save any factory, sometime implementations will do this and others won't.", false, orgHeader.IsInDatabase);
		}

		public void TestCreateAssistTask_PropertiesThatShouldNotBeCopied()
		{
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var orgHeader = Factory.New<OrgHeader>();
			var task = orgHeader.WorkflowItems.Tasks.AddNew();
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = otherUser;
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_GG_AssignedGroup = group.PK;
			task.P9_CardNote = "Card note";
			task.P9_NotesAsString = "Some notes here, maybe some DAT instructions, etc.";

			Assert(AssistWithThisTaskHelper.TryCreateAssistTaskOrGetExistingOne(task, out IProcessTask resultingTask));
			var assistTask = (ProcessTask)resultingTask;
			AssertEquals(Env.CurrentUser.Initials, assistTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("We're assigning the current user directly, so capability isn't needed, and that user may not even be a member of that capability.", ZGuid.Empty, assistTask.P9_G4_RequiredCapability);
			AssertEquals("We're assigning the current user directly, so group isn't needed, and that user may not even be a member of that group.", ZGuid.Empty, assistTask.P9_GG_AssignedGroup);
			AssertEquals(ZString.Empty, assistTask.P9_CardNote);
			AssertEquals(ZString.Empty, assistTask.P9_NotesAsString);
		}

		public void TestGetStaffCodesAndDescriptionsForAssistanceTask_ShouldNotThrowNullReferenceException()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "DUM");

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobLevelWorkflow = helper.GetJobHeaderForParent(dummy, Factory);
			var workflow = jobLevelWorkflow.ProcessHeaders[0];
			var assignedUser = Factory.NewWithValidTestData<GlbStaff>();
			var taskToCreateAssistTaskFor = (ProcessTask)helper.CreateTask(workflow, assignedUser.GS_Code, 60, "INV", sequence: 1);

			AssertNoExceptionThrown(() => AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(null, null));
			AssertNoExceptionThrown(() => AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(null, dummy.WorkflowItems));
			AssertNoExceptionThrown(() => AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, null));

			var taskNoUser = (ProcessTask)helper.CreateTask(workflow, sequence: 2);
			taskNoUser.P9_GS_NKAssignedStaffMember = "NOP";

			var processTaskCollection = new ProcessTaskCollection(Factory);
			processTaskCollection.Add(taskNoUser);

			AssertNoExceptionThrown(() => AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, processTaskCollection));

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			capability.G4_Code = "CAP";

			var capabilityTaskWithoutProcessHeader = (ProcessTask)helper.CreateTask(workflow, "INV", sequence: 3, capability: capability);
			capabilityTaskWithoutProcessHeader.P9_FH_ProcessHeader = ZGuid.NewZGuid();

			processTaskCollection.Add(capabilityTaskWithoutProcessHeader);

			AssertNoExceptionThrown(() => AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, processTaskCollection));

			Env.ClearUserContext();

			AssertNoExceptionThrown(() => AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, processTaskCollection));
		}

		public void TestGetStaffCodesAndDescriptionsForAssistanceTask_GlobalCapabilityScope()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "DUM");

			var assignedUser = Factory.NewWithValidTestData<GlbStaff>();
			assignedUser.GS_Code = "ASS";
			assignedUser.GS_FullName = "Assigned User";

			var currentUser = Factory.NewWithValidTestData<GlbStaff>();
			currentUser.GS_Code = "CUR";
			currentUser.GS_FullName = "Current User";
			currentUser.GS_LoginName = "wordtoyamutha";
			currentUser.Language = Env.CurrentUser.Language;

			var staffUser1 = Factory.NewWithValidTestData<GlbStaff>();
			staffUser1.GS_Code = "ST1";
			staffUser1.GS_FullName = "Staff User The First";

			var staffUser2 = Factory.NewWithValidTestData<GlbStaff>();
			staffUser2.GS_Code = "ST2";
			staffUser2.GS_FullName = "Staff User The Second";

			var staffUser3 = Factory.NewWithValidTestData<GlbStaff>();
			staffUser3.GS_Code = "ST3";
			staffUser3.GS_FullName = "Staff User The Third";

			var creatingUser = Factory.NewWithValidTestData<GlbStaff>();
			creatingUser.GS_Code = "CRE";
			creatingUser.GS_FullName = "The Creator";

			var taskCapability1 = Factory.NewWithValidTestData<GlbCapability>();
			taskCapability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			taskCapability1.G4_Code = "CAP";

			var taskCapability2 = Factory.NewWithValidTestData<GlbCapability>();
			taskCapability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			taskCapability2.G4_Code = "CPA";

			var standaloneCapability = Factory.NewWithValidTestData<GlbCapability>();
			standaloneCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			standaloneCapability.G4_Code = "CAS";

			var capUser1 = Factory.NewWithValidTestData<GlbStaff>();
			capUser1.GS_Code = "CA1";
			capUser1.GS_FullName = "Capability User The First";

			var capUser2 = Factory.NewWithValidTestData<GlbStaff>();
			capUser2.GS_Code = "CA2";
			capUser2.GS_FullName = "Capability User The Second";

			var capUser3 = Factory.NewWithValidTestData<GlbStaff>();
			capUser3.GS_Code = "CA3";
			capUser3.GS_FullName = "Capability User The Third";

			var pivot1 = capUser1.CapabilityPivots.AddNew();
			pivot1.G5_G4_Capability = taskCapability1.PK;
			var pivot2 = capUser2.CapabilityPivots.AddNew();
			pivot2.G5_G4_Capability = taskCapability1.PK;
			var pivot3 = capUser3.CapabilityPivots.AddNew();
			pivot3.G5_G4_Capability = taskCapability2.PK;
			var pivot4 = staffUser1.CapabilityPivots.AddNew();
			pivot4.G5_G4_Capability = taskCapability1.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var dummy = Factory.New<DummyWithWorkflow>();
				var jobLevelWorkflow = helper.GetJobHeaderForParent(dummy, Factory);
				var workflow1 = jobLevelWorkflow.ProcessHeaders[0];
				var workflow2 = helper.CreateWorkflow(jobLevelWorkflow, "More work");

				var taskToCreateAssistTaskFor = (ProcessTask)helper.CreateTask(workflow1, assignedUser.GS_Code, 60, "INV", sequence: 1);
				taskToCreateAssistTaskFor.P9_SystemCreateUser = creatingUser.GS_Code;

				var cw1UserTask = (ProcessTask)helper.CreateTask(workflow1, "E", 60, "INV", sequence: 3, capability: taskCapability1);
				var serviceUserTask = (ProcessTask)helper.CreateTask(workflow1, "~BP", 60, "INV", sequence: 4, capability: taskCapability1);
				var staffUserTask1 = (ProcessTask)helper.CreateTask(workflow1, staffUser1.GS_Code, 60, "INV", sequence: 5, capability: taskCapability1);
				var staffUserTask2 = (ProcessTask)helper.CreateTask(workflow2, staffUser2.GS_Code, 60, "INV", sequence: 1, capability: taskCapability2);
				var staffUserTask3 = (ProcessTask)helper.CreateTask(workflow2, staffUser3.GS_Code, 60, "INV", sequence: 2, capability: taskCapability1);

				Factory.Save();

				AssertContainsExactElementsInExactOrder("No capability has been assigned to the task, nevertheless all staff belonging to a capability on the job should appear in the list.",
					new string[]
					{
						"~BP - CargoWise Service",
						"CA1 - Capability User The First",
						"CA2 - Capability User The Second",
						"CA3 - Capability User The Third",
						"CRE - The Creator",
						"CUR - Current User",
						"E - CargoWise Support",
						"ST1 - Staff User The First",
						"ST2 - Staff User The Second",
						"ST3 - Staff User The Third",
					},
					AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, dummy.WorkflowItems).Select(s => s.MenuItemDescription.ToString()));

				taskToCreateAssistTaskFor.P9_G4_RequiredCapability = taskCapability1.PK;

				AssertContainsExactElementsInExactOrder("Even with a capability assigned to the task, all staff belonging to a capability on the job should appear in the list and no users should appear more than once",
					new string[]
					{
						"~BP - CargoWise Service",
						"CA1 - Capability User The First",
						"CA2 - Capability User The Second",
						"CA3 - Capability User The Third",
						"CRE - The Creator",
						"CUR - Current User",
						"E - CargoWise Support",
						"ST1 - Staff User The First",
						"ST2 - Staff User The Second",
						"ST3 - Staff User The Third",
					},
					AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, dummy.WorkflowItems).Select(s => s.MenuItemDescription.ToString()));

				creatingUser.GS_IsActive = false;
				Factory.Save();

				AssertContainsExactElementsInExactOrder("Deactivating the task creator should remove it from the list.",
					new string[]
					{
						"~BP - CargoWise Service",
						"CA1 - Capability User The First",
						"CA2 - Capability User The Second",
						"CA3 - Capability User The Third",
						"CUR - Current User",
						"E - CargoWise Support",
						"ST1 - Staff User The First",
						"ST2 - Staff User The Second",
						"ST3 - Staff User The Third",
					},
					AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, dummy.WorkflowItems).Select(s => s.MenuItemDescription.ToString()));
			}
		}

		public void TestGetStaffCodesAndDescriptionsForAssistanceTask_GroupCapabilityScope()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "DUM");

			var assignedUser = Factory.NewWithValidTestData<GlbStaff>();
			assignedUser.GS_Code = "ASS";
			assignedUser.GS_FullName = "Assigned User";

			var currentUser = Factory.NewWithValidTestData<GlbStaff>();
			currentUser.GS_Code = "CUR";
			currentUser.GS_FullName = "Current User";
			currentUser.GS_LoginName = "wordtoyamutha";
			currentUser.Language = Env.CurrentUser.Language;

			var creatingUser = Factory.NewWithValidTestData<GlbStaff>();
			creatingUser.GS_Code = "CRE";
			creatingUser.GS_FullName = "The Creator";

			var staffUser1 = Factory.NewWithValidTestData<GlbStaff>();
			staffUser1.GS_Code = "ST1";
			staffUser1.GS_FullName = "Staff User The First";

			var staffUser2 = Factory.NewWithValidTestData<GlbStaff>();
			staffUser2.GS_Code = "ST2";
			staffUser2.GS_FullName = "Staff User The Second";

			var staffUser3 = Factory.NewWithValidTestData<GlbStaff>();
			staffUser3.GS_Code = "ST3";
			staffUser3.GS_FullName = "Staff User The Third";

			var directlyAssignedCapability = Factory.NewWithValidTestData<GlbCapability>();
			directlyAssignedCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			directlyAssignedCapability.G4_Code = "CDA";

			var commonUser1 = Factory.NewWithValidTestData<GlbStaff>();
			commonUser1.GS_Code = "CO1";
			commonUser1.GS_FullName = "Common User To Task Group And Capability";

			var commonUser2 = Factory.NewWithValidTestData<GlbStaff>();
			commonUser2.GS_Code = "CO2";
			commonUser2.GS_FullName = "Common User To Release Group And Capability";

			var commonUserInactive = Factory.NewWithValidTestData<GlbStaff>();
			commonUserInactive.GS_Code = "COI";
			commonUserInactive.GS_FullName = "Common Inactive User";
			commonUserInactive.GS_IsActive = false;

			var capUser1 = Factory.NewWithValidTestData<GlbStaff>();
			capUser1.GS_Code = "CA1";
			capUser1.GS_FullName = "Capability User The First";

			var capUser2 = Factory.NewWithValidTestData<GlbStaff>();
			capUser2.GS_Code = "CA2";
			capUser2.GS_FullName = "Capability User The Second";

			var pivot1 = capUser1.CapabilityPivots.AddNew();
			pivot1.G5_G4_Capability = directlyAssignedCapability.PK;
			var pivot2 = capUser2.CapabilityPivots.AddNew();
			pivot2.G5_G4_Capability = directlyAssignedCapability.PK;
			var pivot3 = commonUser1.CapabilityPivots.AddNew();
			pivot3.G5_G4_Capability = directlyAssignedCapability.PK;
			var pivot4 = commonUser2.CapabilityPivots.AddNew();
			pivot4.G5_G4_Capability = directlyAssignedCapability.PK;
			var pivot5 = commonUserInactive.CapabilityPivots.AddNew();
			pivot5.G5_G4_Capability = directlyAssignedCapability.PK;

			var taskUser1 = Factory.NewWithValidTestData<GlbStaff>();
			taskUser1.GS_Code = "TA1";
			taskUser1.GS_FullName = "Task Group User The First";

			var taskUser2 = Factory.NewWithValidTestData<GlbStaff>();
			taskUser2.GS_Code = "TA2";
			taskUser2.GS_FullName = "Task Group User The Second";

			var taskGroup = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink1 = Factory.New<GlbGroupLink>();
			groupLink1.GK_GG = taskGroup.PK;
			groupLink1.GK_GS = taskUser1.PK;
			var groupLink2 = Factory.New<GlbGroupLink>();
			groupLink2.GK_GG = taskGroup.PK;
			groupLink2.GK_GS = taskUser2.PK;
			var groupLink3 = Factory.New<GlbGroupLink>();
			groupLink3.GK_GG = taskGroup.PK;
			groupLink3.GK_GS = commonUser1.PK;
			var groupLinkInactive1 = Factory.New<GlbGroupLink>();
			groupLinkInactive1.GK_GG = taskGroup.PK;
			groupLinkInactive1.GK_GS = commonUserInactive.PK;

			var rgUser1 = Factory.NewWithValidTestData<GlbStaff>();
			rgUser1.GS_Code = "RG1";
			rgUser1.GS_FullName = "Release Group User The First";

			var rgUser2 = Factory.NewWithValidTestData<GlbStaff>();
			rgUser2.GS_Code = "RG2";
			rgUser2.GS_FullName = "Release Group User The Second";

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink4 = Factory.New<GlbGroupLink>();
			groupLink4.GK_GG = releaseGroup.PK;
			groupLink4.GK_GS = rgUser1.PK;
			var groupLink5 = Factory.New<GlbGroupLink>();
			groupLink5.GK_GG = releaseGroup.PK;
			groupLink5.GK_GS = rgUser2.PK;
			var groupLink6 = Factory.New<GlbGroupLink>();
			groupLink6.GK_GG = releaseGroup.PK;
			groupLink6.GK_GS = commonUser2.PK;
			var groupLinkInactive2 = Factory.New<GlbGroupLink>();
			groupLinkInactive2.GK_GG = releaseGroup.PK;
			groupLinkInactive2.GK_GS = commonUserInactive.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var dummy = Factory.New<DummyWithWorkflow>();
				var jobLevelWorkflow = helper.GetJobHeaderForParent(dummy, Factory);
				var workflow1 = jobLevelWorkflow.ProcessHeaders[0];
				var workflow2 = helper.CreateWorkflow(jobLevelWorkflow, "More work");

				var taskToCreateAssistTaskFor = (ProcessTask)helper.CreateTask(workflow1, assignedUser.GS_Code, 60, "INV", sequence: 1);
				taskToCreateAssistTaskFor.P9_G4_RequiredCapability = directlyAssignedCapability.PK;
				taskToCreateAssistTaskFor.P9_SystemCreateUser = creatingUser.GS_Code;

				var cw1UserTask = (ProcessTask)helper.CreateTask(workflow1, "E", 60, "INV", sequence: 3);
				var serviceUserTask = (ProcessTask)helper.CreateTask(workflow1, "~BP", 60, "INV", sequence: 4);
				var staffUserTask1 = (ProcessTask)helper.CreateTask(workflow1, staffUser1.GS_Code, 60, "INV", sequence: 5);
				var staffUserTask2 = (ProcessTask)helper.CreateTask(workflow2, staffUser2.GS_Code, 60, "INV", sequence: 1);
				var staffUserTask3 = (ProcessTask)helper.CreateTask(workflow2, staffUser3.GS_Code, 60, "INV", sequence: 2);

				Factory.Save();

				AssertContainsExactElementsInExactOrder("All users assigned to capability should be in the list when the task has no group allocations",
					new string[]
					{
						"~BP - CargoWise Service",
						"CA1 - Capability User The First",
						"CA2 - Capability User The Second",
						"CO1 - Common User To Task Group And Capability",
						"CO2 - Common User To Release Group And Capability",
						"CRE - The Creator",
						"CUR - Current User",
						"E - CargoWise Support",
						"ST1 - Staff User The First",
						"ST2 - Staff User The Second",
						"ST3 - Staff User The Third",
					},
					AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, dummy.WorkflowItems).Select(s => s.MenuItemDescription.ToString()));

				taskToCreateAssistTaskFor.P9_GG_AssignedGroup = taskGroup.PK;

				AssertContainsExactElementsInExactOrder("The user that is common to the task group and capability should appear in the list when the task is assigned to a task group",
					new string[]
					{
						"~BP - CargoWise Service",
						"CO1 - Common User To Task Group And Capability",
						"CRE - The Creator",
						"CUR - Current User",
						"E - CargoWise Support",
						"ST1 - Staff User The First",
						"ST2 - Staff User The Second",
						"ST3 - Staff User The Third",
					},
					AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, dummy.WorkflowItems).Select(s => s.MenuItemDescription.ToString()));

				taskToCreateAssistTaskFor.P9_GG_AssignedGroup = ZGuid.Empty;
				workflow1.FH_GG_ReleaseGroup = releaseGroup.PK;

				AssertContainsExactElementsInExactOrder("The user that is common to the release group and capability should appear in the list when the task is assigned to a release group",
					new string[]
					{
						"~BP - CargoWise Service",
						"CO2 - Common User To Release Group And Capability",
						"CRE - The Creator",
						"CUR - Current User",
						"E - CargoWise Support",
						"ST1 - Staff User The First",
						"ST2 - Staff User The Second",
						"ST3 - Staff User The Third",
					},
					AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, dummy.WorkflowItems).Select(s => s.MenuItemDescription.ToString()));
			}
		}

		public void TestGetCapabilityCodesAndDescriptionsForAssistanceTask_ShouldNotThrowNullReferenceException()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "DUM");

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobLevelWorkflow = helper.GetJobHeaderForParent(dummy, Factory);
			var workflow = jobLevelWorkflow.ProcessHeaders[0];
			var assignedUser = Factory.NewWithValidTestData<GlbStaff>();
			var taskToCreateAssistTaskFor = (ProcessTask)helper.CreateTask(workflow, assignedUser.GS_Code, 60, "INV", sequence: 1);

			AssertNoExceptionThrown(() => AssistWithThisTaskHelper.GetCapabilityCodesAndDescriptionsForAssistanceTask(null, null));
			AssertNoExceptionThrown(() => AssistWithThisTaskHelper.GetCapabilityCodesAndDescriptionsForAssistanceTask(null, dummy.WorkflowItems));
			AssertNoExceptionThrown(() => AssistWithThisTaskHelper.GetCapabilityCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, null));

			var taskWithInvalidCapability = (ProcessTask)helper.CreateTask(workflow, sequence: 2);
			taskWithInvalidCapability.P9_G4_RequiredCapability = ZGuid.NewZGuid();

			var processTaskCollection = new ProcessTaskCollection(Factory);
			processTaskCollection.Add(taskWithInvalidCapability);

			AssertNoExceptionThrown(() => AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(taskToCreateAssistTaskFor, processTaskCollection));
		}

		public void TestGetCapabilityCodesAndDescriptionsForAssistanceTask()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "DUM");

			var taskCapability1 = Factory.NewWithValidTestData<GlbCapability>();
			taskCapability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			taskCapability1.G4_Code = "CAP";
			taskCapability1.G4_Description = "Capable";

			var taskCapability2 = Factory.NewWithValidTestData<GlbCapability>();
			taskCapability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			taskCapability2.G4_Code = "TAL";
			taskCapability2.G4_Description = "Talented";

			var taskCapability3 = Factory.NewWithValidTestData<GlbCapability>();
			taskCapability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			taskCapability3.G4_Code = "COM";
			taskCapability3.G4_Description = "Competent";

			var otherLinkedCapability1 = Factory.NewWithValidTestData<GlbCapability>();
			otherLinkedCapability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			otherLinkedCapability1.G4_Code = "SKI";
			otherLinkedCapability1.G4_Description = "Skilled";

			var otherLinkedCapability2 = Factory.NewWithValidTestData<GlbCapability>();
			otherLinkedCapability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			otherLinkedCapability2.G4_Code = "MAS";
			otherLinkedCapability2.G4_Description = "Masterful";

			var unlinkedCapability = Factory.NewWithValidTestData<GlbCapability>();
			unlinkedCapability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			unlinkedCapability.G4_Code = "DUM";
			unlinkedCapability.G4_Description = "Dumb";

			var staffUser1 = Factory.NewWithValidTestData<GlbStaff>();
			staffUser1.GS_Code = "ST1";
			staffUser1.GS_FullName = "Staff User The First";

			var staffUser2 = Factory.NewWithValidTestData<GlbStaff>();
			staffUser2.GS_Code = "ST2";
			staffUser2.GS_FullName = "Staff User The Second";

			var staffUser3 = Factory.NewWithValidTestData<GlbStaff>();
			staffUser3.GS_Code = "ST3";
			staffUser3.GS_FullName = "Staff User The Third";

			var pivot1 = staffUser1.CapabilityPivots.AddNew();
			pivot1.G5_G4_Capability = otherLinkedCapability1.PK;

			var pivot2 = staffUser2.CapabilityPivots.AddNew();
			pivot2.G5_G4_Capability = otherLinkedCapability2.PK;

			var pivot3 = staffUser3.CapabilityPivots.AddNew();
			pivot3.G5_G4_Capability = taskCapability3.PK;

			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobLevelWorkflow = helper.GetJobHeaderForParent(dummy, Factory);
			var workflow = jobLevelWorkflow.ProcessHeaders[0];

			var task1 = (ProcessTask)helper.CreateTask(workflow, "ST1", 60, "INV", sequence: 1, capability: taskCapability1);
			var task2 = (ProcessTask)helper.CreateTask(workflow, "ST2", 60, "INV", sequence: 2, capability: taskCapability2);
			var task3 = (ProcessTask)helper.CreateTask(workflow, "ST3", 60, "INV", sequence: 3, capability: taskCapability3);

			var expectedList = new string[]
			{
				"CAP - Capable",
				"COM - Competent",
				"TAL - Talented",
				"MAS - Masterful",
				"SKI - Skilled",
			};

			AssertContainsExactElementsInExactOrder(
				"Only capabilities on the job and other capabilties that users on the job possess should be listed here, sorted in code order, grouped first by job capabilities then other capabilities",
				expectedList,
				AssistWithThisTaskHelper.GetCapabilityCodesAndDescriptionsForAssistanceTask(task1, dummy.WorkflowItems).Select(s => s.MenuItemDescription.ToString()));

			AssertContainsExactElementsInExactOrder("The capability list should be the same regardless of the single task passed in",
				expectedList,
				AssistWithThisTaskHelper.GetCapabilityCodesAndDescriptionsForAssistanceTask(task2, dummy.WorkflowItems).Select(s => s.MenuItemDescription.ToString()));
		}

		protected override void SetUp()
		{
			base.SetUp();

			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.AddTaskTypesToRegistry("INQ", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 15, 3);
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("INQ", "INV", 20, 4);
		}
	}
}
