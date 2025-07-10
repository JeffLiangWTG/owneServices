using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class ScreenedPartyModelTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test";
			var party = new ScreeningParty(header, "ABC", header);
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), TypeOfEntity = "PER" },
			};
			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			var viewModel = new ScreenedPartyModel(responseWithParty, Factory);

			CombineAssertions(() =>
			{
				AssertEquals(PartyTypes.Organization, viewModel.PartyType);
				AssertEquals(header.OH_FullName, viewModel.PartyName);
				AssertEquals(0, viewModel.PotentialMatchModels.Count);
				AssertEquals(header.OH_Code + ": ABC", viewModel.ParentsDescription);

				AssertExceptionThrown<ArgumentException>(() => new ScreenedPartyModel(null, Factory));
				AssertExceptionThrown<ArgumentException>(() => new ScreenedPartyModel(responseWithParty, null));
			});

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			party = new ScreeningParty(vessel, string.Empty, vessel);
			responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			viewModel = new ScreenedPartyModel(responseWithParty, Factory);
			AssertEquals(vessel.RV_Code + ": ", viewModel.ParentsDescription);

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			party = new ScreeningParty(docAddress, "Dummy", docAddress);
			responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			viewModel = new ScreenedPartyModel(responseWithParty, Factory);
			AssertEquals("Dummy", viewModel.ParentsDescription);
		}

		public void TestScreenMatchedAndSanctionedCountry_ScreeningPartyIsCountry_UnmatchedProfileIncluded()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_IsSanctioned = true;
			Factory.Save();

			var party = new ScreeningParty(country, "Country", country);

			var dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(party,
				new DpsResponse
				{
					Profiles = new List<ProfileHeaderInfo>(),
					CountryMatches = new List<CountryMatchInfo>()
				},
				new DpsRequestHeaderWithAddressMatching() { DpsCountryCandidates = new[] { new DpsCountryCandidate() { CountryCode = country.RN_Code } } });
			var screenedPartyModel = new ScreenedPartyModel(dpsResponseWithScreeningParty, Factory);
			CombineAssertions(() =>
			{
				AssertEquals(1, screenedPartyModel.PotentialMatchModels.Count);
				AssertEquals(Compressor.Zip("This country has been marked as sanctioned by your administrator."), screenedPartyModel.PotentialMatchModels[0].ProfileHeaderInfo.ProfileNotes);
				AssertEquals(true, screenedPartyModel.PotentialMatchModels[0].ProfileHeaderInfo.SourceListCodes.ToList().Contains(ScreenedPartyModel.SourceListWatermark));
			});
		}

		public void TestScreenMatchedAndSanctionedCountry_ScreeningPartyIsCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_IsSanctioned = true;
			Factory.Save();

			var party = new ScreeningParty(country, "Country", country);

			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Array.Empty<byte>(),
					TypeOfEntity = "COY",
					ProfileCountries = new List<ProfileCountryInfo>() { new ProfileCountryInfo { Code = country.RN_Code } },
					SourceListCodes = new List<string> { "AAA" }
				},
			};
			var dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(party,
				new DpsResponse
				{
					Profiles = profiles,
					CountryMatches = new List<CountryMatchInfo>()
					{
						new CountryMatchInfo
						{
							MatchingCountryCode = country.RN_Code
						}
					}
				},
				new DpsRequestHeaderWithAddressMatching());
			var screenedPartyModel = new ScreenedPartyModel(dpsResponseWithScreeningParty, Factory);
			CombineAssertions(() =>
			{
				AssertEquals(1, screenedPartyModel.PotentialMatchModels.Count);
				AssertNotEquals(Compressor.Zip("This country has been marked as sanctioned by your administrator."), screenedPartyModel.PotentialMatchModels[0].ProfileHeaderInfo.ProfileNotes);
				AssertEquals(false, screenedPartyModel.PotentialMatchModels[0].ProfileHeaderInfo.SourceListCodes.ToList().Contains(ScreenedPartyModel.SourceListWatermark));
			});
		}

		public void TestScreenMatchedButNotSanctionedCountry_ScreeningPartyIsCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_IsSanctioned = false;
			Factory.Save();

			var party = new ScreeningParty(country, "Country", country);

			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Array.Empty<byte>(),
					TypeOfEntity = "COY",
					ProfileCountries = new List<ProfileCountryInfo>() { new ProfileCountryInfo { Code = country.RN_Code } },
					SourceListCodes = new List<string> { "AAA" }
				},
			};
			var dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(party,
				new DpsResponse
				{
					Profiles = profiles,
					CountryMatches = new List<CountryMatchInfo>()
					{
						new CountryMatchInfo
						{
							MatchingCountryCode = country.RN_Code
						}
					}
				},
				new DpsRequestHeaderWithAddressMatching() { DpsCountryCandidates = new[] { new DpsCountryCandidate() { CountryCode = country.RN_Code } } });
			var screenedPartyModel = new ScreenedPartyModel(dpsResponseWithScreeningParty, Factory);
			CombineAssertions(() =>
			{
				AssertEquals(1, screenedPartyModel.PotentialMatchModels.Count);
				AssertNotEquals(Compressor.Zip("This country has been marked as sanctioned by your administrator."), screenedPartyModel.PotentialMatchModels[0].ProfileHeaderInfo.ProfileNotes);
				AssertEquals(false, screenedPartyModel.PotentialMatchModels[0].ProfileHeaderInfo.SourceListCodes.ToList().Contains(ScreenedPartyModel.SourceListWatermark));
			});
		}

		public void TestScreenMatchedAndSanctionedCountry_ScreeningPartyIsNotCountry_UnmatchedProfileIncluded()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_IsSanctioned = true;
			Factory.Save();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var party = new ScreeningParty(vessel, string.Empty, vessel);
			vessel.RV_RN_NKCountryOfReg = country.RN_Code;

			var dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(party,
				new DpsResponse
				{
					Profiles = new List<ProfileHeaderInfo>(),
					CountryMatches = new List<CountryMatchInfo>()
				},
				new DpsRequestHeaderWithAddressMatching() { DpsCountryCandidates = new[] { new DpsCountryCandidate() { CountryCode = country.RN_Code } } });
			var screenedPartyModel = new ScreenedPartyModel(dpsResponseWithScreeningParty, Factory);
			CombineAssertions(() =>
			{
				AssertEquals(1, screenedPartyModel.PotentialMatchModels.Count);
				AssertEquals(Compressor.Zip("This country has been marked as sanctioned by your administrator."), screenedPartyModel.PotentialMatchModels[0].ProfileHeaderInfo.ProfileNotes);
				AssertEquals(true, screenedPartyModel.PotentialMatchModels[0].ProfileHeaderInfo.SourceListCodes.ToList().Contains(ScreenedPartyModel.SourceListWatermark));
			});
		}

		public void TestScreenMatchedAndSanctionedCountry_ScreeningPartyIsNotCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_IsSanctioned = true;
			Factory.Save();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var party = new ScreeningParty(vessel, string.Empty, vessel);
			vessel.RV_RN_NKCountryOfReg = country.RN_Code;

			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Array.Empty<byte>(),
					TypeOfEntity = "COY",
					ProfileCountries = new List<ProfileCountryInfo>() { new ProfileCountryInfo { Code = country.RN_Code } },
					SourceListCodes = new List<string> { "AAA" }
				},
			};
			var dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(party,
				new DpsResponse
				{
					Profiles = profiles,
					CountryMatches = new List<CountryMatchInfo>()
					{
						new CountryMatchInfo
						{
							MatchingCountryCode = country.RN_Code
						}
					}
				},
				new DpsRequestHeaderWithAddressMatching() { DpsCountryCandidates = new[] { new DpsCountryCandidate() { CountryCode = country.RN_Code } } });
			var screenedPartyModel = new ScreenedPartyModel(dpsResponseWithScreeningParty, Factory);
			CombineAssertions(() =>
			{
				AssertEquals(1, screenedPartyModel.PotentialMatchModels.Count);
				AssertEquals(Compressor.Zip("This country has been marked as sanctioned by your administrator."), screenedPartyModel.PotentialMatchModels[0].ProfileHeaderInfo.ProfileNotes);
				AssertEquals(true, screenedPartyModel.PotentialMatchModels[0].ProfileHeaderInfo.SourceListCodes.ToList().Contains(ScreenedPartyModel.SourceListWatermark));
			});
		}

		public void TestScreenMatchedAndSanctionedCountry_UnmatchedProfile_ScreeningPartyIsNotCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_IsSanctioned = true;
			Factory.Save();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var party = new ScreeningParty(vessel, string.Empty, vessel);
			vessel.RV_RN_NKCountryOfReg = country.RN_Code;

			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Array.Empty<byte>(),
					TypeOfEntity = "COY",
					ProfileCountries = new List<ProfileCountryInfo>() { new ProfileCountryInfo { Code = country.RN_Code + "A" } },
					SourceListCodes = new List<string> { "AAA" }
				},
			};
			var dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(party,
				new DpsResponse
				{
					Profiles = profiles,
					CountryMatches = new List<CountryMatchInfo>()
					{
						new CountryMatchInfo
						{
							MatchingCountryCode = country.RN_Code
						}
					}
				},
				new DpsRequestHeaderWithAddressMatching());
			var screenedPartyModel = new ScreenedPartyModel(dpsResponseWithScreeningParty, Factory);
			AssertEquals(0, screenedPartyModel.PotentialMatchModels.Count);
		}

		public void TestScreenMatchedButNotSanctionedCountry_ScreeningPartyIsNotCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_IsSanctioned = false;
			Factory.Save();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var party = new ScreeningParty(vessel, string.Empty, vessel);
			vessel.RV_RN_NKCountryOfReg = country.RN_Code;

			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Array.Empty<byte>(),
					TypeOfEntity = "COY",
					ProfileCountries = new List<ProfileCountryInfo>() { new ProfileCountryInfo { Code = country.RN_Code } },
					SourceListCodes = new List<string> { "AAA" }
				},
			};
			var dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(party,
				new DpsResponse
				{
					Profiles = profiles,
					CountryMatches = new List<CountryMatchInfo>()
					{
						new CountryMatchInfo
						{
							MatchingCountryCode = country.RN_Code
						}
					}
				},
				new DpsRequestHeaderWithAddressMatching());
			var screenedPartyModel = new ScreenedPartyModel(dpsResponseWithScreeningParty, Factory);
			AssertEquals(0, screenedPartyModel.PotentialMatchModels.Count);
		}

		public void TestPartyTypeCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "IR";

			var party = new ScreeningParty(country, "Country", country);
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Country },
			};

			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			var viewModel = new ScreenedPartyModel(responseWithParty, Factory);

			AssertEquals(PartyTypes.Country, viewModel.PartyType);
		}

		public void TestPartyNameCountryCode()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Desc = "Test Country Description";

			var party = new ScreeningParty(country, "Country", country);
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.Country },
			};

			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			var viewModel = new ScreenedPartyModel(responseWithParty, Factory);

			AssertEquals("Test Country Description", viewModel.PartyName);
		}

		public void TestPartyNameForNaturalPerson()
		{
			var parent = Factory.NewWithValidTestData<RefVessel>();
			Assert("Precondition", parent is IScreeningStatusProvider);

			var party = new ScreeningParty(parent, "Person", "Good King Moggle Mog XII", "Address 1", "Address 2", "City", "Postcode", "State", "Country", "Additional Address Line");
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), TypeOfEntity = DeniedPartyConstants.ScreeningNameTypes.NaturalPerson },
			};

			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			var viewModel = new ScreenedPartyModel(responseWithParty, Factory);

			AssertEquals("Good King Moggle Mog XII", viewModel.PartyName);
		}

		public void TestPotentialMatchModels()
		{
			var sourceListCode = "SourceList1";
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var headerPk = Guid.NewGuid();
			var headerNamePk = Guid.NewGuid();
			var responseWithParty = new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "For test1",
				Profiles = new List<ProfileHeaderInfo>()
				{
					new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } }, SourceListCodes = new List<string> { sourceListCode }, TypeOfEntity = "PER" },
				},
				NameMatches = new List<NameMatchInfo>()
				{
					new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = headerNamePk, MatchingNameScore = 100, SourceProfileID = headerPk },
				},
				AddressMatches = new List<AddressMatchInfo>(),
				RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
			}, new DpsRequestHeaderWithAddressMatching());
			var viewModel = new ScreenedPartyModel(responseWithParty, Factory);

			AssertEquals("Code not exist in compliance list, still show", 1, viewModel.PotentialMatchModels.Count);
		}

		public void TestPartyType()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Header";
			header.OH_Code = "HeaderCode";
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_CompanyName = "DocAddress";
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "Vessel";
			var notLinkedVessel = Factory.New<ITransport>();
			notLinkedVessel.ParentType = typeof(ITransportParentCommon);
			notLinkedVessel.JW_Vessel = "notLinkedVessel";

			var response = new DpsResponse();
			var screeningParty = new ScreeningParty(header, "AAA", header);
			var requestHeader = new DpsRequestHeaderWithAddressMatching();
			var dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(screeningParty, response, requestHeader);
			var model = new ScreenedPartyModel(dpsResponseWithScreeningParty, Factory);
			AssertScreenedPartyModel(response, screeningParty, header.OH_FullName, header.OH_Code + ": AAA", PartyTypes.Organization, model);

			screeningParty = new ScreeningParty(vessel, "BBB", vessel);
			dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(screeningParty, response, requestHeader);
			model = new ScreenedPartyModel(dpsResponseWithScreeningParty, Factory);
			AssertScreenedPartyModel(response, screeningParty, vessel.RV_Code, vessel.RV_Code + ": BBB", PartyTypes.Vessel, model);

			screeningParty = new ScreeningParty(vessel, "CCC", notLinkedVessel as IScreeningPartyForVessel);
			dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(screeningParty, response, requestHeader);
			model = new ScreenedPartyModel(dpsResponseWithScreeningParty, Factory);
			AssertScreenedPartyModel(response, screeningParty, notLinkedVessel.JW_Vessel, vessel.RV_Code + ": CCC", PartyTypes.Vessel, model);

			screeningParty = new ScreeningParty(docAddress, "DDD", docAddress);
			dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(screeningParty, response, requestHeader);
			model = new ScreenedPartyModel(dpsResponseWithScreeningParty, Factory);
			AssertScreenedPartyModel(response, screeningParty, docAddress.E2_CompanyName, "DocAddress: DDD", PartyTypes.JobDocAddress, model);

			docAddress.E2_CompanyName = string.Empty;
			AssertScreenedPartyModel(response, screeningParty, "Unknown", "DocAddress: DDD", PartyTypes.JobDocAddress, model);
		}

		void AssertScreenedPartyModel(DpsResponse expectedResponse, ScreeningParty expectedScreeningParty, string expectedName, string expectedDesc, PartyTypes expectedPartyType, ScreenedPartyModel model)
		{
			CombineAssertions(() =>
			{
				AssertEquals(expectedResponse, model.ResponseWithScreeningParty.Response);
				AssertEquals(expectedScreeningParty, model.ResponseWithScreeningParty.ScreeningParty);
				AssertEquals(expectedName, model.PartyName);
				AssertEquals(expectedDesc, model.ParentsDescription);
				AssertEquals(expectedPartyType, model.PartyType);
			});
		}

		public void TestScreenedPartyModel_ProperlyExcludeProfilesFromHighConfidenceResults()
		{
			(NameMatchInfo includedNameMatchInfo, ProfileHeaderInfo includedProfileHeaderInfo) = CreateNameMatchAndProfileHeaderInfo("Should be included", "III", 85);
			(NameMatchInfo excludedNameMatchInfo, ProfileHeaderInfo excludedProfileHeaderInfo) = CreateNameMatchAndProfileHeaderInfo("Should be excluded", "EEE", 90);

			var excludedList = Factory.NewWithValidTestData<RefComplianceList>();
			excludedList.RCL_ListCode = "EEE";
			excludedList.RCL_IsExcluded = true;

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(header, "", header);
			var responseWithParty = new DpsResponseWithScreeningParty(
												party,
												new DpsResponse
												{
													NameMatches = new List<NameMatchInfo>() { includedNameMatchInfo, excludedNameMatchInfo },
													Profiles = new List<ProfileHeaderInfo>() { includedProfileHeaderInfo, excludedProfileHeaderInfo },
												},
												new DpsRequestHeaderWithAddressMatching()
									);
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 80)))
			{
				var screenedPartyModel = new ScreenedPartyModel(responseWithParty, Factory);
				var highConfidenceResults = screenedPartyModel.HighConfidenceResults;

				AssertContains("1 RECORDS", highConfidenceResults);
				AssertContains("Profile Name: Should be included", highConfidenceResults);
				AssertNotContains("Profile Name: Should be excluded", highConfidenceResults);
			}
		}

		public void TestScreenedPartyModel_ProperlyExcludeProfilesFromMediumConfidenceResults()
		{
			(NameMatchInfo includedNameMatchInfo, ProfileHeaderInfo includedProfileHeaderInfo) = CreateNameMatchAndProfileHeaderInfo("Should be included", "III", 65);
			(NameMatchInfo excludedNameMatchInfo, ProfileHeaderInfo excludedProfileHeaderInfo) = CreateNameMatchAndProfileHeaderInfo("Should be excluded", "EEE", 70);

			var excludedList = Factory.NewWithValidTestData<RefComplianceList>();
			excludedList.RCL_ListCode = "EEE";
			excludedList.RCL_IsExcluded = true;

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(header, "", header);
			var responseWithParty = new DpsResponseWithScreeningParty(
												party,
												new DpsResponse
												{
													NameMatches = new List<NameMatchInfo>() { includedNameMatchInfo, excludedNameMatchInfo },
													Profiles = new List<ProfileHeaderInfo>() { includedProfileHeaderInfo, excludedProfileHeaderInfo },
												},
												new DpsRequestHeaderWithAddressMatching()
									);
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 80)))
			{
				var screenedPartyModel = new ScreenedPartyModel(responseWithParty, Factory);
				var mediumConfidenceResults = screenedPartyModel.MediumConfidenceResults;

				AssertContains("1 RECORDS", mediumConfidenceResults);
				AssertContains("Profile Name: Should be included", mediumConfidenceResults);
				AssertNotContains("Profile Name: Should be excluded", mediumConfidenceResults);
			}
		}

		public (NameMatchInfo, ProfileHeaderInfo) CreateNameMatchAndProfileHeaderInfo(string fullName, string sourceListCode, int matchingNameScore)
		{
			var sourceProfileId = Guid.NewGuid();
			var nameId = Guid.NewGuid();

			var excludedProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo() { FullName = fullName, ID = nameId } };
			var profileHeaderInfo = new ProfileHeaderInfo
			{
				ProfileNames = excludedProfileNames,
				SourceProfileID = sourceProfileId,
				ProfileNotes = Array.Empty<byte>(),
				TypeOfEntity = "ORG",
				SourceListCodes = new List<string>() { sourceListCode }
			};
			var nameMatchInfo = new NameMatchInfo
			{
				MatchingNameScore = matchingNameScore,
				SourceProfileID = sourceProfileId,
				MatchingNameID = nameId,
				RequestName = new DpsNameCandidate() { NameType = "ORG" }
			};

			return (nameMatchInfo, profileHeaderInfo);
		}
	}
}
