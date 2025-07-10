using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class AgencyContainerEventDataModel : BusinessObjectEventDataModel
	{
		public AgencyContainerEventDataModel(AgencyShipmentContainer container)
			: base(container)
		{
		}

		#region Properties

		protected AgencyShipmentContainer Container
		{
			get
			{
				return (AgencyShipmentContainer)base.Parent;
			}
		}

		public string Origin
		{
			get
			{
				var parent = Container.Booking;
				return parent != null ? parent.JS_RL_NKOrigin.ToString() : string.Empty;
			}
		}

		public string Destination
		{
			get
			{
				var parent = Container.Booking;
				return parent != null ? parent.JS_RL_NKDestination.ToString() : string.Empty;
			}
		}

		public TransportEventDataModel FirstLeg
		{
			get
			{
				return Transports.Any() ? new TransportEventDataModel(Transports.First()) : null;
			}
		}

		public TransportEventDataModel SecondLeg
		{
			get
			{
				return Transports.Length >= 2 ? new TransportEventDataModel(Transports[1]) : null;
			}
		}

		public TransportEventDataModel ThirdLeg
		{
			get
			{
				return Transports.Length >= 3 ? new TransportEventDataModel(Transports[2]) : null;
			}
		}

		public TransportEventDataModel FourthLeg
		{
			get
			{
				return Transports.Length >= 4 ? new TransportEventDataModel(Transports[3]) : null;
			}
		}

		public TransportEventDataModel LastLeg
		{
			get
			{
				return Transports.Any() ? new TransportEventDataModel(Transports.Last()) : null;
			}
		}

		#endregion

		#region Internal 

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		protected Transport[] Transports
		{
			get
			{
				var parent = Container.Booking;
				if (parent == null)
				{
					return System.Array.Empty<Transport>();
				}

				if (transports == null)
				{
					transports = new CachedProperty<Transport[]>(parent.Factory, () =>
					{
						var legs = parent.Transports.Cast<Transport>().ToArray();
						MovementLegComparer.SortMovementLegsByPorts(legs);
						return legs;
					});
				}

				return transports.Value;
			}
		}

		CachedProperty<Transport[]> transports;

		#endregion
	}
}









