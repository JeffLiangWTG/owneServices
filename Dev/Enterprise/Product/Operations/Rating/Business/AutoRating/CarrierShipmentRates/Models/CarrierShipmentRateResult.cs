using System.Collections.Generic;

namespace Enterprise.Rating.Business;

public sealed record CarrierShipmentRateResult(
	ICollection<CarrierShipmentRateResultDto> Rates,
	string[] Logs,
	string[] Errors);
