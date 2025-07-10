//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSITDocAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSITDocAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USITDocAddInfoValidation : AutoUSITDocAddInfoValidation
	{
		public USITDocAddInfoValidation(AutoUSITDocAddInfo parent)
			: base(parent)
		{
		}
	}
}
