using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.Business.Testing
{
	public class CustomsRuleRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRuleCodes()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			var lookups = rule.Lookups;
			AssertEquals("CustomsRuleRuleCodeList", typeof(CustomsRuleRuleCodeList), lookups.RuleCodes.GetType());
			AssertEquals("CustomsRuleRuleCodeList count", 7, lookups.RuleCodes.Count);
		}

		public void TestValueFromCodes()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			var lookups = rule.Lookups;
			AssertEquals("Default", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.ADCEligible;
			AssertEquals("ADCEligible", typeof(CustomsRuleRuleADCEligibleCodeList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.PaymentType;
			AssertEquals("PaymentType", typeof(CustomsRuleRulePaymentTypeValueFromCodeList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			AssertEquals("SingleTransactionBondAmount", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			AssertEquals("TariffNumber", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			AssertEquals("TotalCustomsDisbursement", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
			AssertEquals("TotalCustomsValue", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			AssertEquals("TotalDuty", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());
		}

		IDisposable countryDisposition;
		protected override void SetUp()
		{
			base.SetUp();
			countryDisposition = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
		}

		protected override void TearDown()
		{
			base.TearDown();
			countryDisposition.Dispose();
		}
	}
}
