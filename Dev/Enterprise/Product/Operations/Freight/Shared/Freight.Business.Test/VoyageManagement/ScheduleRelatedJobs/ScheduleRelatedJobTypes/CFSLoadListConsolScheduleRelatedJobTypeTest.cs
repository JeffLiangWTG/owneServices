using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CFSLoadListConsolScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.LoadListConsol;

		protected override string ExpectedBizOType => "Enterprise.Freight.CFS.Business.CFSLoadListConsol";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new CFSLoadListConsolScheduleRelatedJobType(code, description);
	}
}
