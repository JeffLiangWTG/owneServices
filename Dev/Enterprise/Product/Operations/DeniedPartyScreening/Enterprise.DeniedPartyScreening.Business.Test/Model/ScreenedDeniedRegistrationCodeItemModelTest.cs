using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class ScreenedDeniedRegistrationCodeItemModelTest : TestCaseWithFactory
	{
		public void TestRegCodeItemModel_WithHighMatchScore()
		{
			var model = new ScreenedDeniedRegistrationCodeItemModel(GetRegistrationCodeMatchInfo(100), GetProfileRegistrationCodeInfo());
			AssertNotNull(model.RegistrationCodeMatchInfo);
			AssertEquals("Exact", model.DisplayScore);
			AssertEquals(ScoreGrades.High, model.ScoreGrade);
			AssertEquals(100, model.Score);
		}

		public void TestRegCodeItemModel_WithEmptyMatchScore()
		{
			var model = new ScreenedDeniedRegistrationCodeItemModel(GetProfileRegistrationCodeInfo());
			AssertNull(model.RegistrationCodeMatchInfo);
			AssertEquals(string.Empty, model.DisplayScore);
			AssertEquals(ScoreGrades.Low, model.ScoreGrade);
		}
		public void TestRegCodeItemModel_WithLowMatchScore()
		{
			var profileCode = GetProfileRegistrationCodeInfo();
			var model = new ScreenedDeniedRegistrationCodeItemModel(profileCode);
			AssertNull(model.RegistrationCodeMatchInfo);
			AssertEquals(string.Empty, model.DisplayScore);
			AssertEquals(ScoreGrades.Low, model.ScoreGrade);
		}

		public void TestRegCodeItemModel_WithArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedRegistrationCodeItemModel(null));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedRegistrationCodeItemModel(null, GetProfileRegistrationCodeInfo()));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedRegistrationCodeItemModel(GetRegistrationCodeMatchInfo(10), null));
		}

		public void TestRegCodeItemModel_HighConfidenceResultCount()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var complianceList = Factory.NewWithValidTestData<RefComplianceList>();
			var profilePK = Guid.NewGuid();
			var confidenceThreshold = OrganisationsDataRegistry.Instance.GetDPSMatchingConfidenceThreshold(DeniedPartyConstants.ScreeningNameTypes.Person);
			var nameMatches = new List<NameMatchInfo>
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = DeniedPartyConstants.ScreeningNameTypes.Person, FullName = "ABC" }, MatchingNameID = Guid.NewGuid(), MatchingNameScore = confidenceThreshold.HighThreshold, SourceProfileID = profilePK },
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = DeniedPartyConstants.ScreeningNameTypes.Person, FullName = "EFG" }, MatchingNameID = Guid.NewGuid(), MatchingNameScore = confidenceThreshold.MediumThreshold, SourceProfileID = profilePK },
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = DeniedPartyConstants.ScreeningNameTypes.Person, FullName = "EEE" }, MatchingNameID = Guid.NewGuid(), MatchingNameScore = confidenceThreshold.MediumThreshold - 1, SourceProfileID = profilePK },
			};
			var regCode = new List<RegistrationCodeMatchInfo>
			{
				GetRegistrationCodeMatchInfo(100, profilePK),
				GetRegistrationCodeMatchInfo(1, profilePK),
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
						new ProfileNameInfo { ID = nameMatches[1].MatchingNameID, FullName =  "EFG 2", Language = "EN", IsPrimaryName =  true, SourceProfileID = profilePK },
						new ProfileNameInfo { ID = nameMatches[2].MatchingNameID, FullName = "Low Match", Language = "EN", IsPrimaryName =  true, SourceProfileID = profilePK },
						new ProfileNameInfo { ID = Guid.NewGuid(), FullName = "No Match", Language = "EN", IsPrimaryName = true, SourceProfileID = profilePK },
					},
					ProfileRegistrationCodes = new List<ProfileRegistrationCodeInfo>
					{
						GetProfileRegistrationCodeInfo(regCode[0].MatchingRegistrationCodeID, profilePK),
						GetProfileRegistrationCodeInfo(regCode[1].MatchingRegistrationCodeID, profilePK),
						GetProfileRegistrationCodeInfo(Guid.NewGuid(), profilePK),
					},
					SourceListCodes = new[] { complianceList.RCL_ListCode.ToString() },
					TypeOfEntity = "PER"
				}
			};
			var model = new ScreenedPartyModel(new DpsResponseWithScreeningParty(new ScreeningParty(header, "", header), new DpsResponse { NameMatches = nameMatches, RegistrationCodeMatches = regCode, Profiles = profileHeader }, new DpsRequestHeaderWithAddressMatching()), Factory);

			AssertEquals($@"1 RECORDS
{DpsLog.LineBreak}
Profile Name: ABC, Source List: {complianceList.RCL_ListCode}

Name ABC Matched to ABC, Score: 80%
Name EFG Matched to EFG 2, Score: 65%
Registration Code AU Matched to Dummy, Score: Exact
", model.HighConfidenceResults);
			AssertEquals(@"0 RECORDS
", model.MediumConfidenceResults);
			AssertEquals(0, model.LowConfidenceResultsCount);
		}

		ProfileRegistrationCodeInfo GetProfileRegistrationCodeInfo(Guid? pk = null, Guid? profilePK = null)
		{
			return new ProfileRegistrationCodeInfo { ID = pk ?? Guid.NewGuid(), IdType = "Dummy", IdNumber = "Dummy", IdCountry = "Dummy", SourceProfileID = profilePK ?? Guid.NewGuid() };
		}

		RegistrationCodeMatchInfo GetRegistrationCodeMatchInfo(int score, Guid? profilePK = null)
		{
			return new RegistrationCodeMatchInfo { RequestRegistrationCode = new DpsRegistrationCodeCandidate { RegCountryCode = "1234", RegCodeType = "1234", RegCodeValue = "AU" }, MatchingRegistrationCodeID = Guid.NewGuid(), MatchingRegistrationCodeScore = score, SourceProfileID = profilePK ?? Guid.NewGuid() };
		}
	}
}
