using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LastCompletedSubmittedAnswer))]
	sealed class LastCompletedSubmittedAnswerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.New<LearningCentreCampaign>();
			var question = campaign.Questions.AddNew();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			return new LastCompletedSubmittedAnswer(campaignItem, question);
		}

		public void TestNoSubmittedExamAnswersArchive()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignName = "Exam AAA";
			var question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = campaign.PK;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = applicant.PK;
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			var answer1 = examAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem);
			answer1.HZ_Answer = "2";

			Factory.Save();

			var lastAnswers = new LastCompletedSubmittedAnswerTestCase(campaignItem, question1);
			var submittedArchivedAnswer = lastAnswers.GetPersistedAnswerFromQuestionForTest();
			Assert("Submitted archived answer should not exist.", submittedArchivedAnswer == null);

			var examAttempt1 = Factory.New<ExamAttempt>();
			examAttempt1.ExamBegin(campaignItem);
			examAttempt1.ExamEnd(campaignItem);

			examAttempt1.EXA_TestCommencedUtc = new ZDateTime(2016, 2, 22, 15, 0, 0);
			examAttempt1.EXA_TestCompletedUtc = ZDateTime.Empty;

			Factory.Save();

			lastAnswers = new LastCompletedSubmittedAnswerTestCase(campaignItem, question1);
			submittedArchivedAnswer = lastAnswers.GetPersistedAnswerFromQuestionForTest();
			Assert("Submitted archived answer should exist.", submittedArchivedAnswer != null);

			var examAttempt2 = Factory.New<ExamAttempt>();
			examAttempt2.EXA_G8 = campaignItem.PK;
			examAttempt2.EXA_Score = campaignItem.ExamScore;
			var examAnswers = new ExamAnswerArchiveCollection(Factory);
			var examAnswerArchive = examAnswers.AddNew();
			examAnswerArchive.QuestionPK = question1.PK;
			examAnswerArchive.QuestionAsString = question1.HY_QuestionMultilingual;
			examAnswerArchive.QuestionActualNumber = question1.ActualOrder;
			examAnswerArchive.AnswerAsString = "3";
			examAttempt2.EXA_AnswersXML = examAnswers.ConvertToXmlBlob();
			examAttempt2.EXA_TestCommencedUtc = new ZDateTime(2016, 2, 22, 15, 0, 0);
			examAttempt2.EXA_TestCompletedUtc = new ZDateTime(2016, 2, 22, 15, 0, 0);
			Factory.Save();

			lastAnswers = new LastCompletedSubmittedAnswerTestCase(campaignItem, question1);
			submittedArchivedAnswer = lastAnswers.GetPersistedAnswerFromQuestionForTest();
			Assert("Latest submitted archived answer should be the examAttempt2 which the answer is 3.", submittedArchivedAnswer.Answer == "3");
		}

		public void TestIsEmptyAnswer()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignName = "Exam AAA";
			var question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion subQuestion1a = question1.SubQuestions.AddNew();
			subQuestion1a.HY_Question = "1a";
			subQuestion1a.ActualOrder = 1;
			VoteExamSurveyQuestion subQuestion1b = question1.SubQuestions.AddNew();
			subQuestion1b.HY_Question = "1b";
			subQuestion1b.ActualOrder = 2;
			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = campaign.PK;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = applicant.PK;
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;

			Factory.Save();

			var lastAnswers1 = new LastCompletedSubmittedAnswerTestCase(campaignItem, question1);
			Assert("lastAnswers is empty.", lastAnswers1.IsEmptyAnswer());

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			var votedItems = voteExamSurveyAnswerSet.PersistedAnswers.GetCompletedAnswers(campaignItem);
			VoteExamSurveyAnswer answer11 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(subQuestion1a, campaignItem);
			answer11.AnswerAsBool = true;
			Factory.Save();

			var lastAnswers2 = new LastCompletedSubmittedAnswerTestCase(campaignItem, question1);

			Assert("lastAnswers is not empty.", !lastAnswers2.IsEmptyAnswer());

			var question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			VoteExamSurveyAnswer answer22 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem);
			answer22.HZ_Answer = "";
			Factory.Save();

			var lastAnswers3 = new LastCompletedSubmittedAnswerTestCase(campaignItem, question2);
			Assert("lastAnswers is empty.", lastAnswers3.IsEmptyAnswer());

			answer22.HZ_Answer = "3";
			Factory.Save();

			var lastAnswers4 = new LastCompletedSubmittedAnswerTestCase(campaignItem, question2);
			Assert("lastAnswers is not empty.", !lastAnswers4.IsEmptyAnswer());
		}

		public void TestIsRepliedAnswer()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignName = "Exam AAA";
			var question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion subQuestion1a = question1.SubQuestions.AddNew();
			subQuestion1a.HY_Question = "1a";
			subQuestion1a.ActualOrder = 1;
			VoteExamSurveyQuestion subQuestion1b = question1.SubQuestions.AddNew();
			subQuestion1b.HY_Question = "1b";
			subQuestion1b.ActualOrder = 2;

			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = campaign.PK;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = applicant.PK;
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;

			Factory.Save();

			var lastAnswers1 = new LastCompletedSubmittedAnswerTestCase(campaignItem, question1);
			Assert("lastAnswers1 is not replied.", !lastAnswers1.IsRepliedAnswer());

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			var votedItems = voteExamSurveyAnswerSet.PersistedAnswers.GetCompletedAnswers(campaignItem);
			VoteExamSurveyAnswer answer11 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(subQuestion1a, campaignItem);
			answer11.AnswerAsBool = true;
			Factory.Save();

			var lastAnswers2 = new LastCompletedSubmittedAnswerTestCase(campaignItem, question1);

			Assert("lastAnswers2 is replied.", lastAnswers2.IsRepliedAnswer());

			var question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			Factory.Save();

			var lastAnswers3 = new LastCompletedSubmittedAnswerTestCase(campaignItem, question2);
			Assert("lastAnswers3 is not replied.", !lastAnswers3.IsRepliedAnswer());

			VoteExamSurveyAnswer answer22 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem);
			answer22.HZ_Answer = "";
			Factory.Save();

			var lastAnswers4 = new LastCompletedSubmittedAnswerTestCase(campaignItem, question2);
			Assert("lastAnswers4 is replied.", lastAnswers4.IsRepliedAnswer());
		}

		#region implementation
		class LastCompletedSubmittedAnswerTestCase : LastCompletedSubmittedAnswer
		{
			public LastCompletedSubmittedAnswerTestCase(LearningCentreCampaignItem campaignItem, LearningCentreQuestion question)
			: base(campaignItem, question)
			{
			}
			public ExamAnswerArchive GetPersistedAnswerFromQuestionForTest()
			{
				return GetPersistedAnswerFromQuestion();
			}
		}
		#endregion

	}
}
