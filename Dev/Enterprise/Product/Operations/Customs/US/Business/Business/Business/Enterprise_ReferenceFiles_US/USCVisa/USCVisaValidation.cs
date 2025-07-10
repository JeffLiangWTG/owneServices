//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCVisaValidation
//
//    This class should be used for overriding validation in AutoUSCVisaValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCVisaValidation : AutoUSCVisaValidation
	{
		public USCVisaValidation(AutoUSCVisa parent)
			: base(parent)
		{
		}
	}
}
