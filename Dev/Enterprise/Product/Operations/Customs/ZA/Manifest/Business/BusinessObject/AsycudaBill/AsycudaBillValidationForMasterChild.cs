using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		protected override void CheckABL_E_DEP()
		{
			var manifestType = Header?.AMA_ManifestType ?? ZString.Empty;
			if (manifestType != nameof(ManifestDocumentType.RFM))
			{
				base.CheckABL_E_DEP();
			}
		}
	}
}
