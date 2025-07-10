using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BillOfLadingScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.AgencyBillOfLading;

		protected override string ExpectedBizOType => "Enterprise.Freight.Agency.Business.BillOfLading";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new BillOfLadingScheduleRelatedJobType(code, description);
	}
}
