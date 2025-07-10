using System;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class ScreenedDeniedCountryItemModelTest : TestCase
	{
		public void TestCountryItemModel_WithHighMatchScore()
		{
			var profileCountry = GetProfileCountryInfo();
			var model = new ScreenedDeniedCountryItemModel(GetCountryMatchInfo(100), profileCountry);
			AssertNotNull(model.CountryMatchInfo);
			AssertEquals("Exact", model.DisplayScore);
			AssertEquals(ScoreGrades.High, model.ScoreGrade);
			AssertEquals(100, model.Score);
		}

		public void TestCountryItemModel_WithEmptyMatchScore()
		{
			var model = new ScreenedDeniedCountryItemModel(GetProfileCountryInfo());
			AssertNull(model.CountryMatchInfo);
			AssertEquals(string.Empty, model.DisplayScore);
			AssertEquals(ScoreGrades.Low, model.ScoreGrade);
		}

		public void TestCountryItemModel_WithArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedCountryItemModel(null));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedCountryItemModel(null, GetProfileCountryInfo()));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedCountryItemModel(GetCountryMatchInfo(10), null));
		}

		ProfileCountryInfo GetProfileCountryInfo(Guid? pk = null, Guid? profilePK = null)
		{
			return new ProfileCountryInfo { ID = pk ?? Guid.NewGuid(), Code = "AU", CountryName = "Australia", SourceProfileID = profilePK ?? Guid.NewGuid() };
		}

		CountryMatchInfo GetCountryMatchInfo(int score, Guid? profilePK = null)
		{
			return new CountryMatchInfo { RequestCountry = new DpsCountryCandidate { CountryCode = "AU" }, MatchingCountryId = Guid.NewGuid(), MatchingCountryScore = score, SourceProfileID = profilePK ?? Guid.NewGuid() };
		}
	}
}
