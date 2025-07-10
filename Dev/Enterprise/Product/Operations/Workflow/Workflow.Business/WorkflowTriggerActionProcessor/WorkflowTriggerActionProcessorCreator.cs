using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class WorkflowTriggerActionProcessorCreator : IWorkflowTriggerActionProcessorCreator
	{
		public IProcessor GetSetFieldProcessor(IWorkflowTriggerActionSource source)
		{
			return new WorkflowSetFieldProcessor(source);
		}
	}
}
