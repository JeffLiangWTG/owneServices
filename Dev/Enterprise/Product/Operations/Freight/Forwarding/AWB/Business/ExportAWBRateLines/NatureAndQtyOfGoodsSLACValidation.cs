using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class NatureAndQtyOfGoodsSLACValidation : NatureAndQtyOfGoodsValidation
	{
		public NatureAndQtyOfGoodsSLACValidation(NatureAndQtyOfGoodsSLAC parent)
			: base(parent)
		{
		}

		protected new NatureAndQtyOfGoodsSLAC Parent
		{
			get { return (NatureAndQtyOfGoodsSLAC)base.Parent; }
		}

		protected override bool IsValidationApplicable
		{
			get { return Parent.ParentRateLine.ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCount();
		}

		protected override void CheckText()
		{
		}

		public void ValidateCount()
		{
			ValidateCalculatedProperty(Parent.CountInfo);
		}

		protected virtual void CheckCount()
		{
			if (IsValidationApplicable)
			{
				CompareValidation.CheckWithinRange(Parent.CountInfo, 1, 99999);
			}
		}
	}
}
