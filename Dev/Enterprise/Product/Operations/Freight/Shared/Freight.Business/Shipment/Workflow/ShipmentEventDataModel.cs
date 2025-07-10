namespace Enterprise.Freight.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;

	public class ShipmentEventDataModel<T> : BusinessObjectEventDataModel where T : CommonShipment
	{
		public ShipmentEventDataModel(T shipment)
			: base(shipment)
		{
		}

		#region Properties

		protected T Shipment => (T)base.Parent;

		#region All Legs

		public string Origin => Shipment.JS_RL_NKOrigin;

		public string Destination => Shipment.JS_RL_NKDestination;

		public TransportEventDataModel FirstLeg => Transports.Any() ? new TransportEventDataModel(Transports.First()) : null;

		public TransportEventDataModel SecondLeg => Transports.Length >= 2 ? new TransportEventDataModel(Transports[1]) : null;

		public TransportEventDataModel ThirdLeg => Transports.Length >= 3 ? new TransportEventDataModel(Transports[2]) : null;

		public TransportEventDataModel FourthLeg => Transports.Length >= 4 ? new TransportEventDataModel(Transports[3]) : null;

		public TransportEventDataModel LastLeg => Transports.Any() ? new TransportEventDataModel(Transports.Last()) : null;

		#endregion

		#region RoadLegs

		public TransportEventDataModel FirstRoadLeg => RoadTransports.Length > 0 ? new TransportEventDataModel(RoadTransports.First()) : null;

		public TransportEventDataModel SecondRoadLeg => RoadTransports.Length >= 2 ? new TransportEventDataModel(RoadTransports[1]) : null;

		public TransportEventDataModel ThirdRoadLeg => RoadTransports.Length >= 3 ? new TransportEventDataModel(RoadTransports[2]) : null;

		public TransportEventDataModel FourthRoadLeg => RoadTransports.Length >= 4 ? new TransportEventDataModel(RoadTransports[3]) : null;

		public TransportEventDataModel LastRoadLeg => RoadTransports.Length > 0 ? new TransportEventDataModel(RoadTransports.Last()) : null;

		#endregion

		#region RailLegs

		public TransportEventDataModel FirstRailLeg => RailTransports.Length > 0 ? new TransportEventDataModel(RailTransports.First()) : null;

		public TransportEventDataModel SecondRailLeg => RailTransports.Length >= 2 ? new TransportEventDataModel(RailTransports[1]) : null;

		public TransportEventDataModel ThirdRailLeg => RailTransports.Length >= 3 ? new TransportEventDataModel(RailTransports[2]) : null;

		public TransportEventDataModel FourthRailLeg => RailTransports.Length >= 4 ? new TransportEventDataModel(RailTransports[3]) : null;

		public TransportEventDataModel LastRailLeg => RailTransports.Length > 0 ? new TransportEventDataModel(RailTransports.Last()) : null;

		#endregion

		#region AirLegs

		public TransportEventDataModel FirstAirLeg => AirTransports.Length > 0 ? new TransportEventDataModel(AirTransports.First()) : null;

		public TransportEventDataModel SecondAirLeg => AirTransports.Length >= 2 ? new TransportEventDataModel(AirTransports[1]) : null;

		public TransportEventDataModel ThirdAirLeg => AirTransports.Length >= 3 ? new TransportEventDataModel(AirTransports[2]) : null;

		public TransportEventDataModel FourthAirLeg => AirTransports.Length >= 4 ? new TransportEventDataModel(AirTransports[3]) : null;

		public TransportEventDataModel LastAirLeg => AirTransports.Length > 0 ? new TransportEventDataModel(AirTransports.Last()) : null;

		#endregion

		#region SeaLegs

		public TransportEventDataModel FirstSeaLeg => SeaTransports.Length > 0 ? new TransportEventDataModel(SeaTransports.First()) : null;

		public TransportEventDataModel SecondSeaLeg => SeaTransports.Length >= 2 ? new TransportEventDataModel(SeaTransports[1]) : null;

		public TransportEventDataModel ThirdSeaLeg => SeaTransports.Length >= 3 ? new TransportEventDataModel(SeaTransports[2]) : null;

		public TransportEventDataModel FourthSeaLeg => SeaTransports.Length >= 4 ? new TransportEventDataModel(SeaTransports[3]) : null;

		public TransportEventDataModel LastSeaLeg => SeaTransports.Length > 0 ? new TransportEventDataModel(SeaTransports.Last()) : null;

		#endregion

		#endregion

		#region Internal

		protected Transport[] Transports
		{
			get
			{
				if (transports == null)
				{
					transports = new CachedProperty<Transport[]>(Shipment.Factory, () =>
					{
						var legs = Shipment.TransportsIncludingRelated.Cast<Transport>().ToArray();
						MovementLegComparer.SortMovementLegsByPorts(legs);
						return legs;
					});
				}

				return transports.Value;
			}
		}
		CachedProperty<Transport[]> transports;

		Transport[] SeaTransports => seaTransports ?? (seaTransports = Transports.Where(transport => transport.TransportMode == Core.Constants.TransportModes.Sea).ToArray());
		Transport[] seaTransports;

		Transport[] AirTransports => airTransports ?? (airTransports = Transports.Where(transport => transport.TransportMode == Core.Constants.TransportModes.Air).ToArray());
		Transport[] airTransports;

		Transport[] RoadTransports => roadTransports ?? (roadTransports = Transports.Where(transport => transport.TransportMode == Core.Constants.TransportModes.Road).ToArray());
		Transport[] roadTransports;

		Transport[] RailTransports => railTransports ?? (railTransports = Transports.Where(transport => transport.TransportMode == Core.Constants.TransportModes.Rail).ToArray());
		Transport[] railTransports;

		#endregion
	}
}
