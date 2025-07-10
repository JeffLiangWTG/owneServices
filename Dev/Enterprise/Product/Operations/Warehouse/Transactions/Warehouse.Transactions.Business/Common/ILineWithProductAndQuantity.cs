using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ILineWithProductAndQuantity
	{
		ZDecimal Quantity { get; }
		ZGuid ProductPK { get; }
	}
}
