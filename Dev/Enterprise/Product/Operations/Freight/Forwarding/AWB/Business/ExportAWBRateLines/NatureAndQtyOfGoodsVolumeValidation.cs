using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class NatureAndQtyOfGoodsVolumeValidation : NatureAndQtyOfGoodsValidation
	{
		public NatureAndQtyOfGoodsVolumeValidation(NatureAndQtyOfGoodsVolume parent)
			: base(parent)
		{
		}

		protected new NatureAndQtyOfGoodsVolume Parent
		{
			get { return (NatureAndQtyOfGoodsVolume)base.Parent; }
		}

		protected override bool IsValidationApplicable
		{
			get { return Parent.ParentRateLine.ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateVolume();
			ValidateUnit();
		}

		protected override void CheckText()
		{
		}

		public void ValidateVolume()
		{
			ValidateCalculatedProperty(Parent.VolumeInfo);
		}

		protected virtual void CheckVolume()
		{
			if (IsValidationApplicable)
			{
				CompareValidation.CheckWithinRange(Parent.VolumeInfo, 0.01m, 99999.999m);
			}
		}

		public void ValidateUnit()
		{
			ValidateCalculatedProperty(Parent.UnitInfo);
		}

		protected virtual void CheckUnit()
		{
			if (IsValidationApplicable)
			{
				MandatoryValidation.CheckEntered(Parent.UnitInfo);
				ListValidation.ErrorIfInvalidCode(Parent.UnitInfo, Parent.UnitList);
			}
		}
	}
}
