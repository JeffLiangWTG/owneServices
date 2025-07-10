using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Registry.Business;

namespace Enterprise.DeniedPartyScreening.Business.Test.V4
{
	public class NameMatchModelTest : TestCaseWithFactory
	{
		public void TestConstructor_ArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new NameMatchModel(new List<ProfileNameInfo>(), null));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new NameMatchModel(null, new List<NameMatchInfo>()));
		}

		public void TestConstructor()
		{
			var profileId1 = Guid.NewGuid();
			var profileId2 = Guid.NewGuid();
			var profileId3 = Guid.NewGuid();

			var nameId1 = Guid.NewGuid();
			var nameId2 = Guid.NewGuid();
			var nameId3 = Guid.NewGuid();
			var profileNameInfos = new List<ProfileNameInfo>()
			{
				new ProfileNameInfo { ID = nameId1, FullName = "Test Full Name 1", Language = "Chinese", IsPrimaryName = true, SourceProfileID = profileId1 },
				new ProfileNameInfo { ID = nameId2, FullName = "Test Full Name 2", Language = "Chinese", IsPrimaryName = true, SourceProfileID = profileId2 },
				new ProfileNameInfo { ID = nameId3, FullName = "Other Name", Language = "Chinese", IsPrimaryName = true, SourceProfileID = profileId3 },
			};

			var nameMatchInfos = new List<NameMatchInfo>()
			{
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "ORG", FullName = "Test Full Name 3" }, MatchingNameID = nameId1, MatchingNameScore = 99, SourceProfileID = profileId1 },
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "ORG", FullName = "Test Full Name 4" }, MatchingNameID = nameId1, MatchingNameScore = 99, SourceProfileID = profileId1 },
				new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "ORG", FullName = "Test Full Name 5" }, MatchingNameID = nameId2, MatchingNameScore = 59, SourceProfileID = profileId2 },
			};

			using (OrganisationsDataRegistry.Instance.MatchingConfidenceThresholdsForOrganisations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DpsConfidenceThresholdsBusinessObject(65, 90)))
			{
				var model = new NameMatchModel(profileNameInfos, nameMatchInfos);

				CombineAssertions(() =>
				{
					AssertEquals(4, model.TotalScreenedDeniedItems.Count);
					AssertScreenedDeniedItemViewModel(nameMatchInfos[0], profileNameInfos[0], "99%", ScoreGrades.High, model.TotalScreenedDeniedItems[0]);
					AssertScreenedDeniedItemViewModel(nameMatchInfos[1], profileNameInfos[0], "99%", ScoreGrades.High, model.TotalScreenedDeniedItems[1]);
					AssertScreenedDeniedItemViewModel(nameMatchInfos[2], profileNameInfos[1], "59%", ScoreGrades.Low, model.TotalScreenedDeniedItems[2]);
					AssertScreenedDeniedItemViewModel(null, profileNameInfos[2], string.Empty, ScoreGrades.Low, model.TotalScreenedDeniedItems[3]);
				});
			}
		}

		void AssertScreenedDeniedItemViewModel(NameMatchInfo nameMatchInfo, ProfileNameInfo profileNameInfo, string expectedDisplayScore, ScoreGrades expectedScoreGrade, ScreenedDeniedNameItemModel model)
		{
			AssertEquals(nameMatchInfo, model.NameMatchInfo);
			AssertEquals(profileNameInfo, model.ProfileNameInfo);
			AssertEquals(expectedDisplayScore, model.DisplayScore);
			AssertEquals(expectedScoreGrade, model.ScoreGrade);
		}
	}
}
