using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTemplateValidationActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckP0A_ActionSource_MustBeEntered() => CombineAssertions(() =>
		{
			const string message = "Please enter an Action Source";
			TemplateValidationAction.Validation.ValidateP0A_ActionSource();
			AssertHasErrorContaining("Not entered", TemplateValidationAction.P0A_ActionSourceInfo, message);

			TemplateValidationAction.P0A_ActionSource = ProcessTemplateValidationActionSourceList.Codes.Save;
			AssertNoErrorContaining("Entered", TemplateValidationAction.P0A_ActionSourceInfo, message);
		});

		public void TestCheckP0A_ActionSource_ValidCode() => CombineAssertions(() =>
		{
			TemplateValidationAction.P0A_ActionSource = "!@#";
			AssertListValidationInvalidCodeError(TemplateValidationAction.P0A_ActionSourceInfo, true);

			Template.P0_ProcessType = "SHP";
			TemplateValidationAction.P0A_ActionSource = ProcessTemplateValidationActionSourceList.Codes.Save;
			AssertListValidationInvalidCodeError(TemplateValidationAction.P0A_ActionSourceInfo, false);
		});

		public void TestCheckP0A_ActionSource_Unique() => CombineAssertions(() =>
		{
			const string message = "Action source must be unique for each validation rule.";
			var templateValidationAction2 = TemplateValidation.ProcessTemplateValidationActions.AddNew();
			TemplateValidationAction.P0A_ActionSource = "SAV";
			AssertNoError("Single", TemplateValidationAction.P0A_ActionSourceInfo, message);
			templateValidationAction2.P0A_ActionSource = "SAV";
			AssertHasError("Duplicate", templateValidationAction2.P0A_ActionSourceInfo, message);
			templateValidationAction2.P0A_ActionSource = "XXX";
			AssertNoError("Unique", templateValidationAction2.P0A_ActionSourceInfo, message);
		});

		ProcessTaskTemplate Template => template ??= Factory.New<ProcessTaskTemplate>();
		ProcessTaskTemplate template;

		ProcessTemplateValidation TemplateValidation => templateValidation ??= Template.ProcessTemplateValidations.AddNew();
		ProcessTemplateValidation templateValidation;

		ProcessTemplateValidationAction TemplateValidationAction => templateValidationAction ??= TemplateValidation.ProcessTemplateValidationActions.AddNew();
		ProcessTemplateValidationAction templateValidationAction;
	}
}
