using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business
{
	public class BMSWorkflowDescriptorList : BMSWorkflowDescriptorListAuto, IBMSWorkflowDescriptorList
	{
		public BMSWorkflowDescriptorList()
		{
			WorkflowDescriptorDynamicFilters.EnsureProductivitiyWiseAndTestCompatibility(this, w => w.SupportsWorkflowTypeFilters && w.SupportsBufferManagement);
		}
	}
}
