namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public interface IHasChangesTracker
	{
		bool GetHasChanges();
		void EnableTrackingHasChanges();
	}
}
