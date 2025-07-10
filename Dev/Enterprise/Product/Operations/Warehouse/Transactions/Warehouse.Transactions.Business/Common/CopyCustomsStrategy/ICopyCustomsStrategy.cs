namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ICopyCustomsStrategy
	{
		void CopyCustomsData(WhsDocketLine transactionLine, WhsDocketLine inventoryLine);
	}
}
