namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Common
{
	public static class Constants
	{
		public const string ZADataGrouping = "ZA";
		public const string CheckDigit = "CheckDigit";
		public const string VatFeeCode = "VAT";

		public static class ProgramFunctions
		{
			public const string Tariffs = "TARIFF";
			public const string Carriers = "CARRIERS";
			public const string ExchangeRates = "EXCHANGERATES";
		}

		public static class Schedules
		{
			public const string S1P1 = "1P1";
			public const string S2P1 = "2P1";
			public const string S2P3 = "2P3";
			public const string S6P1 = "6P1";
		}

		public static class Preferences
		{
			public const string None = "100";
			public const string PreferentialRate = "200";
			public const string PreferentialQuota = "400";
		}

		public static class RateTypes
		{
			public const string Standard = "STANDARD";
			public const string EU = "EU";
			public const string EFTA = "EFTA";
			public const string MERCOSUR = "MERCOSUR";
			public const string AFCFTA = "AFCFTA";
			public const string SADC = "SADC";
			public const string EUQuota = "EUQUOTA";
			public const string EFTAQuota = "EFTAQUOTA";
		}

		public static class RuleSelectors
		{
			public const string EUQuota = "pp='EUQUOTA'";
			public const string EFTAQuota = "pp='EFTAQUOTA'";
		}

		public static class TradeGroups
		{
			public const string Standard = "STANDARD";
			public const string EU = "EUTRADE";
			public const string EFTA = "EFTA";
			public const string MERCOSUR = "MERCOSUR";
			public const string AFCFTA = "AFCFTA";
			public const string SADC = "SADC";
			public const string EUQuota = "EUQUOTA";
			public const string EFTAQuota = "EFTAQUOTA";
		}

		public static class CountryGroupings
		{
			public const string AllCountries = "ALL COUNTRIES";
			public const string EU = "EU";
		}

		public static class Attributes
		{
			public const string CargoCarrier = "CARGOCARRIER";
			public const string Master = "MASTER";
		}

		public enum TransactionType
		{
			Undefined = 0,
			Original = 9,
			Change = 4,
			Deletion = 3,
			Addition = 2
		}
	}
}
