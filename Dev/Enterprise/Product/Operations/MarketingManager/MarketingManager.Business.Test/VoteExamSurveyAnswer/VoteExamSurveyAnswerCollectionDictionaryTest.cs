using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class VoteExamSurveyAnswerCollectionDictionaryTest : TestCaseWithFactory
	{
		public void TestAddVoteExamSurveyAnswerCollection()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			VoteExamSurveyQuestion surveyQuestion1 = campaign.Questions.AddNew();
			surveyQuestion1.HY_Question = "survey1";
			VoteExamSurveyQuestion surveyQuestion2 = campaign.Questions.AddNew();
			surveyQuestion2.HY_Question = "survey2";
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			VoteExamSurveyAnswerCollectionDictionary dict = new VoteExamSurveyAnswerCollectionDictionary(voteExamSurveyAnswerSet);
			AssertEquals("1 VoteExamSurveyAnswerCollection should be added.", 1, dict.Count);

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, new List<GlbCompanyCampaignItem> { campaignItem, campaignItem2 });
			dict = new VoteExamSurveyAnswerCollectionDictionary(voteExamSurveyAnswerSet);
			AssertEquals("2 VoteExamSurveyAnswerCollections should be added.", 2, dict.Count);
		}

		public void TestFindVoteExamSurveyAnswerCollection()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			VoteExamSurveyQuestion surveyQuestion1 = campaign.Questions.AddNew();
			surveyQuestion1.HY_Question = "survey1";
			VoteExamSurveyQuestion surveyQuestion2 = campaign.Questions.AddNew();
			surveyQuestion2.HY_Question = "survey2";
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			VoteExamSurveyAnswerCollectionDictionary dict = new VoteExamSurveyAnswerCollectionDictionary(voteExamSurveyAnswerSet);
			var answers = dict.GetCampaignItemAnswers(campaignItem);
			Assert("It should find the campaignItem's answers", answers != null);

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			GlbCompanyCampaignItem campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, new List<GlbCompanyCampaignItem> { campaignItem, campaignItem2 });
			dict = new VoteExamSurveyAnswerCollectionDictionary(voteExamSurveyAnswerSet);
			answers = dict.GetCampaignItemAnswers(campaignItem2);
			Assert("It should find the campaignItem's answers", answers != null);

			answers = dict.GetCampaignItemAnswers(campaignItem3);
			Assert("It should not find the campaignItem's answers", answers == null);
		}
	}
}
