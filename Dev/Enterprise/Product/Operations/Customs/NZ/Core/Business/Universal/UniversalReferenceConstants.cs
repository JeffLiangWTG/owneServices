using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business
{
	public static class UniversalReferenceConstants
	{
		public static readonly ZDateTime UNPackTypeStartDate = new ZDateTime(2012, 1, 1);

		public static class TaxOrFeeCodes
		{
			public const string InwardCargoTransactionFeeSea = "ICS";
			public const string InwardCargoTransactionFeeAir = "ICA";
			public const string OutwardCargoTransactionFeeSea = "OCS";
			public const string OutwardCargoTransactionFeeAir = "OCA";
			public const string OutwardReportTransactionFeeSea = "ORS";
			public const string OutwardReportTransactionFeeAir = "ORA";
			public const string ImportEntryTransactionFee = "IET";
			public const string BiosecuritySystemEntryLevy = "BSL";
			public const string ExportEntryTransactionFeeSecureExportPartners = "ESP";
			public const string ExportEntryTransactionFeeNonSecureExportPartners = "ENP";
			public const string LowValue = "LVT";
			public const string ExportValueThreshold = "EVT";
			public const string GoodsAndServicesTax = "GST";
		}

		public static class TariffCodes
		{
			public const string Qualifies = "Q";
			public const string NotQualifies = "N";
		}

		public static class RefCusCodeListAttributeTypes
		{
			public const string TSWCodeListValueRequired = "TSWCodeListValueRequired";
		}

		public static class RefCusCodeListAttributeValues
		{
			public const string No = "N";
			public const string Yes = "Y";
		}

		public static class PackageTypeListCodes
		{
			public const string Package = "PK";
		}
	}
}
