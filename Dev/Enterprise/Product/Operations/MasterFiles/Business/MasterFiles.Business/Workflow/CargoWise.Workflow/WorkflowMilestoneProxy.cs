using System;
using CargoWise.Types;
using CargoWise.Workflow;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowMilestoneProxy : IWorkflowMilestone
	{
		public WorkflowMilestoneProxy(ProcessTask task)
		{
			this.task = task;
		}
		readonly ProcessTask task;

		public Guid Identifier => task.PK.ToGuid();
		public bool IsInDatabase => task.IsInDatabase;
		public string EventCode => task.TriggerConditions.TriggerEventCode;
		public bool ActualDateHasChanges => task.P9_ActualDateInfo.HasChanges;
		public bool IsEstimateChanged => task.P9_ScheduledDateInfo.HasChanges;

		public DateTimeOffset? ActualDate
		{
			get => ToDateTimeOffset(task.P9_ActualDateForBinding);
			set => task.P9_ActualDateForBinding = value ?? ZDateTimeOffset.Empty;
		}

		public DateTimeOffset? OriginalActualDate
		{
			get
			{
				if (task.IsInDatabase)
				{
					return ToDateTimeOffset(new ZDateTimeOffset((ZDateTime)task.P9_ActualDateInfo.OriginalValue));
				}
				else
				{
					return null;
				}
			}
		}

		public DateTimeOffset? EstimateDate => ToDateTimeOffset(task.P9_ScheduledDateForBinding);

		public DateTimeOffset? OriginalEstimateDate
		{
			get
			{
				if (task.IsInDatabase)
				{
					return ToDateTimeOffset(new ZDateTimeOffset((ZDateTime)task.P9_ScheduledDateInfo.OriginalValue));
				}
				else
				{
					return null;
				}
			}
		}

		DateTimeOffset? ToDateTimeOffset(ZDateTimeOffset date) => date.IsValid ? date.ToDateTimeOffset() : null;
	}
}
