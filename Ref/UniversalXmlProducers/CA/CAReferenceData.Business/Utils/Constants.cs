using System;

namespace CargoWise.RefDbRepo.CAReferenceData.Business
{
	public static class Constants
	{
		public static class ProgramFunctions
		{
			public const string CAFacilityData = "CAFACILITYDATA";
			public const string CATariff = "CATARIFF";
			public const string TTCode = "TTCode";
			public const string CARate = "CARATE";
			public const string CATradeGroup = "CATRADEGROUP";
			public const string CFIAAIRSRegistrationTypes = "CFIAAIRSREGISTRATIONTYPES";
			public const string CFIAAIRSMiscData = "CFIAAIRSMISCDATA";
			public const string CAExchangeRate = "CAEXCHANGERATE";
			public const string CBSAErrorCode = "CBSAERRORCODE";
			public const string CAOfficeCode = "CAOFFICECODE";
			public const string ManualProcessor = "MANUALPROCESSOR";
			public const string CASIMAData = "CASIMADATA";
			public const string CASurtaxData = "CASURTAXDATA";
			public const string CAGSTCode = "CAGSTCODE";
		}

		public static class DefaultValues
		{
			public readonly static DateTime MaxDateTime = new DateTime(2079, 06, 06, 23, 59, 00);
			public readonly static DateTime MinDateTime = new DateTime(1900, 01, 01, 00, 00, 00);
			public const string CountryCodeCanada = "CA";
			public const string ENLanguage = "EN";
			public const string FRLanguage = "FR";
			public const string MessageNumber = "Message Number";
			public const string FreeTariff = "Free";
			public const string NA = "N/A";
			public const string Undefined = "UNDEFINED";
		}

		public static class DataSource
		{
			public const string TariffData = "CA Harmonized Tariff";
			public const string TariffDataRateType = "CA Harmonized Tariff Rates";
			public const string ExchangeRate = "CA CBSA CUS Exchange Rate";
			public const string TradeGroup = "CA Tariff Treatment Trade Groups";
			public const string Preference = "CA Trade Preferences";
			public const string OfficeCode = "CA Office Code";
			public const string DocumentTypes = "CA Document Types";
			public const string StandingData = "CA Standing Data";
			public const string CFIAAIRSMiscellaneousCodes = "CA CFIA AIRS Miscellaneous Codes";
			public const string CFIAAIRSRegistrationTypes = "CFIA AIRS Registration Types";
			public const string CARMGSTCodes = "CA CARM GST Codes";
		}

		public static class DownloadFileNames
		{
			public const string AccessDataSourceFileName = "AccessDataSource.zip";
			public const string ClassificationFileUpdates = "ClassificationFileUpdates.zip";
			public const string ClassificationFileExpiries = "ClassificationFileExpiries.zip";
			public const string TariffCodeFileUpdates = "TariffCodeFileUpdates.zip";
			public const string TariffCodeFileExpires = "TariffCodeFileExpires.zip";
			public const string CFIAAIRSRegistrationTypesInEnglish = "CFIAAIRSRegistrationTypes_English.pdf";
			public const string CFIAAIRSRegistrationTypesInFrench = "CFIAAIRSRegistrationTypes_French.pdf";
		}

		public static class CARMAPIQueryTypes
		{
			public const string TariffQueryType = "tariffClassifications";
			public const string ExciseTaxesQueryType = "exciseTaxes";
			public const string ExciseTaxCodesQueryType = "exciseTaxCodes";
			public const string CustomsDutiesQueryType = "customsDuties";
			public const string ExciseDutiesQueryType = "exciseDuties";
			public const string GSTCodesQueryType = "gstCodes";
		}

		public static class CARMConstants
		{
			public const int TariffNumberLength = 10;
			public const int TariffItemNumberLength = 8;
			public const int GSTCodeLength = 3;
			public const string CustomsDutyFilter = "FreeQualifierIndicator ne 'X'";
		}

		public static class CADocumentTypes
		{
			public const string PGA_Name = "PGA";
			public const string ValuesAllowed_Name = "ValuesAllowed";
			public const string GIP80_Value = "GIP80";
			public const string GIP81_Value = "GIP81";
			public const string XXX_Value = "XXX";
			public const string Code2006 = "2006";
			public const string Yes = "Y";
		}

		public static class CodeType
		{
			public const string CAERR = "CAERR";
			public const string CUSOF = "CUSOF";
			public const string SUBLC = "SUBLC";
			public const string CADOC = "CADOC";
			public const string CAGST = "CAGST";
		}

		public static class ExchangeRate
		{
			public const string DefaultRateType = "CUS";
		}

		public static class AttributeName
		{
			public const string Province = "Province";
			public const string USPortOfExit = "USPortOfExit";
			public const string CheckIndicator = "CheckIndicator";
			public const string CheckGroup = "CheckGroup";
			public const string RateType = "RateType";
			public const string Rate = "Rate";
		}

		public static class ConditionValue
		{
			public const string ALL = "ALL";
			public const string API = "API";
			public const string BBC = "BBC";
			public const string CTO = "CTO";
			public const string CPR = "CPR";
			public const string DSE = "DSE";
			public const string HDR = "HDR";
			public const string OCS = "OCS";
			public const string MDE = "MDE";
			public const string NHP = "NHP";
			public const string PES = "PES";
			public const string RED = "RED";
			public const string VET = "VET";
			public const string HAP = "HAP";
			public const string TPR = "TPR";
			public const string VPR = "VPR";
			public const string WRM = "WRM";
			public const string ODS = "ODS";
			public const string VEE = "VEE";
			public const string WEN = "WEN";
			public const string EEF = "EEF";
			public const string EXP = "EXP";
			public const string RDA = "RDA";
			public const string ABI = "ABI";
			public const string AIS = "AIS";
			public const string TTP = "TTP";
		}

		public static class ConditionType
		{
			public const string PGA = "PGA";
			public const string PGAC = "PGAC";
		}

		public const string CanadaHarmonizedTariff = "HSN";
		public const string UOMTypeCU1 = "CU1";
		public const string UOMTypeCU2 = "CU2";
		public const string ConveyanceRequiredTariffAttrName = "CONVEYANCEREQUIRED";
		public const string RateTypeDty = "DTY";
		public const string RateTypeDtyDescription = "Duty";
		public const string RateTypeExc = "EXC";
		public const string RateTypeExcDescription = "Excise Duty";
		public const string RateTypeExs = "EXS";
		public const string RateTypeExsDescription = "Excise Tax";
		public const string EntityMatcherCOUNTRYUri = "COUNTRY";
		public const string EntityMatcherCURRENCYUri = "CURRENCY";
		public const string EntityMatcherUNITCODEUri = "CACUSTOMUOM";
		public const string AntiDumping = "ADD";
		public const string Countervailing = "CVD";
		public const string SpecialImportMeasureAct = "SIMA";
		public const string SurTax = "SUR";
		public const string USCountryCode = "US";

		public static string Xpath(this string column)
		{
			return "d:" + column;
		}
	}
}
