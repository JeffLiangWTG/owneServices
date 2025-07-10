using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCRuleSecondaryTariffExceptionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTariff()
		{
			Exception.FormattedTariff = "";
			AssertHasErrorContaining(Exception.FormattedTariffInfo, MandatoryValidation.MustBeEntered);

			SecondaryTariffRule.FormattedTariffFrom = "9204";
			Exception.FormattedTariff = "9207";
			AssertNoErrorContaining(Exception.FormattedTariffInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(Exception.FormattedTariffInfo, USCRuleSecondaryTariffExceptionValidation.ExceptionTariffShouldStartWithRuleTariff);

			Exception.FormattedTariff = "9204";
			AssertNoErrorContaining(Exception.FormattedTariffInfo, USCRuleSecondaryTariffExceptionValidation.ExceptionTariffShouldStartWithRuleTariff);
			AssertHasError(Exception.FormattedTariffInfo, USCRuleSecondaryTariffExceptionValidation.TariffsForRuleAndExceptionCannotBeTheSame);

			Exception.FormattedTariff = "920710";
			AssertNoError(Exception.FormattedTariffInfo, USCRuleSecondaryTariffExceptionValidation.TariffsForRuleAndExceptionCannotBeTheSame);

			SecondaryTariffRule.FormattedTariffTo = "9206";
			Exception.FormattedTariff = "9205";
			AssertNoErrors(Exception.FormattedTariffInfo);

			Exception.FormattedTariff = "9207";
			AssertHasErrorContaining(Exception.FormattedTariffInfo, USCRuleSecondaryTariffExceptionValidation.ExceptionTariffShouldBeLessThanOrStartWithRuleTariffTo);

			Exception.FormattedTariff = "920610";
			AssertNoErrors(Exception.FormattedTariffInfo);
		}

		public void TestValidateDateFrom()
		{
			SecondaryTariffRule.U3_DateFrom = new ZDateTime(2009, 7, 1);

			Exception.U4_DateFrom = ZDateTime.Empty;
			AssertHasErrorContaining(Exception.U4_DateFromInfo, MandatoryValidation.MustBeEntered);

			Exception.U4_DateFrom = new ZDateTime(2009, 6, 30);
			AssertNoErrorContaining(Exception.U4_DateFromInfo, MandatoryValidation.MustBeEntered);
			AssertHasError(Exception.U4_DateFromInfo, USCRuleSecondaryTariffExceptionValidation.ExceptionDateFromShouldBeLaterOrEqualToRuleDateFrom);

			Exception.U4_DateFrom = new ZDateTime(2009, 7, 1);
			AssertNoError(Exception.U4_DateFromInfo, USCRuleSecondaryTariffExceptionValidation.ExceptionDateFromShouldBeLaterOrEqualToRuleDateFrom);
		}

		public void TestValidateDateTo()
		{
			Exception.U4_DateTo = ZDateTime.Empty;
			AssertNoErrors(Exception.U4_DateToInfo);

			SecondaryTariffRule.U3_DateFrom = new ZDateTime(2009, 7, 1);
			SecondaryTariffRule.U3_DateTo = new ZDateTime(2009, 7, 31);

			Exception.U4_DateFrom = new ZDateTime(2009, 7, 15);
			Exception.U4_DateTo = new ZDateTime(2009, 7, 14);
			AssertHasError(Exception.U4_DateToInfo, USCRuleSecondaryTariffExceptionValidation.DateToShouldBeLaterThanDateFrom);

			Exception.U4_DateTo = new ZDateTime(2009, 8, 1);
			AssertNoError(Exception.U4_DateToInfo, USCRuleSecondaryTariffExceptionValidation.DateToShouldBeLaterThanDateFrom);
			AssertHasError(Exception.U4_DateToInfo, USCRuleSecondaryTariffExceptionValidation.ExceptionDateToShouldBeEarlierOrEqualToRuleDateTo);

			Exception.U4_DateTo = new ZDateTime(2009, 7, 31);
			AssertNoError(Exception.U4_DateToInfo, USCRuleSecondaryTariffExceptionValidation.ExceptionDateToShouldBeEarlierOrEqualToRuleDateTo);
		}

		USCRuleSecondaryTariffException Exception
		{
			get { return exception ?? (exception = SecondaryTariffRule.Exceptions.AddNew()); }
		}
		USCRuleSecondaryTariffException exception;

		USCRuleSecondaryTariff SecondaryTariffRule
		{
			get { return secondaryTariffRule ?? (secondaryTariffRule = TariffRule.SecondaryTariffs.AddNew()); }
		}
		USCRuleSecondaryTariff secondaryTariffRule;

		USCTariffRule TariffRule
		{
			get { return fTariffRule ?? (fTariffRule = Factory.New<USCTariffRule>()); }
		}
		USCTariffRule fTariffRule;
	}
}
