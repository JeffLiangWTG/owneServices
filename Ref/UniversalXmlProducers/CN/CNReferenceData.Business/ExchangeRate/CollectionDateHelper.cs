using System;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public static class CollectionDateHelper
	{
		public static DateTime DefineCollectionDate(DateTime date)
		{
			var result = date.GetThirdWednesdayByMonthYear();
			if (ChinaHolidayHelper.AcceptableYear < result.Year)
			{
				throw new InvalidOperationException("The date is not acceptable, the holiday list should be updated");
			}
			if (ChinaHolidayHelper.IsHoliday(result))
			{
				result = result.AddDays(7);
				if (date.Day < result.Day)
				{
					result = new DateTime(date.Year, date.Month, 1).GetThirdWednesdayByMonthYear();
					if (ChinaHolidayHelper.IsHoliday(result))
					{
						result = result.AddDays(7);
					}
				}
			}
			return result;
		}
	}
}
