using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class DpsCandidateCreator
	{
		public void NewSeparatedOrganizationNames(List<DpsNameCandidate> candidateNames, string name, string nameType)
		{
			var separatedNames = FillSeparatedNames(name, ConvertSeparatorsToRegexPattern());
			foreach (var separatedName in separatedNames)
			{
				candidateNames.Add(NewNameCandidate(separatedName, nameType));
			}
		}

		public DpsNameCandidate NewNameCandidate(string name, string nameType)
		{
			return new DpsNameCandidate { NameType = nameType, FullName = name };
		}

		public DpsAddressCandidate NewAddressCandidate(string address1, string address2, string city, string state, string postCode, string country, string additionalAddressLine)
		{
			return new DpsAddressCandidate { Address1 = address1, Address2 = address2, City = city, State = state, PostCode = postCode, Country = country, AdditionalAddressLine = additionalAddressLine };
		}

		public DpsRegistrationCodeCandidate NewRegistrationCodeCandidate(string regCountryCode, string regCodeType, string regCodeValue)
		{
			return new DpsRegistrationCodeCandidate { RegCountryCode = regCountryCode, RegCodeType = regCodeType, RegCodeValue = regCodeValue };
		}

		(List<DpsRegistrationCodeCandidate> candidateRegCodes, List<string> candidateCountries) NewVesselCandidate(RefVessel vessel)
		{
			var candidateRegCodes = new List<DpsRegistrationCodeCandidate>();
			var candidateCountries = new List<string>();

			if (!vessel.RV_LloydsNumber.IsEmpty)
			{
				candidateRegCodes.Add(NewRegistrationCodeCandidate(vessel.RV_RN_NKCountryOfReg, "IMO", vessel.RV_LloydsNumber));
			}

			if (!vessel.RV_RN_NKCountryOfReg.IsEmpty)
			{
				candidateCountries.Add(vessel.RV_RN_NKCountryOfReg);
			}

			return (candidateRegCodes, candidateCountries);
		}

		internal DpsNameCandidate NewNameCandidate(RefVessel vessel)
		{
			return NewNameCandidate(vessel.RV_Code, DeniedPartyConstants.ScreeningNameTypes.Vessel);
		}

		internal DpsNameCandidate NewNameCandidate(IScreeningPartyForVessel notLinkedVessel)
		{
			return NewNameCandidate(notLinkedVessel.Code, DeniedPartyConstants.ScreeningNameTypes.Vessel);
		}

		internal DpsAddressCandidate NewAddressCandidate(OrgAddress address)
		{
			return NewAddressCandidate(address.OA_Address1, address.OA_Address2, address.OA_City, address.OA_State, address.OA_PostCode, address.OA_RN_NKCountryCode, address.OA_AdditionalAddressInformation);
		}

		internal DpsAddressCandidate NewAddressCandidate(JobDocAddress docAddress)
		{
			return NewAddressCandidate(docAddress.E2_Address1, docAddress.E2_Address2, docAddress.E2_City, docAddress.E2_State, docAddress.E2_Postcode, docAddress.E2_RN_NKCountryCode, docAddress.E2_AdditionalAddressInformation);
		}

		(List<DpsNameCandidate>, List<DpsAddressCandidate>, List<DpsRegistrationCodeCandidate>, List<string>) GetCandidateLists(OrgHeader organization)
		{
			var candidateNames = new List<DpsNameCandidate>();
			var candidateAddresses = new List<DpsAddressCandidate>();
			var candidateRegCodes = new List<DpsRegistrationCodeCandidate>();
			var candidateCountries = new List<string>();
			var nameType = organization.OH_Category == OrgConstants.Category.NaturalPersonIndividual ? DeniedPartyConstants.ScreeningNameTypes.NaturalPerson : DeniedPartyConstants.ScreeningNameTypes.Organization;

			NewSeparatedOrganizationNames(candidateNames, organization.OH_FullName, nameType);

			foreach (OrgBrandOrRelatedName brand in organization.BrandsOrRelatedNames.ToArray())
			{
				NewSeparatedOrganizationNames(candidateNames, brand.P1_RelatedName, nameType);
			}

			foreach (OrgAddress address in organization.Addresses.ToArray())
			{
				if (!address.IsCancelled)
				{
					if (!address.OA_CompanyNameOverride.IsEmpty)
					{
						NewSeparatedOrganizationNames(candidateNames, address.OA_CompanyNameOverride, DeniedPartyConstants.ScreeningNameTypes.Organization);
					}

					candidateAddresses.Add(NewAddressCandidate(address));

					if (!address.OA_RN_NKCountryCode.IsEmpty)
					{
						candidateCountries.Add(address.OA_RN_NKCountryCode);
					}
				}
			}

			foreach (OrgCusCode code in organization.CustomsCodes)
			{
				candidateRegCodes.Add(NewRegistrationCodeCandidate(code.OK_RN_NKCodeCountry, code.OK_CodeType, code.OK_CustomsRegNo));

				if (!code.OK_RN_NKCodeCountry.IsEmpty)
				{
					candidateCountries.Add(code.OK_RN_NKCodeCountry);
				}
			}

			return (candidateNames, candidateAddresses, candidateRegCodes, candidateCountries);
		}

		IEnumerable<string> FillSeparatedNames(string originalName, string pattern)
		{
			if (!string.IsNullOrWhiteSpace(originalName) && !string.IsNullOrEmpty(pattern))
			{
				var results = new List<string>();
				var names = Regex.Split(originalName.Trim(), pattern, RegexOptions.IgnoreCase).Where(n => !string.IsNullOrWhiteSpace(n)).ToArray();
				if (names.Length >= 2)
				{
					results.AddRange(names.Select(n => n.Trim()));
				}
				else
				{
					results.Add(originalName);
				}

				return results;
			}

			return new[] { originalName };
		}

		public DpsRequestHeaderWithAddressMatching NewRequestHeader(OrgHeader organization)
		{
			var candidateLists = GetCandidateLists(organization);
			return GetDpsRequestHeader(candidateLists.Item1, candidateLists.Item2, candidateLists.Item3, candidateLists.Item4);
		}

		public DpsRequestHeaderWithAddressMatching NewRequestHeader(RefCountry country)
		{
			var candidateCountries = new List<string>();
			candidateCountries.Add(country.RN_Code);

			return GetDpsRequestHeader(null, null, null, candidateCountries);
		}

		public DpsRequestHeaderWithAddressMatching NewRequestHeader(string name, string address1, string address2, string city, string state, string postCode, string country, string additionalAddressLine, bool shouldBeConsideredAsOrganization = false)
		{
			var nameCandidate = NewNameCandidate(name, shouldBeConsideredAsOrganization ? DeniedPartyConstants.ScreeningNameTypes.Organization : DeniedPartyConstants.ScreeningNameTypes.NaturalPerson);
			var addressCandidate = NewAddressCandidate(address1, address2, city, state, postCode, country, additionalAddressLine);
			return GetDpsRequestHeader(new[] { nameCandidate }, new[] { addressCandidate }, null, null);
		}

		internal DpsRequestHeaderWithAddressMatching NewRequestHeader(JobDocAddress docAddress)
		{
			var candidateNames = new List<DpsNameCandidate>();
			var candidateAddresses = new List<DpsAddressCandidate>();
			var candidateCountries = new List<string>();

			if (!docAddress.E2_CompanyName.IsEmpty)
			{
				NewSeparatedOrganizationNames(candidateNames, docAddress.E2_CompanyName, docAddress.E2_IsResidential ? DeniedPartyConstants.ScreeningNameTypes.NaturalPerson : DeniedPartyConstants.ScreeningNameTypes.Organization);
			}

			candidateAddresses.Add(NewAddressCandidate(docAddress));

			if (!docAddress.E2_RN_NKCountryCode.IsEmpty)
			{
				candidateCountries.Add(docAddress.E2_RN_NKCountryCode);
			}

			return GetDpsRequestHeader(candidateNames, candidateAddresses, null, candidateCountries);
		}

		public DpsRequestHeaderWithAddressMatching NewRequestHeader(RefVessel vessel)
		{
			var (candidateRegCodes, candidateCountries) = NewVesselCandidate(vessel);

			return GetDpsRequestHeader(new[] { NewNameCandidate(vessel) }, null, candidateRegCodes, candidateCountries);
		}

		internal DpsRequestHeaderWithAddressMatching NewRequestHeader(IScreeningPartyForVessel notLinkedVessel)
		{
			return GetDpsRequestHeader(new[] { NewNameCandidate(notLinkedVessel) }, null, null, null);
		}

		public DpsRequestHeaderWithAddressMatching GetDpsRequestHeader(IEnumerable<DpsNameCandidate> dpsNameCandidates, IEnumerable<DpsAddressCandidate> dpsAddressCandidates, IEnumerable<DpsRegistrationCodeCandidate> dpsRegistrationCodeCandidates, IEnumerable<string> dpsCountryCandidates)
		{
			var dpsFeatureControlHelper = new DpsFeatureControlHelper();
			var featureControlFlags = dpsFeatureControlHelper.GetFeatureControlAddressOnlyFlags;

			return new DpsRequestHeaderWithAddressMatching { DpsNameCandidates = dpsNameCandidates?.Distinct(dpsNameCandidateComparer), DpsAddressCandidates = dpsAddressCandidates?.Distinct(dpsAddressCandidateComparer), DpsRegistrationCodeCandidates = dpsRegistrationCodeCandidates?.Distinct(dpsRegistrationCodeCandidateComparer), DpsCountryCandidates = dpsCountryCandidates?.Distinct().Select(s => new DpsCountryCandidate { CountryCode = s }), IsAddressOnlyScreeningIncluded = featureControlFlags.isAddressOnlyScreeningIncluded, IsAllAddressesIncluded = featureControlFlags.isAllAddressesIncluded, AddressMatchingLevel = featureControlFlags.matchingLevel };
		}

		string ConvertSeparatorsToRegexPattern()
		{
			return string.Join("|", OrganisationsDataRegistry.Instance.NameSeparators.Value.Select(n => " " + n.Trim() + " ").ToArray().Select(Regex.Escape));
		}

		readonly DpsNameCandidateComparer dpsNameCandidateComparer = new DpsNameCandidateComparer();
		readonly DpsAddressCandidateComparer dpsAddressCandidateComparer = new DpsAddressCandidateComparer();
		readonly DpsRegistrationCodeCandidateComparer dpsRegistrationCodeCandidateComparer = new DpsRegistrationCodeCandidateComparer();
	}
}
