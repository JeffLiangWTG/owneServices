namespace Enterprise.Customs.Business
{
	public static class UniversalReferenceConstants
	{
		public const string FlatRate = "FLAT";
		public const string NilCountrySpecificKey = "NIHIL";
		public const string ValueForDuty = "VFD";

		public static class RefCusCodeListAttributes
		{
			public static class Name
			{
				public const string AsycudaXML = "AsycudaXML";
			}

			public static class Value
			{
				public const string Yes = "Y";
			}
		}

		public static class RefCusRateTypes
		{
			public static string AntiDumpingDuty => Universal.Constants.RateTypes.AntiDumping;
			public static string ExportTaxes => Universal.Constants.RateTypes.ExportTaxes;
			public const string CountervailingDuty = "CVD";
			public const string Duty = "DUT";
			public const string Interest = "INT";
			public const string Levies = "LEV";
			public const string SecurityDeposit = "SEC";
			public const string Miscellaneous = "MSC";
			public const string MiscellaneousOnlyForExport = "MOE";
			public static string Excise => Universal.Constants.RateTypes.Excise;
			public static string Dty => Universal.Constants.RateTypes.Duty;
			public const string Vat = "VAT";
		}

		public static class CusTariffTypes
		{
			public const string ImportTariff = "IMP";
			public const string ExportTariff = "EXP";
			public const string MeursingTariff = "MEU";
		}

		public static class MethodOfCalculation
		{
			public const string Percentage = "%";
		}

		public static class RefCusCodeList
		{
			public static class CustomsUq
			{
				public static class Weight
				{
					public const string Kilogram = "KGM";
					public const string Gram = "GRM";
					public const string Hectokilogram = "DTN";
					public const string Tonne = "TNE";
				}

				public static class Volume
				{
					public const string Litre = "LTR";
					public const string Hectolitre = "HLT";
					public const string Kilolitre = "KLT";
				}

				public static class Alcohol
				{
					public const string LitrePureAlcohol = "LPA";
					public const string PercentageVolumeHectolitre = "ASVX"; // It's not "percentage volume per hectolitre", that would have dimensions of "dm^-3"; rather it's "percentage volume hectolites" (i.e.t he product), thus the dimenion is "dm^3" (1dm =1 decimetre = 10cm; so 1dm^3 is one litre)
				}

				public static class Number
				{
					public const string NumberOfItems = "NAR";
					public const string NumberOfItemsPerFlask = "NARB";
					public const string NumberOfCells = "NCL";
					public const string NumberOfPairs = "NPR";
				}
			}
		}

		public static class PackageUnitAttributes
		{
			public const string Bulk = "BULK";
			public const string BreakBulk = "BREAKBULK";
			public const string Vehicle = "VEHICLE";
		}
	}
}
