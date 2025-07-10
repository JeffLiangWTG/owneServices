using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class AirBookingCostBreakdownCharge : IBookingCostBreakdownCharge
	{
		public decimal Amount { get; set; }

		public string Currency { get; set; }

		public string ChargeCode { get; set; }

		public string ChargeCodeDescription { get; set; }
	}
}
