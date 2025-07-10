
namespace Enterprise.Customs.US.Module
{
	public static class OrgSupplierPartFilterConstants
	{
		public const string Show = "Show";

		public static class Classificaton
		{
			public const string HTSClassification = "HTS Classification";
			public const string ScheduleBClassification = "Schedule B Classification";
		}

		public static class Tariff
		{
			public const string TariffNumber = "Tariff Number";
			public const string TariffCaption = "Tariff";
			public const string ProvTariff = "Prov. Tariff";
			public const string TariffProvTariff = "Tariff / Prov. Tariff";
			public const string MultiTariffIndicator = "Multi Tariff Indicator";
			public const string TariffType = "Tariff Type";
			public const string TariffInvalid = "Tariff Expired On?";
		}

		public static class Country
		{
			public const string CountryOfOrigin = "Country of Origin";
			public const string CountryOfExport = "Country of Export";
		}

		public static class SPI
		{
			public const string SPIIndicator = "SPI Indicator";
			public const string ProductClaim = "Product Claim/Sets";
		}

		public static class Permits
		{
			public const string ADDCaseNum = "ADD Case #";
			public const string CVDCaseNum = "CVD Case #";
			public const string CBTPACertificate = "CBTPA Certificate";
			public const string WoolLicenseNum = "Wool License #";
			public const string MiscLicenseNum = "Misc. License #";
			public const string CASugarCertificate = "CA Sugar Certificate";
			public const string AgricultureLicenseNum = "Agriculture License #";
			public const string CottonFeeExempt = "Cotton Fee Exempt";
			public const string RulingType = "Ruling Type";
			public const string RulingNum = "Ruling #";
			public const string MissingADCVInfo = "Missing AD/CV Info?";
		}

		public static class CWO
		{
			public const string ConditionCode = "CWO Condition Code";
			public const string OverrideCode = "CWO Override Code";
		}

		public static class AppliesTo
		{
			public const string Attribute1 = "Attribute 1";
			public const string Attribute2 = "Attribute 2";
			public const string Attribute3 = "Attribute 3";
		}

		public static class Export
		{
			public const string Code = "Export Code";
			public const string OriginIndicator = "Origin Indicator";
			public const string ECCN = "ECCN";
			public const string ITARExemptionNum = "ITAR Exemption #";
		}

		public static class FDA
		{
			public const string ProductCode = "FDA Product Code";
		}

		public static class Manufacturer
		{
			public const string Code = "Manufacturer";
		}

		public static class PGA
		{
			public const string LaceyActIndicator = "Lacey Act Indicator (Import)";
			public const string FDAIndicator = "PGA FDA Indicator (Import)";
			public const string NHTSAIndicator = "NHTSA Indicator (Import)";
			public const string ODSIndicator = "EPA ODS Indicator (Import)";
			public const string PSTIndicator = "EPA PST Indicator (Import)";
			public const string OMCIndicator = "OMC Indicator (Import)";
			public const string TSCAIndicator = "EPA TSCA Indicator (Import)";
			public const string AMSIndicator = "AMS Indicator (Import)";
			public const string NOPIndicator = "NOP Indicator (Import)";
			public const string VNEIndicator = "EPA VNE Indicator (Import)";
			public const string ATFIndicator = "ATF Indicator (Import)";
			public const string TTBIndicator = "TTB Indicator (Import)";
			public const string CPSCIndicator = "CPSC Indicator (Import)";
			public const string DEAIndicator = "DEA Indicator (Import)";
			public const string APHISIndicator = "APHIS Indicator (Import)";
			public const string DDTCIndicator = "DDTC Indicator (Import)";
			public const string FWSIndicator = "FWS Indicator (Import)";
			public const string NMFS370Indicator = "NMFS 370 Indicator (Import)";
			public const string NMFSCOAIndicator = "NMFS COA Indicator (Import)";
			public const string NMFSAMRIndicator = "NMFS AMR Indicator (Import)";
			public const string NMFSHMSIndicator = "NMFS HMS Indicator (Import)";
			public const string NMFSSIMPIndicator = "NMFS SIMP Indicator (Import)";
		}

		public static class ExportPGA
		{
			public const string AMSIndicator = "AMS Indicator (Export)";
			public const string ATFIndicator = "ATF Indicator (Export)";
			public const string DEAIndicator = "DEA Indicator (Export)";
			public const string EPAIndicator = "EPA Indicator (Export)";
			public const string FWSIndicator = "FWS Indicator (Export)";
			public const string NMFSIndicator = "NMFS Indicator (Export)";
			public const string TTBIndicator = "TTB Indicator (Export)";
		}
	}
}
