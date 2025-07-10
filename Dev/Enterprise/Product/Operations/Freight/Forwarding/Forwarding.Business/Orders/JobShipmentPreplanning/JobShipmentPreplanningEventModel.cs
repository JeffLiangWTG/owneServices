
namespace Enterprise.Freight.Forwarding.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Orders.Business;
	using Enterprise.MasterFiles.Business;

	public class JobShipmentPreplanningEventModel : BusinessObjectEventDataModel
	{
		public JobShipmentPreplanningEventModel(JobShipmentPreplanning shipment)
			: base(shipment)
		{
		}

		#region Properties

		protected JobShipmentPreplanning Shipment
		{
			get
			{
				return (JobShipmentPreplanning)base.Parent;
			}
		}

		public string Origin
		{
			get
			{
				return Shipment.EF_RL_NKPortLoad;
			}
		}

		public string Destination
		{
			get
			{
				return Shipment.EF_RL_NKPortDisch;
			}
		}

		public TransportEventDataModel FirstLeg
		{
			get
			{
				return GetTransports().Any() ? new TransportEventDataModel(GetTransports().First()) : null;
			}
		}

		public TransportEventDataModel SecondLeg
		{
			get
			{
				return GetTransports().Length >= 2 ? new TransportEventDataModel(GetTransports()[1]) : null;
			}
		}

		public TransportEventDataModel ThirdLeg
		{
			get
			{
				return GetTransports().Length >= 3 ? new TransportEventDataModel(GetTransports()[2]) : null;
			}
		}

		public TransportEventDataModel FourthLeg
		{
			get
			{
				return GetTransports().Length >= 4 ? new TransportEventDataModel(GetTransports()[3]) : null;
			}
		}

		public TransportEventDataModel LastLeg
		{
			get
			{
				return GetTransports().Any() ? new TransportEventDataModel(GetTransports().Last()) : null;
			}
		}

		#endregion

		#region Internal

		protected Transport[] GetTransports()
		{
			if (transports == null)
			{
				transports = new CachedProperty<Transport[]>(Shipment.Factory, () =>
				{
					var legs = Shipment.PreAdviceTransports.Cast<Transport>().ToArray();
					MovementLegComparer.SortMovementLegsByPorts(legs);
					return legs;
				});
			}

			return transports.Value;
		}

		CachedProperty<Transport[]> transports;

		#endregion
	}
}
