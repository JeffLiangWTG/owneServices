using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class PotentialMatchModel
	{
		public PotentialMatchModel(ProfileHeaderInfo profileHeaderInfo, List<AddressMatchInfo> addressMatchInfos, List<NameMatchInfo> nameMatchInfos, List<RegistrationCodeMatchInfo> registrationCodeMatchInfos, List<CountryMatchInfo> countryMatchInfos, BusinessObjectFactory factory, string typeOfEntity, bool forceAllLists = false)
		{
			Argument.NotNull(profileHeaderInfo, nameof(profileHeaderInfo));
			Argument.NotNull(addressMatchInfos, nameof(addressMatchInfos));
			Argument.NotNull(nameMatchInfos, nameof(nameMatchInfos));
			Argument.NotNull(registrationCodeMatchInfos, nameof(registrationCodeMatchInfos));
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(typeOfEntity, nameof(typeOfEntity));
			Argument.NotNull(countryMatchInfos, nameof(countryMatchInfos));

			ProfileHeaderInfo = profileHeaderInfo;
			AddressMatchInfos = addressMatchInfos;
			NameMatchInfos = nameMatchInfos;
			RegistrationCodeMatchInfos = registrationCodeMatchInfos;
			Factory = factory;
			TypeOfEntity = typeOfEntity;

			ProfileNotesModel = new ProfileNotesModel(Compressor.Unzip(profileHeaderInfo.ProfileNotes));
			SourceListNamesModel = new SourceListNamesModel(factory, profileHeaderInfo.SourceListCodes?.ToArray() ?? Array.Empty<string>(), forceAllLists);
			NameMatchModel = new NameMatchModel(profileHeaderInfo.ProfileNames?.ToList() ?? new List<ProfileNameInfo>(), nameMatchInfos);
			AddressMatchModel = new AddressMatchModel(profileHeaderInfo.ProfileAddresses?.ToList() ?? new List<ProfileAddressInfo>(), addressMatchInfos);
			RegistrationCodeMatchModel = new RegistrationCodeMatchModel(profileHeaderInfo.ProfileRegistrationCodes?.ToList() ?? new List<ProfileRegistrationCodeInfo>(), registrationCodeMatchInfos);
			CountryMatchModel = new CountryMatchModel(profileHeaderInfo.ProfileCountries?.ToList() ?? new List<ProfileCountryInfo>(), countryMatchInfos);

			SetProfileName();
			SetScoreGrade();
		}

		public List<CountryMatchInfo> CountryMatchInfos { get; }

		public CountryMatchModel CountryMatchModel { get; }

		public ProfileHeaderInfo ProfileHeaderInfo { get; }

		public List<AddressMatchInfo> AddressMatchInfos { get; }

		public List<NameMatchInfo> NameMatchInfos { get; }

		public List<RegistrationCodeMatchInfo> RegistrationCodeMatchInfos { get; }

		public BusinessObjectFactory Factory { get; }

		public string TypeOfEntity { get; }

		public NameMatchModel NameMatchModel { get; }

		public ProfileNotesModel ProfileNotesModel { get; }

		public AddressMatchModel AddressMatchModel { get; }

		public RegistrationCodeMatchModel RegistrationCodeMatchModel { get; }

		public SourceListNamesModel SourceListNamesModel { get; }

		public bool IsValid => SourceListNamesModel.IncludedLists.Length + SourceListNamesModel.ExcludedLists.Length > 0;

		public string ProfileName { get; private set; }

		public bool IsExcluded => SourceListNamesModel.IncludedLists.Length == 0;

		public ScoreGrades ScoreGrade { get; private set; }

		void SetProfileName()
		{
			var primaryName = ProfileHeaderInfo.ProfileNames?.FirstOrDefault(x => x.IsPrimaryName)?.FullName;
			if (string.IsNullOrWhiteSpace(primaryName))
			{
				var secondName = ProfileHeaderInfo.ProfileNames?.FirstOrDefault()?.FullName;
				ProfileName = string.IsNullOrWhiteSpace(secondName) ? Res.GetString("044F0D47-6636-449F-9F2A-E709ED342B45", "Unknown") : secondName;
			}
			else
			{
				ProfileName = primaryName;
			}
		}

		void SetScoreGrade()
		{
			if (NameMatchModel.TotalScreenedDeniedItems.Any(x => x.ScoreGrade == ScoreGrades.High) ||
				RegistrationCodeMatchModel.TotalScreenedDeniedItems.Any(x => x.ScoreGrade == ScoreGrades.High) ||
				AddressMatchModel.TotalScreenedDeniedItems.Any(x => x.ScoreGrade == ScoreGrades.High) ||
				TypeOfEntity == DeniedPartyConstants.ScreeningNameTypes.Country)
			{
				ScoreGrade = ScoreGrades.High;
			}
			else if (NameMatchModel.TotalScreenedDeniedItems.Any(x => x.ScoreGrade == ScoreGrades.Medium) || AddressMatchModel.TotalScreenedDeniedItems.Any(x => x.ScoreGrade == ScoreGrades.Medium))
			{
				ScoreGrade = ScoreGrades.Medium;
			}
			else
			{
				ScoreGrade = ScoreGrades.Low;
			}
		}
	}
}
