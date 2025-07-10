//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionRuleStaffDisableValidation
//
//    This class should be used for overriding validation in AutoAccCommissionRuleStaffDisableValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleStaffDisableValidation : AutoAccCommissionRuleStaffDisableValidation
	{
		public AccCommissionRuleStaffDisableValidation(AutoAccCommissionRuleStaffDisable parent) : base(parent)
		{
		}
	}
}
