using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class FeatureProvider : ASYCUDA.Business.FeatureProvider
	{
		protected override ZBool SupportArrivalInformationCore(ASYCUDA.Business.AsycudaManifestHeader header) => true;
		protected override ZBool SupportArrivalTransfersCore => true;
		protected override ZBool SupportsAsycudaPacksCore => true;
	}
}
