//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFWSHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSFWSHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USFWSHeaderAddInfoValidation : AutoUSFWSHeaderAddInfoValidation
	{
		public USFWSHeaderAddInfoValidation(AutoUSFWSHeaderAddInfo parent)
			: base(parent)
		{
		}
	}
}
