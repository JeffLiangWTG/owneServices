namespace Enterprise.Customs.ZA.Business
{
	public static class ExtensionMethods
	{
		public static string GetIncoTermChargeFactoryCacheKey(this JobDeclaration declaration) => GetCustomsChargeTypeListCacheKeyCore(declaration.IsImport);

		public static string GetCustomsChargeTypeListCacheKey(this JobComInvoiceHeader invoice) => GetCustomsChargeTypeListCacheKeyCore(invoice.IsImport);

		public static string GetCustomsChargeTypeListCacheKey(this JobComInvoiceGroupHeader groupInvoice) => GetCustomsChargeTypeListCacheKeyCore(groupInvoice.JobDeclaration?.IsImport ?? false);
		public static string GetCustomsChargeTypeListCacheKey(this JobComInvoiceLine invoiceLine) => GetCustomsChargeTypeListCacheKeyCore(invoiceLine.InvoiceHeader?.IsImport ?? false);

		static string GetCustomsChargeTypeListCacheKeyCore(bool isImport) => isImport ? "IMP" : "";
	}
}
