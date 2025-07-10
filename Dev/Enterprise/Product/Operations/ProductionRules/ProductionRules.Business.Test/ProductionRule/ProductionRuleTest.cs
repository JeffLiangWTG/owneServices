namespace Enterprise.ProductionRules.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ProductionRule))]
	class ProductionRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRuleSet()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A", context: "PWP", contextSubType: "SUB");
			ruleSet.PRS_Context = "TST";

			var rule = Helper.CreateRule(ruleSet, "Rule1", "Desc", 42);
			rule.PRL_RuleDefinition = "DEFINITION";

			AssertEquals(ruleSet, rule.RuleSet);
		}

		public void TestProductionRuleWrapper()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A", context: "PWP", contextSubType: "SUB");
			ruleSet.PRS_Context = "TST";

			var rule = Helper.CreateRule(ruleSet, "Rule1", "Desc", 42);
			rule.PRL_RuleDefinition = "DEFINITION";

			var ruleWrapper = rule.GetProductionRuleWrapper();
			AssertEquals(nameof(ruleWrapper.Definition), rule.PRL_RuleDefinition, ruleWrapper.Definition);
			AssertEquals(nameof(ruleWrapper.Description), rule.PRL_Description, ruleWrapper.Description);
			AssertEquals(nameof(ruleWrapper.Name), rule.PRL_Name, ruleWrapper.Name);
			AssertEquals(nameof(ruleWrapper.Priority), rule.PRL_Priority, ruleWrapper.Priority);
			AssertEquals(nameof(ruleWrapper.Context), ruleSet.PRS_Context, ruleWrapper.Context);
			AssertEquals(nameof(ruleWrapper.ContextSubType), ruleSet.PRS_ContextSubType, ruleWrapper.ContextSubType);
		}

		public void TestProductionRuleWrapper_NullRuleSet()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A");
			ruleSet.PRS_Context = "TST";

			var rule = Helper.CreateRule(ruleSet, "Rule1", "Desc", 42);
			rule.PRL_RuleDefinition = "DEFINITION";

			var ruleWrapper1 = rule.GetProductionRuleWrapper();
			AssertEquals("Precondition.", ruleSet.PRS_Context, ruleWrapper1.Context);

			rule.PRL_PRS_RuleSet = ZGuid.Empty;
			var ruleWrapper2 = rule.GetProductionRuleWrapper();
			AssertEquals("Should return empty if there is no rule set.", string.Empty, ruleWrapper2.Context);
			AssertEquals("Should return empty if there is no rule set.", string.Empty, ruleWrapper2.ContextSubType);
		}

		protected override BusinessObject GetNewBusinessObject() => GetProductionRuleForBaseTests(Helper);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetProductionRuleForBaseTests(new Helper(factory));
		static ProductionRule GetProductionRuleForBaseTests(Helper helper) => helper.CreateRule(helper.CreateRuleSet("A", "A", false), "A1", "A1");

		Helper Helper => helper ?? (helper = new Helper(Factory));
		Helper helper;
	}
}
