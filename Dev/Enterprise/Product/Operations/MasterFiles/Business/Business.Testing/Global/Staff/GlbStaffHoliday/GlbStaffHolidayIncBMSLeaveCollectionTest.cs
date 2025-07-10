using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffHolidayIncBMSLeaveCollection))]
	sealed class GlbStaffHolidayIncBMSLeaveCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRecordTypeFilter()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var holiday = Factory.NewWithValidTestData<GlbStaffHoliday>();
			holiday.GA_RecordType = StaffHolidayRecordTypeCodes.BufferManagementLeave;
			holiday.GA_GS = staff.PK;

			var holiday2 = Factory.NewWithValidTestData<GlbStaffHoliday>();
			holiday2.GA_GS = staff.PK;
			var holiday3 = Factory.NewWithValidTestData<GlbTimeAllocation>();
			holiday3.GA_GS = staff.PK;

			var leave = new GlbStaffHolidayIncBMSLeaveCollection(staff);
			leave.Load();
			AssertEquals(2, leave.Count);
			AssertCollectionContains(holiday, leave);
			AssertCollectionContains(holiday2, leave);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			return new GlbStaffHolidayIncBMSLeaveCollection(staff);
		}
	}
}
