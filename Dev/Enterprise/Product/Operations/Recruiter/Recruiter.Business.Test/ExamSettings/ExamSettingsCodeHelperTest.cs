using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class ExamSettingsCodeHelperTest : TestCaseWithFactory
	{
		public void TestGetExamSettingsCode()
		{
			var factory = new BusinessObjectFactory();
			var helper = new ExamSettingCodeHelper();

			AssertEquals("", helper.GetExamSettingsCode(null, "", factory));

			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignID = "12345";
			var settings1 = campaign.ExamSettingCollection.AddNew();
			settings1.EXS_IsDefault = false;
			settings1.EXS_ExamVersion = "111";

			AssertEquals("", helper.GetExamSettingsCode(campaign, "", factory));

			var settings2 = campaign.ExamSettingCollection.AddNew();
			settings2.EXS_IsDefault = true;
			settings2.EXS_ExamVersion = "222";

			AssertEquals("12345-222", helper.GetExamSettingsCode(campaign, "", factory));
		}

		public void TestGetExamVersion()
		{
			var factory = new BusinessObjectFactory();
			var helper = new ExamSettingCodeHelper();
			AssertEquals("STD", helper.GetExamVersion(null, ZGuid.Empty, factory));
			AssertEquals("STD", helper.GetExamVersion("", ZGuid.Empty, factory));

			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			var settings1 = campaign.ExamSettingCollection.AddNew();
			settings1.EXS_IsDefault = false;
			settings1.EXS_ExamVersion = "111";

			var settings2 = campaign.ExamSettingCollection.AddNew();
			settings2.EXS_IsDefault = true;
			settings2.EXS_ExamVersion = "222";

			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			item.G8_RecipientID = ZGuid.NewZGuid();

			Factory.Save();

			AssertEquals("222", helper.GetExamVersion(null, item.PK, factory));
			AssertEquals("222", helper.GetExamVersion("", item.PK, factory));

			AssertEquals("STD", helper.GetExamVersion(null, ZGuid.NewZGuid(), factory));
			AssertEquals("STD", helper.GetExamVersion("", ZGuid.NewZGuid(), factory));

			AssertEquals("111", helper.GetExamVersion(settings1.EXS_Code, item.PK, factory));
			AssertEquals("222", helper.GetExamVersion(settings2.EXS_Code, item.PK, factory));
			AssertEquals("111", helper.GetExamVersion(settings1.EXS_Code, ZGuid.NewZGuid(), factory));
			AssertEquals("222", helper.GetExamVersion(settings2.EXS_Code, ZGuid.NewZGuid(), factory));
		}
	}
}
