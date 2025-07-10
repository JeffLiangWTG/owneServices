using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class NatureAndQtyOfGoodsOriginValidation : NatureAndQtyOfGoodsValidation
	{
		public NatureAndQtyOfGoodsOriginValidation(NatureAndQtyOfGoodsOrigin parent)
			: base(parent)
		{
		}

		protected new NatureAndQtyOfGoodsOrigin Parent
		{
			get { return (NatureAndQtyOfGoodsOrigin)base.Parent; }
		}

		protected override bool IsValidationApplicable
		{
			get { return Parent.ParentRateLine.ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateCountry();
		}

		protected override void CheckText()
		{
		}

		public void ValidateCountry()
		{
			ValidateCalculatedProperty(Parent.CountryInfo);
		}

		protected virtual void CheckCountry()
		{
			if (IsValidationApplicable)
			{
				MandatoryValidation.CheckEntered(Parent.CountryInfo);
				ListValidation.ErrorIfInvalidCode(Parent.CountryInfo, Parent.CountryList);
			}
		}
	}
}
