using CargoWise.Types;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
