using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.TR.Manifest.Business;

public class AsycudaBillCollectionSynchroniser : ASYCUDA.Business.AsycudaBillCollectionSynchroniser
{
	public AsycudaBillCollectionSynchroniser(AsycudaManifestHeader header)
		: base(header)
	{
	}

	protected override ManifestBillSynchroniser<ASYCUDA.Business.AsycudaBill> GetNewManifestBillSynchroniser(ASYCUDA.Business.AsycudaBill destination, ForwardingShipment source)
	{
		return new AsycudaBillSynchroniser((AsycudaBill)destination, source);
	}
}
