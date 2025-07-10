using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Messaging.CUSCAR;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaPackValidation : ASYCUDA.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(AsycudaPack parent)
			: base(parent)
		{
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;
		protected override void CheckContainerPK()
		{
			base.CheckContainerPK();
			var pivot = Parent.Pivot;
			if (pivot == null)
			{
				var header = Parent.Bill?.Header;
				if (header != null && (header.AMA_ManifestType == nameof(ManifestDocumentType.COM) || header.AMA_ManifestType == nameof(ManifestDocumentType.COH)))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ContainerPKInfo);
				}
			}
		}

		protected override bool IsWeightRequired() => true;
		protected override bool IsPackQtyRequired() => true;
	}
}
