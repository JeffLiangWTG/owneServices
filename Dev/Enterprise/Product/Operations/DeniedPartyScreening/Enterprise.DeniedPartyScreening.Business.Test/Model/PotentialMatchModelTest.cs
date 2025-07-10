using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class PotentialMatchModelTest : TestCaseWithFactory
	{
		public void TestConstructorArgumentNull()
		{
			var profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>(), ProfileAddresses = new List<ProfileAddressInfo>(), ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(), ProfileCountries = new List<ProfileCountryInfo>(), SourceListCodes = new List<string>(), TypeOfEntity = "PER" };
			var addressMatchInfos = new List<AddressMatchInfo>();
			var nameMatchInfos = new List<NameMatchInfo>();
			var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
			var countryMatchInfos = new List<CountryMatchInfo>();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => _ = new PotentialMatchModel(null, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER"));
				AssertExceptionThrown<ArgumentNullException>(() => _ = new PotentialMatchModel(profileHeaderInfo, null, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER"));
				AssertExceptionThrown<ArgumentNullException>(() => _ = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, null, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER"));
				AssertExceptionThrown<ArgumentNullException>(() => _ = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, null, countryMatchInfos, Factory, "PER"));
				AssertExceptionThrown<ArgumentNullException>(() => _ = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, null, Factory, "PER"));
				AssertExceptionThrown<ArgumentNullException>(() => _ = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, null, "PER"));
				AssertExceptionThrown<ArgumentNullException>(() => _ = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, null));
			});
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
					new ProfileNameInfo { ID = Guid.NewGuid(), FullName = "Second Name", Language = string.Empty, IsPrimaryName = false, SourceProfileID = Guid.NewGuid() },
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
			var model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER");

			AssertEquals("Primary Name", model.ProfileName);

			profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>()
				{
					new ProfileNameInfo { ID = Guid.NewGuid(), FullName = "Second Name", Language = string.Empty, IsPrimaryName = false, SourceProfileID = Guid.NewGuid() },
				},
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>(),
				TypeOfEntity = "PER"
			};

			model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER");
			AssertEquals("Second Name", model.ProfileName);

			profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>()
				{
					new ProfileNameInfo { ID = Guid.NewGuid(), FullName = "", Language = string.Empty, IsPrimaryName = false, SourceProfileID = Guid.NewGuid() },
				},
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>(),
				TypeOfEntity = "PER"
			};

			model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER");
			AssertEquals("Unknown", model.ProfileName);

			profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>(), ProfileAddresses = new List<ProfileAddressInfo>(), ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(), ProfileCountries = new List<ProfileCountryInfo>(), SourceListCodes = new List<string>(), TypeOfEntity = "PER" };

			model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER");
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
			var model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER");
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

			model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER");
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

			model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER");
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

			model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, "PER");
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
			var profileHeaderInfo = new ProfileHeaderInfo
			{
				SourceProfileID = Guid.NewGuid(),
				ProfileNotes = Array.Empty<byte>(),
				ProfileNames = new List<ProfileNameInfo>(),
				ProfileAddresses = new List<ProfileAddressInfo>(),
				ProfileRegistrationCodes = profileRegistrationCodeInfos,
				ProfileCountries = new List<ProfileCountryInfo>(),
				SourceListCodes = new List<string>(),
				TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization
			};
			var addressMatchInfos = new List<AddressMatchInfo>();
			var nameMatchInfos = new List<NameMatchInfo>();
			var countryMatchInfos = new List<CountryMatchInfo>();
			var model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, DeniedPartyConstants.ScreeningNameTypes.Organization);

			AssertScoreGradeInformation(ScoreGrades.High, model);
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
				var profileHeaderInfo = new ProfileHeaderInfo
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Array.Empty<byte>(),
					ProfileNames = profileNameInfos,
					ProfileAddresses = new List<ProfileAddressInfo>(),
					ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
					ProfileCountries = new List<ProfileCountryInfo>(),
					SourceListCodes = new List<string>(),
					TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization
				};
				var addressMatchInfos = new List<AddressMatchInfo>();
				var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
				var countryMatchInfos = new List<CountryMatchInfo>();
				var model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, DeniedPartyConstants.ScreeningNameTypes.Organization);

				AssertScoreGradeInformation(ScoreGrades.High, model);
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
				var profileHeaderInfo = new ProfileHeaderInfo
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Array.Empty<byte>(),
					ProfileNames = profileNameInfos,
					ProfileAddresses = new List<ProfileAddressInfo>(),
					ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
					ProfileCountries = new List<ProfileCountryInfo>(),
					SourceListCodes = new List<string>(),
					TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization
				};
				var addressMatchInfos = new List<AddressMatchInfo>();
				var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
				var countryMatchInfos = new List<CountryMatchInfo>();
				var model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, DeniedPartyConstants.ScreeningNameTypes.Organization);

				AssertScoreGradeInformation(ScoreGrades.Medium, model);
			}
		}

		public void TestScoreGradeIsHigh_WhenAddressIsHigh()
		{
			var profileId1 = Guid.NewGuid();
			var addressId1 = Guid.NewGuid();
			var profileAddressInfos = new List<ProfileAddressInfo>
			{
				new ProfileAddressInfo { ID = addressId1, Street = "A", City = "B", StateProvince = "C", Country = "D", PostCode = "E", Language = "F", SourceProfileID = profileId1 },
			};
			var addressMatchInfos = new List<AddressMatchInfo>
			{
				new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "A", Address2 = "B", City = "C", State = "D", PostCode = "E", Country = "F", AdditionalAddressLine = "G" }, MatchingAddressID = addressId1, MatchingAddressScore = 99, SourceProfileID = profileId1 },
			};

			using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 90)))
			{
				var profileHeaderInfo = new ProfileHeaderInfo
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Array.Empty<byte>(),
					ProfileNames = new List<ProfileNameInfo>(),
					ProfileAddresses = profileAddressInfos,
					ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
					ProfileCountries = new List<ProfileCountryInfo>(),
					SourceListCodes = new List<string>(),
					TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization
				};
				var nameMatchInfos = new List<NameMatchInfo>();
				var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
				var countryMatchInfos = new List<CountryMatchInfo>();
				var model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, DeniedPartyConstants.ScreeningNameTypes.Organization);

				AssertScoreGradeInformation(ScoreGrades.High, model);
			}
		}

		public void TestSetScoreGrade_WhenAddressIsMedium_ShouldScoreGradeMedium()
		{
			var profileId1 = Guid.NewGuid();
			var addressId1 = Guid.NewGuid();

			var profileAddressInfos = new List<ProfileAddressInfo>
			{
				new ProfileAddressInfo { ID = addressId1, Street = "A", City = "B", StateProvince = "C", Country = "D", PostCode = "E", Language = "F", SourceProfileID = profileId1 },
			};
			var addressMatchInfos = new List<AddressMatchInfo>
			{
				new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "A", Address2 = "B", City = "C", State = "D", PostCode = "E", Country = "F", AdditionalAddressLine = "G" }, MatchingAddressID = addressId1, MatchingAddressScore = 76, SourceProfileID = profileId1 },
			};

			using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 90)))
			{
				var profileHeaderInfo = new ProfileHeaderInfo
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Array.Empty<byte>(),
					ProfileNames = new List<ProfileNameInfo>(),
					ProfileAddresses = profileAddressInfos,
					ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
					ProfileCountries = new List<ProfileCountryInfo>(),
					SourceListCodes = new List<string>(),
					TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Country
				};
				var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
				var countryMatchInfos = new List<CountryMatchInfo>();
				var nameMatchInfos = new List<NameMatchInfo>();
				var model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, DeniedPartyConstants.ScreeningNameTypes.Organization);

				AssertScoreGradeInformation(ScoreGrades.Medium, model);
			}
		}

		public void TestScoreGradeIsHigh_WhenTypeOfEntityIsCountry()
		{
			var profileID = Guid.NewGuid();
			var countryID = Guid.NewGuid();
			var profileCountryInfos = new List<ProfileCountryInfo>
			{
				new ProfileCountryInfo { ID = countryID, Code = "IR", CountryName = "Islamic Republic of Iran", SourceProfileID = profileID },
			};
			var countryMatchInfos = new List<CountryMatchInfo>
			{
				new CountryMatchInfo { RequestCountry = new DpsCountryCandidate { CountryCode = "IR" }, MatchingCountryId = countryID, MatchingCountryScore = 100, SourceProfileID = profileID },
			};

			using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 90)))
			{
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
				var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
				var addressMatchInfos = new List<AddressMatchInfo>();
				var nameMatchInfos = new List<NameMatchInfo>();
				var model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, DeniedPartyConstants.ScreeningNameTypes.Country);

				AssertScoreGradeInformation(ScoreGrades.High, model);
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
				var profileHeaderInfo = new ProfileHeaderInfo
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Array.Empty<byte>(),
					ProfileNames = profileNameInfos,
					ProfileAddresses = new List<ProfileAddressInfo>(),
					ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>(),
					ProfileCountries = new List<ProfileCountryInfo>(),
					SourceListCodes = new List<string>(),
					TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Organization
				};
				var addressMatchInfos = new List<AddressMatchInfo>();
				var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
				var countryMatchInfos = new List<CountryMatchInfo>();
				var model = new PotentialMatchModel(profileHeaderInfo, addressMatchInfos, nameMatchInfos, registrationCodeMatchInfos, countryMatchInfos, Factory, DeniedPartyConstants.ScreeningNameTypes.Organization);

				AssertScoreGradeInformation(ScoreGrades.Low, model);
			}
		}

		static void AssertScoreGradeInformation(ScoreGrades expectedScoreGrade, PotentialMatchModel model)
		{
			AssertEquals(expectedScoreGrade, model.ScoreGrade);
		}
	}
}
