//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSOGADispositionDetailAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSOGADispositionDetailAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USOGADispositionDetailAddInfoValidation : AutoUSOGADispositionDetailAddInfoValidation
	{
		public USOGADispositionDetailAddInfoValidation(AutoUSOGADispositionDetailAddInfo parent) : base(parent)
		{
		}
	}
}
