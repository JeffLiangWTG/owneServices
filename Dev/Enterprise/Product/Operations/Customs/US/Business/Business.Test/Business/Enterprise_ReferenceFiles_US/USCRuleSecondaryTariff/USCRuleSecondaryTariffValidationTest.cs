using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCRuleSecondaryTariffValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckU3_DateFrom()
		{
			USCRuleSecondaryTariff secondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			secondaryTariff.U3_DateFrom = ZDateTime.Empty;
			AssertHasErrorContaining(secondaryTariff.U3_DateFromInfo, "Please enter a ");

			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;
			AssertNoErrors(secondaryTariff.U3_DateFromInfo);
			AssertNoWarnings(secondaryTariff.U3_DateFromInfo);
		}

		public void TestCheckU3_Tariff2()
		{
			USCRuleSecondaryTariff secondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			secondaryTariff.FormattedTariff2 = "1";
			AssertHasErrorContaining(secondaryTariff.FormattedTariff2Info, USCRuleSecondaryTariffValidation.ASetOfSecondaryTariffsEntered);

			secondaryTariff.FormattedTariffFrom = "2";
			AssertNoErrorContaining(secondaryTariff.FormattedTariff2Info, USCRuleSecondaryTariffValidation.ASetOfSecondaryTariffsEntered);
		}

		public void TestCheckU3_Tariff3()
		{
			USCRuleSecondaryTariff secondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			secondaryTariff.FormattedTariff3 = "1";
			AssertHasErrorContaining(secondaryTariff.FormattedTariff3Info, USCRuleSecondaryTariffValidation.ASetOfSecondaryTariffsEntered);

			secondaryTariff.FormattedTariff2 = "2";
			AssertNoErrorContaining(secondaryTariff.FormattedTariff3Info, USCRuleSecondaryTariffValidation.ASetOfSecondaryTariffsEntered);
		}

		public void TestCheckU3_DateTo()
		{
			USCRuleSecondaryTariff secondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			secondaryTariff.U3_DateTo = ZDateTime.Empty;
			AssertNoErrors(secondaryTariff.U3_DateToInfo);

			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;
			secondaryTariff.U3_DateTo = ZDateTime.BrettsBirthday.AddDays(-1);
			AssertHasErrorContaining(secondaryTariff.U3_DateToInfo, USCRuleSecondaryTariffValidation.DateToShouldBeLaterThanDateFrom);

			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday.AddDays(-1);
			AssertNoNotifications(secondaryTariff.U3_DateToInfo);
		}

		public void TestCheckFormattedTariffFrom()
		{
			USCRuleSecondaryTariff secondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			secondaryTariff.FormattedTariffFrom = "";
			AssertHasErrorContaining(secondaryTariff.FormattedTariffFromInfo, "Please enter a ");

			secondaryTariff.FormattedTariffFrom = "0000";
			AssertNoErrorContaining(secondaryTariff.FormattedTariffFromInfo, "Please enter a ");
			AssertHasWarningContaining(secondaryTariff.FormattedTariffFromInfo, USCRuleSecondaryTariffValidation.InvalidTariffNumber);

			secondaryTariff.FormattedTariffFrom = "9802";
			AssertNoWarningContaining(secondaryTariff.FormattedTariffFromInfo, USCRuleSecondaryTariffValidation.InvalidTariffNumber);
		}

		public void TestCheckFormattedTariffTo()
		{
			USCRuleSecondaryTariff secondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			secondaryTariff.FormattedTariffTo = "0000";
			AssertHasWarningContaining(secondaryTariff.FormattedTariffToInfo, USCRuleSecondaryTariffValidation.InvalidTariffNumber);

			secondaryTariff.FormattedTariffTo = "9802";
			AssertNoWarningContaining(secondaryTariff.FormattedTariffToInfo, USCRuleSecondaryTariffValidation.InvalidTariffNumber);

			secondaryTariff.FormattedTariffFrom = "1010";
			secondaryTariff.FormattedTariffTo = "1010";
			AssertHasErrorContaining(secondaryTariff.FormattedTariffToInfo, USCRuleSecondaryTariffValidation.TariffToNumberShouldBeGreaterThanTariffNumberFrom);

			secondaryTariff.FormattedTariffTo = "1000";
			AssertHasErrorContaining(secondaryTariff.FormattedTariffToInfo, USCRuleSecondaryTariffValidation.TariffToNumberShouldBeGreaterThanTariffNumberFrom);

			secondaryTariff.FormattedTariffTo = "1020";
			AssertNoErrorContaining(secondaryTariff.FormattedTariffToInfo, USCRuleSecondaryTariffValidation.TariffToNumberShouldBeGreaterThanTariffNumberFrom);

			secondaryTariff.FormattedTariff2 = "1";
			AssertHasErrorContaining(secondaryTariff.FormattedTariffToInfo, USCRuleSecondaryTariffValidation.CannotDefineAsRangeOfTariffNumbersWhenThereIsAdditionalTariffNumber);

			secondaryTariff.FormattedTariffTo = "";
			AssertNoErrorContaining(secondaryTariff.FormattedTariffToInfo, USCRuleSecondaryTariffValidation.CannotDefineAsRangeOfTariffNumbersWhenThereIsAdditionalTariffNumber);
		}

		public void TestCheckForDuplicates()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.FormattedTariff = "0000";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;

			USCRuleSecondaryTariff secondaryTariff1 = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff1.FormattedTariffFrom = "0000.00";
			secondaryTariff1.U3_DateFrom = ZDateTime.BrettsBirthday;

			USCRuleSecondaryTariff secondaryTariff2 = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff2.FormattedTariffFrom = "0000.00.00";
			secondaryTariff2.U3_DateFrom = ZDateTime.BrettsBirthday;
			AssertHasErrorContaining(secondaryTariff2.FormattedTariffFromInfo, USCRuleSecondaryTariffValidation.DuplicateRecordAlreadyExists);

			secondaryTariff2.FormattedTariffFrom = "0000.01";
			AssertNoErrorContaining(secondaryTariff2.FormattedTariffFromInfo, USCRuleSecondaryTariffValidation.DuplicateRecordAlreadyExists);
		}
	}
}
