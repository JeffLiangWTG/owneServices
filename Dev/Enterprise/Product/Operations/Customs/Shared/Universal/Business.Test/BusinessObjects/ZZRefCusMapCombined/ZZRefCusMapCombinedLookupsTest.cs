using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal.Testing
{
	internal class ZZRefCusMapCombinedLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMapTypesList()
		{
			var map = Factory.NewWithValidTestData<ZZRefCusMapCombined>();
			AssertEquals(typeof(RefCusMapTypeList), map.Lookups.MapTypesList.GetType());
		}

		public void TestCountryOrGroupingList()
		{
			var map = Factory.NewWithValidTestData<ZZRefCusMapCombined>();
			AssertEquals(typeof(RefCountryCollection), map.Lookups.CountryOrGroupingList.GetType());
		}
	}
}
