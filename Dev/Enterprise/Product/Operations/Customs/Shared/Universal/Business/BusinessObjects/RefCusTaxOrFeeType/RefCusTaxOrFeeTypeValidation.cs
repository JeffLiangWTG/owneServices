//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTaxOrFeeTypeValidation
//
//    This class should be used for overriding validation in AutoRefCusTaxOrFeeTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTaxOrFeeTypeValidation : AutoRefCusTaxOrFeeTypeValidation
	{
		public RefCusTaxOrFeeTypeValidation(AutoRefCusTaxOrFeeType parent) : base(parent)
		{
		}
	}
}
