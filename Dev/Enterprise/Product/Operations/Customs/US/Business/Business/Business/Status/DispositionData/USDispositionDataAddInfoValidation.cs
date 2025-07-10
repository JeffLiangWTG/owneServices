//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDispositionDataAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSDispositionDataAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USDispositionDataAddInfoValidation : AutoUSDispositionDataAddInfoValidation
	{
		public USDispositionDataAddInfoValidation(AutoUSDispositionDataAddInfo parent) : base(parent)
		{
		}
	}
}
