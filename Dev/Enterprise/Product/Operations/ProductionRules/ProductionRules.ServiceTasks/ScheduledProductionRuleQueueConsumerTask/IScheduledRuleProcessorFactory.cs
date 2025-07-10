using Enterprise.ProductionRules.Integration;
using WTG.ProductionRules.Core;

namespace Enterprise.ProductionRules.ServiceTasks
{
	interface IScheduledRuleProcessorFactory
	{
		IScheduledRuleProcessor GetRuleProcessor(RulesContextType rulesContext);
	}
}
