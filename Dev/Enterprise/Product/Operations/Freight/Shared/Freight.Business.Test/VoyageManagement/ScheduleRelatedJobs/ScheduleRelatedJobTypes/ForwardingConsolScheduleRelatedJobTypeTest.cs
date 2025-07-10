using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ForwardingConsolScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.JobConsol;

		protected override string ExpectedBizOType => "Enterprise.Freight.Forwarding.Business.ForwardingConsol";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new ForwardingConsolScheduleRelatedJobType(code, description);
	}
}
