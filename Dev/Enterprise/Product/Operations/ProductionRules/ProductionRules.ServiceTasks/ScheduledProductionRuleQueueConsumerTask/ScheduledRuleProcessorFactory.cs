using System.Collections;
using CargoWise.Application;
using Enterprise.ProductionRules.Integration;
using WTG.ProductionRules.Core;

namespace Enterprise.ProductionRules.ServiceTasks
{
	class ScheduledRuleProcessorFactory : IScheduledRuleProcessorFactory
	{
		public IScheduledRuleProcessor GetRuleProcessor(RulesContextType rulesContext)
		{
			var providers = ObjectFactory.Get<Hashtable>(ScheduledRuleProcessors);
			var providerHandle = (ObjectHandle)providers[rulesContext.ToString()];
			return (IScheduledRuleProcessor)providerHandle?.GetObject();
		}

		const string ScheduledRuleProcessors = nameof(ScheduledRuleProcessors);
	}
}
