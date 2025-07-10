
namespace Enterprise.Tracking.Business
{
	public interface IMilestonesProvider
	{
		TrackingMilestoneCollection Milestones { get; }
		TrackingMilestoneCollection EditableMilestones { get; }
		void ReloadMilestones();
	}
}
