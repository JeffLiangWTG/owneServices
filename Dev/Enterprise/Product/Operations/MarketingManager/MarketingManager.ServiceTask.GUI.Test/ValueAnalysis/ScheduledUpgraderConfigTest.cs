using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MarketingManager.ServiceTask.GUI.Testing
{
	[TestedType(typeof(SalesTradeLanesSyncTaskConfig))]
	internal sealed class ScheduledUpgraderConfigTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSaveAndLoad()
		{
			var taskSchedule = Factory.New<IServiceTaskSchedule>();
			var config = new SalesTradeLanesSyncTaskConfig(taskSchedule);
			AssertEquals(3, config.SyncMonthsOnMainSchedule);
			AssertEquals(true, config.SyncYearly);
			AssertEquals(3, config.SyncYearlyEveryMonths);
			AssertEquals(false, config.SyncYearlyEveryMonths_ReadOnly);

			config.SyncMonthsOnMainSchedule = 5;
			config.SyncYearly = false;
			config.SyncYearlyEveryMonths = 7;
			config.RunYearlyOnDayOfMonth = 20;
			Factory.Save();

			config = new SalesTradeLanesSyncTaskConfig(taskSchedule);
			AssertEquals(5, config.SyncMonthsOnMainSchedule);
			AssertEquals(false, config.SyncYearly);
			AssertEquals(7, config.SyncYearlyEveryMonths);
			AssertEquals(true, config.SyncYearlyEveryMonths_ReadOnly);
			AssertEquals(20, config.RunYearlyOnDayOfMonth);
			AssertEquals(true, config.RunYearlyOnDayOfMonth_ReadOnly);

			config.SyncYearly = true;
			AssertEquals(false, config.SyncYearlyEveryMonths_ReadOnly);
			AssertEquals(false, config.RunYearlyOnDayOfMonth_ReadOnly);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SalesTradeLanesSyncTaskConfig(Factory.New<IServiceTaskSchedule>());
		}

		#endregion
	}
}
