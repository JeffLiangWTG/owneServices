using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreSubQuestionCollection))]
	sealed class LearningCentreSubQuestionCollectionTest : ActiveBusinessObjectCollectionTestCase<LearningCentreSubQuestionCollection>
	{
		public void TestIndexAndAddNew()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question = campaign.Questions.AddNew();
			LearningCentreQuestion subQuestion = question.SubQuestions.AddNew();
			AssertEquals(subQuestion, question.SubQuestions[0]);
		}

		protected override LearningCentreSubQuestionCollection GetCollectionToTest()
		{
			return ParentQuestion.SubQuestions;
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			VoteExamSurveyQuestion result = (VoteExamSurveyQuestion)base.GetNewElementToAddToTheCollection();
			result.HY_QuestionOrder = ParentQuestion.HY_QuestionOrder;
			result.HY_SubQuestionOrder = 1;
			return result;
		}

		LearningCentreQuestion parentQuestion;
		LearningCentreQuestion ParentQuestion
		{
			get { return parentQuestion ?? (parentQuestion = Factory.New<LearningCentreCampaign>().Questions.AddNew()); }
		}
	}
}
