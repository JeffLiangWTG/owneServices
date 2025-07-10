using System.Collections.Generic;

namespace Enterprise.Tracking.Business
{
	public interface IUpdatableMilestoneEventsProvider
	{
		List<string> UpdatableMilestoneEventCodes { get; }
	}
}
