using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class SendCampaignForContactBizOValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCampaignPK()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			SendCampaignForContactBizO sendCampaignBizO = new SendCampaignForContactBizO(contact);

			SendCampaignForContactBizOValidation validation = new SendCampaignForContactBizOValidation(sendCampaignBizO);
			validation.ValidateAll();
			Assert("No campaign", sendCampaignBizO.HasErrors);

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			sendCampaignBizO.CampaignPK = campaign.PK;
			validation.ValidateAll();
			Assert("Should be ok", !sendCampaignBizO.HasErrors);

			GlbCompanyCampaign campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign1.G0_CampaignName = "Hot Sale";
			campaign1.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			GlbCompanyCampaign campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign2.G0_CampaignName = "Everything must go";
			campaign2.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			campaign.G0_BroadcastVoteSurveyExam = "CRT";
			validation.ValidateAll();
			Assert("Not a valid campaign", sendCampaignBizO.HasErrors);
		}
	}
}
