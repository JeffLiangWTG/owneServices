//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProductionRuleValidation
//
//    This class should be used for overriding validation in AutoProductionRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ProductionRules.Business
{
	public class ProductionRuleValidation : AutoProductionRuleValidation
	{
		public ProductionRuleValidation(AutoProductionRule parent)
			: base(parent)
		{
		}
	}
}
