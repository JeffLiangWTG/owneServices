using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IWorkflowTriggerActionSource
	{
		BusinessObject Job { get; }
		IBaseTrigger Trigger { get; }
		IProcessTaskNotification Action { get; }
		IStmALog Event { get; }
	}
}
