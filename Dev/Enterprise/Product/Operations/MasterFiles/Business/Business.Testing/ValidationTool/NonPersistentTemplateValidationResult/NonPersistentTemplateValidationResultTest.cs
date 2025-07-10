using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(NonPersistentTemplateValidationResult))]
sealed class NonPersistentTemplateValidationResultTest : NonPersistentBusinessObjectTestCase
{
	public void TestTotalMatchedRules() => CombineAssertions(() =>
	{
		var actionValidationResult = new NonPersistentTemplateValidationResult(Factory, "SAV", "On Save");
		AssertEquals("Default", 0, actionValidationResult.TotalMatchedRules);

		actionValidationResult.RuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>()).Build());
		actionValidationResult.RuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>()).Build());
		AssertEquals("Added", 2, actionValidationResult.TotalMatchedRules);
	});

	public void TestFailedRulesNumber() => CombineAssertions(() =>
	{
		var actionValidationResult = new NonPersistentTemplateValidationResult(Factory, "SAV", "On Save");
		AssertEquals("Default", 0, actionValidationResult.FailedRulesNumber);

		actionValidationResult.RuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>()).Build());
		actionValidationResult.RuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>()).Build());
		AssertEquals("Added", 2, actionValidationResult.FailedRulesNumber);
	});

	public void TestPassed() => CombineAssertions(() =>
	{
		var actionValidationResult = new NonPersistentTemplateValidationResult(Factory, "SAV", "On Save");
		AssertEquals("Default", true, actionValidationResult.Passed);

		actionValidationResult.RuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>()).Build());
		actionValidationResult.RuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>()).Build());
		AssertEquals("Added", false, actionValidationResult.Passed);
	});

	public void TestRuleResults()
	{
		var actionValidationResult = new NonPersistentTemplateValidationResult(Factory, "SAV", "On Save");
		var ruleValidationResult1 = NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>()).Build();
		actionValidationResult.RuleResults.Add(ruleValidationResult1);
		var ruleValidationResult2 = NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>()).Build();
		actionValidationResult.RuleResults.Add(ruleValidationResult2);
		AssertContainsExactElementsInAnyOrder([ruleValidationResult1, ruleValidationResult2], actionValidationResult.RuleResults);
	}

	public void TestFailedRuleResults()
	{
		var actionValidationResult = new NonPersistentTemplateValidationResult(Factory, "SAV", "On Save");
		var ruleValidationResult1 = NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>()).Build();
		actionValidationResult.RuleResults.Add(ruleValidationResult1);
		var ruleValidationResult2 = NonPersistentRuleValidationResultBuilder.Fail().WithRule(Mock.Of<IProcessTemplateValidation>()).Build();
		actionValidationResult.RuleResults.Add(ruleValidationResult2);
		var ruleValidationResult3 = NonPersistentRuleValidationResultBuilder.Pass().WithRule(Mock.Of<IProcessTemplateValidation>()).Build();
		actionValidationResult.RuleResults.Add(ruleValidationResult3);
		AssertContainsExactElementsInAnyOrder([ruleValidationResult1, ruleValidationResult2], actionValidationResult.FailedRuleResults);
	}

	protected override BusinessObject GetNewBusinessObject() => new NonPersistentTemplateValidationResult(Factory, "SAV", "On Save");
}
