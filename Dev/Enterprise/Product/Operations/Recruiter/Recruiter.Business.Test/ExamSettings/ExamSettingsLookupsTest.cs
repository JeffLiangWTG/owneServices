using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class ExamSettingsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTestCampaigns()
		{
			var settings = Factory.New<ExamSetting>();
			AssertNotNull(settings.Lookups.TestCampaigns);
		}
	}
}
