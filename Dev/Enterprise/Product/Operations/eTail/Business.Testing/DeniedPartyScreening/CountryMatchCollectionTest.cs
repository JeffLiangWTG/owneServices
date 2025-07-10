using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(CountryMatchCollection))]
	sealed class CountryMatchCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CountryMatchCollection>
	{
		protected override CountryMatchCollection GetCollectionToTest() => new CountryMatchCollection(headerInfo, response);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CountryMatch(new CountryMatchInfo(), new ProfileCountryInfo());

		ProfileHeaderInfo headerInfo;
		DpsResponse response;
		Guid country1Id;
		Guid country2Id;

		protected override void SetUp()
		{
			country1Id = Guid.NewGuid();
			country2Id = Guid.NewGuid();
			headerInfo = new ProfileHeaderInfo
			{
				ProfileCountries = new List<ProfileCountryInfo>
				{
					new ProfileCountryInfo { ID = country1Id, CountryName = "USA" },
					new ProfileCountryInfo { ID = country2Id, CountryName = "Canada" }
				}
			};

			response = new DpsResponse
			{
				CountryMatches = new List<CountryMatchInfo>
				{
					new CountryMatchInfo { MatchingCountryId = country1Id, MatchingStandardizedValue = "USA" }
				}
			};
		}

		public void TestBuildCollection_WithMatchingCountries_AddsCountryMatches()
		{
			var collection = new CountryMatchCollection(headerInfo, response);

			AssertEquals(2, collection.Count);
			Assert(collection.OfType<CountryMatch>().Any(x => x.CountryMatchInfo.MatchingCountryId == country1Id));
			Assert(collection.OfType<CountryMatch>().Any(x => x.ProfileCountryInfo.ID == country2Id));
		}

		public void TestBuildCollection_WithNoProfileCountries_DoesNotAddCountryMatches()
		{
			headerInfo.ProfileCountries = null;
			var collection = new CountryMatchCollection(headerInfo, response);

			AssertEquals(0, collection.Count);
		}

		public void TestBuildCollection_WithNoMatchingCountries_AddsProfileCountries()
		{
			response.CountryMatches = new List<CountryMatchInfo>();
			var collection = new CountryMatchCollection(headerInfo, response);

			AssertEquals(2, collection.Count);
			Assert(collection.OfType<CountryMatch>().Any(x => x.CountryMatchInfo == null));
		}

		public void TestBuildCollection_WithEmptyCountryName_DoesNotAddProfileCountries()
		{
			headerInfo.ProfileCountries = new List<ProfileCountryInfo>
			{
				new ProfileCountryInfo { ID = Guid.NewGuid(), CountryName = "" }
			};
			var collection = new CountryMatchCollection(headerInfo, response);

			AssertEquals(0, collection.Count);
		}
	}
}
