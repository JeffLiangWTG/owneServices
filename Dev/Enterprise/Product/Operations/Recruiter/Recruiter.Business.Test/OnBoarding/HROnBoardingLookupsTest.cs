using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	class HROnBoardingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestApplicants()
		{
			var onboarding = Factory.New<HROnBoarding>();
			AssertNotNull("Applicants should not be null", onboarding.Lookups.Applicants);
		}
	}
}
