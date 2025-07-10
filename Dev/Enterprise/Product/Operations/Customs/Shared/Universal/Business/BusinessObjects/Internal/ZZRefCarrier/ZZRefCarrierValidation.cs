//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCarrierValidation
//
//    This class should be used for overriding validation in AutoZZRefCarrierValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal.Internal
{
	public class ZZRefCarrierValidation : AutoZZRefCarrierValidation
	{
		public ZZRefCarrierValidation(AutoZZRefCarrier parent)
			: base(parent)
		{
		}
	}
}
