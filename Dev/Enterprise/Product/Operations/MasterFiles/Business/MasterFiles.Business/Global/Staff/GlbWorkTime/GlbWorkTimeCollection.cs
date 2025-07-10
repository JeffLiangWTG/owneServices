using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.CalendarArithmetic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbWorkTimeCollection : ActiveBusinessObjectCollection<GlbWorkTime>
	{
		public GlbWorkTimeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlbWorkTimeCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
			var queryTableCode = query.Params.FirstOrDefault(p => p.SchemaColumn == GlbWorkTimeSchema.GW_ParentTableCode);
			Argument.NotNull(queryTableCode, nameof(queryTableCode));

			var queryId = query.Params.FirstOrDefault(p => p.SchemaColumn == GlbWorkTimeSchema.GW_ParentID);
			Argument.NotNull(queryId, nameof(queryId));

			fParentTableCode = new ZString(queryTableCode.Value);
			fParentID = new ZGuid(queryId.Value);
		}

		public GlbWorkTimeCollection(BusinessObjectFactory factory, ZGuid parentId, ZString parentTableCode)
			: this(factory, new ZQuery(GlbWorkTimeSchema.GW_ParentID, parentId).AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, parentTableCode))
		{
			Argument.NotNullOrEmpty(parentTableCode, nameof(parentTableCode));
			if (parentId == ZGuid.Empty)
			{
				throw new ArgumentException("parentId should not be empty");
			}
		}

		#region Properties

		#region ParentID

		public ZGuid ParentID
		{
			get { return fParentID; }
		}
		readonly ZGuid fParentID;

		#endregion

		#region ParentTableCode

		public ZString ParentTableCode
		{
			get { return fParentTableCode; }
		}
		readonly ZString fParentTableCode;

		#endregion

		#region WorkingHours

		#region Monday

		public ZString MondayWorkingHours
		{
			get
			{
				return GetWorkingHours(AutoDayOfWeekCodeList.Codes.Monday);
			}
			set
			{
				ValidateInput(value);
				ConvertToIntervals(value, AutoDayOfWeekCodeList.Codes.Monday);
			}
		}

		#endregion

		#region Tuesday

		public ZString TuesdayWorkingHours
		{
			get
			{
				return GetWorkingHours(AutoDayOfWeekCodeList.Codes.Tuesday);
			}
			set
			{
				ValidateInput(value);
				ConvertToIntervals(value, AutoDayOfWeekCodeList.Codes.Tuesday);
			}
		}

		#endregion

		#region Wednesday

		public ZString WednesdayWorkingHours
		{
			get
			{
				return GetWorkingHours(AutoDayOfWeekCodeList.Codes.Wednesday);
			}
			set
			{
				ValidateInput(value);
				ConvertToIntervals(value, AutoDayOfWeekCodeList.Codes.Wednesday);
			}
		}

		#endregion

		#region Thursday

		public ZString ThursdayWorkingHours
		{
			get
			{
				return GetWorkingHours(AutoDayOfWeekCodeList.Codes.Thursday);
			}
			set
			{
				ValidateInput(value);
				ConvertToIntervals(value, AutoDayOfWeekCodeList.Codes.Thursday);
			}
		}

		#endregion

		#region Friday

		public ZString FridayWorkingHours
		{
			get
			{
				return GetWorkingHours(AutoDayOfWeekCodeList.Codes.Friday);
			}
			set
			{
				ValidateInput(value);
				ConvertToIntervals(value, AutoDayOfWeekCodeList.Codes.Friday);
			}
		}

		#endregion

		#region Saturday

		public ZString SaturdayWorkingHours
		{
			get
			{
				return GetWorkingHours(AutoDayOfWeekCodeList.Codes.Saturday);
			}
			set
			{
				ValidateInput(value);
				ConvertToIntervals(value, AutoDayOfWeekCodeList.Codes.Saturday);
			}
		}

		#endregion

		#region Sunday

		public ZString SundayWorkingHours
		{
			get
			{
				return GetWorkingHours(AutoDayOfWeekCodeList.Codes.Sunday);
			}
			set
			{
				ValidateInput(value);
				ConvertToIntervals(value, AutoDayOfWeekCodeList.Codes.Sunday);
			}
		}

		#endregion

		#endregion

		#endregion

		#region Convert WorkingHours

		ZString GetWorkingHours(string dayOfWeek, ZDateTime? timeOfInterest = null)
		{
			if (!timeOfInterest.HasValue)
			{
				timeOfInterest = ZDateTime.Now;
			}

			var yesterdaysIntervals = this.Where(w => w.GW_DayOfWeek == GetYesterdaysCode(dayOfWeek) && w.GW_EndTime > new ZDateTime(1900, 1, 2))
				.Select(w => new Interval(w.GW_StartTime.ToDateTime(), w.GW_EndTime.ToDateTime(), intervalStartsOnPreviousDay: true));
			var intervals = this.Where(w => w.GW_DayOfWeek == dayOfWeek)
				.Select(w => new Interval(w.GW_StartTime.ToDateTime(), w.GW_EndTime.ToDateTime()))
				.Union(yesterdaysIntervals);
			return LegacyWorkTimeConverter.ConvertIntervalsToString(intervals).TrimEnd();
		}

		void ValidateInput(ZString hours)
		{
			foreach (var c in hours)
			{
				if (c != ' ' && c != '*')
				{
					throw new ArgumentException("WorkingHours string should only contain spaces and astericks");
				}
			}
		}

		void ConvertToIntervals(ZString hoursString, string day)
		{
			var oldIntervals = GetExistingIntervals(day);

			var newIntervals = LegacyWorkTimeConverter.ConvertStringToIntervals(hoursString.ToString()).ToArray();
			int i;
			for (i = 0; i < newIntervals.Length; i++)
			{
				if (i < oldIntervals.Length)
				{
					var workTime = oldIntervals[i];
					workTime.GW_StartTime = newIntervals[i].StartTime;
					workTime.GW_EndTime = newIntervals[i].EndTime;
					workTime.GW_DayOfWeek = day;
				}
				else
				{
					var newWorkTime = Factory.New<GlbWorkTime>();
					newWorkTime.GW_ParentID = ParentID;
					newWorkTime.GW_ParentTableCode = ParentTableCode;
					newWorkTime.GW_DayOfWeek = day;
					newWorkTime.GW_StartTime = newIntervals[i].StartTime;
					newWorkTime.GW_EndTime = newIntervals[i].EndTime;
				}
			}

			while (i < oldIntervals.Length)
			{
				oldIntervals[i].Delete();
				i++;
			}
		}

		GlbWorkTime[] GetExistingIntervals(string day)
		{
			return this.Where(w => w.GW_DayOfWeek == day).ToArray();
		}

		#endregion

		#region WorkingHours For Specific Day

		public ZString GetWorkingHoursForDayOfWeek(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
				case DayOfWeek.Monday:
					return MondayWorkingHours;
				case DayOfWeek.Tuesday:
					return TuesdayWorkingHours;
				case DayOfWeek.Wednesday:
					return WednesdayWorkingHours;
				case DayOfWeek.Thursday:
					return ThursdayWorkingHours;
				case DayOfWeek.Friday:
					return FridayWorkingHours;
				case DayOfWeek.Saturday:
					return SaturdayWorkingHours;
				case DayOfWeek.Sunday:
					return SundayWorkingHours;

				default:
					throw new ArgumentException("Day of the week should be Mon-Sun");
			}
		}

		public void SetWorkingHoursForDayOfWeek(DayOfWeek dayOfWeek, ZString value)
		{
			switch (dayOfWeek)
			{
				case DayOfWeek.Monday:
					MondayWorkingHours = value;
					break;
				case DayOfWeek.Tuesday:
					TuesdayWorkingHours = value;
					break;
				case DayOfWeek.Wednesday:
					WednesdayWorkingHours = value;
					break;
				case DayOfWeek.Thursday:
					ThursdayWorkingHours = value;
					break;
				case DayOfWeek.Friday:
					FridayWorkingHours = value;
					break;
				case DayOfWeek.Saturday:
					SaturdayWorkingHours = value;
					break;
				case DayOfWeek.Sunday:
					SundayWorkingHours = value;
					break;

				default:
					throw new ArgumentException("Day of the week should be Mon-Sun");
			}
		}

		public string GetYesterdaysCode(string dayOfWeek)
		{
			switch (dayOfWeek)
			{
				case AutoDayOfWeekCodeList.Codes.Monday:
					return AutoDayOfWeekCodeList.Codes.Sunday;
				case AutoDayOfWeekCodeList.Codes.Tuesday:
					return AutoDayOfWeekCodeList.Codes.Monday;
				case AutoDayOfWeekCodeList.Codes.Wednesday:
					return AutoDayOfWeekCodeList.Codes.Tuesday;
				case AutoDayOfWeekCodeList.Codes.Thursday:
					return AutoDayOfWeekCodeList.Codes.Wednesday;
				case AutoDayOfWeekCodeList.Codes.Friday:
					return AutoDayOfWeekCodeList.Codes.Thursday;
				case AutoDayOfWeekCodeList.Codes.Saturday:
					return AutoDayOfWeekCodeList.Codes.Friday;
				case AutoDayOfWeekCodeList.Codes.Sunday:
					return AutoDayOfWeekCodeList.Codes.Saturday;

				default:
					throw new ArgumentException("Day of the week should be Mon-Sun");
			}
		}

		#endregion

		#region SetDefaultWorkingHours

		public void SetDefaultWorkingHours(ZGuid homeBranchPK, bool includeSaturday)
		{
			var defaultWeeklyWorkingHours = homeBranchPK.IsValid
				? Env.Registry.StandardWorkingHoursDurationForBranch(homeBranchPK.ToGuid())
				: new TimeSpan(40, 0, 0);

			if (defaultWeeklyWorkingHours.TotalHours == 0)
			{
				defaultWeeklyWorkingHours = new TimeSpan(40, 0, 0);
			}

			var weekdays = new List<string>
			{
				AutoDayOfWeekCodeList.Codes.Monday,
				AutoDayOfWeekCodeList.Codes.Tuesday,
				AutoDayOfWeekCodeList.Codes.Wednesday,
				AutoDayOfWeekCodeList.Codes.Thursday,
				AutoDayOfWeekCodeList.Codes.Friday
			};

			if (includeSaturday)
			{
				weekdays.Add(AutoDayOfWeekCodeList.Codes.Saturday);
			}

			var minimumWeeklyWorkingHours = weekdays.Count * 0.5;
			var weeklyHours = Math.Max(defaultWeeklyWorkingHours.TotalHours, minimumWeeklyWorkingHours);
			var weekdayWorkingHoursPattern = GetDailyWorkingHoursPattern(weeklyHours, weekdays.Count);

			weekdays.ForEach(dayCode =>
			{
				weekdayWorkingHoursPattern.ForEach(timeSlot => AddWorkTime(dayCode, timeSlot.start, timeSlot.end));
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "TimeSpan tuple list")]
		static (TimeSpan start, TimeSpan end)[] GetDailyWorkingHoursPattern(double weeklyHours, int weekdays)
		{
			var dailyHours = RoundToLowerHalfHour(weeklyHours / weekdays);
			var startTime = new TimeSpan(8, 30, 0);

			if (dailyHours <= 4)
			{
				var endTime = startTime.Add(TimeSpan.FromHours(dailyHours));
				return new[] { (startTime, endTime) };
			}

			var morningEndTime = new TimeSpan(12, 30, 0);
			var afternoonStartTime = new TimeSpan(13, 0, 0);
			var maxEndTime = new TimeSpan(24, 0, 0);
			var afternoonEndTime = afternoonStartTime.Add(TimeSpan.FromHours(dailyHours - 4));
			afternoonEndTime = afternoonEndTime > maxEndTime ? maxEndTime : afternoonEndTime;

			return new[]
			{
				(startTime, morningEndTime),
				(afternoonStartTime, afternoonEndTime)
			};
		}

		static double RoundToLowerHalfHour(double input)
		{
			return Math.Floor(input * 2) / 2;
		}

		#endregion

		#region AddWorkTime

		public void AddWorkTime(string dayCode, TimeSpan startTime, TimeSpan endTime)
		{
			var workTime = Factory.New<GlbWorkTime>();
			using (workTime.SuspendSettingHasChanges())
			{
				workTime.GW_ParentID = ParentID;
				workTime.GW_ParentTableCode = ParentTableCode;
				workTime.GW_StartTime = GlbWorkTime.CreateTime(startTime.Hours, startTime.Minutes, startTime.TotalDays >= 1);
				workTime.GW_EndTime = GlbWorkTime.CreateTime(endTime.Hours, endTime.Minutes, endTime.TotalDays >= 1);
				workTime.GW_DayOfWeek = dayCode;
			}
		}

		#endregion

		#region AddValidTestData

#if DEBUG

		public void AddValidTestData()
		{
			DeleteAll();

			var workTime1 = Factory.NewWithValidTestData<GlbWorkTime>();
			workTime1.GW_ParentID = ParentID;
			workTime1.GW_ParentTableCode = ParentTableCode;
			workTime1.GW_DayOfWeek = AutoDayOfWeekCodeList.Codes.Monday;
			var workTime2 = Factory.NewWithValidTestData<GlbWorkTime>();
			workTime2.GW_ParentID = ParentID;
			workTime2.GW_ParentTableCode = ParentTableCode;
			workTime2.GW_DayOfWeek = AutoDayOfWeekCodeList.Codes.Tuesday;
			var workTime3 = Factory.NewWithValidTestData<GlbWorkTime>();
			workTime3.GW_ParentID = ParentID;
			workTime3.GW_ParentTableCode = ParentTableCode;
			workTime3.GW_DayOfWeek = AutoDayOfWeekCodeList.Codes.Wednesday;
			var workTime4 = Factory.NewWithValidTestData<GlbWorkTime>();
			workTime4.GW_ParentID = ParentID;
			workTime4.GW_ParentTableCode = ParentTableCode;
			workTime4.GW_DayOfWeek = AutoDayOfWeekCodeList.Codes.Thursday;
			var workTime5 = Factory.NewWithValidTestData<GlbWorkTime>();
			workTime5.GW_ParentID = ParentID;
			workTime5.GW_ParentTableCode = ParentTableCode;
			workTime5.GW_DayOfWeek = AutoDayOfWeekCodeList.Codes.Friday;
		}

#endif

		#endregion

		#region Overrides

		protected override object[] GetCollectionState()
		{
			return new object[] { fParentID, fParentTableCode };
		}

		#endregion

		public const int MaxWorkingHoursLength = 48;
	}
}
