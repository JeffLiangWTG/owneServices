using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business
{
	partial class TransportModeCodes
	{
		public static bool IsContainerised(ZString transportMode)
		{
			switch (transportMode)
			{
				case TransportModeCodes.Codes.AirContainer:
				case TransportModeCodes.Codes.RailContainer:
				case TransportModeCodes.Codes.TruckContainer:
				case TransportModeCodes.Codes.VesselContainer:
					return true;
				default:
					return false;
			}
		}

		public static bool IsAirTransport(ZString transportMode)
		{
			switch (transportMode)
			{
				case TransportModeCodes.Codes.AirContainer:
				case TransportModeCodes.Codes.AirNonContainer:
					return true;
				default:
					return false;
			}
		}
	}
}
