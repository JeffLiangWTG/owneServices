using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing;

sealed class ValidationToolLoaderTest : TestCaseWithFactory
{
	public void TestFactory() => CombineAssertions(() =>
	{
		AssertNull("null job", new ValidationToolLoader(null).Factory);
		var dummyObject = Factory.New<DummyWithWorkflow>();
		AssertSame("job.Factory", dummyObject.Factory, new ValidationToolLoader(dummyObject).Factory);
		var newFactory = new BusinessObjectFactory();
		AssertSame("Factory can be specified", newFactory, new ValidationToolLoader(dummyObject, newFactory).Factory);
	});

	public void TestMatchedTemplates() => CombineAssertions(() =>
	{
		AssertEquals("null job", 0, new ValidationToolLoader(null).MatchedTemplates.Count);
		AssertEquals("Job is not a IWorkflowProvider", 0, new ValidationToolLoader(Mock.Of<IBusiness>()).MatchedTemplates.Count);

		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "T1";
		template1.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		var template2 = Factory.New<ProcessTaskTemplate>();
		template2.P0_Name = "T2";
		template2.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		var template3 = Factory.New<ProcessTaskTemplate>();
		template3.P0_Name = "T3";
		template3.P0_ProcessType = "BRK";

		Factory.Save();

		var dummyObject = Factory.New<DummyWithWorkflow>();
		var matchedTemplates = new ValidationToolLoader(dummyObject).MatchedTemplates;
		AssertContainsExactElementsInAnyOrder([template1, template2], matchedTemplates);
	});

	public void TestMatchedTemplates_ExcludeGlobalTemplates()
	{
		var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
		{
			IsValidationRulesAvailableForGlobalTemplatesForTest = false
		};
		DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);

		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "T1";
		template1.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		var template2 = Factory.New<ProcessTaskTemplate>();
		template2.P0_Name = "T2";
		template2.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		template2.GlobalTemplate = true;
		var template4 = Factory.New<ProcessTaskTemplate>();
		template4.P0_Name = "T3";
		template4.P0_ProcessType = "BRK";

		Factory.Save();

		var dummyObject = Factory.New<DummyWithWorkflow>();
		var matchedTemplates = new ValidationToolLoader(dummyObject).MatchedTemplates;
		AssertContainsExactElementsInAnyOrder([template1], matchedTemplates);
	}

	public void TestMatchedRules_AlwaysFallback() => CombineAssertions(() =>
	{
		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "VWG";
		template1.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		template1.P0_ValidationFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
		var template2 = Factory.New<ProcessTaskTemplate>();
		template2.P0_Name = "T2";
		template2.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		template2.P0_ValidationFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
		var template3 = Factory.New<ProcessTaskTemplate>();
		template3.P0_Name = "T3";
		template3.P0_ProcessType = "BRK";

		var templateValidation11 = template1.ProcessTemplateValidations.AddNew();
		templateValidation11.P0V_Description = "Rule 11";
		templateValidation11.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		templateValidation11.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "FSV";
		var templateValidation12 = template1.ProcessTemplateValidations.AddNew();
		templateValidation12.P0V_Description = "Rule 12";
		templateValidation12.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		var templateValidation21 = template2.ProcessTemplateValidations.AddNew();
		templateValidation21.P0V_Description = "Rule 21";
		templateValidation21.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";

		Factory.Save();

		var dummyObject = Factory.New<DummyWithWorkflow>();
		var matchedRules = new ValidationToolLoader(dummyObject).MatchedRules;
		AssertContainsExactElementsInAnyOrder([templateValidation11, templateValidation12, templateValidation21], matchedRules);
	});

	public void TestMatchedRules_EmptyFallback() => CombineAssertions(() =>
	{
		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "VWG";
		template1.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		template1.P0_SubType1 = "AIR";
		template1.P0_LoadPortCountry = "AU";
		var template2 = Factory.New<ProcessTaskTemplate>();
		template2.P0_Name = "T2";
		template2.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		template2.P0_SubType1 = "AIR";
		var template3 = Factory.New<ProcessTaskTemplate>();
		template3.P0_Name = "T3";
		template3.P0_ProcessType = "BRK";

		var templateValidation21 = template2.ProcessTemplateValidations.AddNew();
		templateValidation21.P0V_Description = "Rule 11";
		templateValidation21.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		templateValidation21.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "FSV";
		var templateValidation22 = template2.ProcessTemplateValidations.AddNew();
		templateValidation22.P0V_Description = "Rule 12";
		templateValidation22.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		var templateValidation23 = template2.ProcessTemplateValidations.AddNew();
		templateValidation23.P0V_Description = "Rule 21";
		templateValidation23.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";

		Factory.Save();

		var dummyObject = Factory.New<DummyWithWorkflow>();
		dummyObject.SubType1 = "AIR";
		dummyObject.LoadPort = "AUSYD";
		var matchedRules = new ValidationToolLoader(dummyObject).MatchedRules;
		AssertContainsExactElementsInAnyOrder([templateValidation21, templateValidation22, templateValidation23], matchedRules);
	});

	public void TestMatchedRules_NeverCallback() => CombineAssertions(() =>
	{
		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "VWG";
		template1.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		template1.P0_SubType1 = "AIR";
		template1.P0_LoadPortCountry = "AU";
		template1.P0_ValidationFallbackMethod = FallbackTypeList.Codes.NeverFallback;
		var template2 = Factory.New<ProcessTaskTemplate>();
		template2.P0_Name = "T2";
		template2.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		template2.P0_SubType1 = "AIR";
		var template3 = Factory.New<ProcessTaskTemplate>();
		template3.P0_Name = "T3";
		template3.P0_ProcessType = "BRK";

		var templateValidation21 = template2.ProcessTemplateValidations.AddNew();
		templateValidation21.P0V_Description = "Rule 11";
		templateValidation21.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		templateValidation21.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "FSV";
		var templateValidation22 = template2.ProcessTemplateValidations.AddNew();
		templateValidation22.P0V_Description = "Rule 12";
		templateValidation22.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		var templateValidation23 = template2.ProcessTemplateValidations.AddNew();
		templateValidation23.P0V_Description = "Rule 21";
		templateValidation23.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";

		Factory.Save();

		var dummyObject = Factory.New<DummyWithWorkflow>();
		dummyObject.SubType1 = "AIR";
		dummyObject.LoadPort = "AUSYD";
		var matchedRules = new ValidationToolLoader(dummyObject).MatchedRules;
		AssertEquals(0, matchedRules.Count);
	});

	public void TestGetMatchedRules() => CombineAssertions(() =>
	{
		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "T1";
		template1.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		template1.P0_ValidationFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
		var template2 = Factory.New<ProcessTaskTemplate>();
		template2.P0_Name = "T2";
		template2.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		template2.P0_ValidationFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
		var template3 = Factory.New<ProcessTaskTemplate>();
		template3.P0_Name = "T3";
		template3.P0_ProcessType = "BRK";

		var templateValidation11 = template1.ProcessTemplateValidations.AddNew();
		templateValidation11.P0V_Description = "Rule 11";
		templateValidation11.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		var templateValidation12 = template1.ProcessTemplateValidations.AddNew();
		templateValidation12.P0V_Description = "Rule 12";
		templateValidation12.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "FSV";
		var templateValidation21 = template2.ProcessTemplateValidations.AddNew();
		templateValidation21.P0V_Description = "Rule 21";
		templateValidation21.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";

		Factory.Save();

		var dummyObject = Factory.New<DummyWithWorkflow>();
		AssertEquals("ActionSourceCode not specified", 0, new ValidationToolLoader(dummyObject).GetMatchedRules(ZString.Empty).Count);
		var matchedRules = new ValidationToolLoader(dummyObject).GetMatchedRules("SAV");
		AssertContainsExactElementsInAnyOrder("ActionSourceCode specified", [templateValidation11, templateValidation21
		], matchedRules);
	});

	public void TestGetMatchedRuleGroupings() => CombineAssertions(() =>
	{
		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "T1";
		template1.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		template1.P0_ValidationFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
		var template2 = Factory.New<ProcessTaskTemplate>();
		template2.P0_Name = "T2";
		template2.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		template2.P0_ValidationFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
		var template3 = Factory.New<ProcessTaskTemplate>();
		template3.P0_Name = "T3";
		template3.P0_ProcessType = "BRK";

		var templateValidation11 = template1.ProcessTemplateValidations.AddNew();
		templateValidation11.P0V_Description = "Rule 11";
		templateValidation11.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		var templateValidation12 = template1.ProcessTemplateValidations.AddNew();
		templateValidation12.P0V_Description = "Rule 12";
		templateValidation12.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		var templateValidation21 = template2.ProcessTemplateValidations.AddNew();
		templateValidation21.P0V_Description = "Rule 21";
		templateValidation21.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";

		Factory.Save();

		var dummyObject = Factory.New<DummyWithWorkflow>();
		AssertEquals("ActionSourceCode not specified", 0, new ValidationToolLoader(dummyObject).GetMatchedRuleGroupings(ZString.Empty).Count);
		var matchedRuleGroupings = new ValidationToolLoader(dummyObject).GetMatchedRuleGroupings("SAV");
		AssertEquals("two groups matched", 2, matchedRuleGroupings.Count);
		AssertEquals("T1 with 2 matched rules", 2, matchedRuleGroupings.Single(x => x.Key.P0_Name == "T1").Count());
		AssertEquals("T2 with 1 matched rules", 1, matchedRuleGroupings.Single(x => x.Key.P0_Name == "T2").Count());
	});
}
