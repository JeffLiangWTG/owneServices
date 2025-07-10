namespace Enterprise.Customs.ZA.Business
{
	public static class UniversalReferenceConstants
	{
		public static class TariffAttributes
		{
			public const string CheckDigit = "CheckDigit";
			public const string VIN = "VIN";
			public const string IntellectualValue = "INTELLECTUAL";
			public const string CostOfRepair = "CostOfRepair";
			public const string RebateSequence = "REBATESEQUENCE";
			public const string Diamond = "Diamond";
			public const string ImportPermit = "ImportPermit";
			public const string ExportPermit = "ExportPermit";
			public const string PRCC = "PRCC";
			public const string SpecifiedMotorVehicle = "SpecifiedMotorVehicle";

			public static class Values
			{
				public const string Mandatory = "MANDATORY";
				public const string Optional = "OPTIONAL";
			}
		}

		public static class RefCusCodeListAttributes
		{
			public static class Values
			{
				public const string Containerised = "Containerised";
				public const string NotAllowSpace = "N";
			}
		}

		public static class RefCusProcedureGroup
		{
			public const string BLNS = "BLNS";
			public const string Excise = "EXCISE";
		}

		public static class RefCusTariffUOMTypes
		{
			public const string StatisticalUOMType = "CU1";
			public const string AdditionalUOMType = "CU2";
			public const string ClassificationUOMType = "RU1";
		}

		public static class TradeAgreement
		{
			public const string Standard = "STANDARD";
			public const string EUTRADE = "EUTRADE";
			public const string EFTA = "EFTA";
		}

		public static class PrimaryPreference
		{
			public const string Standard = "100";
			public const string PreferentialRate = "200";
			public const string PreferentialQuota = "400";
		}

		public static class ConcessionOrder
		{
			public const string Quota = "QUOTA";
		}

		public static class Schedule
		{
			public const string _1 = "1";
			public const string _3 = "3";
			public const string _4 = "4";
			public const string _5 = "5";
			public const string _6 = "6";
		}

		//Don't change this class, speak to Brendon first
		public static class CusTariffCode
		{
			public const string Schedule1Part1 = "1P1";
			public const string Schedule1Part2A = "12A";
			public const string Schedule1Part3D = "13D";
			public const string Schedule1Part8 = "1P8";
		}

		public static class CustomsStatus
		{
			public const string SupportingDocsRequired = "13";
			public const string NoticeToUploadSupportingInformation = "14";
			public const string CaseClosed = "34";
		}

		public static class TaxOrFeeTypeCode
		{
			public const string VAT = "VAT";
			public const string VEX = "VEX";
		}

		public static class AdditionalInformation
		{
			public const string ApprovedExporter = "APE";
			public const string BondHolder = "BHR";
			public const string BondSuretyAmount = "BND";
			public const string DiamondBeneficiaryLicense = "DBL";
			public const string DiamondDealerLicense = "DDL";
			public const string DiamondLevyValue = "DLV";
			public const string DiamondProducerExemption = "DPX";
			public const string DiamondProducerRegistration = "DPR";
			public const string ElectionsExemptionsLevy = "ELX";
			public const string ExportPermitControl = "EPC";
			public const string ImportPermitControl = "IPC";
			public const string KimberleyCertificate = "KBC";
			public const string NewUsedIndicator = "NUI";
			public const string OrdinaryLevyItem = "OLI";
			public const string RebateCreditCertificate = "RCC";
			public const string RebateCreditValue = "RCV";
			public const string RulesOfOrigin = "ROO";
			public const string TemporaryBuyersPermit = "TBP";
			public const string TemporaryExportExemption = "DDX";
			public const string VATTaxExemptions = "VTE";
			public const string ValueDeterminationNumber = "VDN";
			public const string VehicleIdentificationNumber = "VIN";
			public const string SafeguardDutyItem = "SGI";
			public const string CountervailingDutyItem = "CVI";
			public const string ProvisionalPaymentSurety = "PPS";
			public const string AgentRepresentingAForeignImporterOrExporter = "AFT";
			public const string AgentRepresentingAForeignHaulier = "AFH";
			public const string DutyCreditValue = "DCV";
			public const string ProductionRebateCertificate = "PRC";
			public const string ProductionRebateValue = "PRV";
			public const string AdvancePaymentNo = "APN";
		}

		public static class TariffHeading
		{
			public const string OrdinaryLevyItem = "8703";
		}

		public static class TransportMode
		{
			public const string Unknown = "0";
		}

		public static class OrdinaryLevyItem
		{
			public const string Code_19610 = "19610";
			public const string Code_19620 = "19620";
		}

		public static class ProcedureCodes
		{
			public const string _00 = "00";
			public const string _10 = "10";
			public const string _11 = "11";
			public const string _12 = "12";
			public const string _13 = "13";
			public const string _14 = "14";
			public const string _15 = "15";
			public const string _20 = "20";
			public const string _21 = "21";
			public const string _22 = "22";
			public const string _30 = "30";
			public const string _35 = "35";
			public const string _36 = "36";
			public const string _37 = "37";
			public const string _38 = "38";
			public const string _40 = "40";
			public const string _41 = "41";
			public const string _42 = "42";
			public const string _43 = "43";
			public const string _44 = "44";
			public const string _45 = "45";
			public const string _46 = "46";
			public const string _47 = "47";
			public const string _48 = "48";
			public const string _49 = "49";
			public const string _51 = "51";
			public const string _52 = "52";
			public const string _53 = "53";
			public const string _60 = "60";
			public const string _61 = "61";
			public const string _62 = "62";
			public const string _63 = "63";
			public const string _64 = "64";
			public const string _65 = "65";
			public const string _66 = "66";
			public const string _67 = "67";
			public const string _68 = "68";
			public const string _75 = "75";
			public const string _76 = "76";
			public const string _77 = "77";
			public const string _78 = "78";
			public const string _80 = "80";
			public const string _81 = "81";
			public const string _82 = "82";
			public const string _83 = "83";
			public const string _85 = "85";
			public const string _86 = "86";
			public const string _87 = "87";
			public const string _88 = "88";
			public const string _90 = "90";
			public const string _91 = "91";
		}

		public static class ProcedureCategoryCodes
		{
			public const string _A = "A";
			public const string _B = "B";
			public const string _C = "C";
			public const string _D = "D";
			public const string _E = "E";
			public const string _F = "F";
			public const string _H = "H";
			public const string _I = "I";
			public const string _J = "J";
			public const string _K = "K";
			public const string _L = "L";
		}

		public static class CusEntryPayTypes
		{
			public const string Duty = "DTY";
			public const string AllDuties = "ALLDTY";
			public const string ValueAddedTax = "VAT";
			public const string Pending = "PEN";
		}

		public static class RefCusTariffTypes
		{
			public const string StartsWith3 = "3";
			public const string StartsWith4 = "4";
		}
	}
}
