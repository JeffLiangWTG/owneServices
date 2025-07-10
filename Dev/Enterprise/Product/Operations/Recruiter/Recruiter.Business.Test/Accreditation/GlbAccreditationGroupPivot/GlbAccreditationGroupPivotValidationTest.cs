using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class GlbAccreditationGroupPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckHAP_HAC()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();

			var pivot = Factory.New<GlbAccreditationGroupPivot>();

			pivot.Validation.ValidateAll();
			AssertHasError(pivot.HAP_HACInfo, "Please enter a value.");

			pivot.HAP_HAC = accreditation.PK;
			AssertNoErrors(pivot.HAP_HACInfo);
		}

		public void TestCheckHAP_HAG()
		{
			var group = Factory.NewWithValidTestData<GlbAccreditationGroup>();

			var pivot = Factory.New<GlbAccreditationGroupPivot>();

			pivot.Validation.ValidateAll();
			AssertHasError(pivot.HAP_HAGInfo, "Please enter a value.");

			pivot.HAP_HAG = group.PK;
			AssertNoErrors(pivot.HAP_HAGInfo);
		}
	}
}
