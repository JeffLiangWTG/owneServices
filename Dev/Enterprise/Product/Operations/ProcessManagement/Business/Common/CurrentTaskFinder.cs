using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class CurrentTaskFinder
	{
		public CurrentTaskFinder(IWorkflowProvider workflowProvider)
		{
			this.workflowProvider = workflowProvider;

			var businessObject = workflowProvider as BusinessObject;
			factory = businessObject != null ? businessObject.Factory : new BusinessObjectFactory() { NameForDebugging = GetType().Name };
		}
		readonly IWorkflowProvider workflowProvider;
		readonly BusinessObjectFactory factory;

		IOrderedEnumerable<ProcessTask> OpenTasks => workflowProvider.WorkflowItems.Tasks.Cast<ProcessTask>().Where(t => t.IsOpen).OrderBy(t => t.P9_Sequence).ThenBy(t => t.P9_TaskID);

		public ProcessTask FindCurrentStartableTask()
		{
			ProcessTask result = null;
			var tasks = OpenTasks;
			if (IsBufferManagementEnabled)
			{
				result = GetCurrentStartableTask(tasks);
				if (result == null)
				{
					result = GetNextStartableTask(tasks);
				}
			}
			if (result == null)
			{
				result = GetCurrentTask(tasks);
			}
			return result;
		}

		public ProcessTask FindCurrentOrNextStartableTask()
		{
			ProcessTask result = null;
			var tasks = OpenTasks;
			if (IsBufferManagementEnabled)
			{
				result = GetCurrentStartableTask(tasks);
				if (result == null)
				{
					result = GetNextStartableTask(tasks);
				}
				if (result == null)
				{
					result = GetNextOpenTaskBasedOnWorkflow(tasks);
				}
			}
			if (result == null)
			{
				result = GetCurrentTask(tasks);
				if (result == null)
				{
					result = GetNextOpenTask(tasks);
				}
			}
			return result;
		}

		bool IsBufferManagementEnabled => ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(workflowProvider, factory);

		ProcessTask GetCurrentStartableTask(IEnumerable<ProcessTask> tasks)
		{
			return tasks.FirstOrDefault(t => TaskStartableChecker.IsTaskStartable(t));
		}

		ProcessTask GetNextStartableTask(IEnumerable<ProcessTask> tasks)
		{
			return tasks.FirstOrDefault(t => t.IsCurrent && (!TaskStartableChecker.IsProcessHeaderBlocked(t) || TaskStartableChecker.IsProcessHeaderOnlyBlockedByOtherJob(t)));
		}

		ProcessTask GetCurrentTask(IEnumerable<ProcessTask> tasks)
		{
			return tasks.FirstOrDefault(t => t.IsCurrent);
		}

		ProcessTask GetNextOpenTaskBasedOnWorkflow(IEnumerable<ProcessTask> tasks)
		{
			return tasks.FirstOrDefault(t => t.P9_Status == ProcessTaskStatusCodeList.Codes.Open && (!TaskStartableChecker.IsProcessHeaderBlocked(t) || TaskStartableChecker.IsProcessHeaderOnlyBlockedByOtherJob(t)));
		}

		ProcessTask GetNextOpenTask(IEnumerable<ProcessTask> tasks)
		{
			return tasks.FirstOrDefault(t => t.P9_Status == ProcessTaskStatusCodeList.Codes.Open);
		}

		IProcessTaskStartabilityChecker TaskStartableChecker => taskStartableChecker ?? (taskStartableChecker = ObjectFactory.Get<IProcessTaskStartabilityChecker>());
		IProcessTaskStartabilityChecker taskStartableChecker;
	}
}
