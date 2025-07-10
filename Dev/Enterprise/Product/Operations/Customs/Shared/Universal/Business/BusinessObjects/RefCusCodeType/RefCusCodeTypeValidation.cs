//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusCodeTypeValidation
//
//    This class should be used for overriding validation in AutoRefCusCodeTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusCodeTypeValidation : AutoRefCusCodeTypeValidation
	{
		public RefCusCodeTypeValidation(AutoRefCusCodeType parent) : base(parent)
		{
		}
	}
}
