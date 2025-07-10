//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefRateUOMViewValidation
//
//    This class should be used for overriding validation in AutoCusRefRateUOMViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class CusRefRateUOMViewValidation : AutoCusRefRateUOMViewValidation
	{
		public CusRefRateUOMViewValidation(AutoCusRefRateUOMView parent) : base(parent)
		{
		}
	}
}
