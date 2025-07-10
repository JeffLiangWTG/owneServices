using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignItemCampaignDependentCollection))]
	sealed class GlbCompanyCampaignItemCampaignDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			return new GlbCompanyCampaignItemCampaignDependentCollection(campaign);
		}

		public void TestAllowNew()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaignItemCampaignDependentCollection campaigns = new GlbCompanyCampaignItemCampaignDependentCollection(campaign);
			Assert("Collection should not allow new", !campaigns.AllowNew);
		}

		public void TestFindOrCreateNewIfNotExist()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals(0, campaign.CampaignsItemsSent.Count);

			OrgContact contact = Factory.New<OrgContact>();
			campaign.CampaignsItemsSent.FindOrCreateNewIfNotExist(new GlbCompanyCampaignItem.RecipientInfo(contact.PK, OrgContactSchema.Constants.Prefix));
			AssertEquals(1, campaign.CampaignsItemsSent.Count);
			AssertEquals(contact.PK, campaign.CampaignsItemsSent[0].G8_RecipientID);
			AssertEquals(OrgContactSchema.Constants.Prefix, campaign.CampaignsItemsSent[0].G8_RecipientTableCode);
		}

		public void TestFindByRecipientPK()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals(0, campaign.CampaignsItemsSent.Count);

			ZGuid guid1 = ZGuid.NewZGuid();
			ZGuid guid2 = ZGuid.NewZGuid();

			GlbCompanyCampaignItem item1 = campaign.CampaignsItemsSent.AddNew();
			item1.G8_RecipientID = guid1;
			GlbCompanyCampaignItem item2 = campaign.CampaignsItemsSent.AddNew();
			item2.G8_RecipientID = guid2;

			AssertEquals(item1, campaign.CampaignsItemsSent.FindByRecipientPK(guid1));
			AssertEquals(item2, campaign.CampaignsItemsSent.FindByRecipientPK(guid2));
		}

		public void TestAddNewWithRecipientInfo()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals(0, campaign.CampaignsItemsSent.Count);

			OrgContact contact = Factory.New<OrgContact>();
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew(new GlbCompanyCampaignItem.RecipientInfo(contact.PK, OrgContactSchema.Constants.Prefix));
			AssertEquals(contact.PK, item.G8_RecipientID);
			AssertEquals(OrgContactSchema.Constants.Prefix, item.G8_RecipientTableCode);
		}
	}
}
