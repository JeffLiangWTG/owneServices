using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DtbBookingScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.DtbBooking;

		protected override string ExpectedBizOType => "Enterprise.TransportBookings.Business.DtbBooking";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new DtbBookingScheduleRelatedJobType(code, description);
	}
}
