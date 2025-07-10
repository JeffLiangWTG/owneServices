using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class QuestionCategoryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCode()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			QuestionCategory category = new QuestionCategory(campaign);
			category.Validation.ValidateCode();
			AssertMandatoryValidationError(category.CodeInfo, true);

			category.Code = "123";
			AssertMandatoryValidationError(category.CodeInfo, false);
		}

		public void TestDescription()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			QuestionCategory category = new QuestionCategory(campaign);
			category.Validation.ValidateDescription();
			AssertMandatoryValidationError(category.DescriptionInfo, true);

			category.Description = "Meh meh";
			AssertMandatoryValidationError(category.DescriptionInfo, false);
		}
	}
}
