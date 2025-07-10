using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing;

sealed class ConsolidatedDeclarationValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestIsValidationRulesAvailableForGlobalTemplates()
	{
		AssertEquals(false, ValidationToolSettings.IsValidationRulesAvailableForGlobalTemplates);
	}

	public void TestGetProcessTemplateValidationCondition1List()
	{
		var list = ValidationToolSettings.GetProcessTemplateValidationCondition1List();
		Assert(list.ContainsOnly("IMP"));
	}

	public void TestGetProcessTemplateValidationDefaultCondition1()
	{
		var condition1 = ValidationToolSettings.GetProcessTemplateValidationDefaultCondition1();
		AssertEquals("Default From Consolidated Declaration", "IMP", condition1);
	}

	ConsolidatedDeclarationWorkflowDescriptor WorkflowDescriptor => workflowDescriptor ??= new ConsolidatedDeclarationWorkflowDescriptor();
	ConsolidatedDeclarationWorkflowDescriptor workflowDescriptor;

	ConsolidatedDeclarationValidationToolSettings ValidationToolSettings => validationToolSettings ??= new ConsolidatedDeclarationValidationToolSettings(WorkflowDescriptor);
	ConsolidatedDeclarationValidationToolSettings validationToolSettings;
}
