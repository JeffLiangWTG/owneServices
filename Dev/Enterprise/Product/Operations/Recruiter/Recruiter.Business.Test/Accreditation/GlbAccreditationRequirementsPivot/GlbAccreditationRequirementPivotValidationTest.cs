using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class GlbAccreditationRequirementPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckHAR_HAC_Parent()
		{
			var accred1 = Factory.NewWithValidTestData<GlbAccreditation>();
			var accred2 = Factory.NewWithValidTestData<GlbAccreditation>();

			var pivot = Factory.NewWithValidTestData<GlbAccreditationRequirementPivot>();

			pivot.HAR_HAC = accred1.PK;
			pivot.HAR_HAC_Parent = accred1.PK;
			AssertHasError("Should have errors", pivot.HAR_HAC_ParentInfo, "Cannot have itself as a requirement");

			pivot.HAR_HAC_Parent = accred2.PK;
			AssertNoError("Should have no errors", pivot.HAR_HAC_ParentInfo, "Cannot have itself as a requirement");
		}

		public void TestCheckHAR_HAC()
		{
			var accred1 = Factory.NewWithValidTestData<GlbAccreditation>();
			var accred2 = Factory.NewWithValidTestData<GlbAccreditation>();

			var pivot = Factory.NewWithValidTestData<GlbAccreditationRequirementPivot>();

			pivot.HAR_HAC_Parent = accred1.PK;
			pivot.HAR_HAC = accred1.PK;
			AssertHasError("Should have errors", pivot.HAR_HACInfo, "Cannot have itself as a requirement");

			pivot.HAR_HAC = accred2.PK;
			AssertNoError("Should have no errors", pivot.HAR_HACInfo, "Cannot have itself as a requirement");
		}
	}
}
