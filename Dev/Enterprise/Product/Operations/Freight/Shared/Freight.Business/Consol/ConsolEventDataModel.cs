
namespace Enterprise.Freight.Business
{
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Constants = Core.Constants;

	public class ConsolEventDataModel : BusinessObjectEventDataModel
	{
		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Just a property name")]
		public static class Properties
		{
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string FirstLeg = "FirstLeg";
			public const string SecondLeg = "SecondLeg";
			public const string ThirdLeg = "ThirdLeg";
			public const string FourthLeg = "FourthLeg";
			public const string LastLeg = "LastLeg";
			public const string FirstRoadLeg = "FirstRoadLeg";
			public const string SecondRoadLeg = "SecondRoadLeg";
			public const string ThirdRoadLeg = "ThirdRoadLeg";
			public const string FourthRoadLeg = "FourthRoadLeg";
			public const string LastRoadLeg = "LastRoadLeg";
			public const string FirstRailLeg = "FirstRailLeg";
			public const string SecondRailLeg = "SecondRailLeg";
			public const string ThirdRailLeg = "ThirdRailLeg";
			public const string FourthRailLeg = "FourthRailLeg";
			public const string LastRailLeg = "LastRailLeg";
			public const string FirstAirLeg = "FirstAirLeg";
			public const string SecondAirLeg = "SecondAirLeg";
			public const string ThirdAirLeg = "ThirdAirLeg";
			public const string FourthAirLeg = "FourthAirLeg";
			public const string LastAirLeg = "LastAirLeg";
			public const string FirstSeaLeg = "FirstSeaLeg";
			public const string SecondSeaLeg = "SecondSeaLeg";
			public const string ThirdSeaLeg = "ThirdSeaLeg";
			public const string FourthSeaLeg = "FourthSeaLeg";
			public const string LastSeaLeg = "LastSeaLeg";
		}

		#endregion

		public ConsolEventDataModel(CommonConsol consol)
			: base(consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
		}

		#region Properties

		readonly CommonConsol consol;

		public string Origin
		{
			get
			{
				return consol.JK_RL_NKLoadPort;
			}
		}

		public string Destination
		{
			get
			{
				return consol.JK_RL_NKDischargePort;
			}
		}

		#region AllLegs

		public TransportEventDataModel FirstLeg
		{
			get
			{
				return Transports.Length > 0 ? new TransportEventDataModel(Transports.First()) : null;
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
				return Transports.Length > 0 ? new TransportEventDataModel(Transports.Last()) : null;
			}
		}

		#endregion

		#region RoadLegs

		public TransportEventDataModel FirstRoadLeg
		{
			get
			{
				return RoadTransports.Length > 0 ? new TransportEventDataModel(RoadTransports.First()) : null;
			}
		}

		public TransportEventDataModel SecondRoadLeg
		{
			get
			{
				return RoadTransports.Length >= 2 ? new TransportEventDataModel(RoadTransports[1]) : null;
			}
		}

		public TransportEventDataModel ThirdRoadLeg
		{
			get
			{
				return RoadTransports.Length >= 3 ? new TransportEventDataModel(RoadTransports[2]) : null;
			}
		}

		public TransportEventDataModel FourthRoadLeg
		{
			get
			{
				return RoadTransports.Length >= 4 ? new TransportEventDataModel(RoadTransports[3]) : null;
			}
		}

		public TransportEventDataModel LastRoadLeg
		{
			get
			{
				return RoadTransports.Length > 0 ? new TransportEventDataModel(RoadTransports.Last()) : null;
			}
		}

		#endregion

		#region RailLegs

		public TransportEventDataModel FirstRailLeg
		{
			get
			{
				return RailTransports.Length > 0 ? new TransportEventDataModel(RailTransports.First()) : null;
			}
		}

		public TransportEventDataModel SecondRailLeg
		{
			get
			{
				return RailTransports.Length >= 2 ? new TransportEventDataModel(RailTransports[1]) : null;
			}
		}

		public TransportEventDataModel ThirdRailLeg
		{
			get
			{
				return RailTransports.Length >= 3 ? new TransportEventDataModel(RailTransports[2]) : null;
			}
		}

		public TransportEventDataModel FourthRailLeg
		{
			get
			{
				return RailTransports.Length >= 4 ? new TransportEventDataModel(RailTransports[3]) : null;
			}
		}

		public TransportEventDataModel LastRailLeg
		{
			get
			{
				return RailTransports.Length > 0 ? new TransportEventDataModel(RailTransports.Last()) : null;
			}
		}

		#endregion

		#region AirLegs

		public TransportEventDataModel FirstAirLeg
		{
			get
			{
				return AirTransports.Length > 0 ? new TransportEventDataModel(AirTransports.First()) : null;
			}
		}

		public TransportEventDataModel SecondAirLeg
		{
			get
			{
				return AirTransports.Length >= 2 ? new TransportEventDataModel(AirTransports[1]) : null;
			}
		}

		public TransportEventDataModel ThirdAirLeg
		{
			get
			{
				return AirTransports.Length >= 3 ? new TransportEventDataModel(AirTransports[2]) : null;
			}
		}

		public TransportEventDataModel FourthAirLeg
		{
			get
			{
				return AirTransports.Length >= 4 ? new TransportEventDataModel(AirTransports[3]) : null;
			}
		}

		public TransportEventDataModel LastAirLeg
		{
			get
			{
				return AirTransports.Length > 0 ? new TransportEventDataModel(AirTransports.Last()) : null;
			}
		}

		#endregion

		#region SeaLegs

		public TransportEventDataModel FirstSeaLeg
		{
			get
			{
				return SeaTransports.Length > 0 ? new TransportEventDataModel(SeaTransports.First()) : null;
			}
		}

		public TransportEventDataModel SecondSeaLeg
		{
			get
			{
				return SeaTransports.Length >= 2 ? new TransportEventDataModel(SeaTransports[1]) : null;
			}
		}

		public TransportEventDataModel ThirdSeaLeg
		{
			get
			{
				return SeaTransports.Length >= 3 ? new TransportEventDataModel(SeaTransports[2]) : null;
			}
		}

		public TransportEventDataModel FourthSeaLeg
		{
			get
			{
				return SeaTransports.Length >= 4 ? new TransportEventDataModel(SeaTransports[3]) : null;
			}
		}

		public TransportEventDataModel LastSeaLeg
		{
			get
			{
				return SeaTransports.Length > 0 ? new TransportEventDataModel(SeaTransports.Last()) : null;
			}
		}

		#endregion

		public bool IsGateway => consol.IsGatewayConsol;

		#endregion

		#region Internal

		Transport[] Transports
		{
			get
			{
				if (transports == null)
				{
					var legs = consol.Transports.Cast<Transport>().ToArray();
					MovementLegComparer.SortMovementLegsByPorts(legs);
					transports = legs;
				}

				return transports;
			}
		}
		Transport[] transports;

		#region Transport arrays filtered by mode

		Transport[] SeaTransports => seaTransports ?? (seaTransports = Transports.Where(transport => transport.TransportMode == Constants.TransportModes.Sea).ToArray());
		Transport[] seaTransports;

		Transport[] AirTransports => airTransports ?? (airTransports = Transports.Where(transport => transport.TransportMode == Constants.TransportModes.Air).ToArray());
		Transport[] airTransports;

		Transport[] RoadTransports => roadTransports ?? (roadTransports = Transports.Where(transport => transport.TransportMode == Constants.TransportModes.Road).ToArray());
		Transport[] roadTransports;

		Transport[] RailTransports => railTransports ?? (railTransports = Transports.Where(transport => transport.TransportMode == Constants.TransportModes.Rail).ToArray());
		Transport[] railTransports;

		#endregion

		#endregion
	}
}
