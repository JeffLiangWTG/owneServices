using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class ScreenedDeniedAddressItemModelTest : TestCaseWithFactory
	{
		public void TestAddressItemModel_WithEmptyMatchSCore()
		{
			var model = new ScreenedDeniedAddressItemModel(GetProfileAddressInfo());
			AssertNull(model.AddressMatchInfo);
			AssertEquals(string.Empty, model.DisplayScore);
			AssertEquals(ScoreGrades.Low, model.ScoreGrade);
		}

		public void TestAddressItemModel_WithHighMatchSCore()
		{
			var model = new ScreenedDeniedAddressItemModel(GetAddressMatchInfo(85), GetProfileAddressInfo());
			AssertNotNull(model.AddressMatchInfo);
			AssertEquals("85%", model.DisplayScore);
			AssertEquals(ScoreGrades.High, model.ScoreGrade);
			AssertEquals(85, model.Score);
		}

		public void TestAddressItemModel_WithMediumMatchSCore()
		{
			var model = new ScreenedDeniedAddressItemModel(GetAddressMatchInfo(75), GetProfileAddressInfo());
			AssertNotNull(model.AddressMatchInfo);
			AssertEquals("75%", model.DisplayScore);
			AssertEquals(ScoreGrades.Medium, model.ScoreGrade);
			AssertEquals(75, model.Score);
		}

		public void TestAddressItemModel_WithLowMatchSCore()
		{
			var model = new ScreenedDeniedAddressItemModel(GetAddressMatchInfo(60), GetProfileAddressInfo());
			AssertNotNull(model.AddressMatchInfo);
			AssertEquals("60%", model.DisplayScore);
			AssertEquals(ScoreGrades.Low, model.ScoreGrade);
			AssertEquals(60, model.Score);
		}

		public void TestAddressItemModel_WithArgumentNullExcpetion()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedAddressItemModel(null));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedAddressItemModel(null, GetProfileAddressInfo()));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedAddressItemModel(GetAddressMatchInfo(60), null));
		}

		public void TestAddressItemModel_HighConfidenceResultCount()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var complianceList = Factory.NewWithValidTestData<RefComplianceList>();
			var profilePK = Guid.NewGuid();
			var confidenceThreshold = OrganisationsDataRegistry.Instance.GetDPSMatchingConfidenceThreshold(DeniedPartyConstants.ScreeningNameTypes.Person);
			var addressMatches = new List<AddressMatchInfo>
			{
				GetAddressMatchInfo(78, profilePK),
				GetAddressMatchInfo(75, profilePK),
				GetAddressMatchInfo(35, profilePK),
			};
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
						new ProfileNameInfo { ID = nameMatches[1].MatchingNameID, FullName = "EFG 2", Language = "EN", IsPrimaryName = true, SourceProfileID = profilePK },
						new ProfileNameInfo { ID = nameMatches[2].MatchingNameID, FullName = "Low Match", Language = "EN", IsPrimaryName = true, SourceProfileID = profilePK },
						new ProfileNameInfo { ID = Guid.NewGuid(), FullName = "No Match", Language = "EN", IsPrimaryName = true, SourceProfileID = profilePK },
					},
					ProfileAddresses = new List<ProfileAddressInfo>
					{
						GetProfileAddressInfo(addressMatches[0].MatchingAddressID, profilePK),
						GetProfileAddressInfo(addressMatches[1].MatchingAddressID, profilePK),
						GetProfileAddressInfo(addressMatches[2].MatchingAddressID, profilePK),
						GetProfileAddressInfo(Guid.NewGuid(), profilePK),
					},
					SourceListCodes = new[] { complianceList.RCL_ListCode.ToString() },
					TypeOfEntity = "PER"
				}
			};
			var model = new ScreenedPartyModel(new DpsResponseWithScreeningParty(new ScreeningParty(header, "", header), new DpsResponse { NameMatches = nameMatches, AddressMatches = addressMatches, Profiles = profileHeader }, new DpsRequestHeaderWithAddressMatching()), Factory);

			AssertEquals(@"0 RECORDS
", model.HighConfidenceResults);
			AssertEquals($@"1 RECORDS
{DpsLog.LineBreak}
Profile Name: EFG 2, Source List: {complianceList.RCL_ListCode}

Name EFG Matched to EFG 2, Score: 65%
Address 软件大道 天安数码城 D A B C ZH-CN Matched to 软件大道 天安数码城 A C B, Score: 78%
Address 软件大道 天安数码城 D A B C ZH-CN Matched to 软件大道 天安数码城 A C B, Score: 75%
", model.MediumConfidenceResults);
			AssertEquals(0, model.LowConfidenceResultsCount);
		}

		ProfileAddressInfo GetProfileAddressInfo(Guid? pk = null, Guid? profilePK = null)
		{
			return new ProfileAddressInfo { ID = pk ?? Guid.NewGuid(), Street = "软件大道", City = "天安数码城", StateProvince = "A", Country = "B", PostCode = "C", Language = "ZH-CN", SourceProfileID = profilePK ?? Guid.NewGuid() };
		}

		AddressMatchInfo GetAddressMatchInfo(int score, Guid? profilePK = null)
		{
			return new AddressMatchInfo { RequestAddress = new DpsAddressCandidate { Address1 = "软件大道", Address2 = "天安数码城", City = "A", State = "B", PostCode = "C", Country = "ZH-CN", AdditionalAddressLine = "D" }, MatchingAddressID = Guid.NewGuid(), MatchingAddressScore = score, SourceProfileID = profilePK ?? Guid.NewGuid() };
		}
	}
}
