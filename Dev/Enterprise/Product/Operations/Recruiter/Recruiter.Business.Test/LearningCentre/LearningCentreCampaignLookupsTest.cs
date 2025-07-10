using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class LearningCentreCampaignLookupsTest : GlbCompanyCampaignLookupsTest
	{
		public void TestLookups()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			Assert(campaign.Lookups.CampaignTypeList.ContainsCode(Core.Constants.Recruiter.LearningCentreCampaignType));
			Assert(campaign.Lookups.CampaignTypeList.ContainsCode(CampaignTypeList.Codes.Survey));
			AssertNotNull(campaign.Lookups.TestTypes);
			AssertEquals(typeof(LearningCentreAnswerTypeList), campaign.Lookups.DefaultAnswerTypes.GetType());
		}
	}
}
