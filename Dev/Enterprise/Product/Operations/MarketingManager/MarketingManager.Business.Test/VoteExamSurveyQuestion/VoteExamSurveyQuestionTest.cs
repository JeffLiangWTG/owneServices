using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveyQuestion))]
	sealed class VoteExamSurveyQuestionTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			VoteExamSurveyQuestion question = (VoteExamSurveyQuestion)base.GetNewBusinessObject();
			question.HY_G0 = Question.Campaign.PK;
			return question;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Question.Campaign.Questions.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<VoteExamSurveyQuestionForTest>();
		}

		class VoteExamSurveyQuestionForTest : VoteExamSurveyQuestion
		{
			public VoteExamSurveyQuestionForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool ForceAllowDeleteForTest => true;
		}

		#endregion

		public void TestTranslatableDataFieldAttributeHasColumn_HY_Question()
		{
			var property = typeof(VoteExamSurveyQuestion).GetProperty(VoteExamSurveyQuestionSchema.HY_Question.Name);
			AssertNotNull(property);

			var attribute = property.GetCustomAttribute(typeof(TranslatableDataFieldAttribute)) as TranslatableDataFieldAttribute;
			AssertNotNull(attribute);

			AssertEquals(VoteExamSurveyQuestionSchema.HY_Question.Name, attribute.ContextColumnName);
		}

		public void TestHY_Question_Translatable()
		{
			var bizO = Factory.New<VoteExamSurveyQuestion>();
			bizO.HY_Question = "Boom";
			string resKey = bizO.HY_QuestionInfo.CustomizableDataResourceStrings.GetMultilingualString(bizO, "Boom").ResourceKey;
			AssertEquals("Boom", bizO.HY_QuestionMultilingual);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Russian))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "222"));
				AssertEquals("222", bizO.HY_QuestionMultilingual);
				AssertEquals("222", bizO.HY_QuestionLocalized);
			}
		}

		public void TestDefaultValues()
		{
			VoteExamSurveyQuestion question = Factory.New<VoteExamSurveyQuestion>();
			Assert(question.HY_IsActive);
			Assert(question.HY_IsRandomisable);
		}

		#region IsActiveForBinding

		public void TestIsActiveForBinding_ReOrderQuestionsCorrectly()
		{
			#region Creating Test Data

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			question1.HY_Question = "Q1";
			VoteExamSurveyQuestion subQuestion1a = question1.SubQuestions.AddNew();
			subQuestion1a.HY_Question = "1a";
			VoteExamSurveyQuestion subQuestion1b = question1.SubQuestions.AddNew();
			subQuestion1b.HY_Question = "1b";

			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Percentage;

			VoteExamSurveyQuestion header1 = campaign.Questions.AddNew();
			header1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;

			VoteExamSurveyQuestion question3 = campaign.Questions.AddNew();
			question3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;

			VoteExamSurveyQuestion question4 = campaign.Questions.AddNew();
			question4.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion subQuestion4a = question4.SubQuestions.AddNew();
			subQuestion4a.HY_Question = "4a";
			VoteExamSurveyQuestion subQuestion4b = question4.SubQuestions.AddNew();
			subQuestion4b.HY_Question = "4b";
			VoteExamSurveyQuestion subQuestion4c = question4.SubQuestions.AddNew();
			subQuestion4c.HY_Question = "4c";

			AssertEquals("Precondition", 5, campaign.Questions.Count);
			AssertQuestionOrderIncludingHeaders(campaign.Questions, question1, question2, header1, question3, question4);
			AssertSubQuestions(question1, subQuestion1a, subQuestion1b);
			AssertSubQuestions(question4, subQuestion4a, subQuestion4b, subQuestion4c);

			#endregion

			question1.IsActiveForBinding = false;
			AssertEquals(4, campaign.Questions.Count);
			AssertQuestionOrderIncludingHeaders(campaign.Questions, question2, header1, question3, question4);
			AssertEquals(1, campaign.InactiveQuestions.Count);
			AssertCollectionContains(question1, campaign.InactiveQuestions);
			AssertSubQuestions(question1, subQuestion1a, subQuestion1b);
			AssertEquals(new ZShort(short.MinValue + 1), question1.HY_QuestionOrder);

			header1.IsActiveForBinding = false;
			AssertEquals(3, campaign.Questions.Count);
			AssertQuestionOrderIncludingHeaders(campaign.Questions, question2, question3, question4);
			AssertEquals(2, campaign.InactiveQuestions.Count);
			AssertCollectionContains(header1, campaign.InactiveQuestions);
			AssertEquals(new ZShort(short.MinValue + 2), header1.HY_QuestionOrder);

			question2.IsActiveForBinding = false;
			AssertEquals(2, campaign.Questions.Count);
			AssertQuestionOrderIncludingHeaders(campaign.Questions, question3, question4);
			AssertEquals(3, campaign.InactiveQuestions.Count);
			AssertCollectionContains(question2, campaign.InactiveQuestions);
			AssertEquals(new ZShort(short.MinValue + 3), question2.HY_QuestionOrder);

			question1.IsActiveForBinding = true;
			AssertEquals(3, campaign.Questions.Count);
			AssertQuestionOrderIncludingHeaders(campaign.Questions, question3, question4, question1);
			AssertEquals(2, campaign.InactiveQuestions.Count);
			AssertEquals(new ZShort(3), question1.HY_QuestionOrder);
			AssertSubQuestions(question1, subQuestion1a, subQuestion1b);
			AssertSubQuestions(question4, subQuestion4a, subQuestion4b, subQuestion4c);

			question4.IsActiveForBinding = false;
			AssertEquals(2, campaign.Questions.Count);
			AssertQuestionOrderIncludingHeaders(campaign.Questions, question3, question1);
			AssertEquals(3, campaign.InactiveQuestions.Count);
			AssertCollectionContains(question4, campaign.InactiveQuestions);
			AssertEquals(new ZShort(short.MinValue + 4), question4.HY_QuestionOrder);
			AssertSubQuestions(question1, subQuestion1a, subQuestion1b);
			AssertSubQuestions(question4, subQuestion4a, subQuestion4b, subQuestion4c);
		}

		public void TestIsActiveForBinding_SubQuestionsAreNotRemoved()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion option1 = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion option2 = question.SubQuestions.AddNew();
			AssertEquals(2, question.SubQuestions.Count);

			question.IsActiveForBinding = false;
			AssertEquals(2, question.SubQuestions.Count);
			AssertCollectionContains(option1, question.SubQuestions);
			AssertCollectionContains(option2, question.SubQuestions);

			question.IsActiveForBinding = true;
			AssertEquals(2, question.SubQuestions.Count);
			AssertCollectionContains(option1, question.SubQuestions);
			AssertCollectionContains(option2, question.SubQuestions);
		}

		public void TestIsActiveForBindingInfo()
		{
			VoteExamSurveyQuestion question = Factory.New<VoteExamSurveyQuestion>();
			AssertEquals(question.HY_IsActiveInfo, ((ZWrappedPropertyInfo)question.IsActiveForBindingInfo).InnerInfo);
		}

		public void TestIsActiveForBindingReadOnly()
		{
			VoteExamSurveyQuestion question = Factory.NewWithValidTestData<VoteExamSurveyQuestion>();
			Assert("New record, should not allow IsActive toggling", question.IsActiveForBindingInfo.ReadOnly);

			Factory.Save();
			Assert("Show now allow IsActive toggling", !question.IsActiveForBindingInfo.ReadOnly);
		}

		void AssertQuestionOrderIncludingHeaders(VoteExamSurveyQuestionCollection collection, params VoteExamSurveyQuestion[] questionsInOrder)
		{
			ZShort expectedOrder = 1;
			AssertEquals("Invalid count", questionsInOrder.Length, collection.Count);
			for (int i = 0; i < questionsInOrder.Length; i++)
			{
				AssertEquals(questionsInOrder[i], collection[i]);
				AssertEquals(expectedOrder, questionsInOrder[i].HY_QuestionOrder);

				if (!questionsInOrder[i].IsHeader)
				{
					expectedOrder++;
				}
			}
		}

		#endregion

		#region Question / Sub Question ordering

		public void TestActualOrder()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_QuestionOrder = 45;
			question.HY_SubQuestionOrder = 0;
			AssertEquals((ZShort)45, question.ActualOrder);
			AssertEquals("45", question.ActualOrderForBinding);

			VoteExamSurveyQuestion subQuestion = question.SubQuestions.AddNew();
			subQuestion.HY_QuestionOrder = 45;
			subQuestion.HY_SubQuestionOrder = 111;
			AssertEquals((ZShort)111, subQuestion.ActualOrder);
			AssertEquals("111", subQuestion.ActualOrderForBinding);

			VoteExamSurveyQuestion inactiveQuestion = campaign.Questions.AddNew();
			inactiveQuestion.HY_QuestionOrder = 50;
			inactiveQuestion.IsActiveForBinding = false;
			AssertEquals("", inactiveQuestion.ActualOrderForBinding);
		}

		public void TestActualOrderInfo()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			Assert(question.ActualOrderInfo is ZWrappedPropertyInfo);
			AssertEquals(question.HY_QuestionOrderInfo, ((ZWrappedPropertyInfo)question.ActualOrderInfo).InnerInfo);

			VoteExamSurveyQuestion subQuestion = question.SubQuestions.AddNew();
			Assert(subQuestion.ActualOrderInfo is ZWrappedPropertyInfo);
			AssertEquals(subQuestion.HY_SubQuestionOrderInfo, ((ZWrappedPropertyInfo)subQuestion.ActualOrderInfo).InnerInfo);
		}

		public void TestReOrderQuestion()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion questionA = campaign.Questions.AddNew();
			questionA.HY_Question = "Question A";
			VoteExamSurveyQuestion questionB = campaign.Questions.AddNew();
			questionB.HY_Question = "Question B";
			VoteExamSurveyQuestion questionC = campaign.Questions.AddNew();
			questionC.HY_Question = "Question C";
			VoteExamSurveyQuestion questionD = campaign.Questions.AddNew();
			questionD.HY_Question = "Question D";
			VoteExamSurveyQuestion questionE = campaign.Questions.AddNew();
			questionE.HY_Question = "Question E";
			VoteExamSurveyQuestion questionF = campaign.Questions.AddNew();
			questionF.HY_Question = "Question F";

			questionC.ActualOrder = 80;
			AssertQuestionOrder(campaign.Questions, questionA, questionB, questionD, questionE, questionF, questionC);

			questionF.ActualOrder = 0;
			AssertQuestionOrder(campaign.Questions, questionF, questionA, questionB, questionD, questionE, questionC);

			questionA.ActualOrder = 4;
			AssertQuestionOrder(campaign.Questions, questionF, questionB, questionD, questionA, questionE, questionC);
		}

		[ExpectNoExceptions]
		public void TestReOrderQuestion_NotWhenDeletingAllQuestions()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion questionA = campaign.Questions.AddNew();
			questionA.HY_Question = "Question A";
			questionA.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			questionA.SubQuestions.AddNew();
			questionA.SubQuestions.AddNew();

			VoteExamSurveyQuestion questionB = campaign.Questions.AddNew();
			questionB.HY_Question = "Question B";
			questionB.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			questionB.SubQuestions.AddNew();
			questionB.SubQuestions.AddNew();

			VoteExamSurveyQuestion questionC = campaign.Questions.AddNew();
			questionC.HY_Question = "Question C";
			questionC.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			questionC.SubQuestions.AddNew();
			questionC.SubQuestions.AddNew();

			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedCampaign = loadFactory.Load<GlbCompanyCampaign>(campaign.PK);
			loadedCampaign.Delete();
			loadFactory.Save();
		}

		public void TestReOrderQuestion_WithSubQuestions()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion questionA = campaign.Questions.AddNew();
			questionA.HY_Question = "Question A";
			VoteExamSurveyQuestion optionA1 = questionA.SubQuestions.AddNew();
			optionA1.HY_Question = "Option A1";
			VoteExamSurveyQuestion optionA2 = questionA.SubQuestions.AddNew();
			optionA2.HY_Question = "Option A2";
			VoteExamSurveyQuestion questionB = campaign.Questions.AddNew();
			questionB.HY_Question = "Question B";
			VoteExamSurveyQuestion optionB1 = questionB.SubQuestions.AddNew();
			optionB1.HY_Question = "Option B1";
			VoteExamSurveyQuestion optionB2 = questionB.SubQuestions.AddNew();
			optionB2.HY_Question = "Option B2";
			VoteExamSurveyQuestion optionB3 = questionB.SubQuestions.AddNew();
			optionB3.HY_Question = "Option B3";
			VoteExamSurveyQuestion questionC = campaign.Questions.AddNew();
			questionC.HY_Question = "Question C";
			VoteExamSurveyQuestion optionC1 = questionC.SubQuestions.AddNew();
			optionC1.HY_Question = "Option C1";
			VoteExamSurveyQuestion optionC2 = questionC.SubQuestions.AddNew();
			optionC2.HY_Question = "Option C2";

			AssertQuestionOrder(campaign.Questions, questionA, questionB, questionC);

			questionC.ActualOrder = 1;
			AssertQuestionOrder(campaign.Questions, questionC, questionA, questionB);
			AssertSubQuestions(questionA, optionA1, optionA2);
			AssertSubQuestions(questionB, optionB1, optionB2, optionB3);
			AssertSubQuestions(questionC, optionC1, optionC2);

			questionA.ActualOrder = 3;
			AssertQuestionOrder(campaign.Questions, questionC, questionB, questionA);
			AssertSubQuestions(questionA, optionA1, optionA2);
			AssertSubQuestions(questionB, optionB1, optionB2, optionB3);
			AssertSubQuestions(questionC, optionC1, optionC2);

			questionB.ActualOrder = 1;
			AssertQuestionOrder(campaign.Questions, questionB, questionC, questionA);
			AssertSubQuestions(questionA, optionA1, optionA2);
			AssertSubQuestions(questionB, optionB1, optionB2, optionB3);
			AssertSubQuestions(questionC, optionC1, optionC2);
		}

		public void TestReOrderQuestion_WithUnloadedSubQuestions()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion questionA = campaign.Questions.AddNew();
			questionA.HY_Question = "Question A";
			VoteExamSurveyQuestion optionA1 = questionA.SubQuestions.AddNew();
			optionA1.HY_Question = "Option A1";
			VoteExamSurveyQuestion optionA2 = questionA.SubQuestions.AddNew();
			optionA2.HY_Question = "Option A2";
			VoteExamSurveyQuestion questionB = campaign.Questions.AddNew();
			questionB.HY_Question = "Question B";
			VoteExamSurveyQuestion optionB1 = questionB.SubQuestions.AddNew();
			optionB1.HY_Question = "Option B1";
			VoteExamSurveyQuestion optionB2 = questionB.SubQuestions.AddNew();
			optionB2.HY_Question = "Option B2";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbCompanyCampaign campaignInNewFactory = newFactory.Load<GlbCompanyCampaign>(campaign.PK);
			VoteExamSurveyQuestion questionAInNewFactory = (VoteExamSurveyQuestion)campaignInNewFactory.Questions.FindByPK(questionA.PK);
			VoteExamSurveyQuestion questionBInNewFactory = (VoteExamSurveyQuestion)campaignInNewFactory.Questions.FindByPK(questionB.PK);
			AssertQuestionOrder(campaignInNewFactory.Questions, questionAInNewFactory, questionBInNewFactory);

			questionBInNewFactory.ActualOrder = 1;
			AssertQuestionOrder(campaignInNewFactory.Questions, questionBInNewFactory, questionAInNewFactory);
			AssertSubQuestions(questionAInNewFactory, optionA1.PK, optionA2.PK);
			AssertSubQuestions(questionBInNewFactory, optionB1.PK, optionB2.PK);
		}

		public void TestReOrderSubQuestion()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			VoteExamSurveyQuestion votingItemA = campaign.VoteHeader.SubQuestions.AddNew();
			votingItemA.HY_Question = "Vote A";
			VoteExamSurveyQuestion votingItemB = campaign.VoteHeader.SubQuestions.AddNew();
			votingItemB.HY_Question = "Vote B";
			VoteExamSurveyQuestion votingItemC = campaign.VoteHeader.SubQuestions.AddNew();
			votingItemC.HY_Question = "Vote C";
			VoteExamSurveyQuestion votingItemD = campaign.VoteHeader.SubQuestions.AddNew();
			votingItemD.HY_Question = "Vote D";
			VoteExamSurveyQuestion votingItemE = campaign.VoteHeader.SubQuestions.AddNew();
			votingItemE.HY_Question = "Vote E";
			VoteExamSurveyQuestion votingItemF = campaign.VoteHeader.SubQuestions.AddNew();
			votingItemF.HY_Question = "Vote F";

			votingItemA.HY_QuestionOrder = 1;
			votingItemC.ActualOrder = 6;
			AssertQuestionOrder(campaign.VoteHeader.SubQuestions, votingItemA, votingItemB, votingItemD, votingItemE, votingItemF, votingItemC);

			votingItemF.ActualOrder = 1;
			AssertQuestionOrder(campaign.VoteHeader.SubQuestions, votingItemF, votingItemA, votingItemB, votingItemD, votingItemE, votingItemC);

			votingItemA.ActualOrder = 4;
			AssertQuestionOrder(campaign.VoteHeader.SubQuestions, votingItemF, votingItemB, votingItemD, votingItemA, votingItemE, votingItemC);
		}

		void AssertQuestionOrder(VoteExamSurveyQuestionCollection collection, params VoteExamSurveyQuestion[] questionsInOrder)
		{
			AssertEquals("Invalid count", questionsInOrder.Length, collection.Count);
			for (int i = 0; i < questionsInOrder.Length; i++)
			{
				AssertEquals(questionsInOrder[i], collection[i]);
				AssertEquals((ZShort)i + 1, questionsInOrder[i].HY_QuestionOrder);
			}
		}

		void AssertQuestionOrder(VoteExamSurveySubQuestionCollection collection, params VoteExamSurveyQuestion[] questionsInOrder)
		{
			AssertEquals("Invalid count", questionsInOrder.Length, collection.Count);
			for (int i = 0; i < questionsInOrder.Length; i++)
			{
				AssertEquals(questionsInOrder[i], collection[i]);
				AssertEquals((ZShort)i + 1, questionsInOrder[i].HY_SubQuestionOrder);
			}
		}

		void AssertSubQuestions(VoteExamSurveyQuestion question, params VoteExamSurveyQuestion[] expectedSubQuestionsInOrder)
		{
			VoteExamSurveySubQuestionCollection collection = question.SubQuestions;
			AssertEquals(expectedSubQuestionsInOrder.Length, collection.Count);
			for (int i = 0; i < expectedSubQuestionsInOrder.Length; i++)
			{
				AssertEquals(expectedSubQuestionsInOrder[i], collection[i]);
				AssertEquals((ZShort)i + 1, collection[i].HY_SubQuestionOrder);
			}
		}

		void AssertSubQuestions(VoteExamSurveyQuestion question, params ZGuid[] expectedSubQuestionsPKInOrder)
		{
			AssertEquals(expectedSubQuestionsPKInOrder.Length, question.SubQuestions.Count);
			for (int i = 0; i < expectedSubQuestionsPKInOrder.Length; i++)
			{
				AssertEquals(expectedSubQuestionsPKInOrder[i], question.SubQuestions[i].PK);
				AssertEquals((ZShort)i + 1, question.SubQuestions[i].HY_SubQuestionOrder);
			}
		}

		#endregion

		#region Voting

		public void TestShouldRankVotingNominees()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			Assert(!Question.ShouldRankVotingNominees);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.RankedVote;
			Assert(Question.ShouldRankVotingNominees);

			Question.ShouldRankVotingNominees = false;
			AssertEquals(VoteExamSurveyAnswerTypeList.Codes.UnrankedVote, Question.HY_AnswerType);

			Question.ShouldRankVotingNominees = true;
			AssertEquals(VoteExamSurveyAnswerTypeList.Codes.RankedVote, Question.HY_AnswerType);
		}

		public void TestShouldRankVotingNomineesReadOnly()
		{
			Assert("Pre-condition: not readonly", !Question.ShouldRankVotingNomineesInfo.ReadOnly);

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			Question.HY_G0 = campaign.PK;
			Question.Campaign.CampaignsItemsSent.AddNew();

			AssertEquals("1 vote sent", 1, Question.Campaign.CampaignsItemsSent.Count);
			Assert("Should be readonly", Question.ShouldRankVotingNomineesInfo.ReadOnly);
		}

		public void TestIsVotingItem()
		{
			Assert("Pre-condition", !Question.IsVotingItem);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Assert(Question.IsVotingItem);

			Question.HY_AnswerType = "";
			Assert(!Question.IsVotingItem);

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Assert("Question is not a subquestion", !campaign.VoteHeader.IsVotingItem);

			campaign.VoteHeader.SubQuestions.Add(Question);
			Question.HY_G0 = campaign.PK;
			Question.HY_SubQuestionOrder = 1;
			Assert(Question.IsVotingItem);
		}

		public void TestIsVotingHeader()
		{
			Assert("Pre-condition", !Question.IsVotingHeader);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Assert(!Question.IsVotingHeader);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.UnrankedVote;
			Assert(Question.IsVotingHeader);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.RankedVote;
			Assert(Question.IsVotingHeader);
		}

		#endregion

		#region Survey / Exam

		public void TestIsMultipleChoiceQuestion()
		{
			Assert("Pre-condition", !Question.IsMultipleChoiceQuestion);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Assert(!Question.IsMultipleChoiceQuestion);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			Assert(Question.IsMultipleChoiceQuestion);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			Assert(!Question.IsMultipleChoiceQuestion);
		}

		public void TestMinMaxValueCaption()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			AssertEquals("Selection Count:", Question.MinMaxValueCaption);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			AssertEquals("Number Range:", Question.MinMaxValueCaption);
		}

		public void TestShouldSpecifyMinMaxValue()
		{
			Assert(!Question.ShouldSpecifyMinMaxValue);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			Assert(Question.ShouldSpecifyMinMaxValue);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			Assert(Question.ShouldSpecifyMinMaxValue);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			Assert(!Question.ShouldSpecifyMinMaxValue);
		}

		public void TestIsNonNumericExamAnswerType()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			AssertEquals(false, Question.IsNonNumericExamAnswerType);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Percentage;
			AssertEquals(false, Question.IsNonNumericExamAnswerType);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			AssertEquals(true, Question.IsNonNumericExamAnswerType);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			AssertEquals(true, Question.IsNonNumericExamAnswerType);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			AssertEquals(true, Question.IsNonNumericExamAnswerType);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			AssertEquals(true, Question.IsNonNumericExamAnswerType);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			AssertEquals(true, Question.IsNonNumericExamAnswerType);
		}

		#endregion

		public void TestHY_Min()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			Question.HY_Min = 12;
			AssertHasErrors("Should be validated", Question.HY_MinInfo);
			AssertHasErrors("Should also be validated when HY_Min is set", Question.HY_MaxInfo);

			Question.HY_Min = 5;
			AssertNoErrors("Should be validated", Question.HY_MinInfo);
			AssertNoErrors("Should also be validated when HY_Min is set", Question.HY_MaxInfo);
		}

		public void TestHY_Max()
		{
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			Question.HY_Max = 0;
			AssertHasErrors("Should be validated", Question.HY_MaxInfo);
			AssertHasErrors("Should also be validated when HY_Max is set", Question.HY_MinInfo);

			Question.HY_Max = 10;
			AssertNoErrors("Should be validated", Question.HY_MaxInfo);
			AssertNoErrors("Should also be validated when HY_Max is set", Question.HY_MinInfo);
		}

		public void TestDelete_ExistingQuestion()
		{
			Question.FillWithValidTestData();
			VoteExamSurveySubQuestionCollection subQuestions = Question.SubQuestions;
			VoteExamSurveyQuestion subQuestion1 = subQuestions.AddNew();
			subQuestion1.FillWithValidTestData();
			VoteExamSurveyQuestion subQuestion2 = subQuestions.AddNew();
			subQuestion2.FillWithValidTestData();

			VoteExamSurveyQuestion question2 = Question.Campaign.Questions.AddNew();
			question2.FillWithValidTestData();
			Factory.Save();

			try
			{
				Question.Delete();
				Fail("CannotDeleteException should be thrown");
			}
			catch (CannotDeleteException ex)
			{
				AssertContains("Cannot delete an existing question, mark as inactive instead.", ex.Message);
			}

			Assert(!Question.IsDeleted);
			AssertEquals(2, subQuestions.Count);
			AssertQuestionOrder(Question.Campaign.Questions, Question, question2);
		}

		public void TestDelete_NewQuestion()
		{
			VoteExamSurveyQuestion subQuestion1 = Question.SubQuestions.AddNew();
			VoteExamSurveyQuestion subQuestion2 = Question.SubQuestions.AddNew();
			Question.Delete();
			Assert(Question.IsDeleted);
			Assert(subQuestion1.IsDeleted);
			Assert(subQuestion2.IsDeleted);
		}

		public void TestDelete_SetAsLastQuestion()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			AssertQuestionOrder(campaign.Questions, question1, question2);

			question1.Delete();
			Assert(question1.IsDeleted);
			AssertEquals("Should be shifted up", new ZShort(1), question2.HY_QuestionOrder);
		}

		public void TestIsSubQuestion()
		{
			Assert("Pre-condition", !Question.IsSubQuestion);

			VoteExamSurveyQuestion subQuestion = Question.SubQuestions.AddNew();
			Assert(subQuestion.IsSubQuestion);

			VoteExamSurveyQuestion newQuestion = Factory.New<VoteExamSurveyQuestion>();
			Assert(!newQuestion.IsSubQuestion);

			newQuestion.HY_SubQuestionOrder = 1;
			Assert(newQuestion.IsSubQuestion);

			newQuestion.HY_SubQuestionOrder = -12;
			Assert(newQuestion.IsSubQuestion);

			newQuestion.HY_SubQuestionOrder = 0;
			Assert(!newQuestion.IsSubQuestion);
		}

		public void TestSubQuestions()
		{
			AssertEquals(Question, Question.SubQuestions.Master);
			Assert(Question.IsRegisteredEditableChildObject(Question.SubQuestions));
		}

		public void TestSubQuestions_SortedWhenLazyLoaded()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.FillWithValidTestData();
			VoteExamSurveyQuestion header1 = question.SubQuestions.AddNew();
			header1.FillWithValidTestData();
			header1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion subQuestion1 = question.SubQuestions.AddNew();
			subQuestion1.FillWithValidTestData();
			VoteExamSurveyQuestion subQuestion2 = question.SubQuestions.AddNew();
			subQuestion2.FillWithValidTestData();
			VoteExamSurveyQuestion header2 = question.SubQuestions.AddNew();
			header2.FillWithValidTestData();
			header2.HY_SubQuestionOrder = subQuestion2.HY_SubQuestionOrder;
			header2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			Factory.Save();

			VoteExamSurveyQuestion questionInNewFactory = new BusinessObjectFactory().Load<VoteExamSurveyQuestion>(question.PK);
			AssertEquals(4, questionInNewFactory.SubQuestions.Count);
			AssertEquals(header1.PK, questionInNewFactory.SubQuestions[0].PK);
			AssertEquals(subQuestion1.PK, questionInNewFactory.SubQuestions[1].PK);
			AssertEquals(header2.PK, questionInNewFactory.SubQuestions[2].PK);
			AssertEquals(subQuestion2.PK, questionInNewFactory.SubQuestions[3].PK);
		}

		public void TestSubmittedAnswers()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			Question.HY_G0 = campaign.PK;

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			var answer = Factory.New<VoteExamSurveyAnswer>();
			answer.HZ_HY = Question.PK;
			answer.HZ_G8 = campaignItem1.PK;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			var answer2 = Factory.New<VoteExamSurveyAnswer>();
			answer2.HZ_HY = Question.PK;
			answer2.HZ_G8 = campaignItem2.PK;

			VoteExamSurveySubmittedAnswerCollection submittedAnswers = Question.SubmittedAnswers;

			AssertEquals(2, submittedAnswers.Count);
			AssertEquals(Question, submittedAnswers[0].Question);
			AssertEquals(Question, submittedAnswers[1].Question);
		}

		public void TestHY_IsOptionalInfo()
		{
			Assert(!Question.HY_IsOptionalInfo.ReadOnly);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			Assert(Question.HY_IsOptionalInfo.ReadOnly);
		}

		public void TestCampaign()
		{
			VoteExamSurveyQuestion question = Factory.New<VoteExamSurveyQuestion>();
			AssertNull(question.Campaign);

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			question.HY_G0 = campaign.PK;
			AssertEquals(campaign, question.Campaign);
		}

		[ExpectNoExceptions]
		public void TestHY_QuestionOrderAndHY_SubQuestionOrderWithoutQuestionSet()
		{
			Question.HY_QuestionOrder = 4;
			Question.HY_SubQuestionOrder = 45;
		}

		public void TestHtmlEncodedQuestionTextForWeb()
		{
			Question.HY_Question = @"Question		Html Encoded Text
With Tabs and	Newlines and JS script <img src=x onerror=alert(document.domain)>";

			AssertEquals("Question&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Html Encoded Text<br/>With Tabs and&nbsp;&nbsp;&nbsp;&nbsp;Newlines and JS script &lt;img src=x onerror=alert(document.domain)&gt;", Question.QuestionTextForWeb);
		}

		public void TestSetQuestionTextAndEncode()
		{
			Question.SetQuestionTextAndEncode("Question 1:<script src=\"\" media=''>[csharp]<div>[b]\"A\"[/b] && [i]'B'[/i]</div>[/csharp]");
			AssertEquals(@"Question 1:&lt;script src=&quot;&quot; media=&#39;&#39;&gt;[csharp]&lt;div&gt;[b]&quot;A&quot;[/b] &amp;&amp; [i]&#39;B&#39;[/i]&lt;/div&gt;[/csharp]", Question.HY_Question);
		}

		public void TestHY_AnswerType()
		{
			AssertEquals("Pre-condition", "", Question.HY_AnswerType);
			AssertEquals("Pre-condition", ZShort.Zero, Question.HY_Min);
			AssertEquals("Pre-condition", ZShort.Zero, Question.HY_Max);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			Question.HY_ExamCorrectAnswer = "1";
			AssertEquals(VoteExamSurveyQuestion.Constants.DefaultMinSelectionCount, Question.HY_Min);
			AssertEquals(VoteExamSurveyQuestion.Constants.DefaultMaxSelectionCount, Question.HY_Max);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			AssertEquals("Correct answer should be reset", "", Question.HY_ExamCorrectAnswer);
			AssertEquals(VoteExamSurveyQuestion.Constants.DefaultMinNumericRange, Question.HY_Min);
			AssertEquals(VoteExamSurveyQuestion.Constants.DefaultMaxNumericRange, Question.HY_Max);

			CodeDescriptionPairList list = (CodeDescriptionPairList)MetaData.GetListDataSource(Question, Question.HY_AnswerTypeInfo.PropertyDescriptor);
			AssertEquals(Question.Lookups.AnswerTypes.ElementsAsString, list.ElementsAsString);
		}

		public void TestHY_AnswerTypeReadOnly()
		{
			Question.FillWithValidTestData();
			Assert(!Question.HY_AnswerTypeInfo.ReadOnly);

			Factory.Save();
			Assert(Question.HY_AnswerTypeInfo.ReadOnly);
		}

		public void TestSettingHY_AnswerTypeShouldReOrderQuestions()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			VoteExamSurveyQuestion question3 = campaign.Questions.AddNew();
			AssertEquals((ZShort)1, question1.HY_QuestionOrder);
			AssertEquals((ZShort)2, question2.HY_QuestionOrder);
			AssertEquals((ZShort)3, question3.HY_QuestionOrder);

			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			AssertEquals((ZShort)1, question1.HY_QuestionOrder);
			AssertEquals((ZShort)1, question2.HY_QuestionOrder);
			AssertEquals((ZShort)2, question3.HY_QuestionOrder);

			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			AssertEquals((ZShort)1, question1.HY_QuestionOrder);
			AssertEquals((ZShort)2, question2.HY_QuestionOrder);
			AssertEquals((ZShort)3, question3.HY_QuestionOrder);

			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			AssertEquals((ZShort)1, question1.HY_QuestionOrder);
			AssertEquals((ZShort)2, question2.HY_QuestionOrder);
			AssertEquals((ZShort)2, question3.HY_QuestionOrder);
		}

		public void TestIsHeader()
		{
			Assert(!Question.IsHeader);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			Assert(Question.IsHeader);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			Assert(!Question.IsHeader);
		}

		public void TestCountryCodeInfo()
		{
			Assert(!Question.HY_RN_NKCountryCodeInfo.ReadOnly);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			Assert(Question.HY_RN_NKCountryCodeInfo.ReadOnly);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			Assert(!Question.HY_RN_NKCountryCodeInfo.ReadOnly);
		}

		public void TestIsRandomisableInfo()
		{
			Assert(Question.HY_IsRandomisableInfo.ReadOnly);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			Assert(!Question.HY_IsRandomisableInfo.ReadOnly);
		}

		public void TestQuestionCategoryInfo()
		{
			Assert(!Question.HY_QuestionCategoryInfo.ReadOnly);

			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			Assert(Question.HY_QuestionCategoryInfo.ReadOnly);
		}

		public void TestGetQuestionSet()
		{
			AssertEquals(Question.Campaign.Questions, Question.GetQuestionSet());

			VoteExamSurveyQuestion subQuestion = Question.SubQuestions.AddNew();
			AssertEquals(Question.SubQuestions, subQuestion.GetQuestionSet());
		}

		public void TestInactiveSubQuestions()
		{
			VoteExamSurveyQuestion question1 = Question.SubQuestions.AddNew();
			VoteExamSurveyQuestion question2 = Question.SubQuestions.AddNew();
			AssertEquals("Precondition", 2, Question.SubQuestions.Count);
			AssertEquals("Precondition", 0, Question.InactiveSubQuestions.Count);

			question1.HY_IsActive = false;
			AssertEquals(1, Question.SubQuestions.Count);
			AssertEquals(1, Question.InactiveSubQuestions.Count);
			AssertCollectionContains(question1, Question.InactiveSubQuestions);

			question2.HY_IsActive = false;
			AssertEquals(0, Question.SubQuestions.Count);
			AssertEquals(2, Question.InactiveSubQuestions.Count);
			AssertCollectionContains(question2, Question.InactiveSubQuestions);

			question1.HY_IsActive = true;
			AssertEquals(1, Question.SubQuestions.Count);
			AssertCollectionContains(question1, Question.SubQuestions);
			AssertEquals(1, Question.InactiveSubQuestions.Count);
			AssertCollectionContains(question2, Question.InactiveSubQuestions);
		}

		public void TestGetParentQuestion()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			VoteExamSurveyQuestion header = campaign.Questions.AddNew();
			header.HY_QuestionOrder = 1;
			header.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_QuestionOrder = 1;
			VoteExamSurveyQuestion subQuestion1 = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion subQuestion2 = question.SubQuestions.AddNew();
			subQuestion2.HY_IsActive = false;
			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();

			AssertEquals(1, question.SubQuestions.Count);
			AssertEquals(1, question.SubQuestions.InactiveQuestions.Count);
			AssertEquals(question, subQuestion1.GetParentQuestion());
			AssertEquals(question, subQuestion2.GetParentQuestion());
			AssertNull(question2.GetParentQuestion());
		}

		public void TestSuspendResumeSettingSubQuestionsOrder()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			VoteExamSurveyQuestion subQuestion1 = question.SubQuestions.AddNew();
			VoteExamSurveyQuestion subQuestion2 = question.SubQuestions.AddNew();
			question.HY_QuestionOrder = 4;
			AssertEquals((ZByte)4, subQuestion1.HY_QuestionOrder);
			AssertEquals((ZByte)4, subQuestion2.HY_QuestionOrder);

			question.SuspendSettingSubQuestionsOrder();
			question.HY_QuestionOrder = 7;
			AssertEquals((ZByte)4, subQuestion1.HY_QuestionOrder);
			AssertEquals((ZByte)4, subQuestion2.HY_QuestionOrder);

			subQuestion1.HY_QuestionOrder = 7;
			subQuestion2.HY_QuestionOrder = 7;
			question.ResumeSettingSubQuestionsOrder();
			question.HY_QuestionOrder = 8;
			AssertEquals((ZByte)8, subQuestion1.HY_QuestionOrder);
			AssertEquals((ZByte)8, subQuestion2.HY_QuestionOrder);
		}

		public void TestShouldNotTranslateInactiveQuestions()
		{
			Question.FillWithValidTestData();
			Question.HY_Question = "AAA";
			Question.HY_IsActive = true;
			Factory.Save();
			var source = new MultipleDataCaptionSource(new[] { Question.HY_QuestionInfo }, ZString.Empty);
			var captions = string.Join("\r\n", source.GetRuntimeCaptions().Select(x => $"{x.ResourceKey}-{x.EnglishText}"));
			var expectedCaptions = "HY_Question@AAA$QUFB-AAA";
			AssertEquals(expectedCaptions, captions);

			Question.HY_IsActive = false;
			Factory.Save();
			source = new MultipleDataCaptionSource(new[] { Question.HY_QuestionInfo }, ZString.Empty);
			captions = string.Join("\r\n", source.GetRuntimeCaptions().Select(x => $"{x.ResourceKey}-{x.EnglishText}"));
			expectedCaptions = "";
			AssertEquals(expectedCaptions, captions);
		}

		public void TestOnSavingVoteSurveyExamWithNewQuestion_ShouldRefreshCampaignItemAnswers()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_CampaignName = "Survey 123";
			campaign.G0_CampaignComment = "A very important survey";
			VoteExamSurveyQuestion header = campaign.Questions.AddNew();
			header.HY_RN_NKCountryCode = "AU";
			header.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion auQuestion1 = campaign.Questions.AddNew();
			auQuestion1.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion auQuestion2 = campaign.Questions.AddNew();
			auQuestion2.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion nzQuestion1 = campaign.Questions.AddNew();
			nzQuestion1.HY_RN_NKCountryCode = "NZ";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			var answerSet = (IVoteExamSurveyAnswerSet)new VoteExamSurveyAnswerSet(Factory, campaignItem);
			answerSet.StartVoteExamSurvey();
			Factory.Save();

			AssertEquals("Precondition", 4, campaignItem.PersistedAnswers.Count);

			VoteExamSurveyQuestion nzQuestion2 = campaign.Questions.AddNew();
			nzQuestion2.HY_RN_NKCountryCode = "NZ";
			Factory.Save();

			AssertEquals("Should have been reset", GlbCompanyCampaignItemLookups.StagesConstants.Reset, campaignItem.G8_Stage);
		}

		public void TestOnSavingVoteSurveyExamWithQuestionChanges_ShouldRefreshCampaignItemAnswers()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_CampaignName = "Survey 123";
			campaign.G0_CampaignComment = "A very important survey";
			VoteExamSurveyQuestion header = campaign.Questions.AddNew();
			header.HY_RN_NKCountryCode = "AU";
			header.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion auQuestion1 = campaign.Questions.AddNew();
			auQuestion1.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion auQuestion2 = campaign.Questions.AddNew();
			auQuestion2.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion nzQuestion1 = campaign.Questions.AddNew();
			nzQuestion1.HY_RN_NKCountryCode = "NZ";
			nzQuestion1.HY_Question = "Hiya";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			var answerSet = (IVoteExamSurveyAnswerSet)new VoteExamSurveyAnswerSet(Factory, campaignItem);
			answerSet.StartVoteExamSurvey();
			Factory.Save();

			AssertEquals("Precondition", 4, campaignItem.PersistedAnswers.Count);
			nzQuestion1.HY_Question = "Heya";
			Factory.Save();

			AssertEquals("Should have been reset", GlbCompanyCampaignItemLookups.StagesConstants.Reset, campaignItem.G8_Stage);
		}

		public void TestOnSavingVoteSurveyExamWithNoQuestionChanges_ShouldNotRefreshCampaignItemAnswers()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_CampaignName = "Survey 123";
			campaign.G0_CampaignComment = "A very important survey";
			VoteExamSurveyQuestion header = campaign.Questions.AddNew();
			header.HY_RN_NKCountryCode = "AU";
			header.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			VoteExamSurveyQuestion auQuestion1 = campaign.Questions.AddNew();
			auQuestion1.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion auQuestion2 = campaign.Questions.AddNew();
			auQuestion2.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion nzQuestion1 = campaign.Questions.AddNew();
			nzQuestion1.HY_RN_NKCountryCode = "NZ";
			nzQuestion1.HY_Question = "Hiya";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			var answerSet = (IVoteExamSurveyAnswerSet)new VoteExamSurveyAnswerSet(Factory, campaignItem);
			answerSet.StartVoteExamSurvey();
			Factory.Save();

			AssertEquals("Precondition", 4, campaignItem.PersistedAnswers.Count);
			nzQuestion1.HY_Question = "Heya";
			nzQuestion1.HY_Question = "Hiya";
			nzQuestion1.HY_RN_NKCountryCode = "AU";
			nzQuestion1.HY_RN_NKCountryCode = "NZ";
			Factory.Save();

			AssertNotEquals("Should not have been reset", GlbCompanyCampaignItemLookups.StagesConstants.Reset, campaignItem.G8_Stage);
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<VoteExamSurveyQuestion>();
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Question should not be deleted", true);
		}

		VoteExamSurveyQuestion Question
		{
			get
			{
				if (fQuestion == null)
				{
					GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
					fQuestion = campaign.Questions.AddNew();
				}
				return fQuestion;
			}
		}

		VoteExamSurveyQuestion fQuestion;
	}
}
