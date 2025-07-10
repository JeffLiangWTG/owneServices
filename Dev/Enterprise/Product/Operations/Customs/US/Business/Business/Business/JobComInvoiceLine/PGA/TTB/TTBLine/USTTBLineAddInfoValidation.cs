//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSTTBLineAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSTTBLineAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USTTBLineAddInfoValidation : AutoUSTTBLineAddInfoValidation
	{
		public USTTBLineAddInfoValidation(AutoUSTTBLineAddInfo parent) : base(parent)
		{
		}
	}
}
