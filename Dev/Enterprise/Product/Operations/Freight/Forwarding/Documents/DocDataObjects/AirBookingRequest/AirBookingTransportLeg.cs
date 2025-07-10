using System;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class BookingTransportLeg : IBookingTransportLeg
	{
		public string TransportMode { get; set; }
		public int LegOrder { get; set; }
		public string VoyageNumber { get; set; }
		public string VesselType { get; set; }
		public string VesselName { get; set; }
		public DateTime EstimatedDeparture { get; set; }
		public DateTime EstimatedArrival { get; set; }
		public string PortOfLoadingCode { get; set; }
		public string PortOfLoadingName { get; set; }
		public string PortOfDischargeCode { get; set; }
		public string PortOfDischargeName { get; set; }
	}
}
