using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class RegistrationCodeMatchModelTest : TestCaseWithFactory
	{
		public void TestConstructor_ArgumentNullException()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => _ = new RegistrationCodeMatchModel(null, new List<RegistrationCodeMatchInfo>()));
				AssertExceptionThrown<ArgumentNullException>(() => _ = new RegistrationCodeMatchModel(new List<ProfileRegistrationCodeInfo>(), null));
			});
		}

		public void TestScreenedDeniedItems()
		{
			var profileId1 = Guid.NewGuid();
			var profileId2 = Guid.NewGuid();
			var profileId3 = Guid.NewGuid();

			var regCodeId1 = Guid.NewGuid();
			var regCodeId2 = Guid.NewGuid();
			var regCodeId3 = Guid.NewGuid();
			var profileRegistrationCodeInfos = new List<ProfileRegistrationCodeInfo>()
			{
				new ProfileRegistrationCodeInfo { ID = regCodeId1, IdType = "A", IdNumber = "123", IdCountry = "CN", SourceProfileID = profileId1 },
				new ProfileRegistrationCodeInfo { ID = regCodeId2, IdType = "B", IdNumber = "456", IdCountry = "US", SourceProfileID = profileId2 },
				new ProfileRegistrationCodeInfo { ID = regCodeId3, IdType = "C", IdNumber = "789", IdCountry = "AU", SourceProfileID = profileId3 },
			};

			var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>()
			{
				new RegistrationCodeMatchInfo { RequestRegistrationCode = new DpsRegistrationCodeCandidate { RegCountryCode = "CN", RegCodeType = "A", RegCodeValue = "123" }, MatchingRegistrationCodeID = regCodeId1, MatchingRegistrationCodeScore = 100, SourceProfileID = profileId1 },
				new RegistrationCodeMatchInfo { RequestRegistrationCode = new DpsRegistrationCodeCandidate { RegCountryCode = "CN", RegCodeType = "A", RegCodeValue = "12" }, MatchingRegistrationCodeID = regCodeId1, MatchingRegistrationCodeScore = 10, SourceProfileID = profileId1 },
				new RegistrationCodeMatchInfo { RequestRegistrationCode = new DpsRegistrationCodeCandidate { RegCountryCode = "US", RegCodeType = "B", RegCodeValue = "456" }, MatchingRegistrationCodeID = regCodeId2, MatchingRegistrationCodeScore = 10, SourceProfileID = profileId2 },
				new RegistrationCodeMatchInfo { RequestRegistrationCode = new DpsRegistrationCodeCandidate { RegCountryCode = "AU", RegCodeType = "C", RegCodeValue = "789" }, MatchingRegistrationCodeID = Guid.NewGuid(), MatchingRegistrationCodeScore = 1, SourceProfileID = profileId2 },
			};

			var model = new RegistrationCodeMatchModel(profileRegistrationCodeInfos, registrationCodeMatchInfos);

			CombineAssertions(() =>
			{
				AssertEquals(4, model.TotalScreenedDeniedItems.Count);
				AssertScreenedDeniedItemViewModel(registrationCodeMatchInfos[0], profileRegistrationCodeInfos[0], "Exact", ScoreGrades.High, model.TotalScreenedDeniedItems[0]);
				AssertScreenedDeniedItemViewModel(registrationCodeMatchInfos[1], profileRegistrationCodeInfos[0], "None", ScoreGrades.Low, model.TotalScreenedDeniedItems[1]);
				AssertScreenedDeniedItemViewModel(registrationCodeMatchInfos[2], profileRegistrationCodeInfos[1], "None", ScoreGrades.Low, model.TotalScreenedDeniedItems[2]);
				AssertScreenedDeniedItemViewModel(null, profileRegistrationCodeInfos[2], string.Empty, ScoreGrades.Low, model.TotalScreenedDeniedItems[3]);
			});
		}

		void AssertScreenedDeniedItemViewModel(RegistrationCodeMatchInfo matchInfo, ProfileRegistrationCodeInfo profileInfo, string expectedDisplayScore, ScoreGrades expectedScoreGrade, ScreenedDeniedRegistrationCodeItemModel model)
		{
			AssertEquals(matchInfo, model.RegistrationCodeMatchInfo);
			AssertEquals(profileInfo, model.ProfileRegistrationCodeInfo);
			AssertEquals(expectedDisplayScore, model.DisplayScore);
			AssertEquals(expectedScoreGrade, model.ScoreGrade);
		}
	}
}
