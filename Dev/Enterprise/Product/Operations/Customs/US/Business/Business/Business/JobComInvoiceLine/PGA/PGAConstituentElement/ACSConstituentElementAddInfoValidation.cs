using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	class ACSConstituentElementAddInfoValidation : USConstituentElementAddInfoValidation
	{
		public ACSConstituentElementAddInfoValidation(ConstituentElementAddInfo parent) : base(parent)
		{
		}

		protected override bool IsNameRequired
		{
			get { return Parent.US_PGANameOfTheConstituentElement.IsEmpty; }
		}

		protected override void CheckUS_PGAPercentOfConstituentElementIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.US_PGAPercentOfConstituentElementInfo, 6, 3);
		}
	}
}
