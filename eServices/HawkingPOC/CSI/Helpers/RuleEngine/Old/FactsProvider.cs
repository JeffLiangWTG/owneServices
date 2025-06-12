using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace Hawking.RuleEngine
{
	public sealed class FactsProvider
	{
		public IDictionary<string, object> FactValues { get; set; }
		public object Facts { get; set; }

		public FactsProvider(IDictionary<string, object> facts)
		{
			FactValues = facts;

			var dynamicProperties = facts.Select(f => new DynamicProperty(f.Key, f.Value == null ? typeof(object) : f.Value.GetType()));
			var dynamicType = DynamicExpression.CreateClass(dynamicProperties);

			Facts = Activator.CreateInstance(dynamicType);

			foreach (var fact in facts)
				dynamicType.GetProperty(fact.Key).SetValue(Facts, fact.Value);
		}
	}
}
