using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(ExamSurveyMultipleChoiceAnswerCapturerBizO))]
	sealed class ExamSurveyMultipleChoiceAnswerCapturerBizOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEnumerator_FixedMultipleChoiceQuestion()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			AssertFixedMultipleChoiceAnswers("Strongly Disagree", "Disagree", "Undecided", "Agree", "Strongly Agree");

			ResetTestObjects();
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			AssertFixedMultipleChoiceAnswers("Yes", "No");

			ResetTestObjects();
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			AssertFixedMultipleChoiceAnswers("True", "False");
		}

		public void TestEnumerator_CustomisedMultipleChoiceQuestion()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion option1 = Question.SubQuestions.AddNew();
			VoteExamSurveyQuestion option2 = Question.SubQuestions.AddNew();
			VoteExamSurveyQuestion option3 = Question.SubQuestions.AddNew();

			((IVoteExamSurveyAnswerSet)VoteExamSurveyAnswerSet).StartVoteExamSurvey();
			fAnswerWrapper = VoteExamSurveyAnswerSet.AnswerWrappers[0] as VoteExamSurveyAnswerWrapper;

			List<IBindableBooleanItem> booleanItems = new List<IBindableBooleanItem>(AnswerCapturerBizO);
			AssertEquals(3, booleanItems.Count);
			AssertEquals(option1, ((VoteExamSurveyAnswer)booleanItems[0]).Question);
			AssertEquals(option2, ((VoteExamSurveyAnswer)booleanItems[1]).Question);
			AssertEquals(option3, ((VoteExamSurveyAnswer)booleanItems[2]).Question);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return AnswerCapturerBizO;
		}

		void AssertFixedMultipleChoiceAnswers(params string[] expectedOptions)
		{
			List<IBindableBooleanItem> answerCapturerList = new List<IBindableBooleanItem>(AnswerCapturerBizO);
			AssertEquals(expectedOptions.Length, answerCapturerList.Count);
			for (int i = 0; i < expectedOptions.Length; i++)
			{
				AssertEquals(expectedOptions[i], answerCapturerList[i].Text);
			}
		}

		ExamSurveyMultipleChoiceAnswerCapturerBizO AnswerCapturerBizO
		{
			get
			{
				if (fAnswerCapturerBizO == null)
				{
					fAnswerCapturerBizO = new ExamSurveyMultipleChoiceAnswerCapturerBizO(AnswerWrapper);
				}
				return fAnswerCapturerBizO;
			}
		}

		VoteExamSurveyAnswerWrapper AnswerWrapper
		{
			get
			{
				if (fAnswerWrapper == null)
				{
					fAnswerWrapper = new VoteExamSurveyAnswerWrapper(Question, VoteExamSurveyAnswerSet, VoteExamSurveyAnswerSet.ExamCampaignItems[0], VoteExamSurveyAnswerSet.CompanyCampaign.G0_QuestionsPerWebPage);
				}
				return fAnswerWrapper;
			}
		}

		VoteExamSurveyQuestion Question
		{
			get
			{
				if (fQuestion == null)
				{
					fQuestion = VoteExamSurveyAnswerSet.CompanyCampaign.Questions.AddNew();
				}
				return fQuestion;
			}
		}

		VoteExamSurveyAnswerSet VoteExamSurveyAnswerSet
		{
			get
			{
				if (fVoteExamSurveyAnswerSet == null)
				{
					var campaignItem = Factory.New<GlbCompanyCampaignItem>();
					campaignItem.G8_G0 = Factory.New<GlbCompanyCampaign>().PK;
					fVoteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
				}
				return fVoteExamSurveyAnswerSet;
			}
		}

		void ResetTestObjects()
		{
			fVoteExamSurveyAnswerSet = null;
			fAnswerWrapper = null;
			fAnswerCapturerBizO = null;
		}

		ExamSurveyMultipleChoiceAnswerCapturerBizO fAnswerCapturerBizO;
		VoteExamSurveyAnswerWrapper fAnswerWrapper;
		VoteExamSurveyQuestion fQuestion;
		VoteExamSurveyAnswerSet fVoteExamSurveyAnswerSet;

		#endregion
	}
}
