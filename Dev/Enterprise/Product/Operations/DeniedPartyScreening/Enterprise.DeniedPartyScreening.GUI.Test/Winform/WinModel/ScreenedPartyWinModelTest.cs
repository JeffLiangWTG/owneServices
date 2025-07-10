using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class ScreenedPartyWinModelTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var party = new ScreeningParty(header, "EFG", header);
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Compressor.Zip("Test"), TypeOfEntity = "PER" },
			};

			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			var screenedPartyModel = new ScreenedPartyModel(responseWithParty, Factory);
			var winModel = new ScreenedPartyWinModel(screenedPartyModel, a => { }, null, true);

			CombineAssertions(() =>
			{
				AssertEquals(PartyTypes.Organization, winModel.PartyType);
				AssertEquals(DpsImageSources.Organization, winModel.EntityTypeIcon);
				AssertEquals(header.OH_FullName, winModel.PartyName);
				AssertEquals("Potential Matches", winModel.PotentialMatchesText);
				AssertEquals(0, winModel.TotalPotentialMatchWinModelsCount);
				AssertEquals("Show Excluded", winModel.ShowExcludedText);
				AssertEquals(0, winModel.ShowExcludedCount);
				AssertEquals("Profile Name", winModel.ProfileNameText);
				AssertNotNull(winModel.TotalPotentialMatchWinModels);
				AssertEquals(header.OH_Code + ": EFG", winModel.ParentsDescription);
				AssertEquals(true, winModel.StandAlone);
				AssertEquals("Save", winModel.SaveCaption);
				AssertEquals(false, winModel.HasPotentialMatches);
				AssertEquals(ScreeningStatusesList.Codes.Matched, winModel.CurrentScreeningStatus);

				AssertExceptionThrown<ArgumentException>(() => new ScreenedPartyWinModel(null, a => { }));
				AssertExceptionThrown<ArgumentException>(() => new ScreenedPartyWinModel(screenedPartyModel, null));
			});

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			party = new ScreeningParty(vessel, "ABC", vessel);
			responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			screenedPartyModel = new ScreenedPartyModel(responseWithParty, Factory);
			winModel = new ScreenedPartyWinModel(new ScreenedPartyModel(responseWithParty, Factory), a => { });
			AssertEquals(vessel.RV_Code + ": ABC", winModel.ParentsDescription);
			AssertEquals(DpsImageSources.Vessel, winModel.EntityTypeIcon);

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			party = new ScreeningParty(docAddress, "AAA", docAddress);
			responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			screenedPartyModel = new ScreenedPartyModel(responseWithParty, Factory);
			winModel = new ScreenedPartyWinModel(screenedPartyModel, a => { });
			AssertEquals("AAA", winModel.ParentsDescription);
			AssertEquals(DpsImageSources.Organization, winModel.EntityTypeIcon);
		}

		public void TestEntityTypeIcon()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(header, "", header);
			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = new List<ProfileHeaderInfo>() }, new DpsRequestHeaderWithAddressMatching());
			var partyModel = new ScreenedPartyModel(responseWithParty, Factory);
			var winModel = new ScreenedPartyWinModel(partyModel, a => { });

			AssertEquals(DpsImageSources.Organization, winModel.EntityTypeIcon);
			winModel.EntityTypeIcon = DpsImageSources.Vessel;
			AssertEquals(DpsImageSources.Vessel, winModel.EntityTypeIcon);
			winModel.EntityTypeIcon = DpsImageSources.Organization;
			AssertEquals(DpsImageSources.Organization, winModel.EntityTypeIcon);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			party = new ScreeningParty(header, "", vessel);
			responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = new List<ProfileHeaderInfo>() }, new DpsRequestHeaderWithAddressMatching());
			partyModel = new ScreenedPartyModel(responseWithParty, Factory);
			winModel = new ScreenedPartyWinModel(partyModel, a => { });
			AssertEquals(DpsImageSources.Vessel, winModel.EntityTypeIcon);
			winModel.EntityTypeIcon = DpsImageSources.Person;
			AssertEquals(DpsImageSources.Person, winModel.EntityTypeIcon);

			var natHeader = Factory.NewWithValidTestData<OrgHeader>();
			natHeader.OH_Category = "NAT";
			Factory.Save();
			party = new ScreeningParty(natHeader, "", natHeader);
			responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = new List<ProfileHeaderInfo>() }, new DpsRequestHeaderWithAddressMatching());
			partyModel = new ScreenedPartyModel(responseWithParty, Factory);
			winModel = new ScreenedPartyWinModel(partyModel, a => { });
			AssertEquals(DpsImageSources.Person, winModel.EntityTypeIcon);

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_IsResidential = true;
			Factory.Save();
			party = new ScreeningParty(docAddress, "", docAddress);
			responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = new List<ProfileHeaderInfo>() }, new DpsRequestHeaderWithAddressMatching());
			partyModel = new ScreenedPartyModel(responseWithParty, Factory);
			winModel = new ScreenedPartyWinModel(partyModel, a => { });
			AssertEquals(DpsImageSources.Person, winModel.EntityTypeIcon);
		}

		public void TestOnlyOnePotentialMatchAndItIsExcluded()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var headerPk = Guid.NewGuid();
			var headerNamePk = Guid.NewGuid();
			var responseWithParty = new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "For test1",
				Profiles = new List<ProfileHeaderInfo>()
				{
					new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } }, SourceListCodes = new List<string>() { ExcludedSourceListCode }, TypeOfEntity = "PER" },
				},
				NameMatches = new List<NameMatchInfo>()
				{
					new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = headerNamePk, MatchingNameScore = 100, SourceProfileID = headerPk },
				},
				AddressMatches = new List<AddressMatchInfo>(),
				RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
			}, new DpsRequestHeaderWithAddressMatching());
			var screenedPartyModel = new ScreenedPartyModel(responseWithParty, Factory);
			var winModel = new ScreenedPartyWinModel(screenedPartyModel, a => { });

			CombineAssertions(() =>
			{
				AssertEquals(1, winModel.TotalPotentialMatchWinModels.Count);
				AssertEquals(0, winModel.PotentialMatchWinModels.Count);
				AssertEquals(false, winModel.HasPotentialMatches);
				AssertEquals(false, winModel.ShowExcluded);
				AssertEquals(false, winModel.ShowExcludedEnabled);
			});
		}

		public void TestTwoPotentialMatchesAndOneIsExcludedOneIsIncluded()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var headerPk = Guid.NewGuid();
			var headerNamePk = Guid.NewGuid();
			var responseWithParty = new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "For test1",
				Profiles = new List<ProfileHeaderInfo>()
				{
					new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } }, SourceListCodes = new List<string>() { IncludedSourceListCode }, TypeOfEntity = "PER" },
					new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } }, SourceListCodes = new List<string>() { ExcludedSourceListCode }, TypeOfEntity = "PER" },
				},
				NameMatches = new List<NameMatchInfo>()
				{
					new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = headerNamePk, MatchingNameScore = 100, SourceProfileID = headerPk },
				},
				AddressMatches = new List<AddressMatchInfo>(),
				RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
			}, new DpsRequestHeaderWithAddressMatching());
			var model = new ScreenedPartyModel(responseWithParty, Factory);
			var winModel = new ScreenedPartyWinModel(model, a => { });

			AssertEquals(2, winModel.TotalPotentialMatchWinModels.Count);
			var model1 = winModel.TotalPotentialMatchWinModels[0];
			var model2 = winModel.TotalPotentialMatchWinModels[1];

			CombineAssertions(() =>
			{
				AssertEquals(false, model1.IsExcluded);
				AssertEquals(true, model2.IsExcluded);
				AssertEquals(true, winModel.HasPotentialMatches);
				AssertEquals(1, winModel.PotentialMatchWinModels.Count);
				AssertEquals(false, winModel.ShowExcluded);
				AssertEquals(true, winModel.ShowExcludedEnabled);
				AssertEquals(model1, winModel.SelectedPotentialMatchWinModel);
			});

			winModel.ShowExcluded = true;
			AssertEquals(2, winModel.PotentialMatchWinModels.Count);
			AssertEquals(true, winModel.ShowExcluded);

			winModel.SelectedPotentialMatchWinModel = model2;
			AssertEquals(model2, winModel.SelectedPotentialMatchWinModel);

			winModel.ShowExcluded = false;
			AssertEquals(model1, winModel.SelectedPotentialMatchWinModel);
		}

		#region IDeniedPartyResultItem

		public void TestScreeningStatus_Clear()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(header, "", header);
			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = new List<ProfileHeaderInfo>() }, new DpsRequestHeaderWithAddressMatching());
			var partyModel = new ScreenedPartyModel(responseWithParty, Factory);
			var winModel = new ScreenedPartyWinModel(partyModel, a => { });
			AssertEquals(ScreeningStatusesList.Codes.Clear, winModel.NewScreeningStatus);
		}

		public void TestScreeningStatus_CancelledOrNot()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var headerPk = Guid.NewGuid();
			var headerNamePk = Guid.NewGuid();
			var responseWithParty = new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "For test1",
				Profiles = new List<ProfileHeaderInfo>()
				{
					new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } }, SourceListCodes = new List<string>() { IncludedSourceListCode }, TypeOfEntity = "PER" },
				},
				NameMatches = new List<NameMatchInfo>()
				{
					new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = headerNamePk, MatchingNameScore = 100, SourceProfileID = headerPk },
				},
				AddressMatches = new List<AddressMatchInfo>(),
				RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
			}, new DpsRequestHeaderWithAddressMatching());
			var model = new ScreenedPartyModel(responseWithParty, Factory);
			var winModel = new ScreenedPartyWinModel(model, a => { });
			AssertEquals(ScreeningStatusesList.Codes.Canceled, winModel.NewScreeningStatus);

			winModel.ScreeningStatusWinModel.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			winModel.ExecuteSaveCommand();
			AssertEquals(ScreeningStatusesList.Codes.Matched, winModel.NewScreeningStatus);
			AssertEquals(true, winModel.IsSaved);
		}

		public void TestFullClearingReason()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Org";

			var headerPK = Guid.NewGuid();
			var headerNamePK = Guid.NewGuid();

			var responseWithParty = new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "For Test",
				Profiles = new List<ProfileHeaderInfo>()
				{
					new ProfileHeaderInfo { SourceProfileID = headerPK, ProfileNotes = Compressor.Zip("Test"), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePK, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPK } }, SourceListCodes = new List<string>() { "sourceListCode" }, TypeOfEntity = "PER" },
				},
				NameMatches = new List<NameMatchInfo>()
				{
					new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = headerNamePK, MatchingNameScore = 100, SourceProfileID = headerPK },
				},
				AddressMatches = new List<AddressMatchInfo>(),
				RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
			}, new DpsRequestHeaderWithAddressMatching());

			var winModel = new ScreenedPartyWinModel(new ScreenedPartyModel(responseWithParty, Factory), b => { });

			winModel.ScreeningStatusWinModel.ScreeningStatus = "CLR";
			winModel.ScreeningStatusWinModel.ClearingReason = "OTH";
			winModel.ScreeningStatusWinModel.ClearingReasonText = "123 456 789";
			AssertEquals("OTH, Other, 123 456 789", winModel.FullClearingReason);

			winModel.ScreeningStatusWinModel.ScreeningStatus = "MAT";
			AssertEquals("", winModel.FullClearingReason);
		}

		public void TestScreenedEntity()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(header, "", header);
			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = new List<ProfileHeaderInfo>() }, new DpsRequestHeaderWithAddressMatching());
			var model = new ScreenedPartyModel(responseWithParty, Factory);
			var winModel = new ScreenedPartyWinModel(model, a => { });
			AssertEquals(header, winModel.ScreenedEntity);
		}

		public void TestParents()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(header, "", header);
			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = new List<ProfileHeaderInfo>() }, new DpsRequestHeaderWithAddressMatching());
			var model = new ScreenedPartyModel(responseWithParty, Factory);
			var winModel = new ScreenedPartyWinModel(model, a => { });
			AssertContainsExactElementsInAnyOrder(party.Parents, winModel.Parents);
		}

		public void TestHighConfidenceResults_MediumConfidenceResults_LowConfidenceResultsCount()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			CommonTestDataHelper.CreateComplianceList(Factory, "TESTDUMMY1", false);

			var profilePK = Guid.NewGuid();
			var confidenceThreshold = OrganisationsDataRegistry.Instance.GetDPSMatchingConfidenceThreshold(DeniedPartyConstants.ScreeningNameTypes.Person);
			var nameMatches = new List<NameMatchInfo>
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = DeniedPartyConstants.ScreeningNameTypes.Person, FullName = "ABC" }, MatchingNameID = Guid.NewGuid(), MatchingNameScore = confidenceThreshold.HighThreshold, SourceProfileID = profilePK },
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = DeniedPartyConstants.ScreeningNameTypes.Person, FullName = "EFG" }, MatchingNameID = Guid.NewGuid(), MatchingNameScore = confidenceThreshold.MediumThreshold, SourceProfileID = profilePK },
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = DeniedPartyConstants.ScreeningNameTypes.Person, FullName = "EEE" }, MatchingNameID = Guid.NewGuid(), MatchingNameScore = confidenceThreshold.MediumThreshold - 1, SourceProfileID = profilePK },
			};

			var profileHeader = new List<ProfileHeaderInfo>
			{
				new ProfileHeaderInfo
				{
					SourceProfileID = profilePK,
					ProfileNotes = Array.Empty<byte>(),
					ProfileNames = new List<ProfileNameInfo>
					{
						new ProfileNameInfo { ID = nameMatches[0].MatchingNameID, FullName = "ABC", Language = "EN", IsPrimaryName = true, SourceProfileID = profilePK },
						new ProfileNameInfo { ID = nameMatches[1].MatchingNameID, FullName = "EFG 2", Language = "EN", IsPrimaryName = true, SourceProfileID = profilePK },
						new ProfileNameInfo { ID = nameMatches[2].MatchingNameID, FullName = "Low Match", Language = "EN", IsPrimaryName = true, SourceProfileID = profilePK },
						new ProfileNameInfo { ID = Guid.NewGuid(), FullName = "No Match", Language = "EN", IsPrimaryName = true, SourceProfileID = profilePK },
					},
					SourceListCodes = new[] { "TESTDUMMY1" },
					TypeOfEntity = "PER"
				}
			};

			var model = new ScreenedPartyModel(new DpsResponseWithScreeningParty(new ScreeningParty(header, "", header), new DpsResponse { NameMatches = nameMatches, Profiles = profileHeader }, new DpsRequestHeaderWithAddressMatching()), Factory);
			var winModel = new ScreenedPartyWinModel(model, b => { });
			AssertEquals($@"1 RECORDS
{DpsLog.LineBreak}
Profile Name: ABC, Source List: TESTDUMMY1

Name ABC Matched to ABC, Score: 80%
Name EFG Matched to EFG 2, Score: 65%
", winModel.HighConfidenceResults);
			AssertEquals(@"0 RECORDS
", winModel.MediumConfidenceResults);
			AssertEquals(0, winModel.LowConfidenceResultsCount);
		}

		public void TestCountryImagesSourceIcon()
		{
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Compressor.Zip("Test"), TypeOfEntity = "COY" },
			};
			var country = Factory.NewWithValidTestData<RefCountry>();
			var party = new ScreeningParty(country, "Country", country);
			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			var screenedPartyModel = new ScreenedPartyModel(responseWithParty, Factory);
			var winModel = new ScreenedPartyWinModel(new ScreenedPartyModel(responseWithParty, Factory), a => { });
			AssertEquals(DpsImageSources.Country, winModel.EntityTypeIcon);
		}

		#endregion

		const string ExcludedSourceListCode = "SourceList1";
		const string IncludedSourceListCode = "SourceList2";
		protected override void SetUp()
		{
			base.SetUp();

			var complianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList1.RCL_ListCode = ExcludedSourceListCode;
			complianceList1.RCL_IsExcluded = true;

			var complianceList2 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList2.RCL_ListCode = IncludedSourceListCode;
			complianceList2.RCL_IsExcluded = false;
			Factory.Save();
		}
	}
}
