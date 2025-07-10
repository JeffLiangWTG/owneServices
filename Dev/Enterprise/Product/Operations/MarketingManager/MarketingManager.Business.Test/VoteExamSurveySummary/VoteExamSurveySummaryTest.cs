using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveySummary))]
	sealed class VoteExamSurveySummaryTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			return new VoteExamSurveySummary(question);
		}

		#endregion

		public void TestValidateQuestionTextInMultiLingual()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.FreeText;
			question.HY_Question = "得分情况怎么样？";

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary = summaryCollection[0];
			summary.QuestionText = "得分情况怎么样？";
			Factory.Save();

			AssertEquals(summary.QuestionText, "得分情况怎么样？");
			AssertNoErrors("No errors expected", summary.QuestionTextInfo);
		}

		public void TestAnswerText()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			question1.HY_Question = "question1";
			VoteExamSurveyQuestion option11 = question1.SubQuestions.AddNew();
			option11.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option11.HY_Question = "option11";
			VoteExamSurveyQuestion option12 = question1.SubQuestions.AddNew();
			option12.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option12.HY_Question = "option12";
			VoteExamSurveyQuestion option13 = question1.SubQuestions.AddNew();
			option13.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option13.HY_Question = "option13";
			VoteExamSurveyQuestion option14 = question1.SubQuestions.AddNew();
			option14.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option14.HY_Question = "option14";
			VoteExamSurveyQuestion option15 = question1.SubQuestions.AddNew();
			option15.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option15.HY_Question = "option15";
			VoteExamSurveyQuestion option16 = question1.SubQuestions.AddNew();
			option16.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option16.HY_Question = "option16";
			VoteExamSurveyQuestion option17 = question1.SubQuestions.AddNew();
			option17.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option17.HY_Question = "option17";
			VoteExamSurveyQuestion option18 = question1.SubQuestions.AddNew();
			option18.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option18.HY_Question = "option18";
			VoteExamSurveyQuestion option19 = question1.SubQuestions.AddNew();
			option19.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option19.HY_Question = "option19";
			VoteExamSurveyQuestion option110 = question1.SubQuestions.AddNew();
			option110.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option110.HY_Question = "option110";

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals("option11", summary1.Answer1);
			AssertEquals("option12", summary1.Answer2);
			AssertEquals("option13", summary1.Answer3);
			AssertEquals("option14", summary1.Answer4);
			AssertEquals("option15", summary1.Answer5);
			AssertEquals("option16", summary1.Answer6);
			AssertEquals("option17", summary1.Answer7);
			AssertEquals("option18", summary1.Answer8);
			AssertEquals("option19", summary1.Answer9);
			AssertEquals("option110", summary1.Answer10);

			option11.HY_Question = ZString.Replicate('a', VoteExamSurveySummary.Schema.Answer1MaxLength + 1);
			option12.HY_Question = ZString.Replicate('a', VoteExamSurveySummary.Schema.Answer2MaxLength + 1);
			option13.HY_Question = ZString.Replicate('a', VoteExamSurveySummary.Schema.Answer3MaxLength + 1);
			option14.HY_Question = ZString.Replicate('a', VoteExamSurveySummary.Schema.Answer4MaxLength + 1);
			option15.HY_Question = ZString.Replicate('a', VoteExamSurveySummary.Schema.Answer5MaxLength + 1);
			option16.HY_Question = ZString.Replicate('a', VoteExamSurveySummary.Schema.Answer6MaxLength + 1);
			option17.HY_Question = ZString.Replicate('a', VoteExamSurveySummary.Schema.Answer7MaxLength + 1);
			option18.HY_Question = ZString.Replicate('a', VoteExamSurveySummary.Schema.Answer8MaxLength + 1);
			option19.HY_Question = ZString.Replicate('a', VoteExamSurveySummary.Schema.Answer9MaxLength + 1);
			option110.HY_Question = ZString.Replicate('a', VoteExamSurveySummary.Schema.Answer10MaxLength + 1);

			summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();
			summary1 = summaryCollection[0];
			AssertEquals(VoteExamSurveySummary.Schema.Answer1MaxLength, summary1.Answer1.Length);
			AssertEquals(VoteExamSurveySummary.Schema.Answer2MaxLength, summary1.Answer2.Length);
			AssertEquals(VoteExamSurveySummary.Schema.Answer3MaxLength, summary1.Answer3.Length);
			AssertEquals(VoteExamSurveySummary.Schema.Answer4MaxLength, summary1.Answer4.Length);
			AssertEquals(VoteExamSurveySummary.Schema.Answer5MaxLength, summary1.Answer5.Length);
			AssertEquals(VoteExamSurveySummary.Schema.Answer6MaxLength, summary1.Answer6.Length);
			AssertEquals(VoteExamSurveySummary.Schema.Answer7MaxLength, summary1.Answer7.Length);
			AssertEquals(VoteExamSurveySummary.Schema.Answer8MaxLength, summary1.Answer8.Length);
			AssertEquals(VoteExamSurveySummary.Schema.Answer9MaxLength, summary1.Answer9.Length);
			AssertEquals(VoteExamSurveySummary.Schema.Answer10MaxLength, summary1.Answer10.Length);
		}

		public void TestAnswerCount()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;

			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_ClosedDateUtc = DateTime.Now;
			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;

			var answer1 = Factory.New<VoteExamSurveyAnswer>();
			answer1.HZ_HY = question.PK;
			answer1.HZ_G8 = campaignItem.PK;
			answer1.AnswerAsInt = 1;
			var answer2 = Factory.New<VoteExamSurveyAnswer>();
			answer2.HZ_HY = question.PK;
			answer2.HZ_G8 = campaignItem2.PK;
			answer2.AnswerAsInt = 1;

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(2, summary1.Answer1Count);
			AssertEquals(0, summary1.Answer2Count);
			AssertEquals(0, summary1.Answer3Count);
			AssertEquals(0, summary1.Answer4Count);
			AssertEquals(0, summary1.Answer5Count);
			AssertEquals(0, summary1.Answer6Count);
			AssertEquals(0, summary1.Answer7Count);
			AssertEquals(0, summary1.Answer8Count);
			AssertEquals(0, summary1.Answer9Count);
			AssertEquals(0, summary1.Answer10Count);
		}

		public void TestSkippedCount()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test One";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test Two";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			var question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			var question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem1);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			var answer1_1 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem1);
			answer1_1.AnswerAsInt = 1;

			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem2);

			var answer1_2 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem2);
			answer1_2.AnswerAsInt = 1;
			var answer2_1 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem2);
			answer2_1.AnswerAsInt = 3;

			Factory.Save();

			var summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			var summary1 = summaryCollection[0];
			AssertEquals(2, summary1.NumberOfRecipient);
			AssertEquals(2, summary1.NumberOfRecipientReplied);
			AssertEquals(2, summary1.RecipientAnswered);
			AssertEquals(0, summary1.RecipientSkipped);

			var summary2 = summaryCollection[1];
			AssertEquals(2, summary2.NumberOfRecipient);
			AssertEquals(2, summary2.NumberOfRecipientReplied);
			AssertEquals(1, summary2.RecipientAnswered);
			AssertEquals(1, summary2.RecipientSkipped);
		}

		public void TestAnswerCountAsText()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test One";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test Two";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.FreeText;

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem1);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer1 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question, campaignItem1);
			answer1.AnswerAsInt = 1;

			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem2);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer2 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question, campaignItem2);
			answer2.AnswerAsInt = 1;

			Factory.Save();

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals("2", summary1.Answer1CountAsText);
			AssertEquals("0", summary1.Answer2CountAsText);
			AssertEquals("0", summary1.Answer3CountAsText);
			AssertEquals("0", summary1.Answer4CountAsText);
			AssertEquals("0", summary1.Answer5CountAsText);
			AssertEquals(string.Empty, summary1.Answer6CountAsText);
			AssertEquals(string.Empty, summary1.Answer7CountAsText);
			AssertEquals(string.Empty, summary1.Answer8CountAsText);
			AssertEquals(string.Empty, summary1.Answer9CountAsText);
			AssertEquals(string.Empty, summary1.Answer10CountAsText);

			AssertEquals("2", summary1.NumberOfRecipientAsText);
			AssertEquals("2", summary1.NumberOfRecipientRepliedAsText);
			AssertEquals("2", summary1.RecipientAnsweredAsText);
			AssertEquals("0", summary1.RecipientSkippedAsText);
			AssertEquals("1.00", summary1.AverageAsText);
			AssertEquals("0", summary1.AverageAsPercentageAsText);
			AssertEquals("5", summary1.NumberOfAnswersAsText);

			VoteExamSurveySummary summary2 = summaryCollection[1];
			AssertEquals(string.Empty, summary2.Answer1CountAsText);
			AssertEquals(string.Empty, summary2.Answer2CountAsText);
			AssertEquals(string.Empty, summary2.Answer3CountAsText);
			AssertEquals(string.Empty, summary2.Answer4CountAsText);
			AssertEquals(string.Empty, summary2.Answer5CountAsText);
			AssertEquals(string.Empty, summary2.Answer6CountAsText);
			AssertEquals(string.Empty, summary2.Answer7CountAsText);
			AssertEquals(string.Empty, summary2.Answer8CountAsText);
			AssertEquals(string.Empty, summary2.Answer9CountAsText);
			AssertEquals(string.Empty, summary2.Answer10CountAsText);
		}

		public void TestAnalyzeMultipleChoiceQuestion()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test One";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test Two";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Test Three";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			question1.HY_Question = "question1";
			VoteExamSurveyQuestion option11 = question1.SubQuestions.AddNew();
			option11.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option11.HY_Question = "option11";
			VoteExamSurveyQuestion option12 = question1.SubQuestions.AddNew();
			option12.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option12.HY_Question = "option12";

			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			question2.HY_Question = "question2";
			VoteExamSurveyQuestion option21 = question2.SubQuestions.AddNew();
			option21.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option21.HY_Question = "option21";
			VoteExamSurveyQuestion option22 = question2.SubQuestions.AddNew();
			option22.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option22.HY_Question = "option22";

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem1);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer11 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(option11, voteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer11.AnswerAsBool = true;
			VoteExamSurveyAnswer answer12 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(option21, voteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer12.AnswerAsBool = true;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			voteExamSurveyAnswerSet = voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem2);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer21 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(option11, voteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer21.AnswerAsBool = true;

			GlbCompanyCampaignItem campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			Factory.Save();

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(1, summary1.Number);
			AssertEquals("question1", summary1.QuestionText);
			AssertEquals(3, summary1.NumberOfRecipient);
			AssertEquals(2, summary1.NumberOfRecipientReplied);
			AssertEquals(2, summary1.RecipientAnswered);
			AssertEquals(0, summary1.RecipientSkipped);
			AssertEquals("option11", summary1.Answer1);
			AssertEquals(2, summary1.Answer1Count);
			AssertEquals("option12", summary1.Answer2);
			AssertEquals(0, summary1.Answer2Count);
			AssertEquals(new ZDecimal((1 + 1) / (decimal)2), summary1.Average);
			AssertEquals(new ZDecimal((summary1.Average - 1) * 100 / (summary1.NumberOfAnswers - 1)), summary1.AverageAsPercentage);

			VoteExamSurveySummary summary2 = summaryCollection[1];
			AssertEquals(2, summary2.Number);
			AssertEquals("question2", summary2.QuestionText);
			AssertEquals(3, summary2.NumberOfRecipient);
			AssertEquals(2, summary2.NumberOfRecipientReplied);
			AssertEquals(1, summary2.RecipientAnswered);
			AssertEquals(1, summary2.RecipientSkipped);
			AssertEquals("option21", summary2.Answer1);
			AssertEquals(1, summary2.Answer1Count);
			AssertEquals("option22", summary2.Answer2);
			AssertEquals(0, summary2.Answer2Count);
			AssertEquals(new ZDecimal(1 / (decimal)1), summary2.Average);
			AssertEquals(new ZDecimal((summary2.Average - 1) * 100 / (summary2.NumberOfAnswers - 1)), summary2.AverageAsPercentage);
		}

		public void TestAnalyzeLikertScaleQuestion()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test One";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test Two";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Test Three";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			question1.HY_Question = "question1";

			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			question2.HY_Question = "question2";

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem1);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer11 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, voteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer11.AnswerAsInt = 1;
			VoteExamSurveyAnswer answer12 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, voteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer12.AnswerAsInt = 2;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem2);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer21 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem2);
			answer21.AnswerAsInt = 2;

			GlbCompanyCampaignItem campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			Factory.Save();

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(1, summary1.Number);
			AssertEquals("question1", summary1.QuestionText);
			AssertEquals(3, summary1.NumberOfRecipient);
			AssertEquals(2, summary1.NumberOfRecipientReplied);
			AssertEquals(2, summary1.RecipientAnswered);
			AssertEquals(0, summary1.RecipientSkipped);
			AssertEquals("Strongly Disagree", summary1.Answer1);
			AssertEquals(1, summary1.Answer1Count);
			AssertEquals("Disagree", summary1.Answer2);
			AssertEquals(1, summary1.Answer2Count);
			AssertEquals(new ZDecimal((1 + 2) / (decimal)2), summary1.Average);
			AssertEquals(new ZDecimal((summary1.Average - 1) * 100 / (summary1.NumberOfAnswers - 1)), summary1.AverageAsPercentage);

			VoteExamSurveySummary summary2 = summaryCollection[1];
			AssertEquals(2, summary2.Number);
			AssertEquals("question2", summary2.QuestionText);
			AssertEquals(3, summary2.NumberOfRecipient);
			AssertEquals(2, summary2.NumberOfRecipientReplied);
			AssertEquals(1, summary2.RecipientAnswered);
			AssertEquals(1, summary2.RecipientSkipped);
			AssertEquals("Strongly Disagree", summary2.Answer1);
			AssertEquals(0, summary2.Answer1Count);
			AssertEquals("Disagree", summary2.Answer2);
			AssertEquals(1, summary2.Answer2Count);
			AssertEquals(new ZDecimal(2 / (decimal)1), summary2.Average);
			AssertEquals(new ZDecimal((summary2.Average - 1) * 100 / (summary2.NumberOfAnswers - 1)), summary2.AverageAsPercentage);
		}

		public void TestAnalyzeTrueFalseQuestion()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test One";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test Two";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Test Three";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			question1.HY_Question = "question1";

			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			question2.HY_Question = "question2";

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem1);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer11 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem1);
			answer11.AnswerAsInt = 1;
			VoteExamSurveyAnswer answer12 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem1);
			answer12.AnswerAsInt = 2;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem2);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer21 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem2);
			answer21.AnswerAsInt = 2;

			GlbCompanyCampaignItem campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			Factory.Save();

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(1, summary1.Number);
			AssertEquals("question1", summary1.QuestionText);
			AssertEquals(3, summary1.NumberOfRecipient);
			AssertEquals(2, summary1.NumberOfRecipientReplied);
			AssertEquals(2, summary1.RecipientAnswered);
			AssertEquals(0, summary1.RecipientSkipped);
			AssertEquals("True", summary1.Answer1);
			AssertEquals(1, summary1.Answer1Count);
			AssertEquals("False", summary1.Answer2);
			AssertEquals(1, summary1.Answer2Count);
			AssertEquals(new ZDecimal((1 + 2) / (decimal)2), summary1.Average);
			AssertEquals(new ZDecimal((summary1.Average - 1) * 100 / (summary1.NumberOfAnswers - 1)), summary1.AverageAsPercentage);

			VoteExamSurveySummary summary2 = summaryCollection[1];
			AssertEquals(2, summary2.Number);
			AssertEquals("question2", summary2.QuestionText);
			AssertEquals(3, summary2.NumberOfRecipient);
			AssertEquals(2, summary2.NumberOfRecipientReplied);
			AssertEquals(1, summary2.RecipientAnswered);
			AssertEquals(1, summary2.RecipientSkipped);
			AssertEquals("True", summary2.Answer1);
			AssertEquals(0, summary2.Answer1Count);
			AssertEquals("False", summary2.Answer2);
			AssertEquals(1, summary2.Answer2Count);
			AssertEquals(new ZDecimal(2 / (decimal)1), summary2.Average);
			AssertEquals(new ZDecimal((summary2.Average - 1) * 100 / (summary2.NumberOfAnswers - 1)), summary2.AverageAsPercentage);
		}

		public void TestAnalyzeYesNoQuestion()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test One";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test Two";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Test Three";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			question1.HY_Question = "question1";

			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			question2.HY_Question = "question2";

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem1);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer11 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem1);
			answer11.AnswerAsInt = 1;
			VoteExamSurveyAnswer answer12 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem1);
			answer12.AnswerAsInt = 2;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem2);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer21 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem2);
			answer21.AnswerAsInt = 2;

			GlbCompanyCampaignItem campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			Factory.Save();

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(1, summary1.Number);
			AssertEquals("question1", summary1.QuestionText);
			AssertEquals(3, summary1.NumberOfRecipient);
			AssertEquals(2, summary1.NumberOfRecipientReplied);
			AssertEquals(2, summary1.RecipientAnswered);
			AssertEquals(0, summary1.RecipientSkipped);
			AssertEquals("Yes", summary1.Answer1);
			AssertEquals(1, summary1.Answer1Count);
			AssertEquals("No", summary1.Answer2);
			AssertEquals(1, summary1.Answer2Count);
			AssertEquals(new ZDecimal((1 + 2) / (decimal)2), summary1.Average);
			AssertEquals(new ZDecimal((summary1.Average - 1) * 100 / (summary1.NumberOfAnswers - 1)), summary1.AverageAsPercentage);

			VoteExamSurveySummary summary2 = summaryCollection[1];
			AssertEquals(2, summary2.Number);
			AssertEquals("question2", summary2.QuestionText);
			AssertEquals(3, summary2.NumberOfRecipient);
			AssertEquals(2, summary2.NumberOfRecipientReplied);
			AssertEquals(1, summary2.RecipientAnswered);
			AssertEquals(1, summary2.RecipientSkipped);
			AssertEquals("Yes", summary2.Answer1);
			AssertEquals(0, summary2.Answer1Count);
			AssertEquals("No", summary2.Answer2);
			AssertEquals(1, summary2.Answer2Count);
			AssertEquals(new ZDecimal(2 / (decimal)1), summary2.Average);
			AssertEquals(new ZDecimal((summary2.Average - 1) * 100 / (summary2.NumberOfAnswers - 1)), summary2.AverageAsPercentage);
		}

		public void TestAnalyzeNumericScaleQuestion()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test One";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test Two";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Test Three";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			question1.HY_Question = "question1";

			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			question2.HY_Question = "question2";

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem1);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer11 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem1);
			answer11.AnswerAsInt = 1;
			VoteExamSurveyAnswer answer12 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem1);
			answer12.AnswerAsInt = 2;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem2);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswer answer21 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem2);
			answer21.AnswerAsInt = 5;

			GlbCompanyCampaignItem campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			Factory.Save();

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(1, summary1.Number);
			AssertEquals("question1", summary1.QuestionText);
			AssertEquals(3, summary1.NumberOfRecipient);
			AssertEquals(2, summary1.NumberOfRecipientReplied);
			AssertEquals(2, summary1.RecipientAnswered);
			AssertEquals(0, summary1.RecipientSkipped);
			AssertEquals(new ZDecimal((1 + 5) / (decimal)2), summary1.Average);
			AssertEquals(new ZDecimal((summary1.Average - question1.HY_Min) * 100 / (question1.HY_Max - question1.HY_Min)), summary1.AverageAsPercentage);

			VoteExamSurveySummary summary2 = summaryCollection[1];
			AssertEquals(2, summary2.Number);
			AssertEquals("question2", summary2.QuestionText);
			AssertEquals(3, summary2.NumberOfRecipient);
			AssertEquals(2, summary2.NumberOfRecipientReplied);
			AssertEquals(1, summary2.RecipientAnswered);
			AssertEquals(1, summary2.RecipientSkipped);
			AssertEquals(new ZDecimal(2 / (decimal)1), summary2.Average);
			AssertEquals(new ZDecimal((summary2.Average - question1.HY_Min) * 100 / (question1.HY_Max - question1.HY_Min)), summary2.AverageAsPercentage);
		}

		public void TestAnalyzePercentageQuestion()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test One";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test Two";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Test Three";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Percentage;
			question1.HY_Question = "question1";

			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Percentage;
			question2.HY_Question = "question2";

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var answer11 = Factory.New<VoteExamSurveyAnswer>();
			answer11.HZ_HY = question1.PK;
			answer11.HZ_G8 = campaignItem1.PK;
			answer11.AnswerAsInt = 10;
			VoteExamSurveyAnswer answer12 = Factory.New<VoteExamSurveyAnswer>();
			answer12.HZ_HY = question2.PK;
			answer12.HZ_G8 = campaignItem1.PK;
			answer12.AnswerAsInt = 20;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			VoteExamSurveyAnswer answer21 = Factory.New<VoteExamSurveyAnswer>();
			answer21.HZ_HY = question1.PK;
			answer21.HZ_G8 = campaignItem2.PK;
			answer21.AnswerAsInt = 50;

			GlbCompanyCampaignItem campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			Factory.Save();

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(1, summary1.Number);
			AssertEquals("question1", summary1.QuestionText);
			AssertEquals(3, summary1.NumberOfRecipient);
			AssertEquals(2, summary1.NumberOfRecipientReplied);
			AssertEquals(2, summary1.RecipientAnswered);
			AssertEquals(0, summary1.RecipientSkipped);
			AssertEquals(new ZDecimal((10 + 50) / (decimal)2), summary1.Average);
			AssertEquals(new ZDecimal(summary1.Average), summary1.AverageAsPercentage);

			VoteExamSurveySummary summary2 = summaryCollection[1];
			AssertEquals(2, summary2.Number);
			AssertEquals("question2", summary2.QuestionText);
			AssertEquals(3, summary2.NumberOfRecipient);
			AssertEquals(2, summary2.NumberOfRecipientReplied);
			AssertEquals(1, summary2.RecipientAnswered);
			AssertEquals(1, summary2.RecipientSkipped);
			AssertEquals(new ZDecimal(20 / (decimal)1), summary2.Average);
			AssertEquals(new ZDecimal(summary2.Average), summary2.AverageAsPercentage);
		}

		public void TestAnalyzeFreeTextQuestion()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test One";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test Two";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Test Three";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.FreeText;
			question1.HY_Question = "question1";

			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.FreeText;
			question2.HY_Question = "question2";

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var answer11 = Factory.New<VoteExamSurveyAnswer>();
			answer11.HZ_AnswerComment = "answer11";
			answer11.HZ_HY = question1.PK;
			answer11.HZ_G8 = campaignItem1.PK;
			var answer12 = Factory.New<VoteExamSurveyAnswer>();
			answer12.HZ_AnswerComment = "answer12";
			answer12.HZ_HY = question2.PK;
			answer12.HZ_G8 = campaignItem1.PK;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			var answer21 = Factory.New<VoteExamSurveyAnswer>();
			answer21.HZ_AnswerComment = "answer12";
			answer21.HZ_HY = question1.PK;
			answer21.HZ_G8 = campaignItem2.PK;
			answer21.HZ_AnswerComment = "answer21";

			GlbCompanyCampaignItem campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			Factory.Save();

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(1, summary1.Number);
			AssertEquals("question1", summary1.QuestionText);
			AssertEquals(3, summary1.NumberOfRecipient);
			AssertEquals(2, summary1.NumberOfRecipientReplied);
			AssertEquals(2, summary1.RecipientAnswered);
			AssertEquals(0, summary1.RecipientSkipped);

			VoteExamSurveySummary summary2 = summaryCollection[1];
			AssertEquals(2, summary2.Number);
			AssertEquals("question2", summary2.QuestionText);
			AssertEquals(3, summary2.NumberOfRecipient);
			AssertEquals(2, summary2.NumberOfRecipientReplied);
			AssertEquals(1, summary2.RecipientAnswered);
			AssertEquals(1, summary2.RecipientSkipped);
		}

		public void TestHeaderQuestion()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			question1.HY_Question = "question1";

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(1, summary1.Number);
			AssertEquals("question1", summary1.QuestionText);
		}

		public void TestAnalyzeRankedVote()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test One";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test Two";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			campaign.VoteHeader.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.RankedVote;
			campaign.VoteHeader.HY_Max = 10;

			VoteExamSurveyQuestion question1 = campaign.VoteHeader.SubQuestions.AddNew();
			question1.HY_Question = "question1";
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;

			VoteExamSurveyQuestion question2 = campaign.VoteHeader.SubQuestions.AddNew();
			question2.HY_Question = "question2";
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			VoteExamSurveyAnswer answer11 = Factory.New<VoteExamSurveyAnswer>();
			answer11.AnswerAsInt = 1;
			answer11.HZ_HY = question1.PK;
			answer11.HZ_G8 = campaignItem1.PK;
			VoteExamSurveyAnswer answer12 = Factory.New<VoteExamSurveyAnswer>();
			answer12.AnswerAsInt = 2;
			answer12.HZ_HY = question2.PK;
			answer12.HZ_G8 = campaignItem1.PK;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			VoteExamSurveyAnswer answer21 = Factory.New<VoteExamSurveyAnswer>();
			answer21.AnswerAsInt = 5;
			answer21.HZ_HY = question1.PK;
			answer21.HZ_G8 = campaignItem2.PK;

			Factory.Save();

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(1, summary1.Number);
			AssertEquals("question1", summary1.QuestionText);
			AssertEquals(2, summary1.NumberOfRecipient);
			AssertEquals(2, summary1.NumberOfRecipientReplied);
			AssertEquals(2, summary1.RecipientAnswered);
			AssertEquals(0, summary1.RecipientSkipped);
			AssertEquals(new ZDecimal((1 + 5) / (decimal)2), summary1.Average);
			AssertEquals(new ZDecimal((summary1.Average - 1) * 100 / (question1.Campaign.VoteHeader.HY_Max - 1)), summary1.AverageAsPercentage);

			VoteExamSurveySummary summary2 = summaryCollection[1];
			AssertEquals(2, summary2.Number);
			AssertEquals("question2", summary2.QuestionText);
			AssertEquals(2, summary2.NumberOfRecipient);
			AssertEquals(2, summary2.NumberOfRecipientReplied);
			AssertEquals(1, summary2.RecipientAnswered);
			AssertEquals(1, summary2.RecipientSkipped);
			AssertEquals(new ZDecimal(2 / (decimal)1), summary2.Average);
			AssertEquals(new ZDecimal((summary2.Average - 1) * 100 / (question1.Campaign.VoteHeader.HY_Max - 1)), summary2.AverageAsPercentage);
		}

		public void TestAnalyzeUnrankedVote()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test One";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test Two";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			campaign.VoteHeader.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.UnrankedVote;

			VoteExamSurveyQuestion question1 = campaign.VoteHeader.SubQuestions.AddNew();
			question1.HY_Question = "question1";
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;

			VoteExamSurveyQuestion question2 = campaign.VoteHeader.SubQuestions.AddNew();
			question2.HY_Question = "question2";
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;

			GlbCompanyCampaignItem campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_ClosedDateUtc = DateTime.Now;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			VoteExamSurveyAnswer answer12 = Factory.New<VoteExamSurveyAnswer>();
			answer12.HZ_Answer = "Y";
			answer12.HZ_HY = question2.PK;
			answer12.HZ_G8 = campaignItem1.PK;

			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_ClosedDateUtc = DateTime.Now;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			VoteExamSurveyAnswer answer21 = Factory.New<VoteExamSurveyAnswer>();
			answer21.HZ_Answer = "Y";
			answer21.HZ_HY = question1.PK;
			answer21.HZ_G8 = campaignItem2.PK;

			Factory.Save();

			VoteExamSurveySummaryCollection summaryCollection = new VoteExamSurveySummaryCollection(campaign.ActualQuestionsForBinding);
			summaryCollection.Load();

			VoteExamSurveySummary summary1 = summaryCollection[0];
			AssertEquals(1, summary1.Number);
			AssertEquals("question1", summary1.QuestionText);
			AssertEquals(2, summary1.NumberOfRecipient);
			AssertEquals(2, summary1.NumberOfRecipientReplied);
			AssertEquals(2, summary1.RecipientAnswered);
			AssertEquals(0, summary1.RecipientSkipped);
			AssertEquals(new ZDecimal((2 + 1) / (decimal)2), summary1.Average);
			AssertEquals(new ZDecimal((summary1.Average - 1) * 100 / (summary1.NumberOfAnswers - 1)), summary1.AverageAsPercentage);

			VoteExamSurveySummary summary2 = summaryCollection[1];
			AssertEquals(2, summary2.Number);
			AssertEquals("question2", summary2.QuestionText);
			AssertEquals(2, summary2.NumberOfRecipient);
			AssertEquals(2, summary2.NumberOfRecipientReplied);
			AssertEquals(2, summary2.RecipientAnswered);
			AssertEquals(0, summary2.RecipientSkipped);
			AssertEquals(new ZDecimal((2 + 1) / (decimal)2), summary2.Average);
			AssertEquals(new ZDecimal((summary2.Average - 1) * 100 / (summary2.NumberOfAnswers - 1)), summary2.AverageAsPercentage);
		}
	}
}
