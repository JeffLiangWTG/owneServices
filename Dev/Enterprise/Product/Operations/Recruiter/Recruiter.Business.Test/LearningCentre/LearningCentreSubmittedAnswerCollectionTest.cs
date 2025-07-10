using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreSubmittedAnswerCollection))]
	sealed class LearningCentreSubmittedAnswerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LearningCentreSubmittedAnswerCollection>
	{
		public void TestLoadWithCategory()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			campaign.G0_Type = LearningCentreTestTypes.Codes.Scaled;
			campaign.QuestionCategories.RemoveAndDeleteAll();
			campaign.QuestionCategories.AddNew("IQ", "IQ test");
			campaign.QuestionCategories.AddNew("EQ", "EQ test");
			LearningCentreQuestion question1 = campaign.Questions.AddNew();
			question1.HY_QuestionCategory = "IQ";
			LearningCentreQuestion question2 = campaign.Questions.AddNew();
			question2.HY_QuestionCategory = "EQ";

			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);

			examAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem);
			examAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem);

			LearningCentreSubmittedAnswerCollection collection = new LearningCentreSubmittedAnswerCollection(campaignItem, "IQ");
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals(question1, collection[0].Question);

			collection = new LearningCentreSubmittedAnswerCollection(campaignItem, "EQ");
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals(question2, collection[0].Question);
		}

		public void TestEnumerables()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			campaign.G0_DefaultAnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			LearningCentreQuestion question1 = campaign.Questions.AddNew();
			question1.HY_ExamCorrectAnswer = "1";
			LearningCentreQuestion question2 = campaign.Questions.AddNew();
			question2.HY_ExamCorrectAnswer = "1";
			LearningCentreQuestion question3 = campaign.Questions.AddNew();
			question3.HY_ExamCorrectAnswer = "1";
			LearningCentreQuestion question4 = campaign.Questions.AddNew();
			question4.HY_ExamCorrectAnswer = "1";
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);

			examAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem).HZ_Answer = "1";
			examAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem).HZ_Answer = "2";
			examAnswerSet.PersistedAnswers.LoadOrCreateNew(question3, campaignItem).HZ_Answer = "1";
			examAnswerSet.PersistedAnswers.LoadOrCreateNew(question4, campaignItem).HZ_Answer = "";

			LearningCentreSubmittedAnswerCollection collection = new LearningCentreSubmittedAnswerCollection(campaignItem);
			collection.Load();
			AssertEquals(1, collection.IncorrectAnswers.Count());
			AssertEquals(question2, collection.IncorrectAnswers.First().Question);
			AssertEquals(2, collection.CorrectAnswers.Count());
			AssertEquals(1, collection.CorrectAnswers.Count(s => s.Question == question1));
			AssertEquals(1, collection.CorrectAnswers.Count(s => s.Question == question3));
			AssertEquals(1, collection.EmptyAnswers.Count());
			AssertEquals(question4, collection.EmptyAnswers.First().Question);
		}

		protected override LearningCentreSubmittedAnswerCollection GetCollectionToTest()
		{
			LearningCentreCampaignItem campaignItem = Factory.New<LearningCentreCampaignItem>();
			return new LearningCentreSubmittedAnswerCollection(campaignItem);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			LearningCentreCampaignItem campaignItem = Factory.New<LearningCentreCampaignItem>();
			LearningCentreQuestion question = Factory.New<LearningCentreQuestion>();
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			return new LearningCentreSubmittedAnswer(campaignItem, question);
		}
	}
}
