//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAIILineAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSAIILineAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USAIILineAddInfoValidation : AutoUSAIILineAddInfoValidation
	{
		public USAIILineAddInfoValidation(AutoUSAIILineAddInfo parent)
			: base(parent)
		{
		}
	}
}
