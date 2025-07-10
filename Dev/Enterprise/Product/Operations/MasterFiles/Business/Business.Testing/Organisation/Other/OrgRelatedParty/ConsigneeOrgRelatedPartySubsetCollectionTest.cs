using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsigneeOrgRelatedPartySubsetCollection))]
	sealed class ConsigneeOrgRelatedPartySubsetCollectionTest : OrgRelatedPartySubsetCollectionTest<ConsigneeOrgRelatedPartySubsetCollection>
	{
		public void TestListNotContains_ServiceProviderCreditor()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.AddRelatedParty(Factory.New<OrgHeader>().PK, RelatedPartyTypeList.Codes.WarehouseForwarder, RelatedPartyDirectionList.Codes.Forwarder, ZString.Empty, ZString.Empty, null);
			organisation.AddRelatedParty(Factory.New<OrgHeader>().PK, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Delivery, ZString.Empty, ZString.Empty, null);

			var collection = new ConsigneeOrgRelatedPartySubsetCollection(organisation.AllRelatedParties);
			var warehouseParty = organisation.AllRelatedParties.ToList<OrgRelatedParty>().Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.WarehouseForwarder);

			AssertContainsExactElementsInAnyOrder("There should be no service provider creditor (SPC) party", warehouseParty, collection);
		}

		public void TestListNotContains_AuthorisedCargoReporter()
		{
			var organisation = Factory.New<OrgHeader>();
			var acrRelatedParty = organisation.AllRelatedParties.AddNew();
			acrRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.AuthorizedCargoReporter;
			var collection = new ConsigneeOrgRelatedPartySubsetCollection(organisation.AllRelatedParties);
			AssertEquals(false, collection.Contains(acrRelatedParty));
		}

		public void TestListContains_WarehouseForwarder()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.AddRelatedParty(Factory.New<OrgHeader>().PK, RelatedPartyTypeList.Codes.WarehouseForwarder, RelatedPartyDirectionList.Codes.Forwarder, ZString.Empty, ZString.Empty, null);

			var collection = new ConsigneeOrgRelatedPartySubsetCollection(organisation.AllRelatedParties);
			AssertContainsExactElementsInAnyOrder(organisation.AllRelatedParties, collection);
		}

		protected override ConsigneeOrgRelatedPartySubsetCollection GetCollectionToTest()
		{
			return new ConsigneeOrgRelatedPartySubsetCollection(Organisation.AllRelatedParties);
		}

		protected override void AssertSpecificDefaults(OrgRelatedParty relatedParty)
		{
			AssertEquals("Default direction:", RelatedPartyDirectionList.Codes.Delivery, relatedParty.PR_FreightDirection);
		}
	}
}
