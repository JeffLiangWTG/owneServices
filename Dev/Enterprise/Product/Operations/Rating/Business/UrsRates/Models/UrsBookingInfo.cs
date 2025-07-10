using WiseRates.Api.Model;

namespace Enterprise.Rating.Business;

public class UrsBookingInfo : BookingInfo
{
	public UrsSchedule UrsSchedule => Schedule as UrsSchedule;
}
