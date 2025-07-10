using System.Collections;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class VoteExamSurveyAnswerOptionListHelperTest : TestCaseWithFactory
	{
		public void TestAnswerOptionList_LikertScale()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			CodeDescriptionPairList pairList = (CodeDescriptionPairList)VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(Question);
			AssertEquals(5, pairList.Count);
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

			CodeDescriptionPairList pairList = (CodeDescriptionPairList)VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(Question);
			AssertEquals(5, pairList.Count);
			Assert(pairList.ContainsCode(""));
			Assert(pairList.ContainsCode("0"));
			Assert(pairList.ContainsCode("1"));
			Assert(pairList.ContainsCode("2"));
			Assert(pairList.ContainsCode("3"));
		}

		public void TestAnswerOptionList_Percentage()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Percentage;
			CodeDescriptionPairList pairList = (CodeDescriptionPairList)VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(Question);
			AssertEquals(22, pairList.Count);
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
			ICollection collection = VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(Question);
			AssertEquals(Question.SubQuestions, collection);
		}

		public void TestAnswerOptionList_TrueFalse()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			CodeDescriptionPairList pairList = (CodeDescriptionPairList)VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(Question);
			AssertEquals(2, pairList.Count);
			AssertEquals("True", pairList["1"].Description);
			AssertEquals("False", pairList["2"].Description);
		}

		public void TestAnswerOptionList_YesNo()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			CodeDescriptionPairList pairList = (CodeDescriptionPairList)VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(Question);
			AssertEquals(2, pairList.Count);
			AssertEquals("Yes", pairList["1"].Description);
			AssertEquals("No", pairList["2"].Description);
		}

		public void TestGetAnswerOptionList_NoEmptyElementForNumericScale()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			Question.HY_Min = 0;
			Question.HY_Max = 3;

			CodeDescriptionPairList pairList = (CodeDescriptionPairList)VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(Question, false);
			AssertEquals(4, pairList.Count);
			Assert(pairList.ContainsCode("0"));
			Assert(pairList.ContainsCode("1"));
			Assert(pairList.ContainsCode("2"));
			Assert(pairList.ContainsCode("3"));
		}

		#region Implementation

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
					fCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
				}
				return fCampaign;
			}
		}

		VoteExamSurveyQuestion fQuestion;
		GlbCompanyCampaign fCampaign;

		#endregion
	}
}
