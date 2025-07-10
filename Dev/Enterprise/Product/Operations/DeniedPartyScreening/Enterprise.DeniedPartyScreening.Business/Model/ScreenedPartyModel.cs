using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class ScreenedPartyModel
	{
		public ScreenedPartyModel(DpsResponseWithScreeningParty responseWithScreeningParty, BusinessObjectFactory factory, bool forceAllLists = false)
		{
			Argument.NotNull(responseWithScreeningParty, nameof(responseWithScreeningParty));
			Argument.NotNull(factory, nameof(factory));

			this.forceAllLists = forceAllLists;
			ResponseWithScreeningParty = responseWithScreeningParty;
			Factory = factory;
			InitMatchModels();
		}

		readonly bool forceAllLists;

		public static MultilingualString SourceListWatermark => ResString.GetMultilingualString("EAA940ED-5B98-4F14-98B2-D8EB2892D273", "User Configured Country Sanctions");

		MultilingualString ProfileNotesWatermark => ResString.GetMultilingualString("6267DAC3-DDD3-453B-AE04-5CC112955014", "This country has been marked as sanctioned by your administrator.");

		public BusinessObjectFactory Factory { get; }

		public DpsResponseWithScreeningParty ResponseWithScreeningParty { get; }

		public List<PotentialMatchModel> PotentialMatchModels { get; private set; }

		List<PotentialMatchModel> IncludedPotentialMatchModels;

		List<RefCountry> SanctionedCountryCandidates => GetSanctionedCountryCandidates();

		public PartyTypes PartyType
		{
			get
			{
				var result = PartyTypes.Organization;
				if (ResponseWithScreeningParty.ScreeningParty.Vessel != null || ResponseWithScreeningParty.ScreeningParty.NotLinkedVessel != null)
				{
					result = PartyTypes.Vessel;
				}
				else if (ResponseWithScreeningParty.ScreeningParty.DocAddress != null)
				{
					result = PartyTypes.JobDocAddress;
				}
				else if (ResponseWithScreeningParty.ScreeningParty.Country != null)
				{
					result = PartyTypes.Country;
				}

				return result;
			}
		}

		public string PartyName
		{
			get
			{
				string result = null;
				if (ResponseWithScreeningParty.ScreeningParty.Vessel != null)
				{
					result = ResponseWithScreeningParty.ScreeningParty.Vessel.RV_Code;
				}
				else if (ResponseWithScreeningParty.ScreeningParty.NotLinkedVessel != null)
				{
					result = ResponseWithScreeningParty.ScreeningParty.NotLinkedVessel.Code;
				}
				else if (ResponseWithScreeningParty.ScreeningParty.DocAddress != null)
				{
					result = ResponseWithScreeningParty.ScreeningParty.DocAddress.E2_CompanyName;
				}
				else if (ResponseWithScreeningParty.ScreeningParty.Header != null)
				{
					result = ResponseWithScreeningParty.ScreeningParty.Header.OH_FullName;
				}
				else if (ResponseWithScreeningParty.ScreeningParty.Country != null)
				{
					result = ResponseWithScreeningParty.ScreeningParty.Country.RN_Desc;
				}
				else if (ResponseWithScreeningParty.ScreeningParty.NaturalPerson != null)
				{
					result = ResponseWithScreeningParty.ScreeningParty.NaturalPerson.Name;
				}

				return string.IsNullOrEmpty(result) ? Res.GetString("044F0D47-6636-449F-9F2A-E709ED342B45", "Unknown") : result;
			}
		}

		public string ParentsDescription
		{
			get
			{
				return ResponseWithScreeningParty.ScreeningParty.ParentsDescription;
			}
		}

		string highConfidenceResults;
		public string HighConfidenceResults => highConfidenceResults ?? (highConfidenceResults = GetMatchesInfo(ScoreGrades.High));

		string mediumConfidenceResults;
		public string MediumConfidenceResults => mediumConfidenceResults ?? (mediumConfidenceResults = GetMatchesInfo(ScoreGrades.Medium));

		public int LowConfidenceResultsCount => 0;

		string GetMatchesInfo(ScoreGrades scoreGrade)
		{
			var matchesInfo = new StringBuilder();
			var totalMatches = 0;

			foreach (var totalPotentialMatchViewModel in IncludedPotentialMatchModels)
			{
				if (totalPotentialMatchViewModel.ScoreGrade == scoreGrade)
				{
					totalMatches++;
					matchesInfo.AppendLine(DpsLog.LineBreak);

					matchesInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"Profile Name: {0}, Source List: {1}", totalPotentialMatchViewModel.ProfileName, string.Join(", ", totalPotentialMatchViewModel.ProfileHeaderInfo.SourceListCodes ?? Enumerable.Empty<string>())));
					matchesInfo.AppendLine();

					var nameMediumAndHighMatches = totalPotentialMatchViewModel.NameMatchModel.TotalScreenedDeniedItems.Where(match => match.ScoreGrade == ScoreGrades.High || match.ScoreGrade == ScoreGrades.Medium).OrderByDescending(match => match.Score).ToList();
					nameMediumAndHighMatches.ForEach(match => matchesInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"Name {0} Matched to {1}, Score: {2}", match.NameMatchInfo?.RequestName.FullName, match.ProfileNameInfo.FullName, match.DisplayScore.GetUnresolvedString())));

					var addressMediumAndHighMatches = totalPotentialMatchViewModel.AddressMatchModel.TotalScreenedDeniedItems.Where(match => match.ScoreGrade == ScoreGrades.High || match.ScoreGrade == ScoreGrades.Medium).OrderByDescending(match => match.Score).ToList();
					addressMediumAndHighMatches.ForEach(match => matchesInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"Address {0} Matched to {1}, Score: {2}", GetAddress(match.AddressMatchInfo?.RequestAddress), GetAddress(match.ProfileAddressInfo), match.DisplayScore.GetUnresolvedString())));

					var regCodeMediumAndHighMatches = totalPotentialMatchViewModel.RegistrationCodeMatchModel.TotalScreenedDeniedItems.Where(match => match.ScoreGrade == ScoreGrades.High || match.ScoreGrade == ScoreGrades.Medium).OrderByDescending(match => match.Score).ToList();
					regCodeMediumAndHighMatches.ForEach(match => matchesInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"Registration Code {0} Matched to {1}, Score: {2}", match.RegistrationCodeMatchInfo?.RequestRegistrationCode.RegCodeValue, match.ProfileRegistrationCodeInfo.IdNumber, match.DisplayScore.GetUnresolvedString())));
				}
			}

			matchesInfo.Insert(0, Res.GetString("FEB9A8A4-7C99-42E6-BF7A-7CC20D9F95DE", "{0} RECORDS", totalMatches) + System.Environment.NewLine);

			return matchesInfo.ToString();
		}

		string GetAddress(ProfileAddressInfo profileAddressInfo)
		{
			return string.Join(" ", new[] { profileAddressInfo.Street, profileAddressInfo.City, profileAddressInfo.StateProvince, profileAddressInfo.PostCode, profileAddressInfo.Country }.Where(u => !string.IsNullOrWhiteSpace(u)));
		}

		string GetAddress(DpsAddressCandidate addressCandidate)
		{
			return addressCandidate == null ? string.Empty : string.Join(" ", new[] { addressCandidate.Address1, addressCandidate.Address2, addressCandidate.AdditionalAddressLine, addressCandidate.City, addressCandidate.State, addressCandidate.PostCode, addressCandidate.Country }.Where(u => !string.IsNullOrWhiteSpace(u)));
		}

		void InitMatchModels()
		{
			var results = new List<PotentialMatchModel>();
			var profiles = ResponseWithScreeningParty.Response.Profiles;

			if (profiles?.Any() ?? false)
			{
				foreach (var profile in profiles)
				{
					if (profile.TypeOfEntity != DeniedPartyConstants.ScreeningNameTypes.Country || PartyType == PartyTypes.Country)
					{
						AddPotentialMatchModel(results, profile);
					}
					else if (profile.TypeOfEntity == DeniedPartyConstants.ScreeningNameTypes.Country
							 && PartyType != PartyTypes.Country
							 && profile.ProfileCountries != null
							 && SanctionedCountryCandidates.Any(u => profile.ProfileCountries.Any(v => v.Code == u.RN_Code)))
					{
						profile.SourceListCodes = profile.SourceListCodes.Concat(new string[] { SourceListWatermark }).ToArray();
						profile.ProfileNotes = Compressor.Zip(ProfileNotesWatermark.ToString());

						AddPotentialMatchModel(results, profile);
					}
				}
			}

			AddPotentialMatchModelWhenSanctionedCountriesHasNoProfileMatched(results);

			PotentialMatchModels = results.Where(x => x.IsValid).OrderByDescending(x => x.ScoreGrade).ThenBy(x => x.IsExcluded).ToList();
			IncludedPotentialMatchModels = PotentialMatchModels.Where(x => !x.IsExcluded).ToList();
		}

		void AddPotentialMatchModelWhenSanctionedCountriesHasNoProfileMatched(List<PotentialMatchModel> results)
		{
			var responseCountryMatches = ResponseWithScreeningParty.Response.CountryMatches?.ToList() ?? new List<CountryMatchInfo>();
			var requestCountryNoMatches = ResponseWithScreeningParty.RequestHeaderWithAddressMatching.DpsCountryCandidates?
				.Where(candidate => !responseCountryMatches.Select(country => country.MatchingCountryCode)
				.Contains(candidate.CountryCode)) ?? new List<DpsCountryCandidate>();
			var sanctionedCountryNoMatches = SanctionedCountryCandidates.Where(candidate => requestCountryNoMatches.Any(country => country.CountryCode == candidate.RN_Code));

			if (sanctionedCountryNoMatches?.Any() ?? false)
			{
				foreach (var country in sanctionedCountryNoMatches)
				{
					var newSourceProfileId = Guid.NewGuid();
					var countryMatchInfo = new CountryMatchInfo()
					{
						MatchingCountryCode = country.RN_Code,
						MatchingCountryScore = 100,
						SourceProfileID = newSourceProfileId,
						RequestCountry = new DpsCountryCandidate { CountryCode = country.RN_Code },
						MatchingCountryName = country.RN_Desc
					};
					responseCountryMatches.Add(countryMatchInfo);

					var profileCountryInfo = new ProfileCountryInfo()
					{
						Code = country.RN_Code,
						SourceProfileID = newSourceProfileId,
						CountryName = country.RN_Desc
					};

					var profileNameInfo = new ProfileNameInfo()
					{
						FullName = country.RN_Desc,
						IsPrimaryName = true,
						SourceProfileID = newSourceProfileId
					};

					var profileHeaderInfo = new ProfileHeaderInfo()
					{
						SourceListCodes = new string[] { SourceListWatermark },
						ProfileNotes = Compressor.Zip(ProfileNotesWatermark.ToString()),
						ProfileCountries = new[] { profileCountryInfo },
						SourceProfileID = newSourceProfileId,
						TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Country,
						ProfileNames = new[] { profileNameInfo }
					};

					AddPotentialMatchModel(results, profileHeaderInfo);
				}
			}
		}

		List<RefCountry> GetSanctionedCountryCandidates()
		{
			var refCountres = new List<RefCountry>();

			if (ResponseWithScreeningParty.RequestHeaderWithAddressMatching.DpsCountryCandidates?.Any() ?? false)
			{
				var query = new ZQuery(RefCountrySchema.RN_IsSanctioned, true);
				query.AddToFilter(RefCountrySchema.RN_Code, ResponseWithScreeningParty.RequestHeaderWithAddressMatching.DpsCountryCandidates.Select(u => u.CountryCode));
				refCountres = Factory.Load<RefCountry>(query).ToList();
			}

			return refCountres;
		}

		void AddPotentialMatchModel(List<PotentialMatchModel> results, ProfileHeaderInfo profile)
		{
			results.Add(new PotentialMatchModel(profile,
				ResponseWithScreeningParty.Response.AddressMatches?.Where(x => x.SourceProfileID == profile.SourceProfileID).ToList() ?? new List<AddressMatchInfo>(),
				ResponseWithScreeningParty.Response.NameMatches?.Where(x => x.SourceProfileID == profile.SourceProfileID).ToList() ?? new List<NameMatchInfo>(),
				ResponseWithScreeningParty.Response.RegistrationCodeMatches?.Where(x => x.SourceProfileID == profile.SourceProfileID).ToList() ?? new List<RegistrationCodeMatchInfo>(),
				ResponseWithScreeningParty.Response.CountryMatches?.Where(x => x.SourceProfileID == profile.SourceProfileID).ToList() ?? new List<CountryMatchInfo>(),
				Factory,
				profile.TypeOfEntity,
				forceAllLists));
		}
	}
}
