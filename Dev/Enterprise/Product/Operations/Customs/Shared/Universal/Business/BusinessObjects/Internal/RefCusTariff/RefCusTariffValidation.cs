//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffValidation : AutoRefCusTariffValidation
	{
		public RefCusTariffValidation(AutoRefCusTariff parent) : base(parent)
		{
		}
	}
}
