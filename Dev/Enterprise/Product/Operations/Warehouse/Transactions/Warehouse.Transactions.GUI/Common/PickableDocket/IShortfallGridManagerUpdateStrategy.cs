using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public interface IShortfallGridManagerUpdateStrategy
	{
		void Update(WhsPickableDocketLine line);
	}

	public class OrderLineShortfallUpdateStrategy : IShortfallGridManagerUpdateStrategy
	{
		public void Update(WhsPickableDocketLine line)
		{
			line.Validation.ValidateWE_ShortfallQuantityCached();
		}
	}

	// implement this if there are performance problems with auto-calculating shortfall/qtyToBuild on WE_TransactionQuantity changed,
	// and remove the code from WhsWorkOrderLine.WE_TransactionQuantity that auto-calculates every time WE_TransactionQuantity changes.
	public class WorkOrderLineShortfallUpdateStrategy : IShortfallGridManagerUpdateStrategy
	{
		void IShortfallGridManagerUpdateStrategy.Update(WhsPickableDocketLine line)
		{
			line.Validation.ValidateWE_ShortfallQuantityCached();
		}
	}
}
