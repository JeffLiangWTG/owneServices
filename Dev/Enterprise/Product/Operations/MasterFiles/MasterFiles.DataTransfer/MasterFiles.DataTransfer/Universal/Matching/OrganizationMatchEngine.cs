using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Matching
{
	internal static class OrganizationMatchEngine
	{
		public static OrgHeader GetMatchingOrgHeader(IOrgHeaderForMatching orgMatchingData, BusinessObjectFactory factory)
		{
			return GetMatchingOrgHeader(OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(orgMatchingData), factory);
		}

		static OrgHeader GetMatchingOrgHeader(DeduplicationOrgHeader deDuplicationOrgHeader, BusinessObjectFactory factory)
		{
			OrgHeader resultOrg = null;
			var finder = new OrgHeaderMatchEngineFinder(deDuplicationOrgHeader, true, true);
			var potentialMatches = finder.FindPotentialDuplicates(false);

			if (potentialMatches.Any())
			{
				var result = potentialMatches.OrderByDescending(scoringResult => scoringResult.Score).First();
				resultOrg = factory.Load<OrgHeader>(result.TargetPK);
			}
			return resultOrg;
		}

		public static OrgAddress GetMatchingOrgAddress(IOrgHeaderForMatching orgMatchingData, ZGuid targetOrganizationPk, BusinessObjectFactory factory, ISimpleLogger logger)
		{
			OrgAddress matchedAddress = null;

			var deDuplicationOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(orgMatchingData);
			var orgAddress = deDuplicationOrgHeader.OrgAddresses.FirstOrDefault();
			if (orgAddress != null)
			{
				ScoringResult matchedScoringResult = null;
				OrgAddress highestMatchedAddress = null;
				ScoringResult highestMatchedScoringResult = null;

				var finder = targetOrganizationPk.IsEmpty ? new OrgHeaderMatchEngineFinder(deDuplicationOrgHeader, true, true) : new OrgHeaderMatchEngineFinder(deDuplicationOrgHeader, true, true, targetOrganizationPk);
				var potentialMatches = finder.FindPotentialDuplicates(false);

				if (potentialMatches.Any())
				{
					var orgAddressMinimumConfidenceScore = OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.Value / 100d;
					var potentialMatchesWithActiveStatus = CombineScoringWithActiveStatus(factory, potentialMatches);
					var matchResults = potentialMatchesWithActiveStatus.OrderByDescending(o => o.ActiveStatus).ThenByDescending(o => o.Match.Score).Select(o => o.Match).ToArray();

					foreach (var matchResult in matchResults)
					{
						var addressResult = matchResult.ChildResults.OrderByDescending(childResult => childResult.Score).FirstOrDefault(childResult => childResult.MasterType == typeof(IOrgAddress));
						if (addressResult != null)
						{
							var address = factory.Load<OrgAddress>(addressResult.TargetPK);
							if (address != null)
							{
								if (highestMatchedAddress == null)
								{
									highestMatchedAddress = address;
									highestMatchedScoringResult = addressResult;
								}

								if (addressResult.Score > orgAddressMinimumConfidenceScore)
								{
									matchedAddress = address;
									matchedScoringResult = matchResult;
									break;
								}
							}
						}
					}
				}

				var companyName = deDuplicationOrgHeader.OH_FullName;
				if (matchedAddress != null)
				{
					LogMatchedAddressInfo(companyName, orgAddress, matchedAddress, matchedScoringResult, logger);
				}
				else
				{
					if (finder.MeetMinimumRequirements)
					{
						logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Unable to match address for '[Company Name: {0}; Address 1: {1}; City: {2}]'", companyName, orgAddress.OA_Address1, orgAddress.OA_City));

						if (highestMatchedAddress != null)
						{
							LogHighestMatchInfo(highestMatchedAddress, highestMatchedScoringResult, logger);
						}
					}
					else
					{
						logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Not enough information to match address for '[Company Name: {0}; Address 1: {1}; City: {2}]'", companyName, orgAddress.OA_Address1, orgAddress.OA_City));
					}
				}
			}

			return matchedAddress;
		}

		static List<(ScoringResult Match, bool ActiveStatus)> CombineScoringWithActiveStatus(BusinessObjectFactory factory, IReadOnlyList<ScoringResult> potentialMatches)
		{
			var potentialMatchesWithActiveStatus = new List<(ScoringResult Match, bool ActiveStatus)>();
			var targetPKs = potentialMatches.Select(x => x.TargetPK);
			var matchedOrganizations = factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, targetPKs)).ToDictionary(x => x.PK);

			foreach (var match in potentialMatches)
			{
				if (matchedOrganizations.TryGetValue(match.TargetPK, out var orgHeader))
				{
					potentialMatchesWithActiveStatus.Add((match, orgHeader.OH_IsActive));
				}
			}

			return potentialMatchesWithActiveStatus;
		}

		static void LogMatchedAddressInfo(string companyName, IOrgAddress orgAddress, OrgAddress matchedAddress, ScoringResult matchedScoringResult, ISimpleLogger logger)
		{
			logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, " Matched to Organization '{0}' and corresponding Organization Address '{1}' OverallMatch = {2}, AddressConfidence = {3}, NameConfidence = {4}",
				matchedAddress.Header?.OH_Code,
				matchedAddress.OA_Code,
				matchedScoringResult.ConfidenceRating.ToString(),
				matchedScoringResult.ChildResults.FirstOrDefault(x => x.MasterType == typeof(IOrgAddress))?.ConfidenceRating.ToString(),
				matchedScoringResult.ChildResults.FirstOrDefault(x => x.MasterType == typeof(MultiSourceCompanyName))?.ConfidenceRating.ToString()));
			VerboseLogging(companyName, orgAddress, matchedAddress, logger);
		}

		static void VerboseLogging(string companyName, IOrgAddress address, OrgAddress matchedAddress, ISimpleLogger logger)
		{
			logger.LogVerboseOnly(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)@"
Matching
(
	Name = {0}
	Address1 = {1}
	Address2 = {2}
	City = {3}
	Postcode = {4}
	State = {5}
	Country = {6}
)
: Matched to Organization '{7}' and corresponding Organization Address '{8}'
(
	Name = {9}
	Address1 = {10}
	Address2 = {11}
	City = {12}
	Postcode = {13}
	State = {14}
	Country = {15}
)
",
companyName,
address.OA_Address1,
address.OA_Address2,
address.OA_City,
address.OA_PostCode,
address.OA_State,
address.OA_RN_NKCountryCode,
matchedAddress.Header?.OH_Code,
matchedAddress.OA_Code,
matchedAddress.Header?.OH_FullName,
matchedAddress.OA_Address1,
matchedAddress.OA_Address2,
matchedAddress.OA_City,
matchedAddress.OA_PostCode,
matchedAddress.OA_State,
matchedAddress.OA_RN_NKCountryCode));
		}

		static void LogHighestMatchInfo(OrgAddress highestMatchedAddress, ScoringResult result, ISimpleLogger logger)
		{
			logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, @": No match met the threshold. The highest match was to Organization '{0}' and corresponding to Organization Address '{1}' but it was ignored as it did not meet the specified threshold.
	Name = {2}
	Address1 = {3}
	Address2 = {4}
	City = {5}
	Postcode = {6}
	State = {7}
	Country = {8}
OverallMatch = {9}, AddressConfidence = {10}, NameConfidence = {11}
",
highestMatchedAddress.Header?.OH_Code,
highestMatchedAddress.OA_Code,
highestMatchedAddress.Header?.OH_FullName,
highestMatchedAddress.OA_Address1,
highestMatchedAddress.OA_Address2,
highestMatchedAddress.OA_City,
highestMatchedAddress.OA_PostCode,
highestMatchedAddress.OA_State,
highestMatchedAddress.OA_RN_NKCountryCode,
result.ConfidenceRating.ToString(),
result.ChildResults.FirstOrDefault(x => x.MasterType == typeof(IOrgAddress))?.ConfidenceRating.ToString(),
result.ChildResults.FirstOrDefault(x => x.MasterType == typeof(MultiSourceCompanyName))?.ConfidenceRating.ToString()));
		}
	}
}
