//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoSIDataAddInfoValidation
//
//    This class should be used for overriding validation in AutoSIDataAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.TW.Business
{
	public class SIDataAddInfoValidation : AutoSIDataAddInfoValidation
	{
		public SIDataAddInfoValidation(AutoSIDataAddInfo parent) : base(parent)
		{
		}
	}
}
