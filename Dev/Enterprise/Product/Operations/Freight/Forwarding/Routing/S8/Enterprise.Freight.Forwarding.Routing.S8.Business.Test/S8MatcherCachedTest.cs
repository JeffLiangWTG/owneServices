using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Moq;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class S8MatcherCachedTest : TestCaseWithFactory
	{
		Mock<IS8Matcher> matcherMock;
		S8MatcherCached cachedMatcher;

		ScheduleInfo sched1, sched1Equivalent, sched2;
		IS8MatchResult ret1, ret2;

		class S8MatchResultComparer : IEqualityComparer<IS8MatchResult>
		{
			public bool Equals(IS8MatchResult r1, IS8MatchResult r2)
			{
				return r1.MatchedSchedule == r2.MatchedSchedule
					&& r1.MatchErrorMessage == r2.MatchErrorMessage
					&& r1.ScheduleStatus == r2.ScheduleStatus;
			}

			public int GetHashCode(IS8MatchResult r1)
			{
				return base.GetHashCode();
			}
		}
		readonly S8MatchResultComparer s8MatchResultComparer = new S8MatchResultComparer();

		protected override void SetUp()
		{
			base.SetUp();
			sched1 = new ScheduleInfo("carrier", 1, "origin", ZDate.Empty, "destination", ZDate.Empty);
			sched1Equivalent = new ScheduleInfo("carrier", 1, "origin", ZDate.Empty, "destination", ZDate.Empty);
			sched2 = new ScheduleInfo("carrier2", 1, "origin", ZDate.Empty, "destination", ZDate.Empty);

			ret1 = GetResultWithText("ret1");
			ret2 = GetResultWithText("ret2");

			matcherMock = new Mock<IS8Matcher>();
			matcherMock.Setup(r => r.Match(sched1)).Returns(ret1);
			matcherMock.Setup(r => r.Match(sched2)).Returns(ret2);

			var factory = new BusinessObjectFactory();
			cachedMatcher = new S8MatcherCached(matcherMock.Object, factory);
		}

		IS8MatchResult GetResultWithText(string scheduleStatus)
		{
			var res = new S8MatchResult();
			res.ScheduleStatus = scheduleStatus;
			return res;
		}

		public void TestMultipleCallsOfSameKeyCallsMatcherOnce()
		{
			var z = cachedMatcher.Match(sched1);
			Assert(s8MatchResultComparer.Equals(ret1, z));
			Assert(s8MatchResultComparer.Equals(ret1, cachedMatcher.Match(sched1)));
			Assert(s8MatchResultComparer.Equals(ret1, cachedMatcher.Match(sched1Equivalent)));
			Assert(s8MatchResultComparer.Equals(ret1, cachedMatcher.Match(sched1Equivalent)));
			Assert(s8MatchResultComparer.Equals(ret1, cachedMatcher.Match(sched1Equivalent)));

			matcherMock.Verify(obj => obj.Match(sched1), Times.Exactly(1));
		}

		public void TestDifferentKeysResultInSeveralMatcherCalls()
		{
			Assert(s8MatchResultComparer.Equals(ret1, cachedMatcher.Match(sched1)));
			Assert(s8MatchResultComparer.Equals(ret2, cachedMatcher.Match(sched2)));
			Assert(s8MatchResultComparer.Equals(ret1, cachedMatcher.Match(sched1)));

			matcherMock.Verify(obj => obj.Match(sched1), Times.Exactly(1));
			matcherMock.Verify(obj => obj.Match(sched2), Times.Exactly(1));
		}
	}
}
