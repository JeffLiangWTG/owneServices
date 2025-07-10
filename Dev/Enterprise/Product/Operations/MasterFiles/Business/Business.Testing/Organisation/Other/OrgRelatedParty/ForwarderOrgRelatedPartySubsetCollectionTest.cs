using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ForwarderOrgRelatedPartySubsetCollection))]
	sealed class ForwarderOrgRelatedPartySubsetCollectionTest : OrgRelatedPartySubsetCollectionTest<ForwarderOrgRelatedPartySubsetCollection>
	{
		public void TestForwarderCoLoadWithSupport()
		{
			var relatedParty = Organisation.AllRelatedParties.AddNew();
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderCoLoadWith;
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			var collection = GetCollectionToTest();
			collection.Rebuild();
			AssertNotNull(collection.FindByPK(relatedParty.PK));
		}

		public void TestListNotContains_AuthorisedCargoReporter()
		{
			var organisation = Factory.New<OrgHeader>();
			var acrRelatedParty = organisation.AllRelatedParties.AddNew();
			acrRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.AuthorizedCargoReporter;
			var collection = new ForwarderOrgRelatedPartySubsetCollection(organisation.AllRelatedParties);
			AssertEquals(false, collection.Contains(acrRelatedParty));
		}

		#region Implementation

		protected override void AssertSpecificDefaults(OrgRelatedParty relatedParty)
		{
			AssertEquals("Default direction:", RelatedPartyDirectionList.Codes.Forwarder, relatedParty.PR_FreightDirection);
		}

		protected override ForwarderOrgRelatedPartySubsetCollection GetCollectionToTest()
		{
			return new ForwarderOrgRelatedPartySubsetCollection(Organisation.AllRelatedParties);
		}

		#endregion
	}
}
