//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffUOMValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffUOMValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffUOMValidation : AutoRefCusTariffUOMValidation
	{
		public RefCusTariffUOMValidation(AutoRefCusTariffUOM parent) : base(parent)
		{
		}
	}
}
