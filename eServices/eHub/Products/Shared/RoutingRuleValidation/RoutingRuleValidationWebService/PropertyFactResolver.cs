using System;
using System.Collections.Generic;
using System.Linq;
using eServices.eHubRoutingRuleEngine;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService
{
	public class PropertyFactResolver : IFactResolver
	{
		readonly IDictionary<string, string> PropertyFacts;

		public PropertyFactResolver(IDictionary<string, string> propertyFacts)
		{
			PropertyFacts = propertyFacts ?? throw new ArgumentNullException(nameof(propertyFacts));
		}

		public void Resolve(Fact[] facts)
		{
			foreach (var fact in facts.Where(f => (f.Type == "PROPERTY" &&  f.Name != "MessageType") || f.Type == "INJECTED"))
			{
				fact.Value = PropertyFacts.ContainsKey(fact.Name) ? PropertyFacts[fact.Name] : string.Empty;
			}
		}
	}
}