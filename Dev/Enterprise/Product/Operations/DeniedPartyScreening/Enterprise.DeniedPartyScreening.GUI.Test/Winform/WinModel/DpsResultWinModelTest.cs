using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class DpsResultWinModelTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var resultWinModel = new DpsResultWinModel(new DpsResultModel(new List<DpsResponseWithScreeningParty>(), Factory));
			CombineAssertions(() =>
			{
				AssertEquals("Screened Parties", resultWinModel.ScreenedPartiesText);
				AssertEquals(0, resultWinModel.ScreenedPartiesCount);
				AssertEquals(0, resultWinModel.PersistentScreenedPartiesCount);
				AssertEquals(true, resultWinModel.AllPartiesClear);
			});

			resultWinModel = new DpsResultWinModel(CreateResultModel(Factory), true);
			var screenedParties = resultWinModel.ScreenedParties;
			var totalScreenedParties = resultWinModel.TotalScreenedParties;

			CombineAssertions(() =>
			{
				AssertEquals(false, resultWinModel.AllPartiesClear);
				AssertEquals(3, totalScreenedParties.Count);
				AssertEquals("AllScreenedParties same as TotalScreenedParties", ((IDpsResult)resultWinModel).AllScreenedParties, resultWinModel.TotalScreenedParties);
				AssertScreenedPartyWinModel("Hua Wei", PartyTypes.Organization, 2, 1, 2, totalScreenedParties[0]);
				AssertScreenedPartyWinModel("USA", PartyTypes.JobDocAddress, 0, 0, 0, totalScreenedParties[1]);
				AssertScreenedPartyWinModel("DaiMaru", PartyTypes.Vessel, 2, 2, 2, totalScreenedParties[2]);

				AssertEquals(2, screenedParties.Count);
				AssertScreenedPartyWinModel("Hua Wei", PartyTypes.Organization, 2, 1, 2, screenedParties[0]);
				AssertScreenedPartyWinModel("DaiMaru", PartyTypes.Vessel, 2, 2, 2, screenedParties[1]);
				AssertEquals(resultWinModel.ScreenedParties[0], resultWinModel.SelectedScreenedParty);

				resultWinModel.Navigate(true);
				AssertEquals(resultWinModel.ScreenedParties[1], resultWinModel.SelectedScreenedParty);

				resultWinModel.Navigate(false);
				AssertEquals(resultWinModel.ScreenedParties[0], resultWinModel.SelectedScreenedParty);
			});
		}

		public void TestCredentialOverride()
		{
			var resultWinModel = new DpsResultWinModel(CreateResultModel(Factory));
			AssertEquals("Precondition: ", resultWinModel.ScreenedParties[0], resultWinModel.SelectedScreenedParty);

			resultWinModel.ScreenedParties[0].ScreeningStatusWinModel.CredentialOverride = "Test User";
			AssertNull("Precondition: ", resultWinModel.CredentialOverride);

			resultWinModel.ScreenedParties[0].ExecuteSaveCommand();
			AssertEquals("Test User", resultWinModel.CredentialOverride);
		}

		public void TestSave()
		{
			var fireClose = false;
			var resultWinModel = new DpsResultWinModel(CreateResultModel(Factory), false) { Close = () => { fireClose = true; } };
			var screenedParties = resultWinModel.ScreenedParties;
			var screenedPartyModel1 = screenedParties[0];
			var screenedPartyModel2 = screenedParties[1];

			screenedPartyModel1.ExecuteSaveCommand();
			CombineAssertions(() =>
			{
				AssertEquals(true, screenedPartyModel1.IsSaved);
				AssertEquals(screenedPartyModel2, resultWinModel.SelectedScreenedParty);
				AssertEquals(1, resultWinModel.ScreenedPartiesCount);
				AssertEquals(2, resultWinModel.PersistentScreenedPartiesCount);
			});

			screenedPartyModel2.ExecuteSaveCommand();
			CombineAssertions(() =>
			{
				AssertEquals(true, screenedPartyModel2.IsSaved);
				AssertEquals(null, resultWinModel.SelectedScreenedParty);
				AssertEquals(0, resultWinModel.ScreenedPartiesCount);
				AssertEquals(2, resultWinModel.PersistentScreenedPartiesCount);
				AssertEquals(true, fireClose);
			});
		}

		void AssertScreenedPartyWinModel(string expectedName, PartyTypes expectedType, int expectedCount, int expectedIndex, int expectedPersistentTotalRecordsCount, ScreenedPartyWinModel model)
		{
			AssertEquals("Name", expectedName, model.PartyName);
			AssertEquals("PartyType", expectedType, model.PartyType);
			AssertEquals("TotalRecordsCount", expectedCount, model.TotalRecordsCount);
			AssertEquals("Index", expectedIndex, model.Index);
			AssertEquals(true, model.StandAlone);
			AssertEquals(expectedPersistentTotalRecordsCount, model.PersistentTotalRecordsCount);
		}

		public static DpsResultModel CreateResultModel(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Hua Wei";

			var vessel = factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "DaiMaru";

			var docAddress = factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_CompanyName = "USA";

			var header1PK = Guid.NewGuid();
			var header1NamePK = Guid.NewGuid();

			var header2PK = Guid.NewGuid();
			var header2NamePK = Guid.NewGuid();

			var header3PK = Guid.NewGuid();
			var header3NamePK = Guid.NewGuid();

			var header4PK = Guid.NewGuid();
			var header4NamePK = Guid.NewGuid();

			var header5PK = Guid.NewGuid();
			var header5NamePK = Guid.NewGuid();

			var header6PK = Guid.NewGuid();
			var header6NamePK = Guid.NewGuid();

			var responses = new List<DpsResponseWithScreeningParty>()
			{
				new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse
				{
					ResponseCode = DpsResponseCode.Successful,
					ExtraMessage = "For test1",
					Profiles = new List<ProfileHeaderInfo>()
					{
						new ProfileHeaderInfo { SourceProfileID = header1PK, ProfileNotes = Compressor.Zip("Test"), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = header1NamePK, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = header1PK } }, SourceListCodes = new List<string>() { SourceListCode1 }, TypeOfEntity = "PER" },
					},
					NameMatches = new List<NameMatchInfo>()
					{
						new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = header1NamePK, MatchingNameScore = 100, SourceProfileID = header1PK },
					},
					AddressMatches = new List<AddressMatchInfo>(),
					RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
				}, new DpsRequestHeaderWithAddressMatching()),
				new DpsResponseWithScreeningParty(new ScreeningParty(docAddress, string.Empty, docAddress), new DpsResponse
				{
					ResponseCode = DpsResponseCode.Successful,
					ExtraMessage = "For test2",
					Profiles = new List<ProfileHeaderInfo>()
					{
						new ProfileHeaderInfo { SourceProfileID = header2PK, ProfileNotes = Compressor.Zip("Test"), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = header2NamePK, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = header2PK } }, SourceListCodes = new List<string>() { SourceListCode2 }, TypeOfEntity = "PER" },
						new ProfileHeaderInfo { SourceProfileID = header3PK, ProfileNotes = Compressor.Zip("Test"), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = header3NamePK, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = header3PK } }, SourceListCodes = new List<string>() { SourceListCode2 }, TypeOfEntity = "PER" },
					},
					NameMatches = new List<NameMatchInfo>()
					{
						new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = header2NamePK, MatchingNameScore = 100, SourceProfileID = header2PK },
						new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = header3NamePK, MatchingNameScore = 100, SourceProfileID = header3PK },
					},
					AddressMatches = new List<AddressMatchInfo>(),
					RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
				}, new DpsRequestHeaderWithAddressMatching()),
				new DpsResponseWithScreeningParty(new ScreeningParty(vessel, string.Empty, vessel), new DpsResponse
				{
					ResponseCode = DpsResponseCode.Successful,
					ExtraMessage = "For test3",
					Profiles = new List<ProfileHeaderInfo>()
					{
						new ProfileHeaderInfo { SourceProfileID = header4PK, ProfileNotes = Compressor.Zip("Test"), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = header4NamePK, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = header4PK } }, SourceListCodes = new List<string>() { SourceListCode1 }, TypeOfEntity = "PER" },
						new ProfileHeaderInfo { SourceProfileID = header5PK, ProfileNotes = Compressor.Zip("Test"), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = header5NamePK, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = header5PK } }, SourceListCodes = new List<string>() { SourceListCode1 }, TypeOfEntity = "PER" },
						new ProfileHeaderInfo { SourceProfileID = header6PK, ProfileNotes = Compressor.Zip("Test"), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = header6NamePK, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = header6PK } }, SourceListCodes = new List<string>() { SourceListCode1 }, TypeOfEntity = "PER" },
					},
					NameMatches = new List<NameMatchInfo>()
					{
						new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = header4NamePK, MatchingNameScore = 100, SourceProfileID = header4PK },
						new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = header5NamePK, MatchingNameScore = 100, SourceProfileID = header5PK },
						new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = header6NamePK, MatchingNameScore = 100, SourceProfileID = header6PK },
					},
					AddressMatches = new List<AddressMatchInfo>(),
					RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
				}, new DpsRequestHeaderWithAddressMatching())
			};

			return new DpsResultModel(responses, factory);
		}

		public static DpsResultModel CreateResultModel1(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Hua Wei";

			var vessel = factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "DaiMaru";

			var docAddress = factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_CompanyName = "USA";

			var header1PK = Guid.NewGuid();
			var header1NamePK = Guid.NewGuid();

			var responses = new List<DpsResponseWithScreeningParty>()
			{
				new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse
				{
					ResponseCode = DpsResponseCode.Successful,
					ExtraMessage = "For test1",
					Profiles = new List<ProfileHeaderInfo>()
					{
						new ProfileHeaderInfo { SourceProfileID = header1PK, ProfileNotes = Compressor.Zip("Test"), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = header1NamePK, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = header1PK } }, SourceListCodes = new List<string>() { SourceListCode1 }, TypeOfEntity = "PER" },
					},
					NameMatches = new List<NameMatchInfo>()
					{
						new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = header1NamePK, MatchingNameScore = 100, SourceProfileID = header1PK },
					},
					AddressMatches = new List<AddressMatchInfo>(),
					RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
				}, new DpsRequestHeaderWithAddressMatching())
			};

			return new DpsResultModel(responses, factory);
		}

		const string SourceListCode1 = "SourceList1";
		const string SourceListCode2 = "SourceList2";

		public static void SetUpComplianceList(BusinessObjectFactory factory)
		{
			var complianceList1 = factory.NewWithValidTestData<RefComplianceList>();
			complianceList1.RCL_ListCode = SourceListCode1;
			complianceList1.RCL_IsExcluded = false;

			var complianceList2 = factory.NewWithValidTestData<RefComplianceList>();
			complianceList2.RCL_ListCode = SourceListCode2;
			complianceList2.RCL_IsExcluded = true;

			factory.Save();
		}

		protected override void SetUp()
		{
			SetUpComplianceList(Factory);
		}
	}
}
