using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderLineProcessTaskCollection : ProcessTaskCollection
	{
		public OrderLineProcessTaskCollection(OrderLine orderLine) : base(orderLine)
		{
		}

		public new OrderLineProcessTask this[int index]
		{
			get { return (OrderLineProcessTask)Elements[index]; }
		}

		public new OrderLineProcessTask AddNew()
		{
			return (OrderLineProcessTask)base.AddNew();
		}
	}
}
