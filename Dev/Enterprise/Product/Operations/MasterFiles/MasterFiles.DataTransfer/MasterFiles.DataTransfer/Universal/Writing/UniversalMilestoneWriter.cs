using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.Management;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UniversalMilestoneWriter : IUniversalMilestoneWriter
	{
		public void PopulateMilestones(BusinessObject source, IDataObject destination, bool includeInternal)
		{
			var workflowProvider = source as IWorkflowProvider;
			var milestoneCollectionParent = destination as IMilestoneCollectionParent;
			if (workflowProvider != null && milestoneCollectionParent != null)
			{
				var milestones =
					workflowProvider.WorkflowItems.Milestones.Cast<ProcessTask>()
						.Where(t => t.P9_IsPublished || includeInternal)
						.Select(TransformToUniversalMilestone).ToList();
				milestoneCollectionParent.SetMilestoneCollection(() => milestones.Count > 0 ? milestones : null);
			}
		}

		static Milestone TransformToUniversalMilestone(ProcessTask processTask)
		{
			return new Milestone
			{
				Sequence = processTask.P9_Sequence,
				Description = processTask.P9_Description,
				EventCode = processTask.P9_SE_NKMilestoneEvent,
				ConditionType = processTask.P9_TriggerCondition,
				ConditionReference = processTask.P9_TriggerConditionValue,
				ActualDate = processTask.P9_ActualDateOffset,
				EstimatedDate = processTask.P9_ScheduledDateOffset,
			};
		}
	}
}
