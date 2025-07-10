//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTWOrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoTWOrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.TW.Business
{
	public class TWOrgImpAddInfoValidation : AutoTWOrgImpAddInfoValidation
	{
		public TWOrgImpAddInfoValidation(AutoTWOrgImpAddInfo parent) : base(parent)
		{
		}
	}
}
