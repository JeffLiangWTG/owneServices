using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CustomsRuleRuleValidationTest : Customs.Business.Testing.CustomsRuleRuleValidationTest
	{
		public override void TestCheckCPR_RuleCode()
		{
			base.TestCheckCPR_RuleCode();
			var customsRule = Factory.New<CustomsRule>();
			var rule1 = customsRule.Rules.AddNew();
			rule1.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			AssertNoErrors(rule1.CPR_RuleCodeInfo);

			var rule2 = customsRule.Rules.AddNew();
			rule2.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			AssertHasError(rule2.CPR_RuleCodeInfo, CustomsRuleRuleValidation.OnlyOneSTBRuleAvailable);

			rule2.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			AssertNoError(rule2.CPR_RuleCodeInfo, CustomsRuleRuleValidation.OnlyOneSTBRuleAvailable);
		}

		public void TestCheckCPR_RuleCodeForUSSpecificRuleCode()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule1 = customsRule.Rules.AddNew();
			var validation1 = rule1.Validation;

			rule1.CPR_RuleCode = "AAA";
			validation1.ValidateCPR_RuleCode();
			AssertHasErrorContaining("CPR_RuleCode is invalid code.", rule1.CPR_RuleCodeInfo, ListValidation.InvalidCodeError);

			rule1.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			validation1.ValidateCPR_RuleCode();
			AssertNoErrors(rule1.CPR_RuleCodeInfo);

			rule1.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			validation1.ValidateCPR_RuleCode();
			AssertNoErrors(rule1.CPR_RuleCodeInfo);

			rule1.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.ADCEligible;
			validation1.ValidateCPR_RuleCode();
			AssertNoErrors(rule1.CPR_RuleCodeInfo);

			rule1.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			validation1.ValidateCPR_RuleCode();
			AssertNoErrors(rule1.CPR_RuleCodeInfo);
		}

		public override void TestCheckCPR_ValueFromAndCPR_ValueTo()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			var validation = rule.Validation as CustomsRuleRuleValidation;

			rule.CPR_ValueFrom = "1";
			validation.ValidateCPR_ValueFrom();
			AssertNoErrors(rule.CPR_ValueFromInfo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount;
			validation.ValidateCPR_ValueFrom();
			AssertNoErrors(rule.CPR_ValueFromInfo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.ADCEligible;
			validation.ValidateCPR_ValueFrom();
			AssertHasErrorContaining(rule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);

			rule.CPR_ValueFrom = CustomsRuleRuleADCEligibleCodeList.Codes.CaseNumberEntered;
			validation.ValidateCPR_ValueFrom();
			AssertNoErrors(rule.CPR_ValueFromInfo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			rule.CPR_ValueFrom = "1";
			validation.ValidateCPR_ValueFrom();
			AssertHasError("No need to use CPR_ValueFrom for CPR_RuleCode DTY.", rule.CPR_ValueFromInfo, validation.NoNeedToUseCPR_ValueFromError);

			rule.CPR_ValueFrom = "0";
			validation.ValidateCPR_ValueFrom();
			AssertNoError(rule.CPR_ValueFromInfo, validation.NoNeedToUseCPR_ValueFromError);
			AssertNoErrors(rule.CPR_ValueFromInfo);
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
