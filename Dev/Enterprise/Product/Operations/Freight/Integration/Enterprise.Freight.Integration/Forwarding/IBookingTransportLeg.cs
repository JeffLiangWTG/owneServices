using System;

namespace Enterprise.Freight.Integration
{
	public interface IBookingTransportLeg
	{
		string TransportMode { get; }
		int LegOrder { get; }

		string VoyageNumber { get; }
		string VesselType { get; }
		string VesselName { get; }

		DateTime EstimatedDeparture { get; }
		DateTime EstimatedArrival { get; }

		string PortOfLoadingCode { get; }
		string PortOfLoadingName { get; }

		string PortOfDischargeCode { get; }
		string PortOfDischargeName { get; }
	}
}
