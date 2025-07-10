using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TimeInfoTest : TestCaseWithFactory
	{
		public void TestSpanExcludingHolidays()
		{
			var info = new TimeInfo(new ZDateTime(2005, 1, 1, 22, 0, 0), new ZDateTime(2005, 1, 8, 4, 15, 0));

			AssertEquals(8, (int)info.Span.TotalDays);
			AssertEquals(5, (int)info.SpanExcluding(TimeInfo.Exclusion.Weekends | TimeInfo.Exclusion.PublicHolidays).TotalDays);

			GlbHoliday holiday = GlbBranch.CurrentBranch.GlbHolidays.AddNew();
			try
			{
				holiday.GH_Date = new ZDateTime(2005, 1, 7);

				info = new TimeInfo(new ZDateTime(2005, 1, 1), new ZDateTime(2005, 1, 8));
				AssertEquals(4, (int)info.SpanExcluding(TimeInfo.Exclusion.WeekendsPublicHolidays).TotalDays);
				AssertEquals(5, (int)info.SpanExcluding(TimeInfo.Exclusion.Weekends).TotalDays);
				AssertEquals(6, (int)info.SpanExcluding(TimeInfo.Exclusion.Sundays | TimeInfo.Exclusion.PublicHolidays).TotalDays);
			}
			finally
			{
				GlbBranch.CurrentBranch.GlbHolidays.RemoveFromRelationship(holiday);
			}
		}

		public void TestToString()
		{
			var info = new TimeInfo(new ZDateTime(2005, 1, 1, 22, 0, 0), new ZDateTime(2005, 1, 8, 3, 0, 50));

			AssertEquals("8 days (01-Jan-05 - 08-Jan-05)", info.ToString(0));
			AssertEquals("5 days (01-Jan-05 - 08-Jan-05)", info.ToString(TimeInfo.Exclusion.WeekendsPublicHolidays));

			info = new TimeInfo(6, 13, 0);
			AssertEquals("7 days", info.ToString(0));

			info = new TimeInfo(6, 11, 0);
			AssertEquals("7 days", info.ToString(0));
		}

		public void TestUseFreeDays()
		{
			GlbHoliday holiday = GlbBranch.CurrentBranch.GlbHolidays.AddNew();
			try
			{
				holiday.GH_Date = new ZDateTime(2025, 1, 1);

				var infoWithoutFreeDays = new TimeInfo(new ZDateTime(2025, 1, 1, 22, 0, 0), new ZDateTime(2025, 1, 8, 4, 15, 0));

				AssertEquals(8, (int)infoWithoutFreeDays.Span.TotalDays);
				AssertEquals(7, (int)infoWithoutFreeDays.SpanExcluding(TimeInfo.Exclusion.PublicHolidays).TotalDays);
				AssertEquals(6, (int)infoWithoutFreeDays.SpanExcluding(TimeInfo.Exclusion.Weekends).TotalDays);
				AssertEquals(5, (int)infoWithoutFreeDays.SpanExcluding(TimeInfo.Exclusion.Weekends | TimeInfo.Exclusion.PublicHolidays).TotalDays);

				var infoWithFreeDays = new TimeInfo(new ZDateTime(2025, 1, 1, 22, 0, 0), new ZDateTime(2025, 1, 8, 4, 15, 0), freeDays: 6);

				AssertEquals(8, (int)infoWithFreeDays.Span.TotalDays);
				AssertEquals(1, (int)infoWithFreeDays.SpanExcluding(TimeInfo.Exclusion.PublicHolidays).TotalDays);
				AssertEquals(0, infoWithFreeDays.RemainingFreeDays);
				AssertEquals(0, (int)infoWithFreeDays.SpanExcluding(TimeInfo.Exclusion.Weekends).TotalDays);
				AssertEquals(0, infoWithFreeDays.RemainingFreeDays);
				AssertEquals(0, (int)infoWithFreeDays.SpanExcluding(TimeInfo.Exclusion.Weekends | TimeInfo.Exclusion.PublicHolidays).TotalDays);
				AssertEquals(1, infoWithFreeDays.RemainingFreeDays);
			}
			finally
			{
				GlbBranch.CurrentBranch.GlbHolidays.RemoveFromRelationship(holiday);
			}
		}
	}
}
