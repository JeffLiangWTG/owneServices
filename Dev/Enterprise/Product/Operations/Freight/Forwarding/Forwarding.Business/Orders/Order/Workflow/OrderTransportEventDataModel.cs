using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderTransportEventDataModel : TransportEventDataModel
	{
		public OrderTransportEventDataModel(Order order)
			: base(order)
		{
			this.Order = order;
		}

		public OrderTransportEventDataModel(Transport transport)
			: base(transport)
		{
		}

		Order Order { get; set; }

		public override string Origin
		{
			get
			{
				if (Order != null)
				{
					return Order.JD_RL_NKPortOfLoading;
				}
				else
				{
					return base.Origin;
				}
			}
		}

		public override string Destination
		{
			get
			{
				if (Order != null)
				{
					return Order.JD_RL_NKPortOfDischarge;
				}
				else
				{
					return base.Destination;
				}
			}
		}
	}
}
