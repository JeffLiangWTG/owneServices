//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProductionRuleSetValidation
//
//    This class should be used for overriding validation in AutoProductionRuleSetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ProductionRules.Business
{
	public class ProductionRuleSetValidation : AutoProductionRuleSetValidation
	{
		public ProductionRuleSetValidation(AutoProductionRuleSet parent)
			: base(parent)
		{
		}
	}
}
