namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IProtectComplianceSubTypeForEInvoicingTransactions
	{
		string ErrorMessageIfComplianceSubTypeIsProtected(AccTransactionHeader transaction);
	}
}
