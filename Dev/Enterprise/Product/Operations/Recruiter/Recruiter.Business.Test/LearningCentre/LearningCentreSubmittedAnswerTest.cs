using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreSubmittedAnswer))]
	sealed class LearningCentreSubmittedAnswerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestScore()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			LearningCentreCampaignItem campaignItem = Factory.New<LearningCentreCampaignItem>();
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			VoteExamSurveyAnswer answer = examAnswerSet.PersistedAnswers.LoadOrCreateNew(question, campaignItem);
			answer.HZ_Answer = "1";
			AssertEquals(ZByte.Zero, new LearningCentreSubmittedAnswer(campaignItem, question).Score);

			answer.HZ_Answer = "4";
			AssertEquals((ZByte)3, new LearningCentreSubmittedAnswer(campaignItem, question).Score);

			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			answer.HZ_Answer = "1";
			AssertEquals((ZByte)1, new LearningCentreSubmittedAnswer(campaignItem, question).Score);
			answer.HZ_Answer = "2";
			AssertEquals(ZByte.Zero, new LearningCentreSubmittedAnswer(campaignItem, question).Score);

			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			answer.HZ_Answer = "1";
			AssertEquals((ZByte)1, new LearningCentreSubmittedAnswer(campaignItem, question).Score);
			answer.HZ_Answer = "2";
			AssertEquals(ZByte.Zero, new LearningCentreSubmittedAnswer(campaignItem, question).Score);

			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			LearningCentreQuestion option1 = question.SubQuestions.AddNew();
			option1.HY_AnswerWeighting = 2;
			LearningCentreQuestion option2 = question.SubQuestions.AddNew();
			option2.HY_AnswerWeighting = 5;
			LearningCentreQuestion option3 = question.SubQuestions.AddNew();
			option3.HY_AnswerWeighting = 7;
			VoteExamSurveyAnswer option1Answer = examAnswerSet.PersistedAnswers.LoadOrCreateNew(option1, campaignItem);
			option1Answer.AnswerAsBool = true;
			VoteExamSurveyAnswer option2Answer = examAnswerSet.PersistedAnswers.LoadOrCreateNew(option2, campaignItem);
			VoteExamSurveyAnswer option3Answer = examAnswerSet.PersistedAnswers.LoadOrCreateNew(option3, campaignItem);
			option3Answer.AnswerAsBool = true;
			AssertEquals((ZByte)9, new LearningCentreSubmittedAnswer(campaignItem, question).Score);
		}

		public void TestScore_Header()
		{
			LearningCentreQuestion question = Factory.New<LearningCentreQuestion>();
			LearningCentreCampaignItem item = Factory.New<LearningCentreCampaignItem>();
			LearningCentreSubmittedAnswer answer = new LearningCentreSubmittedAnswer(item, question);
			AssertEquals(ZByte.Zero, answer.Score);
		}

		public void TestIsAnsweredCorrectly()
		{
			TestCorrectIncorrectAnswer("IsAnsweredCorrectly");
		}

		public void TestIsAnsweredCorrectly_MultipleChoiceQuestion()
		{
			TestCorrectIncorrectAnswer_MultipleChoiceQuestion("IsAnsweredCorrectly");
		}

		public void TestIsAnsweredIncorrectly()
		{
			TestCorrectIncorrectAnswer("IsAnsweredIncorrectly");
		}

		public void TestIsAnsweredIncorrectly_MultipleChoiceQuestion()
		{
			TestCorrectIncorrectAnswer_MultipleChoiceQuestion("IsAnsweredIncorrectly");
		}

		public void TestSavedRandomizedQuestionNumber()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();

			LearningCentreQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			LearningCentreQuestion option11 = question1.SubQuestions.AddNew();
			option11.HY_Question = "Question 1 Option 1";
			option11.CorrectAnswerAsBool = true;
			LearningCentreQuestion option12 = question1.SubQuestions.AddNew();
			option12.HY_Question = "Question 1 Option 2";
			LearningCentreQuestion option13 = question1.SubQuestions.AddNew();
			option13.HY_Question = "Question 1 Option 3";
			LearningCentreQuestion option14 = question1.SubQuestions.AddNew();
			option14.HY_Question = "Question 1 Option 4";
			option14.CorrectAnswerAsBool = true;

			LearningCentreQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			LearningCentreQuestion option21 = question2.SubQuestions.AddNew();
			option21.HY_Question = "Question 2 Option 1";
			option21.CorrectAnswerAsBool = true;
			LearningCentreQuestion option22 = question2.SubQuestions.AddNew();
			option22.HY_Question = "Question 2 Option 2";
			LearningCentreQuestion option23 = question2.SubQuestions.AddNew();
			option23.HY_Question = "Question 2 Option 3";
			LearningCentreQuestion option24 = question2.SubQuestions.AddNew();
			option24.HY_Question = "Question 2 Option 4";
			option24.CorrectAnswerAsBool = true;

			LearningCentreQuestion question3 = campaign.Questions.AddNew();
			question3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			LearningCentreQuestion option31 = question3.SubQuestions.AddNew();
			option31.HY_Question = "Question 3 Option 1";
			option31.CorrectAnswerAsBool = true;
			LearningCentreQuestion option32 = question3.SubQuestions.AddNew();
			option32.HY_Question = "Question 3 Option 2";
			LearningCentreQuestion option33 = question3.SubQuestions.AddNew();
			option33.HY_Question = "Question 2 Option 3";
			LearningCentreQuestion option34 = question3.SubQuestions.AddNew();
			option34.HY_Question = "Question 3 Option 4";
			option34.CorrectAnswerAsBool = true;

			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)examAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer1 = examAnswerSet.PersistedAnswers.LoadOrCreateNew(option14, campaignItem);
			answer1.AnswerAsBool = true;
			VoteExamSurveyAnswer answer2 = examAnswerSet.PersistedAnswers.LoadOrCreateNew(option24, campaignItem);
			answer2.AnswerAsBool = true;
			VoteExamSurveyAnswer answer3 = examAnswerSet.PersistedAnswers.LoadOrCreateNew(option32, campaignItem);
			answer3.AnswerAsBool = true;
			((IVoteExamSurveyAnswerSet)examAnswerSet).SubmitAnswerSet();

			var examAnswerSetNew = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);

			AssertEquals("There should be 3 submitted answers.", examAnswerSetNew.SubmittedAnswers.Count, 3);
			AssertEquals("First question number should be.", examAnswerSetNew.SubmittedAnswers[0].QuestionNumber, 1);
			Assert(examAnswerSetNew.SubmittedAnswers[0].Answer.Contains("Question 1 Option 4"));
			AssertEquals("First question number should be.", examAnswerSetNew.SubmittedAnswers[1].QuestionNumber, 2);
			Assert(examAnswerSetNew.SubmittedAnswers[1].Answer.Contains("Question 2 Option 4"));
			AssertEquals("First question number should be.", examAnswerSetNew.SubmittedAnswers[2].QuestionNumber, 3);
			Assert(examAnswerSetNew.SubmittedAnswers[2].Answer.Contains("Question 3 Option 2"));
		}

		void TestCorrectIncorrectAnswer(string propertyNameToTest)
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question = campaign.Questions.AddNew();
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			LearningCentreSubmittedAnswer submittedAnswer = new LearningCentreSubmittedAnswer(campaignItem, question);
			ZPropertyInfo info = submittedAnswer.ZPropertyInfoHash[propertyNameToTest];
			bool expectedIsAnsweredCorrectly = propertyNameToTest == "IsAnsweredCorrectly";

			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			question.HY_ExamCorrectAnswer = "1";
			AssertEquals("Answer isn't provided, should be false", false, info.Value);

			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			VoteExamSurveyAnswer answer = examAnswerSet.PersistedAnswers.LoadOrCreateNew(question, campaignItem);
			answer.HZ_Answer = "1";
			AssertEquals(expectedIsAnsweredCorrectly, info.Value);

			answer.HZ_Answer = "2";
			AssertEquals(!expectedIsAnsweredCorrectly, info.Value);
		}

		void TestCorrectIncorrectAnswer_MultipleChoiceQuestion(string propertyNameToTest)
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question = campaign.Questions.AddNew();
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			LearningCentreSubmittedAnswer submittedAnswer = new LearningCentreSubmittedAnswer(campaignItem, question);
			ZPropertyInfo info = submittedAnswer.ZPropertyInfoHash[propertyNameToTest];
			bool expectedIsAnsweredCorrectly = propertyNameToTest == "IsAnsweredCorrectly";

			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			LearningCentreQuestion option1 = question.SubQuestions.AddNew();
			option1.HY_Question = "Option 1";
			option1.CorrectAnswerAsBool = true;
			LearningCentreQuestion option2 = question.SubQuestions.AddNew();
			option2.HY_Question = "Option 2";
			LearningCentreQuestion option3 = question.SubQuestions.AddNew();
			option3.HY_Question = "Option 3";
			LearningCentreQuestion option4 = question.SubQuestions.AddNew();
			option4.HY_Question = "Option 4";
			option4.CorrectAnswerAsBool = true;

			AssertEquals("Answer isn't provided, should be false", false, info.Value);

			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			VoteExamSurveyAnswer answer1 = examAnswerSet.PersistedAnswers.LoadOrCreateNew(option1, campaignItem);
			answer1.AnswerAsBool = true;
			AssertEquals(expectedIsAnsweredCorrectly, info.Value);

			VoteExamSurveyAnswer answer2 = examAnswerSet.PersistedAnswers.LoadOrCreateNew(option2, campaignItem);
			answer2.AnswerAsBool = true;
			AssertEquals(!expectedIsAnsweredCorrectly, info.Value);

			VoteExamSurveyAnswer answer3 = examAnswerSet.PersistedAnswers.LoadOrCreateNew(option3, campaignItem);
			answer3.AnswerAsBool = true;
			AssertEquals(!expectedIsAnsweredCorrectly, info.Value);

			VoteExamSurveyAnswer answer4 = examAnswerSet.PersistedAnswers.LoadOrCreateNew(option4, campaignItem);
			answer4.AnswerAsBool = true;
			AssertEquals(!expectedIsAnsweredCorrectly, info.Value);

			answer2.AnswerAsBool = false;
			answer3.AnswerAsBool = false;
			AssertEquals(expectedIsAnsweredCorrectly, info.Value);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question = campaign.Questions.AddNew();
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			return new LearningCentreSubmittedAnswer(campaignItem, question);
		}
	}
}
