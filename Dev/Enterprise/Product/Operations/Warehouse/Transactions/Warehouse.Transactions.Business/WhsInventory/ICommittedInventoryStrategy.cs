using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ICommittedInventoryStrategy
	{
		ZDecimal TotalTransactionQty { get; }
		ZDecimal TotalQtyCommitted { get; }
	}
}
