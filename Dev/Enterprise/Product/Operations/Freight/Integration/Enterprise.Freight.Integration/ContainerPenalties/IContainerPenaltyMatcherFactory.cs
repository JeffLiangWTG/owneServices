using System.Collections.Generic;

namespace Enterprise.Freight.Integration
{
	public interface IContainerPenaltyMatcherFactory
	{
		IContainerPenaltyMatchResult MatchStorage(IContainerPenaltyMatchFilter filter, bool withRegistryFallBack = true);

		IContainerPenaltyMatchResult MatchDetention(IContainerPenaltyMatchFilter filter, bool withRegistryFallBack = true);

		IContainerPenaltyMatchResult MatchMDD(IContainerPenaltyMatchFilter filter, bool withRegistryFallBack = true);

		IEnumerable<IContainerPenaltyMatchResult> MatchPenalties(IEnumerable<IContainerPenaltyMatchFilter> detentionFilters, IEnumerable<IContainerPenaltyMatchFilter> storageFilters, IEnumerable<IContainerPenaltyMatchFilter> mddFilters, bool withRegistryFallback = true);
	}
}
