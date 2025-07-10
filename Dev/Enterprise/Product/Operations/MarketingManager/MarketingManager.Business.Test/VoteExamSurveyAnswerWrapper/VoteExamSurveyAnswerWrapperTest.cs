
using System.Linq;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveyAnswerWrapper))]
	sealed class VoteExamSurveyAnswerWrapperTest : VoteExamSurveyAnswerWrapperBaseTest<VoteExamSurveyAnswerWrapper>
	{
		public void TestGetMultipleAnswers()
		{
			var item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Campaign.PK;
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, item);
			var answerWrapper = GetNewAnswerWrapperForTest(Question, voteExamSurveyAnswerSet);
			ExamSurveySubAnswerCollection multipleAnswers = AnswerWrapper.GetMultipleAnswers();
			AssertEquals("Pre-condition, no subquestions", 0, multipleAnswers.Count);
			Assert("Should be registered as an editable child", AnswerWrapper.IsRegisteredEditableChildObject(multipleAnswers));

			VoteExamSurveyQuestion option1 = Question.SubQuestions.AddNew();

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			answerWrapper = GetNewAnswerWrapperForTest(Question, voteExamSurveyAnswerSet);
			multipleAnswers = answerWrapper.GetMultipleAnswers();
			AssertEquals(1, multipleAnswers.Count);
			AssertEquals(option1.PK, multipleAnswers[0].HZ_HY);

			VoteExamSurveyQuestion option2 = Question.SubQuestions.AddNew();

			item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Campaign.PK;
			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, item);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			answerWrapper = GetNewAnswerWrapperForTest(Question, voteExamSurveyAnswerSet);
			multipleAnswers = answerWrapper.GetMultipleAnswers();
			AssertEquals(2, multipleAnswers.Count);
			AssertEquals(option2.PK, multipleAnswers[1].HZ_HY);
		}

		public void TestGetMultipleAnswers_FromSubQuestion()
		{
			VoteExamSurveyQuestion subQuestion = Question.SubQuestions.AddNew();
			const int questionsPerPage = 5;
			VoteExamSurveyAnswerWrapper answerWrapper = new VoteExamSurveyAnswerWrapper(subQuestion, VoteExamSurveyAnswerSet, VoteExamSurveyAnswerSet.ExamCampaignItems[0], questionsPerPage);
			ExamSurveySubAnswerCollection multipleAnswers = answerWrapper.GetMultipleAnswers();
			AssertEquals("Should not load MultipleAnswers if Question is a sub-question", 0, multipleAnswers.Count);
		}

		public void TestGetMultipleAnswers_LoadsOrderedSubQuestionPKs()
		{
			var campaign = Factory.New<CampaignWithRandomisedQuestionsForTest>();
			var campaignItem = Factory.New<CampaignItemWithRandomisedQuestionsForTest>();
			campaignItem.G8_G0 = campaign.PK;
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);

			var question = campaignItem.CompanyCampaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			var option1 = question.SubQuestions.AddNew();
			var option2 = question.SubQuestions.AddNew();
			var option3 = question.SubQuestions.AddNew();

			question.HY_IsRandomisable = false;

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			var answerWrapper = GetNewAnswerWrapperForTest(question, voteExamSurveyAnswerSet);
			answerWrapper.GetMultipleAnswers();

			AssertEquals(option1.PK, answerWrapper.OrderedSubQuestionPKs[0]);
			AssertEquals(option2.PK, answerWrapper.OrderedSubQuestionPKs[1]);
			AssertEquals(option3.PK, answerWrapper.OrderedSubQuestionPKs[2]);

			question.HY_IsRandomisable = true;

			campaignItem = Factory.New<CampaignItemWithRandomisedQuestionsForTest>();
			campaignItem.G8_G0 = campaign.PK;
			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			answerWrapper = GetNewAnswerWrapperForTest(question, voteExamSurveyAnswerSet);
			answerWrapper.GetMultipleAnswers();

			AssertEquals(option1.PK, answerWrapper.OrderedSubQuestionPKs[0]);
			AssertEquals(option3.PK, answerWrapper.OrderedSubQuestionPKs[1]);
			AssertEquals(option2.PK, answerWrapper.OrderedSubQuestionPKs[2]);
		}

		public void TestIsAnswered()
		{
			Assert("Should be single choice question", !Question.IsMultipleChoiceQuestion);
			VoteExamSurveyAnswerWrapper answerWrapper = GetNewAnswerWrapperForTest(Question, VoteExamSurveyAnswerSet);
			Assert("Not answered", !answerWrapper.IsAnswered);
			answerWrapper.SingleAnswer.AnswerAsBool = true;
			Assert("Answered", answerWrapper.IsAnswered);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			Question.HY_Min = 2;
			Question.HY_Max = 2;
			VoteExamSurveyQuestion option1 = Question.SubQuestions.AddNew();
			VoteExamSurveyQuestion option2 = Question.SubQuestions.AddNew();
			Assert("Should be multiple choice question", Question.IsMultipleChoiceQuestion);

			((IVoteExamSurveyAnswerSet)VoteExamSurveyAnswerSet).StartVoteExamSurvey();
			answerWrapper = GetNewAnswerWrapperForTest(Question, VoteExamSurveyAnswerSet);
			ExamSurveySubAnswerCollection multipleAnswers = answerWrapper.GetMultipleAnswers();
			multipleAnswers[0].AnswerAsBool = true;
			Assert("Not all options answered", !answerWrapper.IsAnswered);
			multipleAnswers[1].AnswerAsBool = true;
			Assert("Answered", answerWrapper.IsAnswered);
		}

		public void TestGetAllPersistedAnswers()
		{
			var item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Campaign.PK;
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, item);
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswerWrapper answerWrapper = GetNewAnswerWrapperForTest(Question, voteExamSurveyAnswerSet);
			answerWrapper.SingleAnswer.HZ_Answer = "";
			var allPersistedAnswers = answerWrapper.GetAllPersistedAnswers();
			AssertEquals(1, allPersistedAnswers.Count());

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			Question.HY_Min = 2;
			Question.HY_Max = 2;
			VoteExamSurveyQuestion option1 = Question.SubQuestions.AddNew();
			VoteExamSurveyQuestion option2 = Question.SubQuestions.AddNew();

			item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Campaign.PK;
			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, item);
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			answerWrapper.SingleAnswer.HZ_Answer = "";

			answerWrapper = GetNewAnswerWrapperForTest(Question, voteExamSurveyAnswerSet);
			allPersistedAnswers = answerWrapper.GetAllPersistedAnswers();
			AssertEquals(3, allPersistedAnswers.Count());
		}

		protected override VoteExamSurveyAnswerWrapper GetNewAnswerWrapperForTest(VoteExamSurveyQuestion question, IVoteExamSurveyAnswerSet voteExamSurveyAnswerSet)
		{
			return new VoteExamSurveyAnswerWrapper(question, voteExamSurveyAnswerSet, voteExamSurveyAnswerSet.ExamCampaignItems[0], voteExamSurveyAnswerSet.CompanyCampaign.G0_QuestionsPerWebPage);
		}
	}
}
