using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMDeparture : IAIMDeparture
	{
		public AIMDeparture(AdditionalMessageInformation additionalMessageInformation)
		{
			Argument.NotNull(additionalMessageInformation, "additionalMessageInformation");

			header = additionalMessageInformation.Header;
			departureTimeUTC = additionalMessageInformation.FlightDepartureTimeUTC;
			flightDetails = additionalMessageInformation.FlightArrivalDetails.GetSelectedFlights().FirstOrDefault();
			if (flightDetails == null)
			{
				flightDetails = additionalMessageInformation.FlightArrivalDetails.AddNew();
				flightDetails.FlightNo = header.AMA_Voyage;
			}
		}

		readonly AsycudaManifestHeader header;
		readonly FlightDetail flightDetails;
		readonly ZDateTime departureTimeUTC;

		public ZString FlightNumber => AIMFlightHelper.CreatePaddedFlightNumberForMessage(flightDetails.FlightNo, header.AMA_CarrierCode);

		public ZDate DateOfScheduledArrival => AIMFlightHelper.CalculateArrivalDate(header, flightDetails);

		public ZDate LiftoffDate => departureTimeUTC.Date;

		public ZString LiftoffTime => !departureTimeUTC.IsEmpty ? departureTimeUTC.ToShortTimeString() : string.Empty;

		public ZString ActualImportingCarrier => ZString.Empty;

		public ZString ActualFlightNumber => ZString.Empty;
	}
}
