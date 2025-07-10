using CargoWise.EntityFramework.Testing;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruiter.Module.Testing
{
	sealed class GlbAccreditationLatestAttemptForGroupFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAccreditationGroupDescription()
		{
			var group = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			group.HAG_Description = "CCLA";
			Factory.Save();
			AssertEquals("Precondition", "CCLA", group.HAG_Description);
			var filter = new GlbAccreditationHighestLevelByProgramFilter("Dummy");
			filter.AccreditationGroupDescription = string.Empty;
			filter.Validation.ValidateAccreditationGroupDescription();
			AssertNoErrors("Empty group allowed", filter.AccreditationGroupDescriptionInfo);
			filter.AccreditationGroupDescription = "blah";
			AssertHasError("Invalid group not allowed", filter.AccreditationGroupDescriptionInfo, "Enter a valid selection.");
			filter.AccreditationGroupDescription = group.HAG_Description;
			AssertNoErrors("Valid group", filter.AccreditationGroupDescriptionInfo);
		}

		public void TestValidateAll()
		{
			var filter = new GlbAccreditationHighestLevelByProgramFilter("Dummy");
			var validation = new GlbAccreditationLatestAttemptForGroupFilterValidationForValidateAllTest(filter);
			AssertEquals("Precondition", false, validation.CheckAccreditationGroupDescriptionWasCalled);
			validation.ValidateAll();
			AssertEquals("Should have been called", true, validation.CheckAccreditationGroupDescriptionWasCalled);
		}

		class GlbAccreditationLatestAttemptForGroupFilterValidationForValidateAllTest : GlbAccreditationLatestAttemptForGroupFilterValidation
		{
			public GlbAccreditationLatestAttemptForGroupFilterValidationForValidateAllTest(GlbAccreditationHighestLevelByProgramFilter parent) : base(parent)
			{
			}

			public bool CheckAccreditationGroupDescriptionWasCalled;
			protected override void CheckAccreditationGroupDescription()
			{
				CheckAccreditationGroupDescriptionWasCalled = true;
			}
		}
	}
}
