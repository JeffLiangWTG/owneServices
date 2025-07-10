using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskValidationBase : ProcessTasksValidation
	{
		public ProcessTaskValidationBase(AutoProcessTasks parent)
			: base(parent)
		{
		}

		protected new ProcessTask Parent
		{
			get { return (ProcessTask)base.Parent; }
		}

		#region ZValidation Overrides

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIsMilestone();
			ValidateReferenceCode();

			if (Parent.IsMilestone)
			{
				ValidateExceptionTypeCode();
			}
			else if (Parent.IsException)
			{
				ValidateExceptionTypeCode();
				ValidateExceptionCausePK();
				ValidateExceptionResolutionPK();
			}
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == ProcessTasksSchema.Constants.P9_SE_NKMilestoneEvent || info.Name == ProcessTasksSchema.Constants.P9_OC)
			{
				return false; // Validated in TriggerConditionsViewModelValidation. Without this, auto-gen code causes excessive db hits on StmEvent.
			}
			else
			{
				return base.ShouldValidateFKToCancelledRecord(info);
			}
		}

		#endregion

		#region P9_ActualDate

		protected override void CheckP9_ActualDate()
		{
			base.CheckP9_ActualDate();

			if (IsMilestoneActualDateInTheFuture(Parent))
			{
				Parent.P9_ActualDateInfo.AddError(ActualDateIsInTheFutureErrorMessage);
			}

			CheckExceptionStartOrEndDate(Parent.P9_ActualDateInfo);
		}

		protected override void CheckP9_ActualDateUtc()
		{
			base.CheckP9_ActualDateUtc();

			if (IsMilestoneActualDateInTheFuture(Parent))
			{
				Parent.P9_ActualDateUtcInfo.AddError(ActualDateIsInTheFutureErrorMessage);
			}
		}

		public static bool IsMilestoneActualDateInTheFuture(ProcessTask parent)
		{
			return parent.IsMilestone
				&& WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.Value
				&& parent.ActualDateWithOffset_SetDuringSession.IsValid
				&& parent.ActualDateWithOffset_SetDuringSession.IsInTheFuture();
		}

		public static MultilingualString ActualDateIsInTheFutureErrorMessage => ResString.GetMultilingualString("d96ad70f-d8f9-4435-b6fd-1f808ea21906", "Milestones cannot have an Actual Start time that is in the future.");

		#endregion

		#region P9_ActualDateUpdateType

		protected override void CheckP9_ActualDateUpdateType()
		{
			base.CheckP9_ActualDateUpdateType();
			MandatoryValidation.CheckEntered(Parent.P9_ActualDateUpdateTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.P9_ActualDateUpdateTypeInfo);
		}

		#endregion

		#region P9_Description

		protected override void CheckP9_Description()
		{
			base.CheckP9_Description();
			MandatoryValidation.CheckEntered(Parent.P9_DescriptionInfo);
		}

		#endregion

		#region P9_Sequence

		protected override void CheckP9_Sequence()
		{
			base.CheckP9_Sequence();
			if (Parent.P9_Sequence < 0)
			{
				Parent.P9_SequenceInfo.AddError(Res.GetString("746cf16e-7502-4475-ab7d-e2cba0e216f7", "Sequence must have a non-negative value"));
			}
		}

		#endregion

		#region P9_Status

		protected override void CheckP9_Status()
		{
			base.CheckP9_Status();
			if (Parent.P9_Status != ProcessTask.NextToBeCompletedStatusCode && Parent.P9_Status != ProcessTask.LastCompletedStatusCode)
			{
				MandatoryValidation.CheckEntered(Parent.P9_StatusInfo);
				ListValidation.ErrorIfInvalidCode(Parent.P9_StatusInfo);
			}

			CheckExceptionActioned(Parent.P9_StatusInfo);
		}

		void CheckExceptionActioned(ZPropertyInfo info)
		{
			if (Parent.IsException &&
				Parent.IsExceptionActioned &&
				Parent.P9_GS_NKAssignedStaffMember.IsEmpty &&
				Parent.P9_GG_AssignedGroup.IsEmpty &&
				WorkflowDataRegistry.Instance.ExceptionRequireStaffGroup.Value &&
				(!Parent.IsInDatabase || Parent.HasChanges))
			{
				info.AddError(Res.GetString("00eebeae-eaa9-460a-a342-a8e361df7efa", "For actioned exceptions, you must specify either an assigned resource or group."));
			}
		}

		#endregion

		#region P9_Type

		protected override void CheckP9_Type()
		{
			base.CheckP9_Type();
			MandatoryValidation.CheckEntered(Parent.P9_TypeInfo);
		}

		#endregion

		#region P9_GG_AssignedGroup

		protected override void CheckP9_GG_AssignedGroup()
		{
			base.CheckP9_GG_AssignedGroup();
			ListValidation.ErrorIfInvalidPK(Parent.P9_GG_AssignedGroupInfo);
			CheckExceptionActioned(Parent.P9_GG_AssignedGroupInfo);
		}

		#endregion

		#region GS_NKAssignedStaffMember

		protected override void CheckP9_GS_NKAssignedStaffMember()
		{
			base.CheckP9_GS_NKAssignedStaffMember();
			CheckExceptionActioned(Parent.P9_GS_NKAssignedStaffMemberInfo);
		}

		#endregion

		#region P9_EstDuration

		protected override void CheckP9_EstDurationIsValidZDateTimeRange()
		{
			// This property is only used for hours
		}

		#endregion

		#region P9_EstimatedDefaultedFrom

		protected override void CheckP9_EstimatedDefaultedFrom()
		{
			base.CheckP9_EstimatedDefaultedFrom();
			if (Parent.WorkflowDescriptor != null && Parent.P9_EstimatedDefaultedFrom != ProcessTask.P9_EstimatedDefaultedFrom_Actual)
			{
				ListValidation.ErrorIfInvalidCode(Parent.P9_EstimatedDefaultedFromInfo);
			}
		}

		#endregion

		#region P9_EstimatedDefaultTimeDelta

		protected override void CheckP9_EstimatedDefaultTimeDelta()
		{
			base.CheckP9_EstimatedDefaultTimeDelta();
			bool hasDefaultEstimatedFrom = !Parent.P9_EstimatedDefaultedFrom.IsEmpty || Parent.P9_EstimatedDefaultFromPredecessor > 0;
			if (hasDefaultEstimatedFrom && Parent.P9_EstimatedDefaultTimeDelta.IsEmpty)
			{
				Parent.P9_EstimatedDefaultTimeDeltaInfo.AddError(Res.GetString("0d7cdcdc-b765-4ff3-8462-faa63aadf59e", "You must specify an Estimate Default Delta"));
			}
		}

		#endregion

		#region P9_RN_NKOriginCountry / P9_RN_NKDestinationCountry

		protected override void CheckP9_RN_NKOriginCountry()
		{
			base.CheckP9_RN_NKOriginCountry();
			ListValidation.ErrorIfInvalidCode(Parent.P9_RN_NKOriginCountryInfo);
		}

		protected override void CheckP9_RN_NKDestinationCountry()
		{
			base.CheckP9_RN_NKDestinationCountry();
			ListValidation.ErrorIfInvalidCode(Parent.P9_RN_NKDestinationCountryInfo);
		}

		#endregion

		#region P9_IsCalendarItem

		protected override void CheckP9_IsCalendarItem()
		{
			base.CheckP9_IsCalendarItem();

			if (Parent.P9_IsCalendarItem)
			{
				if (Parent.AssignedStaffMember == null && (Parent.AssignedGroup == null || Parent.AssignedGroup.Staff.Count == 0))
				{
					Parent.P9_IsCalendarItemInfo.AddWarning(Res.GetString("0F93ECC1-B120-44E2-A3FE-4EDD7131ADC3", "Appointment items will only be created when you assign a staff member or group with members."));
				}
				else if ((Parent.AssignedStaffMember != null && Parent.AssignedStaffMember.GS_EmailAddress.IsEmpty) || (Parent.AssignedGroup != null && Parent.AssignedGroup.Staff.Cast<GlbStaff>().Any(staff => staff.GS_EmailAddress.IsEmpty)))
				{
					Parent.P9_IsCalendarItemInfo.AddWarning(Res.GetString("44A4380F-6897-4C9F-9EA6-E5412321E508", "Appointment items will only be sent to staff members with a valid email address."));
				}

				if (Parent.P9_ScheduledDate.IsEmpty)
				{
					Parent.P9_IsCalendarItemInfo.AddWarning(Res.GetString("23154ef7-2cf8-445b-b9e3-68bae62d7084", "Appointment items will only be created when you specify a Scheduled Date."));
				}
			}
		}

		#endregion

		#region IsMilestone

		public void ValidateIsMilestone()
		{
			ValidateCalculatedProperty(Parent.IsMilestoneInfo);
		}

		#endregion

		#region ExceptionTypeCode

		public void ValidateExceptionTypeCode()
		{
			if (Parent.IsException || Parent.IsMilestone)
			{
				ValidateCalculatedProperty(Parent.ExceptionTypeCodeInfo);
			}
		}

		protected void CheckExceptionTypeCode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ExceptionTypeCodeInfo);

			var type = Parent.ExceptionType;

			if (type == null)
			{
				if (Parent.IsException && WorkflowDataRegistry.Instance.ExceptionRequireType.Value)
				{
					MandatoryValidation.CheckEntered(Parent.ExceptionTypeCodeInfo, Res.GetString("1b62cdd0-c45b-4f78-b572-c4b056210740", "type"));
				}

				return;
			}

			var notificationType = Parent.IsInDatabase && !Parent.P9_SE_NKExceptionEventInfo.HasChanges ? CargoWise.ComponentModel.NotificationType.Warning : CargoWise.ComponentModel.NotificationType.Error;

			if (!type.WET_IsActive)
			{
				Parent.ExceptionTypeCodeInfo.AddNotification(notificationType, Res.GetString("c8bd4f10-7a3e-46bf-a2ae-668d22236102", "Exception type is inactive."));
			}

			if (!type.WET_JobType.IsEmpty && type.WET_JobType != Parent.WorkflowType)
			{
				Parent.ExceptionTypeCodeInfo.AddNotification(notificationType, Res.GetString("edb8e76b-ca9b-49fb-b5a2-44f9b394be23", "Exception type is invalid."));
			}
		}

		#endregion

		#region Exception Cause/Resolution

		public void ValidateExceptionCausePK()
		{
			if (Parent.IsException)
			{
				ValidateCalculatedProperty(Parent.ExceptionCausePKInfo);
			}
		}

		protected void CheckExceptionCausePK()
		{
			var type = Parent.ExceptionType;

			if (type == null)
			{
				return;
			}

			if (Parent.IsExceptionActioned && type.WET_IsCauseRequired)
			{
				MandatoryValidation.CheckEntered(Parent.ExceptionCausePKInfo, Res.GetString("db100cca-8975-4805-a932-24cd702a2275", "cause"));
			}

			ListValidation.ErrorIfInvalidPK(Parent.ExceptionCausePKInfo);

			var exception = Parent.ProcessWorkflowException;
			if (exception != null && !exception.IsDeleted && (!exception.Cause?.WEC_IsActive ?? false))
			{
				var msg = Res.GetString("96cdfcce-493a-4ae4-9250-a26879720e07", "Cause is not active");

				if (Parent.IsInDatabase && !exception.WEX_WEC_CauseInfo.HasChanges)
				{
					Parent.ExceptionCausePKInfo.AddWarning(msg);
				}
				else
				{
					Parent.ExceptionCausePKInfo.AddError(msg);
				}
			}
		}

		public void ValidateExceptionResolutionPK()
		{
			if (Parent.IsException)
			{
				ValidateCalculatedProperty(Parent.ExceptionResolutionPKInfo);
			}
		}

		protected void CheckExceptionResolutionPK()
		{
			var type = Parent.ExceptionType;

			if (type == null)
			{
				return;
			}

			if (Parent.IsExceptionActioned && type.WET_IsResolutionRequired)
			{
				MandatoryValidation.CheckEntered(Parent.ExceptionResolutionPKInfo, Res.GetString("2EC8FEB9-4D0D-47A4-9A6E-CB08F8C1C406", "resolution"));
			}

			ListValidation.ErrorIfInvalidPK(Parent.ExceptionResolutionPKInfo);

			var exception = Parent.ProcessWorkflowException;
			if (exception != null && !exception.IsDeleted && (!exception.Resolution?.WER_IsActive ?? false))
			{
				var msg = Res.GetString("fe3b1b8c-eaa0-41fa-ae0a-1c87d579cd1a", "Resolution is not active");

				if (Parent.IsInDatabase && !exception.WEX_WER_ResolutionInfo.HasChanges)
				{
					Parent.ExceptionResolutionPKInfo.AddWarning(msg);
				}
				else
				{
					Parent.ExceptionResolutionPKInfo.AddError(msg);
				}
			}
		}

		#endregion

		#region ExceptionEndDate

		protected override void CheckP9_ExceptionEndDate()
		{
			base.CheckP9_ExceptionEndDate();
			CheckExceptionStartOrEndDate(Parent.P9_ExceptionEndDateInfo);
		}

		void CheckExceptionStartOrEndDate(ZPropertyInfo propertyInfo)
		{
			if (Parent.P9_ActualDateOffset > Parent.P9_ExceptionEndDate)
			{
				propertyInfo.AddError(Res.GetString("bf3b301e-91e1-4980-bc8c-d960acf31f45", "Exception End must be after the Exception Time"));
			}
		}

		#endregion

		#region ExceptionDurationHours

		protected override void CheckP9_ExceptionDurationHours()
		{
			base.CheckP9_ExceptionDurationHours();
			MandatoryValidation.CheckNotNegative(Parent.P9_ExceptionDurationHoursInfo);
		}

		#endregion

		#region P9_SE_NKTaskCompletionEvent

		protected override void CheckP9_SE_NKTaskCompletionEvent()
		{
			base.CheckP9_SE_NKTaskCompletionEvent();

			ListValidation.ErrorIfInvalidCode(Parent.P9_SE_NKTaskCompletionEventInfo);
		}

		#endregion

		#region ReferenceCode

		public void ValidateReferenceCode()
		{
			ValidateCalculatedProperty(Parent.ReferenceCodeInfo);
		}

		protected virtual void CheckReferenceCode()
		{
		}

		#endregion

		#region P9_CascadedEventsContext

		protected override void CheckP9_CascadedEventsContext()
		{
			base.CheckP9_CascadedEventsContext();
			if (!Parent.P9_CascadedEventsContext.IsEmpty && Parent.WorkflowDescriptor != null)
			{
				try
				{
					Parent.WorkflowDescriptor.GetContextPathFromString(Parent.P9_CascadedEventsContext);
				}
				catch (InvalidOperationException ex)
				{
					Parent.P9_CascadedEventsContextInfo.AddError(ex.Message);
				}
			}
		}

		#endregion

		#region CheckP9_LineTriggerType

		protected override void CheckP9_LineTriggerType()
		{
			base.CheckP9_LineTriggerType();
			ListValidation.ErrorIfInvalidCode(Parent.P9_LineTriggerTypeInfo);
		}

		#endregion

		#region P9_ShareTasksForAllCompanies

		public void ValidateP9_ShareTasksForAllCompanies()
		{
			ValidateP9_RespondToCascadedEvents();
		}

		protected override void CheckP9_RespondToCascadedEvents()
		{
			base.CheckP9_RespondToCascadedEvents();

			if (Parent.IsTemplateTask && !Parent.P9_ShareTasksForAllCompanies)
			{
				var template = Parent.Factory.Load<ProcessTaskTemplate>(Parent.P9_ParentID);

				if (template != null && template.ShouldWarnAboutNonSharedTasks)
				{
					Parent.P9_ShareTasksForAllCompaniesInfo.AddWarning(Res.GetString("9318d4d4-42dd-46de-9016-26c3f1f65f57", "This task is not marked as 'shared', but its template is marked as 'global'. This task will only be available in the company in which the template is applied."));
				}
			}
		}

		#endregion

		#region P9_EstimatedDefaultTimeDelta

		protected override void CheckP9_EstimatedDefaultTimeDeltaIsValidZDateTimeRange()
		{
			// No need to check time offset. This column allows negative values so it's not relevant to ensure we're within 12 months of the stored date.
		}

		#endregion

		#region P9_ParentTemplateID()

		protected override void CheckP9_ParentTemplateID()
		{
			base.CheckP9_ParentTemplateID();
			if (WorkflowAfterOnSavingBOService.GetTemplateApplicationRaceConditionHandler(Parent.Factory)?.IsSuspectedDuplicate(Parent) ?? false)
			{
				Parent.P9_ParentTemplateIDInfo.AddWarning(Res.GetString("f4b3bab6-6dde-430c-924d-c1294e1a5b4f", "A very similar row with the same template was detected in the database during save. This may be a duplicate created by another user or service task."));
			}
		}

		#endregion

		#region P9_GC

		protected override void CheckP9_GCIsValidZGuid()
		{
			// Suppress this validation because the field is read only.
		}

		#endregion

		#region P9_GE_TriggerDepartment

		protected override void CheckP9_GE_TriggerDepartmentIsValidZGuid()
		{
			// Suppress this validation because the field is read only.
		}

		#endregion

		#region P9_OC

		protected override void CheckP9_OC()
		{
			base.CheckP9_OC();
			if (!Parent.Contact?.OC_IsActive ?? false)
			{
				Parent.P9_OCInfo.AddWarning(Res.GetString("4865cef1-003b-4b53-941d-fd32e4855e78", "This {0} is inactive - it may not be used.", Parent.P9_OCInfo.HumanReadableName));
			}
		}

		#endregion

		#region CompletedTimeLocal

		public void ValidateCompletedTimeLocal()
		{
			ValidateCalculatedProperty(Parent.CompletedTimeLocalInfo);
		}

		protected virtual void CheckCompletedTimeLocal()
		{
		}

		#endregion

		#region Iteration

		public void ValidateIteration()
		{
			ValidateCalculatedProperty(Parent.IterationInfo);
		}

		internal void ValidateIterationAndAddWarning(string warningAboutIterationAutoAssignment)
		{
			using (SetWarningToAddOnCheckIteration(warningAboutIterationAutoAssignment))
			{
				ValidateIteration();
			}
		}

		protected virtual void CheckIteration()
		{
		}

		protected virtual IDisposable SetWarningToAddOnCheckIteration(string warningAboutIterationAutoAssignment)
		{
			return null;
		}

		protected override void CheckP9_SuppressDuplicates()
		{
			base.CheckP9_SuppressDuplicates();
			if (Parent.P9_SuppressDuplicates && Parent.P9_DelayDurationSeconds == 0)
			{
				Parent.P9_SuppressDuplicatesInfo.AddError(Res.GetString("B97B9C19-A544-4381-A23B-4A1573FAD324", "You cannot set 'Suppress Duplicates' when 'Delay Duration' is 0."));
			}
		}

		#endregion
	}
}
