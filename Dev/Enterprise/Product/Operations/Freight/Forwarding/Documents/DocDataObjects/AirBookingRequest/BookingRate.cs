using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class BookingRate : IBookingRate
	{
		public decimal Amount { get; set; }
		public string Currency { get; set; }
		public string ChargeCode { get; set; }
		public string ChargeCodeDescription { get; set; }
		public string CarrierPrefix { get; set; }
		public string Remarks { get; set; }

		public IReadOnlyCollection<IBookingTransportLeg> TransportLegs { get; set; }

		public IReadOnlyCollection<ICodeDescription> AdditionalDetails { get; set; }

		public IReadOnlyCollection<IBookingCostBreakdownCharge> CostBreakdownCharges { get; set; }
	}
}
