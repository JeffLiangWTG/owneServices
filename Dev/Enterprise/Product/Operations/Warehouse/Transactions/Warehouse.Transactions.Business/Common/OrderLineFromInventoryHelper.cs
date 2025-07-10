using CargoWise.ComponentModel;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class OrderLineFromInventoryHelper : PickableDocketLineFromInventoryHelper<WhsOrderLine>
	{
		public OrderLineFromInventoryHelper(INotifications notifications, WhsOrder order)
			: base(notifications, order)
		{
		}

		protected override ICopyCustomsStrategy CreateCopyCustomsStrategyCore() => new OrderCopyCustomsStrategy();
	}
}
