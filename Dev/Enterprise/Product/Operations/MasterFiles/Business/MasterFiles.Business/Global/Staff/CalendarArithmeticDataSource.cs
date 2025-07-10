using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.CalendarArithmetic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CalendarArithmeticDataSource : ICalendarDataSource
	{
		public CalendarArithmeticDataSource(BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK = default(ZGuid), bool addFetchHints = true)
		{
			this.factory = factory;
			this.departmentPK = departmentPK;
			this.branchPK = branchPK;
			this.staffPK = staffPK;

			if (staffPK != default(ZGuid) && addFetchHints)
			{
				factory.AddFetchHint(GlbStaffHolidaySchema.GA_GS, staffPK);
			}
		}

		protected readonly ZGuid staffPK;
		protected readonly BusinessObjectFactory factory;
		readonly ZGuid departmentPK;
		readonly ZGuid branchPK;

		public DateTimeRange GetStaffHolidayForDateTime(DateTime dateTimeToCheck)
		{
			return GetStaffHolidayForDateCore(dateTimeToCheck, checkTime: true);
		}

		public DateTimeRange GetStaffHolidayForDay(DateTime dayToCheck)
		{
			return GetStaffHolidayForDateCore(dayToCheck, checkTime: false);
		}

		DateTimeRange GetStaffHolidayForDateCore(DateTime dateToCheck, bool checkTime)
		{
			if (!staffPK.IsValid)
			{
				return null;
			}

			var query = GetStaffHolidayForDateTimeQuery(dateToCheck, checkTime);
			var holiday = factory.Load<GlbStaffHoliday>(query).FirstOrDefault();
			return holiday == null ? null : new DateTimeRange(holiday.GA_StartTime.ToDateTime(), holiday.GA_EndTime.ToDateTime());
		}

		public virtual IEnumerable<DateTimeRange> GetStaffHolidaysFromRange(DateTimeRange range)
		{
			if (!staffPK.IsValid)
			{
				return Enumerable.Empty<DateTimeRange>();
			}

			var query = GetStaffHolidaysInRangeQuery(range);
			return factory.Load<GlbStaffHoliday>(query)
				.Select(x => new DateTimeRange(x.GA_StartTime.ToDateTime(), x.GA_EndTime.ToDateTime()));
		}

		protected ZQuery GetStaffHolidaysInRangeQuery(DateTimeRange range, bool checkAvailabilityPercentage = true)
		{
			var query = new ZQuery(GlbStaffHolidaySchema.GA_GS, staffPK);
			query.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThan, range.End.Date.AddDays(1));
			query.AddToFilter(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThan, range.Start.Date);
			query.AddToFilter(GlbStaffHolidaySchema.GA_IsWorkingAway, SQLComparisonOperator.Equal, false);

			if (checkAvailabilityPercentage)
			{
				query.AddToFilter(GlbStaffHolidaySchema.GA_AvailabilityPercentage, SQLComparisonOperator.Equal, 0);
			}

			query.AddToFilter(GetStaffHolidayApprovalSubQuery());

			return query;
		}

		ZQuery GetStaffHolidayForDateTimeQuery(DateTime dateToCheck, bool checkTime)
		{
			var endDateToCheck = checkTime ? dateToCheck : dateToCheck.AddDays(1);

			var query = new ZQuery(GlbStaffHolidaySchema.GA_GS, staffPK);
			query.AddToFilter(GlbStaffHolidaySchema.GA_RecordType, new[] { StaffHolidayRecordTypeCodes.BufferManagementLeave, StaffHolidayRecordTypeCodes.Leave });
			query.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThanOrEqualTo, dateToCheck);
			query.AddToFilter(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, endDateToCheck);
			query.AddToFilter(GlbStaffHolidaySchema.GA_AvailabilityPercentage, SQLComparisonOperator.Equal, 0);
			query.AddToFilter(GlbStaffHolidaySchema.GA_IsWorkingAway, SQLComparisonOperator.Equal, false);
			query.AddToFilter(GetStaffHolidayApprovalSubQuery());

			return query;
		}

		static ZQuery GetStaffHolidayApprovalSubQuery()
		{
			var subQuery = new ZQuery();
			subQuery.AddToFilter(GlbStaffHolidaySchema.GA_ApprovalStatus, SQLComparisonOperator.Equal, StaffHolidayApprovalCodes.Approved);
			subQuery.AddToFilter(JoinCondition.Or, GlbStaffHolidaySchema.GA_ApprovalStatus, SQLComparisonOperator.Equal, StaffHolidayApprovalCodes.ConditionalApproval);

			return subQuery;
		}

		public ICollection<DateTime> GetBranchHolidayDatesByYear(int year)
		{
			if (!branchPK.IsValid)
			{
				return Enumerable.Empty<DateTime>().ToList();
			}

			var comparer = new MonthDayDateComparer();
			return new HashSet<DateTime>(BranchHolidayCache
				.Where(h => HolidayFromThisYearOrIsRecuring(h, year))
				.Select(h => h.GetHolidayDateForYear(year))
				.Where(d => d.IsValid)
				.Select(d => d.ToDateTime()),
				comparer);
		}

		static bool HolidayFromThisYearOrIsRecuring(GlbHoliday holiday, int year)
		{
			var holidayType = HolidayCodes.GetTypeFromCode(holiday.GH_RecurrType, holiday.GH_Recurring);
			return (holidayType == HolidayType.Date && holiday.GH_Date.Year == year)
				|| !(holidayType == HolidayType.Date || holidayType == HolidayType.None);
		}

		public WorkTimeWeek StaffWorkTimeWeek
		{
			get
			{
				if (!staffPK.IsValid)
				{
					return null;
				}

				var workTime = LoadGlbWorkTime(factory, staffPK, GlbStaffSchema.Constants.Prefix);
				return GetWorkTimeWeekFromGlbWorkTime(workTime);
			}
		}

		public WorkTimeWeek DepartmentWorkTimeWeek
		{
			get
			{
				if (!departmentPK.IsValid)
				{
					return null;
				}

				var workTime = LoadGlbWorkTime(factory, departmentPK, GlbDepartmentSchema.Constants.Prefix);
				return GetWorkTimeWeekFromGlbWorkTime(workTime);
			}
		}

		static GlbWorkTimeCollection LoadGlbWorkTime(BusinessObjectFactory factory, ZGuid pk, string parentTableCode)
		{
			var query = new ZQuery(GlbWorkTimeSchema.GW_ParentID, pk);
			query.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, parentTableCode);
			return new GlbWorkTimeCollection(factory, query);
		}

		static WorkTimeWeek GetWorkTimeWeekFromGlbWorkTime(GlbWorkTimeCollection workTime)
		{
			if (workTime == null)
			{
				return null;
			}

			var workingHoursDictionary = new Dictionary<DayOfWeek, string>
			{
				{ DayOfWeek.Monday, GetPaddedWorkingHours(workTime.MondayWorkingHours) },
				{ DayOfWeek.Tuesday, GetPaddedWorkingHours(workTime.TuesdayWorkingHours) },
				{ DayOfWeek.Wednesday, GetPaddedWorkingHours(workTime.WednesdayWorkingHours) },
				{ DayOfWeek.Thursday, GetPaddedWorkingHours(workTime.ThursdayWorkingHours) },
				{ DayOfWeek.Friday, GetPaddedWorkingHours(workTime.FridayWorkingHours) },
				{ DayOfWeek.Saturday, GetPaddedWorkingHours(workTime.SaturdayWorkingHours) },
				{ DayOfWeek.Sunday, GetPaddedWorkingHours(workTime.SundayWorkingHours) }
			};
			return new WorkTimeWeek(workingHoursDictionary);
		}

		static string GetPaddedWorkingHours(string workingHours)
		{
			return workingHours.PadRight(GlbWorkTimeCollection.MaxWorkingHoursLength, ' ');
		}

		IEnumerable<GlbHoliday> BranchHolidayCache => new Lazy<IEnumerable<GlbHoliday>>(() => factory.Load<GlbHoliday>(new ZQuery(GlbHolidaySchema.GH_ParentID, branchPK).AddToFilter(GlbHolidaySchema.GH_ParentTableCode, GlbBranchSchema.Constants.Prefix))).Value;
	}
}
