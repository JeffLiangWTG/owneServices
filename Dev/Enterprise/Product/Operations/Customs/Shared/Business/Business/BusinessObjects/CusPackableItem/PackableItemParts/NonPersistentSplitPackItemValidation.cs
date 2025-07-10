using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class NonPersistentSplitPackItemValidation : AutoNonPersistentSplitPackItemValidation
	{
		public NonPersistentSplitPackItemValidation(AutoNonPersistentSplitPackItem parent)
			: base(parent)
		{
		}

		protected override void CheckGoodsDescription()
		{
			base.CheckGoodsDescription();
			MandatoryValidation.CheckEntered(Parent.GoodsDescriptionInfo);
		}

		protected override void CheckPackableQuantity()
		{
			base.CheckPackableQuantity();
			var targetInfo = Parent.PackableQuantityInfo;
			MandatoryValidation.WarnIfIsZero(targetInfo);
			MandatoryValidation.CheckNotNegative(targetInfo);
		}

		protected override void CheckPackableUQ()
		{
			base.CheckPackableUQ();
			var targetInfo = Parent.PackableUQInfo;
			MandatoryValidation.WarnIfNotEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}

		protected override void CheckNetWeight()
		{
			base.CheckNetWeight();
			var targetInfo = Parent.NetWeightInfo;
			MandatoryValidation.WarnIfIsZero(targetInfo);
			MandatoryValidation.CheckNotNegative(targetInfo);
		}

		protected override void CheckNetWeightUQ()
		{
			base.CheckNetWeightUQ();
			var targetInfo = Parent.NetWeightUQInfo;
			MandatoryValidation.WarnIfNotEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}

		new NonPersistentSplitPackItem Parent => (NonPersistentSplitPackItem)base.Parent;
	}
}
