//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewCommissionAgreementValidation
//
//    This class should be used for overriding validation in AutoViewCommissionAgreementValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class ViewCommissionAgreementValidation : AutoViewCommissionAgreementValidation
	{
		public ViewCommissionAgreementValidation(AutoViewCommissionAgreement parent) : base(parent)
		{
		}
	}
}
