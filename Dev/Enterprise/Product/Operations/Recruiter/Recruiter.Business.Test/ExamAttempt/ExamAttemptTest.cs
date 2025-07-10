using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ExamAttempt))]
	sealed class ExamAttemptTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPopulate()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignName = "Exam AAA";
			var question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			var question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			var option2a = question2.SubQuestions.AddNew();
			var option2b = question2.SubQuestions.AddNew();
			var question3 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;

			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = campaign.PK;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = applicant.PK;
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			var answer1 = examAnswerSet.PersistedAnswers.CreateNew(question1, campaignItem);
			answer1.HZ_Answer = "2";
			var answer2 = examAnswerSet.PersistedAnswers.CreateNew(option2b, campaignItem);
			answer2.AnswerAsBool = true;

			Factory.Save();

			var examAttempt = Factory.New<ExamAttempt>();
			AssertEquals(ExamAttempt.StatusCodes.InProgress, examAttempt.EXA_Status);
			examAttempt.ExamBegin(campaignItem);
			AssertEquals(ExamAttempt.StatusCodes.InProgress, examAttempt.EXA_Status);
			examAttempt.ExamEnd(campaignItem);
			AssertEquals(ExamAttempt.StatusCodes.Queued, examAttempt.EXA_Status);

			AssertEquals(campaignItem.PK, examAttempt.EXA_G8);
			AssertEquals(2, examAttempt.ExamAnswers.Count);
			AssertEquals(question1.PK, examAttempt.ExamAnswers[0].QuestionPK);
			AssertEquals(question2.PK, examAttempt.ExamAnswers[1].QuestionPK);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedExamAttempt = anotherFactory.Load<ExamAttempt>(examAttempt.PK);

			AssertEquals(campaignItem.PK, loadedExamAttempt.EXA_G8);
			AssertEquals(2, loadedExamAttempt.ExamAnswers.Count);
			AssertEquals(question1.PK, loadedExamAttempt.ExamAnswers[0].QuestionPK);
			AssertEquals(question2.PK, loadedExamAttempt.ExamAnswers[1].QuestionPK);
		}

		public void TestRollback()
		{
			var examAttempt = Factory.New<ExamAttempt>();
			AssertEquals(ExamAttempt.StatusCodes.InProgress, examAttempt.EXA_Status);
			examAttempt.EXA_Score = 100;
			examAttempt.EXA_TestCompletedUtc = ZDateTime.Now;
			examAttempt.ExamAnswers.AddNew();
			examAttempt.EXA_AnswersXML = new byte[10];
			examAttempt.EXA_Status = ExamAttempt.StatusCodes.Queued;

			AssertEquals((byte)100, examAttempt.EXA_Score);
			AssertEquals(false, examAttempt.EXA_TestCompletedUtc.IsEmpty);
			AssertEquals(true, examAttempt.ExamAnswers.Any());
			AssertEquals(false, examAttempt.EXA_AnswersXML.IsEmpty);
			AssertEquals(ExamAttempt.StatusCodes.Queued, examAttempt.EXA_Status);

			examAttempt.Rollback();
			AssertEquals((byte)0, examAttempt.EXA_Score);
			AssertEquals(true, examAttempt.EXA_TestCompletedUtc.IsEmpty);
			AssertEquals(false, examAttempt.ExamAnswers.Any());
			AssertEquals(true, examAttempt.EXA_AnswersXML.IsEmpty);
			AssertEquals(ExamAttempt.StatusCodes.InProgress, examAttempt.EXA_Status);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return NewBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return NewBusinessObject(factory);
		}

		BusinessObject NewBusinessObject(BusinessObjectFactory factory)
		{
			var campaign = factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignName = "Exam AAA";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			var examAttempt = factory.New<ExamAttempt>();
			examAttempt.EXA_G8 = campaignItem.PK;
			return examAttempt;
		}
	}
}
