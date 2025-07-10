using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business
{
	public class TemplateWorkflowDescriptorList : TemplateWorkflowDescriptorListAuto, ITemplateWorkflowDescriptorList
	{
		public TemplateWorkflowDescriptorList()
		{
			WorkflowDescriptorDynamicFilters.EnsureProductivitiyWiseAndTestCompatibility(this, w => w.SupportsWorkflowTypeFilters && (w.SupportsWorkflowTemplates || w.SupportsUniversalTemplates));
		}
	}
}
