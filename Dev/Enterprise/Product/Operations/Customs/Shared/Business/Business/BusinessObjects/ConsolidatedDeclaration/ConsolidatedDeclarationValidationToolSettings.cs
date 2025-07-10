using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business;

sealed class ConsolidatedDeclarationValidationToolSettings : ValidationToolSettings
{
	public ConsolidatedDeclarationValidationToolSettings(ConsolidatedDeclarationWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
	{
	}

	protected override bool IsValidationRulesAvailableForGlobalTemplatesCore() => false;

	protected override CodeDescriptionPairList GetProcessTemplateValidationCondition1ListCore()
	{
		var list = new CodeDescriptionPairList();
		list.AddPair(JobDeclarationWorkflowCondition1CodeList.Codes.Import, JobDeclarationWorkflowCondition1CodeList.Descriptions.Import);

		return list;
	}

	protected override ZString GetProcessTemplateValidationDefaultCondition1Core()
	{
		return JobDeclarationWorkflowCondition1CodeList.Codes.Import;
	}
}
