using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GlbHolidayValidation : AutoGlbHolidayValidation
	{
		public GlbHolidayValidation(AutoGlbHoliday parent) : base(parent)
		{
		}

		new GlbHoliday Parent
		{
			get { return (GlbHoliday)base.Parent; }
		}

		protected override void CheckGH_Recurring()
		{
			base.CheckGH_Recurring();

			if (Parent.GH_Recurring && Parent.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Date && IsLeapDay(Parent.GH_Date_RawValue.Date))
			{
				Parent.GH_RecurringInfo.AddError(Res.GetString("AF8EC636-38F6-4FC7-B608-729BFF599633", "Leap days cannot be recurring holidays"));
			}
		}

		readonly Calendar calendar = new GregorianCalendar();
		bool IsLeapDay(ZDate date)
			=> date.IsValid && calendar.IsLeapDay(date.Year, date.Month, date.Day);

		#region GH_RecurrType

		protected override void CheckGH_RecurrType()
		{
			base.CheckGH_RecurrType();

			if (!Parent.GH_RecurrTypeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.GH_RecurrTypeInfo);
				if (!Parent.GH_RecurrTypeInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.GH_RecurrTypeInfo);
				}
			}
		}

		#endregion

		#region GH_Date

		protected override void CheckGH_Date()
		{
			base.CheckGH_Date();

			if (!Parent.GH_DateInfo.ReadOnly && !Parent.GH_DateInfo.HasErrors() && Parent.GH_RecurrType == GlbHolidayRecurTypeCodeList.Codes.Date)
			{
				MandatoryValidation.CheckEntered(Parent.GH_DateInfo);
				if (!Parent.GH_DateInfo.HasErrors())
				{
					if (Parent.Branch != null)
					{
						foreach (GlbHoliday holiday in Parent.Branch.GlbHolidays)
						{
							if (holiday != Parent && holiday.GH_Date == Parent.GH_Date)
							{
								Parent.GH_DateInfo.AddError(Res.GetString("46845ce2-266c-48d7-8480-0d3dfc8349c0", "This date already exists. Please enter another date."));
								break;
							}
						}
					}
				}
			}
		}

		#endregion

		#region GH_HolidayName

		protected override void CheckGH_HolidayName()
		{
			base.CheckGH_HolidayName();
			TranslatableDataFieldAttribute.Validate(Parent.GH_HolidayNameInfo);
			if (!Parent.GH_HolidayNameInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.GH_HolidayNameInfo);
				if (!Parent.GH_HolidayNameInfo.HasErrors())
				{
					if (Parent.GH_HolidayName.Length < 4)
					{
						Parent.GH_HolidayNameInfo.AddError(Res.GetString("53f70d6b-d31e-4f02-a6cf-23805be57353", "The length of Holiday Name should be at least 4 characters."));
					}
				}
			}
		}

		#endregion

		#region GH_RecurrMonth

		protected override void CheckGH_RecurrMonth()
		{
			base.CheckGH_RecurrMonth();

			if (!Parent.GH_RecurrMonthInfo.ReadOnly && !Parent.GH_RecurrMonthInfo.HasErrors() && (Parent.GH_RecurrType != GlbHolidayRecurTypeCodeList.Codes.Date && Parent.GH_RecurrType != GlbHolidayRecurTypeCodeList.Codes.Weekly))
			{
				MandatoryValidation.CheckEntered(Parent.GH_RecurrMonthInfo);
				if (!Parent.GH_RecurrMonthInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.GH_RecurrMonthInfo);
				}
			}
		}

		#endregion

		#region GH_RecurrDay

		protected override void CheckGH_RecurrDay()
		{
			base.ValidateGH_RecurrDay();
			if (!Parent.GH_RecurrDayInfo.ReadOnly && !Parent.GH_RecurrDayInfo.HasErrors() && Parent.GH_RecurrType != GlbHolidayRecurTypeCodeList.Codes.Date)
			{
				MandatoryValidation.CheckEntered(Parent.GH_RecurrDayInfo);
				if (!Parent.GH_RecurrDayInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.GH_RecurrDayInfo);
				}
			}
		}

		#endregion
	}
}
