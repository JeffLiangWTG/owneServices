//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProfileTypeValidation
//
//    This class should be used for overriding validation in AutoRefCusProfileTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileTypeValidation : AutoRefCusProfileTypeValidation
	{
		public RefCusProfileTypeValidation(AutoRefCusProfileType parent) : base(parent)
		{
		}
	}
}
