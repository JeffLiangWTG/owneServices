using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business
{
	public class FullWorkflowDescriptorList : FullWorkflowDescriptorListAuto, IFullWorkflowDescriptorList
	{
		public FullWorkflowDescriptorList()
		{
			WorkflowDescriptorDynamicFilters.EnsureProductivitiyWiseAndTestCompatibility(this, _ => true);
		}
	}
}
