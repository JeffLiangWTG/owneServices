using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business
{
	public class WorkflowDescriptorList : WorkflowDescriptorListAuto, IWorkflowDescriptorList
	{
		public WorkflowDescriptorList()
		{
			WorkflowDescriptorDynamicFilters.EnsureProductivitiyWiseAndTestCompatibility(this, w => w.SupportsWorkflowTypeFilters);
		}
	}
}
