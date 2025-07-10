using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(UnsubscribeContactsItemCollection))]
	class UnsubscribeContactsItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UnsubscribeContactsItemCollection>
	{
		#region Implementation

		protected override UnsubscribeContactsItemCollection GetCollectionToTest() => new UnsubscribeContactsItemCollection(Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection() => new UnsubscribeContactsItem(campaignItem);
		UnsubscribeContactsItemCollection GetCollection() => GetCollectionToTest();

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Edwards";
			contact.OC_Email = "john@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			Factory.Save();
		}

		GlbCompanyCampaignItem campaignItem;

		#endregion

		public void TestAddContactsEmptyList()
		{
			var collection = GetCollection();
			collection.AddItems(null);
			AssertEquals("Collection should be empty", false, collection.Any());
			collection.AddItems(System.Array.Empty<GlbCompanyCampaignItem>());
			AssertEquals("Collection should be empty", false, collection.Any());
		}

		public void TestAddContactsEmptyEmail()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = nameof(contact1);
			contact1.OC_Email = "";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = nameof(contact2);
			contact2.OC_Email = "asdf";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			Factory.Save();

			var collection = GetCollection();
			collection.AddItems(new[] { campaignItem1, campaignItem2 });
			AssertEquals("Collection should be only one", 1, collection.Count);
			AssertEquals(campaignItem2.PK, collection[0].GlbCompanyCampaignPk);
		}
	}
}
