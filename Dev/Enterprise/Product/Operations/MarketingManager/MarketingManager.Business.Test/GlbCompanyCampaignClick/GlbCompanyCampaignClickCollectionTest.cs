
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignClickCollection))]
	sealed class GlbCompanyCampaignClickCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbCompanyCampaignClickCollection>
	{
		public void TestCollectionRelationshipQuery()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var contact1 = organisation.Contacts.AddNew();
			contact1.OC_ContactName = "Contact1";
			var contact2 = organisation.Contacts.AddNew();
			contact2.OC_ContactName = "Contact2";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = campaign1.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			var link1 = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			link1.GCL_Context = "AAAAA";
			link1.GCL_G0_Campaign = campaign.PK;
			link1.GCL_IsImage = false;
			link1.GCL_IsTracked = true;
			link1.GCL_URL = "http://www.aaa.com";

			var link2 = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			link2.GCL_Context = "BBBBB";
			link2.GCL_G0_Campaign = campaign1.PK;
			link2.GCL_IsImage = false;
			link2.GCL_IsTracked = true;
			link2.GCL_URL = "http://www.aaa.com";

			var click1 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click1.GCC_ClickTimeUtc = ZDateTime.UtcNow;
			click1.GCC_G8_Recipient = campaignItem1.PK;
			click1.GCC_GCL = link1.PK;

			var click2 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click2.GCC_ClickTimeUtc = ZDateTime.UtcNow;
			click2.GCC_G8_Recipient = campaignItem2.PK;
			click2.GCC_GCL = link2.PK;

			Factory.Save();

			GlbCompanyCampaignClickCollection collection = new GlbCompanyCampaignClickCollection(campaign);
			AssertEquals(1, collection.Count);
		}

		protected override GlbCompanyCampaignClickCollection GetCollectionToTest()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			return new GlbCompanyCampaignClickCollection(campaignItem);
		}
	}
}
