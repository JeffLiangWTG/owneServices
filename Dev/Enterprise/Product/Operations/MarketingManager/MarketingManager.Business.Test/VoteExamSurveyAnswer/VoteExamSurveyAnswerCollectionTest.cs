using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveyAnswerCollection))]
	sealed class VoteExamSurveyAnswerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetCompletedAnswers()
		{
			VoteExamSurveyAnswer answer1 = Collection.AddNew();
			answer1.HZ_HY = Factory.New<VoteExamSurveyQuestion>().PK;
			answer1.HZ_Answer = "1";

			VoteExamSurveyAnswer answer2 = Collection.AddNew();
			answer2.HZ_HY = Factory.New<VoteExamSurveyQuestion>().PK;
			answer2.HZ_AnswerComment = "2";

			VoteExamSurveyAnswer answer3 = Collection.AddNew();
			VoteExamSurveyAnswer[] completedAnswers = Collection.GetCompletedAnswers();
			AssertEquals(2, completedAnswers.Length);
			Assert(((IList<VoteExamSurveyAnswer>)completedAnswers).Contains(answer1));
			Assert(((IList<VoteExamSurveyAnswer>)completedAnswers).Contains(answer2));
		}

		public void TestGetCompletedAnswers_ShouldNotIncludeInactiveAnswers()
		{
			VoteExamSurveyAnswer answer1 = Collection.AddNew();
			answer1.HZ_Answer = "1";

			VoteExamSurveyAnswer answer2 = Collection.AddNew();
			answer2.HZ_AnswerComment = "2";

			VoteExamSurveyAnswer answer3 = Collection.AddNew();
			VoteExamSurveyAnswer[] completedAnswers = Collection.GetCompletedAnswers();
			AssertEquals(0, completedAnswers.Length);

			answer1.HZ_HY = Factory.New<VoteExamSurveyQuestion>().PK;
			completedAnswers = Collection.GetCompletedAnswers();
			AssertEquals(1, completedAnswers.Length);
			AssertEquals(answer1, completedAnswers[0]);

			answer2.HZ_HY = Factory.New<VoteExamSurveyQuestion>().PK;
			answer2.Question.HY_IsActive = false;
			completedAnswers = Collection.GetCompletedAnswers();
			AssertEquals(1, completedAnswers.Length);
			AssertEquals(answer1, completedAnswers[0]);

			answer2.Question.HY_IsActive = true;
			completedAnswers = Collection.GetCompletedAnswers();
			AssertEquals(2, completedAnswers.Length);
			AssertCollectionContains(answer1, completedAnswers);
			AssertCollectionContains(answer2, completedAnswers);
		}

		public void TestFindByQuestion()
		{
			VoteExamSurveyQuestion question = Factory.NewWithValidTestData<VoteExamSurveyQuestion>();
			AssertNull("Pre-condition", Collection.FindByQuestion(question));
			AssertNull("Pre-condition", Collection.FindByQuestion(question.PK));

			VoteExamSurveyAnswer answer = Collection.AddNew();
			answer.FillWithValidTestData();
			answer.HZ_HY = question.PK;
			answer.HZ_Answer = "1";
			answer.CampaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			answer.CampaignItem.G8_RecipientID = ZGuid.NewZGuid();
			AssertEquals(answer, Collection.FindByQuestion(question));
			AssertEquals(answer, Collection.FindByQuestion(question.PK));
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbCompanyCampaignItem campaignItemInNewFactory = newFactory.Load<GlbCompanyCampaignItem>(Collection.Master.PK);
			VoteExamSurveyAnswerCollection collectionInNewFactory = new VoteExamSurveyAnswerCollection(campaignItemInNewFactory);
			AssertNull("Collection is not loaded yet", collectionInNewFactory.FindByQuestion(question));
			AssertNull("Collection is not loaded yet", collectionInNewFactory.FindByQuestion(question.PK));

			collectionInNewFactory.Load();
			AssertEquals(answer.PK, collectionInNewFactory.FindByQuestion(question).PK);
			AssertEquals(answer.PK, collectionInNewFactory.FindByQuestion(question.PK).PK);
		}

		public void TestLoadOrCreateNew()
		{
			AssertEquals("Pre-condition", 0, Collection.Count);

			VoteExamSurveyQuestion question = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswer answer = Collection.AddNew();
			answer.HZ_HY = question.PK;
			VoteExamSurveyAnswer loadedAnswer = Collection.LoadOrCreateNew(question);
			AssertEquals("Should not create new answer, should be loaded", 1, Collection.Count);
			AssertEquals(loadedAnswer.PK, answer.PK);

			VoteExamSurveyQuestion newQuestion = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswer newAnswer = Collection.LoadOrCreateNew(newQuestion);
			AssertEquals("Should create a new answer", 2, Collection.Count);
			AssertEquals(newQuestion.PK, newAnswer.HZ_HY);
		}

		public void TestCreateNew()
		{
			AssertEquals("Pre-condition", 0, Collection.Count);

			VoteExamSurveyQuestion question = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswer answer = Collection.CreateNew(question);
			AssertEquals(question.PK, answer.HZ_HY);
		}

		public void TestFind()
		{
			VoteExamSurveyQuestion question = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswer answer = Collection.CreateNew(question);
			VoteExamSurveyAnswer[] answersFound = Collection.Find(new ZQuery(VoteExamSurveyAnswerSchema.HZ_HY, question.PK));
			AssertEquals(1, answersFound.Length);
			AssertEquals(answer, answersFound[0]);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GlbCompanyCampaignItem item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			return new VoteExamSurveyAnswerCollection(item);
		}

		new VoteExamSurveyAnswerCollection Collection
		{
			get { return (VoteExamSurveyAnswerCollection)base.Collection; }
		}

		#endregion
	}
}
