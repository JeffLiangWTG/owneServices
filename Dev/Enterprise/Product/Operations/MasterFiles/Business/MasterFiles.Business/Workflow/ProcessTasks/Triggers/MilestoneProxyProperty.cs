using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class MilestoneProxyProperty
	{
		public MilestoneProxyProperty(ProcessTaskCollection workflowItems, EstimateActual estimateActual, Event eventType)
		{
			Argument.NotNull(workflowItems, nameof(workflowItems));
			Argument.NotNull(eventType, nameof(eventType));

			this.workflowItems = workflowItems;
			this.estimateActual = estimateActual;
			this.eventType = eventType;
		}

		readonly Event eventType;
		readonly EstimateActual estimateActual;
		readonly ProcessTaskCollection workflowItems;

		ProcessTask FindMilestone() => workflowItems.Milestones[eventType.Code];

		public void SetProperty(ZDateTimeOffset value) => Property = value;

		public ZDateTimeOffset Property
		{
			get
			{
				if (estimateActual == EstimateActual.Actual)
				{
					return FindMilestone()?.P9_ActualDateInternal ?? ZDateTimeOffset.Empty;
				}
				else
				{
					return FindMilestone()?.P9_ScheduledDateForBinding ?? ZDateTimeOffset.Empty;
				}
			}
			set
			{
				var milestone = FindMilestone();
				if (milestone != null)
				{
					if (estimateActual == EstimateActual.Actual)
					{
						if (milestone.P9_ActualDateInternal != value)
						{
							milestone.P9_ActualDateInternal = value;
						}
					}
					else
					{
						if (milestone.P9_ScheduledDateForBinding != value)
						{
							milestone.P9_ScheduledDateForBinding = value;
						}
					}
				}
			}
		}
	}
}
