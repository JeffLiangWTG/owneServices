using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderToBulkUpdateCollection : NonPersistentBusinessObjectCollection<OrderToBulkUpdate>
	{
		public OrderToBulkUpdateCollection(BusinessObjectFactory factory, OrderDetailsBulkUpdateBusinessObject parent) : base(factory)
		{
			this.Parent = parent;
		}

		public readonly OrderDetailsBulkUpdateBusinessObject Parent;

		public OrderToBulkUpdate AddOrderToBulkUpdate(ZGuid orderPK)
		{
			var selectedOrder = Factory.Load<Order>(orderPK);
			OrderToBulkUpdate result = AddNew();
			result.SetOrder(selectedOrder);
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrderToBulkUpdate(Factory, Parent);
		}
	}
}
