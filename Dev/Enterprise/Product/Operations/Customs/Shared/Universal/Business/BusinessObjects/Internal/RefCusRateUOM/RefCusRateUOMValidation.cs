//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusRateUOMValidation
//
//    This class should be used for overriding validation in AutoRefCusRateUOMValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusRateUOMValidation : AutoRefCusRateUOMValidation
	{
		public RefCusRateUOMValidation(AutoRefCusRateUOM parent) : base(parent)
		{
		}
	}
}
