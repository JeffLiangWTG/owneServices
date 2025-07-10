//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffAdditionalCodeCategoryValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffAdditionalCodeCategoryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTariffAdditionalCodeCategoryValidation : AutoRefCusTariffAdditionalCodeCategoryValidation
	{
		public RefCusTariffAdditionalCodeCategoryValidation(AutoRefCusTariffAdditionalCodeCategory parent)
			: base(parent)
		{
		}
	}
}
