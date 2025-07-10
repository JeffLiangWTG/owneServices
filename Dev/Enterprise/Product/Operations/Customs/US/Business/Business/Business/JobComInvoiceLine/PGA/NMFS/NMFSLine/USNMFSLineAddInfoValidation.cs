//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNMFSLineAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSNMFSLineAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USNMFSLineAddInfoValidation : AutoUSNMFSLineAddInfoValidation
	{
		public USNMFSLineAddInfoValidation(AutoUSNMFSLineAddInfo parent)
			: base(parent)
		{
		}
	}
}
