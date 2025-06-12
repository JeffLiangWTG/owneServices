using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Hawking.RuleEngine
{
	public interface IEvaluator
	{
		ICollection<dynamic> Evaluate(dynamic facts);
	}
}
