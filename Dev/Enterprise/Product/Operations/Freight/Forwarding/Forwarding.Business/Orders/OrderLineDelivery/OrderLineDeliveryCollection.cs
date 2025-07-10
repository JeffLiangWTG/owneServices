using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderLineDeliveryCollection : ActiveBusinessObjectCollection<OrderLineDelivery>
	{
		public OrderLineDeliveryCollection(BusinessObjectFactory factory, OrderLine parent)
			: base(factory, parent)
		{
		}

		public void AddToCopiedCollection(OrderLineDeliveryCollection copiedDeliveries)
		{
			foreach (OrderLineDelivery delivery in ToArray())
			{
				OrderLineDelivery newOrderLineDelivery = (OrderLineDelivery)delivery.Clone();
				copiedDeliveries.Add(newOrderLineDelivery);

				foreach (ZPropertyInfo propertyInfo in newOrderLineDelivery.ZPropertyInfoHash)
				{
					if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
					{
						propertyInfo.Value = ZDateTime.Empty;
					}
				}
			}
		}
	}
}
