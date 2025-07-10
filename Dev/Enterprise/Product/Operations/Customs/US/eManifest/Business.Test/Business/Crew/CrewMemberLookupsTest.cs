using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class CrewMemberLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var trip = Factory.New<Trip>();
			var lookups = trip.CrewMembers.AddNew().Lookups;
			AssertType<CrewTypes>(lookups.CrewTypes);
		}
	}
}
