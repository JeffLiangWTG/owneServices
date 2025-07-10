//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProfileAttributeValidation
//
//    This class should be used for overriding validation in AutoRefCusProfileAttributeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileAttributeValidation : AutoRefCusProfileAttributeValidation
	{
		public RefCusProfileAttributeValidation(AutoRefCusProfileAttribute parent) : base(parent)
		{
		}
	}
}
