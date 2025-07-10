using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgTimetableValidationTest : BusinessObjectValidationTestCase
	{
		OrgTimetable timetable;

		protected override void SetUp()
		{
			base.SetUp();
			timetable = Factory.New<OrgTimetable>();
		}

		public void TestTypeValidation()
		{
			timetable.OTT_Type = ZString.Empty;
			AssertHasErrors("Type should not be empty", timetable.OTT_TypeInfo);

			timetable.OTT_Type = "---";
			AssertHasErrors("Invalid Type", timetable.OTT_TypeInfo);

			timetable.OTT_Type = timetable.Lookups.Types[0].Code;
			AssertNoErrors(timetable.OTT_TypeInfo);
		}

		public void TestTimeFromValidation()
		{
			timetable.OTT_TimeFrom = ZDateTime.Empty;
			AssertHasErrors("TimeFrom should not be empty", timetable.OTT_TimeFromInfo);

			timetable.OTT_TimeFrom = new ZDateTime(2000, 7, 7, 8, 0, 0);
			AssertNoErrors(timetable.OTT_TimeFromInfo);
			AssertEquals(new ZDateTime(1900, 1, 1, 8, 0, 0), timetable.OTT_TimeFrom);
		}

		public void TestTimeToValidation()
		{
			timetable.OTT_TimeTo = ZDateTime.Empty;
			AssertHasErrors("TimeTo should not be empty", timetable.OTT_TimeToInfo);

			timetable.OTT_TimeTo = new ZDateTime(2000, 7, 7, 9, 0, 0);
			AssertNoErrors(timetable.OTT_TimeToInfo);
			AssertEquals(new ZDateTime(1900, 1, 1, 9, 0, 0), timetable.OTT_TimeTo);
		}

		public void TestErrorIsDisplayedIfUserAttemptsToUseWeekdayButWeekendsAreNotConfiguredForTheCountry()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			timetable.OTT_OA = address.PK;
			timetable.OTT_IsForAllWeekDays = true;
			var errorMessage = "To use a 'Weekday' option the weekends must be configured for Organization’s country within Maintain > Locations > Countries/Regions > Holidays module.";
			AssertHasErrors(errorMessage, timetable.OTT_IsForAllWeekDaysInfo);
		}

		public void TestNoErrorIsDisplayedIfUserSelectsWeekdayAndWeekendsAreConfiguredForTheCountry()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var country = new RefCountry.Loader(Factory).LoadForCountry(address.OA_RN_NKCountryCode);
			country.IsSaturdayNonWorkingDay = true;
			timetable.OTT_OA = address.PK;
			timetable.OTT_IsForAllWeekDays = true;
			AssertNoErrors(timetable.OTT_IsForAllWeekDaysInfo);
		}

		public void TestValidationShouldRunForWeekdaysToSeeIfWeekendsAreConfiguredInValidateAll()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			timetable.OTT_OA = address.PK;
			timetable.OTT_IsForAllWeekDays = true;

			var errorMessage = "To use a 'Weekday' option the weekends must be configured for Organization’s country within Maintain > Locations > Countries/Regions > Holidays module.";
			AssertHasErrors(errorMessage, timetable.OTT_IsForAllWeekDaysInfo);

			var country = new RefCountry.Loader(Factory).LoadForCountry(address.OA_RN_NKCountryCode);
			country.IsSaturdayNonWorkingDay = true;

			timetable.Validation.ValidateAll();
			AssertNoErrors(timetable.OTT_IsForAllWeekDaysInfo);
		}

		public void TestTimeRangeValidation()
		{
			timetable.OTT_TimeFrom = new ZDateTime(2015, 1, 1, 8, 0, 0);
			timetable.OTT_TimeTo = new ZDateTime(2015, 1, 1, 7, 0, 0);
			AssertHasErrors("'From Time' must be prior to 'To Time'.", timetable.OTT_TimeToInfo);
		}

		public void TestDayOfWeekValidation()
		{
			var address = Factory.New<OrgAddress>();
			timetable.OTT_OA = address.PK;
			address.Timetables.Add(timetable);

			timetable.DayOfWeek = ZString.Empty;
			AssertNoErrors(timetable.DayOfWeekInfo);

			timetable.Address.Timetables.AdvancedRange = true;

			timetable.DayOfWeek = ZString.Empty;
			AssertHasErrors("DayOfWeek should not be empty", timetable.DayOfWeekInfo);

			timetable.DayOfWeek = "---";
			AssertHasErrors("Invalid DayOfWeek", timetable.DayOfWeekInfo);

			timetable.DayOfWeek = timetable.Lookups.Days[0].Code;
			AssertNoErrors(timetable.DayOfWeekInfo);
		}

		public void TestCutOffTimeValidation()
		{
			timetable.OTT_CutOffTime = ZDateTime.Empty;
			AssertNoErrors("CutOffTime can be empty", timetable.OTT_CutOffTimeInfo);

			timetable.OTT_CutOffTime = ZDateTime.Invalid;
			AssertHasErrors("CutOffTime can not be invalid", timetable.OTT_CutOffTimeInfo);

			timetable.OTT_CutOffTime = new ZDateTime(2000, 7, 7, 9, 0, 0);
			AssertNoErrors(timetable.OTT_CutOffTimeInfo);
			AssertEquals(new ZDateTime(1900, 1, 1, 9, 0, 0), timetable.OTT_CutOffTime);
		}

		public void TestProcessingTimeValidation()
		{
			timetable.OTT_ProcessingTimeInMinutes = -1;
			AssertHasErrors("ProcessingTime should not be negative value", timetable.OTT_ProcessingTimeInMinutesInfo);

			timetable.OTT_ProcessingTimeInMinutes = (ZInt)0;
			AssertNoErrors(timetable.OTT_ProcessingTimeInMinutesInfo);

			timetable.OTT_ProcessingTimeInMinutes = (ZInt)999999999;
			AssertNoErrors(timetable.OTT_ProcessingTimeInMinutesInfo);
		}

		public void TestValidateOTTWhenOTT_OAInactive()
		{
			var address = Factory.New<OrgAddress>();
			address.Timetables.Add(timetable);
			address.OA_IsActive = false;

			timetable.Validation.ValidateOTT_OA();
			AssertNoErrors(timetable.OTT_OAInfo);
		}

		public void TestShouldNotValidateTimeTableWhenParentIsNotAccplicableRange()
		{
			var address = Factory.New<OrgAddress>();

			address.Timetables.SetRangeType(OrgTimeTableRangeType.Advanced);
			address.Timetables[0].OTT_TimeFrom = ZDateTime.Empty;
			AssertHasErrors("Precondition", address.Timetables[0].OTT_TimeFromInfo);

			address.Timetables.SetRangeType(OrgTimeTableRangeType.NotApplicable);
			address.Timetables[0].OTT_TimeFrom = ZDateTime.Empty;
			AssertNoErrors(address.Timetables[0].OTT_TimeFromInfo);

			address.Timetables.SetRangeType(OrgTimeTableRangeType.Advanced);
			address.Timetables[0].OTT_TimeTo = ZDateTime.Empty;
			AssertHasErrors("Precondition", address.Timetables[0].OTT_TimeToInfo);

			address.Timetables.SetRangeType(OrgTimeTableRangeType.NotApplicable);
			address.Timetables[0].OTT_TimeTo = ZDateTime.Empty;
			AssertNoErrors(address.Timetables[0].OTT_TimeToInfo);
		}

		public void TestValidateProcessingTimeInHours()
		{
			timetable.ProcessingTimeInHours = ZDateTime.Empty;
			AssertNoErrors("Processing time in hours can be empty", timetable.ProcessingTimeInHoursInfo);

			timetable.ProcessingTimeInHours = ZDateTime.Invalid;
			AssertHasErrors("Processing time in hours can not be invalid", timetable.ProcessingTimeInHoursInfo);

			timetable.ProcessingTimeInHours = new ZDateTime(2000, 7, 7, 9, 10, 0);
			AssertNoErrors(timetable.OTT_CutOffTimeInfo);
		}

		public void TestValidateMultipleCutOffTime()
		{
			var address = Factory.New<OrgAddress>();
			var t1 = Factory.New<OrgTimetable>();
			t1.DayOfWeek = "FRI";
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.OTT_TimeFrom = new ZDateTime(ZDateTime.Today.Year, 1, 1, 9, 0, 0);
			t1.OTT_TimeTo = new ZDateTime(ZDateTime.Today.Year, 1, 1, 12, 0, 0);
			t1.OTT_CutOffTime = new ZDateTime(ZDateTime.Today.Year, 1, 1, 11, 0, 0);
			address.Timetables.Add(t1);
			AssertNoErrors(t1.OTT_CutOffTimeInfo);

			var t2 = Factory.New<OrgTimetable>();
			t2.DayOfWeek = "FRI";
			t2.OTT_Type = OrgTimetableType.Codes.Pickup;
			t2.OTT_TimeFrom = new ZDateTime(ZDateTime.Today.Year, 1, 1, 14, 0, 0);
			t2.OTT_TimeTo = new ZDateTime(ZDateTime.Today.Year, 1, 1, 16, 0, 0);
			address.Timetables.Add(t2);
			AssertNoErrors(t2.OTT_CutOffTimeInfo);

			t2.OTT_CutOffTime = new ZDateTime(ZDateTime.Today.Year, 1, 1, 16, 0, 0);
			AssertHasErrors("Cut-Off Time had already been set", t2.OTT_CutOffTimeInfo);
		}

		public void TestValidateMultipleProcessingTimeInHours()
		{
			var address = Factory.New<OrgAddress>();
			var t1 = Factory.New<OrgTimetable>();
			t1.DayOfWeek = "FRI";
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.OTT_TimeFrom = new ZDateTime(ZDateTime.Today.Year, 1, 1, 14, 0, 0);
			t1.OTT_TimeTo = new ZDateTime(ZDateTime.Today.Year, 1, 1, 18, 0, 0);
			t1.ProcessingTimeInHours = new ZDateTime(ZDateTime.Today.Year, 1, 1, 2, 0, 0);
			address.Timetables.Add(t1);
			AssertNoErrors(t1.ProcessingTimeInHoursInfo);

			var t2 = Factory.New<OrgTimetable>();
			t2.DayOfWeek = "FRI";
			t2.OTT_Type = OrgTimetableType.Codes.Pickup;
			t2.OTT_TimeFrom = new ZDateTime(ZDateTime.Today.Year, 1, 1, 9, 0, 0);
			t2.OTT_TimeTo = new ZDateTime(ZDateTime.Today.Year, 1, 1, 13, 0, 0);
			t2.OTT_CutOffTime = new ZDateTime(ZDateTime.Today.Year, 1, 1, 12, 0, 0);
			address.Timetables.Add(t2);
			AssertNoErrors(t2.ProcessingTimeInHoursInfo);

			t2.ProcessingTimeInHours = new ZDateTime(ZDateTime.Today.Year, 1, 1, 4, 0, 0);
			AssertHasErrors("Processing Time(Hours) had already been set", t2.ProcessingTimeInHoursInfo);
		}

		public void TestValidateProcessingTimeInHoursAndCutoffTime()
		{
			var address = Factory.New<OrgAddress>();
			var t1 = Factory.New<OrgTimetable>();
			t1.DayOfWeek = "TUE";
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.OTT_TimeFrom = new ZDateTime(ZDateTime.Today.Year, 1, 1, 13, 0, 0);
			t1.OTT_TimeTo = new ZDateTime(ZDateTime.Today.Year, 1, 1, 17, 0, 0);
			address.Timetables.Add(t1);
			AssertNoErrors(t1.ProcessingTimeInHoursInfo);
			AssertNoErrors(t1.OTT_CutOffTimeInfo);

			var t2 = Factory.New<OrgTimetable>();
			t2.DayOfWeek = "TUE";
			t2.OTT_Type = OrgTimetableType.Codes.Pickup;
			t2.OTT_TimeFrom = new ZDateTime(ZDateTime.Today.Year, 1, 1, 9, 0, 0);
			t2.OTT_TimeTo = new ZDateTime(ZDateTime.Today.Year, 1, 1, 12, 0, 0);
			address.Timetables.Add(t2);

			t2.OTT_CutOffTime = new ZDateTime(ZDateTime.Today.Year, 1, 1, 12, 0, 0);
			t2.ProcessingTimeInHours = new ZDateTime(ZDateTime.Today.Year, 1, 1, 05, 00, 0);
			AssertNoErrors(t2.ProcessingTimeInHoursInfo);
			AssertNoErrors(t2.OTT_CutOffTimeInfo);
		}
	}
}
