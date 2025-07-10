//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusCodeListAttributeNameValidation
//
//    This class should be used for overriding validation in AutoRefCusCodeListAttributeNameValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusCodeListAttributeNameValidation : AutoRefCusCodeListAttributeNameValidation
	{
		public RefCusCodeListAttributeNameValidation(AutoRefCusCodeListAttributeName parent) : base(parent)
		{
		}
	}
}
