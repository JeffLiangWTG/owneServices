using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	internal class ZZRefCusRulingCombinedLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRulingTypeList()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			AssertEquals(typeof(RefCusRulingTypeList), ruling.Lookups.RulingTypeList.GetType());
			AssertEquals(4, ruling.Lookups.RulingTypeList.Count);
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("2"));
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("6"));
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("R"));
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("T"));
			AssertSame(Factory.GetCachedValue<RefCusRulingTypeList>(), ruling.Lookups.RulingTypeList);
		}
	}
}
