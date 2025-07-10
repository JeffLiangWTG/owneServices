//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSOGADispositionDataAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSOGADispositionDataAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USOGADispositionDataAddInfoValidation : AutoUSOGADispositionDataAddInfoValidation
	{
		public USOGADispositionDataAddInfoValidation(AutoUSOGADispositionDataAddInfo parent) : base(parent)
		{
		}
	}
}
