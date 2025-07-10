using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class ContainerEventDataModel : BusinessObjectEventDataModel
	{
		public ContainerEventDataModel(CommonContainer container)
			: base(container)
		{
		}

		#region Properties

		protected CommonContainer Container
		{
			get
			{
				return (CommonContainer)base.Parent;
			}
		}

		public string Origin
		{
			get
			{
				var parent = Container.ContainerParent;
				return parent != null && parent.LoadPort != null ? parent.LoadPort.RL_Code.ToString() : string.Empty;
			}
		}

		public string Destination
		{
			get
			{
				var parent = Container.ContainerParent;
				return parent != null && parent.DischargePort != null ? parent.DischargePort.RL_Code.ToString() : string.Empty;
			}
		}

		#region AllLegs

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

		#region SeaLegs

		public TransportEventDataModel FirstSeaLeg
		{
			get => SeaTransports.Length >= 1 ? new TransportEventDataModel(SeaTransports.First()) : null;
		}

		public TransportEventDataModel SecondSeaLeg
		{
			get => SeaTransports.Length >= 2 ? new TransportEventDataModel(SeaTransports[1]) : null;
		}

		public TransportEventDataModel ThirdSeaLeg
		{
			get => SeaTransports.Length >= 3 ? new TransportEventDataModel(SeaTransports[2]) : null;
		}

		public TransportEventDataModel FourthSeaLeg
		{
			get => SeaTransports.Length >= 4 ? new TransportEventDataModel(SeaTransports[3]) : null;
		}

		public TransportEventDataModel LastSeaLeg
		{
			get => SeaTransports.Length >= 1 ? new TransportEventDataModel(SeaTransports.Last()) : null;
		}

		#endregion

		#region AirLegs

		public TransportEventDataModel FirstAirLeg
		{
			get => AirTransports.Length >= 1 ? new TransportEventDataModel(AirTransports.First()) : null;
		}

		public TransportEventDataModel SecondAirLeg
		{
			get => AirTransports.Length >= 2 ? new TransportEventDataModel(AirTransports[1]) : null;
		}

		public TransportEventDataModel ThirdAirLeg
		{
			get => AirTransports.Length >= 3 ? new TransportEventDataModel(AirTransports[2]) : null;
		}

		public TransportEventDataModel FourthAirLeg
		{
			get => AirTransports.Length >= 4 ? new TransportEventDataModel(AirTransports[3]) : null;
		}

		public TransportEventDataModel LastAirLeg
		{
			get => AirTransports.Length >= 1 ? new TransportEventDataModel(AirTransports.Last()) : null;
		}

		#endregion

		#region RoadLegs

		public TransportEventDataModel FirstRoadLeg
		{
			get => RoadTransports.Length >= 1 ? new TransportEventDataModel(RoadTransports.First()) : null;
		}

		public TransportEventDataModel SecondRoadLeg
		{
			get => RoadTransports.Length >= 2 ? new TransportEventDataModel(RoadTransports[1]) : null;
		}

		public TransportEventDataModel ThirdRoadLeg
		{
			get => RoadTransports.Length >= 3 ? new TransportEventDataModel(RoadTransports[2]) : null;
		}

		public TransportEventDataModel FourthRoadLeg
		{
			get => RoadTransports.Length >= 4 ? new TransportEventDataModel(RoadTransports[3]) : null;
		}

		public TransportEventDataModel LastRoadLeg
		{
			get => RoadTransports.Length >= 1 ? new TransportEventDataModel(RoadTransports.Last()) : null;
		}

		#endregion

		#region RailLegs

		public TransportEventDataModel FirstRailLeg
		{
			get => RailTransports.Length >= 1 ? new TransportEventDataModel(RailTransports.First()) : null;
		}

		public TransportEventDataModel SecondRailLeg
		{
			get => RailTransports.Length >= 2 ? new TransportEventDataModel(RailTransports[1]) : null;
		}

		public TransportEventDataModel ThirdRailLeg
		{
			get => RailTransports.Length >= 3 ? new TransportEventDataModel(RailTransports[2]) : null;
		}

		public TransportEventDataModel FourthRailLeg
		{
			get => RailTransports.Length >= 4 ? new TransportEventDataModel(RailTransports[3]) : null;
		}

		public TransportEventDataModel LastRailLeg
		{
			get => RailTransports.Length >= 1 ? new TransportEventDataModel(RailTransports.Last()) : null;
		}

		#endregion

		#endregion

		#region Internal

		protected Transport[] Transports
		{
			get
			{
				var parent = Container.ContainerParent;
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

		Transport[] SeaTransports => seaTransports ?? (seaTransports = Transports.Where(transport => transport.TransportMode == Constants.TransportModes.Sea).ToArray());
		Transport[] seaTransports;

		Transport[] AirTransports => airTransports ?? (airTransports = Transports.Where(transport => transport.TransportMode == Constants.TransportModes.Air).ToArray());
		Transport[] airTransports;

		Transport[] RoadTransports => roadTransports ?? (roadTransports = Transports.Where(transport => transport.TransportMode == Constants.TransportModes.Road).ToArray());
		Transport[] roadTransports;

		Transport[] RailTransports => railTransports ?? (railTransports = Transports.Where(transport => transport.TransportMode == Constants.TransportModes.Rail).ToArray());
		Transport[] railTransports;

		#endregion
	}
}
