using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class QuotedBookingScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.QuotedBookings;

		protected override string ExpectedBizOType => "Enterprise.Freight.QuotedBookings.Business.QuotedBooking";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new QuotedBookingScheduleRelatedJobType(code, description);
	}
}
