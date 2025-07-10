using CargoWise.Types;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
