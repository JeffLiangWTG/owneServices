namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class VoyageModeConverter : ListConverter<VoyageModes, MovementVoyageModeList>
	{
		public override MovementVoyageModeList Convert(VoyageModes data, FormattingResult formattingResult)
		{
			MovementVoyageModeList result = null;
			switch (data)
			{
				case VoyageModes.Sea:
					result = MovementVoyageModeList.SeaMaritimeTransport;
					break;
				case VoyageModes.Rail:
					result = MovementVoyageModeList.RailTransport;
					break;
				case VoyageModes.Road:
					result = MovementVoyageModeList.RoadTransport;
					break;
				case VoyageModes.Air:
					result = MovementVoyageModeList.AirTransport;
					break;
			}

			return result;
		}
	}
}
