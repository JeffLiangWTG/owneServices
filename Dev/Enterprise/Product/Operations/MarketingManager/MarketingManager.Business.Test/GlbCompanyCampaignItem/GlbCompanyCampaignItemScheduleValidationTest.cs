using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignItemScheduleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestScheduleSendTime()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItemSchedule scheduler = new GlbCompanyCampaignItemSchedule(campaign);
			scheduler.HasChanges = true;
			scheduler.Validation.ValidateScheduleSendTimeLocal();
			AssertHasError(scheduler.ScheduleSendTimeLocalInfo, "Please enter a value.");

			scheduler.ScheduleSendTimeLocal = ZDateTime.UtcNow;
			scheduler.Validation.ValidateScheduleSendTimeLocal();
			AssertNoErrors(scheduler.ScheduleSendTimeLocalInfo);
		}

		public void TestScheduleSenderTimeZone()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItemSchedule scheduler = new GlbCompanyCampaignItemSchedule(campaign);
			scheduler.SenderTimeZone = "";
			scheduler.Validation.ValidateSenderTimeZone();
			AssertHasError(scheduler.SenderTimeZoneInfo, "Please enter a value.");

			scheduler.SenderTimeZone = Env.CurrentBranch.NKUNLOCO;
			scheduler.Validation.ValidateSenderTimeZone();
			AssertNoErrors(scheduler.SenderTimeZoneInfo);
		}
	}
}
