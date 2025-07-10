using CargoWise.Types;

namespace Enterprise.Customs.NO.Manifest.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsCustomsPortsCore(ASYCUDA.Business.AsycudaManifestHeader header) => header.IsSea;

		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
