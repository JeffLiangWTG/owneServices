using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Workflow.Business.Test
{
	class ReadOnlyUniversalValidationRuleSetTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var helper = new RuleHelper(Factory);
			var ruleSet = helper.CreateRuleSet(DataContextType.ForwardingShipment);
			helper.AddRule(ruleSet, "<ux.ActualChargeable> = 1");
			helper.AddRule(ruleSet, "<ux.ActualChargeable> = 2");
			Factory.Save();

			var actual = new ReadOnlyUniversalValidationRuleSet(ruleSet, ruleSet.Rules);
			AssertEquals(ruleSet.VRS_AlwaysApply, actual.AlwaysApply);
			AssertEquals(ruleSet.VRS_Code, actual.Code);
			AssertEquals(ruleSet.VRS_Criteria, actual.Criteria);
			AssertEquals(ruleSet.VRS_DataContext, actual.DataContext);
			AssertEquals(ruleSet.VRS_Description, actual.Description);
			AssertEquals(ruleSet.VRS_IsActive, actual.IsActive);
			AssertEquals(ruleSet.VRS_Name, actual.Name);

			AssertEquals(2, actual.ActiveRules.Count());
			var rule1 = actual.ActiveRules.First(x => x.Sequence == 1);
			var rule2 = actual.ActiveRules.First(x => x.Sequence == 2);
			AssertEquals("<ux.ActualChargeable> = 1", rule1.BusinessRule);
			AssertEquals("<ux.ActualChargeable> = 2", rule2.BusinessRule);
		}
	}
}
