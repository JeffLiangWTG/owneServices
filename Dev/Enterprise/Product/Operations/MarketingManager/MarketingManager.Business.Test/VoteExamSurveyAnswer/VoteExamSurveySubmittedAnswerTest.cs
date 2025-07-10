using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveySubmittedAnswer))]
	sealed class VoteExamSurveySubmittedAnswerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFactory()
		{
			AssertNotNull(SubmittedAnswer.Factory);
			AssertEquals(SubmittedAnswer.CampaignItem.Factory, SubmittedAnswer.Factory);
		}

		public void TestQuestion()
		{
			AssertEquals("Should be assigned in the constructor", Question, SubmittedAnswer.Question);
		}

		public void TestAnswer_NoCompletedAnswer()
		{
			AssertEquals("Should not throw exception", "", SubmittedAnswer.Answer);
		}

		public void TestAnswer_FreeText()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.FreeText;
			VoteExamSurveyAnswer answer = CampaignItem.PersistedAnswers.LoadOrCreateNew(Question);
			answer.HZ_Answer = "123";
			answer.HZ_AnswerComment = "Answer From Comment";
			AssertEquals("Answer From Comment", SubmittedAnswer.Answer);
		}

		public void TestAnswer_LikertScale()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			VoteExamSurveyAnswer answer = CampaignItem.PersistedAnswers.LoadOrCreateNew(Question);

			answer.AnswerAsInt = 1;
			AssertEquals("Strongly Disagree", SubmittedAnswer.Answer);

			answer.AnswerAsInt = 2;
			AssertEquals("Disagree", SubmittedAnswer.Answer);

			answer.AnswerAsInt = 3;
			AssertEquals("Undecided", SubmittedAnswer.Answer);

			answer.AnswerAsInt = 4;
			AssertEquals("Agree", SubmittedAnswer.Answer);

			answer.AnswerAsInt = 5;
			AssertEquals("Strongly Agree", SubmittedAnswer.Answer);
		}

		public void TestAnswer_TrueFalse()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			VoteExamSurveyAnswer answer = CampaignItem.PersistedAnswers.LoadOrCreateNew(Question);

			answer.AnswerAsInt = 1;
			AssertEquals("True", SubmittedAnswer.Answer);

			answer.AnswerAsInt = 2;
			AssertEquals("False", SubmittedAnswer.Answer);
		}

		public void TestAnswer_YesNo()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			VoteExamSurveyAnswer answer = CampaignItem.PersistedAnswers.LoadOrCreateNew(Question);

			answer.AnswerAsInt = 1;
			AssertEquals("Yes", SubmittedAnswer.Answer);

			answer.AnswerAsInt = 2;
			AssertEquals("No", SubmittedAnswer.Answer);
		}

		public void TestAnswer_MultipleChoice()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion option1 = Question.SubQuestions.AddNew();
			option1.HY_Question = "Option 1";
			VoteExamSurveyQuestion option2 = Question.SubQuestions.AddNew();
			option2.HY_Question = "Option 2";
			VoteExamSurveyQuestion option3 = Question.SubQuestions.AddNew();
			option3.HY_Question = "Option 3";
			VoteExamSurveyQuestion option4 = Question.SubQuestions.AddNew();
			option4.HY_Question = "Option 4";

			VoteExamSurveyAnswer answerOption1 = CampaignItem.PersistedAnswers.LoadOrCreateNew(option1);
			answerOption1.AnswerAsBool = true;
			AssertEquals("1) Option 1\r\n", SubmittedAnswer.Answer);
			List<VoteExamSurveyQuestion> selectedOptions = new List<VoteExamSurveyQuestion>(SubmittedAnswer.SelectedMultipleChoiceOptions);
			AssertEquals(1, selectedOptions.Count);
			AssertCollectionContains(option1, selectedOptions);

			VoteExamSurveyAnswer answerOption2 = CampaignItem.PersistedAnswers.LoadOrCreateNew(option2);
			answerOption2.AnswerAsBool = true;
			AssertEquals("1) Option 1\r\n2) Option 2\r\n", SubmittedAnswer.Answer);
			selectedOptions = new List<VoteExamSurveyQuestion>(SubmittedAnswer.SelectedMultipleChoiceOptions);
			AssertEquals(2, selectedOptions.Count);
			AssertCollectionContains(option1, selectedOptions);
			AssertCollectionContains(option2, selectedOptions);

			answerOption2.AnswerAsBool = false;
			VoteExamSurveyAnswer answerOption3 = CampaignItem.PersistedAnswers.LoadOrCreateNew(option3);
			answerOption3.AnswerAsBool = true;
			VoteExamSurveyAnswer answerOption4 = CampaignItem.PersistedAnswers.LoadOrCreateNew(option4);
			answerOption4.AnswerAsBool = true;
			AssertEquals("1) Option 1\r\n3) Option 3\r\n4) Option 4\r\n", SubmittedAnswer.Answer);
			selectedOptions = new List<VoteExamSurveyQuestion>(SubmittedAnswer.SelectedMultipleChoiceOptions);
			AssertEquals(3, selectedOptions.Count);
			AssertCollectionContains(option1, selectedOptions);
			AssertCollectionContains(option3, selectedOptions);
			AssertCollectionContains(option4, selectedOptions);
		}

		public void TestAnswer()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			VoteExamSurveyAnswer answer = CampaignItem.PersistedAnswers.LoadOrCreateNew(Question);
			answer.AnswerAsInt = 40;
			AssertEquals("40", SubmittedAnswer.Answer);
			AssertEquals(40, SubmittedAnswer.AnswerAsInt);
		}

		public void TestAnswerFieldType_SurveyOrExam()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			AssertEquals(nameof(FieldType.Text), SubmittedAnswer.AnswerFieldType);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.FreeText;
			AssertEquals(nameof(FieldType.TextMultiLine), SubmittedAnswer.AnswerFieldType);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			AssertEquals(nameof(FieldType.TextMultiLine), SubmittedAnswer.AnswerFieldType);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			AssertEquals(nameof(FieldType.Integer), SubmittedAnswer.AnswerFieldType);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Percentage;
			AssertEquals(nameof(FieldType.Integer), SubmittedAnswer.AnswerFieldType);
		}

		public void TestAnswerFieldType_Vote()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Campaign.VoteHeader.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.RankedVote;

			VoteExamSurveyQuestion votingItem = Campaign.VoteHeader.SubQuestions.AddNew();
			VoteExamSurveySubmittedAnswer votingAnswer = new VoteExamSurveySubmittedAnswer(CampaignItem, votingItem);
			AssertEquals(nameof(FieldType.Integer), votingAnswer.AnswerFieldType);
		}

		public void TestIsPopulated_MultipleChoice()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion option1 = Question.SubQuestions.AddNew();
			option1.HY_Question = "Option 1";
			VoteExamSurveyQuestion option2 = Question.SubQuestions.AddNew();
			option2.HY_Question = "Option 2";
			VoteExamSurveyQuestion option3 = Question.SubQuestions.AddNew();
			option3.HY_Question = "Option 3";
			VoteExamSurveyQuestion option4 = Question.SubQuestions.AddNew();
			option4.HY_Question = "Option 4";
			Assert(!SubmittedAnswer.IsPopulated);

			VoteExamSurveyAnswer answer1 = CampaignItem.PersistedAnswers.LoadOrCreateNew(option1);
			answer1.AnswerAsBool = true;
			Assert(SubmittedAnswer.IsPopulated);
		}

		public void TestIsPopulated_NonMultipleChoice()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			Question.HY_ExamCorrectAnswer = "1";
			Assert(!SubmittedAnswer.IsPopulated);

			VoteExamSurveyAnswer answer = CampaignItem.PersistedAnswers.LoadOrCreateNew(Question);
			answer.HZ_Answer = "1";
			Assert(SubmittedAnswer.IsPopulated);
		}

		public void TestPersistedAnswer()
		{
			AssertEquals("", SubmittedAnswer.PersistedAnswer);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			VoteExamSurveyAnswer answer = CampaignItem.PersistedAnswers.LoadOrCreateNew(Question);
			answer.HZ_Answer = "meh";
			answer.HZ_AnswerComment = "long comment";
			AssertEquals("meh", SubmittedAnswer.PersistedAnswer);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.FreeText;
			AssertEquals("long comment", SubmittedAnswer.PersistedAnswer);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new VoteExamSurveySubmittedAnswer(CampaignItem, Question);
		}

		VoteExamSurveySubmittedAnswer SubmittedAnswer
		{
			get { return (VoteExamSurveySubmittedAnswer)CachedBusinessObject; }
		}

		VoteExamSurveyQuestion Question
		{
			get
			{
				if (fQuestion == null)
				{
					fQuestion = Campaign.Questions.AddNew();
				}
				return fQuestion;
			}
		}

		GlbCompanyCampaignItem CampaignItem
		{
			get
			{
				if (fCampaignItem == null)
				{
					fCampaignItem = Campaign.CampaignsItemsSent.AddNew();
				}
				return fCampaignItem;
			}
		}

		GlbCompanyCampaign Campaign
		{
			get
			{
				if (fCampaign == null)
				{
					fCampaign = Factory.New<GlbCompanyCampaign>();
				}
				return fCampaign;
			}
		}

		VoteExamSurveyQuestion fQuestion;
		GlbCompanyCampaignItem fCampaignItem;
		GlbCompanyCampaign fCampaign;

		#endregion
	}
}
