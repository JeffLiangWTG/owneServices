using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreTestSummaryPerQuestionCollection))]
	sealed class LearningCentreTestSummaryPerQuestionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LearningCentreTestSummaryPerQuestionCollection>
	{
		#region Implementation

		protected override LearningCentreTestSummaryPerQuestionCollection GetCollectionToTest()
		{
			return new LearningCentreTestSummaryPerQuestionCollection(GetSurveyQuestion());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var campaign = Factory.New<LearningCentreCampaign>();
			var question = campaign.Questions.AddNew();
			return new LearningCentreTestSummary(question);
		}

		#endregion

		LearningCentreQuestion GetSurveyQuestion()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question1 = Factory.New<LearningCentreQuestion>();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			question1.HY_Question = "question1";
			question1.HY_G0 = campaign.PK;

			LearningCentreCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem1);
			var votedItems = voteExamSurveyAnswerSet.PersistedAnswers.GetCompletedAnswers(campaignItem1);
			VoteExamSurveyAnswer answer11 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem1);
			answer11.AnswerAsInt = 1;
			return question1;
		}

		public void TestSurveyQuestions()
		{
			var question = GetSurveyQuestion();
			var summaryCollection = new LearningCentreTestSummaryPerQuestionCollection(question);
			AssertEquals("question1", summaryCollection.Question.HY_Question);
		}

		public void TestLoad()
		{
			var question = GetSurveyQuestion();
			var summaryCollection = new LearningCentreTestSummaryPerQuestionCollection(question);

			AssertEquals(0, summaryCollection.Count);

			summaryCollection.Load();

			var summary1 = summaryCollection[0];
			AssertEquals(1, summaryCollection.Count);
			AssertEquals(1, summary1.RecipientAnswered);
		}
	}
}
