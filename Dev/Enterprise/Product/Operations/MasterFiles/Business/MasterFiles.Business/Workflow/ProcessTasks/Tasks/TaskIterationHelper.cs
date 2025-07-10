using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	static class TaskIterationHelper
	{
		internal static void TryAutomaticallyAssignIterationAfterChange(ProcessTask task)
		{
			if (!task.IsTask || !task.P9_FH_ProcessHeader.IsValid || task.ProcessHeader == null || !string.IsNullOrEmpty(task.Iteration))
			{
				return;
			}

			var allTasksInWorkflow = task.ProcessHeader.Tasks.Cast<ProcessTask>().Where(x => x != null && !x.IsDeleted).ToHashSet();
			allTasksInWorkflow.Add(task);

			var taskSequenceIterationMap = allTasksInWorkflow
				.Select(x => new
				{
					TaskSequence = x.P9_Sequence,
					IterationId = x.Iteration,
					IterationSequence = x.IterationPivot?.Iteration?.P9I_Sequence ?? new ZByte(0),
					IsChangedTask = x.PK == task.PK,
				})
				.OrderBy(x => x.TaskSequence)
				.ThenBy(x => x.IterationSequence)
				.ToArray();

			var taskSequenceIterationMapExceptChangedTask = taskSequenceIterationMap.Where(x => !x.IsChangedTask).ToArray();

			var iterations = taskSequenceIterationMapExceptChangedTask.Select(x => x.IterationId).Distinct().ToArray();

			if (!iterations.Any() || (iterations.Length == 1 && string.IsNullOrEmpty(iterations.Single())))
			{
				return;
			}

			if (iterations.Length == 1 && iterations.Single() != ZString.Empty)
			{
				AutomaticallyAssignIterationAfterChange(task, iterations.Single());
				return;
			}

			var maxSequenceForOtherTasks = taskSequenceIterationMapExceptChangedTask.Max(x => x.TaskSequence);
			var minSequenceForOtherTasks = taskSequenceIterationMapExceptChangedTask.Min(x => x.TaskSequence);

			if (task.P9_Sequence > maxSequenceForOtherTasks || task.P9_Sequence < minSequenceForOtherTasks)
			{
				AddWarning(task, IterationNotAssignedWarning);
				return;
			}

			var tasksWithMatchingSequence = taskSequenceIterationMapExceptChangedTask.Where(x => x.TaskSequence == task.P9_Sequence).ToArray();
			var lastTaskWithMatchingSequence = tasksWithMatchingSequence.LastOrDefault();

			if (lastTaskWithMatchingSequence != null)
			{
				AutomaticallyAssignIterationAfterChange(task, lastTaskWithMatchingSequence.IterationId);
				return;
			}

			if (taskSequenceIterationMap.First().IsChangedTask || taskSequenceIterationMap.Last().IsChangedTask)
			{
				AddWarning(task, IterationNotAssignedWarning);
				return;
			}

			for (var i = 0; i < taskSequenceIterationMap.Length; i++)
			{
				if (taskSequenceIterationMap[i].IsChangedTask)
				{
					var taskBefore = taskSequenceIterationMap[i - 1];
					var taskAfter = taskSequenceIterationMap[i + 1];

					if (!string.IsNullOrEmpty(taskBefore.IterationId) && !string.IsNullOrEmpty(taskAfter.IterationId))
					{
						var highestIterationForTaskAfterSequence = taskSequenceIterationMapExceptChangedTask
							.Where(x => x.TaskSequence == taskAfter.TaskSequence)
							.MaxBy(x => x.IterationSequence);

						AutomaticallyAssignIterationAfterChange(task, highestIterationForTaskAfterSequence.IterationId);
						return;
					}
				}
			}

			AutomaticallyAssignIterationAfterChange(task, ZString.Empty);
		}

		static void AutomaticallyAssignIterationAfterChange(ProcessTask task, ZString iteration)
		{
			task.Iteration = iteration;
			var warning = string.IsNullOrEmpty(iteration) ? IterationNotAssignedWarning : IterationAutomaticallyAssignedWarning;
			AddWarning(task, warning);
		}

		static void AddWarning(ProcessTask task, string warning)
		{
			task.Validation.ValidateIterationAndAddWarning(warning);
		}

		static string IterationAutomaticallyAssignedWarning => Res.GetString("554c70c7-c124-456e-8da2-caf3c5428f8b",
			"This task's workflow includes at least one quality iteration, and the system has selected an iteration for this task based on the tasks around it. Please ensure that this task is assigned to the correct quality iteration if applicable.");

		static string IterationNotAssignedWarning => Res.GetString("ba0c3a0b-ea8f-45a0-875c-acac92843686",
			"This task's workflow includes at least one quality iteration. Please ensure that this task is assigned to the correct quality iteration if applicable.");
	}
}
