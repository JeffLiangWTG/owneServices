using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignSendScheduleTaskValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateS5_NextScheduledPrintRunTime()
		{
			var sendSettings = Factory.New<GlbCompanyCampaignSendSettings>();
			var task = Factory.New<GlbCompanyCampaignSendScheduleTask>();
			task.S5_ParentID = sendSettings.PK;

			sendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.DEL;
			task.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Empty;
			AssertNoErrors(task.S5_NextScheduledPrintRunTimeUtcInfo);

			sendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.BAT;
			task.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Empty;
			AssertHasErrors(task.S5_NextScheduledPrintRunTimeUtcInfo);

			task.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now;
			AssertNoErrors(task.S5_NextScheduledPrintRunTimeUtcInfo);
		}
	}
}
