using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusPermitRuleValidationTest : SharedCusPermitRuleValidationTest<BaseCusPermitRuleValidation, BaseCusPermitRule>
	{
		public void TestCheckCPR_RuleCode_ListValidation()
		{
			PermitHeader.CPH_RN_NKCountryCode = "ZA";
			PermitRule.CPR_RuleCode = "ZZZ";
			AssertHasErrorContaining(PermitRule.CPR_RuleCodeInfo, ListValidation.InvalidCodeError);
			PermitRule.CPR_RuleCode = "TAR";
			AssertNoErrorContaining(PermitRule.CPR_RuleCodeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckCPR_ValueFrom()
		{
			PermitRule.CPR_ValueFrom = ZString.Empty;
			AssertHasErrorContaining(PermitRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);
			PermitRule.CPR_ValueFrom = "1";
			AssertNoErrorContaining(PermitRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCPR_ValueTo()
		{
			PermitRule.CPR_ValueTo = ZString.Empty;
			AssertNoErrorContaining(PermitRule.CPR_ValueToInfo, BaseCusPermitRuleValidation.ValueToMustBeGreaterThanOrEqualValueFrom);
			PermitRule.CPR_ValueFrom = "5";
			PermitRule.CPR_ValueTo = "4";
			AssertHasErrorContaining(PermitRule.CPR_ValueToInfo, BaseCusPermitRuleValidation.ValueToMustBeGreaterThanOrEqualValueFrom);
			PermitRule.CPR_ValueTo = "5";
			AssertNoErrorContaining(PermitRule.CPR_ValueToInfo, BaseCusPermitRuleValidation.ValueToMustBeGreaterThanOrEqualValueFrom);
		}

		#region Implementation

		protected override BaseCusPermitRule GetNewRule(BusinessObjectFactory factory)
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			var permitRule = permitHeader.CusPermitRules.AddNew();
			permitRule.FillWithValidTestData();
			return permitRule;
		}

		#endregion
	}
}
