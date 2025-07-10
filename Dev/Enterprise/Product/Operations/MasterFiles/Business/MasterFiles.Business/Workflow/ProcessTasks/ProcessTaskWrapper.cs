using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.CalendarArithmetic;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.MasterFiles.Business.ProcessTask;

namespace Enterprise.MasterFiles.Business
{
	class ProcessTaskWrapper : IProcessTaskWrapper
	{
		public ProcessTaskWrapper(ProcessTask processTask)
		{
			ProcessTask = processTask ?? throw new ArgumentNullException(nameof(processTask));
			StatusChangeLogs = new Stack<ProcessTaskStatusChangeLog>();
		}

		public DateTimeOffset? ActualDate
		{
			get => ProcessTask.P9_ActualDateForBinding.ToDateTimeOffsetSafe();
			set => ProcessTask.P9_ActualDateForBinding = new ZDateTimeOffset(value);
		}

		public DateTime? ActualDuration
		{
			get => ConvertToDateTime(ProcessTask.P9_ActualDuration);
			set => ProcessTask.P9_ActualDuration = new ZDateTime(value);
		}

		public DateTime? OverrideActualDuration
		{
			get => ConvertToDateTime(ProcessTask.OverrideActualDuration);
			set => ProcessTask.OverrideActualDuration = new ZDateTime(value);
		}

		public string AssignedStaffMemberCode
		{
			get => ProcessTask.P9_GS_NKAssignedStaffMember;
			set => ProcessTask.P9_GS_NKAssignedStaffMember = new ZString(value);
		}

		public DateTimeOffset? CompletedTime
		{
			get => ProcessTask.P9_CompletedTime.ToDateTimeOffsetSafe();
			set => ProcessTask.P9_CompletedTimeUtc = new ZDateTimeOffset(value).ToUtcZDateTime();
		}

		public DateTimeOffset? SuspendedAt
		{
			get => ProcessTask.P9_SuspendedAtForBinding.ToDateTimeOffsetSafe();
			set => ProcessTask.P9_SuspendedAtForBinding = new ZDateTimeOffset(value);
		}

		public string TaskCompletionEvent
		{
			get => ProcessTask.P9_SE_NKTaskCompletionEvent;
			set => ProcessTask.P9_SE_NKTaskCompletionEvent = value;
		}

		public string Description
		{
			get => ProcessTask.P9_Description;
			set => ProcessTask.P9_Description = value;
		}

		public string Type
		{
			get => ProcessTask.P9_Type;
			set => ProcessTask.P9_Type = value;
		}

		public void AddEventLog(IEventValue eventValue)
		{
			var @event = Events.All[eventValue.Code];
			if (@event != null)
			{
				(ProcessTask.Parent as IStmALogParent)?.Logs.CreateRecreateOrUpdateEventLog(eventValue.ToEventValue());
			}
		}

		public Guid? RequiredCapabilityId
		{
			get
			{
				if (ProcessTask.P9_G4_RequiredCapability.IsEmpty || !ProcessTask.P9_G4_RequiredCapability.IsValid)
				{
					return null;
				}
				return ProcessTask.P9_G4_RequiredCapability.ToGuid();
			}

			set => ProcessTask.P9_G4_RequiredCapability = new ZGuid(value);
		}

		public string Status
		{
			get => ProcessTask.P9_Status;
			set => ProcessTask.SetStatusCore(new ZString(value));
		}

		public DateTimeOffset? SuspendedAtUtc
		{
			get => ProcessTask.P9_SuspendedAtForBinding.ToDateTimeOffsetSafe();
			set => ProcessTask.P9_SuspendedAtForBinding = new ZDateTimeOffset(value);
		}

		public DateTime? TotalSuspendedDuration
		{
			get => ConvertToDateTime(ProcessTask.P9_TotalSuspendedDuration);
			set => ProcessTask.P9_TotalSuspendedDuration = new ZDateTime(value);
		}

		public bool CanCancelTask => WorkflowDataRegistry.Instance.TaskTypes.Value
			.GetTaskTypesFromWorkflowCode(ProcessTask.WorkflowType)
			.OfType<WorkflowTaskType>()
			.SingleOrDefault(w => w.Code == ProcessTask.P9_Type)?.CanCancelTask ?? true;

		public bool CanCloseTaskNotAssignedToSelf =>
			((WorkflowTaskType)ProcessTask.Lookups.Types.FindByCode(ProcessTask.P9_Type))?.CanCloseTaskNotAssignedToSelf ?? true;

		public bool IsInDatabase => ProcessTask.IsInDatabase;

		public bool IsTask => ProcessTask.IsTask;

		public bool ShouldPromptToResumeSuspendedTasks => ProcessTask.ShouldPromptToResumeSuspendedTasks;

		public int TaskDurationTrackingOutOfHoursLimit => WorkflowDataRegistry.Instance.TaskDurationTrackingOutOfHoursLimit.Value;

		public IDeletableItem IterationPivot =>
			ProcessTask.IterationPivot == null ? null : new ProcessTaskDeletableRelationWrapper<IProcessTaskIterationLinkPivot>(ProcessTask.IterationPivot);

		public IDeletableItem UdfConditionHiddenNote => ProcessTask.UdfConditionHiddenNote;

		public IWorkTimeArithmetic WorkTimeArithmetic => ProcessTask.WorkDaysHelper;

		public ICollection<IDeletableItem> TagLinks => ((ITagable)ProcessTask).TagLinks.Select(x => new ProcessTaskDeletableRelationWrapper<ITagLink>(x)).ToArray();

		public ICollection<IDeletableItem> IterationLinks =>
			ProcessTask.IterationLinks.OfType<IProcessTaskIterationLink>().Select(x => new ProcessTaskDeletableRelationWrapper<IProcessTaskIterationLink>(x)).ToArray();

		public bool CanChangeStatusTo(string statusCode) => ProcessTask.DoStatusChangeRespondersAllowStatusChange(new ZString(statusCode));

		public Task MarkParentsAsModifiedAsync(DateTimeOffset now)
		{
			var workflow = ProcessTask.ProcessHeader;
			if (workflow != null)
			{
				var jobHeader = workflow.JobHeader;
				if (jobHeader != null)
				{
					jobHeader.FH_SystemLastEditTimeUtc = new ZDateTimeOffset(now).ToUtcZDateTime();
				}

				workflow.Validation.ValidateFH_GG_ReleaseGroup();
				workflow.FH_SystemLastEditTimeUtc = new ZDateTimeOffset(now).ToUtcZDateTime();
			}

			return Task.CompletedTask;
		}

		public void OnNonCancellableTaskPermissionOverriden()
		{
			if (!ProcessTask.IsValidationOnTaskCancellationSuspended)
			{
				ProcessTask.AppendNote(Res.GetString("bf8e97e1-7fd8-4e82-b27d-4688404412e6", "Task canceled by user with permission to cancel tasks that have been configured to not allow cancellation."));
			}
		}

		public void HandlePreviouslyCancelledStatus() => ProcessTask.ResumeValidationOnTaskCancellation();

		public bool CanAuthenticateAsAssignedStaff()
		{
			var result = true;
			if (ProcessTask.AssignedStaffMember != null)
			{
				var args = new PasswordRequestEventArgs(ProcessTask.AssignedStaffMember.GS_LoginName);
				ProcessTask.OnTaskOwnerPasswordRequested(args);
				result = args.IsValidPassword;
				if (!result)
				{
					ProcessTask.P9_StatusInfo.RefreshBinding();
				}
			}

			return result;
		}

		public DateTime? OriginalActualDuration => ConvertToDateTime((ZDateTime)ProcessTask.P9_ActualDurationInfo.OriginalValue);
		public DateTimeOffset? OriginalActualDate => ProcessTask.OriginalActualDateOffset.ToDateTimeOffsetSafe();
		public DateTimeOffset? OriginalSuspendedAt => ProcessTask.OriginalSuspendedAtOffset.ToDateTimeOffsetSafe();
		public DateTime? OriginalTotalSuspendedDuration => ConvertToDateTime((ZDateTime)ProcessTask.P9_TotalSuspendedDurationInfo.OriginalValue);

		public Stack<ProcessTaskStatusChangeLog> StatusChangeLogs { get; }

		public DateTimeOffset? LastActualDurationEvent { get; set; }
		public DateTimeOffset? LastSuspendedDurationEvent { get; set; }

		DateTime? ConvertToDateTime(ZDateTime input)
		{
			if (input.IsEmpty || !input.IsValid)
			{
				return null;
			}
			return input.ToDateTime();
		}

		internal ProcessTask ProcessTask { get; }
	}
}
