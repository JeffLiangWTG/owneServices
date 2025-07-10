using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefAccTaxRate))]
	sealed class RefAccTaxRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCountryName_ReturnsEmpty_WhenZAT_RN_NKCountryIsNotSet()
		{
			var refAccTaxRate = Factory.New<RefAccTaxRate>();
			AssertEquals(ZString.Empty, refAccTaxRate.CountryName);

			refAccTaxRate.ZAT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals("Australia", refAccTaxRate.CountryName);
		}

		public void TestZAT_Rate_ReturnsNonZeroValues_WhenZAT_RateNumeratorAndZAT_RateDenominatorHasNonDefaultValues()
		{
			var refAccTaxRate = Factory.New<RefAccTaxRate>();
			AssertEquals(0M, refAccTaxRate.ZAT_Rate);

			refAccTaxRate.ZAT_RateNumerator = 25;
			AssertEquals(25M, refAccTaxRate.ZAT_Rate);

			refAccTaxRate.ZAT_RateDenominator = 100;
			AssertEquals(0.25M, refAccTaxRate.ZAT_Rate);

			refAccTaxRate.ZAT_RateNumerator = 2556;
			refAccTaxRate.ZAT_RateDenominator = 10000;
			AssertEquals(0.2556M, refAccTaxRate.ZAT_Rate);
		}

		public void TestStartDateForDisplay_ReturnsDateInStringFormat_WhenZAT_StartDateIsSet()
		{
			var refAccTaxRate = Factory.New<RefAccTaxRate>();
			AssertEquals("", refAccTaxRate.StartDateForDisplay);

			refAccTaxRate.ZAT_StartDate = new ZDate(1900, 1, 1);
			AssertEquals("StartDateForDisplay", "01 Jan 1900", refAccTaxRate.StartDateForDisplay);
		}

		public void TestEndDateForDisplay_ReturnsDateInStringFormat_WhenZAT_EndDateIsSet()
		{
			var refAccTaxRate = Factory.New<RefAccTaxRate>();
			AssertEquals("", refAccTaxRate.EndDateForDisplay);

			refAccTaxRate.ZAT_EndDate = new ZDate(2018, 12, 31);
			AssertEquals("EndDateForDisplay", "31 Dec 2018", refAccTaxRate.EndDateForDisplay);
		}
	}
}
