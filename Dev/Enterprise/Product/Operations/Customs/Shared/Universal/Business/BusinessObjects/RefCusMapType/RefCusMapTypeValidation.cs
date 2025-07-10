//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusMapTypeValidation
//
//    This class should be used for overriding validation in AutoRefCusMapTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusMapTypeValidation : AutoRefCusMapTypeValidation
	{
		public RefCusMapTypeValidation(AutoRefCusMapType parent) : base(parent)
		{
		}
	}
}
