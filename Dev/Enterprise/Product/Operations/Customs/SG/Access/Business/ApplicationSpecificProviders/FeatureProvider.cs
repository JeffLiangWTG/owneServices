using CargoWise.Types;

namespace Enterprise.Customs.SG.Access.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportsCustomsPortsCore(ASYCUDA.Business.AsycudaManifestHeader header) => true;

		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
