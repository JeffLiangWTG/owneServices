namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public static class DataSourceConstants
	{
		public static class Country
		{
			public const string Singapore = "SG";
		}

		public static class Source
		{
			public const string eHubZACustomsRepositoryQueue = "ZAA";
			public const string ZACustomsWebsite = "ZAW";
			public const string InternalWebsite = "INT";
			public const string Upload = "UPL";
		}

		public static class SubSource
		{
			public const string CLASET = "CLASET";
			public const string IFTRIN = "IFTRIN";
			public const string ZaTariffs = "ZA Tariffs";
			public const string ZaFullTariffs = "ZA Full Tariffs";
		}

		public static class ContentType
		{
			public const string ZA_ProDat = "PRO";
			public const string ZA_Gesmes = "MES";
			public const string ZA_TariffPdf = "PDF";
			public const string ZA_TariffCompositeKey = "TCK";
			public const string ZA_Carrier = "CAR";
			public const string UniversalXML = "URD";
			public const string Edifact = "EDF";
			public const string XML = "XML";
		}

		public enum FileType
		{
			TXT,
			PDF,
			XML,
			COM
		}
	}
}
