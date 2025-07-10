using WiseRates.Api.Model;

namespace Enterprise.Rating.Business;

public class UrsSchedule : Schedule
{
	public string ExternalPriceReference { get; set; }

	public string ScheduleId { get; set; }
}
