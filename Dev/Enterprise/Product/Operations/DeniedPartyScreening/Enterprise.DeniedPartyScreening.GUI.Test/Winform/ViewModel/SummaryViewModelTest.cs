using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class SummaryViewModelTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var resultViewModel = CreateResultViewModel();
			var fireClose = false;
			var summaryViewModel = new SummaryViewModel(resultViewModel.TotalScreenedParties.Cast<IScreenedParty>().ToList(), () => { fireClose = true; });
			AssertEquals("Summary", summaryViewModel.Title);
			AssertEquals("Close", summaryViewModel.CloseText);

			summaryViewModel.CloseCommand.Execute(null);
			AssertEquals(true, fireClose);

			CombineAssertions(() =>
			{
				AssertEquals(3, summaryViewModel.SummaryItemViewModels.Count);
				AssertEquals(1, summaryViewModel.SummaryItemViewModels.Count(x => x.Name == "Header1" && x.ScreeningStatus == ScreeningStatusesList.Descriptions.Clear));
				AssertEquals(1, summaryViewModel.SummaryItemViewModels.Count(x => x.Name == "Header2" && x.ScreeningStatus == ScreeningStatusesList.Descriptions.Unknown));
				AssertEquals(1, summaryViewModel.SummaryItemViewModels.Count(x => x.Name == "Header3" && x.ScreeningStatus == ScreeningStatusesList.Descriptions.Matched));
			});
		}

		DpsResultWinModel CreateResultViewModel()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			header1.OH_FullName = "Header1";
			header1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			header2.OH_FullName = "Header2";
			header2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var header3 = Factory.NewWithValidTestData<OrgHeader>();
			header3.OH_FullName = "Header3";
			header3.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			Factory.Save();

			var header1PK = Guid.NewGuid();
			var header1NamePK = Guid.NewGuid();

			var header2PK = Guid.NewGuid();
			var header2NamePK = Guid.NewGuid();

			var header3PK = Guid.NewGuid();
			var header3NamePK = Guid.NewGuid();

			var responses = new List<DpsResponseWithScreeningParty>()
			{
				new DpsResponseWithScreeningParty(new ScreeningParty(header1, string.Empty, header1), new DpsResponse
				{
					ResponseCode = DpsResponseCode.Successful,
					ExtraMessage = "For test3",
					Profiles = new List<ProfileHeaderInfo>()
					{
						new ProfileHeaderInfo { SourceProfileID = header1PK, ProfileNotes = Compressor.Zip("Test"), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = header1NamePK, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = header1PK } }, SourceListCodes = new List<string>() { sourceListCode }, TypeOfEntity = "PER" },
					},
					NameMatches = new List<NameMatchInfo>()
					{
						new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = header1NamePK, MatchingNameScore = 100, SourceProfileID = header1PK },
					},
					AddressMatches = new List<AddressMatchInfo>(),
					RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
				}, new DpsRequestHeaderWithAddressMatching()),
				new DpsResponseWithScreeningParty(new ScreeningParty(header2, string.Empty, header2), new DpsResponse()
				{
					ResponseCode = DpsResponseCode.Successful,
					ExtraMessage = "For test3",
					Profiles = new List<ProfileHeaderInfo>()
					{
						new ProfileHeaderInfo { SourceProfileID = header2PK, ProfileNotes = Compressor.Zip("Test"), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = header2NamePK, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = header2PK } }, SourceListCodes = new List<string>() { sourceListCode }, TypeOfEntity = "PER" },
					},
					NameMatches = new List<NameMatchInfo>()
					{
						new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = header2NamePK, MatchingNameScore = 100, SourceProfileID = header2PK },
					},
					AddressMatches = new List<AddressMatchInfo>(),
					RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
				}, new DpsRequestHeaderWithAddressMatching()),
				new DpsResponseWithScreeningParty(new ScreeningParty(header3, string.Empty, header3), new DpsResponse
				{
					ResponseCode = DpsResponseCode.Successful,
					ExtraMessage = "For test3",
					Profiles = new List<ProfileHeaderInfo>()
					{
						new ProfileHeaderInfo { SourceProfileID = header3PK, ProfileNotes = Compressor.Zip("Test"), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = header3NamePK, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = header3PK } }, SourceListCodes = new List<string>() { sourceListCode }, TypeOfEntity = "PER" },
					},
					NameMatches = new List<NameMatchInfo>()
					{
						new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = header3NamePK, MatchingNameScore = 100, SourceProfileID = header3PK },
					},
					AddressMatches = new List<AddressMatchInfo>(),
					RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
				}, new DpsRequestHeaderWithAddressMatching())
			};

			var resultModel = new DpsResultModel(responses, Factory);
			var resultViewModel = new DpsResultWinModel(resultModel);

			var header1ViewModel = resultViewModel.ScreenedParties.Single(x => x.PartyName == "Header1");
			header1ViewModel.ScreeningStatusWinModel.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			header1ViewModel.ExecuteSaveCommand();

			var header2ViewModel = resultViewModel.ScreenedParties.Single(x => x.PartyName == "Header2");

			header2ViewModel.ExecuteSaveCommand();

			var header3ViewModel = resultViewModel.ScreenedParties.Single(x => x.PartyName == "Header3");
			header3ViewModel.ScreeningStatusWinModel.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			header3ViewModel.ExecuteSaveCommand();

			return resultViewModel;
		}

		readonly string sourceListCode = "SourceList1";
		protected override void SetUp()
		{
			var complianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList1.RCL_ListCode = sourceListCode;
			complianceList1.RCL_IsExcluded = false;
			Factory.Save();
		}
	}
}
