using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	sealed class TaxCoreCountryHelperTest : TestCaseWithFactory
	{
		public void TestGetTaxCoreSupportedCountries()
		{
			AssertContainsExactElementsInAnyOrder(new ZString[] { CountryCodes.Fiji, CountryCodes.WesternSamoa }, TaxCoreCountryHelper.GetTaxCoreSupportedCountries());
		}

		public void TestIsInTaxCoreSupportedCountry()
		{
			Assert(!TaxCoreCountryHelper.IsInTaxCoreSupportedCountry(GlbCompany.CurrentCompany));
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Fiji))
			{
				Assert(TaxCoreCountryHelper.IsInTaxCoreSupportedCountry(GlbCompany.CurrentCompany));
			}
		}

		public void TestIsThisCountrySupportedByTaxCore()
		{
			Assert(!TaxCoreCountryHelper.IsThisCountrySupportedByTaxCore(CountryCodes.Australia));
			Assert(TaxCoreCountryHelper.IsThisCountrySupportedByTaxCore(CountryCodes.Fiji));
		}
	}
}
