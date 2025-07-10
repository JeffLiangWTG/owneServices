using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	public class CustomsRuleRuleValidationTest : CusPermitRuleValidationTest
	{
		public virtual void TestCheckCPR_RuleCode()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule1 = customsRule.Rules.AddNew();
			var validation1 = rule1.Validation;
			validation1.ValidateCPR_RuleCode();
			AssertHasErrorContaining("CPR_RuleCode cannot be empty.", rule1.CPR_RuleCodeInfo, MandatoryValidation.MustBeEntered);

			rule1.CPR_RuleCode = "AAA";
			validation1.ValidateCPR_RuleCode();
			AssertHasErrorContaining("CPR_RuleCode is invalid code.", rule1.CPR_RuleCodeInfo, ListValidation.InvalidCodeError);

			rule1.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			validation1.ValidateCPR_RuleCode();
			AssertNoErrors(rule1.CPR_RuleCodeInfo);

			rule1.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
			validation1.ValidateCPR_RuleCode();
			AssertNoErrors(rule1.CPR_RuleCodeInfo);

			rule1.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.PaymentType;
			validation1.ValidateCPR_RuleCode();
			AssertHasError("Payment Type(PMT) should pair with Total Customs Disbursement(CSD).", rule1.CPR_RuleCodeInfo, CustomsRuleRuleValidation.PaymentTypeShouldPairWithTotalCustomsDisbursement);

			var rule2 = customsRule.Rules.AddNew();
			rule2.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
			validation1.ValidateCPR_RuleCode();
			AssertHasError("Payment Type(PMT) should pair with Total Customs Disbursement(CSD).", rule1.CPR_RuleCodeInfo, CustomsRuleRuleValidation.PaymentTypeShouldPairWithTotalCustomsDisbursement);

			rule2.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			validation1.ValidateCPR_RuleCode();
			AssertNoError(rule1.CPR_RuleCodeInfo, CustomsRuleRuleValidation.PaymentTypeShouldPairWithTotalCustomsDisbursement);
			AssertNoErrors(rule1.CPR_RuleCodeInfo);

			rule2.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			rule1.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			AssertHasError(rule1.CPR_RuleCodeInfo, CustomsRuleRuleValidation.OnlyOneDTYRuleAvailable);

			rule2.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
			validation1.ValidateCPR_RuleCode();
			AssertNoError(rule1.CPR_RuleCodeInfo, CustomsRuleRuleValidation.OnlyOneDTYRuleAvailable);
		}

		public void TestCheckCPR_ValueFrom()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			var validation = rule.Validation as CustomsRuleRuleValidation;
			validation.ValidateCPR_ValueFrom();
			AssertHasErrorContaining("CPR_ValueFrom cannot be empty.", rule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);

			rule.CPR_ValueFrom = "1";
			validation.ValidateCPR_ValueFrom();
			AssertNoErrors(rule.CPR_ValueFromInfo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.PaymentType;
			validation.ValidateCPR_ValueFrom();
			AssertHasErrorContaining(rule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);

			rule.CPR_ValueFrom = CustomsRuleRulePaymentTypeValueFromCodeList.Codes.Broker;
			validation.ValidateCPR_ValueFrom();
			AssertNoErrors(rule.CPR_ValueFromInfo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			rule.CPR_ValueFrom = "1";
			validation.ValidateCPR_ValueFrom();
			AssertHasError("No need to use CPR_ValueFrom for CPR_RuleCode CSD.", rule.CPR_ValueFromInfo, validation.NoNeedToUseCPR_ValueFromError);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
			rule.CPR_ValueFrom = "1";
			validation.ValidateCPR_ValueFrom();
			AssertHasError("No need to use CPR_ValueFrom for CPR_RuleCode CVL.", rule.CPR_ValueFromInfo, validation.NoNeedToUseCPR_ValueFromError);

			rule.CPR_ValueFrom = "0";
			validation.ValidateCPR_ValueFrom();
			AssertNoError(rule.CPR_ValueFromInfo, validation.NoNeedToUseCPR_ValueFromError);
			AssertNoErrors(rule.CPR_ValueFromInfo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			validation.ValidateCPR_ValueFrom();
			AssertNoError(rule.CPR_ValueFromInfo, validation.NoNeedToUseCPR_ValueFromError);
			AssertNoErrors(rule.CPR_ValueFromInfo);
		}

		public virtual void TestCheckCPR_ValueFromAndCPR_ValueTo()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			var validation = rule.Validation as CustomsRuleRuleValidation;

			rule.CPR_ValueFrom = "1";
			validation.ValidateCPR_ValueFrom();
			AssertNoErrors(rule.CPR_ValueFromInfo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			rule.CPR_ValueFrom = "1";
			validation.ValidateCPR_ValueFrom();
			AssertHasError(rule.CPR_ValueFromInfo, CustomsRuleRuleValidation.TariffNumberLengthLeastError);
			AssertNoErrors(rule.CPR_ValueToInfo);

			rule.CPR_ValueFrom = "-11";
			validation.ValidateCPR_ValueFrom();
			AssertHasError(rule.CPR_ValueFromInfo, CustomsRuleRuleValidation.TariffNumberNegativeNotAllowed);
			AssertHasError(rule.CPR_ValueFromInfo, CustomsRuleRuleValidation.TariffNumberLengthLeastError);

			rule.CPR_ValueFrom = "11";
			validation.ValidateCPR_ValueFrom();
			AssertNoErrors(rule.CPR_ValueFromInfo);

			rule.CPR_ValueTo = "-11";
			validation.ValidateCPR_ValueTo();
			AssertHasError(rule.CPR_ValueToInfo, CustomsRuleRuleValidation.TariffNumberNegativeNotAllowed);
			AssertHasError(rule.CPR_ValueToInfo, CustomsRuleRuleValidation.TariffNumberLengthLeastError);
			AssertHasError(rule.CPR_ValueToInfo, CustomsRuleRuleValidation.TariffNumberFromAndToLengthBeSame);

			rule.CPR_ValueTo = "10";
			validation.ValidateCPR_ValueTo();
			AssertHasError(rule.CPR_ValueToInfo, CustomsRuleRuleValidation.TariffNumberToShouldGreaterThanFrom);

			rule.CPR_ValueTo = "";
			validation.ValidateCPR_ValueTo();
			AssertNoErrors(rule.CPR_ValueToInfo);

			rule.CPR_ValueTo = "11";
			validation.ValidateCPR_ValueTo();
			AssertNoErrors(rule.CPR_ValueToInfo);

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			rule.CPR_ValueFrom = "1";
			validation.ValidateCPR_ValueFrom();
			AssertHasError("No need to use CPR_ValueFrom for CPR_RuleCode DTY.", rule.CPR_ValueFromInfo, validation.NoNeedToUseCPR_ValueFromError);

			rule.CPR_ValueFrom = "0";
			validation.ValidateCPR_ValueFrom();
			AssertNoError(rule.CPR_ValueFromInfo, validation.NoNeedToUseCPR_ValueFromError);
			AssertNoErrors(rule.CPR_ValueFromInfo);
		}
	}
}
