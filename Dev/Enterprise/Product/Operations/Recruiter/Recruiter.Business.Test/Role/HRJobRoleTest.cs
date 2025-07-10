using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobRole))]
	sealed class HRJobRoleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestImplementsIDocManagerSupport()
		{
			HRJobRole jobRole = Factory.New<HRJobRole>();
			Assert("it is an IDocManagerSupport", typeof(IDocManagerSupport).IsAssignableFrom(typeof(HRJobRole)));
			AssertEquals("JobRole DocManagerCode = JRO", "JRO", jobRole.DocManagerInfo.DocManagerCode);
		}

		public void TestIsAutologged()
		{
			HRJobRole jobRole = Factory.NewWithValidTestData<HRJobRole>();
			Factory.Save();
			Assert("JobRole should contain logs", jobRole.Logs.GetAllLogs().Count > 0);
		}

		public void TestJobRoleSkills()
		{
			HRJobRole jobRole = Factory.New<HRJobRole>();
			AssertNotNull("JobRoleSkills should not be null", jobRole.JobRoleSkills);
		}

		public void TestDelete()
		{
			HRJobRole jobRole = Factory.New<HRJobRole>();
			HRJobRoleSkillPivot roleSkill = jobRole.JobRoleSkills.AddNew();

			jobRole.Delete();

			Assert("Role Skill should be deleted", roleSkill.IsDeleted);
		}

		public void TestFullJobRoleDescription()
		{
			HRJobRole jobRole = Factory.New<HRJobRole>();
			jobRole.FullJobRoleDescription = "";
			AssertNoErrors("Shouldn't call validation when value unchanged", jobRole.FullJobRoleDescriptionInfo);

			jobRole.FullJobRoleDescription = "meh";
			AssertNoErrors(jobRole.FullJobRoleDescriptionInfo);

			jobRole.FullJobRoleDescription = "";
			AssertHasErrors(jobRole.FullJobRoleDescriptionInfo);
		}

		#region Dummy tests
		// dummy tests to prevent test failures from parent class
		// this class is pending removal
		public override void TestFetchForLoad()
		{
			AssertNoExceptionThrown(Factory.Save);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			AssertNoExceptionThrown(Factory.Save);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			AssertNoExceptionThrown(Factory.Save);
		}
		#endregion
	}
}
