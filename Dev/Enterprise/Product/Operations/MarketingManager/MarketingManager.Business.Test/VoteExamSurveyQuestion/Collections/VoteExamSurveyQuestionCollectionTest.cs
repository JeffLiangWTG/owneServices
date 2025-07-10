using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveyQuestionCollection))]
	sealed class VoteExamSurveyQuestionCollectionTest : VoteExamSurveyQuestionSetTestCase<VoteExamSurveyQuestionCollection>
	{
		public void TestRelationshipFilter()
		{
			VoteExamSurveyQuestion includedQuestion1 = CreateQuestion(Campaign, 1, 0);
			VoteExamSurveyQuestion includedQuestion2 = CreateQuestion(Campaign, 2, 0);
			VoteExamSurveyQuestion includedQuestion3 = CreateQuestion(Campaign, 3, 0);
			VoteExamSurveyQuestion otherQuestion1 = CreateQuestion(Campaign, 3, 1);
			VoteExamSurveyQuestion otherQuestion2 = CreateQuestion(Campaign, 3, 2);
			VoteExamSurveyQuestion includedQuestion4 = CreateQuestion(Campaign, 4, 0);
			GlbCompanyCampaign otherCampaign = Factory.New<GlbCompanyCampaign>();
			VoteExamSurveyQuestion otherQuestion3 = CreateQuestion(otherCampaign, 1, 0);

			AssertEquals(4, Campaign.Questions.Count);
			Assert(Campaign.Questions.Contains(includedQuestion1));
			Assert(Campaign.Questions.Contains(includedQuestion2));
			Assert(Campaign.Questions.Contains(includedQuestion3));
			Assert(Campaign.Questions.Contains(includedQuestion4));
		}

		public void TestSetDefaultsForNewChild()
		{
			Campaign.G0_DefaultAnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion newQuestion = Campaign.Questions.AddNew();
			AssertEquals(new ZShort(1), newQuestion.HY_QuestionOrder);
			AssertEquals(VoteExamSurveyAnswerTypeList.Codes.Header, newQuestion.HY_AnswerType);

			Campaign.G0_DefaultAnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			newQuestion = Campaign.Questions.AddNew();
			AssertEquals(new ZShort(1), newQuestion.HY_QuestionOrder);
			AssertEquals(VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, newQuestion.HY_AnswerType);

			Campaign.G0_DefaultAnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			newQuestion = Campaign.Questions.AddNew();
			AssertEquals(new ZShort(2), newQuestion.HY_QuestionOrder);
			AssertEquals(VoteExamSurveyAnswerTypeList.Codes.NumericScale, newQuestion.HY_AnswerType);
		}

		public void TestSetDefaultsForNewChild_IsOptional()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			Assert("Not a survey campaign", !campaign.Questions.AddNew().HY_IsOptional);

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Assert(campaign.Questions.AddNew().HY_IsOptional);
		}

		public void TestAddNewVoteQuestionHeader()
		{
			VoteExamSurveyQuestion voteQuestionHeader = Collection.AddNewVoteQuestionHeader();
			AssertEquals(VoteExamSurveyAnswerTypeList.Codes.RankedVote, voteQuestionHeader.HY_AnswerType);
			AssertEquals("Please nominate your votes", voteQuestionHeader.HY_Question);
			AssertEquals(new ZByte(10), voteQuestionHeader.HY_Min);
			AssertEquals(new ZByte(10), voteQuestionHeader.HY_Max);
		}

		public void TestAdd_ShouldReportErrorIfQuestionIsAddedWhenVoteHeaderAlreadyExist()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			AssertEquals("Pre-condition", 1, Campaign.Questions.Count);

			Campaign.Questions.Add(Factory.New<VoteExamSurveyQuestion>());
			AssertEquals("VoteExamSurveyQuestion should not be added more than once for Voting Campaign", ErrorReporter.LastMessageReported);
			AssertEquals("VoteExamSurveyQuestionCollection_VoteQuestionHeaderAddedMoreThanOnce", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestAddCancelUncommittedRowShouldNotAccidentallyDeleteSubQuestions()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion question = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion subQuestion1 = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion subQuestion2 = question.SubQuestions.AddNew();

			((IBindingList)Campaign.Questions.InactiveQuestions).AddNew();
			((ICancelAddNew)Campaign.Questions.InactiveQuestions).CancelNew(0);
			AssertEquals("Should not be accidentally deleted", 2, question.SubQuestions.Count);
		}

		#region Old Reordering Tests

		public void TestOnCountChanged()
		{
			VoteExamSurveyQuestion questionA = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion questionB = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion questionC = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion questionD = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion questionE = Campaign.Questions.AddNew();
			AssertQuestionOrder(Campaign.Questions, questionA, questionB, questionC, questionD, questionE);

			Campaign.Questions.Delete(questionC);
			AssertQuestionOrder(Campaign.Questions, questionA, questionB, questionD, questionE);

			Campaign.Questions.Delete(questionB);
			AssertQuestionOrder(Campaign.Questions, questionA, questionD, questionE);
		}

		public void TestOnCountChanged_WithQuestionHeaders()
		{
			VoteExamSurveyQuestion header1 = Campaign.Questions.AddNew();
			header1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion questionA = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion questionB = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion header2 = Campaign.Questions.AddNew();
			header2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion questionC = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion questionD = Campaign.Questions.AddNew();
			VoteExamSurveyQuestion questionE = Campaign.Questions.AddNew();
			AssertQuestionOrder(Campaign.Questions, header1, questionA, questionB, header2, questionC, questionD, questionE);

			Campaign.Questions.Delete(questionA);
			AssertQuestionOrder(Campaign.Questions, header1, questionB, header2, questionC, questionD, questionE);

			Campaign.Questions.Delete(header1);
			AssertQuestionOrder(Campaign.Questions, questionB, header2, questionC, questionD, questionE);

			Campaign.Questions.Delete(questionD);
			AssertQuestionOrder(Campaign.Questions, questionB, header2, questionC, questionE);

			Campaign.Questions.Delete(header2);
			AssertQuestionOrder(Campaign.Questions, questionB, questionC, questionE);
		}

		public void TestReOrderQuestions()
		{
			VoteExamSurveyQuestion questionA = CreateQuestion(Campaign, 1, 0);
			VoteExamSurveyQuestion questionB = CreateQuestion(Campaign, 2, 0);
			VoteExamSurveyQuestion questionC = CreateQuestion(Campaign, 3, 0);
			VoteExamSurveyQuestion questionD = CreateQuestion(Campaign, 4, 0);
			VoteExamSurveyQuestion questionE = CreateQuestion(Campaign, 5, 0);
			AssertQuestionOrder(Campaign.Questions, 1, 2, 3, 4, 5);

			questionE.ActualOrder = 2;
			AssertQuestionOrder(Campaign.Questions, 1, 2, 3, 4, 5);
			AssertEquals(questionA, Campaign.Questions[0]);
			AssertEquals(questionE, Campaign.Questions[1]);
			AssertEquals(questionB, Campaign.Questions[2]);
			AssertEquals(questionC, Campaign.Questions[3]);
			AssertEquals(questionD, Campaign.Questions[4]);

			questionB.ActualOrder = 19;
			AssertQuestionOrder(Campaign.Questions, 1, 2, 3, 4, 5);
			AssertEquals(questionA, Campaign.Questions[0]);
			AssertEquals(questionE, Campaign.Questions[1]);
			AssertEquals(questionC, Campaign.Questions[2]);
			AssertEquals(questionD, Campaign.Questions[3]);
			AssertEquals(questionB, Campaign.Questions[4]);
		}

		void AssertQuestionOrder(VoteExamSurveyQuestionCollection collection, params VoteExamSurveyQuestion[] questionsInOrder)
		{
			AssertEquals("Invalid count", questionsInOrder.Length, collection.Count);
			ZShort currentOrder = 1;
			for (int i = 0; i < questionsInOrder.Length; i++)
			{
				AssertEquals(questionsInOrder[i], collection[i]);
				AssertEquals(currentOrder, questionsInOrder[i].HY_QuestionOrder);
				if (!questionsInOrder[i].IsHeader)
				{
					currentOrder++;
				}
			}
		}

		void AssertQuestionOrder(VoteExamSurveyQuestionCollection collection, params ZShort[] questionOrders)
		{
			for (int i = 0; i < collection.Count; i++)
			{
				AssertEquals("Question Ordering is not as expected", questionOrders[i], collection[i].HY_QuestionOrder);
			}
		}

		VoteExamSurveyQuestion CreateQuestion(GlbCompanyCampaign campaign, ZShort questionOrder, ZShort subQuestionOrder)
		{
			VoteExamSurveyQuestion result = Factory.New<VoteExamSurveyQuestion>();
			result.HY_G0 = campaign.PK;
			result.HY_QuestionOrder = questionOrder;
			result.HY_SubQuestionOrder = subQuestionOrder;
			return result;
		}

		#endregion

		#region Implementation

		protected override VoteExamSurveyQuestionSet GetNewQuestionSet(bool? isActiveFilter)
		{
			return new VoteExamSurveyQuestionCollection(Campaign, isActiveFilter);
		}

		protected override VoteExamSurveyQuestion CreateQuestion(string questionText, ZShort questionOrder, bool isHeader)
		{
			VoteExamSurveyQuestion result = base.CreateQuestion(questionText, questionOrder, isHeader);
			if (!isHeader)
			{
				AddNewMultipleChoiceOption(result, questionText + "-A");
				AddNewMultipleChoiceOption(result, questionText + "-B");
				AddNewMultipleChoiceOption(result, questionText + "-C");
			}
			return result;
		}

		void AddNewMultipleChoiceOption(VoteExamSurveyQuestion question, string optionText)
		{
			VoteExamSurveyQuestion option = question.SubQuestions.AddNew();
			option.HY_Question = optionText;
		}

		protected override void AssertQuestionOrder(VoteExamSurveyQuestion questionToAssert, ZShort expectedOrder, string expectedText)
		{
			base.AssertQuestionOrder(questionToAssert, expectedOrder, expectedText);
			if (questionToAssert.IsMultipleChoiceQuestion)
			{
				AssertEquals("SubQuestions should be moved together with the parent question", 3, questionToAssert.SubQuestions.Count);
				AssertEquals(expectedText + "-A", questionToAssert.SubQuestions[0].HY_Question);
				AssertEquals(expectedText + "-B", questionToAssert.SubQuestions[1].HY_Question);
				AssertEquals(expectedText + "-C", questionToAssert.SubQuestions[2].HY_Question);
			}
			else
			{
				AssertEquals("SubQuestions should not exist", 0, questionToAssert.SubQuestions.Count);
			}
		}

		protected override VoteExamSurveyQuestionCollection GetCollectionToTest()
		{
			return Campaign.Questions;
		}

		GlbCompanyCampaign Campaign
		{
			get { return campaign ?? (campaign = Factory.New<GlbCompanyCampaign>()); }
		}

		GlbCompanyCampaign campaign;

		#endregion

		protected override string DefaultAnswerTypeForNewQuestion
		{
			get { return VoteExamSurveyAnswerTypeList.Codes.MultipleChoice; }
		}

		protected override SchemaShortColumn ExpectedSchemaOrderColumn
		{
			get { return VoteExamSurveyQuestionSchema.HY_QuestionOrder; }
		}
	}
}
