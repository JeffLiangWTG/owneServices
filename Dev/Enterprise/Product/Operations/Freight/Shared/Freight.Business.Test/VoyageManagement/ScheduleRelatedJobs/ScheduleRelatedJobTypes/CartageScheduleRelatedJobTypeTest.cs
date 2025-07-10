using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CartageScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.Cartage;

		protected override string ExpectedBizOType => "Enterprise.Freight.LocalCartage.Business.CommonCartage";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new CartageScheduleRelatedJobType(code, description);
	}
}
