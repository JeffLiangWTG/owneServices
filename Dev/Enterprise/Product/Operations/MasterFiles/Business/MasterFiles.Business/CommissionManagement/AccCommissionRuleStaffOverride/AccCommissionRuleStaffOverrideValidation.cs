//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionRuleStaffOverrideValidation
//
//    This class should be used for overriding validation in AutoAccCommissionRuleStaffOverrideValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleStaffOverrideValidation : AutoAccCommissionRuleStaffOverrideValidation
	{
		public AccCommissionRuleStaffOverrideValidation(AutoAccCommissionRuleStaffOverride parent) : base(parent)
		{
		}
	}
}
