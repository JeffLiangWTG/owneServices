using Enterprise.Integration;

namespace Enterprise.Warehouse.Integration
{
	public interface ITaskPlanningJobWithExternalTasks : ITaskPlanningJob
	{
		IProcessTask[] GetRelatedProcessTasks();
	}
}
