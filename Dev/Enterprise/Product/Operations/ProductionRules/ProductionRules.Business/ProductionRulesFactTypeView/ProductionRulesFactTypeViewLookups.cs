//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProductionRulesFactTypeViewLookups
//
//    This class should be used for overriding collections in AutoProductionRulesFactTypeViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ProductionRules.Business
{
	public class ProductionRulesFactTypeViewLookups : AutoProductionRulesFactTypeViewLookups
	{
		public ProductionRulesFactTypeViewLookups(AutoProductionRulesFactTypeView parent)
			: base(parent)
		{
		}
	}
}
