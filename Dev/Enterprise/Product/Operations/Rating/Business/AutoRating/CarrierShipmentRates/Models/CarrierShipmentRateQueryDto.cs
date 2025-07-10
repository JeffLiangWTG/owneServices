using System.Collections.Generic;

namespace Enterprise.Rating.Business;

public sealed class CarrierShipmentRateQueryDto
{
	public CarrierShipmentRateShipmentDto Shipment { get; set; }

	public IEnumerable<CarrierShipmentRateRouteLegDto> RouteLegs { get; set; }

	public IEnumerable<CarrierShipmentRateCargoDto> Cargo { get; set; }

	public bool GetRatesForSales { get; set; }

	public bool GetRatesForCosts { get; set; }
}
