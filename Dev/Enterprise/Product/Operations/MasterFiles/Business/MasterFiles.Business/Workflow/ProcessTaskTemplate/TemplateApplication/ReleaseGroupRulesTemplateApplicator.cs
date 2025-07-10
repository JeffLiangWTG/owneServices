using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	sealed class ReleaseGroupRulesTemplateApplicator : IWorkflowTemplateApplicator
	{
		internal ReleaseGroupRulesTemplateApplicator(IWorkflowProvider workflowProvider)
		{
			this.workflowProvider = Argument.NotNull(workflowProvider, nameof(workflowProvider));
		}

		readonly IWorkflowProvider workflowProvider;

		IEnumerable<CreateItemsFromTemplateResult> IWorkflowTemplateApplicator.ApplyTemplates(IEnumerable<ProcessTaskTemplate> templates, TemplateApplicationParameters parameters)
		{
			var jobLevelWorkflow = ProcessJobHeaderProvider.GetForParent(workflowProvider, ((IBusiness)workflowProvider).Factory, addDefaultProcessHeaderIfNone: false, checkTemplates: false);

			jobLevelWorkflow?.ApplyReleaseGroupRules();

			return new[] { new CreateItemsFromTemplateResult(shouldPreventFallback: false) };
		}
	}
}
