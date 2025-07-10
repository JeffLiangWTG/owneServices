namespace Enterprise.MasterFiles.Business
{
	public enum GLAccountRegistryType
	{
		NotApplicable,
		Error,
		APControlAccount,
		ARControlAccount,
		TaxTransactionPrepaidAssetControlAccount,
		TaxTransactionRemittanceLiabilityControlAccount,
		TaxTransactionExpenseAccount,
		TaxTransactionNegativeRevenueAccount,
		PendingTaxTransactionPrepaidAssetControlAccount,
		PendingTaxTransactionRemittanceLiabilityControlAccount
	}
}
