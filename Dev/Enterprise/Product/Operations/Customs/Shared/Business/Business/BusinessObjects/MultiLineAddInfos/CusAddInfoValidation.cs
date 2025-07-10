//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusAddInfoValidation
//
//    This class should be used for overriding validation in AutoCusAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business.MultiLineAddInfos
{
	public class CusAddInfoValidation : AutoCusAddInfoValidation
	{
		public CusAddInfoValidation(AutoCusAddInfo parent) : base(parent)
		{
		}
	}
}
