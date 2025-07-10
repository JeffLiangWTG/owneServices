using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	class AgencyHouseBillLookupsTest : TestCaseWithFactory
	{
		public void TestReleaseTypes()
		{
			AssertEquals("OBR, OBO, SWB, EBL", (new AgencyHouseBillLookups(Factory).ReleaseTypes as CodeDescriptionPairList).CodesAsString);
		}

		public void TestCurrencies()
		{
			var currencies = new AgencyHouseBillLookups(Factory).Currencies;

			AssertGreaterThan(currencies.List.Count, 0);
			AssertEquals("Australian Dollar", currencies.DescriptionFromCode("AUD"));
			AssertEquals("Euro", currencies.DescriptionFromCode("EUR"));
		}

		public void TestCountries()
		{
			var countries = new AgencyHouseBillLookups(Factory).Countries as RefCountryCollection;

			AssertGreaterThan(countries.Count, 0);
		}
	}
}
