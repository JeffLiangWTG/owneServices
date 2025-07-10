using System;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public static class Constants
	{
		public const string DataGrouping = "AU";
		public const string DataGroupingTest = "AUT";

		public static class ChangeReportActions
		{
			public const string Delete = "D";
			public const string Insert = "I";
			public const string Modify = "M";
		}

		public static class ProgramFunctions
		{
			public const string AHECC = "AHECC";
			public const string AHECCPartial = "AHECCPARTIAL";
			public const string CMRRefDataFull = "CMRREFDATAFULL";
			public const string CMRRefDataPartial = "CMRREFDATAPARTIAL";
			public const string CMRRefTestData = "CMRREFTESTDATA";
			public const string CustomsTariff = "TARIFF";
			public const string ExchangeRate = "EXCHANGERATE";
			public const string LCTThresholds = "LCTTHRESHOLDS";
			public const string NexDocs = "NEXDOCS";
			public const string Nomenclature = "NOMENCLATURE";
		}

		public static class RefData_Common
		{
			public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);

			public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);
		}

		public static class TariffAttributes
		{
			public const string CMRCharacteristicCode = "CMRCharacteristicCode";
		}

		public static class TariffTypes
		{
			public const string IMP = "IMP";
			public const string EXP = "EXP";
		}

		public static class TariffUOMTypes
		{
			public const string CU1 = "CU1";
			public const string CU2 = "CU2";
			public const string ERR = "ERR";
			public const string NR = "NR";
		}

		public static class TaxOrFeeCodes
		{
			public const string GST = "GST";
		}
	}
}
