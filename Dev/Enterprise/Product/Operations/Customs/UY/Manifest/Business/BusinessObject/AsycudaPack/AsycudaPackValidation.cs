using CargoWise.EntityFramework;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class AsycudaPackValidation : ASYCUDA.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(AsycudaPack parent)
			: base(parent)
		{
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateArrivedQuantity();
			ValidateArrivedWeight();
		}

		public void ValidateArrivedQuantity()
		{
			ValidateCalculatedProperty(Parent.APA_ArrivedQuantityInfo);
		}

		public void ValidateArrivedWeight()
		{
			ValidateCalculatedProperty(Parent.APA_ArrivedWeightInfo);
		}

		protected void CheckAPA_ArrivedQuantity()
		{
			if (Parent.APA_ArrivedQuantity < 0)
			{
				MandatoryValidation.CheckNotNegative(Parent.APA_ArrivedQuantityInfo);
			}

			if (Parent.APA_ArrivedWeight > 0m && Parent.APA_ArrivedQuantity == 0)
			{
				MandatoryValidation.CheckNotZero(Parent.APA_ArrivedQuantityInfo);
			}
		}

		protected void CheckAPA_ArrivedWeight()
		{
			if (Parent.APA_ArrivedWeight < 0m)
			{
				MandatoryValidation.CheckNotNegative(Parent.APA_ArrivedWeightInfo);
			}

			if (Parent.APA_ArrivedQuantity > 0 && Parent.APA_ArrivedWeight == 0m)
			{
				MandatoryValidation.CheckNotZero(Parent.APA_ArrivedWeightInfo);
			}

			TypeValidation.CheckValidDecimal(Parent.APA_ArrivedWeightInfo, AsycudaPack.Schema.APA_ArrivedWeightPrecision, AsycudaPack.Schema.APA_ArrivedWeightScale);
		}

		protected override void CheckAPA_Weight()
		{
			base.CheckAPA_Weight();
			MandatoryValidation.MessageErrorIfIsZero(Parent.APA_WeightInfo);
		}

		protected override void CheckAPA_Volume()
		{
			base.CheckAPA_Volume();
			MandatoryValidation.MessageErrorIfIsZero(Parent.APA_VolumeInfo);
		}

		protected override void CheckAPA_MarksAndNumbers()
		{
			base.CheckAPA_MarksAndNumbers();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_MarksAndNumbersInfo);
		}

		protected override void CheckAPA_GoodsDescription()
		{
			base.CheckAPA_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_GoodsDescriptionInfo);
		}

		protected override void CheckAPA_VolumeUQ()
		{
			base.CheckAPA_VolumeUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.APA_VolumeUQInfo);
		}

		protected override void CheckAPA_PackUQ()
		{
			base.CheckAPA_PackUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.APA_PackUQInfo);
		}
	}
}
