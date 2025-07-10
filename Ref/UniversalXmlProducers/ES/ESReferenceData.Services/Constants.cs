namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public static class Constants
	{
		public static class AEAT
		{
			public static readonly string[] EmptyKeysForExportableDocs = { "DEF_MASIVA", "AYUCMP", "PAGINA", "VEZ", "COLUMNA_ORDEN", "MODO_ORDEN" };

			public const string FirstLineForCSV = "<!--Interfaz usada => es.aeat.adtb.jdit.imp.tablas.htm.HtmTbElementoSrvImpl-->\n";

			public const string LastLineForExportCSV = ";;;;;;;;;;;;;;";

			public const string LastLineForImportCSV = ";;;;;;;;;;;;;;;;;;;;;;;;;;;;;";
		}
	}
}
