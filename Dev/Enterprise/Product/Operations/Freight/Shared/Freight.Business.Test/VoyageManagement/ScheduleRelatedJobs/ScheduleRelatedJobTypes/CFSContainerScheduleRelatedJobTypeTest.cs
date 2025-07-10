using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CFSContainerScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.PackContainerRegistration;

		protected override string ExpectedBizOType => "Enterprise.Freight.CFS.Business.CFSContainer";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new CFSContainerScheduleRelatedJobType(code, description);
	}
}
