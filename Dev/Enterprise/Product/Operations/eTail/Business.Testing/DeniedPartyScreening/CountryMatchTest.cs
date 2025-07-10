using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(CountryMatch))]
	public class CountryMatchTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CountryMatch(GetCountryMatchInfo(10), GetProfileCountryInfo());
		}

		public void TestConstructor_ArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new CountryMatch(GetCountryMatchInfo(10), null));
		}

		public void TestCountryItemModel_WithHighMatchScore()
		{
			var profileCountry = GetProfileCountryInfo();
			var model = new CountryMatch(GetCountryMatchInfo(100), profileCountry);
			CombineAssertions(() =>
			{
				AssertNotNull(model.CountryMatchInfo);
				AssertEquals("Country", model.Type);
				AssertEquals("Australia", model.Name);
				AssertEquals(100, model.Score);
			});
		}

		public void TestCountryItemModel_WithEmptyMatchScore()
		{
			var model = new CountryMatch(null, GetProfileCountryInfo());
			CombineAssertions(() =>
			{
				AssertNull(model.CountryMatchInfo);
				AssertEquals("Country", model.Type);
				AssertEquals("Australia", model.Name);
				AssertEquals(0, model.Score);
			});
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

