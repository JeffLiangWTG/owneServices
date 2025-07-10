using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class VoteExamSurveyAnswerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAnswerOptionList_VotingItem()
		{
			VoteExamSurveyQuestion question = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswer answer = Factory.New<VoteExamSurveyAnswer>();
			answer.HZ_HY = question.PK;
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			AssertEquals("Campaign is not specified. Should be empty", 0, answer.Lookups.AnswerOptionList.Count);

			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Campaign.VoteHeader.SubQuestions.Add(question);
			Campaign.VoteHeader.HY_Max = 4;
			question.HY_G0 = Campaign.PK;
			AssertEquals(5, answer.Lookups.AnswerOptionList.Count);
			CodeDescriptionPairList pairList = (CodeDescriptionPairList)answer.Lookups.AnswerOptionList;
			Assert(pairList.ContainsCode(""));
			Assert(pairList.ContainsCode("1"));
			Assert(pairList.ContainsCode("2"));
			Assert(pairList.ContainsCode("3"));
			Assert(pairList.ContainsCode("4"));
		}

		public void TestAnswerOptionList_LikertScale()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			AssertEquals(5, Answer.Lookups.AnswerOptionList.Count);
			CodeDescriptionPairList pairList = (CodeDescriptionPairList)Answer.Lookups.AnswerOptionList;
			AssertEquals("Strongly Disagree", pairList["1"].Description);
			AssertEquals("Disagree", pairList["2"].Description);
			AssertEquals("Undecided", pairList["3"].Description);
			AssertEquals("Agree", pairList["4"].Description);
			AssertEquals("Strongly Agree", pairList["5"].Description);
		}

		public void TestAnswerOptionList_NumericScale()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			Question.HY_Min = 0;
			Question.HY_Max = 3;
			AssertEquals(5, Answer.Lookups.AnswerOptionList.Count);
			CodeDescriptionPairList pairList = (CodeDescriptionPairList)Answer.Lookups.AnswerOptionList;
			Assert(pairList.ContainsCode(""));
			Assert(pairList.ContainsCode("0"));
			Assert(pairList.ContainsCode("1"));
			Assert(pairList.ContainsCode("2"));
			Assert(pairList.ContainsCode("3"));
		}

		public void TestAnswerOptionList_Percentage()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Percentage;
			AssertEquals(22, Answer.Lookups.AnswerOptionList.Count);
			CodeDescriptionPairList pairList = (CodeDescriptionPairList)Answer.Lookups.AnswerOptionList;
			Assert(pairList.ContainsCode(""));
			Assert(pairList.ContainsCode("0"));
			Assert(pairList.ContainsCode("5"));
			Assert(pairList.ContainsCode("10"));
			Assert(pairList.ContainsCode("15"));
			Assert(pairList.ContainsCode("20"));
			Assert(pairList.ContainsCode("25"));
			Assert(pairList.ContainsCode("30"));
			Assert(pairList.ContainsCode("35"));
			Assert(pairList.ContainsCode("40"));
			Assert(pairList.ContainsCode("45"));
			Assert(pairList.ContainsCode("50"));
			Assert(pairList.ContainsCode("55"));
			Assert(pairList.ContainsCode("60"));
			Assert(pairList.ContainsCode("65"));
			Assert(pairList.ContainsCode("70"));
			Assert(pairList.ContainsCode("75"));
			Assert(pairList.ContainsCode("80"));
			Assert(pairList.ContainsCode("85"));
			Assert(pairList.ContainsCode("90"));
			Assert(pairList.ContainsCode("95"));
			Assert(pairList.ContainsCode("100"));
		}

		public void TestAnswerOptionList_MultipleChoice()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			AssertEquals(Question.SubQuestions, Answer.Lookups.AnswerOptionList);
		}

		public void TestAnswerOptionList_TrueFalse()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			AssertEquals(2, Answer.Lookups.AnswerOptionList.Count);
			CodeDescriptionPairList pairList = (CodeDescriptionPairList)Answer.Lookups.AnswerOptionList;
			AssertEquals("True", pairList["1"].Description);
			AssertEquals("False", pairList["2"].Description);
		}

		public void TestAnswerOptionList_YesNo()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			AssertEquals(2, Answer.Lookups.AnswerOptionList.Count);
			CodeDescriptionPairList pairList = (CodeDescriptionPairList)Answer.Lookups.AnswerOptionList;
			AssertEquals("Yes", pairList["1"].Description);
			AssertEquals("No", pairList["2"].Description);
		}

		VoteExamSurveyAnswer Answer
		{
			get
			{
				if (fAnswer == null)
				{
					fAnswer = Factory.New<VoteExamSurveyAnswer>();
					fAnswer.HZ_HY = Question.PK;
				}
				return fAnswer;
			}
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

		VoteExamSurveyAnswer fAnswer;
		VoteExamSurveyQuestion fQuestion;
		GlbCompanyCampaign fCampaign;
	}
}
