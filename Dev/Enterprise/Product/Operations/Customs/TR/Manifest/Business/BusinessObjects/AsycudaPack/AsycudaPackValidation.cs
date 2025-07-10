using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaPackValidation : ASYCUDA.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(AsycudaPack parent)
			: base(parent)
		{
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;

		protected override void CheckAPA_PackUQ()
		{
			base.CheckAPA_PackUQ();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_PackUQInfo);
		}

		protected override bool IsPackQtyRequired() => true;

		protected override bool IsWeightRequired() => true;
	}
}
