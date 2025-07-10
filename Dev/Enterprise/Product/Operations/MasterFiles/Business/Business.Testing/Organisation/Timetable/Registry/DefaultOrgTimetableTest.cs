using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultOrgTimetable))]
	sealed class DefaultOrgTimetableTest : RegistryBusinessObjectTemplateTestCase<DefaultOrgTimetable>
	{
		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return true;
			}
		}

		public void TestOverlapTimeframe()
		{
			var t1 = new DefaultOrgTimetable();
			t1.Type = OrgTimetableType.Codes.Pickup;
			t1.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 8, 0, 0);
			t1.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 10, 0, 0);
			t1.Day = "MON";

			var t2 = new DefaultOrgTimetable();
			t2.Type = OrgTimetableType.Codes.Pickup;
			t2.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 8, 0, 0);
			t2.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 11, 0, 0);
			t2.Day = "MON";

			var t3 = new DefaultOrgTimetable();
			t3.Type = OrgTimetableType.Codes.Deliver;
			t3.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 8, 0, 0);
			t3.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 11, 0, 0);
			t3.Day = "MON";

			var t4 = new DefaultOrgTimetable();
			t4.Type = OrgTimetableType.Codes.Pickup;
			t4.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 8, 0, 0);
			t4.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 14, 0, 0);
			t4.Day = "TUE";

			var t5 = new DefaultOrgTimetable();
			t5.Type = OrgTimetableType.Codes.Pickup;
			t5.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 12, 0, 0);
			t5.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 14, 0, 0);
			t5.Day = "MON";

			AssertEquals(true, t1.OverlapTimeframe(t2));
			AssertEquals(true, t2.OverlapTimeframe(t1));
			AssertEquals(false, t1.OverlapTimeframe(t3));
			AssertEquals(false, t1.OverlapTimeframe(t4));
			AssertEquals(false, t1.OverlapTimeframe(t5));

			t2.Day = "TUE";
			AssertEquals(false, t1.OverlapTimeframe(t2));

			t2.Day = "MON";
			AssertEquals(true, t1.OverlapTimeframe(t2));

			t2.Type = OrgTimetableType.Codes.Deliver;
			AssertEquals(false, t1.OverlapTimeframe(t2));
		}

		public void TestIsCutOffOrProcessingTimeExisted()
		{
			var t1 = new DefaultOrgTimetable();
			t1.Type = OrgTimetableType.Codes.Deliver;
			t1.From = new ZDateTime(2024, 1, 1, 8, 0, 0);
			t1.To = new ZDateTime(2024, 1, 1, 10, 0, 0);
			t1.Day = "MON";
			t1.CutOffTime = new ZDateTime(ZDateTime.Today.Year, 1, 2, 10, 10, 0);

			var t2 = new DefaultOrgTimetable();
			t2.Type = OrgTimetableType.Codes.Deliver;
			t2.From = new ZDateTime(2024, 1, 1, 11, 0, 0);
			t2.To = new ZDateTime(2024, 1, 1, 15, 0, 0);
			t2.Day = "MON";

			t2.CutOffTime = new ZDateTime(ZDateTime.Today.Year, 1, 2, 10, 10, 0);
			Assert(t1.IsCutOffTimeExisted(t2));

			t1.ProcessingTimeInHours = new ZDateTime(ZDateTime.Today.Year, 1, 2, 10, 10, 0);
			t2.ProcessingTimeInHours = new ZDateTime(ZDateTime.Today.Year, 1, 2, 10, 10, 0);
			Assert(t1.IsProcessingTimeExisted(t2));
		}

		public void TestSetProcessingTimeInHours()
		{
			var timetable = new DefaultOrgTimetable();
			AssertEquals(timetable.ProcessingTime, 0);

			timetable.ProcessingTimeInHours = new ZDateTime(ZDateTime.Today.Year, 1, 1, 0, 10, 0);
			AssertEquals(timetable.ProcessingTime, 10);
			AssertNoErrors(timetable.ProcessingTimeInHoursInfo);

			timetable.ProcessingTimeInHours = new ZDateTime(ZDateTime.Today.Year, 1, 1, 10, 10, 0);
			AssertEquals(timetable.ProcessingTime, 610);
			AssertNoErrors(timetable.ProcessingTimeInHoursInfo);

			timetable.ProcessingTimeInHours = new ZDateTime(ZDateTime.Today.Year, 1, 2, 10, 10, 0);
			AssertEquals(timetable.ProcessingTime, 2050);
			AssertNoErrors(timetable.ProcessingTimeInHoursInfo);

			timetable.ProcessingTimeInHours = ZDateTime.Invalid;
			AssertEquals(timetable.ProcessingTime, 0);
			AssertHasError(timetable.ProcessingTimeInHoursInfo, "Enter a valid processing time.");

			timetable.ProcessingTimeInHours = ZDateTime.Empty;
			AssertEquals(timetable.ProcessingTime, 0);
			AssertNoErrors(timetable.ProcessingTimeInHoursInfo);
		}

		protected override DefaultOrgTimetable GetBusinessObjectToClone()
		{
			return new DefaultOrgTimetable(Factory);
		}

		protected override DefaultOrgTimetable GetBusinessObjectToSerialise()
		{
			return new DefaultOrgTimetable(Factory);
		}
	}
}
