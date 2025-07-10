using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	#region OrgMatchType

	/// <summary>
	/// The type of match found when searching for an organisation
	/// </summary>
	public enum OrgMatchType
	{
		/// <summary>
		/// Match was made from an override in the OrgPatternMatchOverride table
		/// </summary>
		OverrideMatch,

		/// <summary>
		/// Organisation name matches exactly
		/// </summary>
		ExactMatch,

		/// <summary>
		/// More than one OrgHeader had exactly the same name
		/// </summary>
		MultipleExactMatches,

		/// <summary>
		/// A match was found after filtering punctuation and ignored words (PTY, LTD, etc) from the name
		/// </summary>
		FilteredNameMatch,

		/// <summary>
		/// More than one organisation matched the filtered name, hence no result can be provided
		/// </summary>
		MultipleFilteredNameMatches,

		NoMatchesFound
	}

	#endregion

	#region OrgMatchResult

	public class OrgMatchResult
	{
		/// <summary>
		/// The type of result from the search - no matches found, exact match, etc
		/// </summary>
		public OrgMatchType MatchType = OrgMatchType.NoMatchesFound;

		/// <summary>
		/// The organisation that was found. If no match could be made, this will be null
		/// </summary>
		public OrgHeader MatchingOrg;
	}

	#endregion

	public class OrgMatcher
	{
		public OrgMatcher(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public OrgMatchResult MatchToSimilarOrganisations(OrgHeader orgToMatch)
		{
			OrgMatchResult result = new OrgMatchResult();

			if (orgToMatch != null)
			{
				ISimilarOrganisationsFinder similarOrganisationFinder = orgToMatch.SimilarOrgFinder;
				similarOrganisationFinder.FindSimilarOrganisations();

				OrgHeader matchedOrg = null;
				if (similarOrganisationFinder.LikelyMatchFound)
				{
					foreach (OrgPatternMatch match in orgToMatch.SimilarOrgMatches)
					{
						if (match.OS_Rank == 1 && AdditionalOrgPatternMatch(orgToMatch, match))
						{
							matchedOrg = Factory.Load<OrgHeader>(match.OS_OH);
							break;
						}
					}
				}
				if (matchedOrg != null)
				{
					result.MatchType = OrgMatchType.ExactMatch;
					result.MatchingOrg = matchedOrg;
				}
			}
			return result;
		}

		protected virtual bool AdditionalOrgPatternMatch(OrgHeader orgToMatch, OrgPatternMatch match)
		{
			return true;
		}

		/// <summary>
		/// Searches for an organisation, based on its name
		/// </summary>
		/// <param name="orgWithOverrides">The Organisation which has the list of OrgPatternMatch overrides which will be searched</param>
		/// <param name="orgName">The name to search for</param>
		public OrgMatchResult MatchOrgName(OrgHeader orgWithOverrides, ZString orgName)
		{
			if (orgWithOverrides == null)
			{
				throw new ArgumentNullException(nameof(orgWithOverrides), "OrgWithOverrides");
			}
			OrgMatchResult match = SearchPatternMatchOverride(orgWithOverrides, orgName);
			if (match.MatchType == OrgMatchType.NoMatchesFound)
			{
				match = SearchForExactMatch(orgName);
				if (match.MatchType == OrgMatchType.NoMatchesFound)
				{
					match = SearchPatternMatch(orgName);
				}
			}
			return match;
		}

		public OrgMatchResult MatchOrgWithoutOverrides(ZString orgName, ZString port)
		{
			OrgMatchResult match = SearchForExactMatch(orgName, port);
			if (match.MatchType == OrgMatchType.NoMatchesFound)
			{
				match = SearchPatternMatch(orgName, port);
			}
			return match;
		}

		#region Search Pattern Match Override

		OrgMatchResult SearchPatternMatchOverride(OrgHeader orgWithOverrides, ZString orgName)
		{
			ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_ForeignCode, orgName)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, orgWithOverrides.PK)
			.AddToFilter(JoinCondition.And, OrgPatternMatchOverrideSchema.OO_Relationship, SQLComparisonOperator.Equal, Constants.OrgPatternMatchOverrideRelationships.Organisation);

			var matchOverrides = orgWithOverrides.Factory.Load<OrgPatternMatchOverride>(filter);
			OrgMatchResult match = new OrgMatchResult();

			if (matchOverrides.Length == 1)
			{
				match.MatchType = OrgMatchType.OverrideMatch;
				match.MatchingOrg = (OrgHeader)Factory.Load(typeof(OrgHeader), matchOverrides[0].OO_LocalGuid);
			}
			else
			{
				match.MatchingOrg = null;
				match.MatchType = OrgMatchType.NoMatchesFound;
			}

			return match;
		}

		#endregion

		#region Search Exact Match

		OrgMatchResult SearchForExactMatch(ZString orgName)
		{
			return SearchForExactMatch(orgName, ZString.Empty);
		}

		OrgMatchResult SearchForExactMatch(ZString orgName, ZString port)
		{
			OrgMatchResult match = new OrgMatchResult();
			ZQuery exactMatchFilter = new ZQuery();

			if (!orgName.IsEmpty)
			{
				exactMatchFilter.AddToFilter(OrgHeaderSchema.OH_FullName, orgName);
			}
			else if (port.IsEmpty)
			{
				exactMatchFilter.IsNoResultQuery = true;
			}
			if (!port.IsEmpty)
			{
				exactMatchFilter.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.Equal, port);
			}

			OrgHeaderCollection matches = new OrgHeaderCollection(Factory, exactMatchFilter);
			matches.Load();

			if (matches.Count == 1)
			{
				match.MatchType = OrgMatchType.ExactMatch;
				match.MatchingOrg = matches[0];
			}
			else if (matches.Count > 1)
			{
				match.MatchType = OrgMatchType.MultipleExactMatches;
				match.MatchingOrg = null;
			}
			else
			{
				match.MatchType = OrgMatchType.NoMatchesFound;
				match.MatchingOrg = null;
			}

			return match;
		}

		#endregion

		#region Search Pattern Match

		OrgMatchResult SearchPatternMatch(ZString orgName)
		{
			return SearchPatternMatch(orgName, ZString.Empty);
		}

		OrgMatchResult SearchPatternMatch(ZString orgName, ZString port)
		{
			OrgMatchResult match = new OrgMatchResult();

			ZQuery filter = PatternMatchFilter(orgName, port);
			OrgPatternMatchCollection patternMatches = new OrgPatternMatchCollection(Factory, filter);
			patternMatches.Load();

			if (patternMatches.Count > 1)
			{
				match.MatchType = OrgMatchType.MultipleFilteredNameMatches;
				match.MatchingOrg = null;
			}
			else if (patternMatches.Count == 1)
			{
				match.MatchType = OrgMatchType.FilteredNameMatch;
				match.MatchingOrg = patternMatches[0].Header;
			}
			else if (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled)
			{
				match.MatchType = OrgMatchType.ExactMatch;
				match.MatchingOrg = (OrgHeader)Factory.Load(typeof(OrgHeader), OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);
			}
			else
			{
				match.MatchType = OrgMatchType.NoMatchesFound;
				match.MatchingOrg = null;
			}

			return match;
		}

		public ZQuery PatternMatchFilter(ZString orgName, ZString port)
		{
			ZQuery filter = new ZQuery();

			if (!orgName.IsEmpty)
			{
				BusinessObjectFactory tempFactory = PatternMatchFactory;
				OrgHeader tempOrg = (OrgHeader)tempFactory.New(typeof(OrgHeader));
				tempOrg.OH_FullName = orgName.Left(tempOrg.OH_FullNameInfo.MaxLength);

				OrgPatternMatch match = tempOrg.PatternMatchesForThisOrg.AddNew();
				match.EncodeOrganisation(tempOrg, new StringWithLanguage(tempOrg.OH_FullName, tempOrg.OH_Language), tempOrg.MainAddress, "");

				OrgPatternMatchGenerationHelper patternMatchGenerationHelper = OrgPatternMatchGenerationHelper.Get(tempOrg.OH_Language, tempOrg.PortName, tempOrg.CountryName);

				match.EncodeName(new StringWithLanguage(tempOrg.OH_FullName, tempOrg.OH_Language), patternMatchGenerationHelper, true);
				ZString nameWithSplitWords = match.OS_FullCompanyName;

				match.EncodeName(new StringWithLanguage(tempOrg.OH_FullName, tempOrg.OH_Language), patternMatchGenerationHelper, false);
				ZString nameWithCollapsedWords = match.OS_FullCompanyName;

				filter.AddToFilter(OrgPatternMatchSchema.OS_FullCompanyName, nameWithSplitWords);

				if (nameWithSplitWords != nameWithCollapsedWords)
				{
					// Handle case where name may be stored as one or two words, ie: COCA-COLA should match COCA COLA or COCACOLA
					filter.AddToFilter(JoinCondition.Or, OrgPatternMatchSchema.OS_FullCompanyName, SQLComparisonOperator.Equal, nameWithCollapsedWords);
				}
			}
			else if (port.IsEmpty)
			{
				filter.IsNoResultQuery = true;
			}

			if (!port.IsEmpty)
			{
				ZQuery portFilter = new ZQuery(OrgPatternMatchSchema.OS_UNLOCO, port);
				filter.AddToFilter(portFilter, JoinCondition.And);
			}

			filter.AddToFilter(OrgPatternMatchSchema.OS_OH, SQLComparisonOperator.NotEqual, OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);

			return filter;
		}

		#endregion

		#region Implementation

		readonly BusinessObjectFactory Factory;

		protected BusinessObjectFactory PatternMatchFactory
		{
			get
			{
				if (fPatternMatchFactory == null)
				{
					fPatternMatchFactory = new BusinessObjectFactory();
				}
				return fPatternMatchFactory;
			}
		}
		BusinessObjectFactory fPatternMatchFactory;

		#endregion
	}
}
