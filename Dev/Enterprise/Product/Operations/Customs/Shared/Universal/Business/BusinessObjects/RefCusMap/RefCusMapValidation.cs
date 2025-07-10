//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusMapValidation
//
//    This class should be used for overriding validation in AutoRefCusMapValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusMapValidation : AutoRefCusMapValidation
	{
		public RefCusMapValidation(AutoRefCusMap parent) : base(parent)
		{
		}
	}
}
