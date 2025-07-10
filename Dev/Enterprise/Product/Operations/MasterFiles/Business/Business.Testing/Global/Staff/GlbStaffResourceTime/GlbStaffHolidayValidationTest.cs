using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class GlbStaffHolidayValidationTest : BusinessObjectValidationTestCase
	{
		public void TestExistedOverlappedTimeRangeRecordsHasWarnings()
		{
			GlbStaff.CurrentUser.Factory.SuspendValidation();

			Holiday.GA_WorkHolidayType = "ANN";
			Holiday.GA_StartTime = new ZDateTime(2019, 10, 1);
			Holiday.GA_EndTime = new ZDateTime(2019, 10, 6);
			Holiday.GA_DaysLeaveTaken = 5m;
			Holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;

			var existedOverlappedRecord1 = GlbStaff.CurrentUser.Holidays.AddNew();
			existedOverlappedRecord1.GA_WorkHolidayType = "ANN";
			existedOverlappedRecord1.GA_StartTime = new ZDateTime(2019, 10, 2);
			existedOverlappedRecord1.GA_EndTime = new ZDateTime(2019, 10, 7);
			existedOverlappedRecord1.GA_DaysLeaveTaken = 5m;
			existedOverlappedRecord1.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;

			var existedOverlappedRecord2 = GlbStaff.CurrentUser.Holidays.AddNew();
			existedOverlappedRecord2.GA_WorkHolidayType = "ANN";
			existedOverlappedRecord2.GA_StartTime = new ZDateTime(2019, 10, 3);
			existedOverlappedRecord2.GA_EndTime = new ZDateTime(2019, 10, 5);
			existedOverlappedRecord2.GA_DaysLeaveTaken = 5m;
			existedOverlappedRecord2.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Declined;

			GlbStaff.CurrentUser.Factory.Save();
			GlbStaff.CurrentUser.Factory.SuspendValidation();

			var factory = new BusinessObjectFactory();

			var holiday1 = factory.Load<GlbStaffHoliday>(Holiday.PK);
			holiday1.Validation.ValidateAll();
			AssertNoWarnings(holiday1.GA_StartTimeInfo);
			AssertHasWarnings("End time makes overlapped time range with others.", holiday1.GA_EndTimeInfo);

			var holiday2 = factory.Load<GlbStaffHoliday>(existedOverlappedRecord1.PK);
			holiday2.Validation.ValidateAll();
			AssertHasWarnings("Start time makes overlapped time range with others.", holiday2.GA_StartTimeInfo);
			AssertNoWarnings(holiday2.GA_EndTimeInfo);

			var holiday3 = factory.Load<GlbStaffHoliday>(existedOverlappedRecord2.PK);
			holiday3.Validation.ValidateAll();
			AssertNoWarnings(holiday3.GA_StartTimeInfo);
			AssertNoWarnings(holiday3.GA_EndTimeInfo);
		}

		public void TestBMSOverlapDoesntCount()
		{
			Holiday.GA_WorkHolidayType = "ANN";
			Holiday.GA_RecordType = "BMS";
			Holiday.GA_StartTime = new ZDateTime(2019, 10, 1);
			Holiday.GA_EndTime = new ZDateTime(2019, 10, 7);
			Holiday.GA_DaysLeaveTaken = 5m;
			Holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;

			Factory.Save();

			var existedOverlappedRecord = GlbStaff.CurrentUser.Holidays.AddNew();
			existedOverlappedRecord.GA_WorkHolidayType = "ANN";
			existedOverlappedRecord.GA_StartTime = new ZDateTime(2019, 10, 2);
			existedOverlappedRecord.GA_EndTime = new ZDateTime(2019, 10, 6);
			existedOverlappedRecord.GA_DaysLeaveTaken = 5m;
			existedOverlappedRecord.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;

			AssertNoErrors(existedOverlappedRecord.GA_StartTimeInfo);
			AssertNoErrors(existedOverlappedRecord.GA_EndTimeInfo);
		}

		public void TestNewOverlappedTimeRangeRecordsHasErrors()
		{
			Holiday.GA_WorkHolidayType = "ANN";
			Holiday.GA_StartTime = new ZDateTime(2019, 10, 1);
			Holiday.GA_EndTime = new ZDateTime(2019, 10, 7);
			Holiday.GA_DaysLeaveTaken = 5m;
			Holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;

			Factory.Save();

			var existedOverlappedRecord = GlbStaff.CurrentUser.Holidays.AddNew();
			existedOverlappedRecord.GA_WorkHolidayType = "ANN";
			existedOverlappedRecord.GA_StartTime = new ZDateTime(2019, 10, 2);
			existedOverlappedRecord.GA_EndTime = new ZDateTime(2019, 10, 6);
			existedOverlappedRecord.GA_DaysLeaveTaken = 5m;
			existedOverlappedRecord.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;

			AssertHasError(existedOverlappedRecord.GA_StartTimeInfo, $"Start time makes overlapped time range with others.");
			AssertHasError(existedOverlappedRecord.GA_EndTimeInfo, $"End time makes overlapped time range with others.");
		}

		public void TestWorkHolidayType()
		{
			Holiday.GA_WorkHolidayType = "XXX";
			AssertHasErrors(Holiday.GA_WorkHolidayTypeInfo);

			Holiday.GA_WorkHolidayType = "ANN";
			AssertNoErrors(Holiday.GA_WorkHolidayTypeInfo);

			Holiday.GA_WorkHolidayType = "";
			AssertHasErrors(Holiday.GA_WorkHolidayTypeInfo);
		}

		public void TestWorkHolidayType_HasChanges()
		{
			GlbStaff.CurrentUser.Factory.SuspendValidation();

			Holiday.GA_WorkHolidayType = "TST";
			Holiday.GA_StartTime = new ZDateTime(2019, 10, 1);
			Holiday.GA_EndTime = new ZDateTime(2019, 10, 7);
			Holiday.GA_DaysLeaveTaken = 5m;
			Holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Requested;

			GlbStaff.CurrentUser.Factory.Save();
			GlbStaff.CurrentUser.Factory.SuspendValidation();

			var factory = new BusinessObjectFactory();
			var holiday1 = factory.Load<GlbStaffHoliday>(Holiday.PK);

			holiday1.Validation.ValidateAll();
			AssertHasWarnings(holiday1.GA_WorkHolidayTypeInfo);

			holiday1.GA_WorkHolidayType = "XXX";
			AssertHasErrors(holiday1.GA_WorkHolidayTypeInfo);

			holiday1.GA_WorkHolidayType = "ANN";
			AssertNoErrors(holiday1.GA_WorkHolidayTypeInfo);

			holiday1.GA_WorkHolidayType = "";
			AssertHasErrors(holiday1.GA_WorkHolidayTypeInfo);
		}

		[TestDate(2006, 1, 1)]
		public void TestTimes()
		{
			Holiday.GA_StartTime = ZDateTime.Empty;
			AssertHasError(Holiday.GA_StartTimeInfo, "Please enter a Start Time.");

			Holiday.GA_StartTime = new ZDateTime(2005, 1, 2);
			AssertNoErrors(Holiday.GA_StartTimeInfo);

			Holiday.GA_EndTime = ZDateTime.Empty;
			AssertHasError(Holiday.GA_EndTimeInfo, "Please enter an End Time.");

			Holiday.GA_EndTime = new ZDateTime(2005, 1, 1);
			AssertHasError(Holiday.GA_EndTimeInfo, "End time must be after the Start time.");

			Holiday.GA_EndTime = new ZDateTime(2005, 1, 2);
			AssertNoErrors(Holiday.GA_EndTimeInfo);

			Holiday.GA_StartTime = new ZDateTime(2005, 1, 3);
			AssertHasError(Holiday.GA_StartTimeInfo, "Start time must be before the End time.");

			Holiday.GA_StartTime = new ZDateTime(2005, 1, 2);
			AssertNoErrors(Holiday.GA_StartTimeInfo);

			Holiday.GA_EndTime = new ZDateTime(2005, 1, 3);
			AssertNoErrors(Holiday.GA_EndTimeInfo);

			Holiday.GA_EndTime = new ZDateTime(2005, 1, 4);
			AssertNoErrors(Holiday.GA_EndTimeInfo);
		}

		protected GlbStaffHoliday Holiday;

		protected override void SetUp()
		{
			base.SetUp();
			Holiday = GlbStaff.CurrentUser.Holidays.AddNew();
		}
	}
}
