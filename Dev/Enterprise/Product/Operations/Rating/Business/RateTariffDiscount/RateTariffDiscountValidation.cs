//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRateTariffDiscountValidation
//
//    This class should be used for overriding validation in AutoRateTariffDiscountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Rating.Business
{
	public class RateTariffDiscountValidation : AutoRateTariffDiscountValidation
	{
		public RateTariffDiscountValidation(AutoRateTariffDiscount parent)
			: base(parent)
		{
		}
	}
}

