using System;
using System.Globalization;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries;

public static class DictionariesConstants
{
	public const string XmlDateTimeFormat = "yyyy-MM-dd";
	public const string DateTimeConverterFormat = "yyyy-MM-dd";
	public const string DefaultStartDate = "1900-01-01T00:00:00";
	public const string DefaultEndDate = "2079-06-06T23:59:00";
	public static DateTime DefaultEndDateDateTime => Convert.ToDateTime(DefaultEndDate, CultureInfo.InvariantCulture);
	public static DateTime DefaultStartDateDateTime => Convert.ToDateTime(DefaultStartDate, CultureInfo.InvariantCulture);
	
	public const string DictionariesUniversalReferenceDataXmlFilename = "PLDictionaries";

	public const string DataSource = "PL Dictionaries";

	public const int MaxDescriptionLen = 2000;

	public static class SupportedPuescDictionaries
	{//do not use enum numbers as reference, as some can have number + letter, for example : 001A,
	 //for whole list see https://puesc.gov.pl/uslugi/slowniki (currently used by PL customs)
	 //or https://test.puesc.gov.pl/uslugi/slowniki (extended also with currently not in use dictionaries)
		public const string TransactionCodes = "004";
		public const string COUNTRY_CODES = "007";
		public const string ExportRequiredDocuments = "CL213AES";
		public const string ImportRequiredDocuments = "034";
		public const string AdditionalInformationCodes = "036";
		public const string EU_COUNTRY_CODES = "049";
		public const string NOT_IN_EU_WPT_COUNTRY_CODES = "053";
		public const string ExportPreviousDocuments = "CL214AES";
		public const string ImportPreviousDocuments = "081";
		public const string PreviousDocumentsSpecialProcedures = "2900";
		public const string NationalAdditionalCodesForSADField33 = "109";
		public const string CarsMarkAndModelCodes = "3041";
		public const string AuthorisationType = "CL605AES";
		public const string NatureOfTransactionCode = "CL091AES";
		public const string ExportAdditionalInformation = "CL239AES";
		public const string ExportAdditionalReferencesType = "CL380AES";
		public const string ExportTransportDocumentCodes = "CL754AES";
		public const string EXPCustomsDeclarationUnitsOfQuantity = "CL349AES";
		public const string ImportProcedureCodes = "CL092AIS";
		public const string ExportProcedureCodes = "CL092AES";
		public const string ImportPreviousProcedureCodes = "CL093AIS";
		public const string ExportPreviousProcedureCodes = "CL093AES";
		public const string ImportConcessions = "CL457AIS";
		public const string ExportConcessions = "CL102AES";
		public const string NctsSupportingDocuments = "PL213NCTSP5";
		public const string NctsAuthorisationType = "PL235NCTSP5";
		public const string NctsAdditionalInformation = "PL239NCTSP5";
		public const string NctsGuarenteeType = "PL286NCTSP5";
		public const string CountryCodesFullList = "CL008AES";
		public const string EnquiryInformationCode = "CL210AES";
		public const string AlternativeEvidenceType = "CL170AES";
		public const string IdentificationOfGoodsInSpecialProcedures = "3300";
		public const string EconomicConditionsInSpecialProcedures = "3301";
	}

	public static class SupportedPuescDictionariesCW1Codes
	{
		public const string TransactionCodes = "TRNAT";
		public const string ExportRequiredDocuments = "DC44E";
		public const string ImportRequiredDocuments = "DC44I";
		public const string AdditionalInformationCodes = "ADDIN";
		public const string ExportPreviousDocuments = "DC40E";
		public const string ImportPreviousDocuments = "DC40I";
		public const string PreviousDocumentsSpecialProcedures = "DC40S";
		public const string NationalAdditionalCodesForSADField33 = "ADDCD";
		public const string CarsMarkAndModelCodes = "3041";
		public const string CO15 = "CO15";
		public const string CO17 = "CO17";
		public const string EU15 = "EU15";
		public const string EU17 = "EU17";
		public const string EX15 = "EX15";
		public const string EX17 = "EX17";
		public const string IM15 = "IM15";
		public const string IM17 = "IM17";
		public const string AuthorisationType = "AA44E";
		public const string NatureOfTransactionCode = "TNATE";
		public const string ExportAdditionalInformation = "AI44E";
		public const string ExportAdditionalReferencesType = "AR44E";
		public const string ExportTransportDocumentCodes = "TD44E";
		public const string EXPCustomsDeclarationUnitsOfQuantity = "CEXUQ";
		public const string CustomsProcedures = "CPC";
		public const string CustomsConcessions = "CPDC";
		public const string IMP34 = "IMP34";
		public const string EXP34 = "EXP34";
		public const string NctsSupportingDocuments = "DC44N";
		public const string NctsAuthorizationType = "PT235";
		public const string NctsAdditionalInformation = "AI44N";
		public const string NctsGuarenteeType = "PT286";
		public const string EnquiryInformationCode = "CL210";
		public const string AlternativeEvidenceType = "CL170";
		public const string IdentificationOfGoodsInSpecialProcedures = "3300";
		public const string EconomicConditionsInSpecialProcedures = "3301";
	}
}
