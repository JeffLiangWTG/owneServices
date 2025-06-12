
using Microsoft.Extensions.Logging;

namespace Hawking.RuleEngine
{
	public interface ICriterion
	{
		bool Evaluate(FactsProvider factsProvider, ILogger logger);
	}
}
