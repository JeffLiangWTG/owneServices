using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderLineDeliverContainerCollection : ActiveBusinessObjectCollection<OrderLineDeliverContainer>
	{
		public OrderLineDeliverContainerCollection(OrderLineDelivery parent)
			: base(parent)
		{
		}
	}
}
