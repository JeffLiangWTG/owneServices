using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.INReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.INReferenceData.Services.Tariff
{
	public static class TariffExtentions
	{
		public static DateTime? GetTariffDate(this Childcontentlist childcontent)
		{
			var datePattern = @"\b\d{2}\.\d{2}\.\d{4}\b";
			var match = Regex.Match(childcontent.titleEn, datePattern);

			if (match.Success && DateTime.TryParseExact(match.Value, CbicDateFormat, null, System.Globalization.DateTimeStyles.None, out var date))
			{
				return date;
			}

			return null;
		}

		public static string GetDateInCbicString(this DateTime date)
		{
			return date.ToString(CbicDateFormat, CultureInfo.InvariantCulture);
		}

		const string CbicDateFormat = "dd.MM.yyyy";
	}
}
