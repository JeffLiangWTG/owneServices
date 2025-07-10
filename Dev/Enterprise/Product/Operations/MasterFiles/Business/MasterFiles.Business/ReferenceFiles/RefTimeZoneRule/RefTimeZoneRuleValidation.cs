using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class RefTimeZoneRuleValidation : AutoRefTimeZoneRuleValidation
	{
		public RefTimeZoneRuleValidation(AutoRefTimeZoneRule parent) : base(parent)
		{
		}

		new RefTimeZoneRule Parent
		{
			get { return (RefTimeZoneRule)base.Parent; }
		}

		#region DayNumber

		public void ValidateDayNumber()
		{
			ValidateCalculatedProperty(Parent.DayNumberInfo);
		}

		protected void CheckDayNumber()
		{
			if (Parent.R4_DaylightSavingDayWeekDate == TimeZoneConstants.DstRuleWeekday)
			{
				MandatoryValidation.CheckEntered(Parent.DayNumberInfo);
				ListValidation.ErrorIfInvalidCode(Parent.DayNumberInfo);
			}
		}

		#endregion

		#region DateDayNumber

		public void ValidateDateDayNumber()
		{
			ValidateCalculatedProperty(Parent.DateDayNumberInfo);
		}

		protected void CheckDateDayNumber()
		{
			if (Parent.R4_DaylightSavingDayWeekDate == TimeZoneConstants.DstRuleDayOfMonth)
			{
				MandatoryValidation.CheckEntered(Parent.DateDayNumberInfo);
				ListValidation.ErrorIfInvalidCode(Parent.DateDayNumberInfo);
				if (!Parent.R4_DaylightSavingDate.IsEmpty && Parent.R4_DaylightSavingDate.IsValid &&
					!Parent.IsDateValid(ZDateTime.Now.Year, Parent.R4_DaylightSavingDate.Month, ZInt.Parse(Parent.DateDayNumber), Parent.R4_DaylightSavingDate.Hour, Parent.R4_DaylightSavingDate.Minute, Parent.R4_DaylightSavingDate.Second))
				{
					Parent.DateDayNumberInfo.AddError(Res.GetString("58ede79d-1793-42e9-90a5-743b74b51220", "Please select a valid day for the chosen month"));
				}
				if (Parent.DateDayNumber == "29" && Parent.DateMonth == "FEB")
				{
					Parent.DateDayNumberInfo.AddWarning(Res.GetString("ec40d255-c02c-4702-acf1-c2abecd7147b", "Note: February 29th is only a valid date in a leap year. If this date is not manually changed to be a valid date in a non-leap year, daylight saving will not work correctly."));
				}
			}
		}

		#endregion

		#region DateMonth

		public void ValidateDateMonth()
		{
			ValidateCalculatedProperty(Parent.DateMonthInfo);
		}

		protected void CheckDateMonth()
		{
			if (Parent.R4_DaylightSavingDayWeekDate == TimeZoneConstants.DstRuleDayOfMonth)
			{
				MandatoryValidation.CheckEntered(Parent.DateMonthInfo);
				ListValidation.ErrorIfInvalidCode(Parent.DateMonthInfo);
				if (!Parent.R4_DaylightSavingDate.IsEmpty && Parent.R4_DaylightSavingDate.IsValid &&
					!Parent.IsDateValid(ZDateTime.Now.Year, Parent.R4_DaylightSavingDate.Month, Parent.R4_DaylightSavingDate.Day, Parent.R4_DaylightSavingDate.Hour, Parent.R4_DaylightSavingDate.Minute, Parent.R4_DaylightSavingDate.Second))
				{
					Parent.DateMonthInfo.AddError(Res.GetString("bb57f99e-9dda-40d0-85fc-339d994e783d", "Please select a valid month for the chosen day"));
				}
				if (Parent.DateDayNumber == "29" && Parent.DateMonth == "FEB")
				{
					Parent.DateMonthInfo.AddWarning(Res.GetString("ec40d255-c02c-4702-acf1-c2abecd7147b", "Note: February 29th is only a valid date in a leap year. If this date is not manually changed to be a valid date in a non-leap year, daylight saving will not work correctly."));
				}
			}
		}

		#endregion

		#region R4_DaylightSavingDayWeekDate

		protected override void CheckR4_DaylightSavingDayWeekDate()
		{
			base.CheckR4_DaylightSavingDayWeekDate();
			MandatoryValidation.CheckEntered(Parent.R4_DaylightSavingDayWeekDateInfo);
			ListValidation.ErrorIfInvalidCode(Parent.R4_DaylightSavingDayWeekDateInfo);
		}

		#endregion

		#region DaylightSavingChangeTime

		public void ValidateDaylightSavingChangeTime()
		{
			ValidateCalculatedProperty(Parent.DaylightSavingChangeTimeInfo);
		}

		protected void CheckDaylightSavingChangeTime()
		{
			MandatoryValidation.CheckEntered(Parent.DaylightSavingChangeTimeInfo);
			if (!Parent.DaylightSavingChangeTime.IsValid)
			{
				Parent.DaylightSavingChangeTimeInfo.AddError(Res.GetString("350ce9f4-9ea9-43fb-ab62-328e1ebbe6dd", "Invalid time"));
			}
		}

		#endregion

		#region R4_DaylightSavingDayName

		protected override void CheckR4_DaylightSavingDayName()
		{
			if (Parent.R4_DaylightSavingDayWeekDate == TimeZoneConstants.DstRuleWeekday)
			{
				base.CheckR4_DaylightSavingDayName();
				MandatoryValidation.CheckEntered(Parent.R4_DaylightSavingDayNameInfo);
				ListValidation.ErrorIfInvalidCode(Parent.R4_DaylightSavingDayNameInfo);
			}
		}

		#endregion

		#region R4_DaylightSavingMonth

		protected override void CheckR4_DaylightSavingMonth()
		{
			if (Parent.R4_DaylightSavingDayWeekDate == TimeZoneConstants.DstRuleWeekday)
			{
				base.CheckR4_DaylightSavingMonth();
				MandatoryValidation.CheckEntered(Parent.R4_DaylightSavingMonthInfo);
				ListValidation.ErrorIfInvalidCode(Parent.R4_DaylightSavingMonthInfo);
			}
		}

		#endregion

		#region R4_FromYear

		protected override void CheckR4_FromYear()
		{
			base.CheckR4_FromYear();

			MandatoryValidation.CheckEntered(Parent.R4_FromYearInfo);
			if (Parent.R4_FromYear < 1900)
			{
				Parent.R4_FromYearInfo.AddError(Res.GetString("7f97dd6e-7123-4064-bf55-5f322fa6a1e4", "The first year when this rule applies must be greater than 1900."));
			}
			if (!Parent.R4_ToYear.IsEmpty && Parent.R4_ToYear != 0 && Parent.R4_FromYear > Parent.R4_ToYear)
			{
				Parent.R4_FromYearInfo.AddError(Res.GetString("f9811170-c47b-4e11-979d-4d714d983f8f", "The first year when this rule applies must be before or in the same year as when the rule ends."));
			}
		}

		#endregion

		#region R4_ToYear

		protected override void CheckR4_ToYear()
		{
			base.CheckR4_ToYear();
			if (Parent.R4_ToYear < 1900 && Parent.R4_ToYear != 0)
			{
				Parent.R4_ToYearInfo.AddError(Res.GetString("96e87f8c-11e6-435a-953c-458f07a89922", "The last year when this rule applies must be greater than 1900. It can be set to 0 however, which represents infinity."));
			}
			if (!Parent.R4_FromYear.IsEmpty && Parent.R4_ToYear != 0 && Parent.R4_ToYear < Parent.R4_FromYear)
			{
				Parent.R4_ToYearInfo.AddError(Res.GetString("f9811170-c47b-4e11-979d-4d714d983f8f", "The first year when this rule applies must be before or in the same year as when the rule ends."));
			}
		}

		#endregion

		#region R4_TypeOfTime

		protected override void CheckR4_TypeOfTime()
		{
			base.CheckR4_TypeOfTime();
			MandatoryValidation.CheckEntered(Parent.R4_TypeOfTimeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.R4_TypeOfTimeInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDayNumber();
			ValidateDaylightSavingChangeTime();
			ValidateDateMonth();
			ValidateDateDayNumber();
		}
	}
}
