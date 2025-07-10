//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewCommissionAgreementOverallItemValidation
//
//    This class should be used for overriding validation in AutoViewCommissionAgreementOverallItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class ViewCommissionAgreementOverallItemValidation : AutoViewCommissionAgreementOverallItemValidation
	{
		public ViewCommissionAgreementOverallItemValidation(AutoViewCommissionAgreementOverallItem parent) : base(parent)
		{
		}
	}
}
