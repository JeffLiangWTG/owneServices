using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSeaManSlotOrgLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSlotCharterersType()
		{
			CusSeaManSlotOrg header = Factory.New<CusSeaManSlotOrg>();
			AssertEquals(typeof(ShippingProviderCollection), header.Lookups.SlotCharterers.GetType());
		}
	}
}
