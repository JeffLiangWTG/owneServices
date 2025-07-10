using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(UniversalValidationRuleSet))]
	sealed class UniversalValidationRuleSetTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOnSaving_CodeIsSetFromNumberFountain()
		{
			var ruleSet1 = Factory.NewWithValidTestData<UniversalValidationRuleSet>();
			ruleSet1.VRS_Code = ZString.Empty;
			Factory.Save();
			AssertEquals("R001", ruleSet1.VRS_Code);
			var ruleSet2 = Factory.NewWithValidTestData<UniversalValidationRuleSet>();
			ruleSet2.VRS_Code = ZString.Empty;
			Factory.Save();
			AssertEquals("R002", ruleSet2.VRS_Code);
		}

		public void TestVRS_Criteria_ReadOnly()
		{
			var ruleSet1 = Factory.NewWithValidTestData<UniversalValidationRuleSet>();
			ruleSet1.VRS_AlwaysApply = true;
			AssertEquals("criteria is readonly when AlwaysApply", true, ruleSet1.VRS_CriteriaInfo.ReadOnly);

			ruleSet1.VRS_AlwaysApply = false;
			AssertEquals("criteria is not readonly when not AlwaysApply", false, ruleSet1.VRS_CriteriaInfo.ReadOnly);
		}
	}
}
