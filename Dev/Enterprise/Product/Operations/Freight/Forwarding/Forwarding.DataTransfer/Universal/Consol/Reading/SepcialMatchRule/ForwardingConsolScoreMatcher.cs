using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal static class ForwardingConsolScoreMatcher
	{
		public static (int score, ForwardingConsol consol)[] MatchAndScore(BusinessObjectFactory factory, ConsolFetchRule consolFecthRule)
		{
			var consols = Match(factory, consolFecthRule.MatchRules);
			if (consols.Length == 0)
			{
				return null;
			}
			var cosolsWithScore = Score(consols, consolFecthRule.ScoreRules);
			return cosolsWithScore;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public static ForwardingConsol[] Match(BusinessObjectFactory factory, List<ConsolFetchRule.MatchRule> rules)
		{
			ZQuery conditions = null;
			foreach (var rule in rules)
			{
				conditions = conditions == null ? new ZQuery(rule.MatchKeyName, rule.MatchKeyValue) : conditions.AddToFilter(JoinCondition.Or, rule.MatchKeyName, rule.MatchKeyValue);
			}

			var undemandingConsols = factory.Load<ForwardingConsol>(conditions)
				.Where(consol => !consol.IsCancelled).ToArray();

			return undemandingConsols;
		}

		public static (int score, ForwardingConsol consol)[] Score(ForwardingConsol[] consols, List<ConsolFetchRule.ScoreRule> rules)
		{
			CalculateRuleWeight(rules);

			var scoredConsols = consols
				.Select(consol =>
				{
					int score = 0;
					foreach (var rule in rules)
					{
						if (rule.HaveToScore(consol))
						{
							score += (int)Math.Pow(2, rule.Weight);
						}
					}

					return (score, consol);
				}).OrderByDescending(item => item.score).ToArray();

			return scoredConsols;
		}

		public static void CalculateRuleWeight(List<ConsolFetchRule.ScoreRule> rules)
		{
			var orderedRules = rules.OrderByDescending(p => p.Priority).ToArray();

			for (int i = 0; i < orderedRules.Length; i++)
			{
				var priority = orderedRules[i].Priority;
				if (i == 0 || priority != orderedRules[i - 1].Priority)
				{
					orderedRules[i].Weight = i;
				}
				else
				{
					orderedRules[i].Weight = orderedRules[i - 1].Weight;
				}
			}
		}

		public static bool IsFullScore(this int score, List<ConsolFetchRule.ScoreRule> rules)
		{
			int fullScore = 0;
			foreach (var rule in rules)
			{
				fullScore += (int)Math.Pow(2, rule.Weight);
			}

			return score == fullScore;
		}
	}
}
