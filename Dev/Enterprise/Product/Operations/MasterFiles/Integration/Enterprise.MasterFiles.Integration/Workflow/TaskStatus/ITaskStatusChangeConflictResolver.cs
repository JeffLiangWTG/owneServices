using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface ITaskStatusChangeConflictResolver
	{
		ITaskStatusChangeConflictResolution GetExistingWorkTaskExists(IProcessTask task);
	}

	public interface ITaskStatusChangeConflictResolution
	{
		bool CancelUpdate { get; }
		IProcessTask AlternativeUpdate { get; }
	}
}
