using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Scheduler.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class ScheduleTaskHelperTest : TestCaseWithFactory
	{
		public void TestGetScheduleTaskByScheduleType()
		{
			var scheduleType = "TST";

			var expectedTask = Factory.NewWithValidTestData<StmScheduleTask>();
			expectedTask.S5_ScheduleType = scheduleType;

			Factory.Save();

			var actualTask = ScheduleTaskHelper.GetScheduleTaskByScheduleType(scheduleType);

			Assert("Task is not null", actualTask != null);
		}

		public void TestGetNextScheduleServiceTaskDateTime()
		{
			var nextScheduleServiceTaskDateTime = ZDateTime.UtcNow;

			var task = Factory.New<StmScheduleTask>();
			task.S5_IsActive = false;
			task.S5_NextScheduledPrintRunTimeUtc = nextScheduleServiceTaskDateTime;

			var expected = ZDateTime.Empty;
			var actual = ScheduleTaskHelper.GetNextScheduleServiceTaskDateTime(task);

			AssertEquals("S5_NextScheduledPrintRunTimeUtc", expected, actual);

			task.S5_IsActive = true;

			expected = nextScheduleServiceTaskDateTime;
			actual = ScheduleTaskHelper.GetNextScheduleServiceTaskDateTime(task);

			AssertEquals("S5_NextScheduledPrintRunTimeUtc", expected, actual);
		}
	}
}
