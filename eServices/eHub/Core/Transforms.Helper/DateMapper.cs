using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.DataAccess.Integration;
using System.Threading;

namespace CargoWise.eHub.Core.Transforms.Helper
{
	public class DateMapper
	{
		public bool IsValidDate(string dateString, string formatString)
		{
            if (string.IsNullOrEmpty(dateString)) { return false; }

			try
			{
				XmlConvert.ToDateTime(dateString, formatString);
                return true;
			}
			catch
			{
				return false;
			}
		}

		public string ConvertToXmlDate(string dateString, string formatString)
		{
			return XmlConvert.ToDateTime(dateString, formatString).ToString("yyyy-MM-dd") + "T00:00:00";
		}

		public string ConvertToDate(string origDateTime, string formatString)
		{
			if (String.IsNullOrEmpty(origDateTime)  || String.IsNullOrEmpty(formatString))
			{
                return string.Empty;
            }

			DateTime parsedResult;
			if (DateTime.TryParse(origDateTime, out parsedResult))
			{
				return parsedResult.ToString(formatString);
			}

            return string.Empty;
		}

		public string ConvertToDateTimeString(string dateVal)
		{
			return ConvertToDateTimeString(dateVal, null, null, null, null) ?? String.Empty;
		}

		public string ConvertToDateTimeString(string dateVal, string dateValFmt)
		{
			return ConvertToDateTimeString(dateVal, dateValFmt, null, null, null) ?? String.Empty;
		}

		public string ConvertToDateTimeString(string dateVal, string dateValFmt, string outFmt)
		{
			return ConvertToDateTimeString(dateVal, dateValFmt, null, null, outFmt) ?? String.Empty;
		}

		public string ConvertToDateTimeString(string dateVal, string dateValFmt, string timeVal, string timeValFmt)
		{
			return ConvertToDateTimeString(dateVal, dateValFmt, timeVal, timeValFmt, null) ?? String.Empty;
		}

		public string ConvertToDateTimeString(string dateVal, string dateValFmt, string timeVal, string timeValFmt, string outFmt)
		{
			DateTime parsedDate = DateTime.MinValue;
			DateTime parsedTime = DateTime.MinValue;

			if (String.IsNullOrWhiteSpace(outFmt))
			{
				outFmt = "O";
			}

			if (String.IsNullOrWhiteSpace(dateVal))
			{
				return String.Empty;
			}

			var parsedOk = String.IsNullOrWhiteSpace(dateValFmt) ?
				DateTime.TryParse(dateVal, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate) :
				DateTime.TryParseExact(dateVal, dateValFmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
			
			if (!parsedOk)
			{
				return String.Empty;
			}

			if (String.IsNullOrWhiteSpace(timeVal))
			{
                return parsedDate.ToString(outFmt, CultureInfo.InvariantCulture);
            }

            string[] fmts = String.IsNullOrWhiteSpace(timeValFmt) ?
                new[] { "h:mm tt", "h:mm:ss tt", "hh:mm tt", "hh:mm:ss tt", "H:mm", "H:mm:ss", "HH:mm", "HH:mm:ss", "HHmm", "HHmmss" } :
                new[] { timeValFmt.Contains("tt") ? timeValFmt : timeValFmt.Replace("hh", "HH") };

            DateTime.TryParseExact(timeVal, fmts, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces, out parsedTime);

			return parsedDate.AddTicks(parsedTime.Ticks).ToString(outFmt, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts an XML Date String into a datetime, and then output as a date string using your provided format string.
		/// </summary>
		public string ConvertXmlDateString(string sourceXmlDate, string toFormatString)
		{
			return ConvertXmlDateString(sourceXmlDate, toFormatString, string.Empty);
		}

		/// <summary>
		/// Converts an XML Date String into a datetime, and then output as a date string using your provided format string.
		/// </summary>
		public string ConvertXmlDateString(string sourceXmlDate, string toFormatString, string defaultDateString)
		{
            using (new CultureOverride())
            {
                DateTime parsed;
                if (DateTime.TryParse(sourceXmlDate.Replace('T', ' '), out parsed))
                {
                    return parsed.ToString(toFormatString);
                }

                return defaultDateString;
            }
        }

		public string ConvertLocalXmlDateTimeStringToUTC(string localXmlDateTimeString, string toFormatString)
		{
			return ConvertLocalXmlDateTimeStringToUTC(localXmlDateTimeString, toFormatString, string.Empty);
		}

		public string ConvertLocalXmlDateTimeStringToUTC(string localXmlDateTimeString, string toFormatString, string defaultDateString)
		{
			return ConvertLocalXmlDateTimeStringToUTC(localXmlDateTimeString, string.Empty, toFormatString, defaultDateString);
		}

		/// <summary>
		/// Converts a local XML datetime String into a UTC datetime, and then return a date string using your provided format string. 
		/// If source local XML datetime string does not contain a time zone offset, use defaultTimeZoneOffset(e.g. "10:00").
		/// If defaultTimeZoneOffset is null or empty, use "00:00" (i.e. treate it as a UTC time zone).
		/// If parsing source local XML DateTime string failed, return defaultDateString.
		/// </summary>
		public string ConvertLocalXmlDateTimeStringToUTC(string localXmlDateTimeString, string defaultTimeZoneOffset, string toFormatString, string defaultDateString)
		{
			if (string.IsNullOrEmpty(defaultTimeZoneOffset)) defaultTimeZoneOffset = "00:00";
			if (!localXmlDateTimeString.Contains("+")) localXmlDateTimeString += "+" + defaultTimeZoneOffset;

			try
			{
                DateTime parsedDateTime = XmlConvert.ToDateTime(localXmlDateTimeString, XmlDateTimeSerializationMode.Local);
				return parsedDateTime.ToUniversalTime().ToString(toFormatString);
			}
			catch
			{
				return defaultDateString;
			}
		}

		public string ConvertToUTCTime(string sourceXmlDate)
		{
            DateTime parsedDateTime;
            if (DateTime.TryParse(sourceXmlDate.Replace('T', ' '), out parsedDateTime))
            {
                return parsedDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss");
			}

            return string.Empty;
		}

		/// <summary>
		/// Converts UTC time to the time zone of the specified UNLOCO.
		/// Only performs conversion if input date (utcTime) is in UTC format.
		/// </summary>
		public virtual string ConvertUTCToLocalTimeByUNLOCO(string utcTime, string unloco)
		{
			if (string.IsNullOrEmpty(utcTime))
			{
				return string.Empty;
			}

			DateTimeOffset offset;

			if (!DateTimeOffset.TryParse(utcTime, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out offset))
			{
				return string.Empty;
			}

			var dateTime = offset.DateTime;

			var utcFormatSuffixes = new[] { "Z", "z", "+0000", "+00:00" };

			if (!string.IsNullOrEmpty(unloco) && utcFormatSuffixes.Any(suffix => utcTime.EndsWith(suffix)))
			{
				var utcOffset = GetTransformAccessor().CallActionProcedure("CalculateTimeZoneOffset", "@offset", "@UNLOCO", unloco, "@localtime", dateTime.ToString("yyyy-MM-ddTHH:mm:ss"));

				if (!string.IsNullOrEmpty(utcOffset))
				{
					var timeSpan = TimeSpan.Parse(utcOffset.Replace("+", ""));
					dateTime = dateTime.Add(timeSpan);
				}
			}

			return dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
		}

		internal virtual ITransformAccessor GetTransformAccessor()
		{
			return transformAccessor ?? (transformAccessor = DataAccessFactories.NewTransformAccessorInstance());
		}

		ITransformAccessor transformAccessor;

		public string CurrentDateWithTimeZone()
		{
			return CurrentDateTime("yyyy-MM-ddT00:00:00zzz");
		}

		public string CurrentDateTimeWithTimeZone()
		{
			return CurrentDateTime("yyyy-MM-ddTHH:mm:sszzz");
		}

		public virtual string CurrentDateTime(string toFormat)
		{
			return XmlConvert.ToString(DateTime.Now, toFormat);
		}

		public virtual string CurrentDateTimeUTC(string toFormat)
		{
			return XmlConvert.ToString(DateTime.UtcNow, toFormat);
		}

		public string FormatXmlDateTime(string xmlDateTime, string outputFormat)
		{
            using (new CultureOverride())
            {
                if (String.IsNullOrWhiteSpace(xmlDateTime))
                {
                    return "";
                }

                if (String.IsNullOrWhiteSpace(outputFormat))
                {
                    outputFormat = "O";
                }

                DateTime dateTime = Convert.ToDateTime(xmlDateTime);
                return dateTime.ToString(outputFormat);
            }
		}

		public string ConvertToXmlDatePartOnly(string dateString, string formatString)
		{
			return XmlConvert.ToDateTime(dateString, formatString).ToString("yyyy-MM-dd");
		}
	}
}
