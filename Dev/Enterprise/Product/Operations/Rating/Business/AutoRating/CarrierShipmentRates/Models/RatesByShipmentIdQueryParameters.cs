using System;
using System.Collections.Generic;

namespace Enterprise.Rating.Business;

public sealed class CalculateRatesQueryParameters
{
	public Guid ShipmentHeaderId { get; set; }
	public ICollection<Guid> RouteLegIds { get; set; }
	public bool GetRatesForCosts { get; set; }
	public bool GetRatesForSales { get; set; }
}
