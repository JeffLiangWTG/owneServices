//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCarrierValidation
//
//    This class should be used for overriding validation in AutoUSCarrierValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business.Internal
{
	public class USCarrierValidation : AutoUSCarrierValidation
	{
		public USCarrierValidation(AutoUSCarrier parent) : base(parent)
		{
		}
	}
}
