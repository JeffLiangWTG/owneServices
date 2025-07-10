using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class LinkedCusAuthorisationRuleLookupsTest : Customs.Business.Testing.LinkedCusAuthorisationRuleLookupsTest
	{
		public void TestRuleCodeListForMCO()
		{
			linkedCusAuthorisationRule.AuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
			var list = lookups.RuleCodeList;
			Assert("MCO rule types should allow type VCO linked rules", list.ContainsCode(LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices));
		}

		public void TestRuleCodeListForNonMCO()
		{
			linkedCusAuthorisationRule.AuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			var list = lookups.RuleCodeList;
			Assert("Non MCO rule types should not allow type VCO linked rules", !list.ContainsCode(LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var authHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;
			var authRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			authRule.CPR_CPH_PermitHeader = authHeader.PK;
			linkedCusAuthorisationRule = Factory.NewWithValidTestData<LinkedCusAuthorisationRule>();
			linkedCusAuthorisationRule.CPR_CPR_Rule = authRule.PK;
			lookups = new LinkedCusAuthorisationRuleLookups(linkedCusAuthorisationRule);
		}

		LinkedCusAuthorisationRule linkedCusAuthorisationRule;
		LinkedCusAuthorisationRuleLookups lookups;
	}
}
