using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public partial class Milestone
	{
		public static IReadOnlyCollection<Milestone> Create(IWorkflowProvider workflowProvider)
		{
			if (workflowProvider == null)
			{
				return System.Array.Empty<Milestone>();
			}

			return workflowProvider
				.WorkflowItems
				.Milestones
				.OfType<ProcessTask>()
				.Select(milestone =>
				{
					return new Milestone
					{
						Sequence = milestone.P9_Sequence,
						Description = milestone.P9_Description,
						EventCode = milestone.P9_SE_NKMilestoneEvent,
						EstimatedDate = milestone.P9_ScheduledDate,
						ActualDate = milestone.P9_ActualDate,
						ConditionType = milestone.P9_TriggerCondition,
						ConditionReference = milestone.P9_TriggerConditionValue
					};
				}).ToArray();
		}
	}
}
