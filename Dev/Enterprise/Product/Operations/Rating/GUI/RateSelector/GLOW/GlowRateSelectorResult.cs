#nullable enable
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;

namespace Enterprise.Rating.GUI.RateSelector
{
	public record GlowRateSelectorResult(
		GlowRateSelectorOutcome Outcome,
		AutoRateInfoCollection? SelectedCharges = null,
		RateResultDto? Rate = null,
		ApplyRateRequestDto? ApplyRateRequest = null);

	public enum GlowRateSelectorOutcome
	{
		ApplyRates,
		AutorateWithoutSelection,
		AbortSession
	}
}
