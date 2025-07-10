//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCFIRMSValidation
//
//    This class should be used for overriding validation in AutoUSCFIRMSValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCFIRMSValidation : AutoUSCFIRMSValidation
	{
		public USCFIRMSValidation(AutoUSCFIRMS parent)
			: base(parent)
		{
		}
	}
}
