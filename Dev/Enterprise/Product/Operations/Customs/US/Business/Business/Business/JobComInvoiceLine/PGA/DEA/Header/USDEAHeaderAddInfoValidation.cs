//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDEAHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSDEAHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USDEAHeaderAddInfoValidation : AutoUSDEAHeaderAddInfoValidation
	{
		public USDEAHeaderAddInfoValidation(AutoUSDEAHeaderAddInfo parent) : base(parent)
		{
		}
	}
}
