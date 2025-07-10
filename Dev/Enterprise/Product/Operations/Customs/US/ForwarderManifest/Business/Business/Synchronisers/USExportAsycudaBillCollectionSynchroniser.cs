using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaBillCollectionSynchroniser : ASYCUDA.Business.AsycudaBillCollectionSynchroniser
	{
		public USExportAsycudaBillCollectionSynchroniser(USExportAsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override Customs.Business.ManifestBillSynchroniser<ASYCUDA.Business.AsycudaBill> GetNewManifestBillSynchroniser(ASYCUDA.Business.AsycudaBill destination, ForwardingShipment source)
		{
			return new USExportAsycudaBillSynchroniser((USExportAsycudaBill)destination, source);
		}
	}
}
