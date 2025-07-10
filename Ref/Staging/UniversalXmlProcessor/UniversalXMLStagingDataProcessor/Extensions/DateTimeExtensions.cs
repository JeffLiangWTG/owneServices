using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class DateTimeExtensions
	{
		public static string GetFormatString(this DateTime value)
		{
			Argument.NotNull(value, nameof(value));

			return value.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
		}

		public static string GetFormatString(this DateTimeOffset value)
		{
			Argument.NotNull(value, nameof(value));

			return value.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
		}
	}
}
