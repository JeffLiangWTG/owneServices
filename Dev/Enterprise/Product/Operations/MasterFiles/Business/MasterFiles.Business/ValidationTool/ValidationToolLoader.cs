using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business;

sealed class ValidationToolLoader
{
	readonly IBusiness job;

	readonly Lazy<IReadOnlyList<ProcessTaskTemplate>> matchedTemplates;

	readonly Lazy<IReadOnlyList<ProcessTemplateValidation>> matchedRules;

	public ValidationToolLoader(IBusiness job, BusinessObjectFactory factory = null)
	{
		this.job = job;
		Factory = factory ?? job?.Factory;
		matchedTemplates = new Lazy<IReadOnlyList<ProcessTaskTemplate>>(GetMatchedTemplate);
		matchedRules = new Lazy<IReadOnlyList<ProcessTemplateValidation>>(() => MatchedTemplates.SelectMany(GetMatchedRules).ToList());
	}

	public BusinessObjectFactory Factory { get; }

	public IReadOnlyList<ProcessTaskTemplate> MatchedTemplates => matchedTemplates.Value;

	public IReadOnlyList<ProcessTemplateValidation> MatchedRules => matchedRules.Value;

	public IReadOnlyList<ProcessTemplateValidation> GetMatchedRules(ZString actionSourceCode)
	{
		if (actionSourceCode.IsEmpty)
		{
			return [];
		}
		return MatchedRules.Where(x => x.HasValidationAction(actionSourceCode)).ToList();
	}

	public IReadOnlyList<IGrouping<ProcessTaskTemplate, ProcessTemplateValidation>> GetMatchedRuleGroupings(ZString actionSourceCode)
	{
		if (actionSourceCode.IsEmpty)
		{
			return [];
		}
		return GetMatchedRules(actionSourceCode).GroupBy(x => x.WorkflowTemplate).ToList();
	}

	IReadOnlyList<ProcessTaskTemplate> GetMatchedTemplate()
	{
		if (job is not IWorkflowProvider workflowProvider || Factory is null)
		{
			return [];
		}

		var isAvailableForGlobalTemplates = WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType) is { } workflowDescriptor && workflowDescriptor.ValidationToolSettings.IsValidationRulesAvailableForGlobalTemplates;
		var candidateTemplates = ProcessTask.Loader.LoadTemplateMatches(workflowProvider, Factory).Where(x => isAvailableForGlobalTemplates || !x.GlobalTemplate);
		return new TemplateFallbackIterator(candidateTemplates, TemplateEntityType.ValidationTool, HasItemsAppliedFromTemplate).Where(x => x.IsApplicableToJob).Select(x => x.Template).ToArray();

		bool HasItemsAppliedFromTemplate(ProcessTaskTemplate templateMatch, IWorkflowItemCollection workflowItemCollection) => GetMatchedRules(templateMatch).Any();
	}

	IEnumerable<ProcessTemplateValidation> GetMatchedRules(ProcessTaskTemplate templateMatch)
	{
		if (templateMatch is null)
		{
			yield break;
		}

		var rules = templateMatch.ProcessTemplateValidationActions
			.Select(x => x.ValidationRule)
			.WhereNotNull().Where(x =>
			{
				var checker = x.GetValidationToolChecker(job);
				var areConditionsMet = checker?.AreConditionsMet() ?? false;
				return areConditionsMet;
			}).Distinct();
		foreach (var rule in rules)
		{
			yield return rule;
		}
	}
}
