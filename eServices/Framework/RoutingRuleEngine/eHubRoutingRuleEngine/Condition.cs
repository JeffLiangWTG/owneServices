using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
#if NET48
using eServices.eHubDataModel.eHubTransactions;
#else
using eServices.eHubDataModel.eHubTransactionsCore;
#endif
using Common.Logging;
using Linq = System.Linq.Expressions;

namespace eServices.eHubRoutingRuleEngine
{
	public class Condition : Criterion
	{
		public Condition()
			: this(new eHubRoutingRule()) { }

		internal Condition(eHubRoutingRule rule)
			: base(rule)
		{
			this.Recipient = this.Recipient;
		}

		internal override Group FindGroupRule(string groupName)
		{
			if (this.SuccessSubRule != null)
			{
				var result = this.SuccessSubRule.FindGroupRule(groupName);
				if (result != null) return result;
			}
			if (this.FailedSubRule != null)
			{
				var result = this.FailedSubRule.FindGroupRule(groupName);
				if (result != null) return result;
			}
			return null;
		}

		internal override Collection<Result> Evaluate(eHubTransactionsContext context, Dictionary<string, string> facts, ILog logger)
		{
			try
			{
				logger.DebugFormat("Evaluating condition rule: {0}", base.eHubRoutingRule.RR_Condition_Expression);

				lock (this.lockCompile)
				{
					if (this.compiledExpression == null)
					{
						var factsParam = Linq.Expression.Parameter(typeof(Dictionary<string, string>), "facts");
						this.compiledExpression = new Func<Dictionary<string, string>, bool>(Linq.Expression.Lambda<Func<Dictionary<string, string>, bool>>(ParseExpressionText(this.Expression, factsParam, logger), factsParam).Compile());
					}
				}

				bool returnResult;
				try
				{
					returnResult = compiledExpression.Invoke(facts);
				}
				catch (KeyNotFoundException)
				{
					throw new RoutingRuleException("Could not find matching Fact when evaluating rule: " + Expression);
				}

				if (returnResult)
				{
					logger.Debug("Condition rule evaluated to 'true'");
					if (this.SuccessSubRule != null)
					{
						logger.Debug("Evaluating success sub-rule");
						return this.SuccessSubRule.Evaluate(context, facts, logger);
					}
					else
					{
						if (this.ServiceProvider != null)
						{
							logger.DebugFormat("Evaluating provider '{0}'", base.eHubRoutingRule.eHubServiceProvider.eHubClient_Provider.CC_ID);
							var providerResult = base.EvaluateProvider(context, facts, logger);
							if (logger.IsDebugEnabled)
								logger.DebugFormat("Provider evaluation results = [ {0} ]", String.Join(", ", providerResult));
							if (providerResult.Count > 0)
								return providerResult;
						}

						string resultValue = null;
						if (this.Value != null)
						{
							if (this.Value.StartsWith("@") && facts.ContainsKey(this.Value))
								resultValue = facts[this.Value];
							else
								resultValue = this.Value;
						}

						var result = new Result(this.Recipient, this.ErrorCode, this.ErrorDescription, resultValue);

						if (logger.IsDebugEnabled)
							logger.DebugFormat("Condition rule result = {0}", result);

						return new Collection<Result> { result };
					}
				}
				else
				{
					logger.Debug("Condition rule evaluated to 'false'");
					if (this.FailedSubRule != null)
					{
						logger.Debug("Evaluating failed sub-rule");
						return this.FailedSubRule.Evaluate(context, facts, logger);
					}
				}

				return new Collection<Result>();
			}
			catch (Exception ex)
			{
				throw new RoutingRuleException(string.Format("Current row having init error: {0}\r\n", CurrentRowData), ex);
			}
		}

		Linq.Expression ParseExpressionText(string expressionText, Linq.Expression factsParam, ILog logger)
		{
			Linq.Expression parsedExpression;
			logger.TraceFormat("Parsing expression text: {0}", expressionText);

			int pos = 0;
			parsedExpression = ParseConditionGroup(expressionText, factsParam, ref pos, logger);

			logger.TraceFormat("Parsed expression: {0}", parsedExpression);
			return parsedExpression;
		}

		Linq.Expression ParseConditionGroup(string text, Linq.Expression factsParam, ref int pos, ILog logger)
		{
			Linq.Expression parsedExpression = null;
			var skipWhiteSpace = new Func<int, int>(p => Regex.Match(text.Substring(p), @"^\s*").Value.Length);
			pos += skipWhiteSpace(pos);

			if (text[pos] == '(')
			{
				pos++;
				parsedExpression = ParseConditionGroup(text, factsParam, ref pos, logger);
				logger.TraceFormat("Parsed group: {0}", parsedExpression);
				pos++;
			}
			else
			{
				string cond = Regex.Match(text.Substring(pos), @"^\[.+?,.+?,.*?\]", RegexOptions.Compiled).Value;
				if (string.IsNullOrEmpty(cond))
                {
					throw new RoutingRuleException(String.Format("Invalid Routing Rule Condition Expression – {0}. Regex match with condition @ ^\\[.+?,.+?,.*?\\] is not successful", text));
				}
				parsedExpression = ParseConditionText(cond, factsParam, logger);
				pos += cond.Length;
			}

			pos += skipWhiteSpace(pos);

			while (pos < (text.Length - 1) && (text.Substring(pos, 2) == "&&" || text.Substring(pos, 2) == "||"))
			{
				string groupOp = text.Substring(pos, 2);
				pos += 2;
				var nextExpr = ParseConditionGroup(text, factsParam, ref pos, logger);
				if (groupOp == "&&")
					parsedExpression = Linq.Expression.And(parsedExpression, nextExpr);
				else
					parsedExpression = Linq.Expression.Or(parsedExpression, nextExpr);
			}

			return parsedExpression;
		}

		Linq.Expression ParseConditionText(string conditionText, Linq.Expression factsParam, ILog logger)
		{
			Linq.Expression parsedCondition;

			var parts = Regex.Match(conditionText, @"\[(?<leftPart>.*),(?<conditionType>.*),(?<rightPart>.*)\]", RegexOptions.Compiled);
			string leftPart = parts.Groups["leftPart"].Value;
			string comparisonType = parts.Groups["conditionType"].Value;
			string rightPart = parts.Groups["rightPart"].Value;

			var leftExpression = BuildValueExpression(leftPart, factsParam);
			var rightExpression = BuildValueExpression(rightPart, factsParam);

			switch (comparisonType)
			{
				case "IsMatch":
					var regexMethod = typeof(Regex).GetMethod("IsMatch", new[] { typeof(string), typeof(string) });
					parsedCondition = Linq.Expression.Call(regexMethod, leftExpression, rightExpression);
					break;
				default:
					Linq.ExpressionType expressionType;
					if (Enum.TryParse(comparisonType, out expressionType))
						parsedCondition = Linq.Expression.MakeBinary(expressionType, leftExpression, rightExpression);
					else
					{
						var comparisonMethod = typeof(string).GetMethod(comparisonType, new[] { typeof(string) });
						parsedCondition = Linq.Expression.Call(leftExpression, comparisonMethod, rightExpression);
					}
					break;
			}

			logger.TraceFormat("Parsed condition: {0}", parsedCondition);
			return parsedCondition;
		}

		Linq.Expression BuildValueExpression(string valueText, Linq.Expression factsParam)
		{
			if (valueText.StartsWith("@", StringComparison.Ordinal))
				return Linq.Expression.Property(factsParam, "Item", Linq.Expression.Constant(valueText));
			else
				return Linq.Expression.Constant(valueText);
		}

		public string Expression
		{
			get { return base.eHubRoutingRule.RR_Condition_Expression; }
			set { base.eHubRoutingRule.RR_Condition_Expression = value; }
		}

		Func<Dictionary<string, string>, bool> compiledExpression;
		object lockCompile = new object();
	}
}
