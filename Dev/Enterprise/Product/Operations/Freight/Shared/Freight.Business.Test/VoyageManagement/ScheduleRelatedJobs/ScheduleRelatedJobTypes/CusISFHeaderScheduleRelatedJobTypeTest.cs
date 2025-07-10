using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CusISFHeaderScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.ImporterSecurityFiling;

		protected override string ExpectedBizOType => "Enterprise.Customs.US.ISF.Business.CusISFHeader";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new CusISFHeaderScheduleRelatedJobType(code, description);
	}
}
