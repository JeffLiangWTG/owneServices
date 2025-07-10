using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobTradeLaneLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDirectionTypes()
		{
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			Assert("Should contain One value", tradeLane.Lookups.DirectionTypes.ContainsCode("One"));
			Assert("Should contain Two value", tradeLane.Lookups.DirectionTypes.ContainsCode("Two"));
		}

		public void TestLocations()
		{
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			AssertEquals(tradeLane.Factory, tradeLane.Lookups.Locations.Factory);
			AssertEquals(typeof(LocationCollection), tradeLane.Lookups.Locations.GetType());
		}

		public void TestRelatedOrgs()
		{
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			AssertEquals(tradeLane.Factory, tradeLane.Lookups.RelatedOrgs.Factory);
			AssertEquals(typeof(ShipsAgencyPrincipalCollection), tradeLane.Lookups.RelatedOrgs.GetType());
		}
	}
}
