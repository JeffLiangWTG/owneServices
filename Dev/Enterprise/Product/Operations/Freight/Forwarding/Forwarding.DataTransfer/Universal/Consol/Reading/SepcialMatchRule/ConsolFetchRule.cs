using System;
using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal class ConsolFetchRule
	{
		public ConsolFetchRule(bool isUseNonCoLoadToMatchCoLoad = false)
		{
			this.IsUseNonCoLoadToMatchCoLoad = isUseNonCoLoadToMatchCoLoad;
		}

		public bool IsUseNonCoLoadToMatchCoLoad { get; }

		public List<MatchRule> MatchRules { get; private set; } = new List<MatchRule>();

		public List<ScoreRule> ScoreRules { get; private set; } = new List<ScoreRule>();

		internal class MatchRule
		{
			public MatchRule(SchemaColumn matchKeyName, string matchKeyValue)
			{
				MatchKeyName = matchKeyName;
				MatchKeyValue = matchKeyValue;
			}

			public SchemaColumn MatchKeyName { get; private set; }

			public string MatchKeyValue { get; private set; }
		}

		internal class ScoreRule
		{
			public ScoreRule(Func<ForwardingConsol, bool> haveToScore, int priority)
			{
				HaveToScore = haveToScore;
				Priority = priority;
			}

			public int Priority { get; private set; }

			public int Weight { get; set; }

			public Func<ForwardingConsol, bool> HaveToScore { get; }
		}
	}
}
