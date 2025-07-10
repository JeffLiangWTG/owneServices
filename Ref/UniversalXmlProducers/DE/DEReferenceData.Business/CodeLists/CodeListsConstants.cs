using System.Collections.Generic;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public static class CodeListsConstants
	{
		public const string SourceDateFormatXML = "yyyy-MM-ddTHH:mm:ss";
		public const string SourceDateFormatTSV = "yyyyMMddHHmmss";
		public const string CodeListsDownloadLinksPageName = "index.uri";

		public const string OutputFileNamePrefix = "RefCusCodeListZZ_DE_";

		public static readonly HashSet<string> ValidCodeLists = new HashSet<string>()
		{
			nameof(CodeListsConstants.Import.CodeTypes.IMPORT_A2055_ECONOMIC_CONDITIONS),
			nameof(CodeListsConstants.Import.CodeTypes.IMPORT_MOP_METHOD_OF_PAYMENT),
			nameof(CodeListsConstants.Import.CodeTypes.IMPORT_TRNAT_TRANSACTION_NATURE),
			nameof(CodeListsConstants.Import.CodeTypes.IMPORT_DC44I_TARIC_CODES_AND_CERTIFICATES),
			nameof(CodeListsConstants.Import.CodeTypes.IMPORT_EUIAT_DEPARTURE_AIRPORTS),
			nameof(CodeListsConstants.Import.CodeTypes.IMPORT_CUSUQ_CUSTOMS_DECLARATION_UNITS_OF_QUANTITY),
			nameof(CodeListsConstants.Import.CodeTypes.IMPORT_C0754_TRANSPORT_DOCUMENT_TYPES),
			nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0139_PRODUCT_NUMBERS_OILS_AND_GASES),
			nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0809_COUNTRY_LIST_REIMPORT),
			nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0810_COUNTRY_LIST_SPECIFIC_SANCTIONS),
			nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0812_COUNTRY_LIST_CO_REEXPORT),
			nameof(CodeListsConstants.Export.CodeTypes.EXPORT_DC44E_DOCUMENTS),
			nameof(CodeListsConstants.Export.CodeTypes.EXPORT_CURRE_CURRENCY),
			nameof(CodeListsConstants.Export.CodeTypes.EXPORT_TD44E_TRANSPORT_DOCUMENT),
			nameof(CodeListsConstants.Export.CodeTypes.EXPORT_DC40E_PREVIOUS_DOCUMENTS),
			nameof(CodeListsConstants.Export.CodeTypes.EXPORT_AR44E_ADDITIONAL_REFERENCES),
			nameof(CodeListsConstants.Export.CodeTypes.EXPORT_AI44E_ADDITIONAL_INFORMATION),
			nameof(CodeListsConstants.Export.CodeTypes.EXPORT_AI44X_ADDITIONAL_INFORMATION),
			nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_NC008_COUNTRY_CODES_FULL_LIST),
			nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_C0009_COUNTRY_CODES_COMMON_TRANSIT),
			nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_NC010_COUNTRY_CODES_COMMUNITY),
			nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_DC44N_SUPPORTING_DOCUMENTS),
			nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_DC40N_PREVIOUS_DOCUMENTS),
			nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_TD44N_TRANSPORT_DOCUMENTS),
			nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_AI44N_ADDITIONAL_INFORMATION),
			nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_AR44N_ADDITIONAL_REFERENCES),
			nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCMS_MEMBER_STATES),
			nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCPK_PACK_TYPES),
			nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EPC_PRODUCT_CODES),
			nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCCN_CN_CODES),
			nameof(CodeListsConstants.Generic.CodeTypes.CO15_ORIGIN_COUNTRY_LIST),
			nameof(CodeListsConstants.Generic.CodeTypes.CO17_DESTINATION_COUNTRY_LIST),
			nameof(CodeListsConstants.Generic.CodeTypes.EU15_ORIGIN_COUNTRY_LIST),
			nameof(CodeListsConstants.Generic.CodeTypes.EX15_ORIGIN_COUNTRY_LIST),
			nameof(CodeListsConstants.Generic.CodeTypes.EX17_DESTINATION_COUNTRY_LIST),
			nameof(CodeListsConstants.Generic.CodeTypes.IM15_ORIGIN_COUNTRY_LIST),
			nameof(CodeListsConstants.Generic.CodeTypes.IM17_DESTINATION_COUNTRY_LIST)
		};

		public static class Generic
		{
			public static class CodeTypes
			{
				public const string CO15_ORIGIN_COUNTRY_LIST = "CO15";
				public const string CO17_DESTINATION_COUNTRY_LIST = "CO17";
				public const string EU15_ORIGIN_COUNTRY_LIST = "EU15";
				public const string EX15_ORIGIN_COUNTRY_LIST = "EX15";
				public const string EX17_DESTINATION_COUNTRY_LIST = "EX17";
				public const string IM15_ORIGIN_COUNTRY_LIST = "IM15";
				public const string IM17_DESTINATION_COUNTRY_LIST = "IM17";
			}
		}

		public static class Import
		{
			public const string CodeListsDownloadFormatXML = "xml";
			public const string CodeListsDownloadFormatTSV = "tsv";

			public static class CodeTypes
			{
				public const string IMPORT_A2055_ECONOMIC_CONDITIONS = "A2055";
				public const string IMPORT_MOP_METHOD_OF_PAYMENT = "MOP";
				public const string IMPORT_TRNAT_TRANSACTION_NATURE = "TRNAT";
				public const string IMPORT_DC44I_TARIC_CODES_AND_CERTIFICATES = "DC44I";
				public const string IMPORT_DOC44I_TARIC_CODES_AND_CERTIFICATES = "DOC44I";
				public const string IMPORT_EUIAT_DEPARTURE_AIRPORTS = "EUIAT";
				public const string IMPORT_CUSUQ_CUSTOMS_DECLARATION_UNITS_OF_QUANTITY = "CUSUQ";
				public const string IMPORT_C0754_TRANSPORT_DOCUMENT_TYPES = "C0754";
			}

			public static class CustomsCodeListIdentifiers
			{
				public const string IMPORT_A1314_EU_MEMBER_STATES = "A1314";
				public const string IMPORT_A1300_EFTA_COUNTRIES = "A1300";
				public const string IMPORT_I0300_COUNTRY_LIST = "I0300";
				public const string IMPORT_I0600_DEPARTURE_AIRPORTS = "I0600";
				public const string IMPORT_A2055_ECONOMIC_CONDITIONS = "A2055";
				public const string IMPORT_A2060_METHOD_OF_PAYMENT = "A2060";
				public const string IMPORT_A1150_TRANSACTION_NATURE = "A1150";
				public const string IMPORT_I0200_TARIC_CODES_AND_CERTIFICATES = "I0200";
				public const string IMPORT_I0255_ZELOS_DOCUMENTS = "I0255";
				public const string IMPORT_I0700_CUSTOMS_DECLARATION_UNITS_OF_QUANTITY = "I0700";
				public const string IMPORT_C0754_TRANSPORT_DOCUMENT_TYPES = "C0754";
			}
		}

		public static class Export
		{
			public const string CodeListsDownloadFormat = "xml";
			public const string CodeListsLanguageConstantValue = "DE";

			public static class CodeTypes
			{
				public const string EXPORT_I0139_PRODUCT_NUMBERS_OILS_AND_GASES = "I0139";
				public const string EXPORT_I0809_COUNTRY_LIST_REIMPORT = "I0809";
				public const string EXPORT_I0810_COUNTRY_LIST_SPECIFIC_SANCTIONS = "I0810";
				public const string EXPORT_I0812_COUNTRY_LIST_CO_REEXPORT = "I0812";
				public const string EXPORT_CURRE_CURRENCY = "CURRE";
				public const string EXPORT_DC44E_DOCUMENTS = "DC44E";
				public const string EXPORT_TD44E_TRANSPORT_DOCUMENT = "TD44E";
				public const string EXPORT_DC40E_PREVIOUS_DOCUMENTS = "DC40E";
				public const string EXPORT_AR44E_ADDITIONAL_REFERENCES = "AR44E";
				public const string EXPORT_AI44X_ADDITIONAL_INFORMATION = "AI44X";
				public const string EXPORT_AI44E_ADDITIONAL_INFORMATION = "AI44E";
			}

			public static class CustomsCodeListIdentifiers
			{
				public const string EXPORT_C0010_COUNTRY_CODES_COMMUNITY = "C0010";
				public const string EXPORT_C0207_COUNTRY_LIST_EX = "C0207";
				public const string EXPORT_C0352_CURRENCY = "C0352";
				public const string EXPORT_I0139_PRODUCT_NUMBERS_OILS_AND_GASES = "I0139";
				public const string EXPORT_I0806_COUNTRY_LIST_CO = "I0806";
				public const string EXPORT_I0809_COUNTRY_LIST_REIMPORT = "I0809";
				public const string EXPORT_I0810_COUNTRY_LIST_SPECIFIC_SANCTIONS = "I0810";
				public const string EXPORT_I0812_COUNTRY_LIST_CO_REEXPORT = "I0812";
				public const string EXPORT_I0921_DOCUMENTS = "I0921";
				public const string EXPORT_I0922_DOCUMENTS = "I0922";
				public const string EXPORT_I0931_PREVIOUS_DOCUMENTS = "I0931";
				public const string EXPORT_I0932_PREVIOUS_DOCUMENTS = "I0932";
				public const string EXPORT_I0911_ADDITIONAL_REFERENCES = "I0911";
				public const string EXPORT_I0912_ADDITIONAL_REFERENCES = "I0912";
				public const string EXPORT_I0900_ADDITIONAL_INFORMATION = "I0900";
				public const string EXPORT_I0901_ADDITIONAL_INFORMATION = "I0901";
				public const string EXPORT_I0902_ADDITIONAL_INFORMATION = "I0902";
				public const string EXPORT_I0941_TRANSPORT_DOCUMENT = "I0941";
			}
		}

		public static class Ncts
		{
			public const string CodeListsDownloadFormat = "xml";

			public static class CodeTypes
			{
				public const string NCTS_NC008_COUNTRY_CODES_FULL_LIST = "NC008";
				public const string NCTS_C0009_COUNTRY_CODES_COMMON_TRANSIT = "C0009";
				public const string NCTS_NC010_COUNTRY_CODES_COMMUNITY = "NC010";
				public const string NCTS_DC44N_SUPPORTING_DOCUMENTS = "DC44N";
				public const string NCTS_DC40N_PREVIOUS_DOCUMENTS = "DC40N";
				public const string NCTS_TD44N_TRANSPORT_DOCUMENTS = "TD44N";
				public const string NCTS_AR44N_ADDITIONAL_REFERENCES = "AR44N";
				public const string NCTS_AI44N_ADDITIONAL_INFORMATION = "AI44N";
			}

			public static class CustomsCodeListIdentifiers
			{
				public const string NCTS_C0008_COUNTRY_CODES_FULL_LIST = "C0008";
				public const string NCTS_C0009_COUNTRY_CODES_COMMON_TRANSIT = "C0009";
				public const string NCTS_C0010_COUNTRY_CODES_COMMUNITY = "C0010";
				public const string NCTS_I0923_SUPPORTING_DOCUMENTS = "I0923";
				public const string NCTS_I0925_SUPPORTING_DOCUMENTS = "I0925";
				public const string NCTS_I0926_SUPPORTING_DOCUMENTS = "I0926";
				public const string NCTS_I0933_PREVIOUS_DOCUMENTS = "I0933";
				public const string NCTS_I0935_PREVIOUS_DOCUMENTS = "I0935";
				public const string NCTS_I0936_PREVIOUS_DOCUMENTS = "I0936";
				public const string NCTS_I0943_TRANSPORT_DOCUMENTS = "I0943";
				public const string NCTS_I0945_TRANSPORT_DOCUMENTS = "I0945";
				public const string NCTS_I0913_ADDITIONAL_REFERENCES = "I0913";
				public const string NCTS_I0915_ADDITIONAL_REFERENCES = "I0915";
				public const string NCTS_I0916_ADDITIONAL_REFERENCES = "I0916";
				public const string NCTS_I0903_ADDITIONAL_INFORMATION = "I0903";
				public const string NCTS_I0905_ADDITIONAL_INFORMATION = "I0905";
				public const string NCTS_I0906_ADDITIONAL_INFORMATION = "I0906";
			}
		}

		public static class Emcs
		{
			public const string CodeListsDownloadFormat = "zip";

			public static class CodeTypes
			{
				public const string EMCS_EMCMS_MEMBER_STATES = "EMCMS";
				public const string EMCS_EMCPK_PACK_TYPES = "EMCPK";
				public const string EMCS_EPC_PRODUCT_CODES = "EPC";
				public const string EMCS_EMCCN_CN_CODES = "EMCCN";
			}

			public static class CustomsCodeListIdentifiers
			{
				public const string EMCS_CL0011_MEMBER_STATES = "CL0011";
				public const string EMCS_CL0017_PACK_TYPES = "CL0017";
				public const string EMCS_CL0036_PRODUCT_CODES = "CL0036";
				public const string EMCS_CL0037_CN_CODES = "CL0037";
			}
		}

		public static class AttributeValues
		{
			public const string Yes = "Y";
			public const string No = "N";
			public const string Import = "IMPORT";
			public const string Export = "EXPORT";
			public const string Header = "Header";
			public const string House = "House";
			public const string Item = "Item";
			public const string Zero = "0";
			public const string Required = "R";
			public const string NotAllowed = "N";
		}

		// Downloaded Codelists Structure
		public static class XMLEntryElementNames
		{
			//RefCusCodeList
			public const string AIRPORT = "Airport";
			public const string CODE = "Code";
			public const string DESCRIPTION = "Description";
			public const string DOCUMENT_CODE = "DocumentCode";
			public const string END_DATE = "EndDate";
			public const string QUALIFIER = "Qualifier";
			public const string START_DATE = "StartDate";
			//RefCusCodeList attributes
			public const string AUTHORITY = "Authority";
			public const string COMPLEMENT = "Complement";
			public const string COMPLEMENTARY_UNIT = "ComplementaryUnit";
			public const string COPY = "Copy";
			public const string DETAIL = "Detail";
			public const string DIRECTION = "Direction";
			public const string DIVISION = "Division";
			public const string OBLIGATION = "Obligation";
			public const string EXPIRY_DATE = "ExpiryDate";
			public const string ISSUING_DATE = "IssuingDate";
			public const string ITEM_NUMBER = "ItemNumber";
			public const string LEVEL = "Level";
			public const string LEVEL_ITEM = "LevelItem";
			public const string LEVEL_HEADER = "LevelHeader";
			public const string LEVEL_HOUSE = "House";
			public const string MEASUREMENT_UNIT = "MeasurementUnit";
			public const string NOMERGE = "NoMerge";
			public const string PERCENTAGE = "Percentage";
			public const string REFERENCE = "Reference";
			public const string UNIT = "Unit";
			public const string VALIDITY_DATE = "ValidityDate";
			public const string VALUE = "Value";
			public const string ZONE = "Zone";
		}

		public static class TSVColumnIndexes
		{
			public const int CODE = 0;
			public const int QUALIFIER = 1;
			public const int VALID_FROM = 2;
			public const int VALID_TO = 3;
			public const int DESCRIPTION = 4;
		}
	}
}
