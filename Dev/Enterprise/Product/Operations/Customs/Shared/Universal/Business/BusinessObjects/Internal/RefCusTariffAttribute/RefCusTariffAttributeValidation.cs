//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffAttributeValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffAttributeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffAttributeValidation : AutoRefCusTariffAttributeValidation
	{
		public RefCusTariffAttributeValidation(AutoRefCusTariffAttribute parent)
			: base(parent)
		{
		}
	}
}
