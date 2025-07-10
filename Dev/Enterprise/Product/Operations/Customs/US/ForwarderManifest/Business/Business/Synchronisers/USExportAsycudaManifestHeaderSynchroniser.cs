using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public USExportAsycudaManifestHeaderSynchroniser(USExportAsycudaManifestHeader destination, ForwardingConsol sourceConsol)
			: base(destination, sourceConsol)
		{
		}

		protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser()
		{
			return new USExportAsycudaBillCollectionSynchroniser((USExportAsycudaManifestHeader)Destination);
		}

		protected override void SynchronisersMasterBOL()
		{
			if (Destination != null && Destination.IsConsolidator)
			{
				base.Synchronisers.Add(new FieldSynchroniser(Destination.MasterBOLInfo, Source.JK_MasterBillNumInfo));
			}
		}
	}
}
