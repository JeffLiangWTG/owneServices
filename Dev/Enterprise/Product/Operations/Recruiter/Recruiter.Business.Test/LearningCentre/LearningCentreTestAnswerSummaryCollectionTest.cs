using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreTestAnswerSummaryCollection))]
	sealed class LearningCentreTestAnswerSummaryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LearningCentreTestAnswerSummaryCollection>
	{
		#region Implementation

		protected override LearningCentreTestAnswerSummaryCollection GetCollectionToTest()
		{
			return new LearningCentreTestAnswerSummaryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LearningCentreTestAnswerSummary();
		}

		#endregion

		LearningCentreQuestion GetSurveyQuestion()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			question1.HY_Question = "Q1";
			VoteExamSurveyQuestion subQuestion1a = question1.SubQuestions.AddNew();
			subQuestion1a.HY_Question = "1a";
			subQuestion1a.ActualOrder = 1;
			VoteExamSurveyQuestion subQuestion1b = question1.SubQuestions.AddNew();
			subQuestion1b.HY_Question = "1b";
			subQuestion1b.ActualOrder = 2;
			LearningCentreCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem1);
			var votedItems = voteExamSurveyAnswerSet.PersistedAnswers.GetCompletedAnswers(campaignItem1);
			VoteExamSurveyAnswer answer11 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(subQuestion1a, campaignItem1);
			answer11.AnswerAsBool = true;
			VoteExamSurveyAnswer answer12 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(subQuestion1b, campaignItem1);
			answer12.AnswerAsBool = false;

			LearningCentreCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			var voteExamSurveyAnswerSet2 = new VoteExamSurveyAnswerSet(Factory, campaignItem2);
			var votedItems2 = voteExamSurveyAnswerSet2.PersistedAnswers.GetCompletedAnswers(campaignItem2);
			VoteExamSurveyAnswer answer21 = voteExamSurveyAnswerSet2.PersistedAnswers.LoadOrCreateNew(subQuestion1a, campaignItem2);
			answer21.AnswerAsBool = false;
			VoteExamSurveyAnswer answer22 = voteExamSurveyAnswerSet2.PersistedAnswers.LoadOrCreateNew(subQuestion1b, campaignItem2);
			answer22.AnswerAsBool = true;

			return question1;
		}

		public void TestAnalyzeAnswerSummaries()
		{
			var question = GetSurveyQuestion();
			var summaryCollection = new LearningCentreTestSummaryPerQuestionCollection(question);
			summaryCollection.Load();

			var summary1 = summaryCollection[0] as LearningCentreTestSummary;
			var answerSummaries = summary1.AnswerSummaries;
			AssertEquals(2, answerSummaries.Count);
			AssertEquals(1, answerSummaries[0].AnsweredCount);
			AssertEquals("1a", answerSummaries[0].AnswerText);
			AssertEquals(1, answerSummaries[1].AnsweredCount);
			AssertEquals("1b", answerSummaries[1].AnswerText);
		}

		public void TestAnswerSummaryComparer()
		{
			var answerSummaries = new LearningCentreTestAnswerSummaryCollection();
			var answerSummary3 = new LearningCentreTestAnswerSummary();
			answerSummary3.OptionNumber = 3;
			answerSummary3.AnsweredCount = 1;
			answerSummary3.AnswerText = "Option 3";
			answerSummaries.Add(answerSummary3);

			var answerSummary1 = new LearningCentreTestAnswerSummary();
			answerSummary1.OptionNumber = 1;
			answerSummary1.AnsweredCount = 1;
			answerSummary1.AnswerText = "Option 1";
			answerSummaries.Add(answerSummary1);

			var answerSummary2 = new LearningCentreTestAnswerSummary();
			answerSummary2.OptionNumber = 2;
			answerSummary2.AnsweredCount = 1;
			answerSummary2.AnswerText = "Option 2";
			answerSummaries.Add(answerSummary2);

			answerSummaries.Sort(new AnswerSummaryComparer());

			AssertEquals(3, answerSummaries.Count);
			AssertEquals(1, answerSummaries[0].OptionNumber);
			AssertEquals(2, answerSummaries[1].OptionNumber);
			AssertEquals(3, answerSummaries[2].OptionNumber);
		}
	}
}
