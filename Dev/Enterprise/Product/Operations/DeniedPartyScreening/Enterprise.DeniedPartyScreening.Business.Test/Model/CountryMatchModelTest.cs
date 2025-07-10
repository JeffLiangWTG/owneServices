using System;
using System.Collections.Generic;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class CountryMatchModelTest : TestCase
	{
		public void TestConstructorArgumentNull()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => _ = new CountryMatchModel(null, new List<CountryMatchInfo>()));
				AssertExceptionThrown<ArgumentNullException>(() => _ = new CountryMatchModel(new List<ProfileCountryInfo>(), null));
			});
		}

		public void TestTotalScreenedDeniedItems()
		{
			var profileId1 = Guid.NewGuid();
			var profileId2 = Guid.NewGuid();
			var profileId3 = Guid.NewGuid();

			var countryId1 = Guid.NewGuid();
			var countryId2 = Guid.NewGuid();
			var countryId3 = Guid.NewGuid();
			var profileCountryInfos = new List<ProfileCountryInfo>()
			{
				new ProfileCountryInfo { ID = countryId1, Code = "IR", CountryName = "Islamic Republic of Iran", SourceProfileID = profileId1 },
				new ProfileCountryInfo { ID = countryId2, Code = "SY", CountryName = "Syrian Arab Republic", SourceProfileID = profileId2 },
				new ProfileCountryInfo { ID = countryId3, Code = "KP", CountryName = "Korea, Democratic People's Republic", SourceProfileID = profileId3 },
			};

			var score = 100;
			var displayScore = "Exact";
			var countryMatchInfos = new List<CountryMatchInfo>()
			{
				new CountryMatchInfo { RequestCountry = new DpsCountryCandidate { CountryCode = "IR" }, MatchingCountryId = countryId1, MatchingCountryScore = score, SourceProfileID = profileId1 },
				new CountryMatchInfo { RequestCountry = new DpsCountryCandidate { CountryCode = "CU" }, MatchingCountryId = countryId1, MatchingCountryScore = score, SourceProfileID = profileId1 },
				new CountryMatchInfo { RequestCountry = new DpsCountryCandidate { CountryCode = "KP" }, MatchingCountryId = countryId2, MatchingCountryScore = score, SourceProfileID = profileId2 },
				new CountryMatchInfo { RequestCountry = new DpsCountryCandidate { CountryCode = "SY" }, MatchingCountryId = Guid.NewGuid(), MatchingCountryScore = score, SourceProfileID = profileId1 },
			};

			var model = new CountryMatchModel(profileCountryInfos, countryMatchInfos);

			CombineAssertions(() =>
			{
				AssertEquals(4, model.TotalScreenedDeniedItems.Count);
				AssertScreenedDeniedCountryItemModel(countryMatchInfos[0], profileCountryInfos[0], displayScore, ScoreGrades.High, model.TotalScreenedDeniedItems[0]);
				AssertScreenedDeniedCountryItemModel(countryMatchInfos[1], profileCountryInfos[0], displayScore, ScoreGrades.High, model.TotalScreenedDeniedItems[1]);
				AssertScreenedDeniedCountryItemModel(countryMatchInfos[2], profileCountryInfos[1], displayScore, ScoreGrades.High, model.TotalScreenedDeniedItems[2]);
				AssertScreenedDeniedCountryItemModel(null, profileCountryInfos[2], string.Empty, ScoreGrades.Low, model.TotalScreenedDeniedItems[3]);
			});
		}

		void AssertScreenedDeniedCountryItemModel(CountryMatchInfo countryMatchInfo, ProfileCountryInfo profileCountryInfo, string expectedDisplayScore, ScoreGrades expectedScoreGrade, ScreenedDeniedCountryItemModel model)
		{
			AssertEquals(countryMatchInfo, model.CountryMatchInfo);
			AssertEquals(profileCountryInfo, model.ProfileCountryInfo);
			AssertEquals(expectedDisplayScore, model.DisplayScore);
			AssertEquals(expectedScoreGrade, model.ScoreGrade);
		}
	}
}
