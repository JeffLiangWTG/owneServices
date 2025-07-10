
namespace Enterprise.Freight.Forwarding.Orders.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;

	public class OrderEventDataModel : BusinessObjectEventDataModel
	{
		public OrderEventDataModel(Order order)
			: base(order)
		{
		}

		#region Properties

		protected Order Order
		{
			get
			{
				return (Order)base.Parent;
			}
		}

		protected ForwardingShipment Shipment
		{
			get
			{
				return Order.Shipment;
			}
		}

		public string Origin
		{
			get
			{
				return Order.JD_RL_NKPortOfLoading;
			}
		}

		public string Destination
		{
			get
			{
				return Order.JD_RL_NKPortOfDischarge;
			}
		}

		public OrderTransportEventDataModel FirstLeg
		{
			get
			{
				if (Shipment != null)
				{
					var transports = GetTransportsFromShipment();
					return transports.Any() ? new OrderTransportEventDataModel(transports.First()) : null;
				}
				else
				{
					return new OrderTransportEventDataModel(Order);
				}
			}
		}

		public OrderTransportEventDataModel SecondLeg
		{
			get
			{
				var transports = GetTransportsFromShipment();
				return transports.Length >= 2 ? new OrderTransportEventDataModel(transports[1]) : null;
			}
		}

		public OrderTransportEventDataModel ThirdLeg
		{
			get
			{
				var transports = GetTransportsFromShipment();
				return transports.Length >= 3 ? new OrderTransportEventDataModel(transports[2]) : null;
			}
		}

		public OrderTransportEventDataModel FourthLeg
		{
			get
			{
				var transports = GetTransportsFromShipment();
				return transports.Length >= 4 ? new OrderTransportEventDataModel(transports[3]) : null;
			}
		}

		public OrderTransportEventDataModel LastLeg
		{
			get
			{
				if (Shipment != null)
				{
					var transports = GetTransportsFromShipment();
					return transports.Any() ? new OrderTransportEventDataModel(transports.Last()) : null;
				}
				else
				{
					return new OrderTransportEventDataModel(Order);
				}
			}
		}

		#endregion

		#region Internal

		protected Transport[] GetTransportsFromShipment()
		{
			if (Shipment == null)
			{
				return System.Array.Empty<Transport>();
			}

			if (transportsCached == null)
			{
				transportsCached = new CachedProperty<Transport[]>(Shipment.Factory, () =>
				{
					var legs = Shipment.TransportsIncludingRelated.Cast<Transport>().ToArray();
					MovementLegComparer.SortMovementLegsByPorts(legs);
					return legs;
				});
			}

			return transportsCached.Value;
		}

		CachedProperty<Transport[]> transportsCached;

		#endregion
	}
}
