using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ExamAnswerArchive))]
	sealed class ExamAnswerArchiveTest : NonPersistentBusinessObjectTestCase
	{
		public void TestQuestion()
		{
			AssertNull(CachedBusinessObject.Question);

			LearningCentreQuestion question = Factory.New<LearningCentreQuestion>();
			CachedBusinessObject.QuestionPK = question.PK;
			AssertEquals(question, CachedBusinessObject.Question);

			question.HY_Question = "Q1: version 1";
			AssertEquals("Q1: version 1", CachedBusinessObject.QuestionText);

			CachedBusinessObject.QuestionAsString = "Q1: version 2";
			AssertEquals("Q1: version 2", CachedBusinessObject.QuestionText);
		}

		public void TestForeignQuestion()
		{
			CachedBusinessObject.QuestionAsString = "漢字";
			AssertEquals("漢字", CachedBusinessObject.QuestionAsString);
			AssertNoErrors(CachedBusinessObject.QuestionAsStringInfo);
		}

		public void TestAnswer()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion option1 = question.SubQuestions.AddNew();
			option1.HY_Question = "opt1";
			VoteExamSurveyQuestion option2 = question.SubQuestions.AddNew();
			option2.HY_Question = "opt2";
			VoteExamSurveyQuestion option3 = question.SubQuestions.AddNew();
			option3.HY_Question = "opt3";

			CachedBusinessObject.QuestionPK = question.PK;
			CachedBusinessObject.SetSelectedQuestionPKsForTest(new ZGuid[] { option1.PK, option3.PK });
			AssertEquals("1) opt1\r\n3) opt3\r\n", CachedBusinessObject.Answer);
			AssertEquals(nameof(FieldType.TextMultiLine), CachedBusinessObject.AnswerFieldType);

			VoteExamSurveyQuestion anotherQuestion = campaign.Questions.AddNew();
			CachedBusinessObject.QuestionPK = anotherQuestion.PK;
			anotherQuestion.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			CachedBusinessObject.AnswerAsString = "1";
			AssertEquals("True", CachedBusinessObject.Answer);
			AssertEquals(nameof(FieldType.Text), CachedBusinessObject.AnswerFieldType);
		}

		public void TestReadOnly()
		{
			Assert(CachedBusinessObject.ReadOnly);
		}

		public void TestPopulate_MultipleChoice()
		{
			LearningCentreCampaignItem campaignItem = Factory.New<LearningCentreCampaignItem>();
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			LearningCentreQuestion option1 = question.SubQuestions.AddNew();
			option1.CorrectAnswerAsBool = true;
			LearningCentreQuestion option2 = question.SubQuestions.AddNew();
			LearningCentreQuestion option3 = question.SubQuestions.AddNew();
			option3.CorrectAnswerAsBool = true;
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			VoteExamSurveyAnswer answer1 = examAnswerSet.PersistedAnswers.CreateNew(option1, campaignItem);
			answer1.AnswerAsBool = true;
			VoteExamSurveyAnswer answer2 = examAnswerSet.PersistedAnswers.CreateNew(option3, campaignItem);
			answer2.AnswerAsBool = true;

			LearningCentreSubmittedAnswer submittedAnswer = new LearningCentreSubmittedAnswer(campaignItem, question);
			CachedBusinessObject.Populate(submittedAnswer);
			AssertEquals("", CachedBusinessObject.AnswerAsString);
			AssertEquals(2, CachedBusinessObject.SelectedQuestionPKs.Count());
			AssertCollectionContains(option1.PK, CachedBusinessObject.SelectedQuestionPKs);
			AssertCollectionContains(option3.PK, CachedBusinessObject.SelectedQuestionPKs);
			AssertEquals(true, CachedBusinessObject.IsAnsweredCorrectly);
		}

		public void TestPopulate_TrueFalse()
		{
			LearningCentreCampaignItem campaignItem = Factory.New<LearningCentreCampaignItem>();
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			question.HY_ExamCorrectAnswer = "2";
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			VoteExamSurveyAnswer answer = examAnswerSet.PersistedAnswers.CreateNew(question, campaignItem);
			answer.HZ_Answer = "1";

			LearningCentreSubmittedAnswer submittedAnswer = new LearningCentreSubmittedAnswer(campaignItem, question);
			CachedBusinessObject.Populate(submittedAnswer);
			AssertEquals("1", CachedBusinessObject.AnswerAsString);
			AssertEquals(0, CachedBusinessObject.SelectedQuestionPKs.Count());
			AssertEquals(false, CachedBusinessObject.IsAnsweredCorrectly);
		}

		public void TestIVoteExamSurveySubmittedAnswerMembers()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion option1 = question.SubQuestions.AddNew();
			option1.HY_Question = "opt1";
			VoteExamSurveyQuestion option2 = question.SubQuestions.AddNew();
			option2.HY_Question = "opt2";
			VoteExamSurveyQuestion option3 = question.SubQuestions.AddNew();
			option3.HY_Question = "opt3";

			CachedBusinessObject.QuestionPK = question.PK;
			CachedBusinessObject.SetSelectedQuestionPKsForTest(new ZGuid[] { option1.PK, option3.PK });

			IVoteExamSurveySubmittedAnswer submittedAnswer = CachedBusinessObject;
			CachedBusinessObject.AnswerAsString = "MEH";
			AssertEquals("MEH", submittedAnswer.PersistedAnswer);
			List<VoteExamSurveyQuestion> selectedOptions = new List<VoteExamSurveyQuestion>(submittedAnswer.SelectedMultipleChoiceOptions);
			AssertEquals(2, selectedOptions.Count);
			AssertCollectionContains(option1, selectedOptions);
			AssertCollectionContains(option3, selectedOptions);
		}

		public void TestSerialiseDeserialise_MultipleChoice()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion option1 = question.SubQuestions.AddNew();
			option1.HY_Question = "opt1";
			VoteExamSurveyQuestion option2 = question.SubQuestions.AddNew();
			option2.HY_Question = "opt2";
			VoteExamSurveyQuestion option3 = question.SubQuestions.AddNew();
			option3.HY_Question = "opt3";

			CachedBusinessObject.QuestionPK = question.PK;
			CachedBusinessObject.SetSelectedQuestionPKsForTest(new ZGuid[] { option1.PK, option3.PK });
			var orderedOptionPKs = new ZGuid[] { option3.PK, option1.PK, option2.PK };
			CachedBusinessObject.SetOrderedOptionPKsForTest(orderedOptionPKs);
			CachedBusinessObject.AnswerAsString = "MEH";
			StringBuilder builder = new StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(builder))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("root");
				((IXmlSerializable)CachedBusinessObject).WriteXml(writer);
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}

			ExamAnswerArchive newArchive = new ExamAnswerArchive(Factory);
			using (StringReader stringReader = new StringReader(builder.ToString()))
			using (XmlReader reader = XmlReader.Create(stringReader))
			{
				((IXmlSerializable)newArchive).ReadXml(reader);
			}
			AssertEquals(2, newArchive.SelectedQuestionPKs.Count());
			AssertEquals(3, newArchive.OrderedMultipleChoiceOptionPKs.Count());
			AssertArrayEqualsByElements(orderedOptionPKs, newArchive.OrderedMultipleChoiceOptionPKs.ToArray());
			AssertEquals("MEH", newArchive.AnswerAsString);
		}

		public void TestIsAnsweredIncorrectly()
		{
			LearningCentreQuestion question = Factory.New<LearningCentreQuestion>();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;

			ExamAnswerArchive archive = new ExamAnswerArchive(Factory);
			archive.QuestionPK = question.PK;
			archive.IsAnsweredCorrectly = true;
			Assert(!archive.IsAnsweredIncorrectly);

			archive.IsAnsweredCorrectly = false;
			Assert("Not populated", !archive.IsAnsweredIncorrectly);

			archive.AnswerAsString = "Y";
			Assert(archive.IsAnsweredIncorrectly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExamAnswerArchive(Factory);
		}

		new ExamAnswerArchive CachedBusinessObject
		{
			get { return (ExamAnswerArchive)base.CachedBusinessObject; }
		}
	}
}
