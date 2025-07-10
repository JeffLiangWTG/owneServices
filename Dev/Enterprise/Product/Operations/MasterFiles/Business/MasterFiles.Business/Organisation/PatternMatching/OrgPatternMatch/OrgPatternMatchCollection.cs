using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPatternMatchCollection : BusinessObjectCollection<OrgPatternMatch>
	{
		public const int LowMatchThreshold = 45;
		public const int MediumMatchThreshold = 90;
		public const int HighMatchThreshold = 140;
		public const int ExtremeMatchThreshold = 170;

		public OrgPatternMatchCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgPatternMatchCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		#region Likely Matches Exist

		public ZBool LikelyMatchesExist
		{
			get
			{
				foreach (OrgPatternMatch orgMatch in this)
				{
					if (orgMatch.OS_Score >= LikelyMatchScore)
					{
						return true;
					}
				}
				return false;
			}
		}

		ZString fMatchThresholdOverride;
		public ZString MatchThresholdOverride
		{
			get { return fMatchThresholdOverride; }
			set
			{
				if (value == OrgMatchThresholds.Codes.Low ||
					value == OrgMatchThresholds.Codes.Medium ||
					value == OrgMatchThresholds.Codes.High ||
					value == OrgMatchThresholds.Codes.Extreme ||
					value.IsEmpty
					)
				{
					fMatchThresholdOverride = value;
				}
			}
		}

#if DEBUG
		internal
#endif
 int LikelyMatchScore
		{
			get
			{
				int result;
				string threshold = !MatchThresholdOverride.IsEmpty ? MatchThresholdOverride.ToString() : OrganisationsDataRegistry.Instance.OrgMatchThreshold.Value;
				switch (threshold)
				{
					case OrgMatchThresholds.Codes.Low:
						result = LowMatchThreshold;
						break;
					case OrgMatchThresholds.Codes.Medium:
						result = MediumMatchThreshold;
						break;
					case OrgMatchThresholds.Codes.High:
						result = HighMatchThreshold;
						break;
					case OrgMatchThresholds.Codes.Extreme:
						result = ExtremeMatchThreshold;
						break;
					default:
						throw new ArgumentException("Unknown threshold type " + threshold);
				}
				return result;
			}
		}

		#endregion

		#region Generate Pattern Matches From Org

		/// <summary>
		/// Self populates by creating pattern match records from the given org. It will automatically remove any existing pattern match records for itself.
		/// </summary>
		/// <param name="header">Analyses the Company Names and Addresses on the org and creates pattern match combinations</param>
		public void GeneratePatternMatchesFromOrg(IOrgHeaderForMatching header)
		{
			GeneratePatternMatchesFromOrg(header, null, null);
		}

		internal void GeneratePatternMatchesFromOrg(IOrgHeaderForMatching header, List<IMatchingAddress> addedAddressesOverride, List<OrganisationName> addedCompanyNamesOverride)
		{
			new PatternMatchReBuilder(new OrgPatternMatchDataManager(header, AllowEmptyAddresses, this)).Generate(addedAddressesOverride, addedCompanyNamesOverride);
		}

		public virtual ZBool AllowEmptyAddresses
		{
			get { return false; }
		}

		internal bool ContainsBasedOnMatchDetails(OrgPatternMatch matchToFind)
		{
			IOrgPatternMatchComparer comparer = new IOrgPatternMatchComparer();
			return this.Cast<OrgPatternMatch>().Any(match => comparer.Equals(match, matchToFind));
		}

		#endregion

		#region Load Similar Organisations

		/// <summary>
		/// Loads organisations similar to the matches provided
		/// </summary>
		/// <param name="MatchesToMatch">A collection of PatternMatches representing the organisation they want to try and match</param>
		/// <param name="AdditionalFilter">Any additional filtering that should be provided.</param>
		/// <param name="TotalMatchesFound">The total number of similar organisations found, before the count is reduced to MaximumResultsToShow</param>
		public void LoadSimilarOrganisations(OrgPatternMatchCollection patternMatchesForOrganisationToMatch, ZQuery additionalFilter, out ZInt totalMatchesFound, bool allowMorePotentialMatches, bool shouldIncludeUnmatchedOrgInList)
		{
			RemoveAll();

			IEnumerable<ZQuery> filters = patternMatchesForOrganisationToMatch.GetFilter(allowMorePotentialMatches);
			totalMatchesFound = 0;
			var totalMatchesFoundCollection = new List<OrgPatternMatch>();

			var unmatchOrg = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			var miscOrg = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			foreach (ZQuery filter in filters)
			{
				if (!filter.IsEmpty)
				{
					filter.AddToFilter(OrgPatternMatchSchema.OS_OH, SQLComparisonOperator.NotEqual, unmatchOrg);
					filter.AddToFilter(OrgPatternMatchSchema.OS_OH, SQLComparisonOperator.NotEqual, miscOrg);

					if (additionalFilter != null)
					{
						filter.AddToFilter(additionalFilter);
					}
					filter.OrderBy = "";
					foreach (var match in Factory.Load<OrgPatternMatch>(filter))
					{
						if (!totalMatchesFoundCollection.Contains(match))
						{
							totalMatchesFoundCollection.Add(match);
						}
					}
					totalMatchesFound = totalMatchesFoundCollection.Count;
				}
			}
			if (totalMatchesFound != 0)
			{
				foreach (OrgPatternMatch similarOrgMatch in totalMatchesFoundCollection)
				{
					similarOrgMatch.SetMatchScoreForOrg(patternMatchesForOrganisationToMatch);
				}

				RankAndAddOrgMatches(totalMatchesFoundCollection.OrderByDescending(x => x.OS_Score), patternMatchesForOrganisationToMatch);
			}
			else if (Count == 0 && shouldIncludeUnmatchedOrgInList)
			{
				GetUnmatchedOrgPatternAndScore();
			}

			SetReadOnlyIncludingChildren(true);
		}

		public void GetUnmatchedOrgPatternAndScore()
		{
			UnmatchedOrgMatcher.GetUnmatchedOrgPatternIfRegistryConfigurationAllowsIt(this);
			for (int i = 0; i < Count; i++)
			{
				this[i].OS_Score = HighMatchThreshold;
				this[i].OS_Rank = i + 1;
			}
		}

		void RankAndAddOrgMatches(IEnumerable<OrgPatternMatch> orgMatches, OrgPatternMatchCollection patternMatchesForOrganisationToMatch)
		{
			int currentRank = 1;

			foreach (OrgPatternMatch match in orgMatches)
			{
				if (match.OS_Score >= patternMatchesForOrganisationToMatch.LikelyMatchScore && currentRank <= MaximumResultsToShow)
				{
					if (!PatternMatchExistsForSameOrgAndSameAddress(match))
					{
						match.OS_Rank = currentRank;
						Add(match);
						currentRank++;
					}
				}
			}
		}

		bool PatternMatchExistsForSameOrgAndSameAddress(OrgPatternMatch match)
		{
			return this.Cast<OrgPatternMatch>().Any(p => p.OS_OH == match.OS_OH && p.OS_OA == match.OS_OA);
		}

		#endregion

		#region Get Filter

		public IEnumerable<ZQuery> GetFilter()
		{
			return GetFilter(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public IEnumerable<ZQuery> GetFilter(bool allowMorePotentialMatches)
		{
			var result = new List<ZQuery>();

			var subQuery = new ZQuery();
			bool shouldAddForOS_OH = (Count > 0) && !this[0].OS_OH.IsEmpty;

			foreach (var unlocoGroup in BusinessObjectGroup<OrgPatternMatch>.GroupByColumn(OrgPatternMatchSchema.OS_UNLOCO, Elements.Cast<OrgPatternMatch>()))
			{
				var unlocoFilter = GetUnlocoFilter((ZString)unlocoGroup.GroupKey);

				foreach (var regnoGroup in BusinessObjectGroup<OrgPatternMatch>.GroupByColumnAndJoinAllValues(
					OrgPatternMatchSchema.OS_BusinessRegNo,
					new SchemaColumn[]
					{
						OrgPatternMatchSchema.OS_CompanyName1,
						OrgPatternMatchSchema.OS_CompanyName2,
						OrgPatternMatchSchema.OS_CompanyName3,
						OrgPatternMatchSchema.OS_CompanyName4,
						OrgPatternMatchSchema.OS_Address1,
						OrgPatternMatchSchema.OS_Address2,
						OrgPatternMatchSchema.OS_Address3,
						OrgPatternMatchSchema.OS_Address4,
						OrgPatternMatchSchema.OS_FullCompanyName,
						OrgPatternMatchSchema.OS_Email,
						OrgPatternMatchSchema.OS_Phone,
						OrgPatternMatchSchema.OS_FaxNum
					},
					unlocoGroup.Elements))
				{
					ZQuery regnoQuery;

					var collectionKey = regnoGroup.GroupKey as ICollection;
					if (collectionKey != null)
					{
						regnoQuery = new ZQuery();
						foreach (ZString regno in collectionKey)
						{
							regnoQuery.AddToFilter(GetBusinessRegNoFilter(regno), JoinCondition.Or);
						}
					}
					else
					{
						regnoQuery = GetBusinessRegNoFilter((ZString)regnoGroup.GroupKey);
					}

					int processedElementsCount = 0;
					subQuery = new ZQuery();

					foreach (var match in regnoGroup.Elements)
					{
						if ((++processedElementsCount % 100) == 0)
						{
							subQuery.AddToFilter(regnoQuery, GetBusinessRegNoJoinCondition());
							subQuery.AddToFilter(unlocoFilter, GetUnlocoFilterJoinCondition());
							if (shouldAddForOS_OH)
							{
								subQuery.AddToFilter(OrgPatternMatchSchema.OS_OH, SQLComparisonOperator.NotEqual, this[0].OS_OH);
							}
							result.Add(subQuery);
							subQuery = new ZQuery();
						}

						subQuery.AddToFilter(match.GetFilterExcludingUnlocoAndBusinessRegNo(allowMorePotentialMatches), JoinCondition.Or);
					}

					if (!subQuery.IsEmpty)
					{
						subQuery.AddToFilter(regnoQuery, GetBusinessRegNoJoinCondition());
						subQuery.AddToFilter(unlocoFilter, GetUnlocoFilterJoinCondition());
						if (shouldAddForOS_OH)
						{
							subQuery.AddToFilter(OrgPatternMatchSchema.OS_OH, SQLComparisonOperator.NotEqual, this[0].OS_OH);
						}
						subQuery.AddOptionRecompileConditionally = true;
						result.Add(subQuery);
						subQuery = new ZQuery();
					}
				}
			}

			if (result.Count == 0 || !subQuery.IsEmpty)
			{
				result.Add(subQuery);
			}

			return result;
		}

		protected virtual ZQuery GetBusinessRegNoFilter(ZString businessRegNo)
		{
			return OrgPatternMatch.ExcludeOrgsWithDifferentBusinessRegNo(businessRegNo);
		}

		protected virtual JoinCondition GetBusinessRegNoJoinCondition()
		{
			return JoinCondition.And;
		}

		protected virtual ZQuery GetUnlocoFilter(ZString unloco)
		{
			return OrgPatternMatch.ExcludeOrgsNotInSameCountry(unloco);
		}

		protected virtual JoinCondition GetUnlocoFilterJoinCondition()
		{
			return JoinCondition.And;
		}

		#endregion

		#region MaximumResultsToShow

		int maximumResultsToShow = 30;
		public int MaximumResultsToShow
		{
			get { return maximumResultsToShow; }
			set { maximumResultsToShow = value; }
		}

		#endregion
	}
}
