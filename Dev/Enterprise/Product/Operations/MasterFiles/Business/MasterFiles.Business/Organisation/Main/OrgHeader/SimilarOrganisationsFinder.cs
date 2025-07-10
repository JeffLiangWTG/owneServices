using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	class SimilarOrganisationsFinder : ISimilarOrganisationsFinder
	{
		readonly IOrgHeaderForMatching organisation;
		ZInt similarOrganisationMatchesFound;

		public SimilarOrganisationsFinder(IOrgHeaderForMatching org)
		{
			organisation = org;
		}

		BusinessObjectFactory Factory
		{
			get
			{
				return organisation.Factory;
			}
		}

		#region ISimilarOrganisationsFinder Members

		public ZPropertyInfo ClosestPortInfo
		{
			get { return organisation.OH_RL_NKClosestPortInfo; }
		}

		public void FindMaximumSimilarOrganisations()
		{
			FindSimilarOrganisations(new ZQuery(), true, true);
		}

		public void FindSimilarOrganisations(ZQuery additionalFilter, ZBool shouldIncludeUnmatchedInResultList, ZBool allowMorePotentialMatches)
		{
			organisation.SimilarOrgMatches.RemoveAll();

			similarOrganisationMatchesFound = GetApproximateDatabaseCount(allowMorePotentialMatches);

			if (HasNoMatches)
			{
				LoopLoadSimilarOrgsFromLocalCache(organisation.PatternMatchesForThisOrg.GetFilter(allowMorePotentialMatches));
				if (HasNoMatches)
				{
					allowMorePotentialMatches = true;
				}
			}

			if (!HasTooManyMatches)
			{
				organisation.SimilarOrgMatches.LoadSimilarOrganisations(organisation.PatternMatchesForThisOrg, additionalFilter, out similarOrganisationMatchesFound, allowMorePotentialMatches, shouldIncludeUnmatchedInResultList);
			}

			if (organisation.SimilarOrgMatches.Count == 0 && shouldIncludeUnmatchedInResultList)
			{
				organisation.SimilarOrgMatches.GetUnmatchedOrgPatternAndScore();
			}
		}

#if DEBUG
		internal
#endif
		int GetApproximateDatabaseCount(bool allowMorePotentialMatches)
		{
			return organisation.PatternMatchesForThisOrg
				.GetFilter(allowMorePotentialMatches)
				.Where(matches => !matches.IsEmpty)
				.Sum(query => Factory.GetDatabaseCount(typeof(OrgPatternMatch), query));
		}

		void LoopLoadSimilarOrgsFromLocalCache(IEnumerable<ZQuery> queries)
		{
			similarOrganisationMatchesFound = 0;
			foreach (ZQuery query in queries)
			{
				query.FetchOnlyFromLocalCache = true;
				similarOrganisationMatchesFound += Factory.Load<OrgPatternMatch>(query).Length;
			}
		}

		ZBool HasNoMatches
		{
			get { return SimilarOrganisationMatchesFound == 0; }
		}

		public void FindSimilarOrganisations(ZQuery additionalFilter, ZBool shouldIncludeUnmatchedInResultList)
		{
			FindSimilarOrganisations(additionalFilter, shouldIncludeUnmatchedInResultList, false);
		}

		public void FindSimilarOrganisations(ZBool shouldIncludeUnmatchedInResultList)
		{
			FindSimilarOrganisations(new ZQuery(), shouldIncludeUnmatchedInResultList);
		}

		public void FindSimilarOrganisations()
		{
			FindSimilarOrganisations(true);
		}

		public void ForceRegeneratePatternMatch()
		{
			organisation.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organisation);
		}

		public ZBool HasTooManyMatches
		{
			get { return SimilarOrganisationMatchesFound > MaximumResultsToSearch && MaximumResultsToSearch != 0; }
		}

		public ZBool LikelyMatchFound
		{
			get { return organisation.SimilarOrgMatches.LikelyMatchesExist; }
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList OH_State_List
		{
			get
			{
				return organisation.OH_State_List;
			}
		}

		public event EventHandler OnSecurityAccessDenied;

		protected void SecurityAccessDenied()
		{
			if (OnSecurityAccessDenied != null)
			{
				OnSecurityAccessDenied(this, EventArgs.Empty);
			}
		}

		public void ResetHasChanges()
		{
			organisation.ResetHasChanges();
		}

		public ZInt SimilarOrganisationMatchesFound
		{
			get
			{
				return similarOrganisationMatchesFound;
			}
		}

		public ZInt SimilarOrganisationMatchesShown
		{
			get
			{
				return organisation.SimilarOrgMatches.Count;
			}
		}

		protected int MaximumResultsToSearch
		{
			get
			{
				return OverrideMaximumResultsToInfinity ? 0 : OrganisationsDataRegistry.Instance.OrgPatternMatchLimit.Value;
			}
		}

		public ZBool OverrideMaximumResultsToInfinity { get; set; }

		#endregion
	}
}
