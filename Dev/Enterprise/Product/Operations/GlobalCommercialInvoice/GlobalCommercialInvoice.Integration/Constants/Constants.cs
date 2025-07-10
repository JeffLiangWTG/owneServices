using CargoWiseOne.ResourceStrings;

namespace Enterprise.GlobalCommercialInvoice.Integration
{
	public static class Constants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string PluginName = "Commercial Invoice";
		public const string PluginTabPageName = "CommercialInvoiceTabPage";
		public static ResourceStringData PluginTabPageCaption => Res.GetData("F5E0045A-07F3-4146-B389-B978BEE4F787", "Commercial Invoice");

		public static class Compliance
		{
			public static class Parties
			{
				public static string ImporterDescription => Res.GetString("85077336-208F-42B8-B5DD-3ADC4B5B578C", "Commercial Invoice Importer");
				public static string SupplierDescription => Res.GetString("4CBB279C-92E7-4376-9DAD-0A4C8DC6248E", "Commercial Invoice Supplier");
			}

			public static class Locations
			{
				public static string CountryOfImportDescription => Res.GetString("0592B956-C18B-496A-B833-4A7C34131237", "Commercial Invoice Import Country");
				public static string CountryOfExportDescription => Res.GetString("6946DEF3-2383-4C83-BAA5-06E0EF571C82", "Commercial Invoice Export Country");
				public static string GoodsCountryOfOriginDescription => Res.GetString("E86D3DF9-EE14-4AE9-BBBB-DDDE40E3FCC5", "Commercial Invoice Goods Origin");
			}

			public static class Commodities
			{
				public static string CommercialInvoiceDescription => Res.GetString("C304D513-0E0B-46BB-9757-42A39FAE0D67", "Commercial Invoice");
			}
		}
	}
}
