namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class Constants
	{
		public static class DataGrouping
		{
			public const string JP = "JP";
		}

		public static class CodeType
		{
			public const string BondedAreaCode = "JPBLC";
			public const string ResultCode = "NRC";
			public const string UnitOfMeasurement = "CUSUQ";
			public const string ExportApprovalCertificateType = "JPEAC";
			public const string ExportConstantApprovalCertificateType = "JPEAN";
			public const string ImportApprovalCertificateNumber = "JPIAC";
			public const string ImportConstantApprovalCertificateNumber = "JPIAN";
			public const string ImportTradeControlOrdinanceAppendix = "ITCOA";
			public const string CustomsOffices = "CUSOF";
			public const string ConsumptionTaxExemptionCodeExport = "CTEC";
			public const string ConsumptionTaxExemptionReductionCode = "CTERC";
			public const string DutyExemptionRefundCode = "DTYER";
			public const string DutyExemptionCode = "DTYE";
			public const string OtherLawCode = "OLC";
			public const string SpecialCargoCode = "SPC";
			public const string ExportTradeControlOrdinanceAppendix = "ETCOA";
			public const string MSXDocumentType = "JPDOC";
			public const string IATA = "IATA";
			public const string FSB = "JPFSB";
			public const string PORT = "PORT";
			public const string ContainerType = "CONTT";
			public const string ContainerLength = "CONTL";
			public const string ContainerHeight = "CONTH";
			public const string PackageType = "JPPKG";
		}

		public static class YesNoList
		{
			public const string Yes = "Y";
			public const string No = "N";
		}

		public static class CodeList
		{
			public const string G = "G";
			public const string R = "R";
			public const string S = "S";
			public const string N = "N";
			public const string A = "A";
			public const string J = "J";
			public const string B = "B";
			public const string P = "P";
			public const string C = "C";
			public const string T = "T";
			public const string M = "M";
			public const string WK = "WK";
			public const string GS = "GS";
		}

		public static class TariffUOMTypes
		{
			public const string CustomsUnitII = "CU2";
		}

		public static class TariffAttribute
		{
			public const string Reference = "REFERENCE";
			public const string ExportTradeOrdinance = "ETCOA";
			public const string OtherLaw = "OLC";
		}

		public static class AttributeCodeList
		{
			public const string GEN = "GEN";
			public const string WTO = "WTO";
			public const string TEM = "TEM";
			public const string GSP = "GSP";
			public const string LDC = "LDC";
			public const string EPA = "EPA";
		}

		public static class FileNames
		{
			public const string ImportTariffFileName = "JapanImportNaccsTariff.csv";
			public const string ExportTariffFileName = "JapanExportNaccsTariff.csv";
		}
	}
}
