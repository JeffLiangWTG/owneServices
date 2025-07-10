namespace Enterprise.Customs.TR.Business.Declaration
{
	public static class ExtensionMethods
	{
		public static string GetIncoTermChargeFactoryCacheKey(this JobDeclaration declaration) => GetCustomsChargeTypeListCacheKeyCore(declaration.IsImport);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceHeader invoice) => GetCustomsChargeTypeListCacheKeyCore(invoice.IsImport);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceGroupHeader groupInvoice) => GetCustomsChargeTypeListCacheKeyCore(groupInvoice.JobDeclaration?.IsImport ?? false);

		static string GetCustomsChargeTypeListCacheKeyCore(bool isImport) => isImport ? Common.Shared.SharedJobMessageTypeList.Codes.Import : string.Empty;
	}
}
