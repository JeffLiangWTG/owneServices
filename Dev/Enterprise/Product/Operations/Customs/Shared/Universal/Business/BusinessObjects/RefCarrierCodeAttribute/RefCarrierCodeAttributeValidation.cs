//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCarrierCodeAttributeValidation
//
//    This class should be used for overriding validation in AutoRefCarrierCodeAttributeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCarrierCodeAttributeValidation : AutoRefCarrierCodeAttributeValidation
	{
		public RefCarrierCodeAttributeValidation(AutoRefCarrierCodeAttribute parent)
			: base(parent)
		{
		}
	}
}
