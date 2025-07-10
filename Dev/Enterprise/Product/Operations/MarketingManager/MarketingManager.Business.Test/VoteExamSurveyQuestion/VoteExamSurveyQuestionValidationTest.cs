using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class VoteExamSurveyQuestionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateHY_AnswerType()
		{
			Question.Validation.ValidateHY_AnswerType();
			AssertMandatoryValidationError(Question.HY_AnswerTypeInfo, true);

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.Questions.Add(Question);
			Question.HY_AnswerType = "MEH";
			AssertMandatoryValidationError(Question.HY_AnswerTypeInfo, false);
			AssertListValidationInvalidCodeError(Question.HY_AnswerTypeInfo, true);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			AssertListValidationInvalidCodeError(Question.HY_AnswerTypeInfo, false);
		}

		public void TestValidateHY_AnswerType_VotingItem()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			VoteExamSurveyQuestion votingItem = campaign.VoteHeader.SubQuestions.AddNew();
			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			AssertListValidationInvalidCodeError(votingItem.HY_AnswerTypeInfo, true);

			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			AssertListValidationInvalidCodeError(votingItem.HY_AnswerTypeInfo, false);

			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			AssertListValidationInvalidCodeError(votingItem.HY_AnswerTypeInfo, false);
		}

		public void TestValidateHY_Question()
		{
			Question.Validation.ValidateHY_Question();
			AssertMandatoryValidationError(Question.HY_QuestionInfo, true);

			Question.HY_Question = "MEHMEH";
			AssertNoErrors(Question.HY_QuestionInfo);
		}

		public void TestValidationHY_QuestionMultiLingualSupport()
		{
			Question.Validation.ValidateHY_Question();
			AssertMandatoryValidationError(Question.HY_QuestionInfo, true);

			Question.HY_Question = "得分情况怎么样？";
			AssertNoErrors(Question.HY_QuestionInfo);
		}

		public void TestValidateHY_QuestionOrder()
		{
			Question.HY_QuestionOrder = 0;
			AssertMandatoryValidationError(Question.HY_QuestionOrderInfo, true);

			Question.HY_QuestionOrder = 2;
			AssertNoErrors(Question.HY_QuestionOrderInfo);
		}

		public void TestValidateHY_QuestionOrderInCollection()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var question1 = campaign.Questions.AddNew();
			question1.HY_Question = "q1";
			var sub1 = question1.SubQuestions.AddNew();
			sub1.HY_Question = "?";
			sub1.HY_ExamCorrectAnswer = "Y";
			var sub2 = question1.SubQuestions.AddNew();
			sub2.HY_Question = "?";

			var question2 = campaign.Questions.AddNew();
			question2.HY_Question = "q1";
			var sub3 = question2.SubQuestions.AddNew();
			sub3.HY_Question = "?";
			sub3.HY_ExamCorrectAnswer = "Y";
			var sub4 = question2.SubQuestions.AddNew();
			sub4.HY_Question = "?";

			var question3 = campaign.Questions.AddNew();
			question3.HY_Question = "q1";
			var sub5 = question3.SubQuestions.AddNew();
			sub5.HY_Question = "?";
			sub5.HY_ExamCorrectAnswer = "Y";
			var sub6 = question3.SubQuestions.AddNew();
			sub6.HY_Question = "?";

			Factory.Save();
			Db.Connection.ExecuteNonQuery($@"update dbo.VoteExamSurveyQuestion
set HY_QuestionOrder = 1,
HY_SystemLastEditTimeUtc = GETUTCDATE(),
HY_SystemLastEditUser = 'E'
where HY_PK = '{question3.PK}'");

			var factory = new BusinessObjectFactory();
			var reloadQuestion1 = factory.Load<VoteExamSurveyQuestion>(question1.PK);
			var reloadQuestion2 = factory.Load<VoteExamSurveyQuestion>(question2.PK);
			var reloadQuestion3 = factory.Load<VoteExamSurveyQuestion>(question3.PK);

			reloadQuestion1.Validation.ValidateHY_QuestionOrder();
			reloadQuestion2.Validation.ValidateHY_QuestionOrder();
			reloadQuestion3.Validation.ValidateHY_QuestionOrder();

			AssertHasErrors(reloadQuestion1.HY_QuestionOrderInfo);
			AssertNoErrors(reloadQuestion1.SubQuestions[0].HY_QuestionOrderInfo);
			AssertNoErrors(reloadQuestion1.SubQuestions[1].HY_QuestionOrderInfo);

			AssertNoErrors(reloadQuestion2.HY_QuestionOrderInfo);
			AssertNoErrors(reloadQuestion2.SubQuestions[0].HY_QuestionOrderInfo);
			AssertNoErrors(reloadQuestion2.SubQuestions[1].HY_QuestionOrderInfo);

			AssertHasErrors(reloadQuestion3.HY_QuestionOrderInfo);
			AssertNoErrors(reloadQuestion3.SubQuestions[0].HY_QuestionOrderInfo);
			AssertNoErrors(reloadQuestion3.SubQuestions[1].HY_QuestionOrderInfo);
		}

		public void TestValidateHY_SubQuestionOrder()
		{
			Question.Validation.ValidateHY_SubQuestionOrder();
			AssertMandatoryValidationError(Question.HY_SubQuestionOrderInfo, false);

			VoteExamSurveyQuestion subQuestion = Question.SubQuestions.AddNew();
			subQuestion.ActualOrder = 0;
			AssertEquals("Should be automatically assigned to 1", new ZByte(1), subQuestion.HY_SubQuestionOrder);
		}

		public void TestValidateHY_Min_VotingHeader()
		{
			Question.HY_Min = 0;
			Question.HY_Max = 0;
			Question.Validation.ValidateHY_Min();
			AssertNoErrors("Not a Voting Header question", Question.HY_MinInfo);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.RankedVote;
			Question.Validation.ValidateHY_Min();
			AssertEquals(1, Question.HY_MinInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MinInfo, "within the range 1 to 10");

			Question.HY_Max = 10;
			Question.HY_Min = 1;
			AssertNoErrors(Question.HY_MinInfo);

			Question.HY_Min = 8;
			AssertNoErrors(Question.HY_MinInfo);

			Question.HY_Max = 7;
			Question.Validation.ValidateHY_Min();
			AssertEquals(1, Question.HY_MinInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MinInfo, "must be less than or equal to");

			Question.HY_Min = 7;
			AssertNoErrors(Question.HY_MinInfo);
		}

		public void TestValidateHY_Min_MultipleChoiceQuestion()
		{
			Question.HY_Min = 0;
			Question.HY_Max = 0;
			Question.Validation.ValidateHY_Min();
			AssertNoErrors("Not a Multiple Choice question", Question.HY_MinInfo);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			Question.HY_Max = 0;
			Question.HY_Min = 0;
			AssertEquals(1, Question.HY_MinInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MinInfo, "within the range 1 to 255");

			Question.HY_Max = 10;
			Question.HY_Min = 1;
			AssertNoErrors(Question.HY_MinInfo);

			Question.HY_Min = 8;
			AssertNoErrors(Question.HY_MinInfo);

			Question.HY_Max = 7;
			Question.Validation.ValidateHY_Min();
			AssertEquals(1, Question.HY_MinInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MinInfo, "must be less than or equal to");

			Question.SubQuestions.AddNew();
			Question.SubQuestions.AddNew();
			Question.HY_Min = 2;
			AssertNoErrors(Question.HY_MinInfo);

			Question.HY_Min = 3;
			AssertEquals(1, Question.HY_MinInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MinInfo, "within the range 1 to 2");
		}

		public void TestValidateHY_Min_NumericScaleQuestion()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			Question.HY_Min = 1;
			AssertNoErrors(Question.HY_MinInfo);

			Question.HY_Max = 4;
			Question.HY_Min = 5;
			AssertEquals(1, Question.HY_MinInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MinInfo, "must be less than");

			Question.HY_Min = 2;
			AssertNoErrors(Question.HY_MinInfo);
		}

		public void TestValidateHY_Max_VotingHeader()
		{
			Question.HY_Min = 0;
			Question.HY_Max = 11;
			Question.Validation.ValidateHY_Max();
			AssertNoErrors("Not a Voting Header question", Question.HY_MaxInfo);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.UnrankedVote;
			Question.Validation.ValidateHY_Max();
			AssertEquals(1, Question.HY_MaxInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MaxInfo, "within the range 1 to 10");

			Question.HY_Max = 10;
			AssertNoErrors(Question.HY_MaxInfo);

			Question.HY_Max = 8;
			AssertNoErrors(Question.HY_MaxInfo);

			Question.HY_Min = 9;
			Question.Validation.ValidateHY_Max();
			AssertEquals(1, Question.HY_MaxInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MaxInfo, "must be greater than or equal to");

			Question.HY_Max = 9;
			AssertNoErrors(Question.HY_MaxInfo);
		}

		public void TestValidateHY_Max_MultipleChoiceQuestion()
		{
			Question.HY_Min = 0;
			Question.HY_Max = 0;
			Question.Validation.ValidateHY_Max();
			AssertNoErrors("Not a Multiple Choice question", Question.HY_MaxInfo);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			Question.HY_Min = 0;
			Question.HY_Max = 0;
			AssertEquals(1, Question.HY_MaxInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MaxInfo, "within the range 1 to 255");

			Question.HY_Max = 1;
			AssertNoErrors(Question.HY_MaxInfo);

			Question.HY_Max = 4;
			AssertNoErrors(Question.HY_MaxInfo);

			Question.HY_Min = 7;
			Question.Validation.ValidateHY_Max();
			AssertEquals(1, Question.HY_MaxInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MaxInfo, "must be greater than or equal to");

			Question.SubQuestions.AddNew();
			Question.SubQuestions.AddNew();
			Question.HY_Min = 1;
			Question.HY_Max = 2;
			AssertNoErrors(Question.HY_MaxInfo);

			Question.HY_Max = 3;
			AssertEquals(1, Question.HY_MaxInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MaxInfo, "within the range 1 to 2");
		}

		public void TestValidateHY_Max_NumericScaleQuestion()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			Question.HY_Max = 2;
			AssertNoErrors(Question.HY_MaxInfo);

			Question.HY_Min = 5;
			Question.HY_Max = 4;
			AssertEquals(1, Question.HY_MaxInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MaxInfo, "must be greater than");

			Question.HY_Max = 8;
			AssertNoErrors(Question.HY_MaxInfo);

			Question.HY_Min = 0;
			Question.HY_Max = 0;
			AssertEquals(2, Question.HY_MaxInfo.GetErrors().Count());
			AssertHasErrorContaining(Question.HY_MaxInfo, "greater than or equal to 1");
			AssertHasErrorContaining(Question.HY_MaxInfo, "must be greater than");
		}

		public void TestValidateAll_ValidateNumberOfVotingItems()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.RankedVote;
			Question.HY_Min = 10;
			AssertNoRowErrors(Question);

			Question.Validation.ValidateAll();
			AssertHasRowError(Question, "There has to be at least 10 vote item(s). You can either add more vote items or adjust the allowable number of votes.");

			Question.HY_Min = 3;
			Question.Validation.ValidateAll();
			AssertHasRowError(Question, "There has to be at least 3 vote item(s). You can either add more vote items or adjust the allowable number of votes.");

			VoteExamSurveyQuestion header = Question.SubQuestions.AddNew();
			header.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion votingItem1 = Question.SubQuestions.AddNew();
			votingItem1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			VoteExamSurveyQuestion votingItem2 = Question.SubQuestions.AddNew();
			votingItem2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Question.Validation.ValidateAll();
			AssertHasRowError(Question, "There has to be at least 3 vote item(s). You can either add more vote items or adjust the allowable number of votes.");

			VoteExamSurveyQuestion votingItem3 = Question.SubQuestions.AddNew();
			votingItem3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Question.Validation.ValidateAll();
			AssertNoRowErrors(Question);
		}

		public void TestValidateAll_ValidateNumberOfMultipleChoiceOptions()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			Question.Validation.ValidateAll();
			AssertHasRowError(Question, "There has to be at least 2 multiple choice options");

			Question.SubQuestions.AddNew();
			Question.Validation.ValidateAll();
			AssertHasRowError(Question, "There has to be at least 2 multiple choice options");

			Question.SubQuestions.AddNew();
			Question.Validation.ValidateAll();
			AssertNoRowErrors(Question);

			Question.SubQuestions.AddNew();
			Question.Validation.ValidateAll();
			AssertNoRowErrors(Question);
		}

		VoteExamSurveyQuestion Question
		{
			get
			{
				if (fQuestion == null)
				{
					GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
					fQuestion = campaign.Questions.AddNew();
				}
				return fQuestion;
			}
		}

		VoteExamSurveyQuestion fQuestion;
	}
}
