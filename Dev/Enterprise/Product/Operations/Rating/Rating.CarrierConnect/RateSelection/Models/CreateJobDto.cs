#nullable enable
using System;
using System.Collections.Generic;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models;

public class CreateJobDto
{
	public RateQueryDto RateQuery { get; set; } = null!;

	public RateResultDto RateResult { get; set; } = null!;

	public List<Guid> ChargesToApply { get; set; } = [];

	public bool ApplyZeroCharges { get; set; }
}
