using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTimetable))]
	sealed class OrgTimetableTest : EnterpriseBusinessObjectTestCase
	{
		OrgAddress address;
		OrgTimetable timetable;

		protected override void SetUp()
		{
			base.SetUp();
			address = Factory.NewWithValidTestData<OrgAddress>();
			timetable = Factory.New<OrgTimetable>();
			timetable.OTT_Type = OrgTimetableType.Codes.Pickup;
			address.Timetables.Add(timetable);
		}

		public override (ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimes(string propertyName)
		{
			if (propertyName == "OTT_TimeFrom" || propertyName == "OTT_TimeTo" || propertyName == "OTT_CutOffTime")
			{
				return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
				{
					(new ZDateTime(1900, 1, 1, 8, 30, 0), new ZDateTime(1900, 1, 1, 8, 30, 0)),
					(new ZDateTime(1900, 1, 1, 23, 30, 0), new ZDateTime(1900, 1, 1, 23, 30, 0)),
					(new ZDateTime(1900, 1, 1, 0, 0, 0), new ZDateTime(1900, 1, 1, 0, 0, 0)),

					(ZDateTime.Invalid, ZDateTime.Invalid),
					(new ZDateTime(DateTime.MaxValue), new ZDateTime(1900, 1, 1, 23, 59, 59)),
					(new ZDateTime(DateTime.MinValue), new ZDateTime(DateTime.MinValue)),
					(ZDateTime.Empty, ZDateTime.Empty),
				};
			}

			return base.GetValidZDateTimes(propertyName);
		}

		public void TestReadOnlySecurity()
		{
			var oldDetailsNew = Env.Security.OrgAddressAdditionalDetailsNew.IsAllowed;
			var oldDetailsModify = Env.Security.OrgAddressAdditionalDetailsModify.IsAllowed;

			try
			{
				Env.Security.OrgAddressAdditionalDetailsNew.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", timetable.OTT_TypeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", timetable.OTT_TimeFromInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", timetable.OTT_TimeToInfo.ReadOnly);

				Env.Security.OrgAddressAdditionalDetailsNew.IsAllowed = true;
				Assert("Access Allowed - ReadOnly", !timetable.OTT_TypeInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !timetable.OTT_TimeFromInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !timetable.OTT_TimeToInfo.ReadOnly);

				Factory.Save();
				Env.Security.OrgAddressAdditionalDetailsModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", timetable.OTT_TypeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", timetable.OTT_TimeFromInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", timetable.OTT_TimeToInfo.ReadOnly);

				Env.Security.OrgAddressAdditionalDetailsModify.IsAllowed = true;
				Assert("Access Allowed - ReadOnly", !timetable.OTT_TypeInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !timetable.OTT_TimeFromInfo.ReadOnly);
				Assert("Access Allowed - ReadOnly", !timetable.OTT_TimeToInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgAddressAdditionalDetailsNew.IsAllowed = oldDetailsNew;
				Env.Security.OrgAddressAdditionalDetailsModify.IsAllowed = oldDetailsModify;
			}
		}

		public void TestDayOfWeekIsEmptyAtWeekdayRange()
		{
			timetable.OTT_Monday = true;
			timetable.OTT_Tuesday = true;
			timetable.OTT_Wednesday = true;
			timetable.OTT_Thursday = true;
			timetable.OTT_Friday = true;
			timetable.OTT_Saturday = false;
			timetable.OTT_Sunday = false;

			AssertEquals("DayOfWeek should be empty.", "", timetable.DayOfWeek);
		}

		public void TestDayOfWeekIsEmptyWhenIsForAllWeekdaysIsSetRegardlessOfAnyOtherDays()
		{
			timetable.OTT_Monday = true;
			timetable.OTT_IsForAllWeekDays = true;
			AssertEquals("DayOfWeek should be empty.", "", timetable.DayOfWeek);
		}

		public void TestDayOfWeekIsEmptyAtEverydayRange()
		{
			timetable.OTT_Monday = true;
			timetable.OTT_Tuesday = true;
			timetable.OTT_Wednesday = true;
			timetable.OTT_Thursday = true;
			timetable.OTT_Friday = true;
			timetable.OTT_Saturday = true;
			timetable.OTT_Sunday = true;

			AssertEquals("DayOfWeek should be empty.", "", timetable.DayOfWeek);
		}

		public void TestDayOfWeekIsCorrectAtAdvancedRange()
		{
			timetable.OTT_Monday = false;
			timetable.OTT_Tuesday = false;
			timetable.OTT_Wednesday = false;
			timetable.OTT_Thursday = false;
			timetable.OTT_Friday = true;
			timetable.OTT_Saturday = false;
			timetable.OTT_Sunday = false;

			AssertEquals("DayOfWeek should be Friday.", AutoDayOfWeekCodeList.Codes.Friday, timetable.DayOfWeek);
		}

		public void TestOverlapTimeframe()
		{
			timetable.OTT_TimeFrom = new ZDateTime(2015, 1, 1, 10, 0, 0);
			timetable.OTT_TimeTo = new ZDateTime(2015, 1, 1, 12, 0, 0);

			OrgTimetable t1 = Factory.New<OrgTimetable>();
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.OTT_TimeFrom = new ZDateTime(2015, 1, 1, 8, 0, 0);
			t1.OTT_TimeTo = new ZDateTime(2015, 1, 1, 10, 0, 0);

			OrgTimetable t2 = Factory.New<OrgTimetable>();
			t2.OTT_Type = OrgTimetableType.Codes.Pickup;
			t2.OTT_TimeFrom = new ZDateTime(2015, 1, 1, 8, 0, 0);
			t2.OTT_TimeTo = new ZDateTime(2015, 1, 1, 11, 0, 0);

			OrgTimetable t3 = Factory.New<OrgTimetable>();
			t3.OTT_Type = OrgTimetableType.Codes.Deliver;
			t3.OTT_TimeFrom = new ZDateTime(2015, 1, 1, 8, 0, 0);
			t3.OTT_TimeTo = new ZDateTime(2015, 1, 1, 11, 0, 0);

			OrgTimetable t4 = Factory.New<OrgTimetable>();
			t4.OTT_Type = OrgTimetableType.Codes.Pickup;
			t4.OTT_TimeFrom = new ZDateTime(2015, 1, 1, 8, 0, 0);
			t4.OTT_TimeTo = new ZDateTime(2015, 1, 1, 14, 0, 0);

			OrgTimetable t5 = Factory.New<OrgTimetable>();
			t5.OTT_Type = OrgTimetableType.Codes.Pickup;
			t5.OTT_TimeFrom = new ZDateTime(2015, 1, 1, 12, 0, 0);
			t5.OTT_TimeTo = new ZDateTime(2015, 1, 1, 14, 0, 0);

			AssertEquals(true, t1.OverlapTimeframe(t2));
			AssertEquals(true, t2.OverlapTimeframe(t1));
			AssertEquals(false, t1.OverlapTimeframe(t3));
			AssertEquals(true, t1.OverlapTimeframe(t4));
			AssertEquals(false, t1.OverlapTimeframe(t5));

			t3.OTT_Type = OrgTimetableType.Codes.Pickup;
			AssertEquals(true, t1.OverlapTimeframe(t3));

			t4.DayOfWeek = "MON";
			AssertEquals(false, t1.OverlapTimeframe(t4));
		}

		public void TestIsCutOffOrProcessingTimeExisted()
		{
			OrgTimetable t1 = Factory.New<OrgTimetable>();
			t1.OTT_Type = OrgTimetableType.Codes.Deliver;
			t1.OTT_TimeFrom = new ZDateTime(2024, 1, 1, 8, 0, 0);
			t1.OTT_TimeTo = new ZDateTime(2024, 1, 1, 10, 0, 0);
			t1.DayOfWeek = "MON";
			t1.OTT_CutOffTime = new ZDateTime(ZDateTime.Today.Year, 1, 2, 10, 10, 0);

			OrgTimetable t2 = Factory.New<OrgTimetable>();
			t2.OTT_Type = OrgTimetableType.Codes.Deliver;
			t2.OTT_TimeFrom = new ZDateTime(2024, 1, 1, 11, 0, 0);
			t2.OTT_TimeTo = new ZDateTime(2024, 1, 1, 15, 0, 0);
			t2.DayOfWeek = "MON";

			t2.OTT_CutOffTime = new ZDateTime(ZDateTime.Today.Year, 1, 2, 10, 10, 0);
			Assert(t1.IsCutOffTimeExisted(t2));

			t1.ProcessingTimeInHours = new ZDateTime(ZDateTime.Today.Year, 1, 2, 10, 10, 0);
			t2.ProcessingTimeInHours = new ZDateTime(ZDateTime.Today.Year, 1, 2, 10, 10, 0);
			Assert(t1.IsProcessingTimeExisted(t2));
		}

		public void TestSettingDayOfWeekShouldNotValidateFromAndTo()
		{
			var timetable = Factory.New<OrgTimetable>();
			timetable.DayOfWeek = "MON";
			Assert(!timetable.OTT_TimeFromInfo.HasErrors());
			Assert(!timetable.OTT_TimeToInfo.HasErrors());
		}

		public void TestIsForAllWeekdays_TimetableIsAppliedToStandardDaysOfWeekIfNoAddressIsDefined()
		{
			var t1 = Factory.New<OrgTimetable>();
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.OTT_TimeFrom = new ZDateTime(2024, 1, 1, 8, 0, 0);
			t1.OTT_TimeTo = new ZDateTime(2024, 1, 1, 10, 0, 0);
			t1.OTT_IsForAllWeekDays = true;
			Assert("Should apply to standard weekdays if no address is configured", t1.IsAppliedToDayOfWeek(System.DayOfWeek.Friday));
			Assert("Should not apply to standard weekdends if no address is configured", !t1.IsAppliedToDayOfWeek(System.DayOfWeek.Saturday));
		}

		public void TestIsForAllWeekdays_TimetableIsAppliedToStandardDaysOfWeekIfNoWeekendsAreDefinedForTheCountry()
		{
			var t1 = Factory.New<OrgTimetable>();
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.OTT_TimeFrom = new ZDateTime(2024, 1, 1, 8, 0, 0);
			t1.OTT_TimeTo = new ZDateTime(2024, 1, 1, 10, 0, 0);
			t1.OTT_IsForAllWeekDays = true;

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.Address1 = "ABCDEF";
			address1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			t1.OTT_OA = address1.PK;

			Assert("Should apply to standard weekdays if the country has no weekends defined", t1.IsAppliedToDayOfWeek(System.DayOfWeek.Friday));
			Assert("Should not apply to standard weekdends if the country has no weekends defined", !t1.IsAppliedToDayOfWeek(System.DayOfWeek.Saturday));
		}

		public void TestIsForAllWeekdays_TimetableIsAppliedToConfiguredWeekdaysForTheCountry()
		{
			var t1 = Factory.New<OrgTimetable>();
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.OTT_TimeFrom = new ZDateTime(2024, 1, 1, 8, 0, 0);
			t1.OTT_TimeTo = new ZDateTime(2024, 1, 1, 10, 0, 0);
			t1.OTT_IsForAllWeekDays = true;

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.Address1 = "ABCDEF";
			address1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var country1 = new RefCountry.Loader(Factory).LoadForCountry(address1.OA_RN_NKCountryCode);
			country1.IsFridayNonWorkingDay = true;
			t1.OTT_OA = address1.PK;

			Assert("Should apply to weekdays for the country", t1.IsAppliedToDayOfWeek(System.DayOfWeek.Sunday));
			Assert("Should not apply to weekdends for the country", !t1.IsAppliedToDayOfWeek(System.DayOfWeek.Friday));
		}
	}
}
