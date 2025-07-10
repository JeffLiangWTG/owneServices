//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTaxOrFeeValidation
//
//    This class should be used for overriding validation in AutoRefCusTaxOrFeeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTaxOrFeeValidation : AutoRefCusTaxOrFeeValidation
	{
		public RefCusTaxOrFeeValidation(AutoRefCusTaxOrFee parent)
			: base(parent)
		{
		}
	}
}
