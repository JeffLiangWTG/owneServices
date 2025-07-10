using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.Customs.Business
{
	public class CusPackageCusPackableItemRelationValidation : AutoCusPackageCusPackableItemRelationValidation
	{
		public CusPackageCusPackableItemRelationValidation(AutoCusPackageCusPackableItemRelation parent) : base(parent)
		{
		}

		public new CusPackageCusPackableItemRelation Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (CusPackageCusPackableItemRelation)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePackedQty();
			ValidateNetWeightUQ();
			ValidateNetWeight();
		}

		public void ValidatePackedQty()
		{
			ValidateCalculatedProperty(Parent.PackedQtyInfo);
		}

		public void ValidateNetWeightUQ()
		{
			ValidateCalculatedProperty(Parent.NetWeightUQInfo);
		}

		public void ValidateNetWeight()
		{
			ValidateCalculatedProperty(Parent.NetWeightInfo);
		}

		protected void CheckPackedQty()
		{
			if (!Parent.PackableItem.IsDeleted)
			{
				var targetInfo = Parent.PackedQtyInfo;
				var totalPackedQty = Parent.TotalPackedQty;
				var packableQty = Parent.PackableQuantity;
				MandatoryValidation.CheckNotNegative(targetInfo);

				if (totalPackedQty > packableQty)
				{
					targetInfo.AddError(Res.GetString("5D060511-BA11-418C-9804-0450894CA460", "The total packed quantity {0} cannot exceed the packable quantity {1}.", totalPackedQty, packableQty));
				}

				if (Parent.IsPacked)
				{
					CompareValidation.CheckNumberGreaterThanZero(targetInfo);
				}
			}
		}

		protected void CheckNetWeightUQ()
		{
			if (Parent.PackageItemDivot is PkgPackageItemDivot)
			{
				ListValidation.WarnIfInvalidCode(Parent.NetWeightUQInfo);
			}
		}

		protected void CheckNetWeight()
		{
			if (Parent.PackageItemDivot is PkgPackageItemDivot)
			{
				MandatoryValidation.CheckNotNegative(Parent.NetWeightInfo);
			}
		}
	}
}
