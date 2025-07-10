using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Workflow.ProcessTasks.Milestones;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ProcessTaskValidationTest : BusinessObjectValidationTestCase
	{
		#region P9_FH_ProcessHeader

		public void TestProcessHeader_Job()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var system = (BMSystem)helper.CreateSystem(Factory, "DUM");
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(dummy, Factory);
			jobHeader.ProcessHeaders.DeleteAll();
			var header1 = jobHeader.ProcessHeaders.AddNew();
			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_FH_ProcessHeader = header1.PK;
			AssertNoErrors(task.P9_FH_ProcessHeaderInfo);
		}

		public void TestProcessHeader_StandAlone()
		{
			var task = Factory.New<ProcessTask>();
			AssertNoErrors(task.P9_FH_ProcessHeaderInfo);

			task.P9_FH_ProcessHeader = ZGuid.NewZGuid();
			AssertHasErrors(task.P9_FH_ProcessHeaderInfo);

			task.P9_FH_ProcessHeader = ZGuid.Empty;
			AssertNoErrors(task.P9_FH_ProcessHeaderInfo);
		}

		public void TestProcessHeader_Inactive()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "WKI";

			var workflow = template.ProcessHeaders.AddNew();
			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_FH_ProcessHeader = workflow.PK;
			AssertNoErrors(task.P9_FH_ProcessHeaderInfo);

			workflow.FH_IsActive = false;
			task.Validation.ValidateAll();
			AssertNoErrors(task.P9_FH_ProcessHeaderInfo);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task.Validation.ValidateAll();
			AssertNoErrors(task.P9_FH_ProcessHeaderInfo);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task.Validation.ValidateAll();
			AssertNoErrors(task.P9_FH_ProcessHeaderInfo);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task.Validation.ValidateAll();
			AssertNoErrors(task.P9_FH_ProcessHeaderInfo);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task.Validation.ValidateAll();
			AssertNoErrors(task.P9_FH_ProcessHeaderInfo);
		}

		#endregion

		#region P9_Type

		public void TestP9_Type()
		{
			ProcessTask.P9_Type = "ZUB";
			AssertHasErrors(ProcessTask.P9_TypeInfo);

			ProcessTask.P9_Type = ProcessTask.Lookups.Types[0].Code;
			AssertNoErrors(ProcessTask.P9_TypeInfo);

			ProcessTask.P9_Type = "";
			AssertHasErrors(ProcessTask.P9_TypeInfo);
		}

		public void TestP9_Type_WhenTaskTypeDeactivated_ShouldDisplayError()
		{
			MasterFilesTestHelper.AddTaskTypesToRegistry("DUM", "DEA", "ADK");

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task1 = MasterFilesTestHelper.CreateTask(job, taskType: "DEA");

			task1.Validation.ValidateAll();
			AssertNoErrors(task1.P9_TypeInfo);
			AssertNoWarnings(task1.P9_TypeInfo);

			Factory.Save();

			MasterFilesTestHelper.SetTaskTypeActiveStatus("DUM", "DEA", false);
			task1.Lookups.RefreshTypeList();

			task1.Validation.ValidateAll();
			AssertHasWarning(task1.P9_TypeInfo, "This Task Type is inactive and should not be used.");

			var task2 = MasterFilesTestHelper.CreateTask(job, taskType: "DEA");
			AssertHasError(task2.P9_TypeInfo, "This Task Type is inactive and must not be used.");

			task2.P9_Type = "ADK";

			AssertNoErrors(task2.P9_TypeInfo);
			AssertNoWarnings(task2.P9_TypeInfo);

			task2.P9_Type = "ZZZ";

			AssertListValidationInvalidCodeError(task2.P9_TypeInfo, isExpectingError: true);
			AssertNoError(task2.P9_TypeInfo, "This Task Type is inactive and must not be used.");
			AssertNoWarning(task2.P9_TypeInfo, "This Task Type is inactive and must not be used.");
		}

		#endregion

		#region P9_Status / P9_ActualDuration

		[TestDate(2021, 1, 13)]
		public void TestClosedTaskWithRequireActualDuration_ShouldShowError()
		{
			ProcessTask.Lookups.Types[0].IsRequireActualDuration = true;
			ProcessTask.P9_Type = ProcessTask.Lookups.Types[0].Code;
			AssertNoErrors(ProcessTask.P9_ActualDurationInfo);
			AssertNoErrors(ProcessTask.P9_StatusInfo);

			Factory.Save();

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask.P9_ActualDuration = ZDateTime.Empty;
			ProcessTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			AssertHasError(ProcessTask.P9_ActualDurationInfo, "For closed tasks of this type, you must enter a non-zero actual duration.");
			AssertHasError(ProcessTask.P9_StatusInfo, "For closed tasks of this type, you must enter a non-zero actual duration.");

			ProcessTask.P9_ActualDuration = TestDateAttribute.Date.AddHours(1);

			AssertNoErrors(ProcessTask.P9_ActualDurationInfo);
			AssertNoErrors(ProcessTask.P9_StatusInfo);
		}

		[TestDate(2021, 1, 13)]
		public void TestUnsavedClosedTaskWithRequireActualDuration_ShouldShowError()
		{
			ProcessTask.Lookups.Types[0].IsRequireActualDuration = true;
			ProcessTask.P9_Type = ProcessTask.Lookups.Types[0].Code;
			AssertNoErrors(ProcessTask.P9_ActualDurationInfo);
			AssertNoErrors(ProcessTask.P9_StatusInfo);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask.P9_ActualDuration = ZDateTime.Empty;
			ProcessTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			AssertHasError(ProcessTask.P9_ActualDurationInfo, "For closed tasks of this type, you must enter a non-zero actual duration.");
			AssertHasError(ProcessTask.P9_StatusInfo, "For closed tasks of this type, you must enter a non-zero actual duration.");

			ProcessTask.P9_ActualDuration = TestDateAttribute.Date.AddHours(1);

			AssertNoErrors(ProcessTask.P9_ActualDurationInfo);
			AssertNoErrors(ProcessTask.P9_StatusInfo);
		}

		public void TestClosedTaskWithRequireActualDurationByFLD_ShouldBeBlocked()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var shipment = (IWorkflowProvider)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>());

				var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
				var taskTypes = categorisedTaskTypes.AddNew();
				taskTypes.Code = shipment.WorkflowType;
				var taskType = taskTypes.TaskTypes.AddNew();
				taskType.Code = "UDF";
				taskType.IsRequireActualDuration = true;

				WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

				var task1 = shipment.WorkflowItems.Tasks.AddNew();
				task1.P9_Type = taskType.Code;
				task1.P9_Description = "task1";
				AssertNoErrors(task1.P9_TypeInfo);
				AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);

				var trigger = shipment.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "trigger";
				trigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
				action.PQ_FieldName = "<WorkflowItems.Where(\"<P9_Description>\" == \"task1\").P9_Status>";
				action.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				var triggeringEvent = shipment.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Factory.Save();

				MasterFilesTestHelper.RunLogWalker();
				task1.Reload();

				AssertEquals("Status should not be changed", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
			}
		}

		public void TestTemplateTaskValidation_ShouldNotAllowWorking()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "WKI";

			var workflow = template.ProcessHeaders.AddNew();
			var task = template.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertHasError(task.P9_StatusInfo, "Template task status cannot be working.");

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertNoErrors(task.P9_StatusInfo);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertNoErrors(task.P9_StatusInfo);
		}

		#region TaskCancellationValidation

		public void TestPreventCancellationOfTasks_WhenUserDoesNotHavePermissionToCancelNonCancellableTasks()
		{
			var job = (IWorkflowProvider)Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = string.Empty;
			task.P9_Type = "UDF";

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = job.WorkflowType;
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.CanCancelTask = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertHasError(task.P9_StatusInfo, "You do not have permission to cancel this task.");
			AssertEquals("The task assignee should remain unchanged", string.Empty, task.P9_GS_NKAssignedStaffMember);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AAA";
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.Validation.ValidateP9_Status();
			AssertHasError(task.P9_StatusInfo, "You do not have permission to cancel this task.");
			AssertEquals("The task assignee should remain unchanged", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
			MasterFilesTestHelper.AssertRtfText("P9_Notes should be empty", ZString.Empty, task.P9_Notes);
		}

		[TestDate(2021, 1, 1, 12, 10, 10, 10)]
		public void TestAllowCancellationOfTasks_WhenUserHasPermissionToCancelNonCancellableTasks()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AAA";

			var job = (IWorkflowProvider)Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Type = "UDF";

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = job.WorkflowType;
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.CanCancelTask = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = true;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertNoErrors("The task should be allowed to be cancelled since a privileged user is cancelling an otherwise non-cancellable task", task.P9_StatusInfo);
			MasterFilesTestHelper.AssertRtfText("P9_Notes should return", "E 01-Jan-21 12:10: Task canceled by user with permission to cancel tasks that have been configured to not allow cancellation.", task.P9_Notes);
		}

		[TestDate(2021, 1, 1, 12, 10, 10, 10)]
		public void TestAllowCancellationOfTasks_WhenUserHasPermissionToCancelNonCancellableTasks_WithExistingNote()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AAA";

			var job = (IWorkflowProvider)Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Type = "UDF";

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = job.WorkflowType;
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.CanCancelTask = false;

			task.P9_NotesAsString = "Hello";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = true;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertNoErrors("The task should be allowed to be cancelled since a privileged user is cancelling an otherwise non-cancellable task", task.P9_StatusInfo);
			AssertEquals("The task assignee should remain unchanged", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
			MasterFilesTestHelper.AssertRtfText("P9_Notes should return appended cancellation note and existing note", @"Hello

E 01-Jan-21 12:10: Task canceled by user with permission to cancel tasks that have been configured to not allow cancellation.", task.P9_Notes);
		}

		public void TestAllowCancellationOfTasks_WhenTaskIsCancellable()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AAA";

			var job = (IWorkflowProvider)Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Type = "UDF";

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = job.WorkflowType;
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.CanCancelTask = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertNoErrors("The task should be allowed to be cancelled since it is a cancellable task, even by a non-privileged user", task.P9_StatusInfo);
			AssertEquals("The task assignee should remain unchanged", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
			MasterFilesTestHelper.AssertRtfText("P9_Notes should be empty", ZString.Empty, task.P9_Notes);
		}

		public void TestAllowCancellationOfTasks_WhenTaskStatusHasntChanged()
		{
			var job = (IWorkflowProvider)Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = string.Empty;
			task.P9_Type = "UDF";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = job.WorkflowType;
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.CanCancelTask = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = false;

			Factory.Save();

			task.P9_EstDuration = TaskDurationCalculator.GetDurationFromTimeSpan(TimeSpan.FromHours(100));
			task.Validation.ValidateAll();

			AssertNoErrors("The task should be allowed to be changed since it is not changing status, even by a non-privileged user", task.P9_StatusInfo);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task.Validation.ValidateAll();
			AssertHasError(task.P9_StatusInfo, "You do not have permission to cancel this task.");
		}

		public void TestAllowCancellationOfTasks_WhenWorkflowOrJobWasCanceled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.CreateSystem(Factory, "DUM");
			var job = (IWorkflowProvider)Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			var workflow = task.ProcessHeader as ProcessHeader;
			task.P9_GS_NKAssignedStaffMember = string.Empty;
			task.P9_Type = "UDF";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = job.WorkflowType;
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.CanCancelTask = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = false;

			Factory.Save();

			workflow.CancelAllTasksAndCompletionStatements("Cancel Workflow");

			task.Validation.ValidateAll();

			AssertNoErrors("The task should be allowed to be changed since it is changing status from the cancellation of Workflow, even by a non-privileged user", task.P9_StatusInfo);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			job.CancelNonStartedTasksAndClosePartiallyCompletedTasks();

			task.Validation.ValidateAll();
			AssertNoErrors("The task should be allowed to be changed since it is changing status from the cancellation of Job, even by a non-privileged user", task.P9_StatusInfo);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task.Validation.ValidateAll();
			AssertHasError(task.P9_StatusInfo, "You do not have permission to cancel this task.");
		}

		#endregion

		#region WorkingOutOfBufferConditions

		public void TestWorkingOffBuffer_WorkingStatusChangeALW_AllowChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBucket(system).PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			SetWorkingStatusChange((workflow as ProcessHeader).FH_WorkflowType, "ALW");
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertNoErrors(task.P9_StatusInfo);
			AssertNoWarnings(task.P9_StatusInfo);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void TestWorkingOffBuffer_BMNotEnabled_AllowChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBucket(system).PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertNoErrors(task.P9_StatusInfo);
			AssertNoWarnings(task.P9_StatusInfo);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void TestWorkingOffBuffer_SecurityCheckpointAllowed_AllowChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBucket(system).PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: "ASN");
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			SetWorkingStatusChange((workflow as ProcessHeader).FH_WorkflowType, "WRN");
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = true;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertNoErrors(task.P9_StatusInfo);
			AssertNoWarnings(task.P9_StatusInfo);
		}

		public void TestWorkingOffBuffer_NoParentProcessHeader_AllowChanges()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertNoErrors(task.P9_StatusInfo);
			AssertNoWarnings(task.P9_StatusInfo);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void TestWorkingOffBuffer_NoWorkflowTaskType_AllowChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBuffer(system).PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskType: null);
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertNoErrors(task.P9_StatusInfo);
			AssertNoWarnings(task.P9_StatusInfo);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void TestWorkingOffBuffer_CurrentComponentNull_AllowChanges()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = ZGuid.Empty;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertNoErrors(task.P9_StatusInfo);
			AssertNoWarnings(task.P9_StatusInfo);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void TestWorkingOffBuffer_ProcessHeaderStandby_AllowChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBucket(system).PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;
			workflow.FH_IsStandby = true;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertNoErrors(task.P9_StatusInfo);
			AssertNoWarnings(task.P9_StatusInfo);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void TestWorkingOffBuffer_ProcessJobHeaderStandby_AllowChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBucket(system).PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;
			jobHeader.FH_IsStandby = true;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertNoErrors(task.P9_StatusInfo);
			AssertNoWarnings(task.P9_StatusInfo);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void TestWorkingOffBuffer_CurrentComponentBufferType_AllowChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBuffer(system).PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertNoErrors(task.P9_StatusInfo);
			AssertNoWarnings(task.P9_StatusInfo);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		public void TestWorkingOffBuffer_WorkingStatusChangeERR_CurrentStatusSUS_ErrorForChangeToWRK()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBucket(system).PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: "SUS");
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			SetWorkingStatusChange((workflow as ProcessHeader).FH_WorkflowType, "ERR");
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertHasError(task.P9_StatusInfo, "This task’s status cannot be set to 'WRK' because its workflow is not in a buffer component.");
		}

		public void TestWorkingOffBuffer_WorkingStatusChangeERR_CurrentStatusWRK_AllowChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBucket(system).PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: "WRK");
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			SetWorkingStatusChange((workflow as ProcessHeader).FH_WorkflowType, "ERR");
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertNoErrors(task.P9_StatusInfo);
			AssertNoWarnings(task.P9_StatusInfo);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}

		public void TestWorkingOffBuffer_WorkingStatusChangeWRN_CurrentStatusASN_WarnChangesForChangeToWRK()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBucket(system).PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: "ASN");
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			SetWorkingStatusChange((workflow as ProcessHeader).FH_WorkflowType, "WRN");
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertHasWarning(task.P9_StatusInfo, "This task’s status should not be set to 'WRK' because its workflow is not in a buffer component.");
		}

		public void TestWorkingOffBuffer_ChangeASNTaskDifferentStaff_ErrorForChangeToWRK()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBucket(system).PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: "ASN");
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			SetWorkingStatusChange((workflow as ProcessHeader).FH_WorkflowType, "ERR");
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";

			Factory.Save();

			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertHasError(task.P9_StatusInfo, "This task’s status cannot be set to 'WRK' because its workflow is not in a buffer component.");
		}

		public void TestWorkingOffBuffer_ChangeASNTaskParentComponentNull_AllowChanges()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = ZGuid.Empty;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: "ASN");
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertNoErrors(task.P9_StatusInfo);
			AssertNoWarnings(task.P9_StatusInfo);
		}

		public void TestWorkingOffBuffer_MultipleTaskType_SelectedProperlyIfNeeded()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var system = BMSTestHelper.CreateSystem(Factory, "ABC");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = BMSTestHelper.CreateBucket(system).PK;
			var task1 = BMSTestHelper.CreateTask(workflow, staff1.GS_Code, 60, taskType: "UDF");
			var task2 = BMSTestHelper.CreateTask(workflow, staff2.GS_Code, 60, taskType: "INV");
			var task3 = BMSTestHelper.CreateTask(workflow, staff3.GS_Code, 60, taskType: "CDF");
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			SetWorkingStatusChange((workflow as ProcessHeader).FH_WorkflowType, "WRN");
			SetWorkingStatusChange((workflow as ProcessHeader).FH_WorkflowType, "ALW", "INV");
			SetWorkingStatusChange((workflow as ProcessHeader).FH_WorkflowType, "ERR", "CDF");
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertHasWarning(task1.P9_StatusInfo, "This task’s status should not be set to 'WRK' because its workflow is not in a buffer component.");
			AssertNoErrors(task2.P9_StatusInfo);
			AssertNoWarnings(task2.P9_StatusInfo);
			AssertHasError(task3.P9_StatusInfo, "This task’s status cannot be set to 'WRK' because its workflow is not in a buffer component.");
		}

		public void TestWorkingOffBuffer_CLSToSUS_ShouldShowNotifications()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;
			SetWorkingStatusChange(WorkflowDescriptors.DummyWorkflowDescriptorCode, "WRN");

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket = BMSTestHelper.CreateBucket(system);
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = bucket.PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			AssertHasWarning(task.P9_StatusInfo, "This task’s status should not be set to 'SUS' because its workflow is not in a buffer component.");

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNoErrors(task.P9_StatusInfo);

			SetWorkingStatusChange(WorkflowDescriptors.DummyWorkflowDescriptorCode, "ERR");
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			AssertHasError(task.P9_StatusInfo, "This task’s status cannot be set to 'SUS' because its workflow is not in a buffer component.");
		}

		public void TestWorkingOffBuffer_CLSToWRK_ShouldShowNotifications()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;
			SetWorkingStatusChange(WorkflowDescriptors.DummyWorkflowDescriptorCode, "WRN");

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket = BMSTestHelper.CreateBucket(system);
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = bucket.PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertHasWarning(task.P9_StatusInfo, "This task’s status should not be set to 'WRK' because its workflow is not in a buffer component.");

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNoErrors(task.P9_StatusInfo);

			SetWorkingStatusChange(WorkflowDescriptors.DummyWorkflowDescriptorCode, "ERR");
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertHasError(task.P9_StatusInfo, "This task’s status cannot be set to 'WRK' because its workflow is not in a buffer component.");
		}

		public void TestWorkingOffBuffer_CANToSUS_ShouldShowNotifications()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;
			SetWorkingStatusChange(WorkflowDescriptors.DummyWorkflowDescriptorCode, "WRN");

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket = BMSTestHelper.CreateBucket(system);
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = bucket.PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			AssertHasWarning(task.P9_StatusInfo, "This task’s status should not be set to 'SUS' because its workflow is not in a buffer component.");

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertNoErrors(task.P9_StatusInfo);

			SetWorkingStatusChange(WorkflowDescriptors.DummyWorkflowDescriptorCode, "ERR");
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			AssertHasError(task.P9_StatusInfo, "This task’s status cannot be set to 'SUS' because its workflow is not in a buffer component.");
		}

		public void TestWorkingOffBuffer_CANToWRK_ShouldShowNotifications()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			Env.Security.WorkflowTasksAllowWorkingOnAnyTaskInAnyComponent.IsAllowed = false;
			SetWorkingStatusChange(WorkflowDescriptors.DummyWorkflowDescriptorCode, "WRN");

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket = BMSTestHelper.CreateBucket(system);
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "test");
			workflow.FH_FC_CurrentComponent = bucket.PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertHasWarning(task.P9_StatusInfo, "This task’s status should not be set to 'WRK' because its workflow is not in a buffer component.");

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertNoErrors(task.P9_StatusInfo);

			SetWorkingStatusChange(WorkflowDescriptors.DummyWorkflowDescriptorCode, "ERR");
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertHasError(task.P9_StatusInfo, "This task’s status cannot be set to 'WRK' because its workflow is not in a buffer component.");
		}

		#endregion

		#endregion

		#region CompletedTimeLocal

		public void TestClosedTasksWithNoLocalCompletionTime_ShouldShowError()
		{
			AssertNoErrors(ProcessTask.P9_ActualDurationInfo);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask.CompletedTimeLocal = ZDateTime.Empty;
			AssertHasError(ProcessTask.CompletedTimeLocalInfo, "A closed task must have a completion time.");
		}

		public void TestClosedTasksWithNoUTCCompletionTime_ShouldShowError()
		{
			AssertNoErrors(ProcessTask.P9_ActualDurationInfo);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask.P9_CompletedTimeUtc = ZDateTime.Empty;
			AssertHasError(ProcessTask.P9_CompletedTimeUtcInfo, "A closed task must have a completion time.");
		}

		#endregion

		#region P9_GS_NKAssignedStaffMember / P9_GG_AssignedGroup 

		void SetupRegistry(bool active, string restrictionType, string scope, string notificationType, string workflowType)
		{
			var categorisedWorkflowTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();

			var categorisedWorkflowTaskType = categorisedWorkflowTaskTypeCollection.AddNew();
			categorisedWorkflowTaskType.Code = workflowType;

			var workflowTaskType1 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType2 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType3 = categorisedWorkflowTaskType.TaskTypes.AddNew();

			workflowTaskType1.Code = "INV";
			workflowTaskType2.Code = "CDU";
			workflowTaskType3.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedWorkflowTaskTypeCollection);

			var collection = new TaskTypeRestrictionsCollection();

			var restriction = collection.AddNew();
			restriction.Active = active;
			restriction.WorkflowType = workflowType;
			restriction.TaskType = "INV";
			restriction.NotificationType = notificationType;
			restriction.Scope = scope;
			restriction.RestrictionType = restrictionType;

			var taskType1 = restriction.TaskTypesCollection.AddNew();
			var taskType2 = restriction.TaskTypesCollection.AddNew();

			taskType1.Code = "CDU";
			taskType2.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Warning()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Warning, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertHasWarning(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV should not be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasWarning(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU should not be assigned to the same resource as tasks of type INV");
			AssertHasWarning(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF should not be assigned to the same resource as tasks of type INV");
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Warning_ClosedTask()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Warning, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertNoWarnings(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertHasWarning(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU should not be assigned to the same resource as tasks of type INV");
			AssertHasWarning(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF should not be assigned to the same resource as tasks of type INV");
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Warning_CancelledTask()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Warning, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertNoWarnings(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings(task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings(task3.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertHasError(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV cannot be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU cannot be assigned to the same resource as tasks of type INV");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF cannot be assigned to the same resource as tasks of type INV");
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error_ClosedTask()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU cannot be assigned to the same resource as tasks of type INV");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF cannot be assigned to the same resource as tasks of type INV");
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error_CancelledTask()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task3.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_SameResource_Warning()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.SameResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Warning, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflowProvider = jobHeader.Parent as IWorkflowProvider;
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = "OTH";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			workflowProvider.WorkflowItems.ValidationCache.Refresh();

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertHasWarning(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV should be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasWarning(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU should be assigned to the same resource as tasks of type INV, CDF");
			AssertHasWarning(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF should be assigned to the same resource as tasks of type INV, CDU");
		}

		public void TestP9_GS_NKAssignedStaffMember_SameResource_AfterAllStaffUnassigned_ShouldHaveNoWarnings()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.SameResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Warning, "ORG");

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			task1.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task2.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task3.P9_GS_NKAssignedStaffMember = ZString.Empty;

			AssertNoWarnings("Task should have no warnings", task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings("Task should have no warnings", task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings("Task should have no warnings", task3.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_SameResource_AfterStaffAutoAssignedWithSAMRule_ShouldHaveNoWarnings()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.SameResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Warning, "ORG");

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			var workflowProvider = jobHeader.Parent as IWorkflowProvider;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "INV");

			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			workflowProvider.WorkflowItems.ValidationCache.Refresh();
			(task1 as ProcessTasks).Validation.ValidateAll();

			AssertNoWarnings("Task should have no warnings", task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings("Task should have no warnings", task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings("Task should have no warnings", task3.P9_GS_NKAssignedStaffMemberInfo);
		}
		public void TestP9_GS_NKAssignedStaffMember_SameResource_ShouldRevalidateAllRelatedTasksRegardlessIfEmptyOrFilled()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.SameResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Warning, "ORG");

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "INV");

			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			AssertNoWarnings("Task should have no warnings", task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings("Task should have no warnings", task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings("Task should have no warnings", task3.P9_GS_NKAssignedStaffMemberInfo);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_GS_NKAssignedStaffMember = ZString.Empty;

			AssertHasWarning(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF should be assigned to the same resource as tasks of type INV, CDU");
			AssertHasWarning(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU should be assigned to the same resource as tasks of type INV, CDF");
			AssertHasWarning(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV should be assigned to the same resource as tasks of type CDU, CDF");

			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			AssertNoWarnings("Task should have no warnings", task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings("Task should have no warnings", task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings("Task should have no warnings", task3.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_Set_DuringStaffAutoAssignWithSAMRule_ShouldNotBeCalledRecursively()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.SameResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Warning, DummyWorkflowDescriptor.Instance.Code);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");

			var task1 = (DummyProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CDF");
			var task2 = (DummyProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = (DummyProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "INV");

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			task1.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task2.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task3.P9_GS_NKAssignedStaffMember = ZString.Empty;

			task1.NumberOfTimesStaffMemberCalled = 0;
			task2.NumberOfTimesStaffMemberCalled = 0;
			task3.NumberOfTimesStaffMemberCalled = 0;

			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Assert("SAMRule AutoAssign should not have called P9_GS_NKAssignedStaffMember Set directly", task1.NumberOfTimesStaffMemberCalled == 0);
			Assert("SAMRule AutoAssign should not have called P9_GS_NKAssignedStaffMember Set directly", task2.NumberOfTimesStaffMemberCalled == 0);
			Assert("SAMRule AutoAssign should not have called P9_GS_NKAssignedStaffMember Set directly", task3.NumberOfTimesStaffMemberCalled == 1);
		}

		public void TestP9_GS_NKAssignedStaffMember_SameResource_Warning_ClosedTask()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.SameResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Warning, "ORG");

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "OTH";

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertHasWarning(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV should be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasWarning(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU should be assigned to the same resource as tasks of type INV, CDF");
			AssertHasWarning(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF should be assigned to the same resource as tasks of type INV, CDU");
		}

		public void TestP9_GS_NKAssignedStaffMember_SameResource_Warning_CancelledTask()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.SameResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Warning, "ORG");

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "OTH";

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertNoWarnings(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings(task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings(task3.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_SameResource_Error()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.SameResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");
			var workflowProvider = jobHeader.Parent as IWorkflowProvider;

			task1.P9_GS_NKAssignedStaffMember = "OTH";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			workflowProvider.WorkflowItems.ValidationCache.Refresh();

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertHasError(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV must be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU must be assigned to the same resource as tasks of type INV, CDF");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF must be assigned to the same resource as tasks of type INV, CDU");
		}

		public void TestP9_GS_NKAssignedStaffMember_SameResource_Error_ClosedTask()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.SameResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error, "ORG");

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "OTH";

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertHasError(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV must be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU must be assigned to the same resource as tasks of type INV, CDF");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF must be assigned to the same resource as tasks of type INV, CDU");
		}

		public void TestP9_GS_NKAssignedStaffMember_SameResource_Error_CancelledTask()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.SameResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error, "ORG");

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "OTH";

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task3.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_Inactive_DiffResource_NoError()
		{
			SetupRegistry(false, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task3.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error_PreRequisiteWorkflow()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Prerequisites, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2");
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = buffer.PK;

			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow2, taskType: "INV");
			var task3 = BMSTestHelper.CreateTask(workflow2, taskType: "CDU");
			var task4 = BMSTestHelper.CreateTask(workflow2, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task4.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV cannot be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU cannot be assigned to the same resource as tasks of type INV");
			AssertHasError(task4.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF cannot be assigned to the same resource as tasks of type INV");
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error_PreRequisiteWorkflow_ClosedTask()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Prerequisites, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2");
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = buffer.PK;

			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow2, taskType: "INV");
			var task3 = BMSTestHelper.CreateTask(workflow2, taskType: "CDU");
			var task4 = BMSTestHelper.CreateTask(workflow2, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task4.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV cannot be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU cannot be assigned to the same resource as tasks of type INV");
			AssertHasError(task4.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF cannot be assigned to the same resource as tasks of type INV");
		}

		public void TestP9_GS_NKAssignedStaffMember_SameResource_Error_PreRequisiteWorkflow_OnlyParentRestrictionAsPivot()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.SameResource, ScopeList.Codes.Prerequisites, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var otherStaff = GlbStaff.New(Factory);
			otherStaff.GS_Code = "OTH";

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2");
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = buffer.PK;
			workflow3.FH_FC_CurrentComponent = buffer.PK;

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "INV");   // pivot
			var task2 = BMSTestHelper.CreateTask(workflow2, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow2, taskType: "CDF");
			var task4 = BMSTestHelper.CreateTask(workflow3, taskType: "INV");   // pivot
			var task5 = BMSTestHelper.CreateTask(workflow3, taskType: "CDU");
			var task6 = BMSTestHelper.CreateTask(workflow3, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task4.P9_GS_NKAssignedStaffMember = otherStaff.GS_Code;
			task5.P9_GS_NKAssignedStaffMember = otherStaff.GS_Code;
			task6.P9_GS_NKAssignedStaffMember = otherStaff.GS_Code;

			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU must be assigned to the same resource as tasks of type INV, CDF");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF must be assigned to the same resource as tasks of type INV, CDU");
			AssertHasError(task4.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV must be assigned to the same resource as tasks of type CDU, CDF");
			AssertNoErrors(task5.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task6.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_DifferentResource_Error_PreRequisiteWorkflow_ShouldValidateTasksInBothWorkflows()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Prerequisites, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var otherStaff = GlbStaff.New(Factory);
			otherStaff.GS_Code = "OTH";

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2");
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = buffer.PK;

			workflow1.GetOrCreateDependencyLink(workflow2);

			var task2 = BMSTestHelper.CreateTask(workflow2, taskType: "INV");
			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "CDF", staffCode: otherStaff.GS_Code);

			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			AssertNoErrors(task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			AssertHasError(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF cannot be assigned to the same resource as tasks of type INV");
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV cannot be assigned to the same resource as tasks of type CDU, CDF");

			task1.P9_Type = "INV";
			task2.P9_Type = "CDF";
			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			AssertNoErrors(task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_DifferentResource_Error_PostRequisiteWorkflow_ShouldValidateTasksInBothWorkflows()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Postrequisites, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var otherStaff = GlbStaff.New(Factory);
			otherStaff.GS_Code = "OTH";

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2");
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = buffer.PK;

			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "INV");
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow2, taskType: "CDF", staffCode: otherStaff.GS_Code);

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task2.P9_GS_NKAssignedStaffMemberInfo);

			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.Validation.ValidateP9_GS_NKAssignedStaffMember();
			AssertHasError(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV cannot be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF cannot be assigned to the same resource as tasks of type INV");

			task1.P9_Type = "CDF";
			task2.P9_Type = "INV";
			task1.Validation.ValidateAll();
			task2.Validation.ValidateAll();
			AssertNoErrors(task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error_PostRequisiteWorkflow()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Postrequisites, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2");
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = buffer.PK;

			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow2, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow2, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertHasError(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV cannot be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU cannot be assigned to the same resource as tasks of type INV");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF cannot be assigned to the same resource as tasks of type INV");
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error_PostRequisiteWorkflow_ClosedTask()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Postrequisites, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2");
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = buffer.PK;

			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow2, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow2, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU cannot be assigned to the same resource as tasks of type INV");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF cannot be assigned to the same resource as tasks of type INV");
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error_JobWorkflow()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Job, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow3");
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = buffer.PK;
			workflow3.FH_FC_CurrentComponent = buffer.PK;

			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow2, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow3, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;   // pivot
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertHasError(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV cannot be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU cannot be assigned to the same resource as tasks of type INV");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF cannot be assigned to the same resource as tasks of type INV");
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error_ChildWorkflow()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Child, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2");
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = buffer.PK;

			workflow2.GetOrCreateLinkToParent(workflow1);

			var task1 = BMSTestHelper.CreateTask(workflow1, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow2, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow2, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertHasError(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV cannot be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU cannot be assigned to the same resource as tasks of type INV");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF cannot be assigned to the same resource as tasks of type INV");
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error_TaskWithinSameWorkflow()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow3");
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = buffer.PK;
			workflow3.FH_FC_CurrentComponent = buffer.PK;

			workflow2.GetOrCreateLinkToParent(workflow1);

			var task1 = BMSTestHelper.CreateTask(workflow2, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow2, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow1, taskType: "CDF");
			var task4 = BMSTestHelper.CreateTask(workflow3, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task4.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertHasError(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV cannot be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU cannot be assigned to the same resource as tasks of type INV");
			AssertNoErrors(task3.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task4.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error_WithTriggers()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var component = BMSTestHelper.CreateBucket(system);

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(dummy, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = component.PK;

			var trigger = dummy.WorkflowItems.Triggers.AddNew();

			var task1 = dummy.WorkflowItems.Tasks.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;

			task1.P9_Type = "INV";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			AssertEquals(2, task1.ParentTaskCollection.Count);
			AssertEquals(1, task1.ParentTaskCollection.Tasks.Count);
			AssertEquals(1, task1.ParentTaskCollection.Triggers.Count);
			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error2()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error, DummyWorkflowDescriptor.Instance.Code);

			var system = BMSTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);
			var component = BMSTestHelper.CreateBucket(system);

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(dummy, Factory);
			jobHeader.ProcessHeaders[0].FH_FC_CurrentComponent = component.PK;

			var task1 = dummy.WorkflowItems.Tasks.AddNew();
			var task2 = dummy.WorkflowItems.Tasks.AddNew();
			var task3 = dummy.WorkflowItems.Tasks.AddNew();

			task1.P9_FH_ProcessHeader = jobHeader.ProcessHeaders[0].PK;
			task1.P9_Type = "CDU";
			task1.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);

			task2.P9_FH_ProcessHeader = jobHeader.ProcessHeaders[0].PK;
			task2.P9_Type = "CDF";
			task2.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
			AssertNoErrors(task2.P9_GS_NKAssignedStaffMemberInfo);

			task3.P9_FH_ProcessHeader = jobHeader.ProcessHeaders[0].PK;
			task3.P9_Type = "INV";
			task3.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
			task3.Validation.ValidateP9_GS_NKAssignedStaffMember();
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV cannot be assigned to the same resource as tasks of type CDU, CDF");
		}

		public void TestP9_GS_NKAssignedStaffMember_DiffResource_Error_NoResourceAssigned()
		{
			SetupRegistry(true, RestrictionTypeList.Codes.DifferentResource, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error, "ORG");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var jobHeader = BMSTestHelper.CreateJobHeader<IOrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF");

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			((ProcessTask)task1).Validation.ValidateAll();
			((ProcessTask)task2).Validation.ValidateAll();
			((ProcessTask)task3).Validation.ValidateAll();

			AssertHasError(task1.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type INV cannot be assigned to the same resource as tasks of type CDU, CDF");
			AssertHasError(task2.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDU cannot be assigned to the same resource as tasks of type INV");
			AssertHasError(task3.P9_GS_NKAssignedStaffMemberInfo, "Tasks of type CDF cannot be assigned to the same resource as tasks of type INV");

			var capability = Factory.NewWithValidTestData<GlbCapability>();

			task1.P9_G4_RequiredCapability = capability.PK;
			task2.P9_G4_RequiredCapability = capability.PK;
			task3.P9_G4_RequiredCapability = capability.PK;

			task1.P9_GS_NKAssignedStaffMember = "";
			task2.P9_GS_NKAssignedStaffMember = "";
			task3.P9_GS_NKAssignedStaffMember = "";

			AssertNoErrors(task1.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task2.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(task3.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_Buffer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var component = BMSTestHelper.CreateBucket(system);

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(dummy, Factory);
			jobHeader.ProcessHeaders[0].FH_FC_CurrentComponent = component.PK;

			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_FH_ProcessHeader = jobHeader.ProcessHeaders[0].PK;

			task.P9_GS_NKAssignedStaffMember = "ZA";
			task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			AssertNoWarning(task.P9_GS_NKAssignedStaffMemberInfo, "Tasks on workflows that are in a Buffer component should be assigned. If they are not, transfer rules may not operate correctly.");

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			task.P9_GS_NKAssignedStaffMember = "ZA";
			task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			AssertHasWarning(task.P9_GS_NKAssignedStaffMemberInfo, "Tasks on workflows that are in a Buffer component should be assigned. If they are not, transfer rules may not operate correctly.");

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			task.P9_GS_NKAssignedStaffMember = "ZA";
			task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			AssertNoWarning(task.P9_GS_NKAssignedStaffMemberInfo, "Tasks on workflows that are in a Buffer component should be assigned. If they are not, transfer rules may not operate correctly.");
		}

		public void TestBufferValidation_ShouldNotApplyToIgnoredTaskTypes()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var job = Factory.New<OrgHeader>();
			var workflow = ProcessJobHeaderProvider.GetForParent(job, Factory).ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_Type = "CHK";

			task.Validation.ValidateAll();
			AssertHasWarning(task.P9_GS_NKAssignedStaffMemberInfo, "Tasks on workflows that are in a Buffer component should be assigned. If they are not, transfer rules may not operate correctly.");
			AssertHasWarning(task.P9_EstDurationInfo, "Tasks on workflows that are in a Buffer component should have estimates entered. If they do not, transfer rules may not operate correctly.");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = "ORG";
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "CHK";
			taskType.IsExcludedFromTransferRules = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			task.Validation.ValidateAll();
			AssertNoWarning(task.P9_GS_NKAssignedStaffMemberInfo, "Tasks on workflows that are in a Buffer component should be assigned. If they are not, transfer rules may not operate correctly.");
			AssertNoWarning(task.P9_EstDurationInfo, "Tasks on workflows that are in a Buffer component should have estimates entered. If they do not, transfer rules may not operate correctly.");
		}

		public void TestP9_GS_NKAssignedStaffMember_P9_GG_AssignedGroup()
		{
			var group = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL"));
			ObjectFactory.Get<IBMSRegistry>().RequireResourceToCloseTask = false;

			ProcessTask.P9_GG_AssignedGroup = ZGuid.Empty;
			ProcessTask.P9_GS_NKAssignedStaffMember = ZString.Empty;
			AssertNoErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(ProcessTask.P9_GG_AssignedGroupInfo);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ProcessTask.P9_GG_AssignedGroup = ZGuid.Empty;
			ProcessTask.P9_GS_NKAssignedStaffMember = ZString.Empty;
			AssertHasErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertHasErrors(ProcessTask.P9_GG_AssignedGroupInfo);

			ProcessTask.P9_GG_AssignedGroup = group.PK;
			AssertNoErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(ProcessTask.P9_GG_AssignedGroupInfo);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ProcessTask.P9_GG_AssignedGroup = ZGuid.Empty;
			ProcessTask.P9_GS_NKAssignedStaffMember = ZString.Empty;
			ProcessTask.P9_G4_RequiredCapability = Factory.New<GlbCapability>().PK;
			AssertNoErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(ProcessTask.P9_GG_AssignedGroupInfo);

			ProcessTask.P9_G4_RequiredCapability = ZGuid.Empty;
			AssertHasErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertHasErrors(ProcessTask.P9_GG_AssignedGroupInfo);

			ProcessTask.P9_GG_AssignedGroup = group.PK;
			AssertNoErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(ProcessTask.P9_GG_AssignedGroupInfo);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask.P9_GG_AssignedGroup = ZGuid.Empty;
			ProcessTask.P9_GS_NKAssignedStaffMember = ZString.Empty;
			AssertHasErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertHasErrors(ProcessTask.P9_GG_AssignedGroupInfo);

			ObjectFactory.Get<IBMSRegistry>().RequireResourceToCloseTask = true;
			ProcessTask.P9_GG_AssignedGroup = group.PK;
			AssertHasErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			ObjectFactory.Get<IBMSRegistry>().RequireResourceToCloseTask = false;

			ProcessTask.P9_GG_AssignedGroup = group.PK;
			AssertNoErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(ProcessTask.P9_GG_AssignedGroupInfo);

			ProcessTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			AssertNoErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(ProcessTask.P9_GG_AssignedGroupInfo);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			ProcessTask.P9_GG_AssignedGroup = ZGuid.Empty;
			ProcessTask.P9_GS_NKAssignedStaffMember = ZString.Empty;
			AssertNoErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(ProcessTask.P9_GG_AssignedGroupInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_RequireCapability()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "COD";

			ProcessTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			AssertNoWarnings(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings(ProcessTask.P9_G4_RequiredCapabilityInfo);

			ProcessTask.P9_G4_RequiredCapability = capability.PK;
			AssertHasWarnings("The user ST1 does not possess the capability COD.", ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertHasWarnings("The user ST1 does not possess the capability COD.", ProcessTask.P9_G4_RequiredCapabilityInfo);

			staff.Capabilities.Add(capability);
			ProcessTask.Validation.ValidateP9_GS_NKAssignedStaffMember();
			AssertNoWarnings(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings(ProcessTask.P9_G4_RequiredCapabilityInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_RequireCapability_Error()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "CP1";

			ProcessTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			AssertNoWarnings(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoWarnings(ProcessTask.P9_G4_RequiredCapabilityInfo);

			using (WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ProcessTask.P9_G4_RequiredCapability = capability.PK;
				AssertNoWarnings(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
				AssertNoWarnings(ProcessTask.P9_G4_RequiredCapabilityInfo);
				AssertHasError(ProcessTask.P9_GS_NKAssignedStaffMemberInfo, "The user ST1 must possess the capability CP1.");
				AssertHasError(ProcessTask.P9_G4_RequiredCapabilityInfo, "The user ST1 must possess the capability CP1.");

				staff.Capabilities.Add(capability);
				ProcessTask.Validation.ValidateP9_GS_NKAssignedStaffMember();
				AssertNoWarnings(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
				AssertNoWarnings(ProcessTask.P9_G4_RequiredCapabilityInfo);
				AssertNoErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
				AssertNoErrors(ProcessTask.P9_G4_RequiredCapabilityInfo);
			}
		}

		#region Task Group Has No Members

		public void TestP9_GG_AssignedGroup_WhenGroupHasNoMembers_AndTaskHasGlobalCapabilityAssigned_ShouldNotHaveNotification()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			AssertEquals(GlbCapabilityScopeList.Codes.GlobalScope, capability.G4_CapacityScope);

			Factory.Save();

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_GG_AssignedGroup = group.PK;

			Factory.Save();

			AssertHasWarning("There should be a warning because the task doesn't have an assigned capability. SAD!", task.P9_GG_AssignedGroupInfo, "The assigned task group has no members.");

			task.P9_G4_RequiredCapability = capability.PK;
			AssertNoNotifications("The task is now assigned to a global capability, so no warning should be shown. SAD!", task.P9_GG_AssignedGroupInfo);
		}

		public void TestP9_GG_AssignedGroup_WhenTaskGroupHasNoMembers_TriggeredByChangingTaskGroup()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			group2.Staff.Add(staff);

			Factory.Save();

			var task = Factory.NewWithValidTestData<ProcessTask>();
			AssertNoNotifications(task.P9_GG_AssignedGroupInfo);

			task.P9_GG_AssignedGroup = group1.PK;
			AssertHasWarning(task.P9_GG_AssignedGroupInfo, "The assigned task group has no members.");

			task.P9_GG_AssignedGroup = group2.PK;
			AssertNoNotifications(task.P9_GG_AssignedGroupInfo);

			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			task.P9_GG_AssignedGroup = group1.PK;
			AssertHasError(task.P9_GG_AssignedGroupInfo, "The assigned task group has no members.");
		}

		public void TestP9_GG_AssignedGroup_WhenTaskGroupHasNoMembers_TriggeredByChangingStaff()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_GG_AssignedGroup = group.PK;
			AssertHasWarning(task.P9_GG_AssignedGroupInfo, "The assigned task group has no members.");

			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			AssertNoNotifications(task.P9_GG_AssignedGroupInfo);

			task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			AssertHasWarning(task.P9_GG_AssignedGroupInfo, "The assigned task group has no members.");
		}

		public void TestTaskGroupHasNoMembers_WhenCapabilityEmpty_ShouldNotLoadGlbStaffRecords()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = group.Staff.AddNew();
			var staff2 = group.Staff.AddNew();
			staff1.FillWithValidTestData();
			staff2.FillWithValidTestData();

			var task = Factory.New<ProcessTask>();
			task.P9_GG_AssignedGroup = group.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Miss Rona" };
			var loadedTask = newFactory.Load<ProcessTask>(task.PK);
			var validation = new ProcessTaskValidation_ForTest(loadedTask);
			var expectedHits = new Dictionary<string, int>
			{
				{ GlbGroupLinkSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 0 },
			};

			using (TestConnection.TrackExecutedCommands())
			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				validation.CheckTaskGroupHasMembers_Exposed();

				var pivotCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("FROM dbo.GlbGroupLink"));
				AssertEquals("All other conditions were met, so we should have executed a query to check for group membership. SAD!", 1, pivotCommands.Count());
			}

			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			var loadedStaff = newFactory.Load<GlbStaff>(query);
			AssertContainsExactElementsInAnyOrder("No staff records should have been loaded in order to validate Assigned Group. SAD!", Array.Empty<string>(), loadedStaff.Select(x => x.GS_Code));

			var loadedPivots = newFactory.Load<GlbGroupLink>(query);
			AssertEquals("One pivot row should be loaded the first time the validation runs (if there isn't one in the factory already). SAD!", 1, loadedPivots.Length);
		}

		public void TestTaskGroupHasNoMembers_WhenCapabilityIsGlobalScope_ShouldNotCheckGroupStaffMembers_DbHits()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = group.Staff.AddNew();
			var staff2 = group.Staff.AddNew();
			staff1.FillWithValidTestData();
			staff2.FillWithValidTestData();

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var task = Factory.New<ProcessTask>();
			task.P9_GG_AssignedGroup = group.PK;
			task.P9_G4_RequiredCapability = capability.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Miss Rona" };
			var loadedTask = newFactory.Load<ProcessTask>(task.PK);
			var validation = new ProcessTaskValidation_ForTest(loadedTask);
			var expectedHits = new Dictionary<string, int>
			{
				{ GlbGroupLinkSchema.Constants.TableName, 0 },
				{ GlbStaffSchema.Constants.TableName, 0 },
			};

			using (TestConnection.TrackExecutedCommands())
			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				validation.CheckTaskGroupHasMembers_Exposed();

				var pivotCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("FROM dbo.GlbGroupLink"));
				AssertContainsExactElementsInAnyOrder("The most expensive query should be avoided if other conditions aren't first met. SAD!", Array.Empty<string>(), pivotCommands);
			}

			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			var loadedStaff = newFactory.Load<GlbStaff>(query);
			AssertContainsExactElementsInAnyOrder("No staff records should have been loaded in order to validate Assigned Group. SAD!", Array.Empty<string>(), loadedStaff.Select(x => x.GS_Code));

			var loadedPivots = newFactory.Load<GlbGroupLink>(query);
			AssertContainsExactElementsInAnyOrder("No pivot rows should have been loaded in order to validate Assigned Group. SAD!", Array.Empty<object>(), loadedPivots);
		}

		public void TestTaskGroupHasNoMembers_WhenCapabilityIsGroupScope_ShouldNotLoadGlbStaffRecords()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = group.Staff.AddNew();
			var staff2 = group.Staff.AddNew();
			staff1.FillWithValidTestData();
			staff2.FillWithValidTestData();

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var task1 = Factory.New<ProcessTask>();
			task1.P9_GG_AssignedGroup = group.PK;
			task1.P9_G4_RequiredCapability = capability.PK;

			var task2 = Factory.New<ProcessTask>();
			task2.P9_GG_AssignedGroup = group.PK;
			task2.P9_G4_RequiredCapability = capability.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Miss Rona" };
			var loadedTask = newFactory.Load<ProcessTask>(task1.PK);
			var validation = new ProcessTaskValidation_ForTest(loadedTask);
			var expectedHits = new Dictionary<string, int>
			{
				{ GlbGroupLinkSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 0 },
			};

			using (TestConnection.TrackExecutedCommands())
			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				validation.CheckTaskGroupHasMembers_Exposed();

				var pivotCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("FROM dbo.GlbGroupLink"));
				AssertEquals("All other conditions were met, so we should have executed a query to check for group membership. SAD!", 1, pivotCommands.Count());
			}

			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			var loadedStaff = newFactory.Load<GlbStaff>(query);
			AssertContainsExactElementsInAnyOrder("No staff records should have been loaded in order to validate Assigned Group. SAD!", Array.Empty<string>(), loadedStaff.Select(x => x.GS_Code));

			var loadedPivots = newFactory.Load<GlbGroupLink>(query);
			AssertEquals("One pivot row should be loaded the first time the validation runs (if there isn't one in the factory already). SAD!", 1, loadedPivots.Length);

			loadedTask = newFactory.Load<ProcessTask>(task2.PK);
			validation = new ProcessTaskValidation_ForTest(loadedTask);

			expectedHits = new Dictionary<string, int>
			{
				{ GlbGroupLinkSchema.Constants.TableName, 0 },
				{ GlbStaffSchema.Constants.TableName, 0 },
			};

			using (TestConnection.TrackExecutedCommands())
			using (AssertDbHitsForAllFactories("Running validation a second time for the same group should not require any more GlbGroupLinks to be loaded because one will satisfy the Factory.Exists call. SAD!",
				expectedHits, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				validation.CheckTaskGroupHasMembers_Exposed();

				var pivotCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("FROM dbo.GlbGroupLink"));
				AssertEquals("Running validation a second time for the same group should not require any more GlbGroupLinks to be loaded because one will satisfy the Factory.Exists call. SAD!", 0, pivotCommands.Count());
			}

			loadedStaff = newFactory.Load<GlbStaff>(query);
			AssertContainsExactElementsInAnyOrder("No staff records should have been loaded in order to validate Assigned Group. SAD!", Array.Empty<string>(), loadedStaff.Select(x => x.GS_Code));

			var loadedPivotsForSecondRun = newFactory.Load<GlbGroupLink>(query);
			AssertContainsExactElementsInAnyOrder("No new pivots should need to be loaded for subsequent validation runs for the same group. SAD!", loadedPivots, loadedPivotsForSecondRun);
		}

		#endregion

		#region Intersection Task Group and Capability Has No Members

		public void TestGroupCapabilityIntersection_DbHitsAndLoadedBizos()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.Capabilities.Add(capability);
			staff2.Capabilities.Add(capability);
			group.Staff.Add(staff1);
			group.Staff.Add(staff2);

			var task1 = Factory.New<ProcessTask>();
			task1.P9_G4_RequiredCapability = capability.PK;
			task1.P9_GG_AssignedGroup = group.PK;
			var task2 = Factory.New<ProcessTask>();
			task2.P9_G4_RequiredCapability = capability.PK;
			task2.P9_GG_AssignedGroup = group.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Miss Rona" };
			var loadedTask = newFactory.Load<ProcessTask>(task1.PK);
			var validation = new ProcessTaskValidation_ForTest(loadedTask);
			var expectedHits = new Dictionary<string, int>
			{
				{ GlbCapabilitySchema.Constants.TableName, 0 },
				{ GlbGroupLinkSchema.Constants.TableName, 0 },
				{ GlbResourceCapabilityPivotSchema.Constants.TableName, 0 },
				{ GlbStaffSchema.Constants.TableName, 0 },
			};

			using (TestConnection.TrackExecutedCommands())
			using (AssertDbHitsForAllFactories("We shouldn't need to load staff or pivots in order to determine a bad intersection. SAD!", expectedHits, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				validation.CheckCapabilityAllowsTaskToBeAssignedToResource_Exposed();

				var pivotCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("GK_GS = G5_GS_Resource"));
				AssertEquals("All other conditions were met, so we should have executed a query to check the intersection. SAD!", 1, pivotCommands.Count());
			}

			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			var loadedStaff = newFactory.Load<GlbStaff>(query);
			AssertContainsExactElementsInAnyOrder("No staff records should have been loaded in order to validate Assigned Capability. SAD!", Array.Empty<string>(), loadedStaff.Select(x => x.GS_Code));

			var loadedCapabilityPivots = newFactory.Load<GlbResourceCapabilityPivot>(query);
			AssertEquals("We should use a db query to calculate the intersection rather than loading business objects. SAD!", 0, loadedCapabilityPivots.Length);

			var loadedGroupPivots = newFactory.Load<GlbGroupLink>(query);
			AssertEquals("We should use a db query to calculate the intersection rather than loading business objects. SAD!", 0, loadedGroupPivots.Length);

			loadedTask = newFactory.Load<ProcessTask>(task2.PK);
			validation = new ProcessTaskValidation_ForTest(loadedTask);

			using (TestConnection.TrackExecutedCommands())
			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				validation.CheckCapabilityAllowsTaskToBeAssignedToResource_Exposed();

				var pivotCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("GK_GS = G5_GS_Resource"));
				AssertEquals("Running validation a second time for the same capability and group should not require another query because the result should be cached in the factory. SAD!", 0, pivotCommands.Count());
			}

			loadedStaff = newFactory.Load<GlbStaff>(query);
			AssertContainsExactElementsInAnyOrder("No staff records should have been loaded in order to validate Assigned Capability. SAD!", Array.Empty<string>(), loadedStaff.Select(x => x.GS_Code));

			var loadedCapabilityPivotsForSecondRun = newFactory.Load<GlbResourceCapabilityPivot>(query);
			AssertContainsExactElementsInAnyOrder("No new pivots should need to be loaded for subsequent validation runs for the same group and capability. SAD!", loadedCapabilityPivots, loadedCapabilityPivotsForSecondRun);

			var loadedGroupPivotsForSecondRun = newFactory.Load<GlbGroupLink>(query);
			AssertContainsExactElementsInAnyOrder("No new pivots should need to be loaded for subsequent validation runs for the same group and capability. SAD!", loadedGroupPivots, loadedGroupPivotsForSecondRun);
		}

		#endregion

		#endregion

		#region P9_G4_RequiredCapability

		#region Capability Has No Members

		public void TestP9_G4_RequiredCapability_WhenCapabilityHasNoMembers_TriggeredByChangingCapability()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			Factory.Save();

			var task = Factory.New<ProcessTask>();
			AssertNoNotifications(task.P9_G4_RequiredCapabilityInfo);

			task.P9_G4_RequiredCapability = capability.PK;

			AssertHasWarning(task.P9_G4_RequiredCapabilityInfo, "There are no users possessing the assigned capability.");

			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			task.Validation.ValidateP9_G4_RequiredCapability();
			AssertHasError(task.P9_G4_RequiredCapabilityInfo, "There are no users possessing the assigned capability.");
		}

		public void TestP9_G4_RequiredCapability_WhenCapabilityHasNoMembers_TriggeredByChangingStaff()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			Factory.Save();

			var task = Factory.New<ProcessTask>();
			task.P9_G4_RequiredCapability = capability.PK;

			AssertHasWarning(task.P9_G4_RequiredCapabilityInfo, "There are no users possessing the assigned capability.");

			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			AssertNoWarning(task.P9_G4_RequiredCapabilityInfo, "There are no users possessing the assigned capability.");
			AssertNoError(task.P9_G4_RequiredCapabilityInfo, "There are no users possessing the assigned capability.");
		}

		public void TestCapabilityHasNoMembers_WhenCapabilityAssigned_DbHits()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.Capabilities.Add(capability);
			staff2.Capabilities.Add(capability);

			var task1 = Factory.New<ProcessTask>();
			task1.P9_G4_RequiredCapability = capability.PK;
			var task2 = Factory.New<ProcessTask>();
			task2.P9_G4_RequiredCapability = capability.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Miss Rona" };
			var loadedTask = newFactory.Load<ProcessTask>(task1.PK);
			var validation = new ProcessTaskValidation_ForTest(loadedTask);
			var expectedHits = new Dictionary<string, int>
			{
				{ GlbResourceCapabilityPivotSchema.Constants.TableName, 0 },
				{ GlbStaffSchema.Constants.TableName, 0 },
			};

			using (TestConnection.TrackExecutedCommands())
			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				validation.CheckCapabilityHasMembers_Exposed();

				var pivotCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("FROM dbo.GlbResourceCapabilityPivot"));
				AssertEquals("All other conditions were met, so we should have executed a query to check for capability membership. SAD!", 1, pivotCommands.Count());
			}

			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			var loadedStaff = newFactory.Load<GlbStaff>(query);
			AssertContainsExactElementsInAnyOrder("No staff records should have been loaded in order to validate Assigned Capability. SAD!", Array.Empty<string>(), loadedStaff.Select(x => x.GS_Code));

			var loadedPivots = newFactory.Load<GlbResourceCapabilityPivot>(query);
			AssertEquals("One pivot row should be loaded the first time the validation runs (if there isn't one in the factory already). SAD!", 1, loadedPivots.Length);

			loadedTask = newFactory.Load<ProcessTask>(task2.PK);
			validation = new ProcessTaskValidation_ForTest(loadedTask);

			using (TestConnection.TrackExecutedCommands())
			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				validation.CheckCapabilityHasMembers_Exposed();

				var pivotCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("FROM dbo.GlbResourceCapabilityPivot"));
				AssertEquals("Running validation a second time for the same capability should not require any more GlbResourceCapabilityPivots to be loaded because one will satisfy the Factory.Exists call. SAD!", 0, pivotCommands.Count());
			}

			loadedStaff = newFactory.Load<GlbStaff>(query);
			AssertContainsExactElementsInAnyOrder("No staff records should have been loaded in order to validate Assigned Capability. SAD!", Array.Empty<string>(), loadedStaff.Select(x => x.GS_Code));

			var loadedPivotsForSecondRun = newFactory.Load<GlbResourceCapabilityPivot>(query);
			AssertContainsExactElementsInAnyOrder("No new pivots should need to be loaded for subsequent validation runs for the same group. SAD!", loadedPivots, loadedPivotsForSecondRun);
		}

		#endregion

		#region Intersection of Capability and Task Group / Release Group Has No Members

		public void TestIntersectionOfCapabilityAndTaskGroupOrReleaseGroupHasNoMembers_TriggeredByChangingCapability()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.Staff.Add(staff1);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.Staff.Add(staff2);

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability1.ResourcesWithCapability.Add(staff1);

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability2.ResourcesWithCapability.Add(staff2);

			Factory.Save(); // Validation will check the db for these objects' pivots, so they need to be saved first or we get false positives.

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow", releaseGroupPK: group2.PK.ToGuid());
			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow, capability: capability1, group: group1);

			Factory.Save();

			AssertNoNotifications("There should be no warnings yet because the task's capability and group share a resource. SAD!", task.P9_G4_RequiredCapabilityInfo);
			AssertNoNotifications("There should be no warnings yet because the task's capability and group share a resource. SAD!", task.P9_GG_AssignedGroupInfo);

			task.P9_G4_RequiredCapability = capability2.PK;

			AssertHasWarning("There should be a warning because, even though the workflow's release group shares resources with the task's capability, the group has been overridden on the task to one that doesn't have resources in common. SAD!",
				task.P9_G4_RequiredCapabilityInfo, "The intersection of the task capability and the task group / workflow release group has no resources in it.");
			AssertHasWarning("There should be a warning because, even though the workflow's release group shares resources with the task's capability, the group has been overridden on the task to one that doesn't have resources in common. SAD!",
				task.P9_GG_AssignedGroupInfo, "The intersection of the task group and the task capability has no resources in it.");

			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			task.Validation.ValidateP9_G4_RequiredCapability();
			AssertHasError(task.P9_G4_RequiredCapabilityInfo, "The intersection of the task capability and the task group / workflow release group has no resources in it.");
			AssertHasError(task.P9_GG_AssignedGroupInfo, "The intersection of the task group and the task capability has no resources in it.");
		}

		public void TestIntersectionOfCapabilityAndTaskGroupOrReleaseGroupHasNoMembers_TriggeredByChangingTaskGroup()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.Staff.Add(staff1);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.Staff.Add(staff2);

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability1.ResourcesWithCapability.Add(staff2);

			Factory.Save(); // Validation will check the db for these objects' pivots, so they need to be saved first or we get false positives.

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow", releaseGroupPK: group2.PK.ToGuid());
			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow, capability: capability1); // no group

			Factory.Save();

			AssertNoNotifications("There should be no warnings yet because the task's capability and group share a resource. SAD!", task.P9_G4_RequiredCapabilityInfo);
			AssertNoNotifications("There should be no warnings yet because the task's capability and group share a resource. SAD!", task.P9_GG_AssignedGroupInfo);

			task.P9_GG_AssignedGroup = group1.PK;

			AssertHasWarning("There should be a warning because, even though the workflow's release group shares resources with the task's capability, the group has been overridden on the task to one that doesn't have resources in common. SAD!",
				task.P9_G4_RequiredCapabilityInfo, "The intersection of the task capability and the task group / workflow release group has no resources in it.");
			AssertHasWarning("There should be a warning because, even though the workflow's release group shares resources with the task's capability, the group has been overridden on the task to one that doesn't have resources in common. SAD!",
				task.P9_GG_AssignedGroupInfo, "The intersection of the task group and the task capability has no resources in it.");
		}

		public void TestIntersectionOfCapabilityAndTaskGroupOrReleaseGroupHasNoMembers_TriggeredByChangingReleaseGroup()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.Staff.Add(staff1);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.Staff.Add(staff2);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability.ResourcesWithCapability.Add(staff2);

			Factory.Save(); // Validation will check the db for these objects' pivots, so they need to be saved first or we get false positives.

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow", releaseGroupPK: group2.PK.ToGuid());
			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow, capability: capability); // no group

			Factory.Save();

			AssertNoNotifications("There should be no warnings yet because the task's capability and group share a resource. SAD!", task.P9_G4_RequiredCapabilityInfo);
			AssertNoNotifications("Task group is blank so it shouldn't have notifications.", task.P9_GG_AssignedGroupInfo);

			workflow.FH_GG_ReleaseGroup = group1.PK;
			AssertHasWarning(workflow.FH_GG_ReleaseGroupInfo, "The intersection of the workflow release group and task capabilities for some of the tasks has no resources in it.");
			AssertNoNotifications("Setting the release group on the workflow shouldn't trigger all tasks to validate, as this could be slow. The notification will appear on the workflow, though.", task.P9_G4_RequiredCapabilityInfo);
			AssertNoNotifications("Task group is blank so it shouldn't have notifications.", task.P9_GG_AssignedGroupInfo);

			task.Validation.ValidateAll();
			AssertHasWarning("There should be a warning because the workflow's release group was changed to one that doesn't have resources in common with the task's assigned capability. SAD!",
				task.P9_G4_RequiredCapabilityInfo, "The intersection of the task capability and the task group / workflow release group has no resources in it.");
			AssertNoNotifications("Task group is blank so it shouldn't have notifications.", task.P9_GG_AssignedGroupInfo);
		}

		public void TestIntersectionOfCapabilityAndTaskGroupOrReleaseGroupHasNoMembers_TriggeredByChangingStaff()
		{
			WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.Staff.Add(staff1);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.Staff.Add(staff2);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability.ResourcesWithCapability.Add(staff2);

			Factory.Save(); // Validation will check the db for these objects' pivots, so they need to be saved first or we get false positives.

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow", releaseGroupPK: group2.PK.ToGuid());
			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow, staffCode: staff2.GS_Code, capability: capability, group: group1); // no group

			Factory.Save();

			AssertNoNotifications("There should be no warnings yet because there is a staff member assigned. SAD!", task.P9_G4_RequiredCapabilityInfo);
			AssertNoNotifications("There should be no warnings yet because there is a staff member assigned. SAD!", task.P9_GG_AssignedGroupInfo);

			task.P9_GS_NKAssignedStaffMember = ZString.Empty;

			AssertHasWarning("There should be a warning because the assigned staff member has been removed, leaving the task with a group and capability that doesn't have common staff. SAD!",
				task.P9_G4_RequiredCapabilityInfo, "The intersection of the task capability and the task group / workflow release group has no resources in it.");
			AssertHasWarning("There should be a warning because the assigned staff member has been removed, leaving the task with a group and capability that doesn't have common staff. SAD!",
				task.P9_GG_AssignedGroupInfo, "The intersection of the task group and the task capability has no resources in it.");
		}

		#endregion

		#endregion

		#region P9_OA

		public void TestP9_OA()
		{
			AssertNoErrors(ProcessTask.P9_OAInfo);
			ProcessTask.OrganisationPK = ZGuid.NewZGuid();
			Assert("Precondition: P9_OA is Empty", ProcessTask.P9_OA.IsEmpty);
			ProcessTask.Validation.ValidateAll();
			AssertHasError(ProcessTask.P9_OAInfo, "Please enter a Task Client Address.");
		}

		#endregion

		#region P9_EstimateVariationFactor

		public void TestP9_EstimateVariationFactor()
		{
			AssertNoErrors(ProcessTask.P9_EstimateVariationFactorInfo);
			ProcessTask.P9_EstimateVariationFactor = 3;
			AssertNoErrors(ProcessTask.P9_EstimateVariationFactorInfo);

			ProcessTask.P9_EstimateVariationFactor = 0;
			AssertHasErrors(ProcessTask.P9_EstimateVariationFactorInfo);

			ProcessTask.P9_EstimateVariationFactor = 1;
			AssertNoErrors(ProcessTask.P9_EstimateVariationFactorInfo);
		}

		#endregion

		#region P9_EstDuration

		public void TestP9_EstDuration_Buffer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var component = BMSTestHelper.CreateBucket(system);

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(dummy, Factory);
			jobHeader.ProcessHeaders[0].FH_FC_CurrentComponent = component.PK;

			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_FH_ProcessHeader = jobHeader.ProcessHeaders[0].PK;

			task.P9_EstDuration = new ZDateTime(2013, 1, 1, 2, 0, 0);
			task.P9_EstDuration = ZDateTime.Empty;
			AssertNoWarning(task.P9_EstDurationInfo, "Tasks on workflows that are in a Buffer component should have estimates entered. If they do not, transfer rules may not operate correctly.");

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			task.P9_EstDuration = new ZDateTime(2013, 1, 1, 2, 0, 0);
			task.P9_EstDuration = ZDateTime.Empty;
			AssertHasWarning(task.P9_EstDurationInfo, "Tasks on workflows that are in a Buffer component should have estimates entered. If they do not, transfer rules may not operate correctly.");

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			task.P9_EstDuration = new ZDateTime(2013, 1, 1, 2, 0, 0);
			task.P9_EstDuration = ZDateTime.Empty;
			AssertNoWarning(task.P9_EstDurationInfo, "Tasks on workflows that are in a Buffer component should have estimates entered. If they do not, transfer rules may not operate correctly.");
		}

		public void TestP9_EstDuration_CompletionStatementTask()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = "DUM";
			var completionStatementTaskType = taskTypes.TaskTypes.AddNew();
			completionStatementTaskType.Code = "COM";
			completionStatementTaskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(dummy, Factory);
			jobHeader.ProcessHeaders[0].FH_FC_CurrentComponent = buffer.PK;

			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_FH_ProcessHeader = jobHeader.ProcessHeaders[0].PK;

			task.Validation.ValidateP9_EstDuration();
			AssertHasWarning(task.P9_EstDurationInfo, "Tasks on workflows that are in a Buffer component should have estimates entered. If they do not, transfer rules may not operate correctly.");

			task.P9_Type = completionStatementTaskType.Code;
			Assert(task.IsCompletionTask());

			task.Validation.ValidateP9_EstDuration();
			AssertNoWarning(task.P9_EstDurationInfo, "Tasks on workflows that are in a Buffer component should have estimates entered. If they do not, transfer rules may not operate correctly.");
		}

		public void TestVeryHighEstimatedDurationAndVariationFactor_ShouldShowError()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_EstDuration = TaskDurationCalculator.GetDurationFromTimeSpan(TimeSpan.FromHours(100));
			task.P9_EstimateVariationFactor = 37;
			AssertEquals(3700m, task.HighEstimatedDurationHours);
			AssertHasError(task.P9_EstDurationInfo, "The Low Estimate/Estimate Variation Factor combination entered results in a High Estimate which is too high. Please adjust the Low Estimate and/or Estimate Variation Factor values so that the High Estimate is no greater than 150 days (3600 hours).");

			task.P9_EstimateVariationFactor = 36;
			AssertNoErrors(task.P9_EstDurationInfo);

			task.P9_EstDuration = TaskDurationCalculator.GetDurationFromTimeSpan(TimeSpan.FromHours(999));
			AssertHasError(task.P9_EstDurationInfo, "The Low Estimate/Estimate Variation Factor combination entered results in a High Estimate which is too high. Please adjust the Low Estimate and/or Estimate Variation Factor values so that the High Estimate is no greater than 150 days (3600 hours).");
		}

		#endregion

		#region P9_ShareTasksForAllCompanies

		public void TestP9_ShareTasksForAllCompanies_ForProcessTypeSupportingCompanySpecificTasks()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			var standaloneTask = Factory.New<ProcessTask>();

			templateTask.Validation.ValidateAll();

			AssertNoWarnings(templateTask);

			template.GlobalTemplate = true;

			AssertHasWarning(templateTask.P9_ShareTasksForAllCompaniesInfo, "This task is not marked as 'shared', but its template is marked as 'global'. This task will only be available in the company in which the template is applied.");
			AssertNoWarnings(standaloneTask);

			template.GlobalTemplate = false;

			AssertNoWarnings(templateTask);
			AssertNoWarnings(standaloneTask);

			template.GlobalTemplate = true;

			AssertHasWarning(templateTask.P9_ShareTasksForAllCompaniesInfo, "This task is not marked as 'shared', but its template is marked as 'global'. This task will only be available in the company in which the template is applied.");
			AssertNoWarnings(standaloneTask);

			templateTask.P9_ShareTasksForAllCompanies = true;
			standaloneTask.P9_ShareTasksForAllCompanies = true;

			AssertNoWarnings(templateTask);
			AssertNoWarnings(standaloneTask);
		}

		public void TestP9_ShareTasksForAllCompanies_ForProcessTypeNotSupportingCompanySpecificTasks()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "WKI";

			var templateTask = template.WorkflowItems.Tasks.AddNew();

			templateTask.Validation.ValidateAll();

			AssertNoWarnings(templateTask);

			template.GlobalTemplate = true;

			AssertNoWarnings(templateTask);
		}

		#endregion

		#region P9_EstimateDefaultedFromAsString

		public void TestP9_EstimateDefaultedFromAsString()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimateDefaultedFromAsString = "BOK";
			AssertHasError(milestone.P9_EstimateDefaultedFromAsStringInfo, "Enter a valid Estimated Defaulted From.");
		}

		#endregion

		#region P9_ActualDateUpdateType

		public void TestP9_ActualDateUpdateType()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();

			AssertEquals("Default", ActualDateUpdateTypeCodeList.Codes.AD1, milestone.P9_ActualDateUpdateType);

			milestone.P9_ActualDateUpdateType = "";
			AssertHasError(milestone.P9_ActualDateUpdateTypeInfo, "Please enter a Milestone Update Type.");

			milestone.P9_ActualDateUpdateType = "XXX";
			AssertHasError(milestone.P9_ActualDateUpdateTypeInfo, "Enter a valid Milestone Update Type.");

			milestone.P9_ActualDateUpdateType = ActualDateUpdateTypeCodeList.Codes.AD1;
			AssertNoErrors(milestone.P9_ActualDateUpdateTypeInfo);

			milestone.P9_ActualDateUpdateType = ActualDateUpdateTypeCodeList.Codes.UAD;
			AssertNoErrors(milestone.P9_ActualDateUpdateTypeInfo);

			milestone.P9_ActualDateUpdateType = ActualDateUpdateTypeCodeList.Codes.ALW;
			AssertNoErrors(milestone.P9_ActualDateUpdateTypeInfo);
		}

		#endregion

		#region P9_MilestoneCompletionPivotKey

		public void TestP9_MilestoneCompletionPivotKey()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Foo";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var task = dummy.WorkflowItems.Tasks.AddNew();

			task.P9_MilestoneCompletionPivotKey = "";
			AssertNoWarnings(task.P9_MilestoneCompletionPivotKeyInfo);

			task.P9_MilestoneCompletionPivotKey = "Frogs";
			AssertHasWarning(task.P9_MilestoneCompletionPivotKeyInfo, "Does not match any milestone.");

			task.P9_MilestoneCompletionPivotKey = milestone.P9_MilestoneCompletionPivotKey;
			AssertNoWarnings(task.P9_MilestoneCompletionPivotKeyInfo);
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		public void TestShouldValidateFKToCancelledRecord()
		{
			var validation = new ProcessTaskValidation_ForTest(ProcessTask);
			AssertEquals(true, validation.ShouldValidateFKToCancelledRecord_Exposed(ProcessTask.P9_FH_ProcessHeaderInfo));
			AssertEquals(false, validation.ShouldValidateFKToCancelledRecord_Exposed(ProcessTask.P9_OCInfo));
			AssertEquals(true, validation.ShouldValidateFKToCancelledRecord_Exposed(ProcessTask.P9_SE_NKExceptionEventInfo));
			ProcessTask.P9_Status = "CLS";
			AssertEquals(false, validation.ShouldValidateFKToCancelledRecord_Exposed(ProcessTask.P9_FH_ProcessHeaderInfo));
		}

		#endregion

		#region ProcessWorkflowException

		public void TestExceptionTypeCode()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TYP";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);

			var processWorkflowExceptionTypeInactive = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionTypeInactive.WET_Code = "TY0";
			processWorkflowExceptionTypeInactive.WET_Description = nameof(processWorkflowExceptionType);
			processWorkflowExceptionTypeInactive.WET_IsActive = false;

			var processWorkflowExceptionTypeOtherJobType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionTypeOtherJobType.WET_Code = "TYX";
			processWorkflowExceptionTypeOtherJobType.WET_Description = nameof(processWorkflowExceptionTypeOtherJobType);
			processWorkflowExceptionTypeOtherJobType.WET_JobType = "XXX";

			Factory.Save();

			AssertNoErrors(task.ExceptionTypeCodeInfo);
			AssertNoErrors(milestone.ExceptionTypeCodeInfo);
			AssertNoErrors(exception.ExceptionTypeCodeInfo);

			task.ExceptionTypeCode = "BLA";
			milestone.ExceptionTypeCode = "BLA";
			exception.ExceptionTypeCode = "BLA";

			AssertNoErrors(task.ExceptionTypeCodeInfo);
			AssertHasErrors(milestone.ExceptionTypeCodeInfo);
			AssertHasErrors(exception.ExceptionTypeCodeInfo);

			task.ExceptionTypeCode = "TYP";
			milestone.ExceptionTypeCode = "TYP";
			exception.ExceptionTypeCode = "TYP";

			AssertNoErrors(task.ExceptionTypeCodeInfo);
			AssertNoErrors(milestone.ExceptionTypeCodeInfo);
			AssertNoErrors(exception.ExceptionTypeCodeInfo);

			task.ExceptionTypeCode = "TY0";
			milestone.ExceptionTypeCode = "TY0";
			exception.ExceptionTypeCode = "TY0";

			AssertNoErrors(task.ExceptionTypeCodeInfo);
			AssertHasError(milestone.ExceptionTypeCodeInfo, "Exception type is inactive.");
			AssertHasError(exception.ExceptionTypeCodeInfo, "Exception type is inactive.");

			Factory.Save();

			exception.ExceptionTypeCode = "TYP";
			exception.ExceptionTypeCode = "TY0";
			milestone.ExceptionTypeCode = "TYP";
			milestone.ExceptionTypeCode = "TY0";

			AssertHasWarning(milestone.ExceptionTypeCodeInfo, "Exception type is inactive.");
			AssertHasWarning(exception.ExceptionTypeCodeInfo, "Exception type is inactive.");

			exception.ExceptionTypeCode = "TYX";
			milestone.ExceptionTypeCode = "TYX";

			AssertHasError(milestone.ExceptionTypeCodeInfo, "Exception type is invalid.");
			AssertHasError(exception.ExceptionTypeCodeInfo, "Exception type is invalid.");
		}

		public void TestExceptionTypeCode_ShouldBeRequired_WhenRegistryRequireType()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertEquals(ZString.Empty, exception.ExceptionTypeCode);
			AssertNoErrors(exception.ExceptionTypeCodeInfo);

			WorkflowDataRegistry.Instance.ExceptionRequireType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			exception.Validation.ValidateExceptionTypeCode();

			AssertEquals(ZString.Empty, exception.ExceptionTypeCode);
			AssertHasError(exception.ExceptionTypeCodeInfo, "Please enter a type.");
		}

		public void TestExceptionCausePK()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TYP";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);
			exception.ExceptionTypeCode = "TYP";

			var processWorkflowExceptionTypeCauseRequired = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionTypeCauseRequired.WET_Code = "TYC";
			processWorkflowExceptionTypeCauseRequired.WET_Description = nameof(processWorkflowExceptionType);
			processWorkflowExceptionTypeCauseRequired.WET_IsCauseRequired = true;

			var cause1 = processWorkflowExceptionType.Causes.AddNew();
			cause1.WEC_Code = "CA1";
			cause1.WEC_Description = nameof(cause1);
			var cause2 = processWorkflowExceptionType.Causes.AddNew();
			cause2.WEC_Code = "CA2";
			cause2.WEC_Description = nameof(cause2);
			cause2.WEC_IsActive = false;

			Factory.Save();

			AssertNoErrors(exception.ExceptionCausePKInfo);

			exception.ExceptionCausePK = cause1.PK;
			AssertNoErrors(exception.ExceptionCausePKInfo);

			exception.ExceptionCausePK = ZGuid.Empty;
			AssertNoErrors(exception.ExceptionCausePKInfo);

			exception.ExceptionTypeCode = processWorkflowExceptionTypeCauseRequired.WET_Code;
			AssertNoErrors("Only actioned exceptions requires cause", exception.ExceptionCausePKInfo);

			exception.IsExceptionActioned = true;
			exception.Validation.ValidateExceptionCausePK();
			AssertHasError(exception.ExceptionCausePKInfo, "Please enter a cause.");
		}

		public void TestExceptionCausePK_NoExceptionThrow_WhenProcessWorkflowExceptionIsDeleted()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TYP";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);
			exception.ExceptionTypeCode = "TYP";

			var processWorkflowExceptionTypeCauseRequired = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionTypeCauseRequired.WET_Code = "TYC";
			processWorkflowExceptionTypeCauseRequired.WET_Description = nameof(processWorkflowExceptionType);
			processWorkflowExceptionTypeCauseRequired.WET_IsCauseRequired = true;

			var cause1 = processWorkflowExceptionType.Causes.AddNew();
			cause1.WEC_Code = "CA1";
			cause1.WEC_Description = nameof(cause1);
			var cause2 = processWorkflowExceptionType.Causes.AddNew();
			cause2.WEC_Code = "CA2";
			cause2.WEC_Description = nameof(cause2);
			cause2.WEC_IsActive = false;

			Factory.Save();
			exception.ProcessWorkflowException.Delete();
			AssertEquals(false, exception.ProcessWorkflowException.IsInDatabase);
			AssertNoExceptionThrown(() => exception.Validation.ValidateExceptionCausePK());
		}

		public void TestExceptionCausePK_WhenIsNotActive_ShouldShowWarningIfOnDatabase()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TYP";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);
			exception.ExceptionTypeCode = "TYP";

			var processWorkflowExceptionTypeCauseRequired = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionTypeCauseRequired.WET_Code = "TYC";
			processWorkflowExceptionTypeCauseRequired.WET_Description = nameof(processWorkflowExceptionType);

			var cause = processWorkflowExceptionType.Causes.AddNew();
			cause.WEC_Code = "CA1";
			cause.WEC_Description = nameof(cause);
			cause.WEC_IsActive = false;

			AssertNoErrors(exception.ExceptionCausePKInfo);

			exception.ExceptionCausePK = cause.PK;

			AssertHasError(exception.ExceptionCausePKInfo, "Cause is not active");

			Factory.Save();

			exception.Validation.ValidateExceptionCausePK();
			AssertHasWarning(exception.ExceptionCausePKInfo, "Cause is not active");
		}

		public void TestExceptionResolutionPK()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TYP";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);
			exception.ExceptionTypeCode = "TYP";

			var processWorkflowExceptionTypeResolutionRequired = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionTypeResolutionRequired.WET_Code = "TYC";
			processWorkflowExceptionTypeResolutionRequired.WET_Description = nameof(processWorkflowExceptionType);
			processWorkflowExceptionTypeResolutionRequired.WET_IsResolutionRequired = true;

			var resolution1 = processWorkflowExceptionType.Resolutions.AddNew();
			resolution1.WER_Code = "RE1";
			resolution1.WER_Description = nameof(resolution1);
			var resolution2 = processWorkflowExceptionType.Resolutions.AddNew();
			resolution2.WER_Code = "RE2";
			resolution2.WER_Description = nameof(resolution2);
			resolution2.WER_IsActive = false;

			Factory.Save();

			AssertNoErrors(exception.ExceptionResolutionPKInfo);

			exception.ExceptionResolutionPK = resolution1.PK;
			AssertNoErrors(exception.ExceptionResolutionPKInfo);

			exception.ExceptionResolutionPK = Guid.NewGuid();
			AssertHasErrors(exception.ExceptionResolutionPKInfo);

			exception.ExceptionResolutionPK = ZGuid.Empty;
			AssertNoErrors(exception.ExceptionResolutionPKInfo);

			exception.ExceptionTypeCode = processWorkflowExceptionTypeResolutionRequired.WET_Code;
			AssertNoErrors("Only actioned exceptions requires resolution", exception.ExceptionCausePKInfo);

			exception.IsExceptionActioned = true;
			exception.Validation.ValidateExceptionResolutionPK();
			AssertHasError(exception.ExceptionResolutionPKInfo, "Please enter a resolution.");
		}

		public void TestExceptionResolutionPK_NoExceptionThrow_WhenProcessWorkflowExceptionIsDeleted()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TYP";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);
			exception.ExceptionTypeCode = "TYP";

			var processWorkflowExceptionTypeResolutionRequired = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionTypeResolutionRequired.WET_Code = "TYC";
			processWorkflowExceptionTypeResolutionRequired.WET_Description = nameof(processWorkflowExceptionType);
			processWorkflowExceptionTypeResolutionRequired.WET_IsResolutionRequired = true;

			var resolution1 = processWorkflowExceptionType.Resolutions.AddNew();
			resolution1.WER_Code = "RE1";
			resolution1.WER_Description = nameof(resolution1);
			var resolution2 = processWorkflowExceptionType.Resolutions.AddNew();
			resolution2.WER_Code = "RE2";
			resolution2.WER_Description = nameof(resolution2);
			resolution2.WER_IsActive = false;

			Factory.Save();
			exception.ProcessWorkflowException.Delete();
			AssertEquals(false, exception.ProcessWorkflowException.IsInDatabase);
			AssertNoExceptionThrown(() => exception.Validation.ValidateExceptionResolutionPK());
		}

		public void TestExceptionResolutionPK_WhenIsNotActive_ShouldShowWarningIfOnDatabase()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TYP";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);
			exception.ExceptionTypeCode = "TYP";

			var processWorkflowExceptionTypeCauseRequired = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionTypeCauseRequired.WET_Code = "TYC";
			processWorkflowExceptionTypeCauseRequired.WET_Description = nameof(processWorkflowExceptionType);

			var resolution = processWorkflowExceptionType.Resolutions.AddNew();
			resolution.WER_Code = "CA1";
			resolution.WER_Description = nameof(resolution);
			resolution.WER_IsActive = false;

			AssertNoErrors(exception.ExceptionResolutionPKInfo);

			exception.ExceptionResolutionPK = resolution.PK;

			AssertHasError(exception.ExceptionResolutionPKInfo, "Resolution is not active");

			Factory.Save();

			exception.Validation.ValidateExceptionResolutionPK();
			AssertHasWarning(exception.ExceptionResolutionPKInfo, "Resolution is not active");
		}

		public void TestActualDateAndExceptionEndDate()
		{
			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "WW1";
			exceptionType.WET_Description = nameof(exceptionType);
			
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();
			exception.ExceptionTypeCode = exceptionType.WET_Code;
			AssertEquals("Precondition", ZDateTime.Empty, exception.P9_ActualDate);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, exception.P9_ExceptionEndDate);
			AssertNoErrors(exception.P9_ActualDateInfo);
			AssertNoErrors(exception.P9_ExceptionEndDateInfo);

			exception.P9_ActualDateForBinding = ZDateTimeOffset.Now.AddDays(1);
			AssertNoErrors(exception.P9_ActualDateInfo);
			AssertNoErrors(exception.P9_ExceptionEndDateInfo);

			exception.P9_ExceptionEndDate = ZDateTimeOffset.Now.AddDays(1);
			AssertNoErrors(exception.P9_ActualDateForBindingInfo);
			AssertNoErrors(exception.P9_ExceptionEndDateInfo);

			exception.P9_ActualDateForBinding = ZDateTimeOffset.Now.AddDays(1).AddMinutes(1);
			exception.P9_ExceptionEndDate = ZDateTimeOffset.Now.AddDays(1);
			AssertHasError(exception.P9_ExceptionEndDateInfo, "Exception End must be after the Exception Time");

			exception.P9_ActualDateForBinding = exception.P9_ActualDateForBinding.AddSeconds(1);
			AssertHasError(exception.P9_ActualDateForBindingInfo, "Exception End must be after the Exception Time");
		}

		public void TestExceptionDurationHours()
		{
			var exception = Factory.New<ProcessTask>();
			exception.P9_ExceptionDurationHours = -10;
			AssertHasError(exception.P9_ExceptionDurationHoursInfo, "value cannot be negative.");
		}

		#endregion

		#region Implementation

		static void SetWorkingStatusChange(ZString workflowType, ZString value)
		{
			var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var workflowTaskTypes = categorisedTaskTypes.GetTaskTypesFromWorkflowCode(workflowType);

			foreach (WorkflowTaskType taskType in workflowTaskTypes)
			{
				taskType.WorkingStatusChangeType = value;
			}

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
		}

		static void SetWorkingStatusChange(ZString workflowType, ZString value, string taskTypeChoice = "UDF")
		{
			var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var workflowTaskTypes = categorisedTaskTypes.GetTaskTypesFromWorkflowCode(workflowType);
			var taskType = workflowTaskTypes.Cast<WorkflowTaskType>().ToArray().FirstOrDefault(w => w.Code == taskTypeChoice);

			if (taskType == null)
			{
				taskType = workflowTaskTypes.AddNew();
				taskType.Code = taskTypeChoice;
			}

			taskType.WorkingStatusChangeType = value;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
		}

		protected ProcessTask ProcessTask
		{
			get
			{
				if (processTask == null)
				{
					processTask = NewProcessTask();
				}
				return processTask;
			}
		}
		ProcessTask processTask;

		protected virtual ProcessTask NewProcessTask()
		{
			return Factory.New<ProcessTask>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
		}

		internal IBMTestHelper BMSTestHelper
		{
			get { return ObjectFactory.Get<IBMTestHelper>(); }
		}

		#region Classes_ForTest

		class ProcessTaskValidation_ForTest : ProcessTaskValidation
		{
			public ProcessTaskValidation_ForTest(ProcessTask parent)
				: base(parent)
			{
			}

			public void CheckTaskGroupHasMembers_Exposed()
			{
				CheckTaskGroupHasMembers();
			}

			public void CheckCapabilityHasMembers_Exposed()
			{
				CheckCapabilityHasMembers();
			}

			public void CheckCapabilityAllowsTaskToBeAssignedToResource_Exposed()
			{
				CheckCapabilityAllowsTaskToBeAssignedToResource();
			}

			public bool ShouldValidateFKToCancelledRecord_Exposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#endregion
	}

	class ProcessTaskValidationTestForIterationProperty : TestCaseWithFactory
	{
		#region Iteration ListValidation

		public void TestValidateIteration_List()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask = workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask = workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask, containmentBarrierIterationTask, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			AssertEquals(string.Empty, task1.Iteration);
			task1.Validation.ValidateIteration();
			AssertNoNotifications(task1.IterationInfo);

			task1.Iteration = "1";
			AssertNoNotifications(task1.IterationInfo);

			task1.Iteration = "3";
			AssertHasError(task1.IterationInfo, "Enter a valid Quality Iteration.");

			task1.Iteration = "2";
			AssertNoNotifications(task1.IterationInfo);

			task1.Iteration = "ABC";
			AssertHasError(task1.IterationInfo, "Enter a valid Quality Iteration.");

			task1.Iteration = ZString.Empty;
			AssertNoNotifications(task1.IterationInfo);
		}

		public void TestValidateIteration_ForTaskWithoutWorkflow_ShouldAlwaysHaveErrorUnlessEmpty()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			var taskWithoutWorkflow = ((IWorkflowProvider)jobLevelWorkflow.Parent).WorkflowItems.Tasks.AddNew();
			taskWithoutWorkflow.Validation.ValidateIteration();
			AssertNoNotifications(taskWithoutWorkflow.IterationInfo);

			taskWithoutWorkflow.Iteration = "1"; // an iteration that does exist, but the task is not in that workflow.
			AssertHasError(taskWithoutWorkflow.IterationInfo, "Enter a valid Quality Iteration.");

			taskWithoutWorkflow.Iteration = ZString.Empty;
			AssertNoNotifications(taskWithoutWorkflow.IterationInfo);
		}

		public void TestValidateIteration_WhenIterationFromDifferentWorkflowSelected_ShouldHaveError()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow 2");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");
			var task3 = (ProcessTask)BMSTestHelper.CreateTask(workflow2);

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			task1.Iteration = "1";
			AssertNoNotifications(task1.IterationInfo);

			task3.Iteration = "1";
			AssertHasError("The selected iteration exists but is in a different workflow than the task's workflow, so it's invalid.", task3.IterationInfo, "Enter a valid Quality Iteration.");

			task3.Iteration = ZString.Empty;
			AssertNoNotifications(task3.IterationInfo);
		}

		#endregion

		#region Iteration Auto Association Warnings

		public void TestValidateIteration_WhenNewTaskAddedToWorkflowWithoutIterations_ShouldHaveNoNotifications()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);

			AssertEquals(ZString.Empty, task1.Iteration);
			AssertNoNotifications(task1.IterationInfo);

			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow);

			AssertEquals(ZString.Empty, task1.Iteration);
			AssertNoNotifications(task1.IterationInfo);
			AssertEquals(ZString.Empty, task2.Iteration);
			AssertNoNotifications(task2.IterationInfo);
		}

		public void TestValidateIteration_WhenNewTaskAddedToWorkflowWithIterations_AndNoIterationAssigned_ShouldHaveWarning()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1" }, workflow.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			var task3 = (ProcessTask)BMSTestHelper.CreateTask(workflow);

			AssertEquals(ZString.Empty, task3.Iteration);
			AssertHasWarning(task3.IterationInfo, "This task's workflow includes at least one quality iteration. Please ensure that this task is assigned to the correct quality iteration if applicable.");
			AssertNoNotifications(task1.IterationInfo);
			AssertNoNotifications(task2.IterationInfo);

			task3.Iteration = "1";
			AssertNoNotifications("Setting a quality iteration on a task with an iteration-related warning should clear that warning.", task3.IterationInfo);
			AssertNoNotifications(task1.IterationInfo);
			AssertNoNotifications(task2.IterationInfo);

			task3.Iteration = ZString.Empty;
			AssertNoNotifications("Manually resetting the iteration to the state that had the warning should not re-apply the warning because these warnings are about the automatic application (or not) of iterations.", task3.IterationInfo);
			AssertNoNotifications(task1.IterationInfo);
			AssertNoNotifications(task2.IterationInfo);
		}

		public void TestValidateIteration_WhenNewTaskAddedToWorkflowWithIterations_AndIterationIsAutoAssigned_ShouldHaveWarning()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			var iterationWorkflow = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			AssertContainsExactElementsInAnyOrder(new[] { "1", "1" }, iterationWorkflow.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			var task3 = (ProcessTask)BMSTestHelper.CreateTask(iterationWorkflow);

			AssertEquals("1", task3.Iteration);
			AssertHasWarning(task3.IterationInfo, "This task's workflow includes at least one quality iteration, and the system has selected an iteration for this task based on the tasks around it. Please ensure that this task is assigned to the correct quality iteration if applicable.");
			AssertNoNotifications(task1.IterationInfo);
			AssertNoNotifications(task2.IterationInfo);

			task3.Iteration = ZString.Empty;
			AssertNoNotifications("Setting a quality iteration on a task with an iteration-related warning should clear that warning.", task3.IterationInfo);
			AssertNoNotifications(task1.IterationInfo);
			AssertNoNotifications(task2.IterationInfo);

			task3.Iteration = "1";
			AssertNoNotifications("Manually resetting the iteration to the state that had the warning should not re-apply the warning because these warnings are about the automatic application (or not) of iterations.", task3.IterationInfo);
			AssertNoNotifications(task1.IterationInfo);
			AssertNoNotifications(task2.IterationInfo);
		}

		public void TestValidateIteration_WhenTaskMovedToWorkflowWithoutIterations_ShouldHaveNoNotifications()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow 2");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");
			var task3 = (ProcessTask)BMSTestHelper.CreateTask(workflow2);

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			task1.Iteration = "1";
			task1.P9_FH_ProcessHeader = workflow2.PK;

			AssertEquals("Moving the task to a different workflow should clear its iteration, and no iteration should be assigned because there aren't any iterations for the destination workflow.", ZString.Empty, task1.Iteration);
			AssertNoNotifications(task1.IterationInfo);
		}

		public void TestValidateIteration_WhenTaskMovedToWorkflowWithIterations_AndNoIterationAssigned_ShouldHaveWarning()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow 2");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");
			var task3 = (ProcessTask)BMSTestHelper.CreateTask(workflow2, sequence: 10); // to ensure it's the highest sequence when it's moved.

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			task3.P9_FH_ProcessHeader = workflow1.PK;

			AssertEquals("Moving the task to a different workflow should clear its iteration, and no iteration should be assigned because no single iteration can be determined to be correct.", ZString.Empty, task3.Iteration);
			AssertHasWarning(task3.IterationInfo, "This task's workflow includes at least one quality iteration. Please ensure that this task is assigned to the correct quality iteration if applicable.");
		}

		public void TestValidateIteration_WhenTaskMovedToWorkflowWithIterations_AndIterationIsAutoAssigned_ShouldHaveWarning()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			var iterationWorkflow = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			AssertContainsExactElementsInAnyOrder(new[] { "1", "1" }, iterationWorkflow.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));
			AssertEquals(ZString.Empty, task1.Iteration);

			task1.P9_FH_ProcessHeader = iterationWorkflow.PK;
			AssertEquals("The iteration should be auto-assigned on the moved task.", "1", task1.Iteration);
			AssertHasWarning(task1.IterationInfo, "This task's workflow includes at least one quality iteration, and the system has selected an iteration for this task based on the tasks around it. Please ensure that this task is assigned to the correct quality iteration if applicable.");
		}

		public void TestValidateIteration_WhenTaskSequenceChanged_AndWorkflowHasIterations_AndIterationNotAssigned_ShouldHaveWarning()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1" }, workflow.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			task1.P9_Sequence = 10;
			AssertEquals("The new sequence is the highest in the workflow, so the iteration should not have been changed.", ZString.Empty, task1.Iteration);
			AssertHasWarning(task1.IterationInfo, "This task's workflow includes at least one quality iteration. Please ensure that this task is assigned to the correct quality iteration if applicable.");
		}

		public void TestValidateIteration_WhenTaskSequenceChanged_AndIterationIsAutoAssigned_ShouldHaveWarning()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1" }, workflow.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3, 4 }, workflow.Tasks.Select(x => (int)x.P9_Sequence));

			task1.P9_Sequence = 3;
			AssertEquals("The new sequence matches a task with an iteration, so it should get that iteration as well.", "1", task1.Iteration);
			AssertHasWarning(task1.IterationInfo, "This task's workflow includes at least one quality iteration, and the system has selected an iteration for this task based on the tasks around it. Please ensure that this task is assigned to the correct quality iteration if applicable.");
		}

		public void TestValidateIteration_WhenTaskSequenceChanged_AndTaskAlreadyHadIteration_ShouldHaveNoNotifications()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask, containmentBarrierIterationTask, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1", "2", "2" }, workflow.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3, 4, 5, 6 }, workflow.Tasks.Select(x => (int)x.P9_Sequence));

			iterationTask.P9_Sequence = 5;
			AssertEquals("The new sequence matches a task with an iteration, but the re-sequenced task already has an iteration, so its associated iteration shouldn't change.", "1", iterationTask.Iteration);
			AssertNoNotifications("The iteration didn't change so we shouldn't show a notification when changing sequence.", iterationTask.IterationInfo);
		}

		public void TestCreateIteration_IterationTasksShouldNotHaveNotifications_NewWorkflowCreated()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task3 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			var iterationWorkflow = BMSTestHelper.CreateQualityIteration(task1, task3, "Quality Iteration");
			var iterationTask = (ProcessTask)iterationWorkflow.Tasks.Single(x => x.P9_Sequence == 4);
			var clonedTask = (ProcessTask)iterationWorkflow.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask = (ProcessTask)iterationWorkflow.Tasks.Single(x => x.P9_Sequence == 6);

			AssertNoNotifications("Tasks created when creating an iteration should not have any warnings.", iterationTask.IterationInfo);
			AssertNoNotifications("Tasks created when creating an iteration should not have any warnings.", clonedTask.IterationInfo);
			AssertNoNotifications("Tasks created when creating an iteration should not have any warnings.", containmentBarrierIterationTask.IterationInfo);
		}

		public void TestCreateIteration_IterationTasksShouldNotHaveNotifications_SameWorkflowReused()
		{
			var jobLevelWorkflow = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task3 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task3, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);
			var clonedTask = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 6);

			AssertNoNotifications("Tasks created when creating an iteration should not have any warnings.", iterationTask.IterationInfo);
			AssertNoNotifications("Tasks created when creating an iteration should not have any warnings.", clonedTask.IterationInfo);
			AssertNoNotifications("Tasks created when creating an iteration should not have any warnings.", containmentBarrierIterationTask.IterationInfo);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");
		}

		internal IBMTestHelper BMSTestHelper => bmsTestHelper.Value;

		readonly Lazy<IBMTestHelper> bmsTestHelper = new Lazy<IBMTestHelper>(ObjectFactory.Get<IBMTestHelper>);

		#endregion
	}
}
