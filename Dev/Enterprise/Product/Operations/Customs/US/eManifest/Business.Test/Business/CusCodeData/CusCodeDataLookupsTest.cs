using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class CusCodeDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var commodity = Factory.New<Commodity>();
			var lookups = commodity.HarmonizedNumbers.AddNew().Lookups;
			AssertType<CusCodeDataTypeList>("CY_CodeList", lookups.CY_CodeList);
			AssertType<USCTariffCollection>("Tariffs", lookups.Tariffs);
		}
	}
}
