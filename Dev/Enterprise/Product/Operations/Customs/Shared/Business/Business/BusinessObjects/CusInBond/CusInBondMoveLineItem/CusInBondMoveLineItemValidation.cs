//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondMoveLineItemValidation
//
//    This class should be used for overriding validation in AutoCusInBondMoveLineItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusInBondMoveLineItemValidation : AutoCusInBondMoveLineItemValidation
	{
		public CusInBondMoveLineItemValidation(AutoCusInBondMoveLineItem parent)
			: base(parent)
		{
		}

		protected override void CheckBI_WeightIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.BI_WeightInfo, 9, 0);
		}
	}
}
