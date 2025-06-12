using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Hawking.RuleEngine
{
	public interface IEvaluator
	{
		ICollection<Evaluator.Result> Evaluate(FactsProvider factsProvider, ILogger logger);
	}
}
