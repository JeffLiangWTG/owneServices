using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CFSShipmentScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.ShipmentReceival;

		protected override string ExpectedBizOType => "Enterprise.Freight.CFS.Business.CFSShipment";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new CFSShipmentScheduleRelatedJobType(code, description);
	}
}
