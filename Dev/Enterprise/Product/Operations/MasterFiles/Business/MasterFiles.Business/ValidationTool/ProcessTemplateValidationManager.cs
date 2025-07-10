using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business;

sealed class ProcessTemplateValidationManager : IProcessTemplateValidationManager
{
	public void InjectFieldRules(IBusiness businessEntity)
	{
		if (ProcessTemplateValidationActionSourceList.FieldSpecificValidationEnabled())
		{
			new FieldToDisplayValidationInjector(businessEntity).Inject();
		}
	}

	public void ValidateOnValidateAll(IBusiness businessEntity)
	{
		Validate(ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation, businessEntity, null);
	}

	public void ValidateOnSave(IBusiness businessEntity, Action originalAction)
	{
		Validate(ProcessTemplateValidationActionSourceList.Codes.Save, businessEntity, originalAction);
	}

	public void Validate(ZString actionSourceCode, IBusiness businessEntity, Action originalAction)
	{
		if (!CheckValidationToolIsSupported(businessEntity, actionSourceCode))
		{
			originalAction?.Invoke();
			return;
		}

		var businessObject = (BusinessObject)businessEntity ;
		var newFactory = new BusinessObjectFactory { NameForDebugging = nameof(ProcessTemplateValidationManager) };
		var templateValidationResults = Validate(newFactory, actionSourceCode, businessObject);
		if (templateValidationResults.Count == 0)
		{
			originalAction?.Invoke();
			return;
		}

		var allPassed = templateValidationResults.All(x => x.Passed);
		if (allPassed)
		{
			originalAction?.Invoke();
			return;
		}

		var validationFailure = CreateValidationFailure(newFactory, templateValidationResults);
		foreach (NonPersistentRuleValidationResult failedRuleResult in validationFailure.FailedRuleResults)
		{
			failedRuleResult.DisplayValidationOnField();
			failedRuleResult.LogValidationFailEvent();
		}

		var proceed = Globals.IsUserInteractive && ObjectFactory.Get<IValidationToolDialogService>().Proceed(validationFailure);

		if (proceed)
		{
			validationFailure.HandleRequestsOnFailure();
		}

		Save(newFactory);

		if (proceed)
		{
			originalAction?.Invoke();
		}
	}

	static List<NonPersistentTemplateValidationResult> Validate(BusinessObjectFactory factory, ZString actionSourceCode, BusinessObject businessObject)
	{
		List<NonPersistentTemplateValidationResult> result = new();

		var matchedRuleGroupings = new ValidationToolLoader(businessObject, factory).GetMatchedRuleGroupings(actionSourceCode);
		foreach (var grouping in matchedRuleGroupings)
		{
			var rules = grouping.ToList();
			if (rules.Count == 0)
			{
				continue;
			}

			var actionSourceDescription = rules.First().GetValidationAction(actionSourceCode).ActionSourceDescription;
			var templateValidationResult = new NonPersistentTemplateValidationResult(factory, actionSourceCode, actionSourceDescription);
			result.Add(templateValidationResult);
			foreach (var rule in rules)
			{
				var checker = rule.GetValidationToolChecker(businessObject);
				var ruleResult = checker.EvaluateRule();
				templateValidationResult.RuleResults.Add(ruleResult);
			}
		}

		return result;
	}

	static void Save(BusinessObjectFactory factory)
	{
		try
		{
			factory.Save();
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	static NonPersistentValidationFailure CreateValidationFailure(BusinessObjectFactory factory, IEnumerable<NonPersistentTemplateValidationResult> templateValidationResults)
	{
		var failedTemplateValidationResults = templateValidationResults.Where(x => !x.Passed).ToList();
		var templateValidationResult = failedTemplateValidationResults.FirstOrDefault();
		var actionSource = templateValidationResult?.ActionSource ?? ZString.Empty;
		var actionSourceDescription = templateValidationResult?.ActionSourceDescription ?? ZString.Empty;

		var failure = new NonPersistentValidationFailure(factory, actionSource, actionSourceDescription);
		foreach (var failedRuleResult in failedTemplateValidationResults.SelectMany(failedActionValidationResult => failedActionValidationResult.FailedRuleResults))
		{
			failure.FailedRuleResults.Add(failedRuleResult);
		}

		return failure;
	}

	internal static bool CheckValidationToolIsSupported(IBusiness businessEntity, ZString actionSourceCode)
	{
		return !actionSourceCode.IsEmpty &&
				businessEntity is BusinessObject { IsInDatabase: true } and IWorkflowProvider workflowProvider &&
				WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType) is { } workflowDescriptor &&
				workflowDescriptor.ValidationToolSettings.SupportsValidationRules;
	}
}
