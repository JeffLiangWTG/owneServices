using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class WeekendsAndHolidaysValidator
	{
		public void CheckWeekendsAndHolidays(ZPropertyInfo dateInfo)
		{
			if (dateInfo.Value.IsValid)
			{
				DayOfWeek dayOfWeek = ((ZDateTime)dateInfo.Value).DayOfWeek;
				if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
				{
					dateInfo.AddMessageError(DateCannotBeSetOnPublicHolidayOrWeekend);
				}
				else
				{
					CheckFederalCustomsHolidays(dateInfo);
				}
			}
		}
		internal const string DateCannotBeSetOnPublicHolidayOrWeekend = "This date cannot be scheduled to fall on a weekend or a Federal Public Holiday. Validations for weekends (Saturday, Sunday) and Federal Public Holidays, (as identified in the Federal Register), are shipped with the software.";

		public void CheckFederalCustomsHolidays(ZPropertyInfo dateInfo)
		{
			if (dateInfo.Value.IsValid)
			{
				CustomsWorkingDays workingDays = CustomsWorkingDays.GetInstance(new BusinessObjectFactory { NameForDebugging = "WorkingDays" });
				ZDateTime dateToCheck = (ZDateTime)dateInfo.Value;
				if (workingDays.IsDateACustomsFederalHoliday(dateToCheck))
				{
					dateInfo.AddMessageError(DateCannotBeSetOnPublicHoliday);
				}
			}
		}
		internal const string DateCannotBeSetOnPublicHoliday = "This date cannot be scheduled to fall on a Federal Public Holiday. Validations for Federal Public Holidays, (as identified in the Federal Register), are shipped with the software.";
	}
}
