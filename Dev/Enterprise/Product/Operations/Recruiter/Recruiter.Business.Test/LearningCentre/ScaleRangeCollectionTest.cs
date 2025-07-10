using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ScaleRangeCollection))]
	sealed class ScaleRangeCollectionTest : ActiveBusinessObjectCollectionTestCase<ScaleRangeCollection>
	{
		public void TestNewElement()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			QuestionCategory category = new QuestionCategory(campaign);
			category.Code = "AAA";
			ScaleRangeCollection collection = new ScaleRangeCollection(category);
			LearningCentreQuestion scaleRange = collection.AddNew();
			AssertEquals("AAA", scaleRange.HY_QuestionCategory);
			AssertEquals(LearningCentreAnswerTypeList.Codes.ScaleRange, scaleRange.HY_AnswerType);
		}

		public void TestRelationshipFilter()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			LearningCentreQuestion question1 = campaign.Questions.AddNew();
			question1.HY_AnswerType = LearningCentreAnswerTypeList.Codes.ScaleRange;
			question1.HY_QuestionCategory = "A";
			LearningCentreQuestion question2 = campaign.Questions.AddNew();
			question2.HY_AnswerType = LearningCentreAnswerTypeList.Codes.ScaleRange;
			question2.HY_QuestionCategory = "A";
			LearningCentreQuestion question3 = campaign.Questions.AddNew();
			question3.HY_AnswerType = LearningCentreAnswerTypeList.Codes.ScaleRange;
			question3.HY_QuestionCategory = "B";

			QuestionCategory category = new QuestionCategory(campaign);
			category.Code = "A";
			ScaleRangeCollection collection = new ScaleRangeCollection(category);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(question1, collection);
			AssertCollectionContains(question2, collection);

			category = new QuestionCategory(campaign);
			category.Code = "B";
			collection = new ScaleRangeCollection(category);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(question3, collection);
		}

		protected override ScaleRangeCollection GetCollectionToTest()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			return campaign.QuestionCategories[0].ScaleRanges;
		}
	}
}
