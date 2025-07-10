using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaPackCollectionSynchroniser : ASYCUDA.Business.AsycudaPackCollectionSynchroniser
	{
		public AsycudaPackCollectionSynchroniser(ForwardingShipment source, ASYCUDA.Business.AsycudaBill destination) : base(source, destination)
		{
		}

		protected override ASYCUDA.Business.AsycudaPackSynchroniser GetAsycudaPackSynchroniser(ASYCUDA.Business.AsycudaPack asycudaPack, PackLine sourcePack)
		{
			return asycudaPack.PK == Destination.Packs.OfType<AsycudaPack>().FirstOrDefault(ap => !ap.IsDeleted)?.PK ? new AsycudaPackSynchroniser(asycudaPack, sourcePack) : base.GetAsycudaPackSynchroniser(asycudaPack, sourcePack);
		}
	}
}
