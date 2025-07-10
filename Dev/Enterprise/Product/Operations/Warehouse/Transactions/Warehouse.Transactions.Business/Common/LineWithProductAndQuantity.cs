using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	class LineWithProductAndQuantity : ILineWithProductAndQuantity
	{
		public LineWithProductAndQuantity(ZGuid productPK, ZDecimal qty)
		{
			Quantity = qty;
			ProductPK = productPK;
		}

		public ZDecimal Quantity { get; }
		public ZGuid ProductPK { get; }
	}
}
