using System;
using System.Collections.Generic;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class NameMatchWinModelTest : TransactionedTestCase
	{
		public void TestConstructorArgumentNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NameMatchWinModel(null, true));
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
				var model = new NameMatchWinModel(new NameMatchModel(profileNameInfos, nameMatchInfos), true);

				CombineAssertions(() =>
				{
					AssertEquals(2, model.ScreenedDeniedItems.Count);
					ScreenedDeniedItemWinModelTest.AssertScreenedDeniedItemWinModel("Test Full Name 3", "Test Full Name 1", "99%", ScoreGrades.High, 99, model.ScreenedDeniedItems[0]);
					ScreenedDeniedItemWinModelTest.AssertScreenedDeniedItemWinModel("Test Full Name 4", "Test Full Name 1", "99%", ScoreGrades.High, 99, model.ScreenedDeniedItems[1]);

					AssertEquals(2, model.OtherScreenedDeniedItems.Count);
					ScreenedDeniedItemWinModelTest.AssertScreenedDeniedItemWinModel("Test Full Name 5", "Test Full Name 2", "59%", ScoreGrades.Low, 59, model.OtherScreenedDeniedItems[0]);
					ScreenedDeniedItemWinModelTest.AssertScreenedDeniedItemWinModel(string.Empty, "Other Name", string.Empty, ScoreGrades.Low, 0, model.OtherScreenedDeniedItems[1]);

					AssertEquals(true, model.MatchViewVisibility);
				});

				model = new NameMatchWinModel(new NameMatchModel(profileNameInfos, nameMatchInfos), false);

				AssertEquals(false, model.MatchViewVisibility);
			}
		}

		public void TestExpanderWinModel()
		{
			var model = new NameMatchWinModel(new NameMatchModel(new List<ProfileNameInfo>(), new List<NameMatchInfo>()), true);
			CombineAssertions(() =>
			{
				AssertEquals("Name", model.ExpanderTitle);
				AssertEquals("Name", model.OddName);
				AssertEquals("Names", model.PluralName);
			});
		}
	}
}
