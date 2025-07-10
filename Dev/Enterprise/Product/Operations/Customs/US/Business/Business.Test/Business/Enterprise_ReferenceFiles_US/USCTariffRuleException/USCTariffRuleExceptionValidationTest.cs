using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCTariffRuleExceptionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckU2_DateFrom()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;

			USCTariffRuleException exception = tariffRule.RuleExceptions.AddNew();
			exception.U2_DateFrom = ZDateTime.Empty;
			AssertHasErrorContaining(exception.U2_DateFromInfo, "Please enter a ");

			exception.U2_DateFrom = ZDateTime.BrettsBirthday.AddDays(-1);
			AssertNoErrorContaining(exception.U2_DateFromInfo, "Please enter a ");
			AssertHasError(exception.U2_DateFromInfo, USCTariffRuleExceptionValidation.ExceptionDateFromShouldBeLaterOrEqualToRuleDateFrom);

			exception.U2_DateFrom = ZDateTime.BrettsBirthday;
			AssertNoNotifications(exception.U2_DateFromInfo);
		}

		public void TestCheckU2_DateTo()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_DateTo = ZDateTime.BrettsBirthday;

			USCTariffRuleException exception = tariffRule.RuleExceptions.AddNew();
			exception.U2_DateTo = ZDateTime.Empty;
			AssertHasErrorContaining(exception.U2_DateToInfo, "Please enter a ");

			exception.U2_DateTo = ZDateTime.BrettsBirthday.AddDays(1);
			AssertNoErrorContaining(exception.U2_DateToInfo, "Please enter a ");
			AssertHasError(exception.U2_DateToInfo, USCTariffRuleExceptionValidation.ExceptionDateToShouldBeEarlierOrEqualToRuleDateTo);

			exception.U2_DateTo = ZDateTime.BrettsBirthday;
			AssertNoError(exception.U2_DateToInfo, USCTariffRuleExceptionValidation.ExceptionDateToShouldBeEarlierOrEqualToRuleDateTo);

			exception.U2_DateFrom = ZDateTime.BrettsBirthday.AddDays(1);
			AssertHasError(exception.U2_DateToInfo, USCTariffRuleExceptionValidation.DateToShouldBeLaterThanDateFrom);

			exception.U2_DateFrom = ZDateTime.BrettsBirthday.AddDays(-1);
			AssertNoNotifications(exception.U2_DateToInfo);
		}

		public void TestCheckFormattedTariff()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.FormattedTariff = "0000";

			USCTariffRuleException exception = tariffRule.RuleExceptions.AddNew();
			exception.FormattedTariff = "0000";
			AssertHasError(exception.FormattedTariffInfo, USCTariffRuleExceptionValidation.TariffsForRuleAndExceptionCannotBeTheSame);

			exception.FormattedTariff = "0001";
			AssertNoError(exception.FormattedTariffInfo, USCTariffRuleExceptionValidation.TariffsForRuleAndExceptionCannotBeTheSame);
			AssertHasErrorContaining(exception.FormattedTariffInfo, USCTariffRuleExceptionValidation.ExceptionTariffShouldStartWithRuleTariff);

			exception.FormattedTariff = "000010";
			AssertNoErrorContaining(exception.FormattedTariffInfo, USCTariffRuleExceptionValidation.ExceptionTariffShouldStartWithRuleTariff);
			AssertHasWarning(exception.FormattedTariffInfo, USCTariffRuleValidation.InvalidTariffNumber);

			exception.FormattedTariff = "9802";
			AssertNoWarning(exception.FormattedTariffInfo, USCTariffRuleValidation.InvalidTariffNumber);

			tariffRule.FormattedTariff = "0010";
			exception.FormattedTariff = "0000";
			AssertHasErrorContaining(exception.FormattedTariffInfo, USCTariffRuleExceptionValidation.ExceptionTariffToShouldStartWithRuleTariff);

			tariffRule.FormattedTariffTo = "1010";
			exception.FormattedTariff = "1000";// 0010 < 1000 < 1010
			AssertNoErrorContaining(exception.FormattedTariffInfo, USCTariffRuleExceptionValidation.ExceptionTariffToShouldStartWithRuleTariff);

			exception.FormattedTariff = "1020";
			AssertHasErrorContaining(exception.FormattedTariffInfo, USCTariffRuleExceptionValidation.ExceptionTariffShouldBeLessThanOrStartWithRuleTariffTo);

			exception.FormattedTariff = "101000";
			AssertNoErrorContaining(exception.FormattedTariffInfo, USCTariffRuleExceptionValidation.ExceptionTariffShouldBeLessThanOrStartWithRuleTariffTo);
		}

		public void TestCheckFormattedTariffTo()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();

			USCTariffRuleException exception = tariffRule.RuleExceptions.AddNew();
			exception.FormattedTariff = "0010";
			exception.FormattedTariffTo = "0010";
			AssertHasErrorContaining(exception.FormattedTariffToInfo, USCTariffRuleValidation.TariffToNumberShouldBeGreaterThanTariffNumberFrom);

			exception.FormattedTariffTo = "0000";
			AssertHasErrorContaining(exception.FormattedTariffToInfo, USCTariffRuleValidation.TariffToNumberShouldBeGreaterThanTariffNumberFrom);

			exception.FormattedTariffTo = "001000";
			AssertNoErrorContaining(exception.FormattedTariffToInfo, USCTariffRuleValidation.TariffToNumberShouldBeGreaterThanTariffNumberFrom);

			tariffRule.FormattedTariff = "0010";
			exception.FormattedTariffTo = "0011";
			AssertHasErrorContaining(exception.FormattedTariffToInfo, USCTariffRuleExceptionValidation.ExceptionTariffToShouldStartWithRuleTariff);

			exception.FormattedTariffTo = "001000";
			AssertNoErrorContaining(exception.FormattedTariffToInfo, USCTariffRuleExceptionValidation.ExceptionTariffToShouldStartWithRuleTariff);

			tariffRule.FormattedTariff = "0010";
			tariffRule.FormattedTariffTo = "0020";
			exception.FormattedTariffTo = "0011";
			AssertNoErrorContaining(exception.FormattedTariffToInfo, USCTariffRuleExceptionValidation.ExceptionTariffToShouldBeLessThanOrStartWithRuleTariff);

			exception.FormattedTariffTo = "0021";
			AssertHasErrorContaining(exception.FormattedTariffToInfo, USCTariffRuleExceptionValidation.ExceptionTariffToShouldBeLessThanOrStartWithRuleTariff);

			exception.FormattedTariffTo = "002000";
			AssertNoErrorContaining(exception.FormattedTariffToInfo, USCTariffRuleExceptionValidation.ExceptionTariffToShouldBeLessThanOrStartWithRuleTariff);
		}

		public void TestCheckForDuplicates()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.FormattedTariff = "0000";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;

			USCTariffRuleException exception1 = tariffRule.RuleExceptions.AddNew();
			exception1.FormattedTariff = "000000";
			exception1.U2_DateFrom = ZDateTime.BrettsBirthday;

			USCTariffRuleException exception2 = tariffRule.RuleExceptions.AddNew();
			exception2.FormattedTariff = "00000000";
			exception2.U2_DateFrom = ZDateTime.BrettsBirthday;
			AssertHasErrorContaining(exception2.FormattedTariffInfo, USCTariffRuleExceptionValidation.DuplicateRecordAlreadyExists);

			exception2.FormattedTariff = "000001";
			AssertNoErrorContaining(exception2.FormattedTariffInfo, USCTariffRuleExceptionValidation.DuplicateRecordAlreadyExists);
		}
	}
}
