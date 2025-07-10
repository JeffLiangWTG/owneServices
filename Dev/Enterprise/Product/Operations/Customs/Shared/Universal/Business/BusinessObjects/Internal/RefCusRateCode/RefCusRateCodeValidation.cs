//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusRateCodeValidation
//
//    This class should be used for overriding validation in AutoRefCusRateCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusRateCodeValidation : AutoRefCusRateCodeValidation
	{
		public RefCusRateCodeValidation(AutoRefCusRateCode parent) : base(parent)
		{
		}
	}
}
