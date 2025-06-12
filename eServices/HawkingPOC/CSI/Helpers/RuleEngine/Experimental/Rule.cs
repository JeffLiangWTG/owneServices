using System;
using System.Collections.Generic;
using System.Text;

namespace Hawking.RuleEngine
{
    public class Rule
    {
        public Rule(string name, IEvaluator evaluator)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public ICollection<dynamic> Evaluate(dynamic facts)
        {
            return Evaluator.Evaluate(facts);
        }

        public string Name { get; }
        public IEvaluator Evaluator { get; }
    }
}
