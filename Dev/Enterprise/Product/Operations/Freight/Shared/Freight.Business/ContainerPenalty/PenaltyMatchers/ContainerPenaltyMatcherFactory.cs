using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Business
{
	public class ContainerPenaltyMatcherFactory : IContainerPenaltyMatcherFactory
	{
		public IContainerPenaltyMatchResult MatchStorage(IContainerPenaltyMatchFilter filter, bool fallbackToRegistry = true)
		{
			var matchers = GetPenaltyMatchers(fallbackToRegistry);
			foreach (var matcher in matchers)
			{
				var result = matcher.MatchStorage(filter);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		public IContainerPenaltyMatchResult MatchDetention(IContainerPenaltyMatchFilter filter, bool fallbackToRegistry = true)
		{
			var matchers = GetPenaltyMatchers(fallbackToRegistry);
			foreach (var matcher in matchers)
			{
				var result = matcher.MatchDetention(filter);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		public IContainerPenaltyMatchResult MatchMDD(IContainerPenaltyMatchFilter filter, bool fallbackToRegistry = true)
		{
			var matchers = GetPenaltyMatchers(fallbackToRegistry);
			foreach (var matcher in matchers)
			{
				var result = matcher.MatchMDD(filter);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		public IEnumerable<IContainerPenaltyMatchResult> MatchPenalties(IEnumerable<IContainerPenaltyMatchFilter> detentionFilters, IEnumerable<IContainerPenaltyMatchFilter> storageFilters, IEnumerable<IContainerPenaltyMatchFilter> mddFilters, bool fallbackToRegistry = true)
		{
			var penaltyResult = new List<IContainerPenaltyMatchResult>();
			var matchers = GetPenaltyMatchers(fallbackToRegistry);

			foreach (var matcher in matchers)
			{
				var result = matcher.MatchPenalties(detentionFilters, storageFilters, mddFilters);
				if (result != null)
				{
					penaltyResult.AddRange(result);
				}
			}

			return penaltyResult;
		}

		IReadOnlyList<IContainerPenaltyMatcher> GetPenaltyMatchers(bool fallbackToRegistry)
		{
			return fallbackToRegistry
				? ContainerPenaltyMatchers.PriorityOrderedPenaltyMatchers
				: ContainerPenaltyMatchers.PriorityOrderedPenaltyMatchers.Where(matcher => matcher.MatcherType != PenaltyMatcherType.Registry).ToList();
		}
	}
}
