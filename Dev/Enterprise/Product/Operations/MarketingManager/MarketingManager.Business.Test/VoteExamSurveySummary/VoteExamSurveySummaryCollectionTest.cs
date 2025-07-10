using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveySummaryCollection))]
	sealed class VoteExamSurveySummaryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<VoteExamSurveySummaryCollection>
	{
		#region Implementation

		protected override VoteExamSurveySummaryCollection GetCollectionToTest()
		{
			return new VoteExamSurveySummaryCollection(GetSurveyQuestions());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			return new VoteExamSurveySummary(question);
		}

		#endregion

		VoteExamSurveyQuestionSet GetSurveyQuestions()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			question1.HY_Question = "question1";
			question1.HY_G0 = campaign.PK;
			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			question2.HY_Question = "question2";
			question2.HY_G0 = campaign.PK;

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem1);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer11 = Factory.New<VoteExamSurveyAnswer>();
			answer11.HZ_HY = question1.PK;
			answer11.HZ_G8 = campaignItem1.PK;
			answer11.AnswerAsInt = 1;

			VoteExamSurveyQuestionCollection questionCollection = new VoteExamSurveyQuestionCollection(campaign);

			return questionCollection;
		}

		public void TestSurveyQuestions()
		{
			VoteExamSurveyQuestionSet questions = GetSurveyQuestions();
			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(questions);

			AssertEquals(2, summaryCollection.SurveyQuestions.Count);

			AssertEquals("question1", summaryCollection.SurveyQuestions[0].HY_Question);
			AssertEquals("question2", summaryCollection.SurveyQuestions[1].HY_Question);
		}

		public void TestLoad()
		{
			VoteExamSurveyQuestionSet questions = GetSurveyQuestions();
			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(questions);

			AssertEquals(0, summaryCollection.Count);

			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(2, summaryCollection.Count);
			AssertEquals(1, summary1.Number);
		}
	}
}
