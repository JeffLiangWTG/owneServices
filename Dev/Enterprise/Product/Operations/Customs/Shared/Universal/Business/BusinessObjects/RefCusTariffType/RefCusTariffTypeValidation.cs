//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffTypeValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTariffTypeValidation : AutoRefCusTariffTypeValidation
	{
		public RefCusTariffTypeValidation(AutoRefCusTariffType parent)
			: base(parent)
		{
		}
	}
}
