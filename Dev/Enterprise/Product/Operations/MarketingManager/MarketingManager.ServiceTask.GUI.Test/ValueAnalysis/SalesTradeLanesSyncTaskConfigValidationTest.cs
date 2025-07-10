using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.ServiceTask.GUI.Testing
{
	internal sealed class SalesTradeLanesSyncTaskConfigValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSyncMonthsOnMainSchedule()
		{
			var config = new SalesTradeLanesSyncTaskConfig("");
			AssertNoErrors(config.SyncMonthsOnMainScheduleInfo);

			config.SyncMonthsOnMainSchedule = 0;
			AssertHasError("Should have an error", config.SyncMonthsOnMainScheduleInfo, "The value should be between 1 and 24 months");

			config.SyncMonthsOnMainSchedule = 1;
			AssertNoErrors(config.SyncMonthsOnMainScheduleInfo);

			config.SyncMonthsOnMainSchedule = 3;
			AssertNoErrors(config.SyncMonthsOnMainScheduleInfo);

			config.SyncMonthsOnMainSchedule = 5;
			AssertNoErrors(config.SyncMonthsOnMainScheduleInfo);

			config.SyncMonthsOnMainSchedule = 10;
			AssertNoErrors(config.SyncMonthsOnMainScheduleInfo);

			config.SyncMonthsOnMainSchedule = 18;
			AssertNoErrors(config.SyncMonthsOnMainScheduleInfo);

			config.SyncMonthsOnMainSchedule = 25;
			AssertHasError("Should have an error", config.SyncMonthsOnMainScheduleInfo, "The value should be between 1 and 24 months");

			config.SyncMonthsOnMainSchedule = 35;
			AssertHasError("Should have an error", config.SyncMonthsOnMainScheduleInfo, "The value should be between 1 and 24 months");
		}

		public void TestSyncYearlyEveryMonths()
		{
			var config = new SalesTradeLanesSyncTaskConfig("");
			AssertNoErrors(config.SyncYearlyEveryMonthsInfo);

			config.SyncYearlyEveryMonths = 0;
			AssertHasError("Should have an error", config.SyncYearlyEveryMonthsInfo, "The value should be between 1 and 12 months");

			config.SyncYearlyEveryMonths = 1;
			AssertNoErrors(config.SyncYearlyEveryMonthsInfo);

			config.SyncYearlyEveryMonths = 3;
			AssertNoErrors(config.SyncYearlyEveryMonthsInfo);

			config.SyncYearlyEveryMonths = 5;
			AssertNoErrors(config.SyncYearlyEveryMonthsInfo);

			config.SyncYearlyEveryMonths = 10;
			AssertNoErrors(config.SyncYearlyEveryMonthsInfo);

			config.SyncYearlyEveryMonths = 13;
			AssertHasError("Should have an error", config.SyncYearlyEveryMonthsInfo, "The value should be between 1 and 12 months");

			config.SyncYearlyEveryMonths = 18;
			AssertHasError("Should have an error", config.SyncYearlyEveryMonthsInfo, "The value should be between 1 and 12 months");
		}
	}
}
