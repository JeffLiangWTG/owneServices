using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Business
{
	public static class JobVoyageSailingsHelper
	{
		public static bool PortPairShouldCreateSailing(VoyageOrigin origin, VoyageDestination destination, ZString airSeaRoad)
		{
			return PortPairShouldCreateSailing(origin, destination, airSeaRoad, null, null, null, null);
		}

		public static bool PortPairShouldCreateSailing(VoyageOrigin origin, VoyageDestination destination, ZString airSeaRoad, ZDateTime? eta, ZDateTime? etaUtc, ZDateTime? etd, ZDateTime? etdUtc)
		{
			return !((!origin.JA_RL_NKPortOfLoading.IsEmpty && origin.JA_RL_NKPortOfLoading == destination.JB_RL_NKPortOfDischarge)
					 || IsInvalidDateCombinationForSailing(origin, destination, airSeaRoad, eta, etaUtc, etd, etdUtc));
		}

		public static bool IsInvalidDateCombinationForSailing(VoyageOrigin origin, VoyageDestination destination, ZString airSeaRoad)
		{
			return IsInvalidDateCombinationForSailing(origin, destination, airSeaRoad, null, null, null, null);
		}

		static bool IsInvalidDateCombinationForSailing(VoyageOrigin origin, VoyageDestination destination, ZString airSeaRoad, ZDateTime? eta, ZDateTime? etaUtc, ZDateTime? etd, ZDateTime? etdUtc)
		{
			if (origin != null && destination != null)
			{
				return IsInvalidDateCombinationForSailing(etd ?? origin.JA_E_DEP, eta ?? destination.JB_E_ARV, etdUtc ?? origin.JA_E_DEP_UTC, etaUtc ?? destination.JB_E_ARV_UTC, ParseToTransportMode(airSeaRoad));
			}

			return false;
		}

		public static bool IsInvalidDateCombinationForSailing(ZDateTime etd, ZDateTime eta, ZDateTime etdUTC, ZDateTime etaUTC, TransportMode? transportMode)
		{
			if (etd.IsValid && eta.IsValid)
			{
				switch (transportMode)
				{
					case TransportMode.Sea:
					case TransportMode.Rail:
					case TransportMode.Road:
						return etdUTC.IsValid && etaUTC.IsValid
							? etdUTC.Date > etaUTC.Date
							: etd > eta;
					case TransportMode.Air:
						return etd > eta.AddDays(1);
				}
			}
			return false;
		}

		public static TransportMode? ParseToTransportMode(string mode)
		{
			return mode switch
			{
				Core.Constants.TransportModes.Air => TransportMode.Air,
				Core.Constants.TransportModes.Rail => TransportMode.Rail,
				Core.Constants.TransportModes.Road => TransportMode.Road,
				Core.Constants.TransportModes.Sea => TransportMode.Sea,
				_ => null,
			};
		}
	}
}
