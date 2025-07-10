namespace CargoWise.RefDbRepo.MXReferenceData.Business
{
	public static class Constants
	{
		#region Log Names

		public const string MXExchangeRatesLog = "MX_EXCHANGE_RATE";
		public const string MXCustomsFacilitiesLog = "MX_CUSTOMS_SECTION_APPENDICE1";
		public const string MXTariffRatesLog = "MX_TARIFF_RATE";

		#endregion

		public static class ProgramFunctions
		{
			public const string CustomsExchangeRate = "EXCHANGE_RATE";
			public const string CustomsFacilities = "CUSTOMS_FACILITIES";
			public const string CustomsTariffRate = "TARIFF_RATE";
		}

		public static class DataGroupingCodes
		{
			public const string Mexico = "MX";
		}

		public static class ExchangeRateTypes
		{
			public const string CustomsExport = "CUE";
			public const string Customs = "CUS";
		}

		public static class ExchangeCurrencyCodes
		{
			public const string Dolar = "USD";
		}

		public static class ExchangeRateFiles
		{
			public const string CTARC_DEPAIS = "CTARC_DEPAIS.xml";
			public const string CTARC_TIPCAM = "CTARC_TIPCAM.xml";
		}

		public static class CodeTypes
		{
			public static class Codes
			{
				public const string CustomsFacilities = "FAC";
				public const string CustomsFacilitiesDescription = "Customs Facilities";
			}
		}
	}
}
