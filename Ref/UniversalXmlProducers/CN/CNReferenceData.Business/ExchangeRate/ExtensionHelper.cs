using System;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public static class ExtensionHelper
	{
		public static DateTime ToChinaStandardTime(this DateTime utcDate)
		{
			var localTimeZoneId = "China Standard Time";
			var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId);

			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone);
		}

		public static DateTime GetThirdWednesdayByMonthYear(this DateTime dateUtcNow)
		{
			var year = dateUtcNow.Year;
			var month = dateUtcNow.Month;
			for (var day = 1; day <= DateTime.DaysInMonth(year, month); day++)
			{
				var dateTimeOfTheDay = new DateTime(year, month, day);
				if (dateTimeOfTheDay.DayOfWeek == DayOfWeek.Wednesday)
				{
					return dateTimeOfTheDay.AddDays(7 * 2);
				}
			}

			throw new InvalidOperationException("The program should not reach this part");
		}

		public static string FilterHtml(this string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				return string.Empty;
			}
			return Regex.Replace(html, @"\t|\n|\r|&nbsp;", "");
		}

		public static int Scale(this decimal d)
		{
			var text = d.ToString(System.Globalization.CultureInfo.InvariantCulture);
			var decpoint = text.IndexOf('.');
			if (decpoint < 0)
			{
				return 0;
			}
			return text.Length - decpoint - 1;
		}
	}
}
