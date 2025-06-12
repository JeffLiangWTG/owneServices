using System;
using System.Globalization;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudSQLAccess
{
	public static class Helper
	{
		public const string SolarwindsNamespace = "http://schemas.solarwinds.com/2007/08/informationservice";

		public static DateTime ISO8601StringToDateTimeUtc(string value)
		{
			if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dateTime))
			{
				return dateTime;
			}

			return DateTime.MinValue;
		}
	}
}
