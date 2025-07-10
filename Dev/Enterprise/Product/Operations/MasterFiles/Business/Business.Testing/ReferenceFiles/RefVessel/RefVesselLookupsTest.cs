using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefVesselLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestScreeningStatusesList()
		{
			RefVessel vessel = Factory.NewWithValidTestData<RefVessel>();
			AssertEquals(typeof(ScreeningStatusesList), vessel.Lookups.ScreeningStatusesList.GetType());
		}
	}
}
