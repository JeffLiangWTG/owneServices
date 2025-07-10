//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProductionRulesFactTypeViewValidation
//
//    This class should be used for overriding validation in AutoProductionRulesFactTypeViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ProductionRules.Business
{
	public class ProductionRulesFactTypeViewValidation : AutoProductionRulesFactTypeViewValidation
	{
		public ProductionRulesFactTypeViewValidation(AutoProductionRulesFactTypeView parent)
			: base(parent)
		{
		}
	}
}
