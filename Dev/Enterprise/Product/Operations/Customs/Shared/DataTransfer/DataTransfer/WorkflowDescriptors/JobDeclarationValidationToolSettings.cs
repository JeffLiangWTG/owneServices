using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business;

sealed class JobDeclarationValidationToolSettings : ValidationToolSettings
{
	public JobDeclarationValidationToolSettings(JobDeclarationWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
	{
	}

	protected override bool IsValidationRulesAvailableForGlobalTemplatesCore() => false;

	protected override CodeDescriptionPairList GetProcessTemplateValidationCondition1ListCore()
	{
		var result = new JobDeclarationWorkflowCondition1CodeList();
		if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
		{
			result.AddPair(JobDeclarationWorkflowDescriptor.ImportOnlyForCanadaCode, JobDeclarationWorkflowDescriptor.ImportOnlyForCanadaDescription);
		}
		return result;
	}

	protected override CodeDescriptionPairList GetProcessTemplateValidationCondition2ListCore()
	{
		var result = new JobDeclarationWorkflowCondition2CodeList();
		if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
		{
			result.AddPair(JobDeclarationWorkflowDescriptor.ImportOnlyForCanadaCode, JobDeclarationWorkflowDescriptor.ImportOnlyForCanadaDescription);
		}
		return result;
	}

	protected override MasterFiles.Business.ProcessTemplateValidationConditionChecker GetProcessTemplateValidationConditionCheckerCore(IProcessTemplateValidation validationRule, IBusiness businessEntity)
	{
		return businessEntity is BaseJobDeclaration declaration ? new ProcessTemplateValidationConditionChecker(validationRule, declaration) : null;
	}
}
