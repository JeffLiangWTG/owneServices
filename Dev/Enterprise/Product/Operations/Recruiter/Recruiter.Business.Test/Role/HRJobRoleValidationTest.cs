using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobRoleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckHJ_JobTitle()
		{
			HRJobRole jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = ZString.Empty;
			AssertHasErrors("Job role title should be mandatory", jobRole.HJ_JobTitleInfo);
			jobRole.HJ_JobTitle = "blah";
			AssertNoErrors("Job role title should be mandatory", jobRole.HJ_JobTitleInfo);
		}

		public void TestCheckHJ_JobRoleDescription()
		{
			HRJobRole jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobRoleDescription = ZString.Empty;
			AssertHasErrors("Job role description should be mandatory", jobRole.HJ_JobRoleDescriptionInfo);
			jobRole.HJ_JobRoleDescription = "blah";
			AssertNoErrors("Job role description should be mandatory", jobRole.HJ_JobRoleDescriptionInfo);
		}

		public void TestCheckFullJobRoleDescription()
		{
			HRJobRole jobRole = Factory.New<HRJobRole>();
			jobRole.FullJobRoleDescription = "meh mhe";
			AssertNoErrors(jobRole.FullJobRoleDescriptionInfo);

			jobRole.FullJobRoleDescription = "";
			AssertMandatoryValidationError(jobRole.FullJobRoleDescriptionInfo, true);
		}
	}
}
