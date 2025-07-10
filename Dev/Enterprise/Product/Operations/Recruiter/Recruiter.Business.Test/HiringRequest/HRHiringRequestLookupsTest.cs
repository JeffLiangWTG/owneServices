using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRHiringRequestLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestApplicants()
		{
			HRHiringRequest hiringRequest = Factory.New<HRHiringRequest>();
			AssertNotNull("Applicants should not be null", hiringRequest.Lookups.Applicants);
		}

		public void TestTeams()
		{
			HRHiringRequest hiringRequest = Factory.New<HRHiringRequest>();
			AssertNotNull("Teams should not be null", hiringRequest.Lookups.Teams);
		}
	}
}
