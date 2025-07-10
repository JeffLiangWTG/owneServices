using System.Collections.Generic;
using CargoWise.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IBookingRate
	{
		decimal Amount { get; }
		string Currency { get; }
		string ChargeCode { get; }
		string ChargeCodeDescription { get; }
		string CarrierPrefix { get; }
		string Remarks { get; }

		IReadOnlyCollection<IBookingTransportLeg> TransportLegs { get; }

		IReadOnlyCollection<ICodeDescription> AdditionalDetails { get; }

		IReadOnlyCollection<IBookingCostBreakdownCharge> CostBreakdownCharges { get; }
	}
}
