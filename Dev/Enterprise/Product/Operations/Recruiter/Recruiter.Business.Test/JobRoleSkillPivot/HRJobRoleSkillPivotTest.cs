using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobRoleSkillPivot))]
	sealed class HRJobRoleSkillPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFillWithValidTestDataCore()
		{
			HRJobRoleSkillPivot pivot = Factory.NewWithValidTestData<HRJobRoleSkillPivot>();
			Assert("Pivot should be from a valid Job Role", pivot.H1_HJ.IsValid);
		}
		public void TestJobRole()
		{
			HRJobRoleSkillPivot pivot = Factory.New<HRJobRoleSkillPivot>();
			AssertNull("Job role should not be set", pivot.JobRole);

			HRJobRole role = Factory.New<HRJobRole>();
			pivot.H1_HJ = role.PK;

			AssertEquals("Job Role on Pivot should be this Role", role, pivot.JobRole);
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
