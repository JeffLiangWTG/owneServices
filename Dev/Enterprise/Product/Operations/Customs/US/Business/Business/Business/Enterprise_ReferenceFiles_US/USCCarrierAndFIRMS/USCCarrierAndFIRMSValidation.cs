//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCCarrierAndFIRMSValidation
//
//    This class should be used for overriding validation in AutoUSCCarrierAndFIRMSValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCCarrierAndFIRMSValidation : AutoUSCCarrierAndFIRMSValidation
	{
		public USCCarrierAndFIRMSValidation(AutoUSCCarrierAndFIRMS parent)
			: base(parent)
		{
		}
	}
}
