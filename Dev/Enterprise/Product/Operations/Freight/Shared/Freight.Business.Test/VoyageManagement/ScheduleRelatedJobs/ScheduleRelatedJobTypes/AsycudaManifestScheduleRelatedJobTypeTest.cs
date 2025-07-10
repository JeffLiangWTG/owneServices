using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	sealed class AsycudaManifestScheduleRelatedJobTypeTest : ScheduleRelatedJobTypeTest
	{
		protected override ControllerID ExpectedControllerID => ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest;

		protected override string ExpectedBizOType => "Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader";

		protected override ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description) => new AsycudaManifestScheduleRelatedJobType(code, description);
	}
}
