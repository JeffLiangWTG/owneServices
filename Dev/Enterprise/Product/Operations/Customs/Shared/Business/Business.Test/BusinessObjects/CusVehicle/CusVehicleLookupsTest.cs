using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusVehicleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMileageUQList()
		{
			var parent = Factory.New<CusVehicle>();
			var list = parent.Lookups.MileageUQList;
			AssertEquals("KM, MI", parent.Lookups.MileageUQList.CodesAsString);
			AssertSame("cached", list, parent.Lookups.MileageUQList);
		}
	}
}
