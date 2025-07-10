using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	static class TaskAssignmentRestrictionValidationRule
	{
		internal static ValidationRule<ProcessTask> GetRule(ZString workflowType)
		{
			return new ValidationRule<ProcessTask>(nameof(ValidateTaskAssignmentRestrictions),
				ProcessTasks.Schema.P9_GS_NKAssignedStaffMember,
				t => ValidateTaskAssignmentRestrictions(t, workflowType),
				(t, i) => t.Validation.ValidateP9_GS_NKAssignedStaffMember(),
				false,
				nameof(ProcessTask.P9_GS_NKAssignedStaffMember), nameof(ProcessTask.P9_Type), nameof(ProcessTask.P9_Status));
		}

		static IEnumerable<IGrouping<AddValidationNotification<ProcessTask>, ProcessTask>> ValidateTaskAssignmentRestrictions(IEnumerable<ProcessTask> tasks, ZString workflowType)
		{
			var tasksByKey = tasks
				.Where(t => !t.AssignmentRestrictionValidationSuspended && (t.IsOpen || t.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)) // Not cancelled.
				.GroupBy(s => s.P9_Type);

			foreach (var group in tasksByKey)
			{
				var checks = WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.Value.Cast<TaskTypeRestrictions>()
					.Where(r => r.TaskType == group.Key && r.NotificationType != NotificationTypeList.Codes.None && r.WorkflowType == workflowType && r.Active).ToArray();

				if (checks.Any())
				{
					foreach (var check in checks)
					{
						var tasksInCheckFilter = GetRestrictionTypeFilter(check);
						var validationDelegate = ProcessTaskValidation.GetRestrictionDelegate(check);

						foreach (var task in group)
						{
							var relatedTasks = TaskAssignmentHelper.GetValidProcessTaskListAccordingToScope(task, check.Scope, usedInTaskValidation: true)
								.Where(tasksInCheckFilter)
								.ToArray();

							var invalidTasks = relatedTasks.Where(GetInvalidTaskByTaskRestrictionFilter(check, task));

							if (invalidTasks.Any())
							{
								yield return new Grouping<AddValidationNotification<ProcessTask>, ProcessTask>(validationDelegate, invalidTasks.Append(task).Where(t => t.IsOpen));
							}

							foreach (var relatedTask in relatedTasks)
							{
								relatedTask.Validation.ValidateP9_GS_NKAssignedStaffMember();
							}

							task.Validation.ValidateP9_GS_NKAssignedStaffMember();
						}
					}
				}
			}
		}

		static Func<ProcessTask, bool> GetInvalidTaskByTaskRestrictionFilter(TaskTypeRestrictions restriction, ProcessTask existingTask)
		{
			switch (restriction.RestrictionType)
			{
				case RestrictionTypeList.Codes.SameResource:
					return task => task.PK != existingTask.PK && task.P9_GS_NKAssignedStaffMember != existingTask.P9_GS_NKAssignedStaffMember;
				case RestrictionTypeList.Codes.DifferentResource:
					return task => task.PK != existingTask.PK && task.P9_GS_NKAssignedStaffMember == existingTask.P9_GS_NKAssignedStaffMember;

				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unknown restriction type: {0}", restriction.RestrictionType));
			}
		}

		static Func<ProcessTask, bool> GetRestrictionTypeFilter(TaskTypeRestrictions restriction)
		{
			switch (restriction.RestrictionType)
			{
				case RestrictionTypeList.Codes.SameResource:
					{
						var set = new HashSet<ZString>(restriction.TaskTypesCollection.Select(t => ((RestrictedTaskTypes)t).Code));
						set.Add(restriction.TaskType);
						return task => set.Contains(task.P9_Type);
					}
				case RestrictionTypeList.Codes.DifferentResource:
					{
						var set = new HashSet<ZString>(restriction.TaskTypesCollection.Select(t => ((RestrictedTaskTypes)t).Code));
						return task => set.Contains(task.P9_Type);
					}
				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unknown restriction type: {0}", restriction.RestrictionType));
			}
		}
	}
}
