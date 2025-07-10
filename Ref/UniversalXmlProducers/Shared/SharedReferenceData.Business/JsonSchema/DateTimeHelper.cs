using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business
{
	static class DateTimeHelper
	{
		public static DateTime FixedEndDate(this DateTime endDate)
		{
			var fixedEndDate = endDate;
			if (fixedEndDate > Constants.DefaultValues.MaxDateTime)
			{
				fixedEndDate = Constants.DefaultValues.MaxDateTime;
			}
			fixedEndDate = fixedEndDate.AddHours(23 - fixedEndDate.Hour);
			fixedEndDate = fixedEndDate.AddMinutes(59 - fixedEndDate.Minute);
			fixedEndDate = fixedEndDate.AddSeconds(-fixedEndDate.Second);
			return fixedEndDate;
		}

		public static DateTime FixedStartDate(this DateTime startDate)
		{
			var fixedEndDate = startDate;
			if (fixedEndDate < Constants.DefaultValues.MinDateTime)
			{
				fixedEndDate = Constants.DefaultValues.MinDateTime;
			}
			fixedEndDate = fixedEndDate.AddHours(-fixedEndDate.Hour);
			fixedEndDate = fixedEndDate.AddMinutes(-fixedEndDate.Minute);
			fixedEndDate = fixedEndDate.AddSeconds(-fixedEndDate.Second);
			return fixedEndDate;
		}
	}
}
