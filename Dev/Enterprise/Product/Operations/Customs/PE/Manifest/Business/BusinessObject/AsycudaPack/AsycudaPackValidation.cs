using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.PE.Manifest.Business
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

			if (!Parent.APA_PackUQ.IsEmpty)
			{
				AsycudaUniversalReference.CusRefPackLoaderHelper.MessageErrorIfNeeded(Core.Constants.CountryCodes.Peru, Parent.APA_PackUQ, Parent.APA_PackUQInfo, Parent.Factory);
			}
			if (!Parent.APA_PackQty.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.APA_PackUQInfo);
			}
		}

		protected override void CheckAPA_WeightUQ()
		{
			base.CheckAPA_WeightUQ();
			if (!Parent.APA_Weight.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.APA_WeightUQInfo);
			}
		}

		protected override void CheckAPA_VolumeUQ()
		{
			base.CheckAPA_VolumeUQ();
			if (!Parent.APA_Volume.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.APA_VolumeUQInfo);
			}
		}

		protected override void CheckAPA_GoodsDescription()
		{
			base.CheckAPA_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_GoodsDescriptionInfo);
		}

		protected override void CheckLinePrice()
		{
			base.CheckLinePrice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.LinePriceInfo);
		}

		protected override void CheckAPA_Volume()
		{
			base.CheckAPA_Volume();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_VolumeInfo);
		}

		protected override void CheckAPA_Weight()
		{
			base.CheckAPA_Weight();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_WeightInfo);
		}

		protected override void CheckAPA_PackQty()
		{
			base.CheckAPA_PackQty();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_PackQtyInfo);
		}
	}
}
