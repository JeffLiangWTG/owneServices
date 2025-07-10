using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCTariffRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckU1_RuleCode()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = "";
			AssertHasErrorContaining(tariffRule.U1_RuleCodeInfo, "Please enter a ");

			tariffRule.U1_RuleCode = "AAA";
			AssertNoErrorContaining(tariffRule.U1_RuleCodeInfo, "Please enter a ");
			AssertHasErrorContaining(tariffRule.U1_RuleCodeInfo, "Enter a valid selection");

			tariffRule.U1_RuleCode = TariffRuleList.Codes.AdditionalTariffs;
			AssertNoErrorContaining(tariffRule.U1_RuleCodeInfo, "Enter a valid selection");
		}

		public void TestCheckU1_DateFrom()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_DateFrom = ZDateTime.Empty;
			AssertHasErrorContaining(tariffRule.U1_DateFromInfo, "Please enter a ");

			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			AssertNoErrors(tariffRule.U1_DateFromInfo);
			AssertNoWarnings(tariffRule.U1_DateFromInfo);
		}

		public void TestCheckU1_DateTo()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_DateTo = ZDateTime.Empty;
			AssertNoErrors(tariffRule.U1_DateToInfo);

			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_DateTo = ZDateTime.BrettsBirthday.AddDays(-1);
			AssertHasErrorContaining(tariffRule.U1_DateToInfo, USCTariffRuleValidation.DateToShouldBeLaterThanDateFrom);

			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday.AddDays(-1);
			AssertNoNotifications(tariffRule.U1_DateToInfo);
		}

		public void TestCheckFormattedTariff()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.FormattedTariff = "";
			AssertHasErrorContaining(tariffRule.FormattedTariffInfo, "Please enter a ");

			tariffRule.FormattedTariff = "0000";
			AssertNoErrorContaining(tariffRule.FormattedTariffInfo, "Please enter a ");
			AssertHasWarningContaining(tariffRule.FormattedTariffInfo, USCTariffRuleValidation.InvalidTariffNumber);

			tariffRule.FormattedTariff = "9802";
			AssertNoWarningContaining(tariffRule.FormattedTariffInfo, USCTariffRuleValidation.InvalidTariffNumber);

			tariffRule.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			tariffRule.FormattedTariff = "9802";
			AssertHasError(tariffRule.FormattedTariffInfo, USCTariffRuleValidation.FullTariffNumberRequiredForSTNRule);

			tariffRule.FormattedTariff = USCTariff.AGOABenefitsApplicable;
			AssertNoError(tariffRule.FormattedTariffInfo, USCTariffRuleValidation.FullTariffNumberRequiredForSTNRule);
		}

		public void TestCheckFormattedTariffTo()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.FormattedTariffTo = "0000";
			AssertHasWarningContaining(tariffRule.FormattedTariffToInfo, USCTariffRuleValidation.InvalidTariffNumber);

			tariffRule.FormattedTariffTo = "9802";
			AssertNoWarningContaining(tariffRule.FormattedTariffToInfo, USCTariffRuleValidation.InvalidTariffNumber);

			tariffRule.FormattedTariff = "1010";
			tariffRule.FormattedTariffTo = "1010";
			AssertHasErrorContaining(tariffRule.FormattedTariffToInfo, USCTariffRuleValidation.TariffToNumberShouldBeGreaterThanTariffNumberFrom);

			tariffRule.FormattedTariffTo = "1000";
			AssertHasErrorContaining(tariffRule.FormattedTariffToInfo, USCTariffRuleValidation.TariffToNumberShouldBeGreaterThanTariffNumberFrom);

			tariffRule.FormattedTariffTo = "1020";
			AssertNoErrorContaining(tariffRule.FormattedTariffToInfo, USCTariffRuleValidation.TariffToNumberShouldBeGreaterThanTariffNumberFrom);
		}

		public void TestCheckDuplicates()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_Tariff = "9902";
			tariffRule.U1_TariffTo = "990202";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;

			USCTariffRule tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_Tariff = "990201";
			tariffRule2.U1_DateFrom = ZDateTime.BrettsBirthday;
			AssertHasErrorContaining(tariffRule2.FormattedTariffInfo, USCTariffRuleValidation.DuplicateRecordExist);
			AssertHasErrorContaining(tariffRule2.FormattedTariffInfo, "9902.02");

			tariffRule2.U1_DateFrom = ZDateTime.BrettsBirthday.AddDays(1);//still covered by the first tariff rule
			AssertHasErrorContaining(tariffRule2.FormattedTariffInfo, USCTariffRuleValidation.DuplicateRecordExist);

			tariffRule2.U1_DateFrom = ZDateTime.BrettsBirthday.AddDays(-1);//not covered any more
			AssertNoErrorContaining(tariffRule2.FormattedTariffInfo, USCTariffRuleValidation.DuplicateRecordExist);
		}
	}
}
