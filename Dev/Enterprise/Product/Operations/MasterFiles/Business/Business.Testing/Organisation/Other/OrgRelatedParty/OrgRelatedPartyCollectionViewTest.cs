using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRelatedPartyCollectionView))]
	class OrgRelatedPartyCollectionViewTest : BusinessObjectCollectionViewTestCase<OrgRelatedPartyCollectionView>
	{
		protected new OrgRelatedPartyCollectionView Collection
		{
			get { return base.Collection; }
		}

		protected override OrgRelatedPartyCollectionView GetCollectionToTest()
		{
			OrgRelatedPartyCollection relatedPartyCollection = new OrgRelatedPartyCollection(Factory, new ZQuery());
			OrgRelatedPartyCollectionView view = new OrgRelatedPartyCollectionView(relatedPartyCollection);
			return view;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrgRelatedParty party = Factory.New<OrgRelatedParty>();
			party.PR_PartyType = "APN";
			return party;
		}

		public void TestIsThisPartOfTheCollection()
		{
			var party1 = Factory.NewWithValidTestData<OrgRelatedParty>();
			party1.PR_PartyType = "AAA";
			party1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			var party2 = Factory.NewWithValidTestData<OrgRelatedParty>();
			party2.PR_PartyType = "AAA";
			party2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			var party3 = Factory.NewWithValidTestData<OrgRelatedParty>();
			party3.PR_PartyType = "BBB";
			party3.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
			var party4 = Factory.NewWithValidTestData<OrgRelatedParty>();
			party4.PR_PartyType = "BBB";
			party4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;

			OrgRelatedPartyCollection relatedPartyCollection = new OrgRelatedPartyCollection(Factory, new ZQuery());
			relatedPartyCollection.Add(party1);
			relatedPartyCollection.Add(party2);
			relatedPartyCollection.Add(party3);
			relatedPartyCollection.Add(party4);

			OrgRelatedPartyCollectionView view = new OrgRelatedPartyCollectionView(relatedPartyCollection);

			view.FilterByPartyType("AAA", "");
			AssertEquals("2", 2, view.Count);

			view.FilterByPartyType("BBB", "");
			AssertEquals("2", 2, view.Count);

			view.FilterByPartyType("BBB", RelatedPartyDirectionList.Codes.PickupAndDelivery);
			AssertEquals("1", 1, view.Count);

			view.FilterByPartyType("BBB", RelatedPartyDirectionList.Codes.Sales);
			AssertEquals("1", 1, view.Count);

			view.ClearRelatedPartyFilters();
			AssertEquals("4", 4, view.Count);
		}

		public void TestRemoveAndDelete()
		{
			var relatedPartyCollection = new OrgRelatedPartyCollection(Factory, new ZQuery());
			var relatedPartyCollectionView = new OrgRelatedPartyCollectionView(relatedPartyCollection);
			var orgRelatedParty = relatedPartyCollectionView.AddNew();
			orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.Added;
			relatedPartyCollectionView.RemoveAndDelete(orgRelatedParty);
			AssertEquals("Count", 1, relatedPartyCollectionView.Count);
			AssertEquals("Status", CSARelatedPartyStatusList.Codes.DeletePendingSeeCustomsMessagingMenu, relatedPartyCollectionView[0].PR_CustomsStatus);
			orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.AddPendingSeeCustomsMessagingMenu;
			orgRelatedParty.Delete();
			AssertEquals("Count", 0, relatedPartyCollectionView.Count);
		}
	}
}
