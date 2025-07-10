//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffAttributeNameValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffAttributeNameValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffAttributeNameValidation : AutoRefCusTariffAttributeNameValidation
	{
		public RefCusTariffAttributeNameValidation(AutoRefCusTariffAttributeName parent)
			: base(parent)
		{
		}
	}
}
