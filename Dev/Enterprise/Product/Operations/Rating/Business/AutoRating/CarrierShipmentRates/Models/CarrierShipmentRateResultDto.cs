using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Newtonsoft.Json.Converters;

namespace Enterprise.Rating.Business;

public sealed class CarrierShipmentRateResultDto(ICollection<CarrierShipmentRateChargeDto> charges, CarrierShipmentRateCriteriaDto criteria, CostSell costOrSell, string localCurrency)
{
	public ICollection<CarrierShipmentRateChargeDto> Charges { get; init; } = charges;

	public CarrierShipmentRateCriteriaDto Criteria { get; init; } = criteria;

	[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
	public CostSell CostOrSell { get; init; } = costOrSell;

	public string LocalCurrency { get; init; } = localCurrency;
}
