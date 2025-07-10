using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentPreAdviceSailingRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.JobShipmentPreplanning;

		protected override string ExpectedBizOType => "Enterprise.Freight.Forwarding.Orders.Business.JobShipmentPreplanning";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new ShipmentPreAdviceSailingRelatedJobType(code, description);
	}
}
