using CargoWise.Types;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsAsycudaPacksCore => ZBool.True;
	}
}
