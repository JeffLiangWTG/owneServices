using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	internal class RefCusMapTypeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMapDirectionList()
		{
			var map = Factory.NewWithValidTestData<RefCusMapType>();
			AssertEquals(typeof(MapDirectionList), map.Lookups.MapDirectionList.GetType());
			Assert(map.Lookups.MapDirectionList.ContainsCode(MapDirectionList.Codes.BTH));
			Assert(map.Lookups.MapDirectionList.ContainsCode(MapDirectionList.Codes.INW));
			Assert(map.Lookups.MapDirectionList.ContainsCode(MapDirectionList.Codes.OUT));
		}
	}
}
