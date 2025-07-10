using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.CalendarArithmetic;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[PreventDelete(false)]
	public class GlbHoliday : AutoGlbHoliday
	{
		public GlbHoliday(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ResetRecurrProperties();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			ResetRecurrProperties();
		}

		#endregion

		#region Property Overrides

		#region GH_Date

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public ZDateTime GH_Date_RawValue => base.GH_Date;

		public override ZDateTime GH_Date
		{
			get => GetHolidayDateForYear(ZDateTime.Now.Year);
			set
			{
				base.GH_Date = value.Date;

				if (!IsValidationSuspended)
				{
					Validation.ValidateGH_Recurring();
				}
			}
		}

		internal ZDateTime GetHolidayDateForYear(int year)
		{
			if (IsWeekly)
			{
				return base.GH_Date;
			}

			if (Months.ContainsCode(GH_RecurrMonth) && WeekDays.ContainsCode(GH_RecurrDay))
			{
				var holidayDate = base.GH_Date.IsValid ? (DateTime?)base.GH_Date.ToDateTime() : null;
				var holidayParams = new HolidayParams(year, holidayDate, MonthOfYear, DayOfWeek, RecuringHolidayType);
				return HolidayCalculator.GetHolidayDate(holidayParams);
			}
			else if (GH_RecurrType == HolidayRecurTypeCodes.EasterMonday || GH_RecurrType == HolidayRecurTypeCodes.GoodFriday)
			{
				var holidayParams = new HolidayParams(year, null, MonthOfYear, DayOfWeek, RecuringHolidayType);
				return HolidayCalculator.GetHolidayDate(holidayParams);
			}

			return base.GH_Date;
		}

		#endregion

		#region GH_RecurrType

		[List("RecurrTypes")]
		public override ZString GH_RecurrType
		{
			get => base.GH_RecurrType;
			set
			{
				base.GH_RecurrType = value;
				if (value == GlbHolidayRecurTypeCodeList.Codes.GoodFriday ||
					value == GlbHolidayRecurTypeCodeList.Codes.EasterMonday)
				{
					GH_HolidayName = new GlbHolidayRecurTypeCodeList().GetDescriptionFromCode(value);
				}
				ResetRecurrProperties();
				MarkAsNeedingValidation();
			}
		}

		HolidayType RecuringHolidayType => HolidayCodes.GetTypeFromCode(GH_RecurrType, GH_Recurring);

		#endregion

		#region GH_RecurrMonth

		[List("Months")]
		public override ZString GH_RecurrMonth
		{
			get => base.GH_RecurrMonth;
			set => base.GH_RecurrMonth = value;
		}

		MonthOfYear? MonthOfYear => HolidayCodes.GetMonthFromCode(GH_RecurrMonth);

		#endregion

		#region GH_RecurrDay

		[List("WeekDays")]
		public override ZString GH_RecurrDay
		{
			get => base.GH_RecurrDay;
			set => base.GH_RecurrDay = value;
		}

		DayOfWeek? DayOfWeek => HolidayCodes.GetDayFromCode(GH_RecurrDay);

		#endregion

		#region GH_HolidayName

		[TranslatableDataField(Schema.TableName, Schema.GH_HolidayName, MaxLength = Schema.GH_HolidayNameMaxLength, Type = typeof(GlbHoliday), Asmid = ResString.AssemblyId)]
		public override ZString GH_HolidayName
		{
			get => base.GH_HolidayName;
			set => base.GH_HolidayName = value;
		}

		[ResourceStringData("GlbHoliday|GH_HolidayNameMultilingual", Caption = "Holiday Name")]
		public MultilingualString GH_HolidayNameMultilingual => GetMultilingual(GH_HolidayNameInfo);

		public GlbBranch Branch
		{
			get { return GH_ParentTableCode == GlbBranchSchema.Constants.Prefix ? (GlbBranch)Factory.Load(typeof(GlbBranch), GH_ParentID) : null; }
		}

		#endregion

		#endregion

		#region Code Lists

		public GlbHolidayRecurTypeCodeList RecurrTypes
		{
			get
			{
				if (GH_ParentTableCode == GlbBranchSchema.Constants.Prefix)
				{
					if (fGlbHolidayRecurTypeCodeListWithoutWeekly == null)
					{
						fGlbHolidayRecurTypeCodeListWithoutWeekly = new GlbHolidayRecurTypeCodeList();
						fGlbHolidayRecurTypeCodeListWithoutWeekly.RemoveCode(GlbHolidayRecurTypeCodeList.Codes.Weekly);
					}
					return fGlbHolidayRecurTypeCodeListWithoutWeekly;
				}
				else
				{
					return Factory.GetCachedValue<GlbHolidayRecurTypeCodeList>();
				}
			}
		}
		GlbHolidayRecurTypeCodeList fGlbHolidayRecurTypeCodeListWithoutWeekly;

		public CodeDescriptionPairList Months => Factory.GetCachedValue("GlbHoliday.Months", () => new CodeDescriptionPairList(OLookUpEditType.Months));

		public DayOfWeekCodeList WeekDays => Factory.GetCachedValue<DayOfWeekCodeList>();

		#endregion

		#region MatchesDate

		public bool MatchesDate(ZDate date)
		{
			var holidayDate = GH_Date.IsValid ? (DateTime?)GH_Date.ToDateTime() : null;
			var calendarHoliday = new HolidayParams(date.Year, holidayDate, MonthOfYear, DayOfWeek, RecuringHolidayType);

			return HolidayCalculator.IsMatchingHoliday(calendarHoliday, date.ToDateTime());
		}

		#endregion

		#region ReadOnly Fields

		protected bool GH_HolidayName_ReadOnly => IsEaster;
		protected bool GH_Recurring_ReadOnly => !IsDate;
		protected bool GH_Date_ReadOnly => !IsDate;
		protected bool GH_RecurrMonth_ReadOnly => IsEaster || IsDate || IsWeekly;
		protected bool GH_RecurrDay_ReadOnly => IsEaster || IsDate;
		bool IsDate => (GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Date);
		bool IsEaster => GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.GoodFriday ||
			GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.EasterMonday;
		bool IsWeekly => GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Weekly;

		#endregion

		#region HumanReadableNameCore

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (GH_ParentTableCode == RefCountrySchema.Constants.Prefix)
				{
					return $"{GH_HolidayName} - {Country.RN_Code}";
				}
				else if (GH_ParentTableCode == RefCountryStatesSchema.Constants.Prefix)
				{
					return $"{GH_HolidayName} - {State.RW_Code}";
				}
				return base.HumanReadableNameCore;
			}
		}

		#endregion

		#region Implementation

		IHolidayCalculator HolidayCalculator => Factory.GetCachedValue<HolidayCalculator>();

		void ResetRecurrProperties()
		{
			if (IsEaster || IsDate)
			{
				GH_RecurrMonth = "";
				GH_RecurrDay = "";
			}

			if (IsEaster || !IsDate)
			{
				GH_Date = ZDateTime.Empty;
				GH_Recurring = true;
			}

			if (IsWeekly)
			{
				GH_RecurrMonth = "";
			}

			GH_HolidayNameInfo.RefreshBinding();
			GH_RecurringInfo.RefreshBinding();
			GH_DateInfo.RefreshBinding();
			GH_RecurrMonthInfo.RefreshBinding();
			GH_RecurrDayInfo.RefreshBinding();
		}

		public RefCountryStates State
		{
			get
			{
				if (fState == null && GH_ParentTableCode == RefCountryStatesSchema.Constants.Prefix)
				{
					return fState = Factory.Load<RefCountryStates>(GH_ParentID);
				}
				return fState;
			}
		}
		RefCountryStates fState;

		public RefCountry Country
		{
			get
			{
				if (fCountry == null && GH_ParentTableCode == RefCountrySchema.Constants.Prefix)
				{
					return fCountry = Factory.Load<RefCountry>(GH_ParentID);
				}
				else if (fCountry == null && GH_ParentTableCode == RefCountryStatesSchema.Constants.Prefix)
				{
					return fCountry = State?.Country;
				}
				return fCountry;
			}
		}
		RefCountry fCountry;

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		public ZString CountryStatesApplicability
		{
			get
			{
				if (GH_ParentTableCode == RefCountryStatesSchema.Constants.Prefix)
				{
					var country = State.Country;
					var query = new ZQuery(GlbHolidaySchema.GH_HolidayName, GH_HolidayName);
					query.AddToFilter(GlbHolidaySchema.GH_ParentTableCode, RefCountryStatesSchema.Constants.Prefix);
					query.AddToFilter(GlbHolidaySchema.GH_ParentID, country.States.GetPKs());
					query.AddToFilter(GlbHolidaySchema.GH_IsActive, GH_IsActive);
					query.AddToFilter(GlbHolidaySchema.GH_RecurrType, GH_RecurrType);
					query.AddToFilter(GlbHolidaySchema.GH_Recurring, GH_Recurring);
					query.AddToFilter(GlbHolidaySchema.GH_RecurrDay, GH_RecurrDay);
					query.AddToFilter(GlbHolidaySchema.GH_IsWorkingDay, GH_IsWorkingDay);
					query.AddToFilter(GlbHolidaySchema.GH_Date, GH_Date);
					var statesHolidays = Factory.Load<GlbHoliday>(query);

					if (statesHolidays.Length == country.States.Count)
					{
						return $"{country.RN_Code}: whole country";
					}
					return $"{country.RN_Code}: {ZString.Join(", ", statesHolidays.Select(x => x.State.RW_Code).OrderBy(x => x).ToArray())}";
				}
				else
				{
					return $"{Country.RN_Code}: whole country";
				}
			}
		}

		public ZPropertyInfo CountryStatesApplicabilityInfo
		{
			get { return GetZPropertyInfo(nameof(CountryStatesApplicability)); }
		}

		#endregion
	}
}
