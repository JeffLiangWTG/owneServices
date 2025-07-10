using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreTestSummaryCollection))]
	sealed class LearningCentreTestSummaryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LearningCentreTestSummaryCollection>
	{
		#region Implementation

		protected override LearningCentreTestSummaryCollection GetCollectionToTest()
		{
			return new LearningCentreTestSummaryCollection(GetSurveyQuestions());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var campaign = Factory.New<LearningCentreCampaign>();
			var question = campaign.Questions.AddNew();
			return new LearningCentreTestSummary(question);
		}

		#endregion

		VoteExamSurveyQuestionSet GetSurveyQuestions()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question1 = Factory.New<LearningCentreQuestion>();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			question1.HY_Question = "question1";
			question1.HY_G0 = campaign.PK;
			LearningCentreQuestion question2 = Factory.New<LearningCentreQuestion>();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			question2.HY_Question = "question2";
			question2.HY_G0 = campaign.PK;

			LearningCentreCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem1);
			var answer11 = examAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem1);
			answer11.AnswerAsInt = 1;

			LearningCentreQuestionCollection collection = new LearningCentreQuestionCollection(campaign, true);

			return collection;
		}

		public void TestSurveyQuestions()
		{
			var questions = GetSurveyQuestions();
			var summaryCollection = new LearningCentreTestSummaryCollection(questions);

			AssertEquals(2, summaryCollection.SurveyQuestions.Count);

			AssertEquals("question1", summaryCollection.SurveyQuestions[0].HY_Question);
			AssertEquals("question2", summaryCollection.SurveyQuestions[1].HY_Question);
		}

		public void TestLoad()
		{
			var questions = GetSurveyQuestions();
			var summaryCollection = new LearningCentreTestSummaryCollection(questions);

			AssertEquals(0, summaryCollection.Count);

			summaryCollection.Load();

			var summary1 = summaryCollection[0];
			AssertEquals(2, summaryCollection.Count);
			AssertEquals(1, summary1.RecipientAnswered);
		}
	}
}
