using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business;

public class ValidationToolSettings(WorkflowDescriptor workflowDescriptor)
{
	protected WorkflowDescriptor WorkflowDescriptor { get; } = workflowDescriptor;

	public bool SupportsValidationRules => SupportsValidationRulesCore();

	protected virtual bool SupportsValidationRulesCore() => WorkflowDataRegistry.Instance.EnableWorkflowValidation.Value.Contains(WorkflowDescriptor.Code);

	public bool IsValidationRulesAvailableForGlobalTemplates => IsValidationRulesAvailableForGlobalTemplatesCore();

	protected virtual bool IsValidationRulesAvailableForGlobalTemplatesCore() => true;

	public Type GetProcessTemplateValidationRootObjectType(ZString condition1, ZString countryCode) => GetProcessTemplateValidationRootObjectTypeCore(condition1, countryCode);

	protected virtual Type GetProcessTemplateValidationRootObjectTypeCore(ZString condition1, ZString countryCode)
	{
		var parentType = WorkflowDescriptor.WorkflowProviderType;
		if (TypeDecider.GetTypeDeciderFromType(parentType) is CountrySpecificTypeDecider countryTypeDecider)
		{
			parentType = countryTypeDecider.GetTypeForCountryCode(countryCode);
		}

		return parentType;
	}

	public CodeDescriptionPairList GetProcessTemplateValidationCondition1List() => GetProcessTemplateValidationCondition1ListCore();

	protected virtual CodeDescriptionPairList GetProcessTemplateValidationCondition1ListCore() => new CodeDescriptionPairList();

	public CodeDescriptionPairList GetProcessTemplateValidationCondition2List() => GetProcessTemplateValidationCondition2ListCore();

	protected virtual CodeDescriptionPairList GetProcessTemplateValidationCondition2ListCore() => new CodeDescriptionPairList();

	public CodeDescriptionPairList GetProcessTemplateValidationActionSourceList(ZString countryCode) => GetProcessTemplateValidationActionSourceListCore(countryCode);

	protected virtual CodeDescriptionPairList GetProcessTemplateValidationActionSourceListCore(ZString countryCode)
	{
		var result = new CodeDescriptionPairList();
		result.AddRange(new ProcessTemplateValidationActionSourceList());
		var actionSourceList = (CodeDescriptionPairList)ObjectFactory.Get<IValidationToolActionSourceListProvider>().GetActionSourceList(WorkflowDescriptor.ControllerID, countryCode);
		result.AddRange(actionSourceList);
		return result;
	}

	public ZString GetProcessTemplateValidationDefaultCondition1() => GetProcessTemplateValidationDefaultCondition1Core();

	protected virtual ZString GetProcessTemplateValidationDefaultCondition1Core() => ZString.Empty;

	public ProcessTemplateValidationConditionChecker GetProcessTemplateValidationConditionChecker(IProcessTemplateValidation rule, IBusiness businessEntity) => GetProcessTemplateValidationConditionCheckerCore(rule, businessEntity);

	protected virtual ProcessTemplateValidationConditionChecker GetProcessTemplateValidationConditionCheckerCore(IProcessTemplateValidation rule, IBusiness businessEntity) => new ProcessTemplateValidationConditionChecker(rule, businessEntity);

	public virtual string RequestTypeJobType => string.Empty;
}
