#nullable enable
using System;
using Newtonsoft.Json;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models
{
	public class JobUpdateDto(JobConfirmationType type, string? currentValue, string? newValue, Action<ApplyRateRequestDto> commit, bool optional = false, bool requiresConfirmation = true)
	{
		public JobUpdateDto(JobConfirmationType type, string? currentValue, string newValue, Action commit, bool optional = false, bool needDisplay = true)
			: this(type, currentValue, newValue, (_) => commit(), optional, needDisplay) { }

		public JobUpdateDto(JobConfirmationType type, Action commit, bool optional = false, bool needDisplay = true)
			: this(type, (_) => commit(), optional, needDisplay) { }

		public JobUpdateDto(JobConfirmationType type, Action<ApplyRateRequestDto> commit, bool optional = false, bool needDisplay = true)
			: this(type, null, null, commit, optional, needDisplay) { }

		public JobConfirmationType Type { get; set; } = type;

		public string? CurrentValue { get; set; } = currentValue;

		public string? NewValue { get; set; } = newValue;

		public bool IsOptional { get; set; } = optional;

		public bool RequiresConfirmation { get; set; } = requiresConfirmation;

		[JsonIgnore]
		public Action<ApplyRateRequestDto> CommitUpdate { get; set; } = commit;
	}

	public enum JobConfirmationType
	{
		// Job Updates
		Origin,
		Destination,
		Carrier,
		CarrierContractNumber,
		AutoratingDate,
		PaymentTerm,
		ServiceLevel,

		// Spot
		SendSpotBooking,
		SpotPenalties,
		SpotBookingTerms,
		Schedule
	}
}
