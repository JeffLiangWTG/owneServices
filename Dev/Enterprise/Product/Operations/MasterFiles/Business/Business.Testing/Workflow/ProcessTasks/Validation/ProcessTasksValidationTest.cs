using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTasksValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckP9_ActualDate()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var task = TemplateProcessTask;
			task.P9_Type = Core.Constants.Workflow.MilestoneType;

			Assert("TemplateProcessTask is a milestone task", task.IsMilestone);
			Assert("Prevent Milestone Future Actual Start registry is set", WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.Value);
			Assert("TemplateProcessTask is new, not in the database yet", !task.IsInDatabase);

			((IProcessTaskInternals)task).SetActualDateWithoutFiringWorkflow(ZDateTimeOffset.Empty);
			Assert("P9_ActualDate is NOT in the future, it's empty", !((ZDateTime)task.P9_ActualDateInfo.Value).IsInTheFuture());
			Assert("P9_ActualDate is NOT in the past, it's empty", !((ZDateTime)task.P9_ActualDateInfo.Value).IsInThePast());

			task.Validation.ValidateAll();
			AssertNoErrors("No error when P9_ActualDate is in the past", task.P9_ActualDateInfo);

			((IProcessTaskInternals)task).SetActualDateWithoutFiringWorkflow(ZDateTimeOffset.Now);
			Assert("P9_ActualDate is NOT in the future", !((ZDateTime)task.P9_ActualDateInfo.Value).IsInTheFuture());

			task.Validation.ValidateAll();
			AssertNoErrors("No error when P9_ActualDate is in the past", task.P9_ActualDateInfo);
		}

		public void TestCheckP9_ActualDateUtc()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var task = TemplateProcessTask;
			task.P9_Type = Core.Constants.Workflow.MilestoneType;

			Assert("TemplateProcessTask is a milestone task", task.IsMilestone);
			Assert("Prevent Milestone Future Actual Start registry is set", WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.Value);
			Assert("TemplateProcessTask is new, not in the database yet", !task.IsInDatabase);

			task.P9_ActualDateUtc = ZDateTime.Empty;
			Assert("P9_ActualDateUtc is NOT in the future, it's empty", !((ZDateTime)task.P9_ActualDateUtcInfo.Value).IsInTheFutureUtc());
			Assert("P9_ActualDateUtc is NOT in the past, it's empty", !((ZDateTime)task.P9_ActualDateUtcInfo.Value).IsInThePastUtc());

			task.Validation.ValidateAll();
			AssertNoErrors("No error when P9_ActualDateUtc is in the past", task.P9_ActualDateUtcInfo);

			task.P9_ActualDateUtc = ZDateTime.UtcNow;
			Assert("P9_ActualDateUtc is NOT in the future", !((ZDateTime)task.P9_ActualDateUtcInfo.Value).IsInTheFutureUtc());

			task.Validation.ValidateAll();
			AssertNoErrors("No error when P9_ActualDateUtc is in the past", task.P9_ActualDateUtcInfo);
		}

		public void TestIsCalendarItem()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "RAK";

			ProcessTask.P9_ScheduledDate = ZDateTime.Today;

			ProcessTask.P9_IsCalendarItem = true;
			AssertHasWarnings(ProcessTask.P9_IsCalendarItemInfo);

			ProcessTask.P9_IsCalendarItem = false;
			AssertNoWarnings(ProcessTask.P9_IsCalendarItemInfo);

			ProcessTask.P9_IsCalendarItem = false;
			ProcessTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ProcessTask.P9_IsCalendarItem = true;
			AssertHasWarnings(ProcessTask.P9_IsCalendarItemInfo);

			ProcessTask.P9_IsCalendarItem = false;
			staff.GS_EmailAddress = "test@test.com";
			ProcessTask.P9_IsCalendarItem = true;
			AssertNoWarnings(ProcessTask.P9_IsCalendarItemInfo);

			ProcessTask.P9_IsCalendarItem = false;
			ProcessTask.P9_ScheduledDate = ZDateTime.Empty;
			ProcessTask.P9_IsCalendarItem = true;
			AssertHasWarnings(ProcessTask.P9_IsCalendarItemInfo);

			ProcessTask.P9_IsCalendarItem = false;
			ProcessTask.P9_ScheduledDate = ZDateTime.Today;
			ProcessTask.P9_IsCalendarItem = true;
			AssertNoWarnings(ProcessTask.P9_IsCalendarItemInfo);
		}

		public void TestP9_IsSequence()
		{
			AssertNoErrors(ProcessTask.P9_SequenceInfo);
			ProcessTask.P9_Sequence = -1;
			AssertHasErrors(ProcessTask.P9_SequenceInfo);
			ProcessTask.P9_Sequence = 1;
			AssertNoErrors(ProcessTask.P9_SequenceInfo);
		}

		public void TestP9_RN_NKOriginCountry()
		{
			ProcessTask.P9_RN_NKOriginCountry = "AU";
			AssertNoErrors(ProcessTask.P9_RN_NKOriginCountryInfo);

			ProcessTask.P9_RN_NKOriginCountry = "RZ";
			AssertHasErrors(ProcessTask.P9_RN_NKOriginCountryInfo);

			ProcessTask.P9_RN_NKOriginCountry = "";
			AssertNoErrors(ProcessTask.P9_RN_NKOriginCountryInfo);
		}

		public void TestP9_RN_NKDestinationCountry()
		{
			ProcessTask.P9_RN_NKDestinationCountry = "AU";
			AssertNoErrors(ProcessTask.P9_RN_NKDestinationCountryInfo);

			ProcessTask.P9_RN_NKDestinationCountry = "RZ";
			AssertHasErrors(ProcessTask.P9_RN_NKDestinationCountryInfo);

			ProcessTask.P9_RN_NKDestinationCountry = "";
			AssertNoErrors(ProcessTask.P9_RN_NKDestinationCountryInfo);
		}

		public void TestP9_EstDuration_NoRangeValidation()
		{
			ProcessTask.P9_EstDuration = new ZDateTime(2001, 1, 1, 8, 13, 26);
			AssertNoErrors("Estimated Duration should not have range validation", ProcessTask.P9_EstDurationInfo);
		}

		public void TestP9_ActualDuration_NoRangeValidation()
		{
			ProcessTask.P9_ActualDuration = new ZDateTime(2001, 1, 1, 8, 13, 26);
			AssertNoErrors("Actual Duration should not have range validation", ProcessTask.P9_ActualDurationInfo);
		}

		public void TestP9_EstimatedDefaultedFrom()
		{
			ProcessTask dummyTask = Dummy.WorkflowItems.AddNew();
			DummyWorkflowDescriptor.Instance.EstimateDefaultedFromList.Add(new CodeDescriptionPair("VLD", "Valid Item Description"));
			dummyTask.P9_EstimatedDefaultedFrom = "";
			AssertNoErrors("No error when default from not specified", dummyTask.P9_EstimatedDefaultedFromInfo);
			dummyTask.P9_EstimatedDefaultedFrom = "XXX";
			AssertHasErrors("Error when default from not valid", dummyTask.P9_EstimatedDefaultedFromInfo);
			dummyTask.P9_EstimatedDefaultedFrom = "VLD";
			AssertNoErrors("No error when valid item is selected", dummyTask.P9_EstimatedDefaultedFromInfo);
			dummyTask.P9_EstimatedDefaultedFrom = ProcessTask.P9_EstimatedDefaultedFrom_Actual;
			AssertNoErrors("No error when 'ARV' value is selected (despite it is not in list) because this is system value not meant to be shown to user", dummyTask.P9_EstimatedDefaultedFromInfo);
		}

		public void TestP9_EstimatedDefaultTimeDelta()
		{
			ProcessTask.P9_EstimatedDefaultedFrom = "FAV";
			ProcessTask.Validation.ValidateP9_EstimatedDefaultTimeDelta();
			AssertHasErrors(ProcessTask.P9_EstimatedDefaultTimeDeltaInfo);

			ProcessTask.P9_EstimatedDefaultedFrom = "";
			ProcessTask.P9_EstimatedDefaultFromPredecessor = 2;
			ProcessTask.Validation.ValidateP9_EstimatedDefaultTimeDelta();
			AssertHasErrors(ProcessTask.P9_EstimatedDefaultTimeDeltaInfo);

			ProcessTask.P9_EstimatedDefaultedFrom = "";
			ProcessTask.P9_EstimatedDefaultFromPredecessor = 0;
			ProcessTask.Validation.ValidateP9_EstimatedDefaultTimeDelta();
			AssertNoErrors("No errors when estimate not defaulted", ProcessTask.P9_EstimatedDefaultTimeDeltaInfo);

			ProcessTask.P9_EstimatedDefaultedFrom = "FAV";
			ProcessTask.P9_EstimatedDefaultTimeDelta = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			ProcessTask.Validation.ValidateP9_EstimatedDefaultTimeDelta();
			AssertNoErrors("No errors when populated", ProcessTask.P9_EstimatedDefaultTimeDeltaInfo);
		}

		public void TestP9_Description()
		{
			ProcessTask.P9_Description = "Hello";
			AssertNoErrors(ProcessTask.P9_DescriptionInfo);

			ProcessTask.P9_Description = "";
			AssertHasErrors(ProcessTask.P9_DescriptionInfo);
		}

		public void TestP9_Status()
		{
			ProcessTask.P9_Status = ProcessTask.Lookups.Statuses[0].Code;
			AssertNoErrors(ProcessTask.P9_StatusInfo);

			ProcessTask.P9_Status = "ZUB";
			AssertHasErrors(ProcessTask.P9_StatusInfo);

			ProcessTask.P9_Status = "";
			AssertHasErrors(ProcessTask.P9_StatusInfo);
		}

		public void TestP9_Status_Error_WhenExceptionActioned()
		{
			WorkflowDataRegistry.Instance.ExceptionAutoAssignStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkflowDataRegistry.Instance.ExceptionRequireStaffGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();
			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TYP";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);
			processWorkflowExceptionType.WET_IsCauseRequired = true;
			processWorkflowExceptionType.WET_IsResolutionRequired = true;
			exception.ExceptionTypeCode = "TYP";

			AssertNoErrors(exception.P9_StatusInfo);
			AssertNoErrors(exception.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(exception.P9_GG_AssignedGroupInfo);
			AssertNoErrors(exception.ExceptionCausePKInfo);
			AssertNoErrors(exception.ExceptionResolutionPKInfo);

			exception.IsExceptionActioned = true;

			Assert(exception.HasChanges);
			AssertHasError(exception.P9_StatusInfo, "For actioned exceptions, you must specify either an assigned resource or group.");
			AssertHasError(exception.P9_GS_NKAssignedStaffMemberInfo, "For actioned exceptions, you must specify either an assigned resource or group.");
			AssertHasError(exception.P9_GG_AssignedGroupInfo, "For actioned exceptions, you must specify either an assigned resource or group.");
			AssertHasError(exception.ExceptionCausePKInfo, "Please enter a cause.");
			AssertHasError(exception.ExceptionResolutionPKInfo, "Please enter a resolution.");

			Factory.Save();

			exception.Validation.ValidateAll();
			Assert("Should not trigger validation if no changes in the exception", !exception.HasChanges);
			AssertNoErrors(exception.P9_StatusInfo);
			AssertNoErrors(exception.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(exception.P9_GG_AssignedGroupInfo);
			AssertHasError(exception.ExceptionCausePKInfo, "Please enter a cause.");
			AssertHasError(exception.ExceptionResolutionPKInfo, "Please enter a resolution.");

			exception.IsExceptionActioned = false;

			Assert(exception.HasChanges);
			AssertNoErrors(exception.P9_StatusInfo);
			AssertNoErrors(exception.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(exception.P9_GG_AssignedGroupInfo);
			AssertNoErrors(exception.ExceptionCausePKInfo);
			AssertNoErrors(exception.ExceptionResolutionPKInfo);

			Factory.Save();

			exception.IsExceptionActioned = true;

			exception.Validation.ValidateAll();
			Assert(exception.HasChanges);
			AssertHasError(exception.P9_StatusInfo, "For actioned exceptions, you must specify either an assigned resource or group.");
			AssertHasError(exception.P9_GS_NKAssignedStaffMemberInfo, "For actioned exceptions, you must specify either an assigned resource or group.");
			AssertHasError(exception.P9_GG_AssignedGroupInfo, "For actioned exceptions, you must specify either an assigned resource or group.");
			AssertHasError(exception.ExceptionCausePKInfo, "Please enter a cause.");
			AssertHasError(exception.ExceptionResolutionPKInfo, "Please enter a resolution.");
		}

		public void TestP9_Type()
		{
			ProcessTask.P9_Type = "XXX";
			ProcessTask.P9_Type = "";
			AssertHasErrors("P9_Type is mandatory", ProcessTask.P9_TypeInfo);

			ProcessTask.P9_Type = Core.Constants.Workflow.MilestoneType;
			AssertNoErrors("No error when P9_Type has data", ProcessTask.P9_TypeInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember()
		{
			ProcessTask.P9_GS_NKAssignedStaffMember = "ABC";
			AssertHasErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);

			ProcessTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			AssertNoErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			ProcessTask.P9_GS_NKAssignedStaffMember = "";
			AssertNoErrors(ProcessTask.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_GS_NKAssignedStaffMember_Error_WhenExceptionActioned()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();
			WorkflowDataRegistry.Instance.ExceptionRequireStaffGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkflowDataRegistry.Instance.ExceptionAutoAssignStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertNoErrors(exception.P9_StatusInfo);
			AssertNoErrors(exception.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(exception.P9_GG_AssignedGroupInfo);

			exception.IsExceptionActioned = true;

			AssertNoErrors(exception.P9_StatusInfo);
			AssertNoErrors(exception.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(exception.P9_GG_AssignedGroupInfo);

			WorkflowDataRegistry.Instance.ExceptionRequireStaffGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			exception.Validation.ValidateAll();

			AssertHasError(exception.P9_StatusInfo, "For actioned exceptions, you must specify either an assigned resource or group.");
			AssertHasError(exception.P9_GS_NKAssignedStaffMemberInfo, "For actioned exceptions, you must specify either an assigned resource or group.");
			AssertHasError(exception.P9_GG_AssignedGroupInfo, "For actioned exceptions, you must specify either an assigned resource or group.");

			exception.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;

			AssertNoErrors(exception.P9_StatusInfo);
			AssertNoErrors(exception.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(exception.P9_GG_AssignedGroupInfo);
		}

		public void TestP9_GG_AssignedGroup()
		{
			ProcessTask.P9_GG_AssignedGroup = ZGuid.NewZGuid();
			AssertHasErrors(ProcessTask.P9_GG_AssignedGroupInfo);

			ProcessTask.P9_GG_AssignedGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL")).PK;
			AssertNoErrors(ProcessTask.P9_GG_AssignedGroupInfo);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			ProcessTask.P9_GG_AssignedGroup = ZGuid.Empty;
			AssertNoErrors(ProcessTask.P9_GG_AssignedGroupInfo);
		}

		public void TestP9_GG_AssignedGroup_Error_WhenExceptionActioned()
		{
			var group = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL"));
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			WorkflowDataRegistry.Instance.ExceptionRequireStaffGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkflowDataRegistry.Instance.ExceptionAutoAssignStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertNoErrors(exception.P9_StatusInfo);
			AssertNoErrors(exception.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(exception.P9_GG_AssignedGroupInfo);

			exception.IsExceptionActioned = true;

			AssertNoErrors(exception.P9_StatusInfo);
			AssertNoErrors(exception.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(exception.P9_GG_AssignedGroupInfo);

			WorkflowDataRegistry.Instance.ExceptionRequireStaffGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			exception.Validation.ValidateAll();

			AssertHasError(exception.P9_StatusInfo, "For actioned exceptions, you must specify either an assigned resource or group.");
			AssertHasError(exception.P9_GS_NKAssignedStaffMemberInfo, "For actioned exceptions, you must specify either an assigned resource or group.");
			AssertHasError(exception.P9_GG_AssignedGroupInfo, "For actioned exceptions, you must specify either an assigned resource or group.");

			exception.P9_GG_AssignedGroup = group.PK;

			AssertNoErrors(exception.P9_StatusInfo);
			AssertNoErrors(exception.P9_GS_NKAssignedStaffMemberInfo);
			AssertNoErrors(exception.P9_GG_AssignedGroupInfo);
		}

		public void TestP9_SE_NKMilestoneEvent()
		{
			ProcessTask.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			AssertNoErrors("A valid milestone event type has no error", ProcessTask.TriggerConditions.TriggerEventCodeInfo);

			ProcessTask.TriggerConditions.TriggerEventCode = ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily;
			AssertHasErrors("An exception event type doesnt belong in P9_SE_NKMilestoneEvent", ProcessTask.TriggerConditions.TriggerEventCodeInfo);
		}

		public void TestIsMilestoneForTemplate_MilestonesOnlyAvailableOnSupportingModules()
		{
			ProcessTaskTemplate.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;
			TemplateProcessTask.IsMilestone = true;
			AssertEquals("Milestone for process type that supports tracking", false, TemplateProcessTask.IsMilestoneInfo.HasErrors());
		}

		public void TestValidateP9_TriggerCondition()
		{
			ProcessTask.TriggerConditions.TriggerCondition = "meh";
			AssertListValidationInvalidCodeError(ProcessTask.TriggerConditions.TriggerConditionInfo, true);

			ProcessTask.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			AssertNoErrors(ProcessTask.TriggerConditions.TriggerConditionInfo);

			ProcessTask.TriggerConditions.TriggerCondition = "^_^";
			ProcessTask.Validation.ValidateAll();
			AssertListValidationInvalidCodeError(ProcessTask.TriggerConditions.TriggerConditionInfo, true);
		}

		public void TestP9_CascadedEventsContext()
		{
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			Assert(trigger.WorkflowDescriptor is DummyWorkflowDescriptor);

			DummyWorkflowDescriptor.Instance.FollowingContextStepsForTest =
				new List<WorkflowEventContextPair>
				{
						new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null)),
						new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T2", null)),
						new WorkflowEventContextPair(new CodeDescriptionPair("C2", null), new CodeDescriptionPair("T1", null)),
						new WorkflowEventContextPair(new CodeDescriptionPair("C3", null), new CodeDescriptionPair("T3", null)),
				};

			trigger.P9_CascadedEventsContext = "C1 T1,C2 T1";
			AssertNoErrors(trigger.P9_CascadedEventsContextInfo);

			trigger.P9_CascadedEventsContext = "T1,C2 T1";
			AssertHasError(trigger.P9_CascadedEventsContextInfo, "Incorrect step 'T1' in context path 'T1,C2 T1'.");

			trigger.P9_CascadedEventsContext = "C1,C2 T1";
			AssertHasError(trigger.P9_CascadedEventsContextInfo, "Incorrect step 'C1' in context path 'C1,C2 T1'.");

			trigger.P9_CascadedEventsContext = "C1 T1,C2 T2";
			AssertHasError(trigger.P9_CascadedEventsContextInfo, "Incorrect step 'C2 T2' in context path 'C1 T1,C2 T2'.");

			trigger.P9_CascadedEventsContext = "C1 T1,C1 T2 XX";
			AssertHasError(trigger.P9_CascadedEventsContextInfo, "Wrong number of parts in 'C1 T2 XX' in context path 'C1 T1,C1 T2 XX'.");
		}

		[TestDate(2015, 12, 23)]
		public void TestP9_EstimatedDefaultTimeDelta_ShouldNotWarnAboutOldValues()
		{
			ProcessTask.P9_EstimatedDefaultTimeDelta = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1).Add(-TimeSpan.FromHours(999));

			AssertNoWarnings(ProcessTask.P9_EstimatedDefaultTimeDeltaInfo);
		}

		public void TestShouldValidateFKToCancelledRecord()
		{
			var validation = new ProcessTaskValidationBaseForTest(ProcessTask);
			AssertEquals(false, validation.ShouldValidateFKToCancelledRecord_Exposed(ProcessTask.P9_SE_NKMilestoneEventInfo));
			AssertEquals(false, validation.ShouldValidateFKToCancelledRecord_Exposed(ProcessTask.P9_OCInfo));
			AssertEquals(true, validation.ShouldValidateFKToCancelledRecord_Exposed(ProcessTask.P9_SE_NKExceptionEventInfo));
		}

		public void TestCheckP9_OC()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_IsActive = true;
			ProcessTask.P9_OC = contact.PK;
			AssertNoWarnings(ProcessTask.P9_OCInfo);

			contact.OC_IsActive = false;
			ProcessTask.RunPreSaveValidation();
			AssertHasWarning(ProcessTask.P9_OCInfo, "This Task Client Contact is inactive - it may not be used.");

			ProcessTask.P9_OC = ZGuid.Empty;
			AssertNoWarnings(ProcessTask.P9_OCInfo);
		}

		public void TestCheckP9_LineTriggerType()
		{
			var task = Dummy.WorkflowItems.Triggers.AddNew();

			task.P9_LineTriggerType = "ZZZ";
			AssertHasErrors(task.P9_LineTriggerTypeInfo);

			task.P9_LineTriggerType = "TSK";
			AssertNoErrors(task.P9_LineTriggerTypeInfo);

			task.P9_LineTriggerType = "";
			AssertNoErrors(task.P9_LineTriggerTypeInfo);
		}

		public void TestCheckP9_SuppressDuplicates()
		{
			var task = Dummy.WorkflowItems.Triggers.AddNew();
			task.P9_SuppressDuplicates = true;
			AssertHasErrors(task.P9_SuppressDuplicatesInfo);

			task.P9_DelayDurationSeconds = 10;
			task.RunPreSaveValidation();
			AssertNoErrors(task.P9_SuppressDuplicatesInfo);
		}

		#region Implementation

		DummyWithWorkflow Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithWorkflow>();
				}
				return dummy;
			}
		}
		DummyWithWorkflow dummy;

		ProcessTask ProcessTask
		{
			get
			{
				if (processTask == null)
				{
					processTask = Factory.New<ProcessTask>();
				}
				return processTask;
			}
		}
		ProcessTask processTask;

		ProcessTaskTemplate ProcessTaskTemplate
		{
			get
			{
				if (processTaskTemplate == null)
				{
					processTaskTemplate = Factory.New<ProcessTaskTemplate>();
				}
				return processTaskTemplate;
			}
		}
		ProcessTaskTemplate processTaskTemplate;

		TemplateProcessTask TemplateProcessTask
		{
			get
			{
				if (templateProcessTask == null)
				{
					templateProcessTask = (TemplateProcessTask)ProcessTaskTemplate.WorkflowItems.AddNew();
				}
				return templateProcessTask;
			}
		}
		TemplateProcessTask templateProcessTask;

		public class ProcessTaskValidationBaseForTest : ProcessTaskValidationBase
		{
			public ProcessTaskValidationBaseForTest(AutoProcessTasks parent) : base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecord_Exposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
