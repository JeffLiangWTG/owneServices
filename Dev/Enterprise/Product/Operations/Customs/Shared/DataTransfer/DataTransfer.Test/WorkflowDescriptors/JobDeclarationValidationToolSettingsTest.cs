using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.Business.Testing;

sealed class JobDeclarationValidationToolSettingsTest : TestCaseWithFactory
{
	public void TestGetProcessTemplateValidationConditionChecker() => CombineAssertions(() =>
	{
		AssertNull("Unknown Entity", ValidationToolSettings.GetProcessTemplateValidationConditionChecker(Mock.Of<IProcessTemplateValidation>(), Mock.Of<IBusiness>()));
		AssertType<ProcessTemplateValidationConditionChecker>("JobDeclaration", ValidationToolSettings.GetProcessTemplateValidationConditionChecker(Mock.Of<IProcessTemplateValidation>(), Factory.New<BaseJobDeclaration>()));
	});

	public void TestIsValidationRulesAvailableForGlobalTemplates()
	{
		AssertEquals(false, ValidationToolSettings.IsValidationRulesAvailableForGlobalTemplates);
	}

	public void TestGetProcessTemplateValidationCondition1List()
	{
		AssertType<JobDeclarationWorkflowCondition1CodeList>(ValidationToolSettings.GetProcessTemplateValidationCondition1List());
		using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Canada))
		{
			var workflowDescriptor = new JobDeclarationWorkflowDescriptor();
			var validationToolSettings = new JobDeclarationValidationToolSettings(workflowDescriptor);
			Assert(validationToolSettings.GetProcessTemplateValidationCondition1List().ContainsCode(JobDeclarationWorkflowDescriptor.ImportOnlyForCanadaCode));
		}
	}

	public void TestGetProcessTemplateValidationCondition2List()
	{
		AssertType<JobDeclarationWorkflowCondition2CodeList>(ValidationToolSettings.GetProcessTemplateValidationCondition2List());
		using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Canada))
		{
			var workflowDescriptor = new JobDeclarationWorkflowDescriptor();
			var validationToolSettings = new JobDeclarationValidationToolSettings(workflowDescriptor);
			Assert(validationToolSettings.GetProcessTemplateValidationCondition2List().ContainsCode(JobDeclarationWorkflowDescriptor.ImportOnlyForCanadaCode));
		}
	}

	JobDeclarationWorkflowDescriptor WorkflowDescriptor => workflowDescriptor ??= new JobDeclarationWorkflowDescriptor();
	JobDeclarationWorkflowDescriptor workflowDescriptor;

	JobDeclarationValidationToolSettings ValidationToolSettings => validationToolSettings ??= new JobDeclarationValidationToolSettings(WorkflowDescriptor);
	JobDeclarationValidationToolSettings validationToolSettings;
}
