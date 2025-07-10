using System;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public static class Constants
	{
		public static class ProgramFunctions
		{
			public const string CodeLists = "CODELISTS";
			public const string FiscalExchangeRates = "FISCALEXCHANGERATES";
			public const string CustomsExchangeRates = "CUSTOMSEXCHANGERATES";
			public const string Tariffs = "NLTARIFFS";
			public const string ECCNCodeList = "ECCNCODELIST";
		}

		public static class DefaultValues
		{
			public const string ENLanguage = "EN";
			public const string NLDataGrouping = "NL";
			public const string NLCountryCode = "NL";
			public const string EUNCountryCode = "EUN";
			public const string TariffType = "IMP";
			public const string TradeGroup = "1011";
			public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);
			public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);
		}

		public static class AdditionalInformationDefaults
		{
			public const string ImportCodeType = "AI44I";
			public const string ExportCodeType = "AI44E";

			public static class AttributeNames
			{
				public const string Level = "Level";
				public const string Description = "Description";
			}
		}

		public static class AdditionalSupplementsDefaults
		{
			public const string CodeType = "ADDSU";

			public static class AttributeNames
			{
				public const string Direction = "Direction";
			}
		}

		public static class AttributeValues
		{
			public const string HeaderLevel = "Header";
			public const string ItemLevel = "Item";
			public const string Yes = "Y";
			public const string No = "N";
		}

		public static class ItineraryCountriesDefaults
		{
			public const string CodeType = "CL008";
		}

		public static class SupportingDocument
		{
			public const string ImportSupportingDocument = "DC44I";
			public const string ExportSupportingDocument = "DC44E";

			public static class AttributeNames
			{
				public const string Level = "Level";
				public const string Reference = "Reference";
			}
		}

		public static class TransportDocument
		{
			public const string ImportTransportDocument = "TD44I";
			public const string ExportTransportDocument = "TD44E";

			public static class AttributeNames
			{
				public const string Level = "Level";
				public const string Reference = "Reference";
			}
		}

		public static class PreviousDocument
		{
			public const string ImportCodeType = "DC40I";
			public const string ExportCodeType = "DC40E";

			public static class AttributeNames
			{
				public const string Level = "Level";
				public const string Reference = "Reference";
			}
		}

		public static class AdditionalReference
		{
			public const string ImportCodeType = "AR44I";
			public const string ExportCodeType = "AR44E";

			public static class AttributeNames
			{
				public const string Level = "Level";
				public const string Reference = "Reference";
			}
		}

		public static class FolderNames
		{
			public const string ECCNCodeListData = "ECCNCodeListData";
		}

		public static class ECCNCodeTypeDefaults
		{
			public const string CodeType = "ECCN";
			public const string LicenseType = "LicenseType";
		}
	}
}
