using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public static class Constants
	{
		public static class ProgramFunctions
		{
			public const string ExchangeRates = "EXCHANGERATES";
			public const string AdditionalInfo = "ADDITIONALINFO";
			public const string LocationCodes = "LOCATIONCODES";
			public const string ImportTariffs = "IMPORTTARIFFS";
			public const string NctsCodeLists = "NCTSCODELISTS";
			public const string EUCodeLists = "EUCODELISTS";
			public const string TariffData = "TARIFFDATA";
			public const string ImportCodeLists = "IMPCODELISTS";
		}

		public static class Common
		{
			public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);

			public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

			public const string LocalCountryCode = "BE";

			public const string EUNCountryCode = "EUN";
		}

		public static class RefExchangeRate
		{
			public const string CUSRateType = "CUS";
		}

		public static class NctsCodeListNames
		{
			public const string AdditionalReferenceCodes = "CL380";
			public const string AdditionalInformationCodes = "CL239";
			public const string SupportingDocumentCodes = "CL213";
			public const string PreviousDocumentCodesCL214 = "CL214";
			public const string PreviousDocumentCodesCL228 = "CL228";
			public const string TransportDocumentCodes = "CL754";
		}

		public static class UccCodeListNames
		{
			public const string AdditionalReferenceCodes = "CL380";
			public const string AdditionalInformationCodes = "CL239";
			public const string SupportingDocumentCodes = "CL213";
			public const string PreviousDocumentCodesCL214 = "CL214";
			public const string PreviousDocumentCodesCL228 = "CL228";
			public const string TransportDocumentCodes = "CL754";
		}

		public static class ZZRefCusCodeList
		{
			public const string AdditionalCode = "ADDCD";
			public const string AdditionalInfo = "ADDIN";
			public const string SupportingDocuments = "ADDDC";
			public const string MeasurementUnit = "CUSUQ";
			public const string NctsAdditionalCode = "AR44N";
			public static DateTime MinimumDateTime => new DateTime(2017, 01, 01, 00, 00, 00);
			public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);
			public const string AdditionalInformationImport = "AI44I";
			public const string NctsAdditionalInfoCode = "AI44N";
			public const string NctsSupportingDocumentCode = "DC44N";
			public const string NctsPreviousDocumentCode = "DC40N";
			public const string NctsPreviousDocumentExportCode = "DC40N_Export";
			public const string NctsTransportDocumentCode = "TD44N";
			public const string UccTransportDocumentType = "TD44E";
			public const string UccAdditionalInformation = "AI44E";
			public const string UccPreviousDocumentType = "DC40E";
			public const string UccAdditionalReference = "AR44E";
			public const string UccSupportingDocumentType = "DC44E";
			public const string UccImportTransportDocumentType = "TD44I";
			public const string UccImportAdditionalInformation = "AI44I";
			public const string UccImportPreviousDocumentType = "DC40I";
			public const string UccImportAdditionalReference = "AR44I";
			public const string UccImportSupportingDocumentType = "DC44I";
		}
		public static class ZZRefCusCondition
		{
			public static DateTime MinimumDateTime => new DateTime(2017, 01, 01, 00, 00, 00);

			public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);
		}

		public static class Languages
		{
			public const string DefaultLanguage = "NL";
			public const string German = "DE";
			public const string English = "EN";
			public const string French = "FR";
			public const string Dutch = "NL";
		}

		public class IntermediateCodeListData
		{
			public string Code { get; set; }
			public DateTime StartDate { get; set; }
			public Dictionary<string, string> MultilingualDescriptions { get; set; }
		}

		public static class UccConstants
		{
			public const string ExportDomain = "AES";
			public const string ImportDomain = "Import";
			public const string NCTSDomain = "NCTS-P5";
			public const string ExportExtraType = "Export";
		}

		public static class UccCodeListTypes
		{
			public const string TransportDocumentType = "TransportDocumentType";
			public const string AdditionalInformation = "AdditionalInformation";
			public const string PreviousDocumentType = "PreviousDocumentType";
			public const string PreviousDocumentExportType = "PreviousDocumentExportType";
			public const string AdditionalReference = "AdditionalReference";
			public const string SupportingDocumentType = "SupportingDocumentType";
		}

		public static class DataSources
		{
			public const string BeAdditionalInformation = "BE Additional Information";
			public const string BeAdditionalReference = "BE Additional Reference";
			public const string BeTransportDocuments = "BE Transport Documents";
			public const string BePreviousDocuments = "BE Previous Documents";
			public const string BeSupportingDocuments = "BE Supporting Documents";
			public const string BeMeasureUnits = "BE Measure Units";
			public const string BeGeographicalAreas = "BE Trade Groups";
			public const string BeAdditionalCodes = "BE Additional Codes";
			public const string BeMeasures = "BE Tariffs";

		}

		public static class XmlTags
		{
			public const string TariffHistoryItem = "TariffHistoryItem";
			public const string GeographicalArea = "GeographicalArea";
			public const string AdditionalCode = "AdditionalCode";
			public const string MeasurementUnit = "MeasurementUnit";
			public const string Measure = "Measure";
			public const string DefaultLanguage = "EN";
			public const string NationalCodes = "N";
			public const string hjid = "hjid";
			public const string MetaInfo = "metainfo";
			public const string Origin = "origin";
			public const string Status = "status";
			public const string TransactionDate = "transactionDate";
			public const string Description = "description";
			public const string Language = "language";
			public const string LanguageId = "languageId";
			public const string ValidityStartDate = "validityStartDate";
			public const string ValidityEndDate = "validityEndDate";

			public static class MeasurementUnitTags
			{
				public const string Root = "findMeasurementUnitByDatesResponse";
				public const string MeasurementUnit = "MeasurementUnit";
				public const string MeasurementUnitCode = "measurementUnitCode";
				public const string MeasurementUnitDescription = "measurementUnitDescription";
				public const string MeasurementUnitQualifier = "measurementUnitQualifier";
				public const string MeasurementUnitQualifierCode = "measurementUnitQualifierCode";
				public const string OpType = "opType";
			}

			public static class GeographicalAreaTags
			{
				public const string Root = "findGeographicalAreaByDatesResponse";
				public const string AreaId = "geographicalAreaId";
				public const string AreaDescriptionPeriod = "geographicalAreaDescriptionPeriod";
				public const string AreaDescription = "geographicalAreaDescription";
				public const string Membership = "geographicalAreaMembership";
				public const string GroupId = "geographicalAreaGroupSid";
			}

			public static class AdditionalCodes
			{
				public const string AdditionalCode = "AdditionalCode";
				public const string AdditionalCodeCode = "additionalCodeCode";
				public const string AdditionalCodeDescription = "additionalCodeDescription";
				public const string AdditionalCodeDescriptionPeriod = "additionalCodeDescriptionPeriod";
				public const string AdditionalCodeType = "additionalCodeType";
				public const string AdditionalCodeTypeId = "additionalCodeTypeId";
				public const string FindAdditionalCodeByDatesResponse = "findAdditionalCodeByDatesResponse";
				public const string Language = "language";
			}

			public static class MeasureTags
			{
				public const string Root = "findMeasureByDatesResponseHistory";
				public const string Measure = "Measure";
				public const string GoodsNomenclature = "goodsNomenclature";
				public const string GoodsnomenclatureId = "goodsNomenclatureItemId";
				public const string MeasureType = "measureType";
				public const string MeasureTypeId = "measureTypeId";
				public const string MeasureComponent = "measureComponent";
				public const string DutyAmount = "dutyAmount";
				public const string MeasureConditionComponent = "measureConditionComponent";
				public const string MeasureCondition = "measureCondition";
				public const string Certificate = "certificate";
				public const string ValidityStartDate = "validityStartDate";
				public const string ValidityEndDate = "validityEndDate";
				public const string MeasureExcludedGeographicalArea = "measureExcludedGeographicalArea";
				public const string MeasureExcludedGeographicalAreaId = "geographicalAreaId";
				public const string CertificateTypeCode = "certificateTypeCode";
				public const string MeasureConditionCode = "measureConditionCode";
			}

			public static class MeasureTypeTags
			{
				public const string Root = "findMeasureTypeByDatesResponse";
				public const string MeasureType = "MeasureType";
				public const string TradeMovementCode = "tradeMovementCode";
			}

			public static class MeasureConditionCodeTags
			{
				public const string Root = "findMeasureConditionCodeByDatesResponse";
				public const string MeasureConditionCode = "MeasureConditionCode";
				public const string ConditionCode = "conditionCode";
				public const string MeasureConditionCodeDescription = "measureConditionCodeDescription";
				public const string LanguageId = "languageId";
			}

			public static class CertificateTypeTags
			{
				public const string Root = "findCertificateTypeByDatesResponse";
				public const string CertificateType = "CertificateType";
				public const string CertificateCode = "certificateCode";
				public const string CertificateTypeCode = "certificateTypeCode";
			}
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

		public static class TariffTypes
		{
			public const string Import = "IMP";
			public const string Export = "EXP";
		}

		public static class AppSettingsKeys
		{
			public const string DownloadDir = "DownloadDir";

			public const string ServiceDir = "ServiceDir";

			public const string AppSettings = "appsettings";
		}

		public static class FolderNames
		{
			public const string CodeListData = "CodeListData";

			public const string UCCCodeListData = "UCCCodeListData";
		}
	}
}
