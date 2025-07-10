namespace Enterprise.Freight.Business
{
	public static class VesselVoyageCaptionProvider
	{
		public static string GetVoyageCaption(string transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					return Res.GetString("Consol|VoyageLabel|Flight", "Flight");
				case Core.Constants.TransportModes.Sea:
					return Res.GetString("Consol|VoyageLabel|Voyage", "Voyage");
				case Core.Constants.TransportModes.Road:
					return Res.GetString("Consol|VoyageLabel|TruckRego", "Truck Ref");
				case Core.Constants.TransportModes.Rail:
					return Res.GetString("Consol|VoyageLabel|JourneyNo", "Journey Num.");
				default:
					return Res.GetString("Consol|VoyageLabel|VoyageFlight", "Voy./Flight");
			}
		}

		public static string GetVesselCaption(string transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Sea:
					return Res.GetString("Consol|VoyageLabel|Vessel", "Vessel");
				case Core.Constants.TransportModes.Road:
					return Res.GetString("Consol|VoyageLabel|TruckOtherInfo", "Other Info");
				case Core.Constants.TransportModes.Rail:
					return Res.GetString("Consol|VoyageLabel|Journey", "Journey Ref");
				default:
					return Res.GetString("Consol|VoyageLabel|Vessel", "Vessel");
			}
		}
	}
}
