#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models;

public class CalculatorDto
{
	public CalculatorDto() { }

	public CalculatorDto(IRateLine rateLine)
	{
		Id = rateLine.PK.ToGuid();
		RateLineItems = rateLine.ChildRateLineItems.Select(item => new RateLineItemDto(item));
		CalculatorCode = rateLine.TL_RateCalculator;
		Currency = rateLine.TL_RX_NKCurrency;
		WeightVolumeUnit = rateLine.TL_WeightVolume;
		Tags = GetCalculatorTags(rateLine.Calculator);
	}

	public Guid Id { get; init; }
	public string CalculatorCode { get; init; } = string.Empty;
	public string Currency { get; init; } = string.Empty;
	public string WeightVolumeUnit { get; init; } = string.Empty;
	public IEnumerable<RateLineItemDto> RateLineItems { get; init; } = [];
	public IEnumerable<string> Tags { get; init; } = [];

	static List<string> GetCalculatorTags(Calculator calculator)
	{
		var tags = new List<string>();

		if (calculator.IsAccumulated)
		{
			tags.Add(Res.GetString("8ec18e57-9fc9-4f5c-b707-fabfd1e787e6", "Cumulative breaks"));
		}

		if (calculator is BaseCombinedCalculator combinedCalculator && combinedCalculator.UseHigherChargeableLowerRateRule)
		{
			tags.Add(Res.GetString("dafa1d52-d371-4e7e-9066-0b26026178b4", "Higher break lower rate"));
		}

		if (calculator.UseInclusiveBreaks)
		{
			tags.Add(Res.GetString("bb4c12be-1afc-475e-8300-b87abed26643", "Inclusive breaks"));
		}

		if (calculator.BreaksPer.EqualsIgnoringCase("CTT"))
		{
			tags.Add(Res.GetString("ee9f18fe-b585-4dc4-9f69-7ede1d6cb9c9", "Breaks per container type/class"));
		}
		else if (calculator.BreaksPer.EqualsIgnoringCase("CTN"))
		{
			tags.Add(Res.GetString("bb7a977e-4ecb-463a-b341-96d3c385a1b8", "Breaks per container"));
		}

		return tags;
	}

	public override string ToString() => string.Join("|",
		Id,
		CalculatorCode,
		Currency,
		WeightVolumeUnit,
		string.Join("_", Tags));
}

public class RateLineItemDto
{
	public RateLineItemDto() { }

	public RateLineItemDto(IRateLineItem item)
	{
		Operator = item.TM_Type;
		Break = item.TM_Break;
		BreakUnit = item.TM_BreakWeightVolume;
		Rate = item.TM_RelevantValue;
		FlatAmount = item.TM_FlatAmount;
		Reason = item.TM_Text;
		Restrict = item.TM_CallForPricing;
	}

	public string Operator { get; init; } = string.Empty;
	public decimal Break { get; init; }
	public string BreakUnit { get; init; } = string.Empty;
	public decimal Rate { get; init; }
	public decimal FlatAmount { get; init; }
	public bool Restrict { get; init; }
	public string Reason { get; init; } = string.Empty;

	public override string ToString() => string.Join("|",
		Operator,
		Break.ToString("0.##"),
		BreakUnit,
		Rate.ToString("0.##"),
		FlatAmount.ToString("0.##"),
		Restrict,
		Reason);
}
