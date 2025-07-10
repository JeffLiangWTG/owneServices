using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ExamSetting))]
	sealed class ExamSettingsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTestResultsExpireAfterDays()
		{
			var settings = Factory.New<ExamSetting>();
			AssertEquals((short)365, settings.TestResultsExpireAfterDays);

			settings.EXS_TestResultsExpireAfterHours = 240;
			AssertEquals((short)10, settings.TestResultsExpireAfterDays);
		}

		public void TestEXS_Code()
		{
			var settings = Factory.New<ExamSetting>();
			var exam1 = Factory.NewWithValidTestData<LearningCentreCampaign>();
			var exam2 = Factory.NewWithValidTestData<LearningCentreCampaign>();

			settings.EXS_G0 = exam1.PK;
			settings.EXS_ExamVersion = "STD";
			AssertEquals($"{exam1.CampaignID}-STD", settings.EXS_Code);

			settings.EXS_G0 = exam2.PK;
			AssertEquals($"{exam2.CampaignID}-STD", settings.EXS_Code);

			settings.EXS_G0 = exam2.PK;
			settings.EXS_ExamVersion = "ZZZ";
			AssertEquals($"{exam2.CampaignID}-ZZZ", settings.EXS_Code);
		}

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
