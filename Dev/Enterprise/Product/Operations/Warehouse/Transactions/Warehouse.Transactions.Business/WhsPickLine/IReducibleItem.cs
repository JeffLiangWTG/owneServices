using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IReducibleItem : IPackableItem
	{
		bool CanBeReduced { get; }
		ActionResult ReduceStock(ZDecimal quantityToReduce, IPickedStockAdjuster adjuster);
	}
}