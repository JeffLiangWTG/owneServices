using System;
using System.Collections;

using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ISimilarOrganisationsFinder
	{
		event EventHandler OnSecurityAccessDenied;

		void ForceRegeneratePatternMatch();
		void FindMaximumSimilarOrganisations();
		void FindSimilarOrganisations();
		void FindSimilarOrganisations(ZBool shouldIncludeUnmatchedInResultList);
		void FindSimilarOrganisations(ZQuery additionalFilter, ZBool shouldIncludeUnmatchedInResultList);
		void FindSimilarOrganisations(ZQuery additionalFilter, ZBool shouldIncludeUnmatchedInResultList, ZBool allowMorePotentialMatches);
		void ResetHasChanges();
		ZPropertyInfo ClosestPortInfo { get; }

		[SuppressWeaklyTypedCollectionMessage]
		IList OH_State_List { get; }
		ZInt SimilarOrganisationMatchesFound { get; }
		ZInt SimilarOrganisationMatchesShown { get; }
		ZBool HasTooManyMatches { get; }
		ZBool LikelyMatchFound { get; }
		ZBool OverrideMaximumResultsToInfinity { get; set; }
	}
}
