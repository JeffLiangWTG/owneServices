using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobAdPlacementLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAdPlacementPublicationsList()
		{
			HRJobAdPlacement adPlacement = Factory.New<HRJobAdPlacement>();
			AssertNotNull("Ad Placement Publications List should not be null", adPlacement.Lookups.AdPlacementPublicationsList);
		}
	}
}
