using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public interface IWorkflowTemplateApplicator
	{
		IEnumerable<CreateItemsFromTemplateResult> ApplyTemplates(IEnumerable<ProcessTaskTemplate> templates, TemplateApplicationParameters parameters = null);
	}
}
