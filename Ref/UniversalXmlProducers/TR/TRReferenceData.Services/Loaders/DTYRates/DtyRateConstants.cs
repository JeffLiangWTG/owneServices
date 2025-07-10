using System;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class DtyRateConstants
	{
		public const string TariffCellFormat = "0###########";

		public const string NumericCellFormat = "0.###";

		public const string TariffCode = "GTİP";

		public const string Footnote = "DİPNOT";

		public const string TradingPartnerSeparator = ",";

		public const string PreferencesSheetName = "Preferences";

		public const string FootnotesSheetName = "Footnotes";

		public const string OtherCountries = "Other countries";

		public const string DtyRatesSubFolder = "DTYRates";

		public static readonly DateTime DefaultStartDate = new DateTime(2023, 01, 01, 00, 00, 00);

		public static readonly DateTime DefaultEndDate = new DateTime(2076, 06, 06, 23, 59, 00);
	}
}
