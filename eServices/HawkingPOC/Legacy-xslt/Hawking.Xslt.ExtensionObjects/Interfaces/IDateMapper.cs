namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface IDateMapper
    {
        bool IsValidDate(string dateString, string formatString);
        string ConvertToXmlDate(string dateString, string formatString);
        string ConvertToDate(string origDateTime, string formatString);
        string ConvertToDateTimeString(string dateVal);
        string ConvertToDateTimeString(string dateVal, string dateValFmt);
        string ConvertToDateTimeString(string dateVal, string dateValFmt, string outFmt);
        string ConvertToDateTimeString(string dateVal, string dateValFmt, string timeVal, string timeValFmt);
        string ConvertToDateTimeString(string dateVal, string dateValFmt, string timeVal, string timeValFmt, string outFmt);
        string ConvertXmlDateString(string sourceXmlDate, string toFormatString);
        string ConvertXmlDateString(string sourceXmlDate, string toFormatString, string defaultDateString);
        string ConvertLocalXmlDateTimeStringToUTC(string localXmlDateTimeString, string toFormatString);
        string ConvertLocalXmlDateTimeStringToUTC(string localXmlDateTimeString, string toFormatString, string defaultDateString);
        string ConvertLocalXmlDateTimeStringToUTC(string localXmlDateTimeString, string defaultTimeZoneOffset, string toFormatString, string defaultDateString);
        string ConvertToUTCTime(string sourceXmlDate);
        string ConvertUTCToLocalTimeByUNLOCO(string utcTime, string unloco);
        string CurrentDateWithTimeZone();
        string CurrentDateTimeWithTimeZone();
        string CurrentDateTime(string toFormat);
        string CurrentDateTimeUTC(string toFormat);
        string FormatXmlDateTime(string xmlDateTime, string outputFormat);
        string ConvertToXmlDatePartOnly(string dateString, string formatString);
    }
}
