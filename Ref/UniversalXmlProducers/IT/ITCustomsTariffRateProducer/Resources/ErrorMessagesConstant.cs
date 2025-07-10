using System.Globalization;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources
{
	public static class ErrorMessagesConstant
	{
		const string NoResponseFromDatabase = "Unable to get {0} response from database";
		const string AppSettingMissing = "Key: {0} is missing in App Config";
		const string UnableToLoadFromDatabase = "Unable to load {0} from database";

		public static string TariffCodeLoaderNoResponseFromDatabase => string.Format(CultureInfo.InvariantCulture, NoResponseFromDatabase, "Tariff Code");
		public static string TradeGroupLookupNoResponseFromDatabase => string.Format(CultureInfo.InvariantCulture, NoResponseFromDatabase, "Trade Group Code");
		public static string TaxOrFeeCodeLookupNoResponseFromDatabase => string.Format(CultureInfo.InvariantCulture, NoResponseFromDatabase, "TaxOrFee Code");
		public static string RateCodeDataLookupNoResponseFromDatabase => string.Format(CultureInfo.InvariantCulture, NoResponseFromDatabase, "Rate Code");
		public static string PreferenceDataLookupNoResponseFromDatabase => string.Format(CultureInfo.InvariantCulture, NoResponseFromDatabase, "Preference Code");
		public static string TariffCodeLoaderUnableToLoadFromDatabase => string.Format(CultureInfo.InvariantCulture, UnableToLoadFromDatabase, "Tariff Code");
		public static string TradeGroupLookupUnableToLoadFromDatabase => string.Format(CultureInfo.InvariantCulture, UnableToLoadFromDatabase, "Trade Group Code");
		public static string TaxOrFeeCodeLookupUnableToLoadFromDatabase => string.Format(CultureInfo.InvariantCulture, UnableToLoadFromDatabase, "TaxOrFee Code");
		public static string RateCodeDataLookupUnableToLoadFromDatabase => string.Format(CultureInfo.InvariantCulture, UnableToLoadFromDatabase, "Rate Code");
		public static string PreferenceDataLookupUnableToLoadFromDatabase => string.Format(CultureInfo.InvariantCulture, UnableToLoadFromDatabase, "Preference Code");

		public static string CustomsWebUrlAppSettingMissing => string.Format(CultureInfo.InvariantCulture, AppSettingMissing, "ITCustomsWebUrl");
		public static string CustomsWebUrlPublicationDateAppSettingMissing => string.Format(CultureInfo.InvariantCulture, AppSettingMissing, "ITCustomsWebUrl_PublicationDate");
		public static string SafeDbServiceUrlAppSettingMissing => string.Format(CultureInfo.InvariantCulture, AppSettingMissing, "SafeDBServiceUrl");

		public const string InvalidLengthTariffCode = "Tariff Code should contain 10 digits";
		public const string TariffCodeNoResponseFromWeb = "Unable to get response from website";

		public const string NoNationalSection = "Unable to retrieve National section";
		public const string NoTariffDescription = "Unable to retrieve tariff description";
		public const string NoMeasureFound = "No Measure found";
		public const string VatInformationNotErgaOmnes = "VAT is not set to ERGA OMNES trade group";
		public const string InvalidTaxOrFeeCode = "Unable to map TaxOrFeeCode";
		public const string TradeGroupNotFound = "Unable to map Trade Group Code";
		public const string UnableToMapRateFormula = "Unable to map Rate Formula";
		public const string NoRateCodeFound = "Unable to map Rate Code";
		public const string ExciseContainsMultipleCodes = "Excise contains multiple Codes";
		public const string UnexpectedError = "Unexpected error";

		public const string NoMeasureInScrappedRecord = "No measure found in scrapped record";
		public const string NoRequirementInScrappedRecord = "Unable to retrieve requirements";
	}
}
