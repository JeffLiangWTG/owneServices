using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.ManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}

		protected override void CheckAMA_OA_CarrierMandatory()
		{
			if (Parent.AMA_ManifestType == NZManifestTypes.Codes.OCR)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_CarrierInfo);
			}
			else
			{
				base.CheckAMA_OA_CarrierMandatory();
			}
		}

		protected override void CheckAMA_CustomsOffice()
		{
		}
	}
}
