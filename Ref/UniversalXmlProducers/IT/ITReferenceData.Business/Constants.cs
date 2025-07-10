using System.Globalization;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public static class Constants
	{
		public const string RateType = "CUS";
		public const string CountryISO = "IT";

		public static class Regex
		{
			public const string PublicationDateTime = @"Dati aggiornati al: (?<DateTime>\d{2}/\d{2}/\d{4})";
		}

		public static class RefDataGroupings
		{
			public const string Italy = "IT";
		}

		public static class Measures
		{
			public const string ImportTariffType = "IMP";
			public const string ExportTariffType = "EXP";
			public const string EuropeanUnionTradeCode = "EUN";
			public const string AdditionalCodeType = "ADDCD";
			public const string ExportStatisticalMonitoring = "ESM";
		}

		public static class ErrorMessages
		{
			const string NoResponseFromDatabase = "Unable to get {0} response from database";
			const string UnableToLoadFromDatabase = "Unable to load {0} from database";

			public static string TariffCodeLoaderNoResponseFromDatabase => string.Format(CultureInfo.InvariantCulture, NoResponseFromDatabase, "Tariff Code");
			public static string TradeGroupLookupNoResponseFromDatabase => string.Format(CultureInfo.InvariantCulture, NoResponseFromDatabase, "Trade Group Code");
			public static string AdditionalCodeLookupNoResponseFromDatabase => string.Format(CultureInfo.InvariantCulture, NoResponseFromDatabase, "Additional Code");
			public static string TariffCodeLoaderUnableToLoadFromDatabase => string.Format(CultureInfo.InvariantCulture, UnableToLoadFromDatabase, "Tariff Code");
			public static string TradeGroupLookupUnableToLoadFromDatabase => string.Format(CultureInfo.InvariantCulture, UnableToLoadFromDatabase, "Trade Group Code");
			public static string AdditionalCodeLookupUnableToLoadFromDatabase => string.Format(CultureInfo.InvariantCulture, UnableToLoadFromDatabase, "Additional Code");
		}
	}
}
