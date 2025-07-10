using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DeclarationScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.Customs.JobDeclaration;

		protected override string ExpectedBizOType => "Enterprise.Customs.Business.BaseJobDeclaration";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new DeclarationScheduleRelatedJobType(code, description);
	}
}
