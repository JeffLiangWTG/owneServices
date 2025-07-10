//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNOAddInfoValidation
//
//    This class should be used for overriding validation in AutoNOAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NO.Business
{
	public class NOAddInfoValidation : AutoNOAddInfoValidation
	{
		public NOAddInfoValidation(AutoNOAddInfo parent) : base(parent)
		{
		}
	}
}
