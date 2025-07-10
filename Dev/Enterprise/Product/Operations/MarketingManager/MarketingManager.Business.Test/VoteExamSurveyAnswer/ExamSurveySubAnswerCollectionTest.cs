using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(ExamSurveySubAnswerCollection))]
	sealed class ExamSurveySubAnswerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoad_SubQuestion()
		{
			ExamSurveySubAnswerCollection collection = new ExamSurveySubAnswerCollection(Question.SubQuestions[0], VoteExamSurveyAnswerSet, 0);
			collection.Load();
			AssertEquals("Should not load elements if Question is a sub question", 0, collection.Count);
		}

		public void TestQuestionOrder()
		{
			ZShort questionOrder = 1;
			var collection = new ExamSurveySubAnswerCollection(Question, VoteExamSurveyAnswerSet, questionOrder);
			((IVoteExamSurveyAnswerSet)VoteExamSurveyAnswerSet).StartVoteExamSurvey();
			collection.Load();
			AssertEquals(questionOrder, collection[0].HZ_QuestionOrder);
			AssertEquals(questionOrder, collection[1].HZ_QuestionOrder);
			AssertEquals(questionOrder, collection[2].HZ_QuestionOrder);
		}

		public void TestLoad_ClearCollectionBeforeLoad()
		{
			VoteExamSurveyAnswer answerToBeAddedAndRemoved = Factory.New<VoteExamSurveyAnswer>();
			Collection.Add(answerToBeAddedAndRemoved);
			AssertEquals("Pre-condition", 1, Collection.Count);

			((IVoteExamSurveyAnswerSet)VoteExamSurveyAnswerSet).StartVoteExamSurvey();
			Collection.Load();
			AssertEquals(3, Collection.Count);
			AssertCollectionNotContains(answerToBeAddedAndRemoved, Collection);
			Assert("Should only be removed, not deleted", !answerToBeAddedAndRemoved.IsDeleted);
		}

		public void TestEnumerable()
		{
			List<IBindableBooleanItem> booleanItems = new List<IBindableBooleanItem>(Collection);
			AssertEquals("Collection not loaded", 0, booleanItems.Count);

			((IVoteExamSurveyAnswerSet)VoteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer selectedOption = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(Question.SubQuestions.First(x => x.HY_Question == "Option 3"), VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			selectedOption.AnswerAsBool = true;

			Collection.Load();
			booleanItems = new List<IBindableBooleanItem>(Collection);
			AssertEquals(3, booleanItems.Count);
			AssertBooleanItem(booleanItems[0], "Option 1", false);
			AssertBooleanItem(booleanItems[1], "Option 2", false);
			AssertBooleanItem(booleanItems[2], "Option 3", true);
		}

		public void TestSetAnswer()
		{
			AssertEquals(3, Question.SubQuestions.Count);
			AssertEquals(1, VoteExamSurveyAnswerSet.CompanyCampaign.Questions.Count);
			((IVoteExamSurveyAnswerSet)VoteExamSurveyAnswerSet).StartVoteExamSurvey();
			Collection.Load();
			List<IBindableBooleanItem> booleanItems = new List<IBindableBooleanItem>(Collection);
			Assert("Pre-condition", !VoteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(Question.SubQuestions[1], VoteExamSurveyAnswerSet.ExamCampaignItems[0]).AnswerAsBool);

			booleanItems[1].BoolValue = true;
			Assert(VoteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(Question.SubQuestions[1], VoteExamSurveyAnswerSet.ExamCampaignItems[0]).AnswerAsBool);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			// This collection can never be added to via Binding.
			// The AddNew() method throws a NotSupportedException.
			Assert(true);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ExamSurveySubAnswerCollection(Question, VoteExamSurveyAnswerSet, 1);
		}

		new ExamSurveySubAnswerCollection Collection
		{
			get { return (ExamSurveySubAnswerCollection)base.Collection; }
		}

		VoteExamSurveyQuestion Question
		{
			get
			{
				if (fQuestion == null)
				{
					fQuestion = VoteExamSurveyAnswerSet.CompanyCampaign.Questions.AddNew();
					fQuestion.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;

					VoteExamSurveyQuestion option1 = fQuestion.SubQuestions.AddNew();
					option1.HY_Question = "Option 1";

					VoteExamSurveyQuestion option2 = fQuestion.SubQuestions.AddNew();
					option2.HY_Question = "Option 2";

					VoteExamSurveyQuestion option3 = fQuestion.SubQuestions.AddNew();
					option3.HY_Question = "Option 3";

					VoteExamSurveyAnswer selectedOption = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(option3, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
					selectedOption.AnswerAsBool = true;
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

		void AssertBooleanItem(IBindableBooleanItem booleanItem, string expectedText, bool expectedValue)
		{
			AssertEquals(expectedText, booleanItem.Text);
			AssertEquals(expectedValue, booleanItem.BoolValue);
		}

		VoteExamSurveyQuestion fQuestion;
		VoteExamSurveyAnswerSet fVoteExamSurveyAnswerSet;
	}
}
