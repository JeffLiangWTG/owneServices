#nullable enable
using System;
using System.Collections.Generic;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models;

public class RateSearchResponseDto
{
	public string[] Errors { get; set; } = [];

	public string[] Warnings { get; set; } = [];

	public RateResultDto[] Rates { get; set; } = [];

	public Dictionary<Guid, CalculatorDto> Calculators { get; init; } = [];

	public string Log { get; set; } = string.Empty;

	public string? RawResponse { get; set; }
}
