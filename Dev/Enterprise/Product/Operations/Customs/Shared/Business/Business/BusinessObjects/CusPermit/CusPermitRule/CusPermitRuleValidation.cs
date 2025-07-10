//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusPermitRuleValidation
//
//    This class should be used for overriding validation in AutoCusPermitRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusPermitRuleValidation : AutoCusPermitRuleValidation
	{
		public CusPermitRuleValidation(AutoCusPermitRule parent) : base(parent)
		{
		}
	}
}
