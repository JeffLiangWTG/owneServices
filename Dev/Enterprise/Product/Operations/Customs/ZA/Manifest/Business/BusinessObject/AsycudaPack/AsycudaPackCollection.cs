using Enterprise.Customs.Universal.Messaging.CUSCAR;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaPackCollection : ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>
	{
		public AsycudaPackCollection(AsycudaBill master)
			: base(master)
		{
		}

		protected override bool ShouldDefaultContainerPK(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var result = base.ShouldDefaultContainerPK(header);
			if (!result)
			{
				result = header != null && (header.AMA_ManifestType == nameof(ManifestDocumentType.COM)
					|| header.AMA_ManifestType == nameof(ManifestDocumentType.COH));
			}
			return result;
		}
	}
}
