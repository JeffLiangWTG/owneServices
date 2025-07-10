using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgTimetableLookupsTest : BusinessObjectLookupsTestCase
	{
		OrgTimetable timetable;
		OrgTimetableLookups lookups;

		protected override void SetUp()
		{
			base.SetUp();
			timetable = Factory.New<OrgTimetable>();
			lookups = timetable.Lookups;
		}

		public void TestTypeList()
		{
			AssertEquals("Types.Count", 2, lookups.Types.Count);
			AssertEquals("Types should contain 'PIC'", true, lookups.Types.ContainsCode("PIC"));
			AssertEquals("Types should contain 'DLV'", true, lookups.Types.ContainsCode("DLV"));
		}

		public void TestDayList()
		{
			AssertEquals("Days.Count", 7, lookups.Days.Count);
			AssertEquals("Days should contain 'MON'", true, lookups.Days.ContainsCode("MON"));
			AssertEquals("Days should contain 'TUE'", true, lookups.Days.ContainsCode("TUE"));
			AssertEquals("Days should contain 'WED'", true, lookups.Days.ContainsCode("WED"));
			AssertEquals("Days should contain 'THU'", true, lookups.Days.ContainsCode("THU"));
			AssertEquals("Days should contain 'FRI'", true, lookups.Days.ContainsCode("FRI"));
			AssertEquals("Days should contain 'SAT'", true, lookups.Days.ContainsCode("SAT"));
			AssertEquals("Days should contain 'SUN'", true, lookups.Days.ContainsCode("SUN"));
		}
	}
}
