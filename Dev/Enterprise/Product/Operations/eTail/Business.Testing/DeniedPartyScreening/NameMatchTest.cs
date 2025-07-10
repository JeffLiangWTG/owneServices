using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;
using static Enterprise.eTail.Business.DeniedPartyScreening.ProfileHeader;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(NameMatch))]
	public class NameMatchTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NameMatch(GetNameMatchInfo(85), GetProfileNameInfo());
		}

		public void TestNameItemModel_WithEmptyMatchSCore()
		{
			var model = new NameMatch(null, GetProfileNameInfo());
			CombineAssertions(() =>
			{
				AssertNull(model.NameMatchInfo);
				AssertEquals("Name", model.Type);
				AssertEquals("软件大道", model.Name);
				AssertEquals(0, model.Score);
				AssertEquals(RiskLevel.Low, model.Level);
			});
		}

		public void TestNameItemModel_WithHighMatchSCore()
		{
			var model = new NameMatch(GetNameMatchInfo(85), GetProfileNameInfo());
			CombineAssertions(() =>
			{
				AssertNotNull(model.NameMatchInfo);
				AssertEquals("软件大道", model.Name);
				AssertEquals(85, model.Score);
				AssertEquals(RiskLevel.High, model.Level);
			});
		}

		public void TestNameItemModel_WithMediumMatchSCore()
		{
			var model = new NameMatch(GetNameMatchInfo(65), GetProfileNameInfo());
			CombineAssertions(() =>
			{
				AssertNotNull(model.NameMatchInfo);
				AssertEquals("软件大道", model.Name);
				AssertEquals(65, model.Score);
				AssertEquals(RiskLevel.Medium, model.Level);
			});
		}

		public void TestNameItemModel_WithLowMatchSCore()
		{
			var model = new NameMatch(GetNameMatchInfo(60), GetProfileNameInfo());
			CombineAssertions(() =>
			{
				AssertNotNull(model.NameMatchInfo);
				AssertEquals("软件大道", model.Name);
				AssertEquals(60, model.Score);
				AssertEquals(RiskLevel.Low, model.Level);
			});
		}

		public void TestNameItemModel_WithArgumentNullExcpetion()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new NameMatch(GetNameMatchInfo(60), null));
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

