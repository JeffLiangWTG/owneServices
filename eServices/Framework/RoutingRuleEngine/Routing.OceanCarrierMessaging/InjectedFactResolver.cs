using System;
using System.Collections.Generic;
using eServices.eHubRoutingRuleEngine;

namespace eServices.Routing.OceanCarrierMessaging
{
    public class InjectedFactResolver : IFactResolver
	{
		readonly IDictionary<string, string> injectedFacts;

		public InjectedFactResolver(IDictionary<string, string> injectedFacts)
		{
			this.injectedFacts = injectedFacts ?? throw new ArgumentNullException(nameof(injectedFacts));
		}

		public void Resolve(Fact[] facts)
		{
			foreach (var fact in facts)
			{
				if (injectedFacts.ContainsKey(fact.Name))
				{
					fact.Value = injectedFacts[fact.Name];
				}
			}
		}
	}
}
