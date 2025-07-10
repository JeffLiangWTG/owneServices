using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefTimeZoneRule : AutoRefTimeZoneRule
	{
		public RefTimeZoneRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		public const string StartRuleCode = "STA";
		public const string EndRuleCode = "END";

		#region DayNumber

		/// <summary>
		/// This is a calculated property for the database field R4_DaylightSavingCount. It essentially allows
		/// the DB field to be selected in a dropdown box and set in the database - since it is of type ZByte 
		/// and dropdown boxes only take ZString's.
		/// </summary>
		[BusinessObjectTestExclude]
		[List("Lookups.NthDayOfMonthList")]
		[MaxLength(1)]
		public ZString DayNumber
		{
			get { return R4_DaylightSavingDayCount.ToString(); }
			set
			{
				ZByte result;
				bool success = ZByte.TryParse(value, out result);
				if (success)
				{
					R4_DaylightSavingDayCount = result;
				}

				Validation.ValidateDayNumber();
				DayNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DayNumberInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DayNumber));
			}
		}

		protected bool DayNumber_ReadOnly
		{
			get { return R4_DaylightSavingDayWeekDate != TimeZoneConstants.DstRuleWeekday; }
		}

		#endregion

		#region DateDayNumber

		ZString fIncorrectDateDayNumber;

		[BusinessObjectTestExclude]
		[List("Lookups.DaysOfMonthList")]
		[MaxLength(2)]
		public ZString DateDayNumber
		{
			get
			{
				ZString result = "";

				if (R4_DaylightSavingDayWeekDate == TimeZoneConstants.DstRuleDayOfMonth)
				{
					if (!fIncorrectDateDayNumber.IsEmpty)
					{
						result = fIncorrectDateDayNumber;
					}
					else
					{
						if (!R4_DaylightSavingDate.IsEmpty && R4_DaylightSavingDate.IsValid)
						{
							result = R4_DaylightSavingDate.Day.ToString();
						}
					}
				}

				return result;
			}
			set
			{
				fIncorrectDateDayNumber = "";

				ZInt day = ZInt.Zero;
				ZInt.TryParse(value, out day);
				if (day > 0 && day <= 31)
				{
					if (!R4_DaylightSavingDate.IsEmpty && R4_DaylightSavingDate.IsValid)
					{
						if (IsDateValid(ZDateTime.Now.Year, R4_DaylightSavingDate.Month, day, R4_DaylightSavingDate.Hour, R4_DaylightSavingDate.Minute, R4_DaylightSavingDate.Second))
						{
							R4_DaylightSavingDate = new ZDateTime(ZDateTime.Now.Year, R4_DaylightSavingDate.Month, day, R4_DaylightSavingDate.Hour, R4_DaylightSavingDate.Minute, R4_DaylightSavingDate.Second);
						}
						else
						{
							fIncorrectDateDayNumber = day.ToString();
						}
					}
					else
					{
						R4_DaylightSavingDate = new ZDateTime(ZDateTime.Now.Year, 1, day, 0, 0, 0);
					}
				}
				else
				{
					fIncorrectDateDayNumber = day.ToString();
				}

				Validation.ValidateDateDayNumber();
				DateDayNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateDayNumberInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DateDayNumber));
			}
		}

		protected bool DateDayNumber_ReadOnly
		{
			get { return R4_DaylightSavingDayWeekDate != TimeZoneConstants.DstRuleDayOfMonth; }
		}

		#endregion

		#region DateMonth

		ZString fIncorrectDateMonth;

		[BusinessObjectTestExclude]
		[List("Lookups.ZoneMonths")]
		public ZString DateMonth
		{
			get
			{
				ZString result = "";

				if (!fIncorrectDateMonth.IsEmpty)
				{
					result = fIncorrectDateMonth;
				}
				else
				{
					if (!R4_DaylightSavingDate.IsEmpty && R4_DaylightSavingDate.IsValid)
					{
						result = GetMonthAsString(R4_DaylightSavingDate.Month);
					}
				}
				return result;
			}
			set
			{
				fIncorrectDateMonth = "";
				CheckMaximumLength(this.DateMonthInfo, value);

				if (Lookups.ZoneMonths.ContainsCode(value))
				{
					if (!R4_DaylightSavingDate.IsEmpty && R4_DaylightSavingDate.IsValid)
					{
						if (IsDateValid(ZDateTime.Now.Year, TimeZoneConstants.GetMonthAsInt(value), R4_DaylightSavingDate.Day, R4_DaylightSavingDate.Hour, R4_DaylightSavingDate.Minute, R4_DaylightSavingDate.Second))
						{
							R4_DaylightSavingDate = new ZDateTime(ZDateTime.Now.Year, TimeZoneConstants.GetMonthAsInt(value), R4_DaylightSavingDate.Day, R4_DaylightSavingDate.Hour, R4_DaylightSavingDate.Minute, R4_DaylightSavingDate.Second);
						}
						else
						{
							fIncorrectDateMonth = value;
						}
					}
					else
					{
						R4_DaylightSavingDate = new ZDateTime(ZDateTime.Now.Year, TimeZoneConstants.GetMonthAsInt(value), 1, 0, 0, 0);
					}
				}
				else
				{
					fIncorrectDateMonth = value;
				}
				Validation.ValidateDateMonth();
				DateMonthInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DateMonthInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DateMonth));
			}
		}

		protected bool DateMonth_ReadOnly
		{
			get { return R4_DaylightSavingDayWeekDate != TimeZoneConstants.DstRuleDayOfMonth; }
		}

		#endregion

		#region DaylightSavingChangeTime

		ZDateTime fIncorrectDaylightSavingChangeTime;

		[BusinessObjectTestExclude]
		public ZDateTime DaylightSavingChangeTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (!fIncorrectDaylightSavingChangeTime.IsEmpty)
				{
					result = fIncorrectDaylightSavingChangeTime;
				}
				else
				{
					if (!R4_DaylightSavingDate.IsEmpty && R4_DaylightSavingDate.IsValid)
					{
						result = R4_DaylightSavingDate.ToDateTime();
					}
				}
				return result;
			}
			set
			{
				fIncorrectDaylightSavingChangeTime = ZDateTime.Empty;

				if (value.IsValid)
				{
					if (!R4_DaylightSavingDate.IsEmpty && R4_DaylightSavingDate.IsValid)
					{
						R4_DaylightSavingDate = new ZDateTime(R4_DaylightSavingDate.Year, R4_DaylightSavingDate.Month, R4_DaylightSavingDate.Day, value.Hour, value.Minute, value.Second);
					}
					else
					{
						R4_DaylightSavingDate = new ZDateTime(ZDateTime.Now.Year, 1, 1, value.Hour, value.Minute, value.Second);
					}
				}
				else
				{
					fIncorrectDaylightSavingChangeTime = value;
				}
				Validation.ValidateDaylightSavingChangeTime();
				DaylightSavingChangeTimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DaylightSavingChangeTimeInfo
		{
			get { return GetZPropertyInfo(nameof(DaylightSavingChangeTime)); }
		}

		#endregion

		#region R4_DaylightSavingDayName
		[List("Lookups.ZoneDays")]
		public override ZString R4_DaylightSavingDayName
		{
			get
			{
				return base.R4_DaylightSavingDayName;
			}
			set
			{
				base.R4_DaylightSavingDayName = value;
			}
		}
		#endregion

		#region R4_DaylightSavingMonth
		[List("Lookups.ZoneMonths")]
		public override ZString R4_DaylightSavingMonth
		{
			get
			{
				return base.R4_DaylightSavingMonth;
			}
			set
			{
				base.R4_DaylightSavingMonth = value;
			}
		}

		#endregion

		#region R4_DaylightSavingDayWeekDate
		[List("Lookups.ZoneTypes")]
		public override ZString R4_DaylightSavingDayWeekDate
		{
			get { return base.R4_DaylightSavingDayWeekDate; }
			set
			{
				base.R4_DaylightSavingDayWeekDate = value;

				if (value == TimeZoneConstants.DstRuleDayOfMonth)
				{
					R4_DaylightSavingDayCount = (ZByte)0;
					R4_DaylightSavingDayName = "";
					R4_DaylightSavingMonth = "";
				}
				else if (value == TimeZoneConstants.DstRuleWeekday)
				{
					R4_DaylightSavingDate = ZDateTime.Empty;
				}
				else
				{
					R4_DaylightSavingDate = ZDateTime.Empty;
					R4_DaylightSavingDayCount = (ZByte)0;
					R4_DaylightSavingDayName = "";
					R4_DaylightSavingMonth = "";
				}

				DayNumberInfo.RefreshBinding();
				DateDayNumberInfo.RefreshBinding();
				DateMonthInfo.RefreshBinding();
			}
		}

		protected bool R4_DaylightSavingDate_ReadOnly
		{
			get { return R4_DaylightSavingDayWeekDate == TimeZoneConstants.DstRuleWeekday; }
		}

		#endregion

		#region R4_TypeOfTime
		[List("Lookups.TypeOfTimeList")]
		public override ZString R4_TypeOfTime
		{
			get
			{
				return base.R4_TypeOfTime;
			}
			set
			{
				base.R4_TypeOfTime = value;
			}
		}
		#endregion

		#region Set Read Only for Non-calculated properties

		#region R4_DaylightSavingDayNameInfo

		public override ZPropertyInfo R4_DaylightSavingDayNameInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(R4_DaylightSavingDayName));
			}
		}

		protected bool R4_DaylightSavingDayName_ReadOnly
		{
			get { return R4_DaylightSavingDayWeekDate != TimeZoneConstants.DstRuleWeekday; }
		}

		#endregion

		#region R4_DaylightSavingMonthInfo

		public override ZPropertyInfo R4_DaylightSavingMonthInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(R4_DaylightSavingMonth));
			}
		}

		protected bool R4_DaylightSavingMonth_ReadOnly
		{
			get { return R4_DaylightSavingDayWeekDate != TimeZoneConstants.DstRuleWeekday; }
		}

		#endregion

		#endregion

		#region Parent BizObjs

		public DaylightSavingTimeZone DaylightSavingZone
		{
			get
			{
				ZQuery query = new ZQuery(RefTimeZoneSchema.PK, R4_R2);
				return Factory.LoadTop1<DaylightSavingTimeZone>(query);
			}
		}

		#endregion

		#endregion

		#region GetDaylightSavingDateTimeInYear

		public ZDateTime GetDaylightSavingDateTimeInYear(int yearToFind)
		{
			ZDateTime result = ZDateTime.Empty;

			DstRuleParser parser = GetDstRuleParser(yearToFind);

			if (parser != null)
			{
				result = parser.TransitionDateTime;
			}

			return result;
		}

		public ZDateTime GetDaylightSavingLocalDateTimeInYear(int yearToFind, decimal utcOffsetStandard, decimal utcOffsetDst)
		{
			ZDateTime result = ZDateTime.Empty;

			DstRuleParser parser = GetDstRuleParser(yearToFind);

			if (parser != null)
			{
				result = parser.GetTransitionDateTimeInCurrentLocalTime(utcOffsetStandard, utcOffsetDst);
			}

			return result;
		}

		/// <summary>
		/// Returns a DstRuleParser for a given year based on the current property values.
		/// Note: returns a null object if validation fails
		/// </summary>
		DstRuleParser GetDstRuleParser(int yearToFind)
		{
			DstRuleParser result = null;

			if (R4_DaylightSavingDate.IsValid && !R4_DaylightSavingDate.IsEmpty)
			{
				try
				{
					result = new DstRuleParser(
						yearToFind, R4_StartOrEndRule, R4_DaylightSavingDayWeekDate, R4_TypeOfTime,
						R4_DaylightSavingDate.ToDateTime(), R4_DaylightSavingDayCount, R4_DaylightSavingDayName, R4_DaylightSavingMonth);
				}
				catch (DstRuleParsingException)
				{
					// If failed to parse the DST change date, returns null
					result = null;
				}
			}

			return result;
		}

		#endregion

		#region Convert Methods

		internal ZString GetMonthAsString(int month)
		{
			switch (month)
			{
				case 1:
					return "JAN";
				case 2:
					return "FEB";
				case 3:
					return "MAR";
				case 4:
					return "APR";
				case 5:
					return "MAY";
				case 6:
					return "JUN";
				case 7:
					return "JUL";
				case 8:
					return "AUG";
				case 9:
					return "SEP";
				case 10:
					return "OCT";
				case 11:
					return "NOV";
				case 12:
					return "DEC";
				default:
					return "";
			}
		}

		#endregion

		#region IsDateValid

		internal bool IsDateValid(int year, int month, int day, int hour, int minute, int second)
		{
			ZDateTime result = ZDateTime.Empty;
			bool isValid = ZDateTime.TryParseISO8601Date((String.Format("{0}-{1}-{2} {3}:{4}:{5}", year, month, day, hour, minute, second)), out result);
			return isValid;
		}

		#endregion
	}
}
