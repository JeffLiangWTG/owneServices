using System;

namespace CargoWise.RefDbRepo.SEReferenceData.Business
{
	public static class Constants
	{
		public static class ProgramFunctions
		{
			public const string ExchangeRates = "EXCHANGERATES";
			public const string CodeLists = "CODELISTS";
			public const string GoodsNomenclature = "GOODSNOMENCLATURE";
			public const string TradeGroups = "TRADEGROUPS";
			public const string MeasureTypes = "MEASURETYPES";
		}

		public static class LanguageCode
		{
			public const string Swedish = "SV";
			public const string English = "EN";
		}

		public static class DataGrouping
		{
			public const string Sweden = "SE";
		}

		public static class TradegroupLength
		{
			public const int TradeGroupCodeLength = 4;
			public const int CountryCodeLength = 2;
		}

		public static class NationalCodes
		{
			public const int Sweden = 1;
		}

		public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);

		public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

		public static DateTime AnnoDominiOne => new DateTime(01, 01, 01, 00, 00, 00);

		public static DateTime EarliestSupportedEndDate => new DateTime(2023, 01, 01, 00, 00, 00);
	}
}
