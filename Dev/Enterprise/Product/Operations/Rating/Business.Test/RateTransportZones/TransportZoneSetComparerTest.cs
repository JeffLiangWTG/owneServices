using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class TransportZoneSetComparerTest : TestCaseWithFactory
	{
		public void TestTransportZoneSetComparer()
		{
			var zoneSetOwner = Factory.NewWithValidTestData<OrgHeader>();
			zoneSetOwner.OH_Code = "OWNER";

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "Wenty";
			cityTown.R9_RN_NKCountry = Constants.CountryCodes.Australia;

			var ratingZoneForCity = GetZoneSet(RatingConstants.RatingZoneTypes.Rating, zoneSetOwner, cityTown: cityTown);
			var ratingZoneForCountry = GetZoneSet(RatingConstants.RatingZoneTypes.Rating, zoneSetOwner);

			var allZoneForCity = GetZoneSet(RatingConstants.RatingZoneTypes.All, zoneSetOwner, cityTown: cityTown);
			var allZoneForCountry = GetZoneSet(RatingConstants.RatingZoneTypes.All, zoneSetOwner);

			var ratingGenericZoneForCity = GetZoneSet(RatingConstants.RatingZoneTypes.Rating, null, cityTown: cityTown);
			var ratingGenericZoneForCountry = GetZoneSet(RatingConstants.RatingZoneTypes.Rating, null);

			var allGenericZoneForCity = GetZoneSet(RatingConstants.RatingZoneTypes.All, null, cityTown: cityTown);
			var allGenericZoneForCountry = GetZoneSet(RatingConstants.RatingZoneTypes.All, null);

			var resultsInPseudoRandomOrder = new[]
			{
				allZoneForCountry,
				ratingZoneForCountry,
				allZoneForCity,
				allGenericZoneForCity,
				ratingGenericZoneForCountry,
				ratingZoneForCity,
				allGenericZoneForCountry,
				ratingGenericZoneForCity
			};

			Array.Sort(resultsInPseudoRandomOrder, (zoneSet1, zoneSet2) => new TransportZoneSetComparer().Compare(zoneSet1, zoneSet2));

			var sortedResults = resultsInPseudoRandomOrder;

			AssertEquals("First result should be the most specific", ratingZoneForCity, sortedResults[0]);
			AssertEquals("City and Owner matching is more specific than type", allZoneForCity, sortedResults[1]);
			AssertEquals("City is more specific than country", ratingZoneForCountry, sortedResults[2]);
			AssertEquals("", allZoneForCountry, sortedResults[3]);

			AssertEquals("Zones without owner should appear later", ratingGenericZoneForCity, sortedResults[4]);
			AssertEquals(allGenericZoneForCity, sortedResults[5]);
			AssertEquals(ratingGenericZoneForCountry, sortedResults[6]);
			AssertEquals("Should be least relevant zone", allGenericZoneForCountry, sortedResults[7]);
		}

		public void TestTransportZoneSetComparer_ZoneTypeAndZoneMode()
		{
			var zoneSetForAll = GetZoneSet(RatingConstants.RatingZoneTypes.All, null);
			var zoneSetForRating = GetZoneSet(RatingConstants.RatingZoneTypes.Rating, null);
			var zoneSetForRatingAirLoose = GetZoneSet(RatingConstants.RatingZoneTypes.Rating, null, mode: Core.Constants.RateMode.LSE);
			var zoneSetForRatingAir = GetZoneSet(RatingConstants.RatingZoneTypes.Rating, null, mode: Core.Constants.RateMode.AIR);

			var resultsInPseudoRandomOrder = new[]
			{
				zoneSetForRating,
				zoneSetForAll,
				zoneSetForRatingAirLoose,
				zoneSetForRatingAir
			};

			Array.Sort(resultsInPseudoRandomOrder, (zoneSet1, zoneSet2) => new TransportZoneSetComparer().Compare(zoneSet1, zoneSet2));

			var sortedResults = resultsInPseudoRandomOrder;

			AssertEquals("First result should be the most specific", zoneSetForRatingAirLoose, sortedResults[0]);
			AssertEquals("Second result should be the Air", zoneSetForRatingAir, sortedResults[1]);
			AssertEquals("City and Owner matching is more specific than type", zoneSetForRating, sortedResults[2]);
			AssertEquals("City is more specific than country", zoneSetForAll, sortedResults[3]);
		}

		RateTransportProvider GetZoneSet(string type, OrgHeader owner, string mode = null, RefCityTown cityTown = null)
		{
			var zoneSet = Factory.New<RateTransportProvider>();
			zoneSet.TP_ZoneType = type;

			if (!string.IsNullOrEmpty(mode))
			{
				zoneSet.TP_ZoneMode = mode;
			}

			if (owner != null)
			{
				zoneSet.TP_OH_RelatedParty = owner.PK;
			}

			if (cityTown == null)
			{
				zoneSet.TP_RN_NKCountry = Constants.CountryCodes.Australia;
			}
			else
			{
				zoneSet.TP_R9_ZoneHubLocation = cityTown.PK;
			}

			zoneSet.Zones.AddNew();

			return zoneSet;
		}
	}
}
