//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCarrierCodeValidation
//
//    This class should be used for overriding validation in AutoRefCarrierCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCarrierCodeValidation : AutoRefCarrierCodeValidation
	{
		public RefCarrierCodeValidation(AutoRefCarrierCode parent)
			: base(parent)
		{
		}
	}
}
