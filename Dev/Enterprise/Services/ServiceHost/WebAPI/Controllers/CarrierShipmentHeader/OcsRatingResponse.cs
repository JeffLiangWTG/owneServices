using System.Collections.Generic;
using Enterprise.Rating.Business;

namespace Enterprise.Services.ServiceHost;

public sealed class OcsRatingResponse
{
	public IEnumerable<string> Errors { get; set; }

	public IEnumerable<CarrierShipmentRateResultDto> Rates { get; set; }

	public IEnumerable<string> Log { get; set; }
}
