//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusRateTypeValidation
//
//    This class should be used for overriding validation in AutoRefCusRateTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusRateTypeValidation : AutoRefCusRateTypeValidation
	{
		public RefCusRateTypeValidation(AutoRefCusRateType parent) : base(parent)
		{
		}
	}
}
