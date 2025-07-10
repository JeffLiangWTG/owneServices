using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class FixedMultipleChoiceSingleAnswerCapturerTest : TestCaseWithFactory
	{
		public void TestEnumerable()
		{
			List<IBindableBooleanItem> booleanItems = new List<IBindableBooleanItem>(AnswerCapturer);
			AssertEquals(5, booleanItems.Count);
			AssertBooleanItem(booleanItems[0], "Strongly Disagree", false);
			AssertBooleanItem(booleanItems[1], "Disagree", false);
			AssertBooleanItem(booleanItems[2], "Undecided", false);
			AssertBooleanItem(booleanItems[3], "Agree", false);
			AssertBooleanItem(booleanItems[4], "Strongly Agree", false);
		}

		public void TestEnumerable_DescriptionNotAvailable()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			Question.HY_Min = 1;
			Question.HY_Max = 5;

			List<IBindableBooleanItem> booleanItems = new List<IBindableBooleanItem>(AnswerCapturer);
			AssertEquals(6, booleanItems.Count);
			AssertBooleanItem(booleanItems[0], "", false);
			AssertBooleanItem(booleanItems[1], "1", false);
			AssertBooleanItem(booleanItems[2], "2", false);
			AssertBooleanItem(booleanItems[3], "3", false);
			AssertBooleanItem(booleanItems[4], "4", false);
			AssertBooleanItem(booleanItems[5], "5", false);
		}

		public void TestSelectedIndexShouldBeSet()
		{
			Answer.AnswerAsInt = 4;
			List<IBindableBooleanItem> booleanItems = new List<IBindableBooleanItem>(AnswerCapturer);
			AssertEquals(5, booleanItems.Count);
			AssertBooleanItem(booleanItems[0], "Strongly Disagree", false);
			AssertBooleanItem(booleanItems[1], "Disagree", false);
			AssertBooleanItem(booleanItems[2], "Undecided", false);
			AssertBooleanItem(booleanItems[3], "Agree", true);
			AssertBooleanItem(booleanItems[4], "Strongly Agree", false);
		}

		public void TestSetAnswer()
		{
			AssertEquals("Pre-condition", 0, Answer.AnswerAsInt);

			List<IBindableBooleanItem> booleanItems = new List<IBindableBooleanItem>(AnswerCapturer);
			AssertEquals(5, booleanItems.Count);
			booleanItems[0].BoolValue = true;
			AssertEquals(1, Answer.AnswerAsInt);

			booleanItems[0].BoolValue = false;
			booleanItems[3].BoolValue = true;
			AssertEquals(4, Answer.AnswerAsInt);
		}

		#region Implementation

		FixedMultipleChoiceSingleAnswerCapturer AnswerCapturer
		{
			get
			{
				if (fAnswerCapturer == null)
				{
					fAnswerCapturer = new FixedMultipleChoiceSingleAnswerCapturer(Answer);
				}
				return fAnswerCapturer;
			}
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
					fQuestion = Factory.New<VoteExamSurveyQuestion>();
					fQuestion.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
				}
				return fQuestion;
			}
		}

		void AssertBooleanItem(IBindableBooleanItem booleanItem, string expectedText, bool expectedValue)
		{
			AssertEquals(expectedText, booleanItem.Text);
			AssertEquals(expectedValue, booleanItem.BoolValue);
		}

		FixedMultipleChoiceSingleAnswerCapturer fAnswerCapturer;
		VoteExamSurveyAnswer fAnswer;
		VoteExamSurveyQuestion fQuestion;

		#endregion
	}
}
