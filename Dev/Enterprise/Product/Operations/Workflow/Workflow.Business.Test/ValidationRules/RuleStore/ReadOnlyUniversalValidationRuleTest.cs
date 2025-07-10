using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Workflow.Business.Test
{
	class ReadOnlyUniversalValidationRuleTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var helper = new RuleHelper(Factory);
			var ruleSet = helper.CreateRuleSet(DataContextType.ForwardingShipment);
			var rule1 = helper.AddRule(ruleSet, "<ux.ActualChargeable> = 1");
			Factory.Save();

			var actual = new ReadOnlyUniversalValidationRule(rule1);
			AssertEquals(rule1.VR_BusinessRule, actual.BusinessRule);
			AssertEquals(rule1.VR_IsActive, actual.IsActive);
			AssertEquals(rule1.VR_MessageLog, actual.MessageLog);
			AssertEquals(rule1.VR_Sequence, actual.Sequence);
			AssertEquals(rule1.VR_Status, actual.Status);
		}
	}
}
