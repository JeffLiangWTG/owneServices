namespace Enterprise.MasterFiles.Business
{
	public interface IEInvoicingTransactionProxyFactory
	{
		IEInvoicingTransaction GetProxy(AccTransactionHeader transaction);
	}
}
