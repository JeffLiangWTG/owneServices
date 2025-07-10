using System;
using System.Collections.Generic;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	internal class OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsHolidayValidationTest : OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsHolidayTest
	{
		protected override void SetHolidays(string countryCode = "XX", string stateCode = "YY", params DateTime[] days)
		{
			CalendarDayTypeProvider = new CalendarDayTypeProviderTest(days);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CalendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
		}
	}
}
