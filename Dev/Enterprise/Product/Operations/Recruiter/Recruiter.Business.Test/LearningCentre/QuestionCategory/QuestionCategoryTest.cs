using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(QuestionCategory))]
	sealed class QuestionCategoryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCodeAndDescription()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			QuestionCategory category = new QuestionCategory(campaign);
			category.Code = "MEH";
			category.Description = "MEH Description";

			AssertEquals("MEH", ((ICodeDescription)category).Code);
			AssertEquals("MEH Description", ((ICodeDescription)category).Description);
		}

		public void TestScaleRanges()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			QuestionCategory category = new QuestionCategory(campaign);
			category.Code = "MEH";
			category.Description = "MEH Description";
			Assert(category.IsRegisteredEditableChildObject(category.ScaleRanges));

			LearningCentreQuestion range1 = category.ScaleRanges.AddNew();
			LearningCentreQuestion range2 = category.ScaleRanges.AddNew();

			category.Code = "BBB";
			AssertEquals(2, category.ScaleRanges.Count);
			AssertEquals("BBB", range1.HY_QuestionCategory);
			AssertEquals("BBB", range2.HY_QuestionCategory);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new QuestionCategory(Factory.New<LearningCentreCampaign>());
		}

		#endregion
	}
}
