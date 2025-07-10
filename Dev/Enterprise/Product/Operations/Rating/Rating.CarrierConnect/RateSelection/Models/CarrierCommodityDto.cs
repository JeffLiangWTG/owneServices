#nullable enable
using System.Linq;
using WiseRates.Api.Model;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models;

public class CarrierCommodityDto
{
	public CarrierCommodityDto() { }

	public CarrierCommodityDto(CarrierSpecificCommodity commodity, string transportMode)
	{
		Code = commodity.Code;
		GroupName = commodity.GroupName;
		GroupType = commodity.GroupType;
		Commodities = transportMode == TransportModes.Air ? commodity.IncludedCommodities.ToArray() : [];
		IncludedCommodities = transportMode == TransportModes.Sea ? commodity.IncludedCommodities.ToArray() : [];
		ExcludedCommodities = transportMode == TransportModes.Sea ? commodity.ExcludedCommodities.ToArray() : [];
	}

	public string Code { get; init; } = null!;
	public string? GroupName { get; init; }
	public string? GroupType { get; init; }
	public string[]? Commodities { get; init; }
	public string[]? IncludedCommodities { get; init; }
	public string[]? ExcludedCommodities { get; init; }
}
