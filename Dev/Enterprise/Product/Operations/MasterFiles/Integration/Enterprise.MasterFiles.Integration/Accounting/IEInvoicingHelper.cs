namespace Enterprise.MasterFiles.Integration
{
	public interface IEInvoicingHelper
	{
		bool HasActiveEInvoicingTransactionPivot(IAccTransactionHeader transaction, string actionType);
	}
}
