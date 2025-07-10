using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ScaledTestResultByCategory))]
	sealed class ScaledTestResultByCategoryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestScore_NoSubmittedAnswers()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			AssertEquals(ZByte.Zero, new ScaledTestResultByCategory(campaignItem, campaign.QuestionCategories[0]).Score);
		}

		public void TestProperties()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			campaign.G0_Type = LearningCentreTestTypes.Codes.Scaled;
			LearningCentreQuestion question = campaign.Questions.AddNew();
			LearningCentreQuestion option1 = question.SubQuestions.AddNew();
			option1.HY_AnswerWeighting = 10;
			LearningCentreQuestion option2 = question.SubQuestions.AddNew();
			option2.HY_AnswerWeighting = 23;
			LearningCentreQuestion question2 = campaign.Questions.AddNew();
			LearningCentreQuestion option3 = question.SubQuestions.AddNew();
			option3.HY_AnswerWeighting = 5;
			LearningCentreQuestion option4 = question.SubQuestions.AddNew();
			option4.HY_AnswerWeighting = 3;
			LearningCentreQuestion scale1 = campaign.QuestionCategories[0].ScaleRanges.AddNew();
			scale1.HY_Min = 0;
			scale1.HY_Max = 20;
			scale1.HY_Question = "Note for Scale 1";
			LearningCentreQuestion scale2 = campaign.QuestionCategories[0].ScaleRanges.AddNew();
			scale2.HY_Min = 21;
			scale2.HY_Max = 40;
			scale2.HY_Question = "Note for Scale 2";

			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			examAnswerSet.PersistedAnswers.LoadOrCreateNew(option1, campaignItem).AnswerAsBool = true;
			examAnswerSet.PersistedAnswers.LoadOrCreateNew(option3, campaignItem).AnswerAsBool = true;
			ScaledTestResultByCategory testResult = new ScaledTestResultByCategory(campaignItem, campaign.QuestionCategories[0]);
			AssertEquals((ZByte)15, testResult.Score);
			AssertEquals("Note for Scale 1", testResult.ScoreNote);

			examAnswerSet.PersistedAnswers.LoadOrCreateNew(option2, campaignItem).AnswerAsBool = true;
			examAnswerSet.SubmittedAnswers.Load();
			testResult = new ScaledTestResultByCategory(campaignItem, campaign.QuestionCategories[0]);
			AssertEquals((ZByte)38, testResult.Score);
			AssertEquals("Note for Scale 2", testResult.ScoreNote);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			return new ScaledTestResultByCategory(campaignItem, campaign.QuestionCategories[0]);
		}
	}
}
