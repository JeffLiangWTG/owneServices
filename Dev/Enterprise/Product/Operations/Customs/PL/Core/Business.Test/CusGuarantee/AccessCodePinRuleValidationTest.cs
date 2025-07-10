using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Testing;

class AccessCodePinRuleValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckMainAccessCode_DigitsOnly()
	{
		var guaranteeRule = Factory.New<CusGuaranteeRule>();
		guaranteeRule.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin;
		const string additionalAccessCodeOnlyDigitsMessage = "Additional Access Code should contain digits only";
		CombineAssertions(() =>
		{
			guaranteeRule.Validation.ValidateCPR_ValueFrom();
			AssertNoMessageError("No digits only message - empty", guaranteeRule.CPR_ValueFromInfo, additionalAccessCodeOnlyDigitsMessage);
			guaranteeRule.CPR_ValueFrom = "FDSA";
			AssertHasMessageError("Digits only message", guaranteeRule.CPR_ValueFromInfo, additionalAccessCodeOnlyDigitsMessage);
			guaranteeRule.CPR_ValueFrom = "222";
			AssertNoMessageError("No digits only message - correct", guaranteeRule.CPR_ValueFromInfo, additionalAccessCodeOnlyDigitsMessage);
		});
	}

	public void TestCheckMainAccessCode_4Characters()
	{
		var guaranteeRule = Factory.New<CusGuaranteeRule>();
		guaranteeRule.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin;
		const string additionalAccessCodeFourCharactersMessage = "Additional Access Code should contain 4 characters";
		CombineAssertions(() =>
		{
			guaranteeRule.Validation.ValidateCPR_ValueFrom();
			AssertNoMessageError("No four characters message - empty", guaranteeRule.CPR_ValueFromInfo, additionalAccessCodeFourCharactersMessage);
			guaranteeRule.CPR_ValueFrom = "FDSA";
			AssertNoMessageError("No four characters message - correct", guaranteeRule.CPR_ValueFromInfo, additionalAccessCodeFourCharactersMessage);
			guaranteeRule.CPR_ValueFrom = "222";
			AssertHasMessageError("Four characters message", guaranteeRule.CPR_ValueFromInfo, additionalAccessCodeFourCharactersMessage);
		});
	}
}
