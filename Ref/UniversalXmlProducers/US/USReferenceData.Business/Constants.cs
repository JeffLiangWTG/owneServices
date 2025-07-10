using System;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public static class Constants
	{
		public const string USCountryCode = "US";
		public const string AllCountries = "All Countries";
		public const int BatchSize = 50;

		public enum ActionResult { Successful, Failed };

		public static class DefaultValues
		{
			public readonly static DateTime MaxDateTime = new DateTime(2079, 06, 06, 23, 59, 00);
			public readonly static DateTime MinDateTime = new DateTime(1900, 01, 01, 00, 00, 00);
		}

		public static class CodeType
		{
			public const string CUSOF = "CUSOF";
			public const string PORT = "PORT";
			public const string USDDC = "USDDC";
			public const string AESCD = "AESCD";
			public const string FIRMS = "FIRMS";
			public const string ITAR = "ITAR";
		}

		public static class ExchangeRate
		{
			public const string DefaultRateType = "CUS";
			public const string InvalidRate = "N/A";
		}

		public static class ProgramFunctions
		{
			public const string Tariffs = "TARIFFS";
			public const string A99Tariffs = "A99";
			public const string ACEValidationRules = "ACEVR";
			public const string UnitedNationsStandardProductAndServiceCodes = "UNSPC";
			public const string ExchangeRate = "EXF";
			public const string ECCNNumbers = "ECCN";
			public const string NewWatchRules = "NEWWATCHRULES";
			public const string USIncomingMessageQuery = "USICMQ";
			public const string USIncomingMessageDownload = "USICMD";
			public const string DISCodes = "DISCODE";
			public const string AESDispostionCodes = "AESDISPOSTIONCODES";
			public const string ExemptionNumber = "EXEMPTIONNUMBER";
		}

		public static class USIncomingMessageRequest
		{
			public const string F101 = "F101";
			public const string F201 = "F201";
			public const string F301 = "F301";
			public const string F104 = "F104";
			public const string F111 = "F111";
			public const string F211 = "F211";
			public const string F311 = "F311";
			public const string F411 = "F411";
		}

		public static class RateTypes
		{
			public const string DTY = "DTY";
		}

		public static class TariffTypes
		{
			public const string EXP = "EXP";
			public const string SHB = "SHB";
			public const string HSN = "HSN";
		}

		public static class TransportMode
		{
			public const string AIR = "AIR";
			public const string FIX = "FIX";
			public const string INW = "INW";
			public const string MAI = "MAI";
			public const string RAI = "RAI";
			public const string ROA = "ROA";
			public const string SEA = "SEA";
			public const string AllModesValid = "B";
			public const string ExceptionOfAir = "V";
			public const string ExceptionOfVessel = "A";
			public const string SpaceFill = " ";
		}

		public static class DownloadFileNames
		{
			public const string EXP = "impaes.txt";
			public const string SHB = "expaes.txt";
		}

		public static class HtmlNodeNames
		{
			public const string H3 = "h3";
			public const string A = "a";
			public const string SPAN = "span";
			public const string ARTICLE = "article";
			public const string B = "b";
			public const string H2 = "h2";
		}

		public static class TariffUOMTypes
		{
			public const string CU1 = "CU1";
			public const string CU2 = "CU2";
		}

		public static class FileStructureAndUpdateInfoNodeKeywords
		{
			public const string EXP = "AES Import Concordance";
			public const string SHB = "AES Export Concordance";
		}

		public static class AttributeNames
		{
			public const string USDA_AMS = "USDA_AMS";
			public const string USDA_AMS_PGM = "USDA_AMS_PGM";
			public const string US_ECCN_MEU = "MEU";
			public const string US_ECCN_LicenseType = "LicenseType";
			public const string USDISDocCode = "USDISDocCode";
			public const string OptionalData = "OptionalData";
			public const string USDISRequiredData = "USDISRequiredData";
			public const string USDISFormGroup = "USDISFormGroup";
			public const string USDISPackageCategory = "USDISPackageCategory";
			public const string USDISSupportedFiletypes = "USDISSupportedFiletypes";
			public const string PortValidType = "PortValidType";
			public const string Address1 = "Address1";
			public const string Address2 = "Address2";
			public const string Address3 = "Address3";
			public const string City = "City";
			public const string State = "State";
			public const string PostCode = "PostCode";
			public const string Unlading = "Unlading";
			public const string ROLE = "ROLE";
			public const string AESSeverity = "AESSeverity";
			public const string AESReason = "AESReason";
			public const string AESResolution = "AESResolution";
			public const string EV1 = "EV1";
			public const string FacilityType = "FacilityType";
			public const string DistrictPortCode = "DistrictPortCode";
			public const string FacilityAddress = "FacilityAddress";
			public const string Country = "Country";
			public const string ZIPCode = "ZIPCode";
		}

		public static class AttributeValues
		{
			public const string Y = "Y";
			public const string MO6 = "MO6";
			public const string OTH = "OTH";
			public const string EG1 = "EG1";
			public const string PN1 = "PN1";
			public const string OR1 = "OR1";
			public const string INV = "INV";
			public const string PCK = "PCK";
			public const string CER = "CER";
			public const string BND = "BND";
			public const string COM = "COM";
			public const string TOX = "TOX";
			public const string PER = "PER";
			public const string CertificateNumber = "CertificateNumber";
			public const string IssueDate = "IssueDate";
			public const string ExpiryDate = "ExpiryDate";
			public const string GrossTonnage = "GrossTonnage";
			public const string NetTonnage = "NetTonnage";
			public const string DRW = "DRW";
			public const string FTZ = "FTZ";
			public const string NOGROUP = "NOGROUP";
			public const string CBMA = "CBMA";
			public const string USMCA = "USMCA";
			public const string NAFTA = "NAFTA";
			public const string GEN = "GEN";
			public const string PDF = "PDF";
			public const string AES = "AES";
			public const string InBond = "INB";
			public const string Common = "";
			public const string EXP = "EXP";
		}

		public static class RegionDistrictPortCode
		{
			public const string return1 = "1";
		}

		public static class DISCodeValues
		{
			public const string CommercialInvoice = "COMMERCIALINVOICENO-O";
			public const string InvoiceNumber = "INVOICENUMBER–O";
			public const string PurchaseOrder = "PURCHASEORDERNO–O";
			public const string CertifcateNumber = "CERTIFICATE_NUMBER–M";
			public const string IssueDdate = "ISSUE_DATE–M";
			public const string ExpirationDate = "EXPIRATION_DATE–M";
			public const string GrossTonnage = "GROSS_TONNAGE–M";
			public const string NetTonnage = "NET_TONNAGE–M";
			public const string BondType = "BONDTYPE–O";
			public const string BondAmount = "BOND AMOUNT–O";
			public const string Commodity = "COMMODITY";
			public const string VehicleIdentificationNumber = "VEHICLEIDENTIFICATIONNUMBER–O";
			public const string VehicleManufacturerNumber = "VEHICLEMANUFACTURER/MODEL/SERIALNUMBER–O";
			public const string CasNumber = "CASNUMBER–O";
			public const string ManufacturerNumber = "MANUFACTURER/MODEL/SERIALNUMBER–O";
			public const string EpaRegistrationNo = "EPAREGISTRATIONNO–O";
			public const string EpaProducerEstablishmentNo = "EPAPRODUCERESTABLISHMENTNO–O";
			public const string PermitNumber = "PERMITNUMBER–O";
			public const string Drawback = "DRAWBACK";
			public const string ForeignTradeZone = "FOREIGNTRADEZONE";
			public const string PDFOnly = "PDFSONLY";
		}

		public static class AESDispostionCodeMatchingText
		{
			public const string ResponseCode = "Response Code:";
			public const string NarrativeText = "Narrative Text:";
			public const string Severity = "Severity:";
			public const string Reason = "Reason:";
			public const string Reasons = "Reasons:";
			public const string Resolution = "Resolution:";
		}

		public static class SeverityCodes
		{
			public const string F = "F";
			public const string I = "I";
			public const string V = "V";
			public const string C = "C";
			public const string W = "W";
		}

		public static class ConditionType
		{
			public const string PGA = "PGA";
		}

		public static class PGACodes
		{
			public const string AMS = "AMS";
			public const string ATF = "ATF";
			public const string DEA = "DEA";
			public const string FWS = "FWS";
			public const string EPA = "EPA";
			public const string NMFS = "NMFS";
			public const string TTB = "TTB";
		}

		public static class ExcelColumnName
		{
			public const string HTSCode = "HTS Code";
			public const string ScheduleB = "Schedule B";
		}

		public static class FirmsCode
		{
			public const string Active = "A";
			public const string LastUpdateDate = "USIncomingMessage.F111.LastUpdateDate";
		}

		public const string MMDDYY = "MMddyy";
	}
}
