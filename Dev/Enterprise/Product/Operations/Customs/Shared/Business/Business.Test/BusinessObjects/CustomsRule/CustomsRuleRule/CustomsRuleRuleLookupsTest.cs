using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	public class CustomsRuleRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRuleCodes()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Afghanistan))
			{
				var customsRule = Factory.New<CustomsRule>();
				var rule = customsRule.Rules.AddNew();
				var lookups = rule.Lookups;
				AssertEquals("CustomsRuleRuleCodeList", typeof(CustomsRuleRuleCodeList), lookups.RuleCodes.GetType());
				AssertEquals("CustomsRuleRuleCodeList count", 5, lookups.RuleCodes.Count);
			}
		}

		public void TestValueFromCodes()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			var lookups = rule.Lookups;
			AssertEquals("CodeDescriptionPairList", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.PaymentType;
			AssertEquals("CustomsRuleRulePaymentTypeValueFromCodeList", typeof(CustomsRuleRulePaymentTypeValueFromCodeList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			AssertEquals("CodeDescriptionPairList", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
			AssertEquals("CodeDescriptionPairList", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			AssertEquals("CodeDescriptionPairList", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			AssertEquals("CodeDescriptionPairList", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());
		}
	}
}
