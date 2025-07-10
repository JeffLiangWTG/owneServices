using System.Collections.Generic;

namespace Enterprise.Freight.Integration
{
	public interface IContainerPenaltyMatcher
	{
		IContainerPenaltyMatchResult MatchStorage(IContainerPenaltyMatchFilter filter);

		IContainerPenaltyMatchResult MatchDetention(IContainerPenaltyMatchFilter filter);

		IContainerPenaltyMatchResult MatchMDD(IContainerPenaltyMatchFilter filter);

		IEnumerable<IContainerPenaltyMatchResult> MatchPenalties(IEnumerable<IContainerPenaltyMatchFilter> detentionFilters, IEnumerable<IContainerPenaltyMatchFilter> storageFilters, IEnumerable<IContainerPenaltyMatchFilter> mddFilters);

		PenaltyMatcherType MatcherType { get; }
	}

	public enum PenaltyMatcherType
	{
		Registry,
		Organization,
		CarrierContact,
		ClientContract
	}
}
