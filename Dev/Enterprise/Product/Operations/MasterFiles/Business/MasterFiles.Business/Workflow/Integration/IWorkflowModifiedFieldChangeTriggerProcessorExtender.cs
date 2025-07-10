using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IWorkflowModifiedFieldChangeTriggerProcessorExtender
	{
		IWorkflowTrigger[] GetTriggersToRun(StmChangeLog changeLog, string[] changedPropertyNames);
	}
}
