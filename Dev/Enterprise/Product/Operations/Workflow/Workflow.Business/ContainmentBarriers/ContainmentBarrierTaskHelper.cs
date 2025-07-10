using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business
{
	public static class ContainmentBarrierTaskHelper
	{
		public static IEnumerable<IProcessTask> GetContainmentBarriersPrerequisiteTasks(IProcessTask task, bool checkWithinJobOnly = false)
		{
			var concreteTask = (ProcessTask)task;
			if (concreteTask.Parent != null)
			{
				foreach (var taskInJob in GetPreviousTasksInSameOrChildWorkflows(concreteTask))
				{
					yield return taskInJob;
				}

				var workflow = concreteTask.ProcessHeader;
				if (workflow != null)
				{
					foreach (var prereqTask in GetPrerequisiteTasks(workflow, checkWithinJobOnly))
					{
						yield return prereqTask;
					}
				}
			}
		}

		static IEnumerable<IProcessTask> GetPreviousTasksInSameOrChildWorkflows(ProcessTask concreteTask)
		{
			if (concreteTask.ProcessHeader != null)
			{
				return concreteTask.Parent.WorkflowItems.Tasks.Cast<IProcessTask>().Where(
						t => t.P9_Sequence <= concreteTask.P9_Sequence
						&& t.PK != concreteTask.PK
						&& (t.P9_FH_ProcessHeader == concreteTask.ProcessHeader.PK)
						|| concreteTask.ProcessHeader.GetChildWorkflowsDownTheHierarchy().Any(c => c.PK == t.P9_FH_ProcessHeader));
			}
			else
			{
				return concreteTask.Parent.WorkflowItems.Tasks.Cast<IProcessTask>().Where(t => t.P9_Sequence < concreteTask.P9_Sequence && t.P9_FH_ProcessHeader == concreteTask.P9_FH_ProcessHeader);
			}
		}

		static IEnumerable<IProcessTask> GetPrerequisiteTasks(IProcessHeader workflow, bool checkWithinJobOnly)
		{
			foreach (var prereq in workflow.GetPrerequisitesUpTheTree().Where(w => !checkWithinJobOnly || w.FH_ParentId == workflow.FH_ParentId))
			{
				var tasks = new List<IProcessTask>();
				tasks.AddRange(prereq.Tasks);

				foreach (var c in prereq.GetChildWorkflowsDownTheHierarchy())
				{
					tasks.AddRange(c.Tasks);
				}

				foreach (var prereqTask in tasks)
				{
					yield return prereqTask;
				}
			}
		}
	}
}
