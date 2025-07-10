using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public sealed class CalendarDayType
	{
		CalendarDayType(string name, bool weekend, bool publicHoliday)
		{
			_name = name;
			_isWeekend = weekend;
			_isPublicHoliday = publicHoliday;
		}

		readonly string _name;
		readonly bool _isWeekend;
		readonly bool _isPublicHoliday;
		public string Name => _name;

		public bool IsNonWorkingDay() => _isWeekend || _isPublicHoliday;

		public bool IsWorkingDay() => !IsNonWorkingDay();

		public bool IsWeekend() => _isWeekend;

		public bool IsPublicHoliday() => _isPublicHoliday;

		public static CalendarDayType WorkingDay = new CalendarDayType((NoResString)"WorkingDay", false, false);
		public static CalendarDayType Weekend = new CalendarDayType((NoResString)"Weekend", true, false);
		public static CalendarDayType PublicHoliday = new CalendarDayType((NoResString)"PublicHoliday", false, true);
		public static CalendarDayType WeekendAndPublicHoliday = new CalendarDayType((NoResString)"WeekendAndPublicHoliday", true, true);

		public bool IntersectsWith(CalendarDayType calendarDayType)
		{
			return _isWeekend && calendarDayType._isWeekend || _isPublicHoliday && calendarDayType._isPublicHoliday;
		}

		public override string ToString() => _name;

		public static CalendarDayType Get(bool weekend, bool publicHoliday)
		{
			return (weekend, publicHoliday) switch
			{
				(false, false) => WorkingDay,
				(true, false) => Weekend,
				(false, true) => PublicHoliday,
				(true, true) => WeekendAndPublicHoliday
			};
		}
	}

	public sealed class DateWithCalendarDayType
	{
		public DateWithCalendarDayType(DateTime date, CalendarDayType calendarDayType)
		{
			Date = date;
			CalendarDayType = calendarDayType;
		}

		public DateTime Date { get; private set; }

		public CalendarDayType CalendarDayType { get; private set; }
	}

	public sealed class RefCalendarDayTypeHelper
	{
		public RefCalendarDayTypeHelper()
		{
			factory = new BusinessObjectFactory();
		}

		public DateWithCalendarDayType[] GetDatesWithCalendarDayTypes(OrgAddress address, ZString pickupOrDeliveryType, DateTime fromLocalTime, DateTime toLocalTimeInclusive)
		{
			var stateQuery = new ZQuery(RefCountryStatesSchema.RW_Code, address.OA_State);
			stateQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, address.OA_RN_NKCountryCode);
			var state = factory.LoadTop1<RefCountryStates>(stateQuery);
			var glbHolidays = factory.Load<GlbHoliday>(GetRelatedGlbHolidayQuery(state, address.OA_RN_NKCountryCode, fromLocalTime, toLocalTimeInclusive));
			return GetDatesWithCalendarDayTypes(glbHolidays, fromLocalTime, toLocalTimeInclusive, pickupOrDeliveryType, address);
		}

		public DateWithCalendarDayType[] GetDatesWithCalendarDayTypes(JobDocAddress jobDocAddress, ZString pickupOrDeliveryType, DateTime fromLocalTime, DateTime toLocalTimeInclusive)
		{
			var address = jobDocAddress.Address;
			if (address != null)
			{
				return GetDatesWithCalendarDayTypes(address, pickupOrDeliveryType, fromLocalTime, toLocalTimeInclusive);
			}

			var stateQuery = new ZQuery(RefCountryStatesSchema.RW_Code, jobDocAddress.E2_State);
			stateQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, jobDocAddress.E2_RN_NKCountryCode);
			var state = factory.LoadTop1<RefCountryStates>(stateQuery);
			var glbHolidays = factory.Load<GlbHoliday>(GetRelatedGlbHolidayQuery(state, jobDocAddress.E2_RN_NKCountryCode, fromLocalTime, toLocalTimeInclusive));
			return GetDatesWithCalendarDayTypes(glbHolidays, fromLocalTime, toLocalTimeInclusive, ZString.Empty);
		}

		DateWithCalendarDayType[] GetDatesWithCalendarDayTypes(GlbHoliday[] glbHolidays, DateTime fromLocalTime, DateTime toLocalTimeInclusive, ZString pickupOrDeliveryType, OrgAddress address = null)
		{
			bool[] weekArray = new bool[7];
			if (address == null)
			{
				SetWeekendFromGlbHoliday(weekArray, glbHolidays);
			}
			else
			{
				SetWeekend(weekArray, glbHolidays, address, pickupOrDeliveryType);
			}

			var nonRecurringStateHolidays = glbHolidays.Where(x => x.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Date && !x.GH_Recurring && x.GH_ParentTableCode == RefCountryStatesSchema.Constants.Prefix && !x.GH_IsWorkingDay);
			var nonRecurringStateWorkingDays = glbHolidays.Where(x => x.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Date && !x.GH_Recurring && x.GH_ParentTableCode == RefCountryStatesSchema.Constants.Prefix && x.GH_IsWorkingDay);
			var nonRecurringCountryHolidays = glbHolidays.Where(x => x.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Date && !x.GH_Recurring && x.GH_ParentTableCode == RefCountrySchema.Constants.Prefix && !x.GH_IsWorkingDay);
			var nonRecurringCountryWorkingDays = glbHolidays.Where(x => x.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Date && !x.GH_Recurring && x.GH_ParentTableCode == RefCountrySchema.Constants.Prefix && x.GH_IsWorkingDay);
			var recurringStateHolidays = glbHolidays.Where(x => x.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Date && x.GH_Recurring && x.GH_ParentTableCode == RefCountryStatesSchema.Constants.Prefix && !x.GH_IsWorkingDay);
			var recurringStateWorkingDays = glbHolidays.Where(x => x.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Date && x.GH_Recurring && x.GH_ParentTableCode == RefCountryStatesSchema.Constants.Prefix && x.GH_IsWorkingDay);
			var recurringCountryHolidays = glbHolidays.Where(x => x.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Date && x.GH_Recurring && x.GH_ParentTableCode == RefCountrySchema.Constants.Prefix && !x.GH_IsWorkingDay);
			var recurringCountryWorkingDays = glbHolidays.Where(x => x.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Date && x.GH_Recurring && x.GH_ParentTableCode == RefCountrySchema.Constants.Prefix && x.GH_IsWorkingDay);

			var stateHolidays = GetDatesFromNonRecurringGlbHolidaysAndRecurringDates(nonRecurringStateHolidays, recurringStateHolidays, fromLocalTime, toLocalTimeInclusive);
			var countryHolidays = GetDatesFromNonRecurringGlbHolidaysAndRecurringDates(nonRecurringCountryHolidays, recurringCountryHolidays, fromLocalTime, toLocalTimeInclusive);
			var stateWorkingDays = GetDatesFromNonRecurringGlbHolidaysAndRecurringDates(nonRecurringStateWorkingDays, recurringStateWorkingDays, fromLocalTime, toLocalTimeInclusive);
			var countryWorkingDays = GetDatesFromNonRecurringGlbHolidaysAndRecurringDates(nonRecurringCountryWorkingDays, recurringCountryWorkingDays, fromLocalTime, toLocalTimeInclusive);

			var results = new List<DateWithCalendarDayType>();

			for (var date = fromLocalTime; date <= toLocalTimeInclusive; date = date.AddDays(1))
			{
				var weekend = weekArray[(int)date.DayOfWeek];
				var publicHoliday = (countryHolidays.Contains(date) && !countryWorkingDays.Contains(date) || stateHolidays.Contains(date)) && !stateWorkingDays.Contains(date);

				results.Add(new DateWithCalendarDayType(date, CalendarDayType.Get(weekend, publicHoliday)));
			}

			return results.ToArray();
		}

		ZQuery GetRelatedGlbHolidayQuery(RefCountryStates refCountryStates, ZString countryCode, DateTime from, DateTime to)
		{
			var subNonRecurringHolidayQuery = new ZQuery(GlbHolidaySchema.GH_Recurring, false);
			subNonRecurringHolidayQuery.AddToFilter(GlbHolidaySchema.GH_Date, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, from);

			var subRecurringCombinedHolidayQuery = new ZQuery(GlbHolidaySchema.GH_Recurring, true);
			subRecurringCombinedHolidayQuery.AddToFilter(subNonRecurringHolidayQuery, JoinCondition.Or);

			var subHolidayQuery = new ZQuery(GlbHolidaySchema.GH_RecurrType, GlbHolidayRecurTypeCodeList.Codes.Date);
			subHolidayQuery.AddToFilter(GlbHolidaySchema.GH_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, to);
			subHolidayQuery.AddToFilter(subRecurringCombinedHolidayQuery);

			var subHolidayWeeklyCombinedQuery = new ZQuery(subHolidayQuery, JoinCondition.Or, new ZQuery(GlbHolidaySchema.GH_RecurrType, GlbHolidayRecurTypeCodeList.Codes.Weekly));

			if (refCountryStates == null)
			{
				var country = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
				if (country == null)
				{
					return ZQuery.NoResultQuery;
				}
				var holidayCountryQuery = new ZQuery(GlbHolidaySchema.GH_ParentTableCode, RefCountrySchema.Constants.Prefix);
				holidayCountryQuery.AddToFilter(GlbHolidaySchema.GH_ParentID, country.PK);
				holidayCountryQuery.AddToFilter(subHolidayWeeklyCombinedQuery);

				var holidayQuery = new ZQuery(GlbHolidaySchema.GH_IsActive, true);
				holidayQuery.AddToFilter(holidayCountryQuery);

				return holidayQuery;
			}
			else
			{
				var holidayStateQuery = new ZQuery(GlbHolidaySchema.GH_ParentTableCode, RefCountryStatesSchema.Constants.Prefix);
				holidayStateQuery.AddToFilter(GlbHolidaySchema.GH_ParentID, refCountryStates.PK);
				holidayStateQuery.AddToFilter(subHolidayWeeklyCombinedQuery);

				var holidayCountryQuery = new ZQuery(GlbHolidaySchema.GH_ParentTableCode, RefCountrySchema.Constants.Prefix);
				holidayCountryQuery.AddToFilter(GlbHolidaySchema.GH_ParentID, refCountryStates.Country.PK);
				holidayCountryQuery.AddToFilter(subHolidayWeeklyCombinedQuery);

				var holidayQuery = new ZQuery(GlbHolidaySchema.GH_IsActive, true);
				holidayQuery.AddToFilter(new ZQuery(holidayStateQuery, JoinCondition.Or, holidayCountryQuery));

				return holidayQuery;
			}
		}

		void SetWeekend(bool[] weekArray, IEnumerable<GlbHoliday> glbHolidays, OrgAddress address, ZString pickupOrDeliveryType)
		{
			var timetableCollection = address.Timetables;
			if (timetableCollection.NotApplicable)
			{
				SetWeekendFromGlbHoliday(weekArray, glbHolidays);
			}
			else if (timetableCollection.DefaultRange)
			{
				for (int i = 0; i < weekArray.Length; i++)
				{
					weekArray[i] = true;
				}
				var defaultTimetables = OrganisationRegistry.Instance.DefaultOrgTimetable.Value.DefaultOrgTimetablesForCountry(address.OA_RN_NKCountryCode);
				foreach (DefaultOrgTimetable defaultOrgTimetable in defaultTimetables)
				{
					if (defaultOrgTimetable.Type == pickupOrDeliveryType)
					{
						switch (defaultOrgTimetable.Day)
						{
							case AutoDayOfWeekCodeList.Codes.Sunday:
								weekArray[0] = false;
								break;
							case AutoDayOfWeekCodeList.Codes.Monday:
								weekArray[1] = false;
								break;
							case AutoDayOfWeekCodeList.Codes.Tuesday:
								weekArray[2] = false;
								break;
							case AutoDayOfWeekCodeList.Codes.Wednesday:
								weekArray[3] = false;
								break;
							case AutoDayOfWeekCodeList.Codes.Thursday:
								weekArray[4] = false;
								break;
							case AutoDayOfWeekCodeList.Codes.Friday:
								weekArray[5] = false;
								break;
							case AutoDayOfWeekCodeList.Codes.Saturday:
								weekArray[6] = false;
								break;
						}
					}
				}
			}
			else if (timetableCollection.AdvancedRange)
			{
				var weeklySetting = ((OrgTimetable[])timetableCollection).Where(x => x.OTT_Type == pickupOrDeliveryType);
				weekArray[0] = !weeklySetting.Any(x => x.OTT_Sunday);
				weekArray[1] = !weeklySetting.Any(x => x.OTT_Monday);
				weekArray[2] = !weeklySetting.Any(x => x.OTT_Tuesday);
				weekArray[3] = !weeklySetting.Any(x => x.OTT_Wednesday);
				weekArray[4] = !weeklySetting.Any(x => x.OTT_Thursday);
				weekArray[5] = !weeklySetting.Any(x => x.OTT_Friday);
				weekArray[6] = !weeklySetting.Any(x => x.OTT_Saturday);
			}
			else if (timetableCollection.WeekdayRange)
			{
				var country = address.Country;
				if (country == null || country.Weekends == null || country.Weekends.All(x => x.GH_IsWorkingDay))
				{
					weekArray[0] = true;
					weekArray[1] = false;
					weekArray[2] = false;
					weekArray[3] = false;
					weekArray[4] = false;
					weekArray[5] = false;
					weekArray[6] = true;
				}
				else
				{
					weekArray[0] = country.IsSundayNonWorkingDay;
					weekArray[1] = country.IsMondayNonWorkingDay;
					weekArray[2] = country.IsTuesdayNonWorkingDay;
					weekArray[3] = country.IsWednesdayNonWorkingDay;
					weekArray[4] = country.IsThursdayNonWorkingDay;
					weekArray[5] = country.IsFridayNonWorkingDay;
					weekArray[6] = country.IsSaturdayNonWorkingDay;
				}
			}
			else
			{
				var weeklySetting = ((OrgTimetable[])timetableCollection).First(x => x.OTT_Type == pickupOrDeliveryType);
				weekArray[0] = !weeklySetting.OTT_Sunday;
				weekArray[1] = !weeklySetting.OTT_Monday;
				weekArray[2] = !weeklySetting.OTT_Tuesday;
				weekArray[3] = !weeklySetting.OTT_Wednesday;
				weekArray[4] = !weeklySetting.OTT_Thursday;
				weekArray[5] = !weeklySetting.OTT_Friday;
				weekArray[6] = !weeklySetting.OTT_Saturday;
			}
		}

		void SetWeekendFromGlbHoliday(bool[] weekArray, IEnumerable<GlbHoliday> glbHolidays)
		{
			var weeklyStatesRecords = glbHolidays.Where(x => x.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Weekly && x.GH_ParentTableCode == RefCountryStatesSchema.Constants.Prefix);
			var weeklyCountryRecords = glbHolidays.Where(x => x.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Weekly && x.GH_ParentTableCode == RefCountrySchema.Constants.Prefix);
			var weekendGlbholidays = new List<string>();
			if (weeklyStatesRecords.Any())
			{
				weekendGlbholidays.AddRange(weeklyStatesRecords.Where(x => !x.GH_IsWorkingDay).Select(x => x.GH_RecurrDay.ToString()));
			}
			else if (weeklyCountryRecords.Any())
			{
				weekendGlbholidays.AddRange(weeklyCountryRecords.Where(x => !x.GH_IsWorkingDay).Select(x => x.GH_RecurrDay.ToString()));
			}

			if (weekendGlbholidays.Any())
			{
				weekArray[0] = weekendGlbholidays.Any(x => x == AutoDayOfWeekCodeList.Codes.Sunday);
				weekArray[1] = weekendGlbholidays.Any(x => x == AutoDayOfWeekCodeList.Codes.Monday);
				weekArray[2] = weekendGlbholidays.Any(x => x == AutoDayOfWeekCodeList.Codes.Tuesday);
				weekArray[3] = weekendGlbholidays.Any(x => x == AutoDayOfWeekCodeList.Codes.Wednesday);
				weekArray[4] = weekendGlbholidays.Any(x => x == AutoDayOfWeekCodeList.Codes.Thursday);
				weekArray[5] = weekendGlbholidays.Any(x => x == AutoDayOfWeekCodeList.Codes.Friday);
				weekArray[6] = weekendGlbholidays.Any(x => x == AutoDayOfWeekCodeList.Codes.Saturday);
			}
		}

		IEnumerable<DateTime> GetDatesFromNonRecurringGlbHolidaysAndRecurringDates(IEnumerable<GlbHoliday> nonrecurringGlbHolidays, IEnumerable<GlbHoliday> recurringGlbHolidays, DateTime from, DateTime to)
		{
			var result = new List<DateTime>();
			if (nonrecurringGlbHolidays.Any())
			{
				result.AddRange(nonrecurringGlbHolidays.Select(x => x.GH_Date.ToDateTime()));
			}
			var recurringDates = recurringGlbHolidays.Select(x => x.GH_Date.ToDateTime());
			foreach (var date in recurringDates)
			{
				var tempDate = new DateTime(from.Year, date.Month, date.Day);
				while (tempDate <= to && tempDate >= from)
				{
					result.Add(tempDate);
					tempDate = tempDate.AddYears(1);
				}
			}
			return result;
		}

		readonly BusinessObjectFactory factory;
	}
}
