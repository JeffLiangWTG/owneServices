using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateEntryLocation))]
	public class RateEntryLocationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParentCollectionNotToBeNull()
		{
			AssertExceptionThrown<ArgumentNullException>(
				"ParentCollection has to be NOT null",
				() => new RateEntryLocation(parentCollection: null));
		}

		public void TestLocation()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.FCL);
			var locationInfo = entry.RateEntryLocations.AddNew().LocationInfo;

			AssertEquals("Location as a UNLOCO has got maximum 5 letters", 5, locationInfo.MaxLength);
			AssertEquals(true, locationInfo.HasHumanReadableName);
			AssertEquals("Location", locationInfo.HumanReadableName);
		}

		public void TestLocationSourceOption()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.FCL);
			var sourceOptionInfo = entry.RateEntryLocations.AddNew().LocationSourceOptionInfo;

			AssertEquals("Related Job Field is a 3-Letter-Code", 3, sourceOptionInfo.MaxLength);
			Assert("The source option info should have a human-readable name.", sourceOptionInfo.HasHumanReadableName);
			AssertEquals("The human-readable name should be 'Related Job Field'.", "Related Job Field", sourceOptionInfo.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObject() =>
			Helper
				.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.FCL)
				.RateEntryLocations
				.AddNew();

		#region Helpers

		protected TestHelper Helper =>
			helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		#endregion
	}
}
