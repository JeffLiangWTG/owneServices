using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	class PatternMatchCreator
	{
		internal PatternMatchCreator(IPatternMatchDataManager dataManager)
		{
			this.dataManager = Argument.NotNull(dataManager, "OrgPatternMatchDataManager dataManager");
			this.existingPatternMatches = new HashSet<IOrgPatternMatch>(dataManager.GetPatternMatchesAlreadyLoaded(), new IOrgPatternMatchComparer());
		}

		readonly IPatternMatchDataManager dataManager;
		readonly HashSet<IOrgPatternMatch> existingPatternMatches;

		internal void CreateMatchesForAddressesAndCompanyNames(IEnumerable<OrganisationName> companyNames, IEnumerable<IMatchingAddress> orgAddresses, Dictionary<string, Dictionary<IMatchingAddress, object>> addedMatches)
		{
			foreach (var companyName in companyNames)
			{
				Dictionary<IMatchingAddress, object> matchedAddresses;
				addedMatches.TryGetValue(companyName.Value, out matchedAddresses);

				foreach (var address in orgAddresses)
				{
					if (address.OA_IsActive && (matchedAddresses == null || !matchedAddresses.ContainsKey(address)))
					{
						CreateMatches(companyName, address);

						if (matchedAddresses == null)
						{
							matchedAddresses = new Dictionary<IMatchingAddress, object>();
							addedMatches.Add(companyName.Value, matchedAddresses);
						}
						matchedAddresses.Add(address, null);
					}
				}
			}
		}

		internal void CreateMatchesForCusCodes(IEnumerable<IMatchingCusCode> newOrModifiedOrgCusCodes, Dictionary<string, Dictionary<IMatchingAddress, object>> addedMatches)
		{
			foreach (var code in newOrModifiedOrgCusCodes)
			{
				if (!code.IsInDatabase && !code.OK_CustomsRegNo.IsEmpty && code.OK_CodeType.IsAcceptedForMatching())
				{
					foreach (var companyName in dataManager.Organisation.AllOrganisationNames())
					{
						Dictionary<IMatchingAddress, object> matchedAddresses;
						addedMatches.TryGetValue(companyName.Value, out matchedAddresses);

						foreach (var address in dataManager.Organisation.Addresses)
						{
							if (CanGeneratePatternMatch(dataManager.Organisation, companyName, address, dataManager.AllowPatternMatchesWithEmptyAddress))
							{
								if (matchedAddresses == null || !matchedAddresses.ContainsKey(address))
								{
									CreateMatch(companyName, address, code.OK_CustomsRegNo);
								}
							}
						}
					}
				}
			}
		}

		void CreateMatches(OrganisationName companyName, IMatchingAddress address)
		{
			if (CanGeneratePatternMatch(dataManager.Organisation, companyName, address, dataManager.AllowPatternMatchesWithEmptyAddress))
			{
				var applicableCusCodes = dataManager.Organisation.CustomsCodes.Where(cusCode => cusCode.OK_CodeType.IsAcceptedForMatching()).ToArray();

				if (applicableCusCodes.Length > 0)
				{
					foreach (IMatchingCusCode code in applicableCusCodes)
					{
						CreateMatch(companyName, address, code.OK_CustomsRegNo);
					}
				}
				else
				{
					CreateMatch(companyName, address, "");
				}
			}
		}

		void CreateMatch(OrganisationName companyName, IMatchingAddress address, string localBusinessNumber)
		{
			var newMatch = dataManager.CreateNewPatternMatch();

			new PatternMatchRowBuilder(newMatch).EncodeOrganisation(dataManager.Organisation, companyName, address, localBusinessNumber);

			if (!existingPatternMatches.Contains(newMatch))
			{
				existingPatternMatches.Add(newMatch);
			}
			else
			{
				newMatch.Delete();
			}
		}

		static bool CanGeneratePatternMatch(IMatchingOrganisation organisation, OrganisationName companyName, IMatchingAddress address, bool allowEmptyAddress)
		{
			return SpecifiedOrgIsValid(organisation)
				&& !companyName.IsEmpty
				&& SpecifiedAddressIsValid(address, allowEmptyAddress)
				&& (companyName.OrgAddressPK.IsEmpty || companyName.OrgAddressPK == address.PK);
		}

		static bool SpecifiedOrgIsValid(IMatchingOrganisation organisation)
		{
			return organisation != null && !organisation.OH_FullName.IsEmpty;
		}

		static bool SpecifiedAddressIsValid(IMatchingAddress address, bool allowEmptyAddress)
		{
			return address != null && address.OA_Address1 != OrgAddress.AddressNotOnFile && (allowEmptyAddress || !address.OA_Address1.IsEmpty);
		}

		#region Test Points
#if DEBUG

		internal static bool CanGeneratePatternMatchForTesting(IMatchingOrganisation organisation, OrganisationName companyName, IMatchingAddress address, bool allowEmptyAddress)
		{
			return CanGeneratePatternMatch(organisation, companyName, address, allowEmptyAddress);
		}

#endif
		#endregion
	}
}
