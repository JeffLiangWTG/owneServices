using Enterprise.ZArchitecture.Business;
namespace Enterprise.Tracking.Business
{
	public interface ITrackingEventsProvider : IStmALogParent
	{
		StmALogCollection TrackingEvents { get; }
		bool CanViewTrackingEvents { get; }
	}
}
