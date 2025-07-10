// ReSharper disable MemberCanBePrivate.Global - Used in JSON serializing/deserializing.
#nullable enable
using Enterprise.Rating.Business;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models;

public class PayloadDto
{
	public PayloadDto()
	{
	}

	public PayloadDto(WiseEntry entry)
	{
		MaxPayloadWeight = entry.ContainerPayloadWeight;
		MaxPayloadVolume = entry.ContainerPayloadVolume;
		PivotWeight = entry.ContainerPivotWeight;
	}

	public decimal MaxPayloadWeight { get; set; }
	public decimal MaxPayloadVolume { get; set; }
	public decimal PivotWeight { get; set; }

	public override string ToString() => string.Join("|", MaxPayloadWeight, MaxPayloadVolume, PivotWeight);
}
