using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class GlbWorkTimeValidationTest_TestStartAndEndTime : BusinessObjectValidationTestCase
	{
		public void TestPartiallyInside_FirstStartTimeBeforeTheSecondStartTime()
		{
			var glbWorkTime1 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime1.GW_StartTime = CreateTime(8, 30);
			glbWorkTime1.GW_EndTime = CreateTime(10, 30);

			var glbWorkTime2 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime2.GW_StartTime = CreateTime(9, 30);
			glbWorkTime2.GW_EndTime = CreateTime(11, 30);

			AssertHasError("Start time should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertHasError("Start time should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");

			glbWorkTime2.GW_StartTime = CreateTime(11, 0);

			AssertNoError("Start time should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertNoError("Start time should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");
		}

		public void TestPartiallyInside_FirstStartTimeBehindTheSecondStartTime()
		{
			var glbWorkTime1 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime1.GW_StartTime = CreateTime(12, 30);
			glbWorkTime1.GW_EndTime = CreateTime(14, 30);

			var glbWorkTime2 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime2.GW_StartTime = CreateTime(11, 30);
			glbWorkTime2.GW_EndTime = CreateTime(13, 30);

			AssertHasError("End time should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertHasError("End time should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");

			glbWorkTime2.GW_EndTime = CreateTime(12, 0);

			AssertNoError("End time should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertNoError("End time should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");
		}

		public void TestFullyInside_FirstTimeRangeContainsSecondTimeRange()
		{
			var glbWorkTime1 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime1.GW_StartTime = CreateTime(10, 30);
			glbWorkTime1.GW_EndTime = CreateTime(14, 30);

			var glbWorkTime2 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime2.GW_StartTime = CreateTime(11, 30);
			glbWorkTime2.GW_EndTime = CreateTime(13, 30);

			AssertHasError("Time range should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertHasError("Time range should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");

			glbWorkTime2.GW_StartTime = CreateTime(15, 00);
			glbWorkTime2.GW_EndTime = CreateTime(16, 00);

			AssertNoError("Time range should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertNoError("Time range should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");
		}

		public void TestFullyInside_SecondTimeRangeContainsFirstTimeRange()
		{
			var glbWorkTime1 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime1.GW_StartTime = CreateTime(10, 30);
			glbWorkTime1.GW_EndTime = CreateTime(14, 30);

			var glbWorkTime2 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime2.GW_StartTime = CreateTime(9, 30);
			glbWorkTime2.GW_EndTime = CreateTime(15, 30);

			AssertHasError("Work time should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertHasError("Work time should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");

			glbWorkTime2.GW_StartTime = CreateTime(15, 00);

			AssertNoError("Work time should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertNoError("Work time should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");
		}

		public void TestWorkTimeStartsExactlyWhenOtherWorkTimeFinishes()
		{
			var glbWorkTime1 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime1.GW_StartTime = CreateTime(10, 30);
			glbWorkTime1.GW_EndTime = CreateTime(14, 30);

			var glbWorkTime2 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime2.GW_StartTime = CreateTime(14, 30);
			glbWorkTime2.GW_EndTime = CreateTime(15, 30);

			AssertNoErrors("The start of one work time can be the same as the end of another work time.", glbWorkTime2.GW_StartTimeInfo);
			AssertNoErrors("The start of one work time can be the same as the end of another work time.", glbWorkTime2.GW_EndTimeInfo);
		}

		public void TestWorkTimeFinishesExactlyWhenOtherWorkTimeStarts()
		{
			var glbWorkTime1 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime1.GW_StartTime = CreateTime(14, 30);
			glbWorkTime1.GW_EndTime = CreateTime(15, 30);

			var glbWorkTime2 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime2.GW_StartTime = CreateTime(10, 30);
			glbWorkTime2.GW_EndTime = CreateTime(14, 30);

			AssertNoErrors("The end of one work time can be the same as the start of another work time", glbWorkTime2.GW_StartTimeInfo);
			AssertNoErrors("The end of one work time can be the same as the start of another work time", glbWorkTime2.GW_EndTimeInfo);
		}

		public void TestWorkTimeFullyBehindOtherWorkTime()
		{
			var glbWorkTime1 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime1.GW_StartTime = CreateTime(10, 30);
			glbWorkTime1.GW_EndTime = CreateTime(14, 30);

			var glbWorkTime2 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime2.GW_StartTime = CreateTime(10, 30);
			glbWorkTime2.GW_EndTime = CreateTime(15, 30);

			AssertHasError("Start time should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertHasError("Start time should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");

			glbWorkTime2.GW_StartTime = CreateTime(15, 00);

			AssertNoError("Start time should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertNoError("Start time should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");
		}

		public void TestWorkTimeFullyBeforeOtherWorkTime()
		{
			var glbWorkTime1 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime1.GW_StartTime = CreateTime(14, 30);
			glbWorkTime1.GW_EndTime = CreateTime(15, 30);

			var glbWorkTime2 = Factory.NewWithValidTestData<GlbWorkTime>();
			glbWorkTime2.GW_StartTime = CreateTime(13, 30);
			glbWorkTime2.GW_EndTime = CreateTime(15, 30);

			AssertHasError("End time should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertHasError("End time should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");

			glbWorkTime2.GW_EndTime = CreateTime(14, 00);

			AssertNoError("End time should not overlap", glbWorkTime2.GW_StartTimeInfo, "Working times should not overlap.");
			AssertNoError("End time should not overlap", glbWorkTime2.GW_EndTimeInfo, "Working times should not overlap.");
		}

		ZDateTime CreateTime(int hour, int minute) => new ZDateTime(1900, 1, 1, hour, minute, 0);
	}
}