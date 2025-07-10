using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class CargoReportTransportValidation : TransportValidation
	{
		public CargoReportTransportValidation(Transport transport)
			: base(transport)
		{
			Argument.NotNull(transport, "Transport");
		}

		protected override void CheckJW_VoyageFlight()
		{
			base.CheckJW_VoyageFlight();

			if (Parent.Parent is ForwardingConsol consol && consol.IsAir && !Parent.JW_VoyageFlight.IsEmpty)
			{
				if (!FlightsAndVesselsHelper.IsValidFlightOrVessel(Parent.Factory, Parent.JW_VoyageFlight))
				{
					Parent.JW_VoyageFlightInfo.AddMessageError(FlightNotOnCustomsSupportedList);
				}
			}
		}

		protected override void CheckJW_Vessel()
		{
			base.CheckJW_Vessel();

			if (Parent.Parent is ForwardingConsol consol && consol.IsSea && !Parent.JW_Vessel.IsEmpty)
			{
				if (!FlightsAndVesselsHelper.IsValidFlightOrVessel(Parent.Factory, Parent.JW_Vessel))
				{
					Parent.JW_VesselInfo.AddMessageError(VesselNotOnCustomsSupportedList);
				}
			}
		}
		internal const string FlightNotOnCustomsSupportedList = "Flight number is not in the list of valid Flights supported by NZ Customs.";
		internal const string VesselNotOnCustomsSupportedList = "Vessel name is not in the list of valid Vessel names supported by NZ Customs.";
	}
}
