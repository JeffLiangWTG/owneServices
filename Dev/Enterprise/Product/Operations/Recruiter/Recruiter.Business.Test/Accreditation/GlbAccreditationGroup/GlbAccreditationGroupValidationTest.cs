using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class GlbAccreditationGroupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckHAG_Description()
		{
			var accreditationGroup = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			accreditationGroup.HAG_Description = string.Empty;
			AssertHasError("Mandatory validation should prompt user to enter value", accreditationGroup.HAG_DescriptionInfo, "Please enter a value.");

			accreditationGroup.HAG_Description = "CCLP";
			AssertNoErrors(accreditationGroup.HAG_DescriptionInfo);
			Factory.Save();

			var accreditationGroup2 = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			accreditationGroup2.HAG_Description = "CCLP";
			AssertHasError("Should not be able to have duplicate descriptions", accreditationGroup2.HAG_DescriptionInfo, "Description is not unique. Please enter a unique description.");
		}
	}
}
