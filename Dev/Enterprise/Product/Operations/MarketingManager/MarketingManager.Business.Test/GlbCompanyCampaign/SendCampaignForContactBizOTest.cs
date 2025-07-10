using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SendCampaignForContactBizO))]
	sealed class SendCampaignForContactBizOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContact()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Samuel";
			SendCampaignForContactBizO sendCampaignBizO = new SendCampaignForContactBizO(contact);

			AssertEquals("Should be the same contact", contact.PK, sendCampaignBizO.Contact.PK);
			AssertEquals("Should be the same contact", contact.OC_ContactName, sendCampaignBizO.Contact.OC_ContactName);
		}

		public void TestCampaign()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Hot Sale";
			SendCampaignForContactBizO sendCampaignBizO = new SendCampaignForContactBizO(contact);

			sendCampaignBizO.CampaignPK = campaign.PK;

			AssertEquals("Should be the same campaign", campaign.PK, sendCampaignBizO.Campaign.PK);
			AssertEquals("Should be the same campaign", campaign.G0_CampaignName, sendCampaignBizO.Campaign.G0_CampaignName);
		}

		public void TestCampaignList()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			SendCampaignForContactBizO sendCampaignBizO = new SendCampaignForContactBizO(contact);

			GlbCompanyCampaign campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign1.G0_CampaignName = "Hot Sale";
			campaign1.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			GlbCompanyCampaign campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign2.G0_CampaignName = "Everything must go";
			campaign2.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			GlbCompanyCampaign campaign3 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign3.G0_CampaignName = "Exam Exam";
			campaign3.G0_BroadcastVoteSurveyExam = "CRT";

			AssertNotNull("Should not be null", sendCampaignBizO.CampaignList);
			sendCampaignBizO.CampaignList.Load();
			AssertEquals("Should be only two filtered campaigns", 2, sendCampaignBizO.CampaignList.Count);

			GlbCompanyCampaignItem campaignItem = (GlbCompanyCampaignItem)contact.Campaigns.AddNew();
			campaignItem.G8_G0 = campaign1.PK;
			Factory.Save();

			sendCampaignBizO = new SendCampaignForContactBizO(contact);
			sendCampaignBizO.CampaignList.Load();
			AssertEquals("Should be only one filtered campaign", 1, sendCampaignBizO.CampaignList.Count);
		}

		public void TestValidation()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			SendCampaignForContactBizO sendCampaignBizO = new SendCampaignForContactBizO(contact);

			sendCampaignBizO.RunPreSaveValidation();
			Assert("No campaign", sendCampaignBizO.HasErrors);

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			sendCampaignBizO.CampaignPK = campaign.PK;

			sendCampaignBizO.RunPreSaveValidation();
			Assert("Should be ok", !sendCampaignBizO.HasErrors);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			return new SendCampaignForContactBizO(contact);
		}
	}
}
