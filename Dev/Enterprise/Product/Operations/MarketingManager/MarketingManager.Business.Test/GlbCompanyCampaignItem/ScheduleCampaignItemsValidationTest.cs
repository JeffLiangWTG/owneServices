using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class ScheduleCampaignItemsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestScheduleSendTimeUTC()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			ScheduleCampaignItems scheduleCampaignItem = new ScheduleCampaignItems(campaign);
			scheduleCampaignItem.Status = "SNT";
			scheduleCampaignItem.Validation.ValidateScheduleSendTimeUTC();
			AssertNoErrors(scheduleCampaignItem.ScheduleSendTimeUTCInfo);

			scheduleCampaignItem.Status = "QUE";
			scheduleCampaignItem.Validation.ValidateScheduleSendTimeUTC();
			AssertHasError(scheduleCampaignItem.ScheduleSendTimeUTCInfo, "Please enter a value.");

			scheduleCampaignItem.ScheduleSendTimeUTC = ZDateTime.UtcNow.AddDays(-1);
			scheduleCampaignItem.Validation.ValidateScheduleSendTimeUTC();
			AssertHasError(scheduleCampaignItem.ScheduleSendTimeUTCInfo, "Date cannot be set behind the current UTC time.");

			scheduleCampaignItem.ScheduleSendTimeUTC = ZDateTime.UtcNow.AddDays(42);
			scheduleCampaignItem.Validation.ValidateScheduleSendTimeUTC();
			AssertHasError(scheduleCampaignItem.ScheduleSendTimeUTCInfo, "Date exceeds allowable limit of 40 days.");

			scheduleCampaignItem.ScheduleSendTimeUTC = ZDateTime.UtcNow.AddDays(8);
			scheduleCampaignItem.Validation.ValidateScheduleSendTimeUTC();
			AssertNoErrors(scheduleCampaignItem.ScheduleSendTimeUTCInfo);
		}

		public void TestScheduleSendTimeLocal()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			ScheduleCampaignItems scheduleCampaignItem = new ScheduleCampaignItems(campaign);
			scheduleCampaignItem.Validation.ValidateScheduleSendTimeLocal();
			AssertHasError(scheduleCampaignItem.ScheduleSendTimeLocalInfo, "Please enter a value.");

			scheduleCampaignItem.ScheduleSendTimeLocal = ZDateTime.UtcNow;
			scheduleCampaignItem.Validation.ValidateScheduleSendTimeLocal();
			AssertNoErrors(scheduleCampaignItem.ScheduleSendTimeLocalInfo);
		}
	}
}
