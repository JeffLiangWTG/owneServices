using System;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public static class Constants
	{
		public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);
		public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

		public static class Functions
		{
			public const string OfficeCodes = "OFFICECODES";
			public const string CustomsMeursing = "CUSTOMSMEURSING";
			public const string CUSNumbers = "CUSNUMBERS";
			public const string KindOfPackages = "KINDOFPACKAGES";
			public const string MethodOfPayment = "METHODOFPAYMENT";
			public const string DocumentTypeCommon = "DOCUMENTTYPECOMMON";
			public const string AdditionalSupplyChainActorRoleCode = "ADDITIONALINFORMATIONCODE";
			public const string AdditionalInformationCodeSubset = "ADDITIONALINFORMATIONCODESUBSET";
			public const string ICS2FunctionalErrorCodes = "ICS2FUNCTIONALERRORCODES";
			public const string ICS2HRCMScreeningMethod = "ICS2HRCMSCREENINGMETHOD";
			public const string ICS2UnLocodeExtended = "ICS2UNLOCODEEXTENDED";
			public const string AESAdditionalInformation = "AESADDITIONALINFORMATION";
			public const string AESAdditionalReference = "AESADDITIONALREFERENCE";
			public const string AESTransportDocumentType = "AESTRANSPORTDOCUMENTTYPE";
			public const string AESPreviousDocumentType = "AESPREVIOUSDOCUMENTTYPE";
			public const string AESNationality = "AESNATIONALITY";
			public const string EUCodeLists = "EUCODELISTS";
			public const string TypeOfGoods = "TYPEOFGOODS";
			public const string TypeOfMeansOfTransport = "TYPEOFMEANSOFTRANSPORT";
			public const string CountryCodeICS2MS = "COUNTRYCODEICS2MS";
			public const string AuthorisationType = "AUTHORISATIONTYPE";
			public const string StateSubset = "STATESUBSET";
			public const string CCICodes= "CCICODES";
		}

		public static class OfficeCodes
		{
			static readonly XNamespace ns0 = "http://xmlns.ec.eu/BusinessObjects/CSRD2/RDEntityEntryListType/V2";
			static readonly XNamespace ns1 = "http://xmlns.ec.eu/BusinessObjects/CSRD2/RDEntityType/V2";
			static readonly XNamespace ns2 = "http://xmlns.ec.eu/BusinessObjects/CSRD2/RDEntryType/V2";
			static readonly XNamespace ns3 = "http://xmlns.ec.eu/BusinessObjects/CSRD2/RDStatusType/V2";
			public readonly static XName RootElementName = ns0 + "RDEntity";
			public readonly static XName CustomsOfficeElementName = ns1 + "RDEntry";
			public readonly static XName DefaultElementName = ns2 + "dataItem";
			public readonly static XName RoleParentElementName = ns2 + "dataGroup";
			public readonly static XName StartDateElement = ns3 + "activeFrom";
			public readonly static XName EndDateElement = ns3 + "activeTo";
			public readonly static XName LanguagesElementName = ns2 + "dataGroup";
			public const string RootAttributeValue = "CustomsOffices";
			public const string ReferenceNumberAttributeValue = "ReferenceNumber";
			public const string CountryCodeAttributeValue = "CountryCode";
			public const string UsualNameAttributeValue = "CustomsOfficeUsualName";
			public const string CityAttributeValue = "City";
			public const string PostalCodeAttributeValue = "PostalCode";
			public const string StreetAndNumberAttributeValue = "StreetAndNumber";
			public const string EMailAddressAttributeValue = "EMailAddress";
			public const string LanguageCodeAttributeValue = "LanguageCode";
			public const string RoleParentAttributeValue = "CustomsOfficeRoleTrafficCompetence";
			public const string RoleAttributeValue = "Role";
			public const string TrafficTypeAttributeValue = "TrafficType";
			public const string LanguagesAttributeValue = "CustomsOfficeLsd";
		}

		public static class CUSNumbers
		{
			public const string LASTUPDATESTRING = "Last update:";
			public const int LENGTHOFLASTUPDATESEARCHAREA = 100;

			public const int ItemsPerPage = 25;
			public const string NoDataFoundText = "No data matches the criteria";
			public const string TableText = "<table id=\"tblData";

			public const string CNCodeAttributeValue = "CNCODE";
			public const string CASrnAttributeValue = "CASRN";
			public const string ECNUMBERAttributeValue = "ECNUMBER";
			public const string UNnumberAttributeValue = "UNNUMBER";
			public const string CusCodeAttributeName = "CL016";
		}

		public static class Common
		{
			public static readonly XNamespace ns2 = "http://xmlns.ec.eu/BusinessObjects/CSRD2/RDEntityEntryListType/V2";
			public static readonly XNamespace ns3 = "http://xmlns.ec.eu/BusinessObjects/CSRD2/RDEntityType/V2";
			public static readonly XNamespace ns4 = "http://xmlns.ec.eu/BusinessObjects/CSRD2/RDEntryType/V2";
			public static readonly XNamespace ns5 = "http://xmlns.ec.eu/BusinessObjects/CSRD2/RDStatusType/V2";
			public static readonly XNamespace ns7 = "http://xmlns.ec.eu/BusinessObjects/CSRD2/LsdListType/V2";
			public static readonly XName RDEntity = ns2 + "RDEntity";
			public static readonly XName RDEntry = ns3 + "RDEntry";
			public static readonly XName DataItem = ns4 + "dataItem";
			public static readonly XName State = ns5 + "state";
			public static readonly XName ActiveFrom = ns5 + "activeFrom";
			public static readonly XName Description = ns7 + "description";

			public const string SourceDateFormatXML = "yyyy-MM-dd";
			public const string DefaultLanguage = "EN";
			public const string DescriptionAttributeValue = "lang";
			public const string Valid = "valid";
			public const string RDEntityAttributeName = "name";

			public const string EUNCountryCode = "EUN";
		}

		public static class MethodOfPayment
		{
			public const string RDEntityAttributeValue = "TransportChargesMethodOfPayment";
		}

		public static class DocumentTypeCommon
		{
			public const string RDEntityAttributeValue = "DocumentTypeCommon";
			public const string DocumentTypeAttributeValue = "DocumentType";
			public const string TransportDocumentAttributeValue = "TransportDocument";
			public const string IsTransportDocument = "IsTransportDocument";
			public const string Yes = "Y";
		}

		public static class KindOfPackages
		{
			public const string RDEntityAttributeValue = "KindOfPackages";
			public const string CodeAttributeValue = "KindOfPackages";
		}

		public static class AdditionalSupplyChainActorRoleCode
		{
			public const string RDEntityAttributeValue = "AdditionalSupplyChainActorRoleCode";
			public const string RoleAttributeValue = "Role";
		}

		public static class ICS2HRCMScreeningMethod
		{
			public const string RDEntityAttributeValue = "HRCMScreeningMethod";
			public const string RoleAttributeValue = "Code";
		}

		public static class AdditionalInformationSubsetCode
		{
			public const string RDEntityAttributeValue = "AdditionalInformationCodeSubset";
			public const string CodeAttributeValue = "Code";
		}

		public static class ICS2FunctionalErrorCodes
		{
			public const string RDEntityAttributeValue = "ICS2FunctionalErrorCodes";
			public const string CodeAttributeValue = "Code";
		}

		public static class AESAdditionalInformation
		{
			public const string RDEntityAttributeValue = "AdditionalInformation";
			public const string CodeAttributeValue = "AdditionalInformationCode";
		}

		public static class AESAdditionalReference
		{
			public const string RDEntityAttributeValue = "AdditionalReference";
			public const string CodeAttributeValue = "DocumentType";
		}

		public static class AESTransportDocumentType
		{
			public const string RDEntityAttributeValue = "TransportDocumentType";
			public const string CodeAttributeValue = "Type";
		}

		public static class AESPreviousDocumentType
		{
			public const string RDEntityAttributeValue = "PreviousDocumentType";
			public const string CodeAttributeValue = "PreviousDocumentTypeCode";

			public const string OutputFileName = "EUAES_PreviousDocumentType.xml";
			public const string OutputFileDataSource = "EU AES - PreviousDocumentType";
		}

		public static class AESNationality
		{
			public const string RDEntityAttributeValue = "Nationality";
			public const string CodeAttributeValue = "CountryCode";

			public const string OutputFileName = "EUAES_Nationality.xml";
			public const string OutputFileDataSource = "EU Export Nationality";
		}

		public static class TypeOfGoods
		{
			public const string RDEntityAttributeValue = "TypeoOfGoods";
			public const string CodeAttributeValue = "Code";

			public const string OutputFileName = "EUICS2_TypeOfGoods.xml";
			public const string OutputFileDataSource = "EU ICS2 - Type of Goods (CL749)";
		}

		public static class TypeOfMeansOfTransport
		{
			public const string RDEntityAttributeValue = "TypeOfMeansOfTransport";

			public const string OutputFileName = "EUICS2_TypeOfMeansOfTransport.xml";
			public const string OutputFileDataSource = "EU ICS2 - Type of Means of Transport (CL751)";
		}

		public static class StateSubset
		{
			public const string RDEntityAttributeValue = "StateSubset";
			public const string CodeAttributeValue = "Code";
			public const string OutputFileName = "EUICS2_StateSubset.xml";
			public const string OutputFileDataSource = "EU ICS2 - State Subset";
		}

		public static class CountryCodeICS2MS
		{
			public const string RDEntityAttributeValue = "CountryCodeICS2MS";
			public const string CodeAttributeValue = "CountryCode";
			public const string OutputFileName = "EUICS2_CountryCodeICS2MS.xml";
			public const string OutputFileDataSource = "EU ICS2 - Country Code ICS2MS (CL717)";
		}

		public static class UnLocodeExtended
		{
			public const string RDEntityAttributeValue = "UnLocodeExtended";
			public const string CodeAttributeValue = "UnLocodeExtendedCode";
			public const string DescriptionAttributeValue = "Name";

			public const string OutputFileName = "EUICS2_UnLocodeExtended.xml";
			public const string OutputFileDataSource = "EU ICS2 - UnLocode Extended (CL244)";
		}

		public static class AuthorisationType
		{
			public const string RDEntityAttributeValue = "AuthorisationType";
			public const string OutputFileName = "RefCusCodeListZZ_EUN_AUTH.xml";
			public const string OutputFileDataSource = "EU Authorisation Type";
		}

		public static class CCIPreviousDocumentType
		{
			public const string RDEntityAttributeValue = "PreviousDocumentType";
			public const string DataItemAttributeValue = "PreviousDocumentTypeCode";
			public const string Description = "CCI Previous Document Type (UCC6 Import)";
			public const string CodeType = "214IM";
			public const string OutputFileName = "RefCusCodeListZZ_EUN_214IM.xml";
			public const string OutputFileDataSource = "EUN CCI Previous Document Type";
		}

		public static class CCIMethodOfPayment
		{
			public const string RDEntityAttributeValue = "MethodOfPayment";
			public const string DataItemAttributeValue = "MethodOfPayment";
			public const string Description = "CCI Method Of Payment";
			public const string CodeType = "104IM";
			public const string OutputFileName = "RefCusCodeListZZ_EUN_104IM.xml";
			public const string OutputFileDataSource = "EUN CCI Method Of Payment";
		}

		public static class ZZRefCusCodeList
		{
			public const string NctsAdditionalInfoCode = "AI44N";
			public const string NctsAdditionalReferenceCode = "AR44N";
			public const string NctsCountryCodesCommunity = "CL010";
			public const string NctsXmlErrorCodes = "CL030";
			public const string NctsIncidentCode = "CL019";
			public const string NctsCountryCodesCTC = "CL112";
			public const string NctsCountryCustomsSecurityAgreementArea = "CL147";
			public const string NCTSReleaseTypeCode = "CL163";
			public const string NCTSReleaseNotificationCode = "CL164";
			public const string NctsPreviousDocumentUnionGoodsCode = "CL178";
			public const string NctsFunctionalErrorCodesIeCA = "CL180";
			public const string NctsNoReleaseMotivationCode = "CL211";
			public const string NctsRejectionCodeDepartureExport = "CL226";
			public const string NctsRejectionCodeDestinationExitCode = "CL227";
			public const string NctsGuaranteeTypeCTCCode = "CL229";
			public const string NctsGuaranteeTypeEUNonTIRCode = "CL230";
			public const string NctsDocumentTypeExcise = "CL234";
			public const string NctsCountryOutsideCustomsSecurityAgreementArea = "CL247";
			public const string NctsCountryCodesCommonTransit = "C0009";
			public const string NctsGuaranteeTypeCode = "CL251";
			public const string NCTSInvalidGuaranteeReasonCode = "CL252";
			public const string NctsBusinessRejectionTypeDepExpCode = "CL560";
			public const string NctsBusinessRejectionTypeDesExt = "CL570";
			public const string ExportPreviousDocumentCode = "DC40E";
			public const string NctsP5PreviousDocumentCode = "DC40M";
			public const string NctsPreviousDocumentCode = "DC40N";
			public const string NctsSupportingDocumentCode = "DC44N";
			public const string AESNationality = "EXNAT";
			public const string NctsNationalityCode = "NCNAT";
			public const string NctsDeclarationTypeCode = "NCTDT";
			public const string NctsTransportDocumentCode = "TD44N";
			public const string NctsCountryCodeType = "NC008";
			public const string NctsQueryIdentifierCodeType = "CL054";
			public const string NctsRoleOfRequesterCodeType = "CL156";
		}

		public static class ZZRefCusTradeCountry
		{
			public const string NctsCountryCustomsSecurityAgreementArea = "EUSEC";
		}

		public static class UccConstants
		{
			public const string CCIDomain = "CCI";
			public const string NCTSDomain = "NCTS-P5";
			public const string ExportExtraType = "Export";
		}

		public static class UccCodeListTypes
		{
			public const string AdditionalInformation = "AdditionalInformation";
			public const string AdditionalReference = "AdditionalReference";
			public const string BusinessRejectionTypeDepExp = "BusinessRejectionTypeDepExp";
			public const string TransportDocumentType = "TransportDocumentType";
			public const string SupportingDocumentType = "SupportingDocumentType";
			public const string PreviousDocumentType = "PreviousDocumentType";
			public const string PreviousDocumentExportType = "PreviousDocumentExportType";
			public const string DeclarationTypeType = "DeclarationType";
			public const string Nationality = "Nationality";
			public const string CountryCodesCommunity = "CountryCodesCommunity";
			public const string CountryCustomsSecurityAgreementArea = "CountryCustomsSecurityAgreementArea";
			public const string NctsCountryOutsideCustomsSecurityAgreementArea = "NCTSCountryOutsideCustomsSecurityAgreementArea";
			public const string IncidentCode = "IncidentCode";
			public const string DocumentTypeExcise = "DocumentTypeExcise";
			public const string CountryCodesCommonTransit = "CountryCodesCommonTransit";
			public const string RejectionCodeDestinationExit = "RejectionCodeDestinationExit";
			public const string FunctionalErrorCodesIeCA = "FunctionalErrorCodesIeCA";
			public const string GuaranteeTypeCTC = "GuaranteeTypeCTC";
			public const string GuaranteeTypeEUNonTIR = "GuaranteeTypeEUNonTIR";
			public const string CountryCodesCTC = "CountryCodesCTC";
			public const string PreviousDocumentUnionGoods = "PreviousDocumentUnionGoods";
			public const string ReleaseType = "ReleaseType";
			public const string ReleaseNotification = "ReleaseNotification";
			public const string InvalidGuaranteeReason = "InvalidGuaranteeReason";
			public const string GuaranteeType = "GuaranteeType";
			public const string BusinessRejectionTypeDesExt = "BusinessRejectionTypeDesExt";
			public const string NoReleaseMotivation = "NoReleaseMotivation";
			public const string RejectionCodeDepartureExport = "RejectionCodeDepartureExport";
			public const string XmlErrorCodes = "XmlErrorCodes";
			public const string CountryCodesFullList = "CountryCodesFullList";
			public const string CusCode = "CUSCode";
			public const string QueryIdentifier = "QueryIdentifier";
			public const string RoleOfRequester = "RoleRequester";
		}

		public static class UccDataSources
		{
			public const string AdditionalInformation = "EUN Additional Information";
			public const string AdditionalReference = "EUN Additional Reference";
			public const string BusinessRejectionTypeDepExp = "EUN Business Rejection Type Dep Exp";
			public const string TransportDocuments = "EUN Transport Document";
			public const string SupportingDocuments = "EUN Supporting Document";
			public const string PreviousDocumentsNcts = "EUN DC40N Previous Document";
			public const string PreviousDocumentsManifest = "EUN DC40M Previous Document";
			public const string DeclarationTypes = "EU DeclarationTypeCode";
			public const string Nationality = "EUN Nationality";
			public const string CountryCodesCommunity = "EUN Country Codes Community";
			public const string IncidentCode = "Incident Code";
			public const string DocumentTypeExcise = "Document Type Excise";
			public const string CountryCodesCommonTransit = "Country Codes Common Transit";
			public const string RejectionCodeDestinationExit = "Rejection Code Destination Exit";
			public const string FunctionalErrorCodesIeCA = "EUN Functional Error CodesIeCA";
			public const string CountryCustomsSecurityAgreementArea = "Membership of EU for the purpose of Safety and Security";
			public const string NctsCountryOutsideCustomsSecurityAgreementArea = "NCTS Country Outside Customs Security Agreement Area";
			public const string GuaranteeTypeCTC = "EUN Guarantee Type CTC";
			public const string GuaranteeTypeEUNonTIR = "EUN Guarantee Type NON TIR";
			public const string CountryCodesCTC = "EUN Country Codes CTC";
			public const string PreviousDocumentUnionGoods = "EUN Previous Document Union Goods";
			public const string ReleaseType = "EUN Release Type";
			public const string ReleaseNotification = "EUN Release Notification";
			public const string InvalidGuaranteeReason = "EUN Invalid Guarantee Reason";
			public const string GuaranteeType = "EUN Guarantee Type";
			public const string BusinessRejectionTypeDesExt = "EUN Business Rejection Type Des Ext";
			public const string NoReleaseMotivation = "EUN No Release Motivation";
			public const string RejectionCodeDepartureExport = "EUN Rejection Code Departure Export";
			public const string XmlErrorCodes = "EUN XML Error Codes";
			public const string CountryCodesFullList = "Country Codes Full List";
			public const string QueryIdentifier = "EUN Query Identifier";
			public const string RoleOfRequester = "EUN Role of Requester";
		}

		public static class AttributeNames
		{
			public const string Level = "Level";
			public const string Reference = "Reference";
			public const string Description = "Description";
			public const string ItemNumber = "ItemNumber";
			public const string Complement = "Complement";
		}

		public static class AttributeValues
		{
			public const string Header = "Header";
			public const string Item = "Item";
			public const string House = "House";
			public const string Y = "Y";
			public const string N = "N";
		}

		public static class NCTS
		{
			public const string rdentry = "RDEntry";
			public const string state = "state";
			public const string valid = "valid";
			public const string activefrom = "activeFrom";
			public const string dataitem = "dataItem";
			public const string lsdlist = "LsdList";
			public const string English = "en";
			public const string descElemName = "description";
			public const string lang = "lang";
			public const string startDateFormat = "yyyy-MM-dd";
			public const string code = "CODE";
			public const string type = "TYPE";
			public const string remark = "Remark";
		}
	}
}
