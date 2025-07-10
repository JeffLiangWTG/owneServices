//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSPSTLineAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSPSTLineAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USPSTLineAddInfoValidation : AutoUSPSTLineAddInfoValidation
	{
		public USPSTLineAddInfoValidation(AutoUSPSTLineAddInfo parent) : base(parent)
		{
		}
	}
}
