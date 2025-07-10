using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateEntryLocationCollection))]
	public class RateEntryLocationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RateEntryLocationCollection>
	{
		public void TestParentRateEntryNotToBeNull()
		{
			AssertExceptionThrown<ArgumentNullException>(
				"ParentRateEntry cannot be null",
				() => new RateEntryLocationCollection(parentRateEntry: null));

			Assert("FluentAssertions used", true);
		}

		public void TestLoadTI_FirstLoadLRC()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.FCL);
			entry.TI_FirstLoadLRC = "HKHKG";

			var rateEntryLocations = entry.RateEntryLocations.Cast<RateEntryLocation>();

			var actualLocation = rateEntryLocations
				.Single(x => x.LocationSourceOption == RateEntryLookups.LocationSourceOption.Code.FirstLoad)
				.Location;

			AssertEquals("The location should match TI_FirstLoadLRC.", entry.TI_FirstLoadLRC, actualLocation);
		}

		public void TestLoadTI_LastDischargeLRC()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.FCL);
			entry.TI_LastDischargeLRC = "HKHKG";

			var rateEntryLocations = entry.RateEntryLocations.Cast<RateEntryLocation>();

			var actualLocation = rateEntryLocations
				.Single(x => x.LocationSourceOption == RateEntryLookups.LocationSourceOption.Code.LastDischarge)
				.Location;

			AssertEquals("The location should match TI_LastDischargeLRC of the entry.", entry.TI_LastDischargeLRC, actualLocation);
		}

		public void TestLoadTI_FirstRouteSetLoadPortLRC()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.FCL);
			entry.TI_FirstRouteSetLoadPortLRC = "HKHKG";

			var rateEntryLocations = entry.RateEntryLocations.Cast<RateEntryLocation>();

			var actualLocation = rateEntryLocations
				.Single(x => x.LocationSourceOption == RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad)
				.Location;

			AssertEquals("The location should match the TI_FirstRouteSetLoadPortLRC", entry.TI_FirstRouteSetLoadPortLRC, actualLocation);
		}

		public void TestLoadTI_LastRouteSetDischargePortLRC()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.FCL);
			entry.TI_LastRouteSetDischargePortLRC = "HKHKG";

			var rateEntryLocations = entry.RateEntryLocations.Cast<RateEntryLocation>();

			var actualLocation = rateEntryLocations
				.Single(x => x.LocationSourceOption == RateEntryLookups.LocationSourceOption.Code.LastRouteSetDischarge)
				.Location;

			AssertEquals("The location does not match the expected value.", entry.TI_LastRouteSetDischargePortLRC, actualLocation);
		}

		public void TestFindLocation()
		{
			var rateEntryLocations = Helper
				.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.FCL)
				.RateEntryLocations;

			// When there is not
			var result = rateEntryLocations.FindLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad);
			AssertEquals("Expected result to be ZString.Empty when no matching location is found.", ZString.Empty, result);

			var lastDischargeSetting = rateEntryLocations.AddNew();
			lastDischargeSetting.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.LastDischarge;
			lastDischargeSetting.Location = "HKHKG";

			// When there is only one
			result = rateEntryLocations.FindLocation(RateEntryLookups.LocationSourceOption.Code.LastDischarge);
			AssertEquals("Expected the location to match the only entry.", "HKHKG", result);

			var firstRouteSetLoadSetting1 = rateEntryLocations.AddNew();
			firstRouteSetLoadSetting1.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad;
			firstRouteSetLoadSetting1.Location = "AUSYD";

			var firstRouteSetLoadSetting2 = rateEntryLocations.AddNew();
			firstRouteSetLoadSetting2.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad;
			firstRouteSetLoadSetting2.Location = "HKHKG";

			// When there are more than one
			result = rateEntryLocations.FindLocation(RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad);
			AssertEquals("Expected result to be ZString.Empty when multiple matching locations are found.", ZString.Empty, result);
		}

		public void TestUpdateLocation()
		{
			var rateEntryLocations = Helper
				.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.FCL)
				.RateEntryLocations;

			// When there is not
			rateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad, "HKHKG");
			AssertEquals("HKHKG", rateEntryLocations.FindLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad));

			var lastDischargeSetting = rateEntryLocations.AddNew();
			lastDischargeSetting.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.LastDischarge;
			lastDischargeSetting.Location = "AUSYD";
			// When there is only one
			rateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.LastDischarge, "HKHKG");
			AssertEquals("HKHKG", rateEntryLocations.FindLocation(RateEntryLookups.LocationSourceOption.Code.LastDischarge));

			var firstRouteSetLoadSetting1 = rateEntryLocations.AddNew();
			firstRouteSetLoadSetting1.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad;
			firstRouteSetLoadSetting1.Location = "AUSYD";
			var firstRouteSetLoadSetting2 = rateEntryLocations.AddNew();
			firstRouteSetLoadSetting2.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad;
			firstRouteSetLoadSetting2.Location = "USLAX";
			// When there are more than one
			rateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad, "HKHKG");
			AssertEquals("HKHKG", rateEntryLocations.FindLocation(RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad));
		}

		public void TestPopulateBackToRateEntry()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.FCL);

			AssertEquals("TI_FirstLoadLRC should be empty initially.", ZString.Empty, entry.TI_FirstLoadLRC);
			AssertEquals("TI_LastDischargeLRC should be empty initially.", ZString.Empty, entry.TI_LastDischargeLRC);
			AssertEquals("TI_FirstRouteSetLoadPortLRC should be empty initially.", ZString.Empty, entry.TI_FirstRouteSetLoadPortLRC);
			AssertEquals("TI_LastRouteSetDischargePortLRC should be empty initially.", ZString.Empty, entry.TI_LastRouteSetDischargePortLRC);

			var rateEntryLocations = entry.RateEntryLocations;

			rateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad, "AUSYD");
			rateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.LastDischarge, "AUBRN");
			rateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad, "AUMEL");
			rateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.LastRouteSetDischarge, "USLAX");
			rateEntryLocations.PopulateBackToRateEntry();

			// when there is only one item in the collection for each location field
			AssertEquals("TI_FirstLoadLRC should be populated with 'AUSYD'", "AUSYD", entry.TI_FirstLoadLRC);
			AssertEquals("TI_LastDischargeLRC should be populated with 'AUBRN'", "AUBRN", entry.TI_LastDischargeLRC);
			AssertEquals("TI_FirstRouteSetLoadPortLRC should be populated with 'AUMEL'", "AUMEL", entry.TI_FirstRouteSetLoadPortLRC);
			AssertEquals("TI_LastRouteSetDischargePortLRC should be populated with 'USLAX'", "USLAX", entry.TI_LastRouteSetDischargePortLRC);

			rateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad, "");
			rateEntryLocations.PopulateBackToRateEntry();

			// when there is an item in the collection with empty value
			AssertEquals("Cannot populate back due to mandatory Location.", "AUSYD", entry.TI_FirstLoadLRC);

			rateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad, "AUSYD");

			rateEntryLocations.RemoveAndDelete(rateEntryLocations.Cast<RateEntryLocation>().Single(x => x.LocationSourceOption == RateEntryLookups.LocationSourceOption.Code.LastDischarge));
			rateEntryLocations.PopulateBackToRateEntry();

			// when there is no item in the collection
			AssertEquals("TI_LastDischargeLRC should be empty when no item exists in the collection.", ZString.Empty, entry.TI_LastDischargeLRC);
		}

		public void TestPopulateBackToRateEntry_WhenThereIsError()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.FCL);
			var rateEntryLocations = entry.RateEntryLocations;

			rateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad, "AUSYD");
			rateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.LastDischarge, "AUBRN");
			rateEntryLocations[0].LocationSourceOption = "XYZ"; // Invalid code to cause an error

			rateEntryLocations.PopulateBackToRateEntry();
			// when there is an error
			AssertEquals("TI_FirstLoadLRC should be empty when there is an error.", ZString.Empty, entry.TI_FirstLoadLRC);
			AssertEquals("TI_LastDischargeLRC should be empty when there is an error.", ZString.Empty, entry.TI_LastDischargeLRC);

			rateEntryLocations[0].LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.FirstLoad;

			rateEntryLocations.PopulateBackToRateEntry();
			// when the error has been fixed
			AssertEquals("TI_FirstLoadLRC should match 'AUSYD' after fixing the error.", "AUSYD", entry.TI_FirstLoadLRC);
			AssertEquals("TI_LastDischargeLRC should match 'AUBRN' after fixing the error.", "AUBRN", entry.TI_LastDischargeLRC);
		}

		public void TestLoad()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.FCL);

			entry.TI_FirstLoadLRC = "AUSYD";
			entry.TI_LastDischargeLRC = "AUBRN";
			entry.TI_FirstRouteSetLoadPortLRC = "AUMEL";
			entry.TI_LastRouteSetDischargePortLRC = "USLAX";

			var collection = new RateEntryLocationCollection(entry); // Load method will be called in the construction method
			AssertEquals("The count of locations should be 4.", 4, collection.Count);
			AssertEquals("The first load location should be 'AUSYD'.", "AUSYD", collection.FindLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad));
			AssertEquals("The last discharge location should be 'AUBRN'.", "AUBRN", collection.FindLocation(RateEntryLookups.LocationSourceOption.Code.LastDischarge));
			AssertEquals("The first route set load location should be 'AUMEL'.", "AUMEL", collection.FindLocation(RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad));
			AssertEquals("The last route set discharge location should be 'USLAX'.", "USLAX", collection.FindLocation(RateEntryLookups.LocationSourceOption.Code.LastRouteSetDischarge));

			entry.TI_FirstLoadLRC = "";
			entry.TI_LastDischargeLRC = "AUBRN";
			entry.TI_FirstRouteSetLoadPortLRC = "AUMEL";
			entry.TI_LastRouteSetDischargePortLRC = "";

			collection = new RateEntryLocationCollection(entry); // Load method will be called in the construction method
			AssertEquals("The count of locations should be 2.", 2, collection.Count);
			AssertEquals("The first load location should be an empty string.", "", collection.FindLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad));
			AssertEquals("The last discharge location should be 'AUBRN'.", "AUBRN", collection.FindLocation(RateEntryLookups.LocationSourceOption.Code.LastDischarge));
			AssertEquals("The first route set load location should be 'AUMEL'.", "AUMEL", collection.FindLocation(RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad));
			AssertEquals("The last route set discharge location should be an empty string.", "", collection.FindLocation(RateEntryLookups.LocationSourceOption.Code.LastRouteSetDischarge));
		}

		protected override RateEntryLocationCollection GetCollectionToTest() =>
			Helper.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.FCL)
				.RateEntryLocations;

		protected override BusinessObject GetNewElementToAddToTheCollection() =>
			Collection.AddNew();

		#region Helpers

		protected TestHelper Helper =>
			helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		#endregion
	}
}
