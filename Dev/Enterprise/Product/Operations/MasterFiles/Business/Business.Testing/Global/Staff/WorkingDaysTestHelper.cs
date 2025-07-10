using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.CalendarArithmetic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class WorkingDaysTestHelper
	{
		public const string WeekEndHours = "";
		public const string NineToFive = "                  ****************";
		public const string EightThirtyToSix = "                 *******************";

		public static string GetWorkingHoursString(float fromHour, float toHour)
		{
			return new string(' ', (int)(fromHour * 2)) + new string('*', (int)((toHour - fromHour) * 2));
		}

		public static void UpdateEveryDayTo9To5(BusinessObjectFactory factory, ZGuid deptPK)
		{
			foreach (var day in GetEveryDay())
			{
				UpdateDepartmentDay(factory, deptPK, day, NineToFive);
			}
		}

		public static void UpdateWeekDaysTo9To5(BusinessObjectFactory factory, ZGuid deptPK)
		{
			UpdateDepartmentDay(factory, deptPK, DayOfWeek.Monday, NineToFive);
			UpdateDepartmentDay(factory, deptPK, DayOfWeek.Tuesday, NineToFive);
			UpdateDepartmentDay(factory, deptPK, DayOfWeek.Wednesday, NineToFive);
			UpdateDepartmentDay(factory, deptPK, DayOfWeek.Thursday, NineToFive);
			UpdateDepartmentDay(factory, deptPK, DayOfWeek.Friday, NineToFive);
			UpdateDepartmentDay(factory, deptPK, DayOfWeek.Saturday, WeekEndHours);
			UpdateDepartmentDay(factory, deptPK, DayOfWeek.Sunday, WeekEndHours);
		}

		public static void UpdateStaffWeekDaysTo9To5(BusinessObjectFactory factory, ZGuid staffPK)
		{
			UpdateStaffDay(factory, staffPK, DayOfWeek.Monday, NineToFive);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Tuesday, NineToFive);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Wednesday, NineToFive);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Thursday, NineToFive);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Friday, NineToFive);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Saturday, WeekEndHours);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Sunday, WeekEndHours);
		}

		public static void UpdateStaffWeekDaysTo8_30To6(BusinessObjectFactory factory, ZGuid staffPK)
		{
			UpdateStaffDay(factory, staffPK, DayOfWeek.Monday, EightThirtyToSix);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Tuesday, EightThirtyToSix);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Wednesday, EightThirtyToSix);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Thursday, EightThirtyToSix);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Friday, EightThirtyToSix);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Saturday, WeekEndHours);
			UpdateStaffDay(factory, staffPK, DayOfWeek.Sunday, WeekEndHours);
		}

		public static void UpdateDepartmentDay(BusinessObjectFactory factory, ZGuid deptPK, DayOfWeek day, string workingHours)
		{
			var query = GetDepartmentQuery(deptPK);
			UpdateWorkingHoursDay(factory, query, day, workingHours);
		}

		static ZQuery GetDepartmentQuery(ZGuid deptPK)
		{
			var query = new ZQuery(GlbWorkTimeSchema.GW_ParentID, deptPK);
			query.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, GlbDepartmentSchema.Constants.Prefix);
			return query;
		}

		public static void UpdateStaffDay(BusinessObjectFactory factory, ZGuid staffPK, DayOfWeek day, string workingHours)
		{
			var query = new ZQuery(GlbWorkTimeSchema.GW_ParentID, staffPK);
			query.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, GlbStaffSchema.Constants.Prefix);
			UpdateWorkingHoursDay(factory, query, day, workingHours);
		}

		public static void UpdateAllStaffDays(BusinessObjectFactory factory, ZGuid staffPK, string workingHours)
		{
			var query = new ZQuery(GlbWorkTimeSchema.GW_ParentID, staffPK);
			query.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, GlbStaffSchema.Constants.Prefix);
			var workTime = new GlbWorkTimeCollection(factory, query);

			foreach (var day in GetEveryDay())
			{
				workTime.SetWorkingHoursForDayOfWeek(day, workingHours);
			}
		}

		public static void DeleteAllStaffWorkTimes(BusinessObjectFactory factory, ZGuid staffPK)
		{
			var query = new ZQuery(GlbWorkTimeSchema.GW_ParentID, staffPK);
			query.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, GlbStaffSchema.Constants.Prefix);

			var allWorkTimes = factory.Load<GlbWorkTime>(query);
			for (int i = 0; i < allWorkTimes.Length; i++)
			{
				allWorkTimes[i].Delete();
			}
		}

		static void UpdateWorkingHoursDay(BusinessObjectFactory factory, ZQuery query, DayOfWeek day, string workingHours)
		{
			var workTime = new GlbWorkTimeCollection(factory, query);
			workTime.SetWorkingHoursForDayOfWeek(day, workingHours);
		}

		public static void SetDefaultStaffWorkTimeWeek(BusinessObjectFactory factory, ZGuid staffPk)
		{
			CreateWorkTime(factory, staffPk, GlbStaffSchema.Constants.Prefix, GetDefaultWorkWeek());
		}

		public static void SetDefaultDepartmentWorkTimeWeek(BusinessObjectFactory factory, ZGuid deptPK)
		{
			CreateWorkTime(factory, deptPK, GlbDepartmentSchema.Constants.Prefix, GetDefaultWorkWeek());
		}

		public static void SetBranchHolidays(BusinessObjectFactory factory, ZGuid branchPK)
		{
			CreateHoliday(factory, branchPK, new DateTime(2000, 1, 1), "New Years Day", isRecurring: true);
			CreateHoliday(factory, branchPK, new DateTime(2000, 1, 2), "New Years Day Was Once Two Days");

			CreateHoliday(factory, branchPK, null, "Good Friday", isRecurring: true, recurrenceType: HolidayRecurTypeCodes.GoodFriday); //Friday, April 21 in 2000
			CreateHoliday(factory, branchPK, null, "Easter Monday", isRecurring: true, recurrenceType: HolidayRecurTypeCodes.EasterMonday); //Monday, April 24 in 2000
			CreateHoliday(factory, branchPK, null, "LabourDay", CalendarCodes.Months.October, CalendarCodes.Days.Monday, true, HolidayRecurTypeCodes.First);
		}

		public static GlbHoliday CreateHoliday(BusinessObjectFactory factory, ZGuid branchPKToUse, DateTime? date = null, string holidayName = "holiday", string recurringMonth = "", string recurringDay = "", bool isRecurring = false, string recurrenceType = HolidayRecurTypeCodes.Date)
		{
			var holiday = factory.New<GlbHoliday>();
			holiday.GH_HolidayName = holidayName;
			holiday.GH_Date = date ?? ZDateTime.Empty;
			holiday.GH_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			holiday.GH_ParentID = branchPKToUse;
			holiday.GH_RecurrMonth = recurringMonth;
			holiday.GH_RecurrDay = recurringDay;
			holiday.GH_Recurring = isRecurring;
			holiday.GH_RecurrType = recurrenceType;

			return holiday;
		}

		public static GlbStaffHoliday CreateStaffHoliday(BusinessObjectFactory factory, ZGuid staffPKToUse, DateTimeRange holidayRange, string approvalCode = StaffHolidayApprovalCodes.Approved, int availabilityPercentage = 0, bool workingAway = false)
		{
			var holiday = factory.New<GlbStaffHoliday>();
			holiday.GA_GS = staffPKToUse;
			holiday.GA_StartTime = holidayRange.Start;
			holiday.GA_EndTime = holidayRange.End;
			holiday.GA_ApprovalStatus = approvalCode;
			holiday.GA_AvailabilityPercentage = (ZByte)availabilityPercentage;
			holiday.GA_IsWorkingAway = workingAway;

			return holiday;
		}

		public static GlbWorkTimeCollection CreateWorkTime(BusinessObjectFactory factory, ZGuid pkToUse, string parentTableCode, Dictionary<DayOfWeek, string> workingHours)
		{
			var query = new ZQuery(GlbWorkTimeSchema.GW_ParentID, pkToUse);
			query.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, parentTableCode);
			var workTime = new GlbWorkTimeCollection(factory, query)
			{
				MondayWorkingHours = workingHours[DayOfWeek.Monday],
				TuesdayWorkingHours = workingHours[DayOfWeek.Tuesday],
				WednesdayWorkingHours = workingHours[DayOfWeek.Wednesday],
				ThursdayWorkingHours = workingHours[DayOfWeek.Thursday],
				FridayWorkingHours = workingHours[DayOfWeek.Friday],
				SaturdayWorkingHours = workingHours[DayOfWeek.Saturday],
				SundayWorkingHours = workingHours[DayOfWeek.Sunday]
			};

			return workTime;
		}

		static IEnumerable<DayOfWeek> GetEveryDay()
		{
			yield return DayOfWeek.Monday;
			yield return DayOfWeek.Tuesday;
			yield return DayOfWeek.Wednesday;
			yield return DayOfWeek.Thursday;
			yield return DayOfWeek.Friday;
			yield return DayOfWeek.Saturday;
			yield return DayOfWeek.Sunday;
		}

		public static Dictionary<DayOfWeek, string> GetDefaultWorkWeek()
		{
			return new Dictionary<DayOfWeek, string>
			{
				{ DayOfWeek.Monday, GetPaddedWorkingHours(NineToFive) },
				{ DayOfWeek.Tuesday, GetPaddedWorkingHours(NineToFive) },
				{ DayOfWeek.Wednesday, GetPaddedWorkingHours(NineToFive) },
				{ DayOfWeek.Thursday, GetPaddedWorkingHours(NineToFive) },
				{ DayOfWeek.Friday, GetPaddedWorkingHours(NineToFive) },
				{ DayOfWeek.Saturday, GetPaddedWorkingHours(WeekEndHours) },
				{ DayOfWeek.Sunday, GetPaddedWorkingHours(WeekEndHours) },
			};
		}

		public static Dictionary<DayOfWeek, string> GetWedSunWorkWeek()
		{
			return new Dictionary<DayOfWeek, string>
			{
				{ DayOfWeek.Monday, GetPaddedWorkingHours(WeekEndHours) },
				{ DayOfWeek.Tuesday, GetPaddedWorkingHours(WeekEndHours) },
				{ DayOfWeek.Wednesday, GetPaddedWorkingHours(NineToFive) },
				{ DayOfWeek.Thursday, GetPaddedWorkingHours(WeekEndHours) },
				{ DayOfWeek.Friday, GetPaddedWorkingHours(WeekEndHours) },
				{ DayOfWeek.Saturday, GetPaddedWorkingHours(WeekEndHours) },
				{ DayOfWeek.Sunday, GetPaddedWorkingHours(NineToFive) },
			};
		}

		static string GetPaddedWorkingHours(string workingHours)
		{
			return workingHours.PadRight(GlbWorkTimeCollection.MaxWorkingHoursLength, ' ');
		}

		public static IEnumerable<(string DayCode, Interval Interval)> ConvertWorkingHoursToIntervals(string workingHours, string dayCode)
		{
			var intervalsWithoutDays = ConvertWorkingHoursToIntervals(workingHours);
			return intervalsWithoutDays.Select(i => (dayCode, i));
		}

		public static IEnumerable<Interval> ConvertWorkingHoursToIntervals(string workingHours)
		{
			return LegacyWorkTimeConverter.ConvertStringToIntervals(workingHours);
		}

		public static IEnumerable<(string DayCode, Interval Interval)> ConvertWorkingHoursToIntervals(string workingHours, DayOfWeek day)
		{
			var dayCode = DateCalculator.GetDayCode(day);
			return ConvertWorkingHoursToIntervals(workingHours, dayCode);
		}
	}
}
