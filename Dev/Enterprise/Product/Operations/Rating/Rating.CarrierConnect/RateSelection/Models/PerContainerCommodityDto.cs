#nullable enable
namespace Enterprise.Rating.CarrierConnect.RateSelection.Models;

public record PerContainerCommodityDto
{
	public string? Container { get; init; }

	public string? Commodity { get; init; }

	public CarrierCommodityDto? CarrierCommodity { get; init; }

	public string? ContainerQuality { get; init; }

	public string? ChargeableFactor { get; init; }

	public string? Remarks { get; init; }

	public string? AddOn { get; init; }

	public string? RateType { get; init; }

	public string? RateType2 { get; init; }
}
