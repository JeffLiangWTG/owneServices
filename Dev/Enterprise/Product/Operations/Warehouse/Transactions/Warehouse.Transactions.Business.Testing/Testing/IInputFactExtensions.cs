using System.Collections.Generic;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public static class IInputFactExtensions
	{
		public static IEnumerable<IInputFact> GetNestedFacts(this IEnumerable<IInputFact> topLevelFacts)
		{
			var finder = new NestedFactsFinder();
			var queue = new Queue<IInputFact>(topLevelFacts);
			var result = new HashSet<IInputFact>();
			while (queue.Count > 0)
			{
				var fact = queue.Dequeue();
				var nestedFacts = finder.GetDirectNestedInputFacts(fact);
				foreach (var nestedFact in nestedFacts)
				{
					if (result.Add(nestedFact.Fact))
					{
						queue.Enqueue(nestedFact.Fact);
					}
				}
			}

			return result;
		}
	}
}
