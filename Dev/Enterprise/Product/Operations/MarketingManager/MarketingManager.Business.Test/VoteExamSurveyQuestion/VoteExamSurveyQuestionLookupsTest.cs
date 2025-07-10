using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class VoteExamSurveyQuestionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAnswerTypes()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			VoteExamSurveyQuestion question = campaign.VoteHeader.SubQuestions.AddNew();
			AssertEquals(new VoteExamSurveyAnswerTypeList(question).CodesAsString, question.Lookups.AnswerTypes.CodesAsString);
		}

		public void TestCorrectAnswerOptionList()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			string expected = ((CodeDescriptionPairList)VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(question)).CodesAsString;
			string generated = ((CodeDescriptionPairList)question.Lookups.CorrectAnswerOptionList).CodesAsString;
			AssertEquals(expected, generated);
		}

		public void TestCorrectAnswerOptionListForNonMultipleChoiceQuestion()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			AssertNull("Should be returning null as Customized Multiple Choice options should only be shown in the grid", question.Lookups.CorrectAnswerOptionListForNonMultipleChoiceQuestion);

			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			AssertEquals(question.Lookups.CorrectAnswerOptionList.GetType(), question.Lookups.CorrectAnswerOptionListForNonMultipleChoiceQuestion.GetType());
			AssertContainsExactElementsInAnyOrder(question.Lookups.CorrectAnswerOptionList, question.Lookups.CorrectAnswerOptionListForNonMultipleChoiceQuestion);
		}
	}
}
