using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class AgencyBookingScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.AgencyBooking;

		protected override string ExpectedBizOType => "Enterprise.Freight.Agency.Business.AgencyBooking";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new AgencyBookingScheduleRelatedJobType(code, description);
	}
}
