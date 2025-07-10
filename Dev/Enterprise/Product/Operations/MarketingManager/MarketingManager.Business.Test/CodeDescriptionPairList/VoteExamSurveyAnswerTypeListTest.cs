using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class VoteExamSurveyAnswerTypeListTest : TestCaseWithFactory
	{
		public void TestGetAnswerTypeList_VotingItem()
		{
			GlbCompanyCampaign voteCampaign = NewCampaign(CampaignTypeList.Codes.Voting);
			VoteExamSurveyQuestion votingItem = voteCampaign.VoteHeader.SubQuestions.AddNew();
			VoteExamSurveyAnswerTypeList answerTypeList = new VoteExamSurveyAnswerTypeList(votingItem);
			AssertEquals(2, answerTypeList.Count);
			AssertContainsCodeDescriptionPair(answerTypeList, VoteExamSurveyAnswerTypeList.Codes.VotingItem, VoteExamSurveyAnswerTypeList.Descriptions.VotingItem);
			AssertContainsCodeDescriptionPair(answerTypeList, VoteExamSurveyAnswerTypeList.Codes.Header, VoteExamSurveyAnswerTypeList.Descriptions.Header);
		}

		public void TestGetAnswerTypeList_VotingHeader()
		{
			GlbCompanyCampaign voteCampaign = NewCampaign(CampaignTypeList.Codes.Voting);
			VoteExamSurveyAnswerTypeList answerTypeList = new VoteExamSurveyAnswerTypeList(voteCampaign.VoteHeader);
			AssertEquals(2, answerTypeList.Count);
			AssertContainsCodeDescriptionPair(answerTypeList, VoteExamSurveyAnswerTypeList.Codes.RankedVote, VoteExamSurveyAnswerTypeList.Descriptions.RankedVote);
			AssertContainsCodeDescriptionPair(answerTypeList, VoteExamSurveyAnswerTypeList.Codes.UnrankedVote, VoteExamSurveyAnswerTypeList.Descriptions.UnrankedVote);
		}

		public void TestGetAnswerTypeList_Survey()
		{
			GlbCompanyCampaign campaign = NewCampaign(CampaignTypeList.Codes.Survey);
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			VoteExamSurveyAnswerTypeList list = new VoteExamSurveyAnswerTypeList(question);
			AssertSurveyAnswerTypeList(list);
		}

		public void TestGetAnswerTypeList_SurveySubQuestion()
		{
			GlbCompanyCampaign campaign = NewCampaign(CampaignTypeList.Codes.Survey);
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			VoteExamSurveyQuestion subQuestion = question.SubQuestions.AddNew();
			VoteExamSurveyAnswerTypeList answerTypeList = new VoteExamSurveyAnswerTypeList(subQuestion);
			AssertEquals(1, answerTypeList.Count);
			AssertContainsCodeDescriptionPair(answerTypeList, VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption, VoteExamSurveyAnswerTypeList.Descriptions.MultipleChoiceOption);
		}

		public void TestGetAnswerTypeList_FromCampaign()
		{
			GlbCompanyCampaign campaign = NewCampaign(CampaignTypeList.Codes.Broadcast);
			VoteExamSurveyAnswerTypeList answerTypeList = new VoteExamSurveyAnswerTypeList(campaign);
			AssertEquals(0, answerTypeList.Count);

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			answerTypeList = new VoteExamSurveyAnswerTypeList(campaign);
			AssertEquals(2, answerTypeList.Count);
			AssertContainsCodeDescriptionPair(answerTypeList, VoteExamSurveyAnswerTypeList.Codes.RankedVote, VoteExamSurveyAnswerTypeList.Descriptions.RankedVote);
			AssertContainsCodeDescriptionPair(answerTypeList, VoteExamSurveyAnswerTypeList.Codes.UnrankedVote, VoteExamSurveyAnswerTypeList.Descriptions.UnrankedVote);

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			answerTypeList = new VoteExamSurveyAnswerTypeList(campaign);
			AssertSurveyAnswerTypeList(answerTypeList);
		}

		public void TestGetAnswerTypeList_FromQuestionWithoutCampaign()
		{
			VoteExamSurveyQuestion question = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswerTypeList list = new VoteExamSurveyAnswerTypeList(question);
			AssertEquals(0, list.Count);
		}

		GlbCompanyCampaign NewCampaign(string type)
		{
			GlbCompanyCampaign result = Factory.New<GlbCompanyCampaign>();
			result.G0_BroadcastVoteSurveyExam = type;
			return result;
		}

		void AssertSurveyAnswerTypeList(CodeDescriptionPairList list)
		{
			AssertEquals(8, list.Count);
			AssertContainsCodeDescriptionPair(list, VoteExamSurveyAnswerTypeList.Codes.YesNo, VoteExamSurveyAnswerTypeList.Descriptions.YesNo);
			AssertContainsCodeDescriptionPair(list, VoteExamSurveyAnswerTypeList.Codes.NumericScale, VoteExamSurveyAnswerTypeList.Descriptions.NumericScale);
			AssertContainsCodeDescriptionPair(list, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, VoteExamSurveyAnswerTypeList.Descriptions.MultipleChoice);
			AssertContainsCodeDescriptionPair(list, VoteExamSurveyAnswerTypeList.Codes.LikertScale, VoteExamSurveyAnswerTypeList.Descriptions.LikertScale);
			AssertContainsCodeDescriptionPair(list, VoteExamSurveyAnswerTypeList.Codes.FreeText, VoteExamSurveyAnswerTypeList.Descriptions.FreeText);
			AssertContainsCodeDescriptionPair(list, VoteExamSurveyAnswerTypeList.Codes.Percentage, VoteExamSurveyAnswerTypeList.Descriptions.Percentage);
			AssertContainsCodeDescriptionPair(list, VoteExamSurveyAnswerTypeList.Codes.TrueFalse, VoteExamSurveyAnswerTypeList.Descriptions.TrueFalse);
			AssertContainsCodeDescriptionPair(list, VoteExamSurveyAnswerTypeList.Codes.Header, VoteExamSurveyAnswerTypeList.Descriptions.Header);
		}

		void AssertContainsCodeDescriptionPair(CodeDescriptionPairList list, string expectedCode, string expectedDescription)
		{
			string failureMessage = string.Format("Should contain {0} - {1}", expectedCode, expectedDescription);
			AssertEquals(failureMessage, expectedDescription, list.GetDescriptionFromCode(expectedCode));
		}
	}
}
