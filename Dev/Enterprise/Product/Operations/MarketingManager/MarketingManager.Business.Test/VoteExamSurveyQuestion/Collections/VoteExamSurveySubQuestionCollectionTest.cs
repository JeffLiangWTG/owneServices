using System.ComponentModel;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveySubQuestionCollection))]
	sealed class VoteExamSurveySubQuestionCollectionTest : VoteExamSurveyQuestionSetTestCase<VoteExamSurveySubQuestionCollection>
	{
		public void TestAllow_ParentQuestion()
		{
			ParentQuestion.HY_Question = "";
			AssertEquals(false, ((IBindingList)ParentQuestion.SubQuestions).AllowNew);

			ParentQuestion.HY_Question = "zzz";
			AssertEquals(true, ((IBindingList)ParentQuestion.SubQuestions).AllowNew);

			var collection = ParentQuestion.SubQuestions;
			AssertEquals(true, ((IBindingList)collection).AllowNew);
			parentQuestion.Delete();
			AssertEquals(true, collection.Master.IsDeleted);
			AssertEquals(false, ((IBindingList)collection).AllowNew);
		}

		public void TestRelationshipFilter_MasterIsASubQuestion()
		{
			VoteExamSurveyQuestion subQuestion = ParentQuestion.SubQuestions.AddNew();
			subQuestion.HY_QuestionOrder = 7;
			subQuestion.HY_SubQuestionOrder = 1;
			CreateSubQuestion(subQuestion, 1);
			CreateSubQuestion(subQuestion, 2);
			CreateSubQuestion(subQuestion, 3);
			CreateSubQuestion(subQuestion, 4);
			AssertNull("SubQuestion should not have Sub questions", subQuestion.SubQuestions);
		}

		public void TestCollectionRefreshedWhenParentOrderChanged()
		{
			VoteExamSurveyQuestion subQuestion1 = ParentQuestion.SubQuestions.AddNew();
			VoteExamSurveyQuestion subQuestion2 = ParentQuestion.SubQuestions.AddNew();
			AssertEquals("Precondition", 2, ParentQuestion.SubQuestions.Count);

			ParentQuestion.HY_QuestionOrder++;
			AssertEquals("Should be refreshed", 2, ParentQuestion.SubQuestions.Count);
			AssertEquals(ParentQuestion.HY_QuestionOrder, ParentQuestion.SubQuestions[0].HY_QuestionOrder);
			AssertEquals(ParentQuestion.HY_QuestionOrder, ParentQuestion.SubQuestions[1].HY_QuestionOrder);
		}

		public void TestMatchesFilter_Deleted()
		{
			ZShort questionOrder = ParentQuestion.HY_QuestionOrder;
			VoteExamSurveySubQuestionCollection subQuestionCollection = ParentQuestion.SubQuestions;
			ParentQuestion.Delete();

			VoteExamSurveyQuestion subQuestion1 = CreateSubQuestion(Campaign, questionOrder, 1);
			VoteExamSurveyQuestion subQuestion2 = CreateSubQuestion(Campaign, questionOrder, 2);
			AssertEquals(0, subQuestionCollection.Count);

			VoteExamSurveyQuestion question = Campaign.Questions.AddNew();
			subQuestion1.HY_QuestionOrder = question.HY_QuestionOrder;
			subQuestion1.HY_QuestionOrder = question.HY_QuestionOrder;
			AssertEquals(2, question.SubQuestions.Count);

			subQuestion1.Delete();
			AssertEquals(1, question.SubQuestions.Count);
			AssertEquals(subQuestion2.PK, question.SubQuestions[0].PK);
		}

		public void TestMatchesFilter_QuestionOrderAndSubOrder()
		{
			VoteExamSurveyQuestion question1 = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion question2 = Campaign.Questions.AddNew();

			VoteExamSurveyQuestion subQuestionA = CreateSubQuestion(question1, 1);
			VoteExamSurveyQuestion subQuestionB = CreateSubQuestion(question1, 2);
			VoteExamSurveyQuestion subQuestionC = CreateSubQuestion(question2, 1);
			VoteExamSurveyQuestion subQuestionD = CreateSubQuestion(question2, 2);
			AssertEquals(2, question1.SubQuestions.Count);
			AssertCollectionContains(subQuestionA, question1.SubQuestions);
			AssertCollectionContains(subQuestionB, question1.SubQuestions);

			AssertEquals(2, question2.SubQuestions.Count);
			AssertCollectionContains(subQuestionC, question2.SubQuestions);
			AssertCollectionContains(subQuestionD, question2.SubQuestions);

			subQuestionA.HY_SubQuestionOrder = 0;
			AssertEquals(1, question1.SubQuestions.Count);
			AssertCollectionContains(subQuestionB, question1.SubQuestions);

			subQuestionD.HY_SubQuestionOrder = -1;
			AssertEquals(2, question2.SubQuestions.Count);
			AssertCollectionContains(subQuestionC, question2.SubQuestions);
			AssertCollectionContains(subQuestionD, question2.SubQuestions);
		}

		public void TestCollectionState()
		{
			object[] collectionState = (object[])Collection.GetType().InvokeMember("GetCollectionState", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Collection, null);
			AssertEquals(1, collectionState.Length);
			AssertEquals(ParentQuestion, collectionState[0]);
		}

		public void TestShouldDeleteQuestionTreeIfCampaignIsDeleted()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			VoteExamSurveyQuestion subQuestion1 = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion subQuestion2 = question.SubQuestions.AddNew();
			campaign.Delete();

			Assert(question.IsDeleted);
			Assert(subQuestion1.IsDeleted);
			Assert(subQuestion2.IsDeleted);
		}

		public void TestSetDefaultsForNewChild()
		{
			VoteExamSurveyQuestion subQuestion1 = ParentQuestion.SubQuestions.AddNew();
			AssertEquals(Campaign, subQuestion1.Campaign);
			AssertEquals(ParentQuestion.HY_QuestionOrder, subQuestion1.HY_QuestionOrder);
			AssertEquals(new ZShort(1), subQuestion1.HY_SubQuestionOrder);

			VoteExamSurveyQuestion subQuestion2 = ParentQuestion.SubQuestions.AddNew();
			AssertEquals(Campaign, subQuestion2.Campaign);
			AssertEquals(ParentQuestion.HY_QuestionOrder, subQuestion2.HY_QuestionOrder);
			AssertEquals(new ZShort(2), subQuestion2.HY_SubQuestionOrder);
		}

		public void TestSetDefaultsForNewChild_VotingItem()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			VoteExamSurveyQuestion votingItem = Campaign.VoteHeader.SubQuestions.AddNew();
			AssertEquals(Campaign, votingItem.Campaign);
			AssertEquals(Campaign.VoteHeader.HY_QuestionOrder, votingItem.HY_QuestionOrder);
			AssertEquals(new ZShort(1), votingItem.HY_SubQuestionOrder);
			AssertEquals(VoteExamSurveyAnswerTypeList.Codes.VotingItem, votingItem.HY_AnswerType);
		}

		public void TestSetDefaultsForNewChild_MultipleChoice()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion question = Campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion subQuestion = question.SubQuestions.AddNew();
			AssertEquals(Campaign, subQuestion.Campaign);
			AssertEquals(question.HY_QuestionOrder, subQuestion.HY_QuestionOrder);
			AssertEquals(new ZShort(1), subQuestion.HY_SubQuestionOrder);
			AssertEquals(VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption, subQuestion.HY_AnswerType);
		}

		#region Old ReOrdering Tests

		public void TestOnCountChanged()
		{
			VoteExamSurveyQuestion question = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion subQuestionA = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion subQuestionB = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion subQuestionC = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion subQuestionD = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion subQuestionE = question.SubQuestions.AddNew();
			AssertSubQuestionOrder(question.SubQuestions, subQuestionA, subQuestionB, subQuestionC, subQuestionD, subQuestionE);

			question.SubQuestions.Delete(subQuestionC);
			AssertSubQuestionOrder(question.SubQuestions, subQuestionA, subQuestionB, subQuestionD, subQuestionE);

			question.SubQuestions.Delete(subQuestionB);
			AssertSubQuestionOrder(question.SubQuestions, subQuestionA, subQuestionD, subQuestionE);
		}

		public void TestOnCountChanged_WithQuestionHeaders()
		{
			VoteExamSurveyQuestion question = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion header1 = question.SubQuestions.AddNew();
			header1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion questionA = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion questionB = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion header2 = question.SubQuestions.AddNew();
			header2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion questionC = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion questionD = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion questionE = question.SubQuestions.AddNew();
			AssertSubQuestionOrder(question.SubQuestions, header1, questionA, questionB, header2, questionC, questionD, questionE);

			question.SubQuestions.Delete(questionA);
			AssertSubQuestionOrder(question.SubQuestions, header1, questionB, header2, questionC, questionD, questionE);

			question.SubQuestions.Delete(header1);
			AssertSubQuestionOrder(question.SubQuestions, questionB, header2, questionC, questionD, questionE);

			question.SubQuestions.Delete(questionD);
			AssertSubQuestionOrder(question.SubQuestions, questionB, header2, questionC, questionE);

			question.SubQuestions.Delete(header2);
			AssertSubQuestionOrder(question.SubQuestions, questionB, questionC, questionE);
		}

		public void TestReOrderQuestionSet()
		{
			VoteExamSurveyQuestion subQuestionA = CreateSubQuestion(ParentQuestion, 1);
			VoteExamSurveyQuestion subQuestionB = CreateSubQuestion(ParentQuestion, 2);
			VoteExamSurveyQuestion subQuestionC = CreateSubQuestion(ParentQuestion, 3);
			VoteExamSurveyQuestion subQuestionD = CreateSubQuestion(ParentQuestion, 4);
			VoteExamSurveyQuestion subQuestionE = CreateSubQuestion(ParentQuestion, 5);
			AssertQuestionsOrdering(ParentQuestion.SubQuestions, 1, 2, 3, 4, 5);

			subQuestionE.ActualOrder = 2;
			AssertQuestionsOrdering(ParentQuestion.SubQuestions, 1, 2, 3, 4, 5);
			AssertEquals(subQuestionA, ParentQuestion.SubQuestions[0]);
			AssertEquals(subQuestionE, ParentQuestion.SubQuestions[1]);
			AssertEquals(subQuestionB, ParentQuestion.SubQuestions[2]);
			AssertEquals(subQuestionC, ParentQuestion.SubQuestions[3]);
			AssertEquals(subQuestionD, ParentQuestion.SubQuestions[4]);

			subQuestionA.ActualOrder = 999;
			AssertQuestionsOrdering(ParentQuestion.SubQuestions, 1, 2, 3, 4, 5);
			AssertEquals(subQuestionE, ParentQuestion.SubQuestions[0]);
			AssertEquals(subQuestionB, ParentQuestion.SubQuestions[1]);
			AssertEquals(subQuestionC, ParentQuestion.SubQuestions[2]);
			AssertEquals(subQuestionD, ParentQuestion.SubQuestions[3]);
			AssertEquals(subQuestionA, ParentQuestion.SubQuestions[4]);
		}

		void AssertQuestionsOrdering(VoteExamSurveySubQuestionCollection collection, params ZShort[] subQuestionOrders)
		{
			for (int i = 0; i < collection.Count; i++)
			{
				AssertEquals("Question Ordering is not as expected", subQuestionOrders[i], collection[i].HY_SubQuestionOrder);
			}
		}

		void AssertSubQuestionOrder(VoteExamSurveySubQuestionCollection collection, params VoteExamSurveyQuestion[] subQuestionsInOrder)
		{
			AssertEquals("Invalid count", subQuestionsInOrder.Length, collection.Count);
			ZShort currentOrder = 1;
			for (int i = 0; i < subQuestionsInOrder.Length; i++)
			{
				AssertEquals(subQuestionsInOrder[i], collection[i]);
				AssertEquals(currentOrder, subQuestionsInOrder[i].HY_SubQuestionOrder);
				if (!subQuestionsInOrder[i].IsHeader)
				{
					currentOrder++;
				}
			}
		}

		#endregion

		#region Implementation

		protected override VoteExamSurveyQuestionSet GetNewQuestionSet(bool? isActiveFilter)
		{
			ParentQuestion.HY_Question = "hi";
			return new VoteExamSurveySubQuestionCollection(ParentQuestion, isActiveFilter);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			VoteExamSurveyQuestion result = (VoteExamSurveyQuestion)base.GetNewElementToAddToTheCollection();
			result.HY_QuestionOrder = ParentQuestion.HY_QuestionOrder;
			result.HY_SubQuestionOrder = 1;
			return result;
		}

		protected override VoteExamSurveyQuestion CreateQuestion(string questionText, ZShort questionOrder, bool isHeader)
		{
			VoteExamSurveyQuestion result = base.CreateQuestion(questionText, questionOrder, isHeader);
			result.HY_QuestionOrder = ParentQuestion.HY_QuestionOrder;
			return result;
		}

		VoteExamSurveyQuestion CreateSubQuestion(VoteExamSurveyQuestion parentQuestion, ZShort subQuestionOrder)
		{
			return CreateSubQuestion(parentQuestion.Campaign, parentQuestion.HY_QuestionOrder, subQuestionOrder);
		}

		VoteExamSurveyQuestion CreateSubQuestion(GlbCompanyCampaign campaign, ZShort questionOrder, ZShort subQuestionOrder)
		{
			VoteExamSurveyQuestion result = Factory.New<VoteExamSurveyQuestion>();
			result.HY_G0 = campaign.PK;
			result.HY_QuestionOrder = questionOrder;
			result.HY_SubQuestionOrder = subQuestionOrder;
			return result;
		}

		protected override VoteExamSurveySubQuestionCollection GetCollectionToTest()
		{
			return ParentQuestion.SubQuestions;
		}

		VoteExamSurveyQuestion ParentQuestion
		{
			get { return parentQuestion ?? (parentQuestion = Campaign.Questions.AddNew()); }
		}

		GlbCompanyCampaign Campaign
		{
			get { return campaign ?? (campaign = Factory.New<GlbCompanyCampaign>()); }
		}

		GlbCompanyCampaign campaign;
		VoteExamSurveyQuestion parentQuestion;

		#endregion

		protected override string DefaultAnswerTypeForNewQuestion
		{
			get { return VoteExamSurveyAnswerTypeList.Codes.VotingItem; }
		}

		protected override SchemaShortColumn ExpectedSchemaOrderColumn
		{
			get { return VoteExamSurveyQuestionSchema.HY_SubQuestionOrder; }
		}
	}
}
