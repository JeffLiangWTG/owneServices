using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class PotentialMatchWinModelTest : TestCaseWithFactory
	{
		public void TestConstructorArgumentNull()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => new PotentialMatchWinModel(null));
			});
		}

		public void TestConstructor()
		{
			var factory = new BusinessObjectFactory();
			var profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>(), ProfileAddresses = new List<ProfileAddressInfo>(), ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(), ProfileCountries = new List<ProfileCountryInfo>(), SourceListCodes = new List<string>(), TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization };
			var addressMatchInfos = new List<AddressMatchInfo>();
			var nameMatchInfos = new List<NameMatchInfo>();
			var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
			var countryMatchInfos = new List<CountryMatchInfo>();
			var model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, factory, DeniedPartyConstants.ScreeningNameTypes.Organization));

			CombineAssertions(() =>
			{
				AssertEquals("Unknown", model.ProfileName);
				AssertEquals(DpsImageSources.Organization, model.EntityTypeIcon);
				AssertNotNull(model.AddressMatchWinModel);
				AssertNotNull(model.RegistrationCodeWinModel);
				AssertNotNull(model.NameMatchWinModel);
				AssertNotNull(model.ProfileNotesWinModel);
				AssertNotNull(model.SourceListNamesWinModel);
			});

			model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, factory, DeniedPartyConstants.ScreeningNameTypes.Vessel));
			AssertEquals(DpsImageSources.Vessel, model.EntityTypeIcon);

			model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, factory, DeniedPartyConstants.ScreeningNameTypes.Person));
			AssertEquals(DpsImageSources.Person, model.EntityTypeIcon);

			model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, factory, DeniedPartyConstants.ScreeningNameTypes.Country));
			AssertEquals(DpsImageSources.Country, model.EntityTypeIcon);
		}

		public void TestProfileName()
		{
			var profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>()
				{
					new ProfileNameInfo { ID = Guid.NewGuid(), FullName = "Primary Name", Language = string.Empty, IsPrimaryName = true, SourceProfileID = Guid.NewGuid() },
					new ProfileNameInfo { ID = Guid.NewGuid(), FullName = "Second Name", Language = string.Empty, IsPrimaryName =  false, SourceProfileID = Guid.NewGuid() },
				},
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>(),
				TypeOfEntity = "PER"
			};
			var addressMatchInfos = new List<AddressMatchInfo>();
			var nameMatchInfos = new List<NameMatchInfo>();
			var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
			var countryMatchInfos = new List<CountryMatchInfo>();
			var model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER"));

			AssertEquals("Primary Name", model.ProfileName);

			profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>()
				{
					new ProfileNameInfo { ID = Guid.NewGuid(), FullName = "Second Name", Language = string.Empty, IsPrimaryName =  false, SourceProfileID = Guid.NewGuid() },
				},
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>(),
				TypeOfEntity = "PER"
			};

			model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER"));
			AssertEquals("Second Name", model.ProfileName);

			profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>()
				{
					new ProfileNameInfo { ID = Guid.NewGuid(), FullName = string.Empty, Language = string.Empty, IsPrimaryName = false, SourceProfileID = Guid.NewGuid() },
				},
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>(),
				TypeOfEntity = "PER"
			};

			model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER"));
			AssertEquals("Unknown", model.ProfileName);

			profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>(), ProfileAddresses = new List<ProfileAddressInfo>(), ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(), ProfileCountries = new List<ProfileCountryInfo>(), SourceListCodes = new List<string>(), TypeOfEntity = "PER" };

			model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER"));
			AssertEquals("Unknown", model.ProfileName);
		}

		public void TestIsExcluded()
		{
			var complianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList1.RCL_ListCode = "SourceList1";
			complianceList1.RCL_IsExcluded = true;
			var complianceList2 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList2.RCL_ListCode = "SourceList2";
			complianceList2.RCL_IsExcluded = false;
			Factory.Save();

			var profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>(),
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>()
				{
					"SourceList1",
					"SourceList2",
				},
				TypeOfEntity = "PER"
			};
			var addressMatchInfos = new List<AddressMatchInfo>();
			var nameMatchInfos = new List<NameMatchInfo>();
			var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
			var countryMatchInfos = new List<CountryMatchInfo>();
			var model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER"));

			AssertEquals(false, model.IsExcluded);

			profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>(),
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>()
				{
					"SourceList1",
				},
				TypeOfEntity = "PER"
			};

			model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER"));
			AssertEquals(true, model.IsExcluded);
			AssertEquals(true, model.IsValid);

			profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>(),
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>()
				{
					"UnKnownList",
				},
				TypeOfEntity = "PER"
			};

			model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER"));
			AssertEquals(false, model.IsExcluded);
			AssertEquals(true, model.IsValid);

			profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>(),
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>(),
				TypeOfEntity = "PER"
			};

			model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER"));
			AssertEquals(true, model.IsExcluded);
			AssertEquals(false, model.IsValid);
		}

		public void TestScoreGradeIsHigh_WhenRegCodeIsHigh()
		{
			var profileId1 = Guid.NewGuid();
			var regCodeId1 = Guid.NewGuid();
			var profileRegistrationCodeInfos = new List<ProfileRegistrationCodeInfo>()
			{
				new ProfileRegistrationCodeInfo { ID = regCodeId1, IdType = "A", IdNumber = "123", IdCountry = "CN", SourceProfileID = profileId1 },
			};
			var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>()
			{
				new RegistrationCodeMatchInfo { RequestRegistrationCode = new DpsRegistrationCodeCandidate { RegCountryCode = "CN", RegCodeType = "A", RegCodeValue = "123" }, MatchingRegistrationCodeID = regCodeId1, MatchingRegistrationCodeScore = 100, SourceProfileID = profileId1 },
			};
			var factory = new BusinessObjectFactory();
			var profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>(), ProfileAddresses = new List<ProfileAddressInfo>(), ProfileRegistrationCodes = profileRegistrationCodeInfos, ProfileCountries = new List<ProfileCountryInfo>(), SourceListCodes = new List<string>(), TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization };
			var addressMatchInfos = new List<AddressMatchInfo>();
			var nameMatchInfos = new List<NameMatchInfo>();
			var countryMatchInfos = new List<CountryMatchInfo>();
			var model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, factory, DeniedPartyConstants.ScreeningNameTypes.Organization));

			AssertScoreGradeInformation(ScoreGrades.High, DpsImageSources.WarningRed, "High", "High Risk - Review Required", model);
		}

		public void TestScoreGradeIsHigh_WhenNameIsHigh()
		{
			var profileId1 = Guid.NewGuid();
			var nameId1 = Guid.NewGuid();
			var profileNameInfos = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId1, FullName = "Test Full Name 1", Language = "Chinese", IsPrimaryName = true, SourceProfileID = profileId1 },
			};
			var nameMatchInfos = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "ORG", FullName = "Test Full Name 3" }, MatchingNameID = nameId1, MatchingNameScore = 99, SourceProfileID = profileId1 },
			};

			using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 90)))
			{
				var factory = new BusinessObjectFactory();
				var profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfos, ProfileAddresses = new List<ProfileAddressInfo>(), ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(), ProfileCountries = new List<ProfileCountryInfo>(), SourceListCodes = new List<string>(), TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization };
				var addressMatchInfos = new List<AddressMatchInfo>();
				var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
				var countryMatchInfos = new List<CountryMatchInfo>();
				var model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, factory, DeniedPartyConstants.ScreeningNameTypes.Organization));

				AssertScoreGradeInformation(ScoreGrades.High, DpsImageSources.WarningRed, "High", "High Risk - Review Required", model);
			}
		}

		public void TestScoreGradeIsMedium_WhenNameIsMedium()
		{
			var profileId1 = Guid.NewGuid();
			var nameId1 = Guid.NewGuid();
			var profileNameInfos = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId1, FullName = "Test Full Name 1", Language = "Chinese", IsPrimaryName = true, SourceProfileID = profileId1 },
			};
			var nameMatchInfos = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "ORG", FullName = "Test Full Name 3" }, MatchingNameID = nameId1, MatchingNameScore = 66, SourceProfileID = profileId1 },
			};

			using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 90)))
			{
				var factory = new BusinessObjectFactory();
				var profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfos, ProfileAddresses = new List<ProfileAddressInfo>(), ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(), ProfileCountries = new List<ProfileCountryInfo>(), SourceListCodes = new List<string>(), TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization };
				var addressMatchInfos = new List<AddressMatchInfo>();
				var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
				var countryMatchInfos = new List<CountryMatchInfo>();
				var model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, factory, DeniedPartyConstants.ScreeningNameTypes.Organization));

				AssertScoreGradeInformation(ScoreGrades.Medium, DpsImageSources.WarningOrange, "Medium", "Medium Risk - Review Required", model);
			}
		}

		public void TestScoreGradeIsLow()
		{
			var profileId1 = Guid.NewGuid();
			var nameId1 = Guid.NewGuid();
			var profileNameInfos = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId1, FullName = "Test Full Name 1", Language = "Chinese", IsPrimaryName = true, SourceProfileID = profileId1 },
			};
			var nameMatchInfos = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "ORG", FullName = "Test Full Name 3" }, MatchingNameID = nameId1, MatchingNameScore = 10, SourceProfileID = profileId1 },
			};

			using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 90)))
			{
				var factory = new BusinessObjectFactory();
				var profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfos, ProfileAddresses = new List<ProfileAddressInfo>(), ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(), ProfileCountries = new List<ProfileCountryInfo>(), SourceListCodes = new List<string>(), TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization };
				var addressMatchInfos = new List<AddressMatchInfo>();
				var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
				var countryMatchInfos = new List<CountryMatchInfo>();
				var model = new PotentialMatchWinModel(new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, factory, DeniedPartyConstants.ScreeningNameTypes.Organization));

				AssertScoreGradeInformation(ScoreGrades.Low, DpsImageSources.None, string.Empty, string.Empty, model);
			}
		}

		public void TestVisibilityToVisibleForOrganizationAndVessel()
		{
			var winModel = GetNewPotentialMatchWinModel(DeniedPartyConstants.ScreeningNameTypes.Organization);
			AssertEquals(true, winModel.NameMatchWinModel.MatchViewVisibility);
			AssertEquals(true, winModel.AddressMatchWinModel.MatchViewVisibility);
			AssertEquals(true, winModel.RegistrationCodeWinModel.MatchViewVisibility);

			winModel = GetNewPotentialMatchWinModel(DeniedPartyConstants.ScreeningNameTypes.Vessel);
			AssertEquals(true, winModel.NameMatchWinModel.MatchViewVisibility);
			AssertEquals(true, winModel.AddressMatchWinModel.MatchViewVisibility);
			AssertEquals(true, winModel.RegistrationCodeWinModel.MatchViewVisibility);
		}

		public void TestVisibilityToCollapsedForCountry()
		{
			var profileId = Guid.NewGuid();
			var countryId = Guid.NewGuid();
			var profileCountryInfos = new List<ProfileCountryInfo>()
			{
				new ProfileCountryInfo { ID = countryId, Code = "IR", CountryName = "Islamic Republic of Iran", SourceProfileID = profileId },
			};
			var countryMatchInfos = new List<CountryMatchInfo>()
			{
				new CountryMatchInfo { RequestCountry = new DpsCountryCandidate { CountryCode = "IR" }, MatchingCountryId = countryId, MatchingCountryScore = 100, SourceProfileID = profileId },
			};
			var profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>(),
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = profileCountryInfos,
				SourceListCodes = new List<string>(),
				TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Country
			};
			var potentialMatchModel = new PotentialMatchModel(profileHeaderInfo, new List<AddressMatchInfo>(), new List<NameMatchInfo>(), new List<RegistrationCodeMatchInfo>(), countryMatchInfos, new BusinessObjectFactory(), DeniedPartyConstants.ScreeningNameTypes.Country);
			var winModel = new PotentialMatchWinModel(potentialMatchModel);

			AssertEquals(false, winModel.NameMatchWinModel.MatchViewVisibility);
			AssertEquals(false, winModel.AddressMatchWinModel.MatchViewVisibility);
			AssertEquals(false, winModel.RegistrationCodeWinModel.MatchViewVisibility);
		}

		public static PotentialMatchWinModel GetNewPotentialMatchWinModel(string screeningEntityType)
		{
			var profileId1 = Guid.NewGuid();
			var nameId1 = Guid.NewGuid();
			var profileNameInfos = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId1, FullName = "Test Full Name 1", Language = "Chinese", IsPrimaryName = true, SourceProfileID = profileId1 },
			};
			var nameMatchInfos = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = screeningEntityType, FullName = "Test Full Name 3" }, MatchingNameID = nameId1, MatchingNameScore = 10, SourceProfileID = profileId1 },
			};
			var profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), ProfileNames = profileNameInfos, ProfileAddresses = new List<ProfileAddressInfo>(), ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(), ProfileCountries = new List<ProfileCountryInfo>(), SourceListCodes = new List<string>(), TypeOfEntity = screeningEntityType };
			var potentialMatchModel = new PotentialMatchModel(profileHeaderInfo, new List<AddressMatchInfo>(), nameMatchInfos, new List<RegistrationCodeMatchInfo>(), new List<CountryMatchInfo>(), new BusinessObjectFactory(), screeningEntityType);

			return new PotentialMatchWinModel(potentialMatchModel);
		}

		static void AssertScoreGradeInformation(ScoreGrades expectedScoreGrade, DpsImageSources expectedImage, string expectedScoreGradeText, string expectedScoreGradeReviewText, PotentialMatchWinModel model)
		{
			CombineAssertions(() =>
			{
				AssertEquals(expectedScoreGrade, model.ScoreGrade);
				AssertEquals(expectedImage, model.WarningIcon);
				AssertEquals(expectedScoreGradeText, model.ScoreGradeText);
				AssertEquals(expectedScoreGradeReviewText, model.ScoreGradeReviewText);
			});
		}
	}
}
