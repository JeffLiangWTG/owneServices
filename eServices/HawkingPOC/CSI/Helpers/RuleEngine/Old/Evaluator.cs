using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

//using Common.Logging;
//using Trace.Core.Utils.Helpers;

namespace Hawking.RuleEngine
{
    public static class Evaluator
    {
        public static ICollection<Result> Evaluate(this IEvaluator evaluator, IDictionary<string, object> facts, ILogger logger)
        {
            var factsContext = new FactsProvider(facts);
            var results = evaluator.Evaluate(factsContext, logger);

            return results;
        }

        static readonly Result[] EmptyResult = new Result[] { };

        public class Decision : IEvaluator
        {
            public ICriterion Criterion { get; set; }
            public IEvaluator Success { get; set; }
            public IEvaluator Fail { get; set; }

            public ICollection<Result> Evaluate(FactsProvider factsProvider, ILogger logger)
            {
                ICollection<Result> result = EmptyResult;

                bool conditionResult = Criterion.Evaluate(factsProvider, logger);

                if (conditionResult)
                {
                    if (Success != null)
                    {
                        result = Success.Evaluate(factsProvider, logger);
                    }
                }
                else
                {
                    if (Fail != null)
                    {
                        result = Fail.Evaluate(factsProvider, logger);
                    }
                }

                return result;
            }
        }

        public class Group : Collection<IEvaluator>, IEvaluator
        {
            public new IList<IEvaluator> Items { get { return base.Items; } }
            public bool FirstResultOnly { get; set; }
            public IEvaluator Default { get; set; }

            public ICollection<Result> Evaluate(FactsProvider factsProvider, ILogger logger)
            {
                ICollection<Result> result;

                if (FirstResultOnly)
                    result = Items.Select(i => i.Evaluate(factsProvider, logger)).FirstOrDefault(r => r.Any()) ?? EmptyResult;
                else
                    result = Items.SelectMany(i => i.Evaluate(factsProvider, logger)).ToArray();

                if (result.Count == 0)
                    if (Default != null)
                    {
                        logger.LogDebug("Group Default");
                        result = Default.Evaluate(factsProvider, logger);
                    }

                return result;
            }
        }

        public class Result : List<Value>, IEvaluator
        {
            public Result() { }
            public Result(IEnumerable<Value> list) : base(list) { }

            public Value[] Values { get { return this.ToArray(); } }

            public ICollection<Result> Evaluate(FactsProvider factsProvider, ILogger logger)
            {
                var result = new[] { new Result(this.Select(v => v.Evaluate(factsProvider)).ToList()) };

                logger.LogDebug(FormatResults(result, String.Empty));
                return result;
            }
        }

        public class MultipleResults : Collection<Result>, IEvaluator
        {
            public ICollection<Result> Evaluate(FactsProvider factsProvider, ILogger logger)
            {
                var results = Items.SelectMany(i => i.Evaluate(factsProvider, logger)).ToArray();

                return results;
            }
        }

        static string FormatResults(ICollection<Result> results, string message)
        {
            var sb = new StringBuilder(message);

            if (!message.EndsWith(" ")) sb.Append(' ');

            if (results.Count == 0)
                sb.Append("No Result");
            else if (results.Count == 1)
                sb.Append("Result: ").Append(FormatResult(results.First()));
            else
            {
                sb.AppendLine("Results:");
                foreach (var result in results)
                    sb.AppendLine(FormatResult(result));
            }

            return sb.ToString().TrimEnd(new[] { '\r', '\n' });
        }

        static string FormatResult(Result result)
        {
            return "[ " + String.Join("', ", result.Select(v => String.Format("{0} = '{1}", v.Name, v.Contents))) + "' ]";
        }
    }
}
