using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateEntryLocationValidation))]
	public class RateEntryLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLocation()
		{
			var collection = Helper
				.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.FCL)
				.RateEntryLocations;

			var locationSetting = collection.AddNew();
			locationSetting.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.FirstLoad;

			locationSetting.Location = ZString.Empty;
			AssertEquals(
				"LocationInfo should have error when location is empty.",
				true,
				locationSetting.LocationInfo.HasError("Please enter a Location.")
			);

			locationSetting.Location = "XXYYZ";
			AssertEquals(
				"LocationInfo should have error when location is invalid.",
				true,
				locationSetting.LocationInfo.HasError("Enter a valid Location.")
			);

			locationSetting.Location = "AUSYD";
			AssertEquals(
				"LocationInfo should not have errors when location is valid.",
				false,
				locationSetting.LocationInfo.HasErrors()
			);
		}

		public void TestLocationSourceOption_MandatoryValidation()
		{
			var collection = Helper
				.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.FCL)
				.RateEntryLocations;

			var locationSetting = collection.AddNew();
			locationSetting.Location = "AUSYD";

			locationSetting.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.FirstLoad;
			AssertEquals("Location source option should not have errors initially.", false, locationSetting.LocationSourceOptionInfo.HasErrors());

			locationSetting.LocationSourceOption = ZString.Empty;
			AssertEquals("An error should be present when LocationSourceOption is empty.", true, locationSetting.LocationSourceOptionInfo.HasError("Please enter a Related Job Field."));

			locationSetting.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.FirstLoad;
			AssertEquals("Location source option should not have errors after assigning FirstLoad.", false, locationSetting.LocationSourceOptionInfo.HasErrors());
		}

		public void TestLocationSourceOption_ListValidation()
		{
			var validOptionCodes = new[]
			{
				"1LD", // FirstLoad             refer to TI_FirstLoadLRC
				"LDC", // LastDischarge         refer to TI_LastDischargeLRC
				"MLD", // FirstRouteSetLoad     refer to TI_FirstRouteSetLoadPortLRC
				"MDC", // LastRouteSetDischarge refer to TI_LastRouteSetDischargePortLRC
			};

			var collection = Helper
				.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.FCL)
				.RateEntryLocations;

			var locationSetting = collection.AddNew();
			locationSetting.Location = "AUSYD";

			foreach (var locationSourceOption in validOptionCodes)
			{
				locationSetting.LocationSourceOption = locationSourceOption;
				AssertEquals(
					$"Expected no errors for LocationSourceOption '{locationSourceOption}' but found errors.",
					false,
					locationSetting.LocationSourceOptionInfo.HasErrors()
				);
			}

			locationSetting.LocationSourceOption = "XYZ";
			AssertEquals(
				"Expected an error message 'Enter a valid Related Job Field.' for invalid LocationSourceOption.",
				true,
				locationSetting.LocationSourceOptionInfo.HasError("Enter a valid Related Job Field.")
			);

			locationSetting.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.FirstLoad;
			AssertEquals(
				"Expected no errors for valid LocationSourceOption 'FirstLoad'.",
				false,
				locationSetting.LocationSourceOptionInfo.HasErrors()
			);
		}

		public void TestGivenOriginChargesRateEntry_WhenOriginIsEmpty_ThenOriginIsValid()
		{
			var originCharges = Helper
				.NewCosting(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.ORG);

			originCharges.TI_OriginLRC = "";
			originCharges.TI_DestinationLRC = "";

			AssertEquals("Origin LRC Info should not have errors.", false, originCharges.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Destination LRC Info should not have errors.", false, originCharges.TI_DestinationLRCInfo.HasErrors());
		}

		public void TestGivenDestinationChargesRateEntry_WhenDestinationIsEmpty_ThenDestinationIsValid()
		{
			var destinationCharges = Helper
				.NewCosting(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.DST);

			destinationCharges.TI_OriginLRC = "";
			destinationCharges.TI_DestinationLRC = "";

			AssertEquals("Origin LRC should not have errors.", false, destinationCharges.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Destination LRC should not have errors.", false, destinationCharges.TI_DestinationLRCInfo.HasErrors());
		}

		#region Helpers

		protected TestHelper Helper =>
			helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		#endregion
	}
}
