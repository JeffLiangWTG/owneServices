using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgTradePeriodLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnitOfWeightList()
		{
			var period = Factory.New<OrgTradePeriod>();
			Assert("UnitOfWeightList.Count > 0", period.Lookups.UnitOfWeightList.Count > 0);
		}

		public void TestUnitOfVolumeList()
		{
			var period = Factory.New<OrgTradePeriod>();
			Assert("UnitOfVolumeList.Count > 0", period.Lookups.UnitOfVolumeList.Count > 0);
		}
	}
}
