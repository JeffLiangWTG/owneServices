using System;
using System.Globalization;
using Enterprise.Edifact;

namespace Enterprise.Freight.Agency.Business
{
	using Enterprise.Edifact.D95B.Elements;
	using Enterprise.Edifact.D95B.Segments;

	partial class EdifactDateParser
	{
		public static DateTime GetDate(DTMSegment segment)
		{
			if (segment.DateTimePeriod.DateTimePeriodFormatQualifier == DateTimePeriodFormatQualifierList.Ccyymmddhhmmss)
			{
				return ParseDate(segment.DateTimePeriod.DateTimePeriod, "yyyyMMddHHmmss", "CCYYMMDDHHMMSS");
			}
			else if (segment.DateTimePeriod.DateTimePeriodFormatQualifier == DateTimePeriodFormatQualifierList.Ccyymmddhhmm)
			{
				return ParseDate(segment.DateTimePeriod.DateTimePeriod, "yyyyMMddHHmm", "CCYYMMDDHHMM");
			}
			else
			{
				throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Unrecognised date format ({0})", segment.DateTimePeriod.DateTimePeriodFormatQualifier.ToString()));
			}
		}
	}
}

namespace Enterprise.Freight.Agency.Business
{
	using System.Globalization;
	using Enterprise.Edifact.D99A.Elements;
	using Enterprise.Edifact.D99A.Segments;

	partial class EdifactDateParser
	{
		public static DateTime GetDate(DTMSegment segment)
		{
			if (segment.DateTimePeriod.DateTimePeriodFormatQualifier == DateTimePeriodFormatQualifierList.Ccyymmddhhmmss)
			{
				return ParseDate(segment.DateTimePeriod.DateTimePeriod, "yyyyMMddHHmmss", "CCYYMMDDHHMMSS");
			}
			else if (segment.DateTimePeriod.DateTimePeriodFormatQualifier == DateTimePeriodFormatQualifierList.Ccyymmddhhmm)
			{
				return ParseDate(segment.DateTimePeriod.DateTimePeriod, "yyyyMMddHHmm", "CCYYMMDDHHMM");
			}
			else
			{
				throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Unrecognised date format ({0})", segment.DateTimePeriod.DateTimePeriodFormatQualifier.ToString()));
			}
		}
	}
}

namespace Enterprise.Freight.Agency.Business
{
	public static partial class EdifactDateParser
	{
		static DateTime ParseDate(string date, string parsePattern, string displayPattern)
		{
			try
			{
				string sanitisedDate;

				if (date.Length < parsePattern.Length)
				{
					sanitisedDate = date.PadRight(parsePattern.Length, '0');
				}
				else if (date.Length > parsePattern.Length)
				{
					sanitisedDate = date.Substring(0, parsePattern.Length);
				}
				else
				{
					sanitisedDate = date;
				}

				return DateTime.ParseExact(sanitisedDate, parsePattern, null);
			}
			catch (FormatException)
			{
				throw new InvalidFormatException(
					Res.GetString(
						"58d3a7e1-5338-4322-bc9a-7d4d61f97a00",
						"Unable to parse the date in a DTM segment.\r\n'{0}' does not fit the specified pattern ({1}).",
						date, displayPattern));
			}
		}
	}
}


