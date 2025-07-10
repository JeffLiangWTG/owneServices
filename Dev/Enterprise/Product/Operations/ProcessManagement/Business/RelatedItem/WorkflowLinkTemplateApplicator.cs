using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkflowLinkTemplateApplicator : IWorkflowLinkTemplateApplicator
	{
		internal WorkflowLinkTemplateApplicator(IWorkflowProvider workflowProvider)
		{
			this.workflowProvider = Argument.NotNull(workflowProvider, nameof(workflowProvider));
		}

		readonly IWorkflowProvider workflowProvider;

		public IEnumerable<CreateItemsFromTemplateResult> ApplyTemplates(IEnumerable<ProcessTaskTemplate> templates, TemplateApplicationParameters parameters = null)
		{
			var applyingTemplates = templates.ToArray();
			var factory = ((BusinessObject)workflowProvider).Factory;
			var workflowProviderLinkageService = ObjectFactory.Get<IWorkflowProvidersLinkageService>();
			if (workflowProvider is IWorkTaskRelatedItemSource relatedItemSource)
			{
				foreach (var relatedItem in relatedItemSource.RelatedItems)
				{
					if (relatedItem is IWorkflowProviderCore relatedWorkflowProvider)
					{
						workflowProviderLinkageService.WorkflowLinkedTemplatesApplied(workflowProvider, relatedWorkflowProvider, factory, applyingTemplates);
					}
				}
			}

			return new[] { new CreateItemsFromTemplateResult(shouldPreventFallback: false) };
		}
	}
}
