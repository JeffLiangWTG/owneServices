using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class LearningCentreQuestionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateHY_ExamCorrectAnswer()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			Question.Validation.ValidateHY_ExamCorrectAnswer();
			AssertMandatoryValidationError(Question.HY_ExamCorrectAnswerInfo, true);

			Question.HY_ExamCorrectAnswer = "TRU";
			AssertMandatoryValidationError(Question.HY_ExamCorrectAnswerInfo, false);
			AssertListValidationInvalidCodeError(Question.HY_ExamCorrectAnswerInfo, true);

			Question.HY_ExamCorrectAnswer = "1";
			AssertNoErrors(Question.HY_ExamCorrectAnswerInfo);

			Question.HY_ExamCorrectAnswer = "2";
			AssertNoErrors(Question.HY_ExamCorrectAnswerInfo);
		}

		public void TestValidateAll_ValidateNumberOfCorrectMultipleChoiceExamOptions()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			Question.HY_Max = 2;

			Question.Validation.ValidateAll();
			AssertHasRowError(Question, "You need to specify 2 correct answer(s)");

			LearningCentreQuestion option1 = Question.SubQuestions.AddNew();
			LearningCentreQuestion option2 = Question.SubQuestions.AddNew();
			LearningCentreQuestion option3 = Question.SubQuestions.AddNew();
			option1.CorrectAnswerAsBool = true;
			Question.Validation.ValidateAll();
			AssertHasRowError(Question, "You need to specify 2 correct answer(s)");

			option2.CorrectAnswerAsBool = true;
			Question.Validation.ValidateAll();
			AssertNoRowErrors(Question);

			option2.CorrectAnswerAsBool = false;
			Question.Validation.ValidateAll();
			AssertHasRowError(Question, "You need to specify 2 correct answer(s)");

			Question.HY_Max = 1;
			Question.Validation.ValidateAll();
			AssertNoRowErrors(Question);
		}

		LearningCentreQuestion question;
		LearningCentreQuestion Question
		{
			get { return question ?? (question = Campaign.Questions.AddNew()); }
		}

		LearningCentreCampaign campaign;
		LearningCentreCampaign Campaign
		{
			get { return campaign ?? (campaign = Factory.New<LearningCentreCampaign>()); }
		}
	}
}
