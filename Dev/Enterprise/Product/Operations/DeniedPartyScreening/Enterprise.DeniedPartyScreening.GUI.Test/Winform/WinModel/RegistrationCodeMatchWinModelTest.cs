using System;
using System.Collections.Generic;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class RegistrationCodeMatchWinModelTest : TestCase
	{
		public void TestConstructorArgumentNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new RegistrationCodeMatchWinModel(null, true));
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

			var model = new RegistrationCodeMatchWinModel(new RegistrationCodeMatchModel(profileRegistrationCodeInfos, registrationCodeMatchInfos), true);

			CombineAssertions(() =>
			{
				AssertEquals(1, model.ScreenedDeniedItems.Count);
				ScreenedDeniedItemWinModelTest.AssertScreenedDeniedItemWinModel("A : 123", "A : 123", "Exact", ScoreGrades.High, 100, model.ScreenedDeniedItems[0]);

				AssertEquals(3, model.OtherScreenedDeniedItems.Count);
				ScreenedDeniedItemWinModelTest.AssertScreenedDeniedItemWinModel("A : 12", "A : 123", "None", ScoreGrades.Low, 10, model.OtherScreenedDeniedItems[0]);
				ScreenedDeniedItemWinModelTest.AssertScreenedDeniedItemWinModel("B : 456", "B : 456", "None", ScoreGrades.Low, 10, model.OtherScreenedDeniedItems[1]);
				ScreenedDeniedItemWinModelTest.AssertScreenedDeniedItemWinModel(string.Empty, "C : 789", string.Empty, ScoreGrades.Low, 0, model.OtherScreenedDeniedItems[2]);

				AssertEquals(true, model.MatchViewVisibility);
			});

			model = new RegistrationCodeMatchWinModel(new RegistrationCodeMatchModel(profileRegistrationCodeInfos, registrationCodeMatchInfos), false);

			AssertEquals(false, model.MatchViewVisibility);
		}

		public void TestExpanderWinModel()
		{
			var model = new RegistrationCodeMatchWinModel(new RegistrationCodeMatchModel(new List<ProfileRegistrationCodeInfo>(), new List<RegistrationCodeMatchInfo>()), true);
			CombineAssertions(() =>
			{
				AssertEquals("Registration Code", model.ExpanderTitle);
				AssertEquals("No Codes Matched", model.ExpanderDescription);
				AssertEquals("Code", model.OddName);
				AssertEquals("Codes", model.PluralName);
			});
		}
	}
}
