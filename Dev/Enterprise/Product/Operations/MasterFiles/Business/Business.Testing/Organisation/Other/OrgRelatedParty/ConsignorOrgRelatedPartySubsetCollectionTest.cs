using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsignorOrgRelatedPartySubsetCollection))]
	sealed class ConsignorOrgRelatedPartySubsetCollectionTest : OrgRelatedPartySubsetCollectionTest<ConsignorOrgRelatedPartySubsetCollection>
	{
		public void TestListNotContains_ServiceProviderCreditor()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.AddRelatedParty(Factory.New<OrgHeader>().PK, RelatedPartyTypeList.Codes.WarehouseForwarder, RelatedPartyDirectionList.Codes.Forwarder, ZString.Empty, ZString.Empty, null);
			organisation.AddRelatedParty(Factory.New<OrgHeader>().PK, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Pickup, ZString.Empty, ZString.Empty, null);

			var collection = new ConsignorOrgRelatedPartySubsetCollection(organisation.AllRelatedParties);
			var warehouseParty = organisation.AllRelatedParties.ToList<OrgRelatedParty>().Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.WarehouseForwarder);

			AssertContainsExactElementsInAnyOrder("There should be no service provider creditor (SPC) party", warehouseParty, collection);
		}

		public void TestListNotContains_AuthorisedCargoReporter()
		{
			var organisation = Factory.New<OrgHeader>();
			var acrRelatedParty = organisation.AllRelatedParties.AddNew();
			acrRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.AuthorizedCargoReporter;
			var collection = new ConsignorOrgRelatedPartySubsetCollection(organisation.AllRelatedParties);
			AssertEquals(false, collection.Contains(acrRelatedParty));
		}

		public void TestListContains_WarehouseForwarder()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.AddRelatedParty(Factory.New<OrgHeader>().PK, RelatedPartyTypeList.Codes.WarehouseForwarder, RelatedPartyDirectionList.Codes.Forwarder, ZString.Empty, ZString.Empty, null);

			var collection = new ConsignorOrgRelatedPartySubsetCollection(organisation.AllRelatedParties);
			AssertContainsExactElementsInAnyOrder(organisation.AllRelatedParties, collection);
		}

		protected override void AssertSpecificDefaults(OrgRelatedParty relatedParty)
		{
			AssertEquals("Default direction:", RelatedPartyDirectionList.Codes.Pickup, relatedParty.PR_FreightDirection);
		}

		protected override ConsignorOrgRelatedPartySubsetCollection GetCollectionToTest()
		{
			return new ConsignorOrgRelatedPartySubsetCollection(Organisation.AllRelatedParties);
		}
	}
}
