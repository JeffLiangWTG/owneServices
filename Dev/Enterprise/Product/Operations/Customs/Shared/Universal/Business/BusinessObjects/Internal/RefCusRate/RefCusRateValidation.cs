//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusRateValidation
//
//    This class should be used for overriding validation in AutoRefCusRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusRateValidation : AutoRefCusRateValidation
	{
		public RefCusRateValidation(AutoRefCusRate parent) : base(parent)
		{
		}
	}
}
