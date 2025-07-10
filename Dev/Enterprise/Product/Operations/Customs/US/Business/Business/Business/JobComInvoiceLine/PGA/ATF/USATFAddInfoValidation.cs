//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSATFAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSATFAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USATFAddInfoValidation : AutoUSATFAddInfoValidation
	{
		public USATFAddInfoValidation(AutoUSATFAddInfo parent) : base(parent)
		{
		}
	}
}
