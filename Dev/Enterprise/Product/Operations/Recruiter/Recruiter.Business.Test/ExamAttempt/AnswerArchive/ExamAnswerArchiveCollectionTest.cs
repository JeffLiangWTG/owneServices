using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ExamAnswerArchiveCollection))]
	sealed class ExamAnswerArchiveCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExamAnswerArchiveCollection>
	{
		public void TestPopulate()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion option2a = question2.SubQuestions.AddNew();
			VoteExamSurveyQuestion option2b = question2.SubQuestions.AddNew();
			VoteExamSurveyQuestion question3 = campaign.Questions.AddNew();
			question3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			VoteExamSurveyQuestion question4 = campaign.Questions.AddNew();
			question4.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			question2.SubQuestions.AddNew();
			question2.SubQuestions.AddNew();
			VoteExamSurveyQuestion question5 = campaign.Questions.AddNew();
			question5.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			question2.SubQuestions.AddNew();
			question2.SubQuestions.AddNew();

			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			VoteExamSurveyAnswer answer1 = examAnswerSet.PersistedAnswers.CreateNew(question1, campaignItem);
			answer1.HZ_Answer = "2";
			VoteExamSurveyAnswer answer2 = examAnswerSet.PersistedAnswers.CreateNew(option2b, campaignItem);
			answer2.AnswerAsBool = true;
			examAnswerSet.PersistedAnswers.CreateNew(question4, campaignItem);

			ExamAnswerArchiveCollection collection = new ExamAnswerArchiveCollection(Factory);
			collection.Populate(campaignItem);
			AssertEquals(3, collection.Count);
			AssertEquals("Should include populated answers", question1.PK, collection[0].QuestionPK);
			AssertEquals("Should include populated answers", question2.PK, collection[1].QuestionPK);
			AssertEquals("Should include empty (unanswered) answers", question4.PK, collection[2].QuestionPK);

			examAnswerSet.PersistedAnswers.CreateNew(question5, campaignItem);
			campaignItem.SubmittedAnswers.Load();
			collection = new ExamAnswerArchiveCollection(Factory);
			collection.Populate(campaignItem);
			AssertEquals(4, collection.Count);
			AssertEquals("Should include populated answers", question1.PK, collection[0].QuestionPK);
			AssertEquals("Should include populated answers", question2.PK, collection[1].QuestionPK);
			AssertEquals("Should include empty (unanswered) answers", question4.PK, collection[2].QuestionPK);
			AssertEquals("Should include empty (unanswered) answers", question5.PK, collection[3].QuestionPK);
		}

		public void TestReadOnly()
		{
			Assert(Collection.ReadOnly);
		}

		public void TestSerialiseDeserialise()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion option2a = question2.SubQuestions.AddNew();
			VoteExamSurveyQuestion option2b = question2.SubQuestions.AddNew();
			VoteExamSurveyQuestion question3 = campaign.Questions.AddNew();
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			VoteExamSurveyAnswer answer1 = examAnswerSet.PersistedAnswers.CreateNew(question1, campaignItem);
			answer1.HZ_Answer = "2";
			VoteExamSurveyAnswer answer2 = examAnswerSet.PersistedAnswers.CreateNew(option2b, campaignItem);
			answer2.AnswerAsBool = true;
			ExamAnswerArchiveCollection collection = new ExamAnswerArchiveCollection(Factory);
			collection.Populate(campaignItem);

			StringBuilder builder = new StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(builder))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("root");
				((IXmlSerializable)collection).WriteXml(writer);
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}

			ExamAnswerArchiveCollection newCollection = new ExamAnswerArchiveCollection(Factory);
			using (StringReader stringReader = new StringReader(builder.ToString()))
			using (XmlReader reader = XmlReader.Create(stringReader))
			{
				((IXmlSerializable)newCollection).ReadXml(reader);
			}
			AssertEquals(2, newCollection.Count);
			AssertEquals(question1.PK, newCollection[0].QuestionPK);
			AssertEquals(question2.PK, newCollection[1].QuestionPK);
		}

		protected override ExamAnswerArchiveCollection GetCollectionToTest()
		{
			return new ExamAnswerArchiveCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExamAnswerArchive(Factory);
		}
	}
}
