namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor
{
	public static class Constants
	{
		public const string ReleaseDateFormat = "yyyyMMdd";

		public static class ReferenceTypes
		{
			public const string PortCode = "PRTCODE";
			public const string FacilityCode = "LOCCODE";
			public const string CustomsProcedureCode = "CPCCODE";
			public const string CommodityCode = "CPDCODE";
		}

		public const string FacilityAttributeName = "SGCTYPE";
		public const string SegmentGroup12DateFormat = "yyyyMMddHHmmss";

		public static class TariffSegmentIdentifiers
		{
			public const string ControlType = "CTL";
			public const string TariffCode = "CD";
			public const string Description = "DES";
			public const string UnitOfMeasurement = "UOM";
		}

		public static class UpdateType
		{
			public const string Added = "ADD";
			public const string Updated = "UPD";
			public const string Deleted = "DEL";
		}

		public static class ControlTypes
		{
			public static class AttributeNames
			{
				public const string Export = "ISEXPORTCONTROL";
				public const string Import = "ISIMPORTCONTROL";
				public const string Transhipment = "ISTRANSHIPMENTCONTROL";
			}

			public const string Export = "EX";
			public const string Import = "IM";
			public const string Transhipment = "TP";

			public const string IsControlled = "Y";
		}

		public static class DeclarationTypeCodes
		{
			public const string ApprovedPremisesSchemes = "APS";
			public const string Blanket = "BKO";
			public const string BlanketIncludingBlanketGstReliefAndDutyExemption = "BKN";
			public const string BlanketIncludingBlanketGstPaymentAndDutyExemption = "BKP";
			public const string BlanketRemoval = "BRE";
			public const string Destruction = "DES";
			public const string DirectIncludingStorageInFtz = "DRT";
			public const string Duty = "DUT";
			public const string DutyAndGst = "DNG";
			public const string ForReExport = "REX";
			public const string Generic = "BKT";
			public const string GstIncludingDutyExemption = "GST";
			public const string GstReliefAndDutyExemption = "GTR";
			public const string InterGatewayMovement = "IGM";
			public const string Removal = "REM";
			public const string ShutOut = "SHO";
			public const string StorageInFtz = "SFZ";
			public const string TemporaryExportReImportedGoods = "TCI";
			public const string TemporaryImportForExhibitionAuctionsWithoutSales = "TCE";
			public const string TemporaryImportForExhibitionAuctionsWithSales = "TCS";
			public const string TemporaryImportForOtherPurposes = "TCO";
			public const string TemporaryImportForRepairs = "TCR";
			public const string ThruTranshipmentWithinSameFtz = "TTF";
			public const string ThruTranshipmentWithInterGatewayMovement = "TTI";
		}

		public static class InPayment
		{
			public const string CommonAccessReference = "1";
			public const string ShipmentCode = "IPT";

			public static class DeclarationTypeRefs
			{
				public const string GstIncludingDutyExemption = "10";
				public const string Duty = "11";
				public const string DutyAndGst = "12";
				public const string BlanketIncludingBlanketGstPaymentAndDutyExemption = "90";
			}
		}

		public static class InNonPayment
		{
			public const string CommonAccessReference = "2";
			public const string ShipmentCode = "INP";

			public static class DeclarationTypeRefs
			{
				public const string ApprovedPremisesSchemes = "20";
				public const string GstReliefAndDutyExemption = "21";
				public const string ShutOut = "22";
				public const string Destruction = "23";
				public const string ForReExport = "24";
				public const string StorageInFtz = "25";
				public const string BlanketIncludingBlanketGstReliefAndDutyExemption = "90";
				public const string TemporaryImportForExhibitionAuctionsWithSales = "91";
				public const string TemporaryImportForRepairs = "92";
				public const string TemporaryImportForExhibitionAuctionsWithoutSales = "93";
				public const string TemporaryImportForOtherPurposes = "94";
				public const string TemporaryExportReImportedGoods = "95";
			}
		}

		public static class Out
		{
			public static class CommonAccessReference
			{
				public const string WithCertificateOfOrigin = "5";
				public const string WithoutCertificateOfOrigin = "4";
			}

			public const string ShipmentCode = "OUT";

			public static class DeclarationTypeRefs
			{
				public const string ApprovedPremisesSchemes = "20";
				public const string DirectIncludingStorageInFtz = "40";
				public const string Blanket = "90";
				public const string TemporaryImportForExhibitionAuctionsWithSales = "91";
				public const string TemporaryImportForRepairs = "92";
				public const string TemporaryImportForExhibitionAuctionsWithoutSales = "93";
				public const string TemporaryImportForOtherPurposes = "94";
				public const string TemporaryExportReImportedGoods = "95";
			}
		}

		public static class TranshipmentMovement
		{
			public const string CommonAccessReference = "7";
			public const string ShipmentCode = "TNP";

			public static class DeclarationTypeRefs
			{
				public const string ThruTranshipmentWithInterGatewayMovement = "70";
				public const string ThruTranshipmentWithinSameFtz = "71";
				public const string InterGatewayMovement = "72";
				public const string Removal = "80";
				public const string BlanketRemoval = "90";
			}
		}

		public static class RefCusCodeListTypes
		{
			public const string Port = "PORT";
			public const string Facility = "FAC";
		}

		public static class RefCusTariffTypes
		{
			public const string Commodity = "COM";
			public const string HarmonizedCode = "HSN";
		}

		public static class ProcedureConcessionTypes
		{
			public const string AEO = "1000";
			public const string SEASTORE = "3000";
		}

		public static class ProcedureAttributeNames
		{
			public const string ISCOO = "ISCOO";
			public const string ISAEO = "ISAEO";
			public const string ISSEASTORE = "ISSEASTORE";

			public const string PC1 = "PC1";
			public const string PC2 = "PC2";
		}

		public static class ProcedureAttributeValues
		{
			public const string ISCOO = "This CPC is a Certificate of Origin CPC";
			public const string ISAEO = "This CPC is an AEO CPC";
			public const string ISSEASTORE = "This CPC is a SEASTORE CPC";

			public const string AEOPC1 = "Enter the AEO number's country of issue";
			public const string AEOPC2 = "Enter the AEO number";

			public const string SEASTOREPC1 = "The number of crew members must be entered as a number, (e.g. 15)";
			public const string SEASTOREPC2 = "The voyage duration in days must be entered as a number, (e.g. 8)";
		}
	}
}
