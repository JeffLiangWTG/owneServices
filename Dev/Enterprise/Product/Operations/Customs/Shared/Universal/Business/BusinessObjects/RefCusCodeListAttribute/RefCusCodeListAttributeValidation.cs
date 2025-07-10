//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusCodeListAttributeValidation
//
//    This class should be used for overriding validation in AutoRefCusCodeListAttributeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusCodeListAttributeValidation : AutoRefCusCodeListAttributeValidation
	{
		public RefCusCodeListAttributeValidation(AutoRefCusCodeListAttribute parent) : base(parent)
		{
		}
	}
}
