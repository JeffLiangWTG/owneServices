using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ServiceOrgRelatedPartySubsetCollection))]

	sealed class ServiceOrgRelatedPartySubsetCollectionTest : OrgRelatedPartySubsetCollectionTest<ServiceOrgRelatedPartySubsetCollection, OrgRelatedParty>
	{
		public void TestIsThisPartOfTheCollection()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var partyRecord1 = orgHeader.AllRelatedParties.AddNew();
			partyRecord1.PR_PartyType = "ARN";
			var partyRecord2 = orgHeader.AllRelatedParties.AddNew();
			partyRecord2.PR_PartyType = "ACR";
			var partyRecord3 = orgHeader.AllRelatedParties.AddNew();
			partyRecord3.PR_PartyType = "ACR";
			var partyRecord4 = orgHeader.AllRelatedParties.AddNew();
			partyRecord4.PR_PartyType = "ICT";

			AssertContainsExactElementsInAnyOrder(new[] { partyRecord2.PK, partyRecord3.PK }, orgHeader.ServiceRelatedParties.Select(x => x.PK));
		}

		protected override void AssertSpecificDefaults(OrgRelatedParty relatedParty)
		{
		}

		protected override ServiceOrgRelatedPartySubsetCollection GetCollectionToTest()
		{
			return new ServiceOrgRelatedPartySubsetCollection(Organisation.AllRelatedParties);
		}
	}
}
