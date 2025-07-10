using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(CusAuthorisationRuleLookups))]
	sealed class CusAuthorisationRuleLookupsTest : CusAuthorisationRuleLookupsAbstractTest<CusAuthorisationRuleLookups>
	{
		public void TestRuleCodeListForIDE()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration;
			AssertContainsExactElementsInAnyOrder("IDE authorization types should allow type MCO rules", new[] { CusAuthorisationRuleTypeList.Codes.MainCustomsOffice }, lookups.RuleCodeList.GetAllCodes());
		}

		public void TestRuleCodeListForEDE()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration;
			AssertContainsExactElementsInAnyOrder("EDE authorization types should allow type MCO rules", new[] { CusAuthorisationRuleTypeList.Codes.MainCustomsOffice }, lookups.RuleCodeList.GetAllCodes());
		}

		public void TestRuleCodeListForNonIDENonEDE()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			var list = lookups.RuleCodeList;
			Assert("Non IDE/EDE authorization types should not allow type MCO rules", !list.ContainsCode(CusAuthorisationRuleTypeList.Codes.MainCustomsOffice));
		}

		protected override CusAuthorisationRuleLookups CusAuthorisationRuleLookupsForTesting()
		{
			var cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;
			return new CusAuthorisationRuleLookups(cusAuthorisationRule);
		}
	}
}
