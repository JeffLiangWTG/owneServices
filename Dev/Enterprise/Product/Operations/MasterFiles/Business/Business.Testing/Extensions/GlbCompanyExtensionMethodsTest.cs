using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	sealed class GlbCompanyExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetExchangeRate_Decimals()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			AssertEquals(6, GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces);
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			AssertEquals(6, GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces);
		}

		public void TestIsExtraTaxApplicable()
		{
			AssertExtraTaxForCountry(Constants.CountryCodes.Australia, false);
			AssertExtraTaxForCountry(Constants.CountryCodes.India, true);
			AssertExtraTaxForCountry(Constants.CountryCodes.Mexico, true);
			AssertExtraTaxForCountry(Constants.CountryCodes.Canada, true);
			AssertExtraTaxForCountry(Constants.CountryCodes.Ghana, true);
			AssertExtraTaxForCountry(Constants.CountryCodes.Italy, true);
			AssertExtraTaxForCountry(Constants.CountryCodes.CostaRica, true);
			AssertExtraTaxForCountry(Constants.CountryCodes.Turkey, true);
			AssertExtraTaxForCountry(Constants.CountryCodes.Congo, true);
			AssertExtraTaxForCountry(Constants.CountryCodes.Gabon, true);
		}

		void AssertExtraTaxForCountry(string countryCode, bool expected)
		{
			GlbCompany.CurrentCompany.SetCountry(countryCode);
			AssertEquals("IsExtraTaxApplicable", expected, GlbCompany.CurrentCompany.IsExtraTaxApplicable());
		}
	}
}
