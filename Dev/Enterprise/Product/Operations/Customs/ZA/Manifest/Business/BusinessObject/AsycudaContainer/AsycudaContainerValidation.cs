using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaContainerValidation : ASYCUDA.Business.AsycudaContainerValidation
	{
		public AsycudaContainerValidation(AsycudaContainer parent)
			: base(parent)
		{
		}

		protected new AsycudaContainer Parent => (AsycudaContainer)base.Parent;

		protected override void CheckACN_GoodsWeight()
		{
			base.CheckACN_GoodsWeight();
			MandatoryValidation.MessageErrorIfIsZero(Parent.ACN_GoodsWeightInfo);
		}

		protected override void CheckACN_SealType1()
		{
			base.CheckACN_SealType1();
			ValidateSealType(Parent.ACN_SealType1Info, Parent.ACN_Seal1);
		}

		protected override void CheckACN_SealType2()
		{
			base.CheckACN_SealType2();
			ValidateSealType(Parent.ACN_SealType2Info, Parent.ACN_Seal2);
		}

		protected override void CheckACN_SealType3()
		{
			base.CheckACN_SealType3();
			ValidateSealType(Parent.ACN_SealType3Info, Parent.ACN_Seal3);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLandedPurpose();
		}

		void ValidateSealType(ZPropertyInfo info, ZString sealNo)
		{
			if (Parent.Header.IsContainerized && !(sealNo.IsEmpty || sealNo == ZA.Business.MessageBuilders.COSTCO.Contants.SealNumber.NoSealNo))
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
		}

		public void ValidateLandedPurpose()
		{
			ValidateCalculatedProperty(Parent.LandedPurposeInfo);
		}

		protected void CheckLandedPurpose()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.LandedPurposeInfo);
		}
	}
}
