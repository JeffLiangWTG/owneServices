namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public static class Constants
	{
		public const string CNCountryCode = "CN";
		public const int BitTrue = 1;
		public const int BitFalse = 0;

		public static class ProgramFunctions
		{
			public const string CIQOfficeCode = "CIQOF";
			public const string Tariffs = "TARIFFS";
			public const string TariffsFromExcel = "TARIFFSFROMEXCEL";
			public const string DecTpAccess = "DSS";
			public const string ExchangeRate = "EXCHANGERATE";
		}

		public static class FileNames
		{
			public const string Xml_CN_RefCusCodeList = "CNRefCusCodeList.xml";
			public const string Xml_CN_RefCusTariff = "CNRefCusTariff.xml";
		}

		public static class TariffTypes
		{
			public const string HSN = "HSN";
			public const string CIQ = "CIQ";
		}

		public static class DecTpAccess
		{
			public const int CodeTypeMaxLength = 14;
			public const string LanguageCode = "ZHS";

			public static class AttributeNames
			{
				public const string CodeSuffix = "CodeSuffix";
				public const string CustomsOffice = "CustomsOffice";
			}
		}

		public static class DataTypes
		{
			public const string String = "String";
		}
	}
}
