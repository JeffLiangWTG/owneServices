using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class MilestoneCompletionHelper
	{
		internal static void TriggerTaskMilestoneCompletion(ProcessTask milestone)
		{
			var tasksToComplete = GetTasksToCompleteForCompletedMilestone(milestone);

			if (!tasksToComplete.Any())
			{
				return;
			}

			AddTasksFetchHint(tasksToComplete);

			var completionMilestoneOutcomeRegistry = WorkflowDataRegistry.Instance.CompletionMilestoneOutcome.Value;
			var completionMilestoneOutcomeRegistryIsAttemptToClose = completionMilestoneOutcomeRegistry == CompletionMilestoneOutcomeModes.Codes.AttemptToClose;

			var jobHeadersNeedsRefreshStatus = new HashSet<IProcessJobHeader>();

			foreach (var taskToComplete in tasksToComplete)
			{
				CompleteTask(milestone, taskToComplete, completionMilestoneOutcomeRegistry, completionMilestoneOutcomeRegistryIsAttemptToClose, jobHeadersNeedsRefreshStatus);
			}

			foreach (var jobHeader in jobHeadersNeedsRefreshStatus)
			{
				jobHeader.RefreshWorkflowsStatus();
			}
		}

		static ProcessTask[] GetTasksToCompleteForCompletedMilestone(ProcessTask milestone)
		{
			return GetMilestoneCompletionProcessHeaders(milestone)
				.SelectMany(h => h.TaskCollectionIncludingChildWorkflowTasksBindingListView.Cast<ProcessTask>())
				.Union(GetMilestoneCompletionTasks(milestone))
				.DistinctBy(t => t.PK)
				.Where(t => !t.IsQualityContainmentBarrierTask())
				.Where(TaskStatusIsOpenAssignedOrSuspended).ToArray();
		}

		static IEnumerable<ProcessTask> GetMilestoneCompletionTasks(ProcessTask milestone)
		{
			var parent = milestone.Parent;
			if (parent == null)
			{
				return Enumerable.Empty<ProcessTask>();
			}

			return parent.WorkflowItems.Tasks.Cast<ProcessTask>()
				.Where(t => t.P9_MilestoneCompletionPivotKey == milestone.P9_MilestoneCompletionPivotKey);
		}

		static IEnumerable<IProcessHeader> GetMilestoneCompletionProcessHeaders(ProcessTask milestone)
		{
			var parent = milestone.Parent;
			if (parent == null)
			{
				return Enumerable.Empty<IProcessHeader>();
			}

			return parent.Workflows.Cast<IProcessHeader>()
				.Where(t => t.FH_MilestoneCompletionPivotKey == milestone.P9_MilestoneCompletionPivotKey);
		}

		static void CompleteTask(ProcessTask milestone, ProcessTask taskToComplete, string completionMilestoneOutcomeRegistry, bool completionMilestoneOutcomeRegistryIsAttemptToClose, HashSet<IProcessJobHeader> jobHeadersNeedsRefreshStatus)
		{
			var taskNote = Res.GetString("1649ed09-c31c-405f-a3f2-780839ef5529",
				"This task was completed automatically by the {0} milestone at {1} (UTC) based on the {2} event caused by {3}",
				milestone.P9_Description,
				milestone.P9_ActualDateUtc,
				milestone.TriggerConditions?.TriggerEventCode,
				milestone.TriggerEventSourceStaffCode);
			var noteForCanceledTaskWhenAttemptToClose = taskNote + Res.GetString("27267568-365b-4fb1-a6c2-7869fb367d73",
				", due to validation problems this task was canceled");

			var previousStatus = taskToComplete.P9_Status;
			taskToComplete.P9_Status = milestoneOutcomeRegistryTaskStatus[completionMilestoneOutcomeRegistry];

			var currentTaskNote = taskNote;

			if (taskToComplete.HasErrors)
			{
				if (completionMilestoneOutcomeRegistryIsAttemptToClose)
				{
					taskToComplete.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
					currentTaskNote = noteForCanceledTaskWhenAttemptToClose;
				}

				if (taskToComplete.HasErrors)
				{
					taskToComplete.P9_Status = previousStatus;
					return;
				}
			}

			if (previousStatus != taskToComplete.P9_Status)
			{
				taskToComplete.AppendNote(currentTaskNote, addInitialsAndDateTime: false);

				var processHeader = taskToComplete.ProcessHeader;

				if (processHeader != null && processHeader.Tasks.All(t =>
						t.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled ||
						t.P9_Status == ProcessTaskStatusCodeList.Codes.Closed))
				{
					var jobHeader = processHeader.JobHeader;

					if (jobHeader != null && !jobHeadersNeedsRefreshStatus.Contains(jobHeader))
					{
						jobHeadersNeedsRefreshStatus.Add(jobHeader);
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
		static readonly HashSet<string> taskStatusForMilestoneCompletion = new HashSet<string>
		{
			new ZString(ProcessTaskStatusCodeList.Codes.Open),
			new ZString(ProcessTaskStatusCodeList.Codes.Assigned),
			new ZString(ProcessTaskStatusCodeList.Codes.Suspended)
		};

		static bool TaskStatusIsOpenAssignedOrSuspended(ProcessTask task) => taskStatusForMilestoneCompletion.Contains(task.P9_Status);

		static void AddTasksFetchHint(IEnumerable<ProcessTask> tasks)
		{
			foreach (var task in tasks)
			{
				task.Factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, task.P9_FH_ProcessHeader));

				if (!task.P9_GS_NKAssignedStaffMember.IsEmpty)
				{
					var query = new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, task.P9_GS_NKAssignedStaffMember);
					query.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Working);
					query.AddToFilter(ProcessTasksSchema.PK, SQLComparisonOperator.NotEqual, task.PK);

					task.Factory.AddFetchHint(ProcessTasksSchema.Instance, query);
				}
			}
		}

		public static void SetCompletionMilestone(IProcessHeader workflow, ProcessTask milestone)
		{
			if (workflow == null)
			{
				throw new ArgumentNullException(nameof(workflow));
			}
			if (milestone == null)
			{
				throw new ArgumentNullException(nameof(milestone));
			}
			workflow.FH_MilestoneCompletionPivotKey = milestone.P9_MilestoneCompletionPivotKey;
		}

		public static void SetCompletionMilestone(ProcessTask task, ProcessTask milestone)
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}
			if (milestone == null)
			{
				throw new ArgumentNullException(nameof(milestone));
			}
			task.P9_MilestoneCompletionPivotKey = milestone.P9_MilestoneCompletionPivotKey;
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
		static readonly Dictionary<string, string> milestoneOutcomeRegistryTaskStatus = new Dictionary<string, string>
		{
			{ CompletionMilestoneOutcomeModes.Codes.Cancel, ProcessTaskStatusCodeList.Codes.Cancelled },
			{ CompletionMilestoneOutcomeModes.Codes.Close, ProcessTaskStatusCodeList.Codes.Closed },
			{ CompletionMilestoneOutcomeModes.Codes.AttemptToClose, ProcessTaskStatusCodeList.Codes.Closed }
		};
	}
}
