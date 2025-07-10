using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business.Testing
{
	abstract class VoteExamSurveyQuestionSetTestCase<T> : ActiveBusinessObjectCollectionTestCase<T> where T : VoteExamSurveyQuestionSet
	{
		public void TestSupportsSorting()
		{
			Assert("Should not support sorting since we have a customised sort logic", !((IBindingList)Collection).SupportsSorting);
		}

		public void TestSortingLogic()
		{
			AddQuestionsForTest();
			AssertEquals(8, Collection.Count);
			AssertQuestionsOrder(
				"Header 1",
				"Question 1",
				"Question 2",
				"Header 2",
				"Question 3",
				"Question 4",
				"Question 5",
				"Header 3");
		}

		#region Question Ordering

		public void TestDelete_ShouldReOrderQuestionsCorrectly()
		{
			AddQuestionsForTest();
			Collection.Delete(Collection[2]);
			AssertQuestionsOrder(
				"Header 1",
				"Question 1",
				"Header 2",
				"Question 3",
				"Question 4",
				"Question 5",
				"Header 3");

			Collection.Delete(Collection[2]);
			AssertQuestionsOrder(
				"Header 1",
				"Question 1",
				"Question 3",
				"Question 4",
				"Question 5",
				"Header 3");
		}

		public void TestHandleAnswerTypeChanging()
		{
			AddQuestionsForTest();
			Collection[0].HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Percentage;
			AssertQuestionsOrder("Header 1", "Question 1", "Question 2", "Header 2", "Question 3", "Question 4", "Question 5", "Header 3");

			Collection[0].HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			Collection[1].HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			AssertQuestionsOrder("Header 1", "Question 1", "Question 2", "Header 2", "Question 3", "Question 4", "Question 5", "Header 3");

			Collection[1].HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			Collection[2].HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			AssertQuestionsOrder("Header 1", "Question 1", "Question 2", "Header 2", "Question 3", "Question 4", "Question 5", "Header 3");

			Collection[5].HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			Collection[6].HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			AssertQuestionsOrder("Header 1", "Question 1", "Question 2", "Header 2", "Question 3", "Question 4", "Question 5", "Header 3");
		}

		public void TestHandleQuestionOrderChanging_ShiftForward()
		{
			AddQuestionsForTest();
			Collection[0].ActualOrder = (ZShort)8;
			AssertQuestionsOrder("Question 1", "Question 2", "Header 2", "Question 3", "Question 4", "Question 5", "Header 1", "Header 3");

			Collection[0].ActualOrder = (ZShort)8;
			AssertQuestionsOrder("Question 2", "Header 2", "Question 3", "Question 4", "Question 5", "Header 1", "Header 3", "Question 1");

			Collection[0].ActualOrder = (ZShort)8;
			AssertQuestionsOrder("Header 2", "Question 3", "Question 4", "Question 5", "Header 1", "Header 3", "Question 1", "Question 2");

			Collection[1].ActualOrder = (ZShort)4;
			AssertQuestionsOrder("Header 2", "Question 4", "Question 5", "Header 1", "Header 3", "Question 1", "Question 3", "Question 2");

			Collection[0].ActualOrder = (ZShort)2;
			AssertQuestionsOrder("Question 4", "Header 2", "Question 5", "Header 1", "Header 3", "Question 1", "Question 3", "Question 2");

			Collection[5].ActualOrder = (ZShort)4;
			AssertQuestionsOrder("Question 4", "Header 2", "Question 5", "Header 1", "Header 3", "Question 3", "Question 1", "Question 2");
		}

		public void TestHandleQuestionOrderChanging_ShiftBackward()
		{
			AddQuestionsForTest();
			Collection[7].ActualOrder = (ZShort)1;
			AssertQuestionsOrder("Header 1", "Header 3", "Question 1", "Question 2", "Header 2", "Question 3", "Question 4", "Question 5");

			Collection[7].ActualOrder = (ZShort)1;
			AssertQuestionsOrder("Question 5", "Header 1", "Header 3", "Question 1", "Question 2", "Header 2", "Question 3", "Question 4");

			Collection[7].ActualOrder = (ZShort)1;
			AssertQuestionsOrder("Question 4", "Question 5", "Header 1", "Header 3", "Question 1", "Question 2", "Header 2", "Question 3");

			Collection[7].ActualOrder = (ZShort)2;
			AssertQuestionsOrder("Question 4", "Question 3", "Question 5", "Header 1", "Header 3", "Question 1", "Question 2", "Header 2");

			Collection[7].ActualOrder = (ZShort)5;
			AssertQuestionsOrder("Question 4", "Question 3", "Question 5", "Header 1", "Header 3", "Question 1", "Header 2", "Question 2");

			Collection[5].ActualOrder = (ZShort)2;
			AssertQuestionsOrder("Question 4", "Question 1", "Question 3", "Question 5", "Header 1", "Header 2", "Header 3", "Question 2");
		}

		public void TestHandleQuestionOrderChanging_ShiftForwardAndBackward()
		{
			AddQuestionsForTest();
			Collection[0].ActualOrder = (ZShort)8;
			AssertQuestionsOrder("Question 1", "Question 2", "Header 2", "Question 3", "Question 4", "Question 5", "Header 1", "Header 3");

			Collection[7].ActualOrder = (ZShort)1;
			AssertQuestionsOrder("Header 3", "Question 1", "Question 2", "Header 2", "Question 3", "Question 4", "Question 5", "Header 1");

			Collection[4].ActualOrder = (ZShort)1;
			AssertQuestionsOrder("Question 3", "Header 3", "Question 1", "Question 2", "Header 2", "Question 4", "Question 5", "Header 1");

			Collection[2].ActualOrder = (ZShort)4;
			AssertQuestionsOrder("Question 3", "Header 3", "Question 2", "Header 2", "Question 4", "Question 1", "Question 5", "Header 1");

			Collection[4].ActualOrder = (ZShort)4;
			AssertQuestionsOrder("Question 3", "Header 3", "Question 2", "Header 2", "Question 1", "Question 4", "Question 5", "Header 1");

			Collection[4].ActualOrder = (ZShort)6;
			AssertQuestionsOrder("Question 3", "Header 3", "Question 2", "Header 2", "Question 4", "Question 5", "Header 1", "Question 1");
		}

		public void TestHandleQuestionOrderChanging_NewRows()
		{
			AddQuestionsForTest();
			VoteExamSurveyQuestion newQuestion = Collection.AddNew();
			newQuestion.HY_Question = "New Question";
			newQuestion.ActualOrder = (short)1;
			AssertQuestionsOrder("New Question", "Header 1", "Question 1", "Question 2", "Header 2", "Question 3", "Question 4", "Question 5", "Header 3");

			newQuestion = Collection.AddNew();
			newQuestion.HY_Question = "New Question 2";
			newQuestion.ActualOrder = (short)2;
			AssertQuestionsOrder("New Question", "New Question 2", "Header 1", "Question 1", "Question 2", "Header 2", "Question 3", "Question 4", "Question 5", "Header 3");
		}

		#endregion

		public void TestGetValidQuestionOrder()
		{
			AddQuestionsForTest();
			AssertEquals((short)1, Collection.GetValidQuestionOrder(0));
			AssertEquals((short)1, Collection.GetValidQuestionOrder(-43));
			AssertEquals((short)1, Collection.GetValidQuestionOrder(-2));

			AssertEquals((short)6, Collection.GetValidQuestionOrder(1000));
			AssertEquals((short)6, Collection.GetValidQuestionOrder(7));
			AssertEquals((short)6, Collection.GetValidQuestionOrder(666));

			AssertEquals((short)5, Collection.GetValidQuestionOrder(5));
		}

		public void TestSchemaOrderColumn()
		{
			AssertEquals(ExpectedSchemaOrderColumn, Collection.SchemaOrderColumn);
		}

		public void TestFilteredQuestions()
		{
			VoteExamSurveyQuestionSet activeQuestions = GetNewQuestionSet(true);
			VoteExamSurveyQuestionSet inactiveQuestions = GetNewQuestionSet(false);
			VoteExamSurveyQuestionSet allQuestions = GetNewQuestionSet(null);

			AssertEquals(GetCollectionIndex(allQuestions), GetCollectionIndex(allQuestions.AllQuestions));
			AssertEquals(GetCollectionIndex(inactiveQuestions), GetCollectionIndex(allQuestions.InactiveQuestions));
			AssertEquals(GetCollectionIndex(activeQuestions), GetCollectionIndex(allQuestions.ActiveQuestions));

			AssertEquals(GetCollectionIndex(allQuestions), GetCollectionIndex(inactiveQuestions.AllQuestions));
			AssertEquals(GetCollectionIndex(inactiveQuestions), GetCollectionIndex(inactiveQuestions.InactiveQuestions));
			AssertEquals(GetCollectionIndex(activeQuestions), GetCollectionIndex(inactiveQuestions.ActiveQuestions));

			AssertEquals(GetCollectionIndex(allQuestions), GetCollectionIndex(activeQuestions.AllQuestions));
			AssertEquals(GetCollectionIndex(inactiveQuestions), GetCollectionIndex(activeQuestions.InactiveQuestions));
			AssertEquals(GetCollectionIndex(activeQuestions), GetCollectionIndex(activeQuestions.ActiveQuestions));

			VoteExamSurveyQuestion question1 = CreateQuestion("q1", 1);
			VoteExamSurveyQuestion question2 = CreateQuestion("q2", 2);
			VoteExamSurveyQuestion question3 = CreateQuestion("q3", 3);
			question1.HY_IsActive = false;
			AssertEquals(3, allQuestions.Count);
			AssertEquals(2, activeQuestions.Count);
			AssertEquals(1, inactiveQuestions.Count);
		}

		static object GetCollectionIndex(IActiveBusinessObjectCollection collection)
		{
			return collection.Index;
		}

		public void TestAllowNew()
		{
			VoteExamSurveyQuestionSet activeQuestions = GetNewQuestionSet(true);
			VoteExamSurveyQuestionSet inactiveQuestions = GetNewQuestionSet(false);
			VoteExamSurveyQuestionSet allQuestions = GetNewQuestionSet(null);

			Assert(((IActiveBusinessObjectCollection)activeQuestions).AllowNew);
			Assert(((IActiveBusinessObjectCollection)allQuestions).AllowNew);
			Assert(!((IActiveBusinessObjectCollection)inactiveQuestions).AllowNew);
		}

		protected abstract string DefaultAnswerTypeForNewQuestion { get; }
		protected abstract SchemaShortColumn ExpectedSchemaOrderColumn { get; }
		protected abstract VoteExamSurveyQuestionSet GetNewQuestionSet(bool? isActiveFilter);

		#region Implementation

		void AddQuestionsForTest()
		{
			using (Collection.DisableQuestionsShiftingForTest())
			{
				CreateQuestion("Question 5", 5);
				CreateQuestion("Question 4", 4);
				CreateQuestion("Question 1", 1);
				CreateQuestion("Question 2", 2);
				CreateQuestion("Question 3", 3);

				CreateQuestion("Header 1", 1, true);
				CreateQuestion("Header 2", 3, true);
				CreateQuestion("Header 3", 6, true);
			}
		}

		void AssertQuestionsOrder(params string[] questionTextArray)
		{
			ZShort expectedOrder = 1;
			AssertEquals(questionTextArray.Length, Collection.Count);
			for (int i = 0; i < questionTextArray.Length; i++)
			{
				AssertQuestionOrder(Collection[i], expectedOrder, questionTextArray[i]);
				if (!Collection[i].IsHeader)
				{
					expectedOrder++;
				}
			}
		}

		protected virtual void AssertQuestionOrder(VoteExamSurveyQuestion questionToAssert, ZShort expectedOrder, string expectedText)
		{
			string failureMessage = string.Format("{0} for Question '{1}' should have value of {2} but value is {3} instead.",
					Collection.SchemaOrderColumn.Name,
					questionToAssert.HY_Question,
					expectedOrder,
					questionToAssert[Collection.SchemaOrderColumn]);
			AssertEquals(failureMessage, expectedOrder, questionToAssert[Collection.SchemaOrderColumn]);
			AssertEquals("Question Set incorrectly ordered", expectedText, questionToAssert.HY_Question);
		}

		VoteExamSurveyQuestion CreateQuestion(string questionText, ZShort questionOrder)
		{
			return CreateQuestion(questionText, questionOrder, false);
		}

		protected virtual VoteExamSurveyQuestion CreateQuestion(string questionText, ZShort questionOrder, bool isHeader)
		{
			VoteExamSurveyQuestion result = Factory.New<VoteExamSurveyQuestion>();
			result.HY_G0 = Collection.campaign.PK;
			result.HY_Question = questionText;
			result[Collection.SchemaOrderColumn] = questionOrder;
			result.HY_AnswerType = (isHeader) ? VoteExamSurveyAnswerTypeList.Codes.Header : DefaultAnswerTypeForNewQuestion;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			activeBizOInvariant = ActiveBusinessObjectCollection.EnableInvariant();
		}

		protected override void TearDown()
		{
			activeBizOInvariant.Dispose();
			base.TearDown();
		}

		IDisposable activeBizOInvariant;

		#endregion
	}
}
