//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffAdditionalCodeValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffAdditionalCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffAdditionalCodeValidation : AutoRefCusTariffAdditionalCodeValidation
	{
		public RefCusTariffAdditionalCodeValidation(AutoRefCusTariffAdditionalCode parent)
			: base(parent)
		{
		}
	}
}
