using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Consol.Reading
{
	sealed class ForwardingConsolScoreMatcherTest : TestCaseWithFactory
	{
		public void TestCalculateRuleWeight()
		{
			var rules = new List<ConsolFetchRule.ScoreRule>();
			rules.Add(new ConsolFetchRule.ScoreRule(null, 0));
			rules.Add(new ConsolFetchRule.ScoreRule(null, 1));
			rules.Add(new ConsolFetchRule.ScoreRule(null, 2));
			rules.Add(new ConsolFetchRule.ScoreRule(null, 3));
			ForwardingConsolScoreMatcher.CalculateRuleWeight(rules);

			AssertEquals(3, rules[0].Weight);
			AssertEquals(2, rules[1].Weight);
			AssertEquals(1, rules[2].Weight);
			AssertEquals(0, rules[3].Weight);

			rules = new List<ConsolFetchRule.ScoreRule>();
			rules.Add(new ConsolFetchRule.ScoreRule(null, 5));
			rules.Add(new ConsolFetchRule.ScoreRule(null, 100));
			rules.Add(new ConsolFetchRule.ScoreRule(null, 100));
			rules.Add(new ConsolFetchRule.ScoreRule(null, 10000));
			ForwardingConsolScoreMatcher.CalculateRuleWeight(rules);

			AssertEquals(3, rules[0].Weight);
			AssertEquals(1, rules[1].Weight);
			AssertEquals(1, rules[2].Weight);
			AssertEquals(0, rules[3].Weight);
		}

		public void TestMatchAndScore()
		{
			var consolA = Factory.New<ForwardingConsol>();
			consolA.JK_UniqueConsignRef = "C00001001";
			consolA.JK_MasterBillNum = "123456789";

			var consolB = Factory.New<ForwardingConsol>();
			consolB.JK_UniqueConsignRef = "C00001002";
			consolB.JK_BookingReference = "987654321";

			var consolC = Factory.New<ForwardingConsol>();
			consolC.JK_UniqueConsignRef = "C00001003";
			consolC.JK_MasterBillNum = "123456789";
			consolC.JK_BookingReference = "XXXXXXX";

			var consolD = Factory.New<ForwardingConsol>();
			consolD.JK_UniqueConsignRef = "C00001004";
			consolD.JK_MasterBillNum = "123456789";
			consolD.JK_BookingReference = "987654321";

			var consolE = Factory.New<ForwardingConsol>();
			consolE.JK_UniqueConsignRef = "C00001005";
			consolE.JK_MasterBillNum = "XXXXXXX";
			consolE.JK_BookingReference = "XXXXXXX";

			Factory.Save();

			var matchRules = new List<ConsolFetchRule.MatchRule>();
			matchRules.Add(new ConsolFetchRule.MatchRule(JobConsolSchema.JK_MasterBillNum, "123456789"));
			matchRules.Add(new ConsolFetchRule.MatchRule(JobConsolSchema.JK_BookingReference, "987654321"));
			var undemandingConsols = ForwardingConsolScoreMatcher.Match(Factory, matchRules);

			AssertEquals(4, undemandingConsols.Length);
			AssertEquals(true, undemandingConsols.Any(consol => consol.JK_UniqueConsignRef == "C00001001"));
			AssertEquals(true, undemandingConsols.Any(consol => consol.JK_UniqueConsignRef == "C00001002"));
			AssertEquals(true, undemandingConsols.Any(consol => consol.JK_UniqueConsignRef == "C00001003"));
			AssertEquals(true, undemandingConsols.Any(consol => consol.JK_UniqueConsignRef == "C00001004"));

			var scoreRules = new List<ConsolFetchRule.ScoreRule>();
			scoreRules.Add(new ConsolFetchRule.ScoreRule(delegate (ForwardingConsol consol)
			{
				var actualValue = consol[JobConsolSchema.JK_MasterBillNum];
				if (actualValue != null && actualValue.ToString() == "123456789")
				{
					return true;
				}

				return false;
			}, 0));
			scoreRules.Add(new ConsolFetchRule.ScoreRule(delegate (ForwardingConsol consol)
			{
				var actualValue = consol[JobConsolSchema.JK_BookingReference];
				if (actualValue != null && actualValue.ToString() == "987654321")
				{
					return true;
				}

				return false;
			}, 1));
			var consolsWithScore = ForwardingConsolScoreMatcher.Score(undemandingConsols, scoreRules);

			AssertEquals(2, consolsWithScore.First(scoredConsol => scoredConsol.consol.JK_UniqueConsignRef == "C00001001").score);
			AssertEquals(1, consolsWithScore.First(scoredConsol => scoredConsol.consol.JK_UniqueConsignRef == "C00001002").score);
			AssertEquals(2, consolsWithScore.First(scoredConsol => scoredConsol.consol.JK_UniqueConsignRef == "C00001003").score);
			AssertEquals(3, consolsWithScore.First(scoredConsol => scoredConsol.consol.JK_UniqueConsignRef == "C00001004").score);
		}

		public void TestFullScore()
		{
			var rules = new List<ConsolFetchRule.ScoreRule>();
			rules.Add(new ConsolFetchRule.ScoreRule(null, 0));
			rules.Add(new ConsolFetchRule.ScoreRule(null, 1));
			rules.Add(new ConsolFetchRule.ScoreRule(null, 2));
			rules.Add(new ConsolFetchRule.ScoreRule(null, 3));
			ForwardingConsolScoreMatcher.CalculateRuleWeight(rules);

			AssertEquals(true, 15.IsFullScore(rules));
		}
	}
}
