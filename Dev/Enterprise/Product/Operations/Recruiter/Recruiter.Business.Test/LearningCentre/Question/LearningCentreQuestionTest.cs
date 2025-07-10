using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreQuestion))]
	sealed class LearningCentreQuestionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCorrectAnswerList()
		{
			AssertEquals(Question.SubQuestions, MetaData.GetListDataSource(Question, Question.HY_ExamCorrectAnswerInfo.PropertyDescriptor));
		}

		public void TestCorrectAnswerAsByte()
		{
			AssertEquals(ZByte.Zero, Question.CorrectAnswerAsByte);

			Question.HY_ExamCorrectAnswer = "100";
			AssertEquals((ZByte)100, Question.CorrectAnswerAsByte);

			Question.HY_ExamCorrectAnswer = "lkj";
			AssertEquals(ZByte.Zero, Question.CorrectAnswerAsByte);
		}

		public void TestCorrectAnswerAsBool()
		{
			AssertEquals("Should be false when Answer is not specified", false, Question.CorrectAnswerAsBool);

			Question.HY_ExamCorrectAnswer = ZBool.True.ToString();
			AssertEquals(true, Question.CorrectAnswerAsBool);

			Question.HY_ExamCorrectAnswer = ZBool.False.ToString();
			AssertEquals(false, Question.CorrectAnswerAsBool);

			Question.HY_ExamCorrectAnswer = "meh";
			AssertEquals("Should be false when Answer is not a valid boolean", false, Question.CorrectAnswerAsBool);

			Question.CorrectAnswerAsBool = true;
			AssertEquals(ZBool.True.ToString(), Question.HY_ExamCorrectAnswer);

			Question.CorrectAnswerAsBool = false;
			AssertEquals("", Question.HY_ExamCorrectAnswer);
		}

		public void TestPropertyTypes()
		{
			Question.HY_G0 = Factory.New<LearningCentreCampaign>().PK;
			AssertEquals(typeof(LearningCentreSubQuestionCollection), Question.SubQuestions.GetType());
			AssertEquals(typeof(LearningCentreSubmittedAnswerCollection), Question.SubmittedAnswers.GetType());
			AssertEquals(typeof(LearningCentreCampaign), Question.Campaign.GetType());
			AssertEquals(typeof(LearningCentreQuestionLookups), Question.Lookups.GetType());
			AssertEquals(typeof(LearningCentreQuestionValidation), Question.Validation.GetType());
		}

		public void TestSetDefaultCategory()
		{
			AssertEquals("DEF", Question.HY_QuestionCategory);

			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			campaign.QuestionCategories.RemoveAndDeleteAll();
			campaign.QuestionCategories.AddNew("MEH", "sdf;kl");
			LearningCentreQuestion question = campaign.Questions.AddNew();
			AssertEquals("MEH", question.HY_QuestionCategory);
		}

		public void TestQuestionCategoryList()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question = campaign.Questions.AddNew();
			AssertEquals(campaign.QuestionCategories, MetaData.GetListDataSource(question, question.HY_QuestionCategoryInfo.PropertyDescriptor));
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Cannot delete saved question", true);
		}

		LearningCentreQuestion Question
		{
			get { return (LearningCentreQuestion)CachedBusinessObject; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			return campaign.Questions.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<LearningCentreQuestionForTest>();
		}

		class LearningCentreQuestionForTest : LearningCentreQuestion
		{
			public LearningCentreQuestionForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool ForceAllowDeleteForTest => true;
		}
	}
}
