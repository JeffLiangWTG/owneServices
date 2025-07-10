//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProductionRuleScheduleQueueValidation
//
//    This class should be used for overriding validation in AutoProductionRuleScheduleQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ProductionRules.Business
{
	public class ProductionRuleScheduleQueueValidation : AutoProductionRuleScheduleQueueValidation
	{
		public ProductionRuleScheduleQueueValidation(AutoProductionRuleScheduleQueue parent)
			: base(parent)
		{
		}
	}
}
