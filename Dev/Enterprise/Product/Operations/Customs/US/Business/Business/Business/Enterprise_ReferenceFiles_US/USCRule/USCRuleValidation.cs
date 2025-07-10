//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCRuleValidation
//
//    This class should be used for overriding validation in AutoUSCRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCRuleValidation : AutoUSCRuleValidation
	{
		public USCRuleValidation(AutoUSCRule parent)
			: base(parent)
		{
		}
	}
}
