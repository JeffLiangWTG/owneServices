using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class TaskAssignmentHelperTest : TestCaseWithFactory
	{
		public void TestGetRelatedTasks_ShouldProperlyCheckGetResourceDelegate()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();

			var primaryTask = workflow.Parent.WorkflowItems.Tasks.AddNew();
			primaryTask.P9_FH_ProcessHeader = workflow.PK;
			primaryTask.P9_Type = "INV";
			primaryTask.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			primaryTask.P9_GC = newCompany.PK;
			primaryTask.P9_ShareTasksForAllCompanies = false;

			var taskA = workflow.Parent.WorkflowItems.Tasks.AddNew();
			taskA.P9_FH_ProcessHeader = workflow.PK;
			taskA.P9_Type = "INV";
			taskA.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			taskA.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var taskB = workflow.Parent.WorkflowItems.Tasks.AddNew();
			taskB.P9_FH_ProcessHeader = workflow.PK;
			taskB.P9_Type = "INV";
			taskB.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			taskB.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var taskC = workflow.Parent.WorkflowItems.Tasks.AddNew();
			taskC.P9_FH_ProcessHeader = workflow.PK;
			taskC.P9_Type = "INV";
			taskC.P9_GS_NKAssignedStaffMember = string.Empty;
			taskC.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var tasks = TaskAssignmentHelper.GetValidProcessTaskListAccordingToScope(primaryTask, ScopeList.Codes.Job, usedInTaskValidation: true);
			AssertEquals("We should get no tasks because our ternary operator is not the dominant check in the GetRelatedTasks method anymore", 0, tasks.Count());
		}

		public void TestGetRelatedTasks_ShouldReportErrorAndReturnEmptyEnumerable_WhenTaskIsNull()
		{
			var returnedTasks = TaskAssignmentHelper.GetValidProcessTaskListAccordingToScope(null, ScopeList.Codes.Job, usedInTaskValidation: true);

			CombineAssertions(() =>
			{
				AssertEquals("We should get no tasks returned because task was null", 0, returnedTasks.Count());
				AssertEquals("Error should have been reported because task was null", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("Error reporter should have displayed details about what was null", "Error: GetRelatedTasks was called when task was null", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			});
		}

		public void TestGetRelatedTasks_ShouldNotReportErrorAndReturnEmptyEnumerable_WhenTaskHasNoParentPK()
		{
			var processTask = Factory.New<ProcessTask>();

			var returnedTasks = TaskAssignmentHelper.GetValidProcessTaskListAccordingToScope(processTask, ScopeList.Codes.Job, usedInTaskValidation: true);

			CombineAssertions(() =>
			{
				AssertEquals("We should get no tasks returned because task.ParentID was null", 0, returnedTasks.Count());
				AssertEquals("Error should not have been reported because tasks can exist without a ParentID in some situations.", 0, ErrorReporter.TotalErrorCount);

				ErrorReporter.Clear();
			});
		}

		public void TestGetRelatedTasks_ShouldReportErrorAndReturnEmptyEnumerable_WhenTaskHasNoParentType()
		{
			var processTask = Factory.New<ProcessTask>();
			var parentGUID = ZGuid.NewZGuid();
			processTask.P9_ParentID = parentGUID;
			processTask.P9_FH_ProcessHeader = Factory.New<ProcessHeader>().PK;

			var returnedTasks = TaskAssignmentHelper.GetValidProcessTaskListAccordingToScope(processTask, ScopeList.Codes.Job, usedInTaskValidation: true);

			CombineAssertions(() =>
			{
				AssertEquals("We should get no tasks returned because task.ParentTaskCollection.Tasks was null", 0, returnedTasks.Count());
				AssertEquals("Error should have been reported because task has no parent type", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("Error reporter should have displayed details about what was null",
$@"Error: GetRelatedTasks was called when task.ParentTaskCollection?.Tasks was null.
task:												Enterprise.MasterFiles.Business.ProcessTask
task.ParentTaskCollection:							null

task.IsDeleted:										False
task.P9_ParentID:									{parentGUID}
task.P9_ParentID.IsValid:							True
task.P9_ParentTableCode:							
task.ParentType:									CargoWise.EntityFramework.BusinessObject
task.GetWorkflowDescriptor().WorkflowProviderType:	null
(task as ILineTriggerSupport):						Enterprise.MasterFiles.Business.ProcessTask
(task as ILineTriggerSupport).LineTriggerType:		"
				, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			});
		}

		public void TestGetRelatedTasks_ShouldReportErrorAndReturnEmptyEnumerable_WhenTaskHaslineTriggerSupport_ButNoWorkflowDescriptor()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var primaryTask = dummy.WorkflowItems.Tasks.AddNew();
			dummy.WorkflowItems.Triggers.AddNew();

			(primaryTask as Enterprise.Integration.ILineTriggerSupport).LineTriggerType = new ZString('D');

			var returnedTasks = TaskAssignmentHelper.GetValidProcessTaskListAccordingToScope(primaryTask, ScopeList.Codes.Job, usedInTaskValidation: true);

			AssertEquals("line trigger actually had nothing to do with whether siblings are related.", 0, returnedTasks.Count());
		}

		public void TestStaffAssignment_WithSAMTaskRestruction_ShouldNotAssign_WhenStaffCodeIsInvalid()
		{
			SetupTaskTypesInRegistry();

			var collection = new TaskTypeRestrictionsCollection();

			var restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "ORG";
			restriction.TaskType = "INV";
			restriction.NotificationType = NotificationTypeList.Codes.None;
			restriction.Scope = ScopeList.Codes.Job;
			restriction.RestrictionType = RestrictionTypeList.Codes.SameResource;

			var taskType1 = restriction.TaskTypesCollection.AddNew();
			taskType1.Code = "CDU";

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();

			var task1 = MasterFilesTestHelper.CreateTask(workflow.Parent, status: ProcessTaskStatusCodeList.Codes.Open, taskType: "INV");
			task1.P9_GS_NKAssignedStaffMember = string.Empty;
			var task2 = MasterFilesTestHelper.CreateTask(workflow.Parent, status: ProcessTaskStatusCodeList.Codes.Open, taskType: "CDU");
			task2.P9_GS_NKAssignedStaffMember = string.Empty;

			task1.P9_FH_ProcessHeader = task2.P9_FH_ProcessHeader = workflow.PK;

			var resource = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			AssertEquals("Precodition", string.Empty, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Precodition", string.Empty, task2.P9_GS_NKAssignedStaffMember);

			task1.P9_GS_NKAssignedStaffMember = "A%%";
			AssertEquals("Should not populate an invalid code", string.Empty, task2.P9_GS_NKAssignedStaffMember);

			task1.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			AssertEquals("Should populate", resource.GS_Code, task2.P9_GS_NKAssignedStaffMember);
		}

		public void TestStaffAssignment_WithSAMTaskRestruction_ShouldRunValidationForAllAssignedTasks()
		{
			SetupTaskTypesInRegistry();

			var collection = new TaskTypeRestrictionsCollection();

			var restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "ORG";
			restriction.TaskType = "INV";
			restriction.NotificationType = NotificationTypeList.Codes.None;
			restriction.Scope = ScopeList.Codes.Job;
			restriction.RestrictionType = RestrictionTypeList.Codes.SameResource;

			var taskType1 = restriction.TaskTypesCollection.AddNew();
			taskType1.Code = "CDU";

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();

			var task1 = MasterFilesTestHelper.CreateTask(workflow.Parent, status: ProcessTaskStatusCodeList.Codes.Open, taskType: "INV");
			task1.P9_GS_NKAssignedStaffMember = string.Empty;
			var task2 = MasterFilesTestHelper.CreateTask(workflow.Parent, status: ProcessTaskStatusCodeList.Codes.Open, taskType: "CDU");
			task2.P9_GS_NKAssignedStaffMember = string.Empty;

			task1.P9_FH_ProcessHeader = task2.P9_FH_ProcessHeader = workflow.PK;

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_IsActive = false;

			Factory.Save();

			task1.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			AssertEquals("Should populate", resource.GS_Code, task2.P9_GS_NKAssignedStaffMember);

			AssertHasError(task1.P9_GS_NKAssignedStaffMemberInfo, "This Task Assigned To is inactive - it may not be used.");
			AssertHasError("Should validate the second assinged task as well", task2.P9_GS_NKAssignedStaffMemberInfo, "This Task Assigned To is inactive - it may not be used.");
		}

		public void TestStaffAssignmentOnTaskWithoutParentID_ShouldNotReportError_WithSAMTaskRestriction()
		{
			MasterFilesTestHelper.AddTaskTypesToRegistry("WKI", "INV", "CDU");
			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new TaskTypeRestrictionsCollection());
			var collection = WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.Value;

			var restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "WKI";
			restriction.TaskType = "INV";
			restriction.NotificationType = NotificationTypeList.Codes.None;
			restriction.Scope = ScopeList.Codes.Job;
			restriction.RestrictionType = RestrictionTypeList.Codes.SameResource;

			var cduTaskType = restriction.TaskTypesCollection.AddNew();
			cduTaskType.Code = "CDU";

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var processTask = Factory.New<ProcessTask>();
			processTask.P9_Type = "INV";

			processTask.P9_GS_NKAssignedStaffMember = "DE";

			AssertEquals("Error should not have been reported as related tasks should not be populated if there is no ParentID", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestAssignTasks_ShouldNotAssignTaskToResourceWithoutCapability_WhenRegistrySaysSo()
		{
			WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var capability1 = MasterFilesTestHelper.CreateCapability(Factory, resource);
			var capability2 = MasterFilesTestHelper.CreateCapability(Factory);

			Factory.Save();

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();

			var primaryTask = MasterFilesTestHelper.CreateTask(workflow.Parent, status: ProcessTaskStatusCodeList.Codes.Open, taskType: "INV", requiredCapability: capability1);
			primaryTask.P9_GS_NKAssignedStaffMember = string.Empty;
			var taskA = MasterFilesTestHelper.CreateTask(workflow.Parent, status: ProcessTaskStatusCodeList.Codes.Open, taskType: "CDU", requiredCapability: capability1);
			taskA.P9_GS_NKAssignedStaffMember = string.Empty;
			var taskB = MasterFilesTestHelper.CreateTask(workflow.Parent, status: ProcessTaskStatusCodeList.Codes.Open, taskType: "CDU", requiredCapability: capability2);
			taskB.P9_GS_NKAssignedStaffMember = string.Empty;
			var taskC = MasterFilesTestHelper.CreateTask(workflow.Parent, status: ProcessTaskStatusCodeList.Codes.Open, taskType: "CDU", requiredCapability: capability2);
			taskC.P9_GS_NKAssignedStaffMember = string.Empty;

			primaryTask.P9_FH_ProcessHeader = taskA.P9_FH_ProcessHeader = taskB.P9_FH_ProcessHeader = taskC.P9_FH_ProcessHeader = workflow.PK;

			SetupTaskRestrictionsRegistry();

			Factory.Save();

			primaryTask.P9_GS_NKAssignedStaffMember = resource.GS_Code;

			AssertEquals("We succesfully assigned the capability task to our resource with SAM restrictions", resource.GS_Code, taskA.P9_GS_NKAssignedStaffMember);
			AssertEquals("We succesfully assigned the capability task to our resource with SAM restrictions", resource.GS_Code, taskB.P9_GS_NKAssignedStaffMember);
			AssertEquals("We succesfully assigned the capability task to our resource with SAM restrictions", resource.GS_Code, taskC.P9_GS_NKAssignedStaffMember);

			primaryTask.P9_GS_NKAssignedStaffMember = string.Empty;
			taskA.P9_GS_NKAssignedStaffMember = string.Empty;
			taskB.P9_GS_NKAssignedStaffMember = string.Empty;
			taskC.P9_GS_NKAssignedStaffMember = string.Empty;

			WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();

			primaryTask.P9_GS_NKAssignedStaffMember = resource.GS_Code;

			AssertEquals("We succesfully assigned the capability task to our resource with SAM restrictions", resource.GS_Code, taskA.P9_GS_NKAssignedStaffMember);
			AssertEquals("We succesfully DIDN'T assign the capability task to our resource with SAM restrictions", string.Empty, taskB.P9_GS_NKAssignedStaffMember);
			AssertEquals("We succesfully DIDN'T assign the capability task to our resource with SAM restrictions", string.Empty, taskC.P9_GS_NKAssignedStaffMember);

			primaryTask.P9_GS_NKAssignedStaffMember = "RAN";
			AssertEquals("Assign random staff code didn't crash the program and shouldn't change old assignment", resource.GS_Code, taskA.P9_GS_NKAssignedStaffMember);
		}

		void SetupTaskRestrictionsRegistry()
		{
			SetupTaskTypesInRegistry();

			var collection = new TaskTypeRestrictionsCollection();

			var restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "ORG";
			restriction.TaskType = "INV";
			restriction.NotificationType = NotificationTypeList.Codes.Error;
			restriction.Scope = ScopeList.Codes.Job;
			restriction.RestrictionType = RestrictionTypeList.Codes.SameResource;

			var taskType1 = restriction.TaskTypesCollection.AddNew();
			var taskType2 = restriction.TaskTypesCollection.AddNew();

			taskType1.Code = "CDU";
			taskType2.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		void SetupTaskTypesInRegistry()
		{
			var categorisedWorkflowTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();

			var categorisedWorkflowTaskType = categorisedWorkflowTaskTypeCollection.AddNew();
			categorisedWorkflowTaskType.Code = "ORG";

			var workflowTaskType1 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType2 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType3 = categorisedWorkflowTaskType.TaskTypes.AddNew();

			workflowTaskType1.Code = "INV";
			workflowTaskType2.Code = "CDU";
			workflowTaskType3.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedWorkflowTaskTypeCollection);
		}
	}
}
