using System;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class ScreenedDeniedNameItemModelTest : TestCase
	{
		public void TestAddressItemModel_WithEmptyMatchSCore()
		{
			var model = new ScreenedDeniedNameItemModel(GetProfileNameInfo());
			AssertNull(model.NameMatchInfo);
			AssertEquals(string.Empty, model.DisplayScore);
			AssertEquals(ScoreGrades.Low, model.ScoreGrade);
		}

		public void TestAddressItemModel_WithHighMatchSCore()
		{
			var model = new ScreenedDeniedNameItemModel(GetNameMatchInfo(85), GetProfileNameInfo());
			AssertNotNull(model.NameMatchInfo);
			AssertEquals("85%", model.DisplayScore);
			AssertEquals(ScoreGrades.High, model.ScoreGrade);
			AssertEquals(85, model.Score);
		}

		public void TestAddressItemModel_WithMediumMatchSCore()
		{
			var model = new ScreenedDeniedNameItemModel(GetNameMatchInfo(65), GetProfileNameInfo());
			AssertNotNull(model.NameMatchInfo);
			AssertEquals("65%", model.DisplayScore);
			AssertEquals(ScoreGrades.Medium, model.ScoreGrade);
			AssertEquals(65, model.Score);
		}

		public void TestAddressItemModel_WithLowMatchSCore()
		{
			var model = new ScreenedDeniedNameItemModel(GetNameMatchInfo(60), GetProfileNameInfo());
			AssertNotNull(model.NameMatchInfo);
			AssertEquals("60%", model.DisplayScore);
			AssertEquals(ScoreGrades.Low, model.ScoreGrade);
			AssertEquals(60, model.Score);
		}

		public void TestAddressItemModel_WithArgumentNullExcpetion()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedNameItemModel(null));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedNameItemModel(null, GetProfileNameInfo()));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new ScreenedDeniedNameItemModel(GetNameMatchInfo(60), null));
		}

		ProfileNameInfo GetProfileNameInfo()
		{
			return new ProfileNameInfo { ID = Guid.NewGuid(), FullName = "软件大道", Language = "ZH-CN", IsPrimaryName = true, SourceProfileID = Guid.NewGuid() };
		}

		NameMatchInfo GetNameMatchInfo(int score)
		{
			return new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "PER", FullName = "天安数码城", LanguageCode = "ZH-CN" }, MatchingNameID = Guid.NewGuid(), MatchingNameScore = score, SourceProfileID = Guid.NewGuid() };
		}
	}
}
