using CargoWise.EntityFramework;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected override void CheckAMA_Nature()
		{
			base.CheckAMA_Nature();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_NatureInfo);
		}

		protected override void CheckAMA_CustomsOffice()
		{
			base.CheckAMA_CustomsOffice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_CustomsOfficeInfo);
		}

		protected override void CheckAMA_OA_CarrierMandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_CarrierInfo);
		}
	}
}
