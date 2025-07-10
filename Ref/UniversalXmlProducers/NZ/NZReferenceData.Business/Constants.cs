using System;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public static class Constants
	{
		public const string DataGrouping = "NZ";

		public const char SupplierSplitChar = '~';

		public static class ProgramFunctions
		{
			public const string NZVessel = "NZVESSEL";
			public const string NZSupplier = "NZSUPPLIER";
			public const string NZTariff = "NZTARIFF";
			public const string NZConcession = "NZCONCESSION";
		}

		public static class CodeType
		{
			public const string Vessel = "NZFAV";
			public const string Supplier = "SUPCD";
		}

		public static class AttributeName
		{
			public const string Country = "Country";
		}

		public static class TariffTypes
		{
			public const string HSN = "HSN";
			public const string CON = "CON";
		}

		public static class DataGroupingCodes
		{
			public const string NewZealand = "NZ";
		}

		public static class ApplicabilityAdditionalCodes
		{
			public const string IsManual = "IsManual";
		}

		public static class TaxOrFeeCodes
		{
			public const string GST = "GST";
		}

		public static class TariffRateCodes
		{
			public const string DTY = "DTY";
			public const string LVY = "LVY";
		}

		public static class TariffLevyTypes
		{
			public const string AC = "AC";
			public const string AL = "AL";
			public const string GG = "GG";
			public const string PF = "PF";
			public const string SL = "SL";
		}

		public static class TariffRatePreferences
		{
			public const string Q = "Q";
		}

		public static class TariffTradeGroups
		{
			public const string NML = "NML";
			public const string RCEP = "RCEP";
			public const string RCE = "RCE";
		}

		public static class TariffUOMTypes
		{
			public const string CU1 = "CU1";
			public const string CU2 = "CU2";
		}

		public static class TariffUOMs
		{
			public const string NMB = "NMB";
		}

		public const char TariffSplit = '~';

		public static DateTime DefaultStartDateTime => new DateTime(2010, 01, 01);

		public static DateTime DefaultEndDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

		public static DateTime MinSmallDateTime => new DateTime(1900, 01, 01);

		public static DateTime MaxSmallDateTime => new DateTime(2079, 06, 06, 23, 59, 00);
	}
}
