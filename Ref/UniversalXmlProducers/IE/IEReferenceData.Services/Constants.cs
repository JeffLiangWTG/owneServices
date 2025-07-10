using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public static class Constants
	{
		public const string IECountryCode = "IE";

		public static class DataGroupings
		{
			public const string IEUCC5 = "IE5";
		}

		public static class ProgramFunctions
		{
			public const string ExchangeRates = "EXCHANGERATES";
			public const string CodeLists = "CODELISTS";
			public const string ROSErrorsList = "ROSERRORSLIST";
			public const string ExciseTaxes = "EXCISETAXES";
		}

		public static class CodeListConstants
		{
			public static readonly HashSet<string> ValidCodeLists = new HashSet<string>()
			{
				AESCodeTypes.AdditionalInformationType,
				AESCodeTypes.AdditionalReferenceType,
				AESCodeTypes.BusinessRejectionType,
				AESCodeTypes.CalculationOfTaxesType,
				AESCodeTypes.CountryCodesCommonTransitOutsideCommunity,
				AESCodeTypes.CountryCodesCountryRegimeOTH,
				AESCodeTypes.DestinationCountry,
				AESCodeTypes.DiversionRejectionCodeType,
				AESCodeTypes.ExitControlResultCode,
				AESCodeTypes.FunctionalErrorCode,
				AESCodeTypes.MeasurementUnitAndQualifierType,
				AESCodeTypes.NotificationType,
				AESCodeTypes.PreviousDocumentType,
				AESCodeTypes.RejectionReasonType,
				AESCodeTypes.SpecificCircumstanceIndicatorType,
				AESCodeTypes.SupportingDocumentType,
				AESCodeTypes.TransportChargesType,
				AESCodeTypes.TransportDocumentType,
				AESCodeTypes.TypeOfAlternativeEvidenceType,
				AISCodeTypes.AdditionalDeclarationType,
				AISCodeTypes.AdditionalInformationType,
				AISCodeTypes.AdditionalReferenceType,
				AISCodeTypes.AuthorisationCodeType,
				AISCodeTypes.ErrorType,
				AISCodeTypes.KindOfPackages,
				AISCodeTypes.GoodsLocation,
				AISCodeTypes.LegalBasisCode,
				AISCodeTypes.LocationIdentificationQualifier,
				AISCodeTypes.LocationType,
				AISCodeTypes.NatureTransaction,
				AISCodeTypes.PreviousDocumentType,
				AISCodeTypes.SupportingDocumentType,
				AISCodeTypes.TransportDocumentType,
				AISCodeTypes.CountryCode,
				CommonCodeTypes.AdditionalProcedure,
				CommonCodeTypes.TypeOfControls,
				CommonCodeTypes.UnitsOfMeasurement,
				NctsCodeTypes.AdditionalInformationType,
				NctsCodeTypes.AdditionalReferenceType,
				NctsCodeTypes.AuthorisationType,
				NctsCodeTypes.CountryCodeType,
				NctsCodeTypes.DeclarationTypeAdditionalType,
				NctsCodeTypes.DocumentTypeType,
				NctsCodeTypes.GuaranteeType,
				NctsCodeTypes.IncidentCodeType,
				NctsCodeTypes.PreviousDocumentType,
				NctsCodeTypes.QualifierOfIdentificationIncidentType,
				NctsCodeTypes.QueryIdentifierType,
				NctsCodeTypes.RejectionCodeDepartureExportType,
				NctsCodeTypes.RejectionCodeDestinationExitType,
				NctsCodeTypes.SpecificCircumstanceIndicatorCodeType,
				NctsCodeTypes.SupportingDocumentType,
				NctsCodeTypes.TransportChargesMethodOfPaymentType,
				NctsCodeTypes.UNDangerousGoodsCodeType,
				NctsCodeTypes.XmlErrorCodesCodeType,
				// NctsCodeTypes.PreviousDocumentExciseType, // This is currently not part of the NCTS code lists document, but is expected to be added soon
			};

			public static readonly HashSet<string> UnpublishedCodeLists = new HashSet<string>()
			{
				AESCodeTypes.CountryCodesCommonTransitOutsideCommunity,
				AESCodeTypes.CountryCodesCountryRegimeOTH,
				AESCodeTypes.BusinessRejectionType
			};
		}

		public static class CodeListAttributes
		{
			public const string AdditionalInfo = "AdditionalInfo";
			public const string AdditionalReference = "AdditionalReference";
			public const string TransportDocument = "TransportDocument";
			public const string Yes = "Y";
			public const string Level = "Level";
			public const string House = "House";
			public const string Header = "Header";
			public const string Item = "Item";
		}

		public static class CommonCodeTypes
		{
			public const string AdditionalProcedure = "CPDC";
			public const string TypeOfControls = "CL716";
			public const string UnitsOfMeasurement = "CUSUQ";
		}

		public static class AESCodeTypes
		{
			public const string AdditionalInformationType = "AI44E";
			public const string AdditionalReferenceType = "AR44E";
			public const string BusinessRejectionType = "CL570";
			public const string CalculationOfTaxesType = "MOP";
			public const string CountryCodesCommonTransitOutsideCommunity = "CL063";
			public const string CountryCodesCountryRegimeOTH = "CL140";
			public const string DestinationCountry = "CL008";
			public const string DiversionRejectionCodeType = "CL046";
			public const string ExitControlResultCode = "CL393";
			public const string FunctionalErrorCode = "CL180";
			public const string MeasurementUnitAndQualifierType = "SUPUQ";
			public const string NotificationType = "CL384";
			public const string PreviousDocumentType = "DC40E";
			public const string RejectionReasonType = "CL560";
			public const string ROSErrorListType = "IEROS";
			public const string SpecificCircumstanceIndicatorType = "CL296";
			public const string SupportingDocumentType = "DC44E";
			public const string TransportChargesType = "TCMOP";
			public const string TransportDocumentType = "TD44E";
			public const string TypeOfAlternativeEvidenceType = "CL170";
		}

		public static class AISCodeTypes
		{
			public const string AdditionalDeclarationType = "ENSUB";
			public const string AdditionalFiscalRefRoleCode = "AFRC";
			public const string AdditionalReferenceType = "AR44I";
			public const string AdditionalInformationType = "AI44I";
			public const string AuthorisationCodeType = "AUTH";
			public const string ErrorType = "ERRCD";
			public const string GoodsLocation = "FAC";
			public const string KindOfPackages = "PKG";
			public const string LegalBasisCode = "LBC";
			public const string LocationIdentificationQualifier = "QUA";
			public const string LocationType = "LOC";
			public const string NatureTransaction = "TRNAT";
			public const string PreviousDocumentType = "DC40I";
			public const string SupportingDocumentType = "DC44I";
			public const string TARICNationalAdditionalCode = "TARIC";
			public const string TransportDocumentType = "TD44I";
			public const string CountryCode = "AI008";
			public const string DutyAndTaxType = "DUTX";
		}

		public static class NctsCodeTypes
		{
			public const string AdditionalInformationType = "AI44N";
			public const string AdditionalReferenceType = "AR44N";
			public const string AuthorisationType = "AUTHN";
			public const string CountryCodeType = "NC008";
			public const string DeclarationTypeAdditionalType = "NDECA";
			public const string DocumentTypeType = "CL215";
			public const string GuaranteeType = "GUART";
			public const string IncidentCodeType = "CL019";
			public const string PreviousDocumentExciseType = "CL234";
			public const string PreviousDocumentType = "DC40N";
			public const string QualifierOfIdentificationIncidentType = "CL038";
			public const string QueryIdentifierType = "NGUAQ";
			public const string RejectionCodeDepartureExportType = "CL226";
			public const string RejectionCodeDestinationExitType = "CL227";
			public const string SpecificCircumstanceIndicatorCodeType = "296N";
			public const string SupportingDocumentType = "DC44N";
			public const string TransportChargesMethodOfPaymentType = "NMOP";
			public const string UNDangerousGoodsCodeType = "NDGC";
			public const string XmlErrorCodesCodeType = "CL030";
		}

		public static class ExciseDutyRate
		{
			public const string AlcoholProductsTax = "Alcohol Products Tax";
			public const string MineralOilTax = "Mineral Oil Ta";
			public const string TobaccoProductsTax = "Tobacco Products Tax";
		}

		public static class NCTSPreviousDocumentsCodeLists
		{
			public const string GoodsDeclarationForExportationCode = "N830";
		}

		public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);

		public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);
	}
}
