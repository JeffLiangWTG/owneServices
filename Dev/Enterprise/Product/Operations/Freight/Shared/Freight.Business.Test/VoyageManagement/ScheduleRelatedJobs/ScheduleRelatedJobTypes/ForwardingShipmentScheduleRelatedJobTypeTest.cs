using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ForwardingShipmentScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.JobShipment;

		protected override string ExpectedBizOType => "Enterprise.Freight.Forwarding.Business.ForwardingShipment";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new ForwardingShipmentScheduleRelatedJobType(code, description);
	}
}
