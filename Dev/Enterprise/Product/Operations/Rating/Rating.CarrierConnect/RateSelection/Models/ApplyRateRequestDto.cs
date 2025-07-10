#nullable enable
using System;
using System.Collections.Generic;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models
{
	public class ApplyRateRequestDto
	{
		public Guid RateId { get; set; }

		public DateTime? AutoratingDate { get; set; }

		public List<ApplyJobChangeDto> JobChangesAccepted { get; set; } = [];

		public bool ApplyDuplicatePenalties { get; set; }

		public List<Guid> ChargesToApply { get; set; } = [];

		public bool ApplyZeroCharges { get; set; }
	}

	public class ApplyJobChangeDto
	{
		public JobConfirmationType Type { get; set; }

		public bool Accepted { get; set; }
	}
}
