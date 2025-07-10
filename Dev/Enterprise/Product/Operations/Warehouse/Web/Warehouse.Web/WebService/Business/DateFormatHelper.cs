using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public static class DateFormatHelper
	{
		public static string GetDateFormatForCountry(string countryCode)
		{
			return CountryShortDateFormats.GetOrAdd(countryCode, (key) =>
			{
				var cultureInfo = CultureInfoHelper.GetCulture(key);
				var dateTimeFormat = cultureInfo.DateTimeFormat;

				var split = Regex.Split(dateTimeFormat.ShortDatePattern, dateTimeFormat.DateSeparator ?? string.Empty);

				var dateFormat = string.Empty;
				var dateAdded = false;
				var monthAdded = false;
				var yearAdded = false;

				foreach (var item in split)
				{
					if (Regex.IsMatch(item, "d|D") && !dateAdded)
					{
						dateFormat += (NoResString)"dd"; // Date format for day
						dateAdded = true;
					}
					else if (Regex.IsMatch(item, "m|M") && !monthAdded)
					{
						dateFormat += "MM"; // Date format for month
						monthAdded = true;
					}
					else if (Regex.IsMatch(item, "y|Y") && !yearAdded)
					{
						dateFormat += (NoResString)"yy"; // Date format for year
						yearAdded = true;
					}
				}

				if (!dateAdded || !monthAdded || !yearAdded)
				{
					dateFormat = fallbackDateFormat;
				}

				return dateFormat;
			});
		}

		const string fallbackDateFormat = "MMddyy"; // Date format for EN-US

		static readonly ConcurrentDictionary<string, string> CountryShortDateFormats = new ConcurrentDictionary<string, string>();
	}
}
