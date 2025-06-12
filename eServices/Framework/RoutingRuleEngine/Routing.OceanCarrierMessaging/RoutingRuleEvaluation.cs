using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;

namespace eServices.Routing.OceanCarrierMessaging
{
    public static class RoutingRuleEvaluation
	{
		public static List<Result> EvaluateOCM(eHubTransactionsContext context, IRoutingRuleFactory ruleFactory, string ruleId, IFactResolver[] resolvers, ILog logger)
		{
			return EvaluateOCM(context, ruleFactory, ruleId, null, resolvers, logger);
		}

		public static List<Result> EvaluateOCM(eHubTransactionsContext context, IRoutingRuleFactory ruleFactory, string ruleId1, string ruleId2, IFactResolver[] resolvers, ILog logger)
		{
			var results = new List<Result>();

			var rule1 = ruleFactory.GetForReading(ruleId1);
			var result1 = rule1?.Evaluate(context, resolvers, logger).SingleOrDefault();
			results.Add(result1);

			if (ruleId2 == null)
			{
				return results;
			}

			var facts = new Dictionary<string, string>
			{
				{
					"CarrierAgent",
					result1 == null || string.IsNullOrEmpty(result1.Value)
						? string.Empty
						: "DefaultCarrier"
				}
			};
			var injectedResolver = new InjectedFactResolver(facts);
			var resolversWithInjected = resolvers.Concat(new[] { injectedResolver }).ToArray();

			var rule2 = ruleFactory.GetForReading(ruleId2);
			var result2 = rule2.Evaluate(context, resolversWithInjected, logger).Single();

			results.Add(result2);

			if (result1?.Value != null)
			{
				facts["CarrierAgent"] = result1.Value;

				var result3 = rule2.Evaluate(context, resolversWithInjected, logger).Single();
				results.Add(result3);
			}

			return results;
		}
	}
}
