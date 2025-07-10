using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(CrewMemberCollection))]
	sealed class CrewMemberCollectionTest : ActiveBusinessObjectCollectionTestCase<CrewMemberCollection>
	{
		public void TestSetDefaultsForNewElementCore()
		{
			var trip = Factory.New<Trip>();
			var crewMember = trip.CrewMembers.AddNew();
			AssertEquals("Default type for first crew member", CrewTypes.Codes.ResponsibleParty, crewMember.CP_Type);
			crewMember = trip.CrewMembers.AddNew();
			AssertEquals("Default type for next crew members", string.Empty, crewMember.CP_Type);
		}

		protected override CrewMemberCollection GetCollectionToTest() => new CrewMemberCollection(Factory.New<Trip>());
	}
}
