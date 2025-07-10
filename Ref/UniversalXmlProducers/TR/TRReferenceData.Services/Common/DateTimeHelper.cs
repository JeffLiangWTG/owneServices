using System;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Common
{
	public static class DateTimeHelper
	{
		public static DateTime? ParseDate(string dateAsString)
		{
			DateTime? date = null;
			if (!string.IsNullOrEmpty(dateAsString))
			{
				if (DateTime.TryParse(dateAsString, out DateTime dateAsDate))
				{
					date = dateAsDate;
				}
				else
				{
					throw new FormatException($"Parse DateTime Error: {dateAsString}");
				}
			}
			return date;
		}
	}
}
