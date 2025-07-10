namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public static class Constants
	{
		public static class ProgramFunctions
		{
			public const string ExchangeRates = "EXCHANGERATES";
			public const string Tariffs = "TARIFFS";
			public const string ExportNonGAReasonTypes = "EXPORTNONGAREASONTYPES";
			public const string ImportNonGAReasonTypes = "IMPORTNONGAREASONTYPES";
			public const string Preferences = "PREFERENCES";
			public const string PreferenceRefCusMap = "PREFERENCEREFCUSMAP";
			public const string ExportFTAType = "EXPORTFTATYPE";
			public const string OGAImport = "OGAIMPORT";
			public const string OGAExport = "OGAEXPORT";
			public const string TradeGroup = "TRADEGROUP";
			public const string WCONomenclatures = "WCONOMENCLATURES";
			public const string KRNomenclatures = "KRNOMENCLATURES";
			public const string SteelNomenclatures = "STEELNOMENCLATURES";
			public const string SimpleDrawback = "SIMPLEDRAWBACK";
			public const string CustomsOffice = "CUSTOMSOFFICE";
			public const string CustomsDepartment = "CUSTOMSDEPARTMENT";
			public const string ForwarderIDs = "FORWARDERIDS";
			public const string OtherGovernment = "OTHERGOVERNMENT";
			public const string ExpressDeliveryServiceID = "EXPRESSDELIVERYSERVICEID";
			public const string BrandCodes = "BRANDCODES";
			public const string DutyRates = "DUTYRATES";
			public const string DutyReductionExemption = "DUTYREDUCTIONEXEMPTION";
			public const string DomesticTaxExemption = "DOMESTICTAXEXEMPTION";
			public const string DomesticTaxRates = "DOMESTICTAXRATES";
			public const string SpecialUseCodeDutyRates = "SPECIALUSECODEDUTYRATES";
			public const string InstalmentCodes = "INSTALMENTCODES";
			public const string AdditionalPaymentReasons = "ADDITIONALPAYMENTREASONS";
			public const string TaxOffice = "TAXOFFICE";
			public const string OGARegulationCategory = "OGAREGULATIONCATEGORY";
			public const string HSExtensionCodesMain = "HSEXTENSIONCODESMAIN";
			public const string HSExtensionCodesSub = "HSEXTENSIONCODESSUB";
		}
		public static class RateTypes
		{
			public const string ExportExRateType = "CUE";
			public const string ImportExRateType = "CUS";
		}
		public static class RateCodes
		{
			public const string AdValoremRate = "DTA";
			public const string SpecificRate = "DTS";
		}
		public static class TariffTypes
		{
			public const string HSN = "HSN";
		}
		public static class CountryCodes
		{
			public const string KoreaSouth = "KR";
		}
		public static class LanguageCodes
		{
			public const string Korean = "KO";
		}
		public static class DataGrouping
		{
			public const string WCO = "WCO";
		}
		public static class RomanNumericCharacters
		{
			public const string I = "I";
			public const string IV = "IV";
			public const string V = "V";
			public const string IX = "IX";
			public const string X = "X";
			public const string XL = "XL";
			public const string L = "L";
			public const string XC = "XC";
			public const string C = "C";
		}
		public static class NomenclatureCategories
		{
			public const string Section = "Section";
			public const string Chapter = "Chapter";
		}
		public static class DataSources
		{
			public const string ExportExchangeRate = "KR Export Exchange Rates";
			public const string ImportExchangeRate = "KR Import Exchange Rates";
			public const string Tariffs = "KR Tariffs";
			public const string ExportNonGAReasonType = "KR Export Non GA Reason Type";
			public const string ImportNonGAReasonType = "KR Import Non GA Reason Type";
			public const string RefCusPreference = "KR RefCusPreference";
			public const string PreferenceRefCusMap = "KR Preference RefCusMap";
			public const string ExportFTAType = "KR Export FTA Type";
			public const string OGA = "KR OGA";
			public const string TradeGroup = "KR RefCusTradeGroup";
			public const string WCOCopiedNomenclature = "KR Nomenclatures copies of WCO";
			public const string KRNomenclature = "KR Nomenclatures";
			public const string SteelNomenclature = "KR Steel Nomenclatures";
			public const string SimpleDrawback = "KR Simple Drawback";
			public const string CustomsOffice = "KR Customs Offices";
			public const string CustomsDepartment = "KR Customs Departments";
			public const string ForwarderIDs = "KR Forwarder IDs";
			public const string OtherGovernment = "KR Other Government and Associated agencies";
			public const string ExpressDeliveryServiceID = "KR Express Delivery Service IDs";
			public const string BrandCodes = "KR Brand Codes";
			public const string DutyRates = "KR Duty Rates";
			public const string DutyReductionExemption = "KR Duty Reduction Exemption";
			public const string DomesticTaxExemption = "KR Domestic Tax Exemption";
			public const string DomesticTaxRates = "KR Domestic Tax Rates";
			public const string SpecialUseCodeDutyRates = "KR Special Use Code Duty Rates";
			public const string InstalmentCodes = "KR Instalment Codes";
			public const string AdditionalPaymentReasons = "KR Additional Payment Reasons";
			public const string TaxOffice = "KR Tax Office";
			public const string OGARegulationCategory = "KR OGA Regulation Category";
			public const string HSExtensionCodesMain = "KR HS Extension Codes Main";
			public const string HSExtensionCodesSub = "KR HS Extension Codes Sub";
		}
		public static class OutputFileName
		{
			public const string ExportExchangeRate = "KRRefExchangeRateZZ_KR_Export.xml";
			public const string ImportExchangeRate = "KRRefExchangeRateZZ_KR_Import.xml";
			public const string Tariffs = "KRRefCusTariff_KR.xml";
			public const string ExportNonGAReasonType = "KRNonGAReasonType_Export.xml";
			public const string ImportNonGAReasonType = "KRNonGAReasonType_Import.xml";
			public const string RefCusPreference = "KRRefCusPreference_Result.xml";
			public const string PreferenceRefCusMap = "KRPreferenceRefCusMap_Result.xml";
			public const string ExportFTAType = "KRExportFTAType_Result.xml";
			public const string OGAExport = "KRRefCusTariff_OGA_Export.xml";
			public const string OGAImport = "KRRefCusTariff_OGA_Import.xml";
			public const string TradeGroup = "KRTradeGroupAndCountry.xml";
			public const string WCOCopiedNomenclature = "KRRefCusNomenclatureGroup_CopiedFromWCO.xml";
			public const string KRNomenclature = "KRRefCusNomenclatureGroup.xml";
			public const string SteelNomenclature = "KRSteelRefCusNomenclatureGroup.xml";
			public const string SimpleDrawback = "KRSimpleDrawback_Result.xml";
			public const string CustomsOffice = "KRCustomsOffice_Result.xml";
			public const string CustomsDepartment = "KRCustomsDepartment_Result.xml";
			public const string ForwarderIDs = "KRForwarderIDs_Result.xml";
			public const string OtherGovernment = "KROtherGovernment_Result.xml";
			public const string ExpressDeliveryServiceID = "KRExpressDeliveryServiceID_Result.xml";
			public const string BrandCodes = "KRBrandCodes_Result.xml";
			public const string DutyRates = "KRDutyRate.xml";
			public const string DutyReductionExemption = "KRDutyReductionExemption.xml";
			public const string DomesticTaxExemption = "KRDomesticTaxExemption_Result.xml";
			public const string DomesticTaxRates = "KRDomesticTaxRates_Result.xml";
			public const string SpecialUseCodeDutyRates = "KRSpecialUseCodeDutyRate.xml";
			public const string InstalmentCodes = "KRInstalmentCode_Result.xml";
			public const string AdditionalPaymentReasons = "KRAdditionalPaymentReasons.xml";
			public const string TaxOffice = "KRTaxOffice_Result.xml";
			public const string OGARegulationCategory = "KROGARegulationCategoryResult.xml";
			public const string HSExtensionCodesMain = "KRHSExtensionCodes_Main.xml";
			public const string HSExtensionCodesSub = "KRHSExtensionCodes_Sub.xml";
		}

		public static class Suffix
		{
			public const string Import = "_Import";
			public const string Export = "_Export";
		}
		public static class FileExtensions
		{
			public const string XLSX = ".xlsx";
			public const string XLS = ".xls";
			public const string XML = ".xml";
		}

		public static class DataType
		{
			public const string Datetime = "datetime";
			public const string Varchar = "varchar";
			public const string NVarchar = "nvarchar";
			public const string NVarcharMax = "nvarchar(max)";
		}

		public static class CodeTypes
		{
			public const string EXFTA = "EXFTA";
			public const string EXTPF = "EXTPF";
		}

		public static class UOMTypes
		{
			public const string CU1 = "CU1";
			public const string CU2 = "CU2";
			public const string CU3 = "CU3";
		}
		public static class CodeListAttributeNames
		{
			public const string FTATradeGroup = "FTATradeGroup";
			public const string TradePreferenceTradeGroup = "TradePreferenceTradeGroup";
		}

		public static class CodeListAttributeValues
		{
			public const string SpecialConsumptionTax = "SCT";
			public const string LiquorTax = "LQT";
			public const string VAT = "VAT";
			public const string TransportationTax = "TRT";
			public const string EducationTax = "EDT";
		}

		public static class NomenclaturePadZerosToRemove
		{
			public const string TwoZeros = "00";
			public const string FourZeros = "0000";
			public const string SixZeros = "000000";
		}

		public static class DutyRateApplicableTypes
		{
			public const string Min = "MIN";
			public const string Max = "MAX";
		}

		public static class YesNo
		{
			public const string Yes = "Y";
			public const string No = "N";
		}

		public static class DomesticTaxClassificationCode
		{
			public const string A = "A";
			public const string B = "B";
			public const string C = "C";
			public const string D = "D";
			public const string E = "E";
			public const string F = "F";
		}

		public static class DomesticRateTypes
		{
			public const string StandardRate = "A";
			public const string PreferentialRate = "B";
		}

		public static class HSExtensionCodesConstants
		{
			public const int DescriptionMaxLength = 200;
			public const string EmptySymbol = "-";
		}

		public const int TariffLength = 10;
		public const int WCONomenclatureMaxLength = 6;
		public const string PublicationDateFormat = "yyyy/MM/dd";
	}
}
