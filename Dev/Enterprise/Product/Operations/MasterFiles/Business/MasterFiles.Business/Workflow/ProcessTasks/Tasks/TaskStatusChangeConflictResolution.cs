using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class TaskStatusChangeConflictResolution : ITaskStatusChangeConflictResolution
	{
		public static TaskStatusChangeConflictResolution Continue() => new TaskStatusChangeConflictResolution();
		public static TaskStatusChangeConflictResolution SelectAlternative(IProcessTask task) => new TaskStatusChangeConflictResolution(task);

		TaskStatusChangeConflictResolution() => CancelUpdate = false;

		TaskStatusChangeConflictResolution(IProcessTask taskToStart)
		{
			CancelUpdate = true;
			AlternativeUpdate = taskToStart;
		}

		public bool CancelUpdate { get; }
		public IProcessTask AlternativeUpdate { get; }
	}
}
