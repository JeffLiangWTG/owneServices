namespace CargoWise.RefDbRepo.PLReferenceData.Business.Cud
{
	public static class CudConstants
	{
		// for every single currency value from 2001.01.01 go to https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml
		public const string EUCentralBankExchangeRateXmlUrl = @"https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist-90d.xml"; // currencies from last 90 days

		public const string CudUniversalReferenceDataXmlFilename = "PLExchangeRate_CUD.xml";

		public static decimal UpperDifferenceMultiplier => 1.05m;
		public static decimal LowerDifferenceMultiplier => 0.95m;

		public static class SupportedCurrencyCodes
		{
			public const string PolishZloty = "PLN";
		}

		public const string CurrencyCodeEUR = "EUR";
	}
}
