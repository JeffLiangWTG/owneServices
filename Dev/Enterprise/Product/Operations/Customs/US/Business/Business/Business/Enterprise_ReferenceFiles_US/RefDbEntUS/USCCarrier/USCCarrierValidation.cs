//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCCarrierValidation
//
//    This class should be used for overriding validation in AutoUSCCarrierValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business.RefDbEntUS
{
	public class USCCarrierValidation : AutoUSCCarrierValidation
	{
		public USCCarrierValidation(AutoUSCCarrier parent) : base(parent)
		{
		}
	}
}
