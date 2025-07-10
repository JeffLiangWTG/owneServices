using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USFeatureProvider : FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
