//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProfileValidation
//
//    This class should be used for overriding validation in AutoRefCusProfileValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileValidation : AutoRefCusProfileValidation
	{
		public RefCusProfileValidation(AutoRefCusProfile parent) : base(parent)
		{
		}
	}
}
