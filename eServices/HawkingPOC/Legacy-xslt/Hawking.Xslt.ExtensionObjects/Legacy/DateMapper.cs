using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Hawking.Xslt.ExtensionObjects.Interfaces;

namespace Hawking.Xslt.ExtensionObjects.Legacy
{
    public class DateMapper : IDateMapper
    {
        ITransformAccessor transformAccessor;

        public DateMapper(ITransformAccessor transformAccessor)
        {
            this.transformAccessor = transformAccessor;
        }

        public bool IsValidDate(string dateString, string formatString)
        {
            bool isValid = !string.IsNullOrEmpty(dateString);

            if (isValid)
            {
                try
                {
                    XmlConvert.ToDateTime(dateString, formatString);
                }
                catch
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        public string ConvertToXmlDate(string dateString, string formatString)
        {
            return XmlConvert.ToDateTime(dateString, formatString).ToString("yyyy-MM-dd") + "T00:00:00";
        }

        public string ConvertToDate(string origDateTime, string formatString)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(origDateTime) && !string.IsNullOrEmpty(formatString))
            {
                if (DateTime.TryParse(origDateTime, out DateTime parsedResult))
                {
                    result = parsedResult.ToString(formatString);
                }
            }

            return result;
        }

        public string ConvertToDateTimeString(string dateVal)
        {
            return ConvertToDateTimeString(dateVal, null, null, null, null) ?? string.Empty;
        }

        public string ConvertToDateTimeString(string dateVal, string dateValFmt)
        {
            return ConvertToDateTimeString(dateVal, dateValFmt, null, null, null) ?? string.Empty;
        }

        public string ConvertToDateTimeString(string dateVal, string dateValFmt, string outFmt)
        {
            return ConvertToDateTimeString(dateVal, dateValFmt, null, null, outFmt) ?? string.Empty;
        }

        public string ConvertToDateTimeString(string dateVal, string dateValFmt, string timeVal, string timeValFmt)
        {
            return ConvertToDateTimeString(dateVal, dateValFmt, timeVal, timeValFmt, null) ?? string.Empty;
        }

        public string ConvertToDateTimeString(string dateVal, string dateValFmt, string timeVal, string timeValFmt, string outFmt)
        {
            DateTime parsedDate = DateTime.MinValue;
            DateTime parsedTime = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(outFmt))
            {
                outFmt = "O";
            }

            if (string.IsNullOrWhiteSpace(dateVal))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(dateValFmt))
            {
                DateTime.TryParse(dateVal, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
            }
            else
            {
                DateTime.TryParseExact(dateVal ?? "", dateValFmt ?? "", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
            }

            if (parsedDate == DateTime.MinValue)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(timeVal))
            {
                if (string.IsNullOrWhiteSpace(timeValFmt))
                {
                    string[] fmts = { "h:mm tt", "h:mm:ss tt", "hh:mm tt", "hh:mm:ss tt", "H:mm", "H:mm:ss", "HH:mm", "HH:mm:ss", "HHmm", "HHmmss" };
                    DateTime.TryParseExact(timeVal, fmts, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces, out parsedTime);
                }
                else
                {
                    if (timeValFmt.Contains("hh") && !timeValFmt.Contains("tt"))
                    {
                        timeValFmt = timeValFmt.Replace("hh", "HH");
                    }
                    DateTime.TryParseExact(timeVal, timeValFmt, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces, out parsedTime);
                }
            }

            return parsedDate.AddTicks(parsedTime.Ticks).ToString(outFmt, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts an XML Date string into a datetime, and then output as a date string using your provided format string.
        /// </summary>
        public string ConvertXmlDateString(string sourceXmlDate, string toFormatString)
        {
            return ConvertXmlDateString(sourceXmlDate, toFormatString, string.Empty);
        }

        /// <summary>
        /// Converts an XML Date string into a datetime, and then output as a date string using your provided format string.
        /// </summary>
        public string ConvertXmlDateString(string sourceXmlDate, string toFormatString, string defaultDateString)
        {
            string result;

            try
            {
                result = DateTime.Parse(sourceXmlDate.Replace('T', ' ')).ToString(toFormatString);
            }
            catch
            {
                result = defaultDateString;
            }

            return result;
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
        /// Converts a local XML datetime string into a UTC datetime, and then return a date string using your provided format string. 
        /// If source local XML datetime string does not contain a time zone offset, use defaultTimeZoneOffset(e.g. "10:00").
        /// If defaultTimeZoneOffset is null or empty, use "00:00" (i.e. treate it as a UTC time zone).
        /// If parsing source local XML DateTime string failed, return defaultDateString.
        /// </summary>
        public string ConvertLocalXmlDateTimeStringToUTC(string localXmlDateTimeString, string defaultTimeZoneOffset, string toFormatString, string defaultDateString)
        {
            string result;
            DateTime parsedDateTime;

            if (string.IsNullOrEmpty(defaultTimeZoneOffset)) defaultTimeZoneOffset = "00:00";
            if (!localXmlDateTimeString.Contains("+")) localXmlDateTimeString += "+" + defaultTimeZoneOffset;

            try
            {
                parsedDateTime = System.Xml.XmlConvert.ToDateTime(localXmlDateTimeString, System.Xml.XmlDateTimeSerializationMode.Local);
                result = parsedDateTime.ToUniversalTime().ToString(toFormatString);
            }
            catch
            {
                result = defaultDateString;
            }

            return result;
        }

        public string ConvertToUTCTime(string sourceXmlDate)
        {
            string result;

            try
            {
                result = DateTime.Parse(sourceXmlDate.Replace('T', ' ')).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss");
            }
            catch
            {
                result = string.Empty;
            }

            return result;
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

            if (!DateTimeOffset.TryParse(utcTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset offset))
            {
                return string.Empty;
            }

            var dateTime = offset.DateTime;

            var utcFormatSuffixes = new[] { "Z", "z", "+0000", "+00:00" };

            if (!string.IsNullOrEmpty(unloco) && utcFormatSuffixes.Any(suffix => utcTime.EndsWith(suffix)))
            {
                var utcOffset = transformAccessor.CallActionProcedure("CalculateTimeZoneOffset", "@offset", "@UNLOCO", unloco, "@localtime", dateTime.ToString("yyyy-MM-ddTHH:mm:ss"));

                if (!string.IsNullOrEmpty(utcOffset))
                {
                    var timeSpan = TimeSpan.Parse(utcOffset.Replace("+", ""));
                    dateTime = dateTime.Add(timeSpan);
                }
            }

            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
        }

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
            if (string.IsNullOrWhiteSpace(xmlDateTime))
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(outputFormat))
            {
                outputFormat = "O";
            }

            DateTime dateTime = Convert.ToDateTime(xmlDateTime);
            return dateTime.ToString(outputFormat);
        }

        public string ConvertToXmlDatePartOnly(string dateString, string formatString)
        {
            return XmlConvert.ToDateTime(dateString, formatString).ToString("yyyy-MM-dd");
        }
    }
}
