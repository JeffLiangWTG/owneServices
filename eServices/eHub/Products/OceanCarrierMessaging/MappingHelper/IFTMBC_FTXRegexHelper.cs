using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class IFTMBC_FTXRegexHelper
  {
    const string verifiedGrossMassPrefix = @"(VGM|Verified Gross Mass|SI and VGM)(\W+)";
    const string shippingInstructionPrefix = @"(SI|Shipping Instruction(s)?|SI and VGM)(\W+)";
    const string suffix = @"(Cut Off|cutoff|cut-off|cut|Deadlines|Deadline)\s*";
    const string additionalSuffix = @"((-\s*\w*)|\)|\(.*?\)|[^\d]*)?\s*";
    const string endSymbol = @"\W*\s*";

    public string GetVGMCutOffDateTime(string text, string etdString) => TryGetCutOffDateTime(verifiedGrossMassPrefix, text, etdString);

    public string GetSICutOffDateTime(string text, string etdString) => TryGetCutOffDateTime(shippingInstructionPrefix, text, etdString);

    string TryGetCutOffDateTime(string prefix, string text, string etdString)
    {
      if (string.IsNullOrEmpty(text))
      {
        return string.Empty;
      }

      if (!DateTime.TryParseExact(etdString, new[] { "yyyyMMdd", "yyyyMMddHHmm", "yyMMddHHmm" }, null, DateTimeStyles.None, out DateTime etd))
      {
        return string.Empty;
      }

      var formattedText = FormattedText(text);
      var pattern = prefix + suffix + additionalSuffix + endSymbol;
      var matchCollection = Regex.Matches(formattedText, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

      if (matchCollection.Count == 0)
      {
        return string.Empty;
      }

      foreach (Match match in matchCollection)
      {
        var result = GetDateTimeString(formattedText.Substring(match.Index + match.Length), etd);

        if (!string.IsNullOrEmpty(result))
        {
          return result;
        }
      }

      return string.Empty;
    }

    string GetDateTimeString(string text, DateTime etd)
    {
      if (string.IsNullOrEmpty(text))
      {
        return string.Empty;
      }

      var timeRegexPattern = @"(\d{4}|\d{1,2}:\d{1,2}(:\d{1,2})?(\s*(AM|PM)\b)?)?";
      var intervalSymbol = @"\s*\W*\s*";

      var dateTimeRegexPattern = new List<string>
      {
        @"^(\s*\d{4}[-/]\d{1,2}[-/]\d{1,2}(\s+|\b))" + intervalSymbol + timeRegexPattern,
        @"^(\s*\d{1,2}[-/]\d{1,2}[-/]\d{4}(\s+|\b))" + intervalSymbol + timeRegexPattern
      };

      foreach (var pattern in dateTimeRegexPattern)
      {
        var matchedValue = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        if (matchedValue.Success)
        {
          var date = matchedValue.Groups[1].Value;
          var time = matchedValue.Groups[3].Success ? " " + matchedValue.Groups[3] : string.Empty;

          var dateTimeString = TryParseDateTime(date + time, etd);
          if (!string.IsNullOrEmpty(dateTimeString))
          {
            return dateTimeString;
          }
        }
      }

      return string.Empty;
    }

    string FormattedText(string text) => text.Replace("?:", ":");

    string TryParseDateTime(string dateString, DateTime etd)
    {
      dateString = Regex.Replace(dateString, @"\s+", " ").Trim();

      var dateFormats = new List<string>
      {
        "yyyy-MM-dd",
        "MM-dd-yyyy",
        "dd-MM-yyyy",
        "yyyy/MM/dd",
        "MM/dd/yyyy",
        "dd/MM/yyyy",
      };

      var timeFormats = new List<string>
      {
        "HHmm",
        "h:m:s tt",
        "H:m:s",
        "h:m tt",
        "H:m"
      };

      DateTime cutOff;

      foreach (var dateFormat in dateFormats)
      {
        foreach (var timeFormat in timeFormats)
        {
          if (DateTime.TryParseExact(dateString, dateFormat + " " + timeFormat, null, DateTimeStyles.None, out cutOff) && IsValidDatetime(cutOff, etd, 14))
          {
            return ConvertDateToString(cutOff);
          }
        }
      }

      foreach (var dateFormat in dateFormats)
      {
        if (DateTime.TryParseExact(dateString, dateFormat, null, DateTimeStyles.None, out cutOff) && IsValidDatetime(cutOff, etd, 14))
        {
          return ConvertDateToString(cutOff);
        }
      }

      return string.Empty;
    }

    string ConvertDateToString(DateTime dateValue) => dateValue.ToString("yyyyMMddHHmm");

    bool IsValidDatetime(DateTime cutOff, DateTime etd, int offset) => cutOff >= etd.AddDays(-offset) && cutOff <= etd;
  }
}