namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IQRCodeDataProvider
	{
		string GetTransactionQRCodeString(ITransactionQRCodeDataProvider transactionData);
	}
}
