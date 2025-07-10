using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class LearningCentreAnswerTypeListTest : TestCaseWithFactory
	{
		public void TestList_ScaleRange()
		{
			LearningCentreQuestion question = Factory.New<LearningCentreQuestion>();
			question.HY_AnswerType = LearningCentreAnswerTypeList.Codes.ScaleRange;
			LearningCentreAnswerTypeList list = new LearningCentreAnswerTypeList(question);
			AssertEquals(1, list.Count);
			AssertEquals(LearningCentreAnswerTypeList.Codes.ScaleRange, list[0].Code);
		}

		public void TestList_Exam()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question = campaign.Questions.AddNew();
			LearningCentreAnswerTypeList list = new LearningCentreAnswerTypeList(question);
			AssertEquals(5, list.Count);
			AssertEquals(MarketingManager.Business.VoteExamSurveyAnswerTypeList.Descriptions.Header, list[MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.Header].Description);
			AssertEquals(MarketingManager.Business.VoteExamSurveyAnswerTypeList.Descriptions.MultipleChoice, list[MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.MultipleChoice].Description);
			AssertEquals(MarketingManager.Business.VoteExamSurveyAnswerTypeList.Descriptions.YesNo, list[MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.YesNo].Description);
			AssertEquals(MarketingManager.Business.VoteExamSurveyAnswerTypeList.Descriptions.TrueFalse, list[MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.TrueFalse].Description);
			AssertEquals(MarketingManager.Business.VoteExamSurveyAnswerTypeList.Descriptions.NumericScale, list[MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.NumericScale].Description);
		}

		public void TestList_ScaledTest()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			campaign.G0_Type = LearningCentreTestTypes.Codes.Scaled;
			LearningCentreQuestion question = campaign.Questions.AddNew();
			LearningCentreAnswerTypeList list = new LearningCentreAnswerTypeList(question);
			AssertEquals(5, list.Count);
			AssertEquals(MarketingManager.Business.VoteExamSurveyAnswerTypeList.Descriptions.Header, list[MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.Header].Description);
			AssertEquals(MarketingManager.Business.VoteExamSurveyAnswerTypeList.Descriptions.MultipleChoice, list[MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.MultipleChoice].Description);
			AssertEquals(MarketingManager.Business.VoteExamSurveyAnswerTypeList.Descriptions.YesNo, list[MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.YesNo].Description);
			AssertEquals(MarketingManager.Business.VoteExamSurveyAnswerTypeList.Descriptions.TrueFalse, list[MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.TrueFalse].Description);
			AssertEquals(MarketingManager.Business.VoteExamSurveyAnswerTypeList.Descriptions.LikertScale, list[MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.LikertScale].Description);
		}

		public void TestList_MultipleChoiceQuestion()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			campaign.G0_Type = LearningCentreTestTypes.Codes.Scaled;
			LearningCentreQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			LearningCentreQuestion subQuestion = question.SubQuestions.AddNew();
			LearningCentreAnswerTypeList list = new LearningCentreAnswerTypeList(subQuestion);
			AssertEquals(1, list.Count);
			AssertEquals(MarketingManager.Business.VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption, list[0].Code);
		}
	}
}
