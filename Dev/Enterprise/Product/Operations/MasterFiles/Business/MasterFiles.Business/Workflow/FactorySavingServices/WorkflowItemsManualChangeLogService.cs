using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowItemsManualChangeLogService : IService
	{
		readonly Dictionary<IStmALogParent, (int taskCount, int triggerCount, int milestoneCount)> workflowItemAddedLogCollection = new Dictionary<IStmALogParent, (int taskCount, int triggerCount, int milestoneCount)>();
		readonly Dictionary<IStmALogParent, (int taskCount, int triggerCount, int milestoneCount)> workflowItemDeletedLogCollection = new Dictionary<IStmALogParent, (int taskCount, int triggerCount, int milestoneCount)>();

		public enum WorkflowItemActionType { Added, Deleted }

		public void ProcessTaskManualChangeLog(ProcessTask processTaskAdded, WorkflowItemActionType type)
		{
			if (WorkflowDataRegistry.Instance.EnableWorkflowManualChangeEvent.Value && !processTaskAdded.IsException && !SuppressCreatingWorkflowChangeLog)
			{
				if (!processTaskAdded.IsInDatabase && type == WorkflowItemActionType.Deleted)
				{
					RevertAddedLogCount(processTaskAdded);
				}
				else
				{
					IncrementLogCount(processTaskAdded, type);
				}
			}
		}

		public void ProcessLogs()
		{
			foreach (IStmALogParent parent in workflowItemAddedLogCollection.Keys)
			{
				AddLogToParentIfRequired(parent, WorkflowItemActionType.Added);
			}
			foreach (IStmALogParent parent in workflowItemDeletedLogCollection.Keys)
			{
				AddLogToParentIfRequired(parent, WorkflowItemActionType.Deleted);
			}
			workflowItemAddedLogCollection.Clear();
			workflowItemDeletedLogCollection.Clear();
		}

		public IDisposable SuppressWorkflowChangeLog()
		{
			SuppressCreatingWorkflowChangeLog = true;
			return new DisposableAction(() => SuppressCreatingWorkflowChangeLog = false);
		}

		bool SuppressCreatingWorkflowChangeLog { get; set; }

		void AddLogToParentIfRequired(IStmALogParent parent, WorkflowItemActionType type)
		{
			var currentDict = type == WorkflowItemActionType.Added ? workflowItemAddedLogCollection : workflowItemDeletedLogCollection;
			var currentValue = currentDict[parent];
			if (currentValue != (0, 0, 0) && !parent.IsDeleted)
			{
				var eventValue = new EventValue(type == WorkflowItemActionType.Added ? Events.WorkflowItemsManuallyAdded : Events.WorkflowItemsDeleted, isEstimate: false, eventTime: ZDateTimeOffset.Now, reference: GetEventLogReferenceBuilder(currentDict[parent]).Build());
				var log = parent.Logs.AddNew(eventValue);
			}
		}

		void IncrementLogCount(ProcessTask processTask, WorkflowItemActionType type)
		{
			var parent = (IStmALogParent)processTask.ParentBusinessObject;
			if (parent != null)
			{
				var currentDict = type == WorkflowItemActionType.Added ? workflowItemAddedLogCollection : workflowItemDeletedLogCollection;
				(int taskCount, int triggerCount, int milestoneCount) currentLogCount = (0, 0, 0);
				currentDict.TryGetValue(parent, out currentLogCount);
				if (processTask.IsTask)
				{
					currentLogCount.taskCount++;
				}
				else if (processTask.IsMilestone)
				{
					currentLogCount.milestoneCount++;
				}
				else if (processTask.IsWorkflowTrigger)
				{
					currentLogCount.triggerCount++;
				}
				currentDict[parent] = currentLogCount;
			}
		}

		void RevertAddedLogCount(ProcessTask processTask)
		{
			var parent = (IStmALogParent)processTask.ParentBusinessObject;
			var currentDict = workflowItemAddedLogCollection;
			(int taskCount, int triggerCount, int milestoneCount) currentLogCount;
			if (parent != null && currentDict.TryGetValue(parent, out currentLogCount))
			{
				if (processTask.IsTask)
				{
					currentLogCount.taskCount--;
				}
				else if (processTask.IsMilestone)
				{
					currentLogCount.milestoneCount--;
				}
				else if (processTask.IsWorkflowTrigger)
				{
					currentLogCount.triggerCount--;
				}
				currentDict[parent] = currentLogCount;
			}
		}

		EventLogReferenceBuilder GetEventLogReferenceBuilder((int taskCount, int triggerCount, int milestoneCount) currentLogCount)
		{
			var refBuilder = EventLogReferenceBuilder.New();

			if (currentLogCount.taskCount > 0)
			{
				refBuilder.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.TaskCode, currentLogCount.taskCount.ToString(CultureInfo.InvariantCulture));
			}

			if (currentLogCount.milestoneCount > 0)
			{
				refBuilder.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MilestoneCode, currentLogCount.milestoneCount.ToString(CultureInfo.InvariantCulture));
			}

			if (currentLogCount.triggerCount > 0)
			{
				refBuilder.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.TriggerCode, currentLogCount.triggerCount.ToString(CultureInfo.InvariantCulture));
			}

			ServiceTaskTrackingLogHelper.AddServiceTaskDetails(refBuilder);

			return refBuilder;
		}
	}
}
