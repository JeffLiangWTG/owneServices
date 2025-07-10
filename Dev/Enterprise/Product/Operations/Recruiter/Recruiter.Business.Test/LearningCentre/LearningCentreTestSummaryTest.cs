using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreTestSummary))]
	sealed class LearningCentreTestSummaryTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.New<LearningCentreCampaign>();
			var question = campaign.Questions.AddNew();
			return new LearningCentreTestSummary(question);
		}

		#endregion

		LearningCentreQuestion GetSurveyQuestion1()
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

		LearningCentreQuestion GetSurveyQuestion2()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			question2.HY_Question = "Q1";
			LearningCentreCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem1);
			var votedItems = voteExamSurveyAnswerSet.PersistedAnswers.GetCompletedAnswers(campaignItem1);
			VoteExamSurveyAnswer answer1 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem1);
			answer1.HZ_Answer = "3";
			LearningCentreCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			var voteExamSurveyAnswerSet2 = new VoteExamSurveyAnswerSet(Factory, campaignItem2);
			var votedItems2 = voteExamSurveyAnswerSet2.PersistedAnswers.GetCompletedAnswers(campaignItem2);
			VoteExamSurveyAnswer answer2 = voteExamSurveyAnswerSet2.PersistedAnswers.LoadOrCreateNew(question2, campaignItem2);
			answer2.HZ_Answer = "";
			return question2;
		}

		public void TestAnalyzeCampaignResultLastStep()
		{
			var question1 = GetSurveyQuestion1();
			var summaryCollection = new LearningCentreTestSummaryPerQuestionCollection(question1);

			var summary = new LearningCentreTestSummary(question1);

			foreach (GlbCompanyCampaignItem campaignItem in question1.Campaign.CampaignsItemsSent)
			{
				var answer = new LastCompletedSubmittedAnswer((LearningCentreCampaignItem)campaignItem, question1);
				summary.AddSubmittedAnswerIntoSummary(answer);
			}
			summary.AnalyzeCampaignResultLastStep();
			AssertEquals("Should have 2 records in answers analyze report.", 2, summary.AnswerSummaries.Count);
			AssertEquals("Should have 2 recipients", 2, summary.NumberOfRecipient);
			AssertEquals("Should have 2 replies", 2, summary.NumberOfRecipientReplied);
			AssertEquals("Should have 2 answers", 2, summary.RecipientAnswered);
			AssertEquals("Should have 0 skipped answers", 0, summary.RecipientSkipped);

			var question2 = GetSurveyQuestion2();
			summaryCollection = new LearningCentreTestSummaryPerQuestionCollection(question2);

			summary = new LearningCentreTestSummary(question2);

			foreach (GlbCompanyCampaignItem campaignItem in question2.Campaign.CampaignsItemsSent)
			{
				var answer = new LastCompletedSubmittedAnswer((LearningCentreCampaignItem)campaignItem, question2);
				summary.AddSubmittedAnswerIntoSummary(answer);
			}
			summary.AnalyzeCampaignResultLastStep();
			AssertEquals("Should have 0 records in answers analyze report because it's a Numeric Scale question.", 0, summary.AnswerSummaries.Count);
			AssertEquals("Should have 2 recipients", 2, summary.NumberOfRecipient);
			AssertEquals("Should have 2 replies", 2, summary.NumberOfRecipientReplied);
			AssertEquals("Should have 1 answers", 1, summary.RecipientAnswered);
			AssertEquals("Should have 1 skipped answers", 1, summary.RecipientSkipped);
		}

		public void TestAddSubmittedAnswerIntoSummary()
		{
			var question = GetSurveyQuestion1();
			var summaryCollection = new LearningCentreTestSummaryPerQuestionCollection(question);

			var summary = new LearningCentreTestSummaryForTest(question);

			foreach (GlbCompanyCampaignItem campaignItem in question.Campaign.CampaignsItemsSent)
			{
				var answer = new LastCompletedSubmittedAnswer((LearningCentreCampaignItem)campaignItem, question);
				summary.AddSubmittedAnswerIntoSummary(answer);
			}
			AssertEquals("AnsweredNumber should be 2.", 2, summary.AnsweredNumber);
			AssertEquals("RepliedAnswers should be 2.", 2, summary.RepliedAnswers);

			var question2 = GetSurveyQuestion2();
			summaryCollection = new LearningCentreTestSummaryPerQuestionCollection(question2);

			summary = new LearningCentreTestSummaryForTest(question2);

			foreach (GlbCompanyCampaignItem campaignItem in question2.Campaign.CampaignsItemsSent)
			{
				var answer = new LastCompletedSubmittedAnswer((LearningCentreCampaignItem)campaignItem, question2);
				summary.AddSubmittedAnswerIntoSummary(answer);
			}
			AssertEquals("AnsweredNumber should be 1.", 1, summary.AnsweredNumber);
			AssertEquals("RepliedAnswers should be 2.", 2, summary.RepliedAnswers);
		}
	}
}
