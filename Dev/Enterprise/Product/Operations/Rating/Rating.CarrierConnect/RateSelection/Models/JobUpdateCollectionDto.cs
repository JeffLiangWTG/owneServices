using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models;

public class JobUpdateCollectionDto
{
	[JsonIgnore]
	public List<JobUpdateDto> JobUpdates { get; set; }

	List<JobUpdateDto> updatesRequiringConfirmation;
	public List<JobUpdateDto> UpdatesRequiringConfirmation => updatesRequiringConfirmation ??= JobUpdates.Where(update => update.RequiresConfirmation).ToList();

	public bool NeedDuplicatePenaltiesConfirmation { get; set; }
}
