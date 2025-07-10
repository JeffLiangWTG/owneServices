using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface IWorkflowTriggerActionProcessorCreator
	{
		IProcessor GetSetFieldProcessor(IWorkflowTriggerActionSource source);
	}
}
