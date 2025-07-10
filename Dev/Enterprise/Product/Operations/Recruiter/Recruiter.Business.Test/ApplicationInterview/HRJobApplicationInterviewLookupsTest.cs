using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobApplicationInterviewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInterviewStatuses()
		{
			HRJobApplicationInterview interview = Factory.New<HRJobApplicationInterview>();
			AssertNotNull("Interview statuses should not be null", interview.Lookups.InterviewStatuses);
		}
	}
}
