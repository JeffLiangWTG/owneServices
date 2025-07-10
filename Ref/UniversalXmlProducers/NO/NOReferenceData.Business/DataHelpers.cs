using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace CargoWise.RefDbRepo.NOReferenceData.Business
{
	public static class DataHelpers
	{
		public static (bool SuccessfullyParsed, DateTime DateTime) TryParseDateTime(this string dateTimeAsString, string format = Constants.YearMonthDateFormat)
			=> GetDateTime(dateTimeAsString, format, null);

		public static (bool SuccessfullyParsed, DateTime DateTime) TryParseEndDateTime(this string dateTimeAsString, string format = Constants.YearMonthDateFormat)
		{
			var result = GetDateTime(dateTimeAsString, format, Constants.MaximumDateTime);
			if (result.DateTime.Hour == 0 && result.DateTime.Minute == 0)
			{
				result.DateTime = result.DateTime.AddHours(23).AddMinutes(59);
			}
			return result;
		}

		public static string KeepNumerics(this string originalString) => new string(originalString.Where(c => char.IsDigit(c) || c == ',').ToArray());

		public static DateTime GetModifiedDateTime(string lastUpdated, StringBuilder errorBuilder)
		{
			errorBuilder = errorBuilder ?? throw new ArgumentNullException(nameof(errorBuilder));
			var (modifiedDateTimeOk, modifiedDateTime) = lastUpdated.TryParseDateTime("MM/dd/yyyy HH:mm:ss");
			if (!modifiedDateTimeOk)
			{
				errorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Failed to parse lastUpdated DateTime {0}", lastUpdated).AppendLine();
				modifiedDateTime = DateTime.Now;
			}
			return modifiedDateTime;
		}

		static (bool SuccessfullyParsed, DateTime DateTime) GetDateTime(this string dateTimeAsString, string format, DateTime? valueForEmpty)
		{
			var result = false;
			var dateTime = DateTime.MinValue;

			if (!string.IsNullOrEmpty(dateTimeAsString) && !string.IsNullOrEmpty(format))
			{
				result = DateTime.TryParseExact(dateTimeAsString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
				if (result && dateTime > Constants.MaximumDateTime)
				{
					dateTime = Constants.MaximumDateTime;
				}
				if (result && dateTime < Constants.MinimumDateTime)
				{
					dateTime = Constants.MinimumDateTime;
				}
			}
			if (string.IsNullOrEmpty(dateTimeAsString) && valueForEmpty.HasValue)
			{
				dateTime = valueForEmpty.Value;
				result = true;
			}

			return (result, dateTime);
		}

		public static (bool IsValid, DateTime StartDateParsed, DateTime EndDateParsed) IsValidDates(string startDate, string endDate)
		{
			var (startDateOk, startDateParsed) = startDate.TryParseDateTime();
			var (endDateOk, endDateParsed) = endDate.TryParseEndDateTime();

			return (startDateOk && endDateOk && startDateParsed < endDateParsed, startDateParsed, endDateParsed);
		}

		public static string SubstringSafe(this string value, int startIndex, int length)
		{
			if (value == null)
			{
				return null;
			}

			return new string(value.Skip(startIndex)
				.Take(length)
				.ToArray());
		}

		public static string CleanHtmlStringIfApplicable(string input)
		{
			return WebUtility.HtmlDecode(input);
		}
	}
}
