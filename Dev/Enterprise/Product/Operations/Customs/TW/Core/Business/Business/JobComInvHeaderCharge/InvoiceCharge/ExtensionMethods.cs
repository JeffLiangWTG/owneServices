using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public static class ExtensionMethods
	{
		public static string GetIncoTermChargeFactoryCacheKey(this JobDeclaration declaration, ZString incoTerm) => GetCustomsChargeTypeListCacheKeyCore(declaration.IsImport, incoTerm);

		public static string GetCustomsChargeTypeListCacheKey(this JobComInvoiceHeader invoice) => GetCustomsChargeTypeListCacheKeyCore(invoice.IsImport, invoice.JZ_IncoTerm);

		public static string GetCustomsChargeTypeListCacheKey(this JobComInvoiceGroupHeader groupInvoice) => GetCustomsChargeTypeListCacheKeyCore(groupInvoice.JobDeclaration?.IsImport ?? false, ZString.Empty);

		static string GetCustomsChargeTypeListCacheKeyCore(bool isImport, ZString incoTerm) => isImport ? "IMP" : (incoTerm == Core.Constants.IncoTerms.ExWorks ? "EXPEXW" : "");
	}
}
