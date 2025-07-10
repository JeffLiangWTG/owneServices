using CargoWise.EntityFramework.Testing;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Mode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.Rating.Business.Testing
{
	class BulkRateUpdaterContainerTypeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRC_Code()
		{
			var bulkRateUpdater = new BulkRateUpdater();
			bulkRateUpdater.Module = "FWD";
			bulkRateUpdater.Type = Category.FCL;
			bulkRateUpdater.Mode = Mode.SEA;

			var containerType20GP = bulkRateUpdater.ContainerTypes.AddNew();
			containerType20GP.RC_Code = "XXX";
			AssertHasError("Invalid ContainerType", containerType20GP.RC_CodeInfo, "Enter a valid Container Code.");

			containerType20GP.RC_Code = "20GP";
			AssertNoErrors("Valid ContainerType", containerType20GP.RC_CodeInfo);
		}

		public void TestInvalidateRC_Code_DifferentModeSelected_AIR()
		{
			var bulkRateUpdater = new BulkRateUpdater();
			bulkRateUpdater.Module = "FWD";
			bulkRateUpdater.Type = Category.AIR;
			bulkRateUpdater.Mode = Mode.ULD;

			var seaContainer = bulkRateUpdater.ContainerTypes.AddNew();
			seaContainer.RC_Code = "20GP";
			AssertHasError("Invalid ContainerType", seaContainer.RC_CodeInfo, "Enter a valid Container Code.");

			var roadContainer = bulkRateUpdater.ContainerTypes.AddNew();
			roadContainer.RC_Code = "FLAT";
			AssertHasError("Invalid ContainerType", roadContainer.RC_CodeInfo, "Enter a valid Container Code.");

			var airContainer = bulkRateUpdater.ContainerTypes.AddNew();
			airContainer.RC_Code = "AAA";
			AssertNoErrors("Valid ContainerType", airContainer.RC_CodeInfo);
		}
		public void TestInvalidateRC_Code_DifferentModeSelected_SEA()
		{
			var bulkRateUpdater = new BulkRateUpdater();
			bulkRateUpdater.Module = "FWD";
			bulkRateUpdater.Type = Category.FCL;
			bulkRateUpdater.Mode = Mode.SEA;

			var seaContainer = bulkRateUpdater.ContainerTypes.AddNew();
			seaContainer.RC_Code = "20GP";
			AssertNoErrors("Valid ContainerType", seaContainer.RC_CodeInfo);

			var roadContainer = bulkRateUpdater.ContainerTypes.AddNew();
			roadContainer.RC_Code = "FLAT";
			AssertHasError("Invalid ContainerType", roadContainer.RC_CodeInfo, "Enter a valid Container Code.");

			var airContainer = bulkRateUpdater.ContainerTypes.AddNew();
			airContainer.RC_Code = "AAA";
			AssertHasError("Invalid ContainerType", airContainer.RC_CodeInfo, "Enter a valid Container Code.");
		}

		public void TestInvalidateRC_Code_DifferentModeSelected_ROA()
		{
			var bulkRateUpdater = new BulkRateUpdater();
			bulkRateUpdater.Module = "FWD";
			bulkRateUpdater.Type = Category.FCL;
			bulkRateUpdater.Mode = Mode.ROA;

			var seaContainer = bulkRateUpdater.ContainerTypes.AddNew();
			seaContainer.RC_Code = "20GP";
			AssertHasError("Invalid ContainerType", seaContainer.RC_CodeInfo, "Enter a valid Container Code.");

			var roadContainer = bulkRateUpdater.ContainerTypes.AddNew();
			roadContainer.RC_Code = "FLAT";
			AssertNoErrors("Valid ContainerType", roadContainer.RC_CodeInfo);

			var airContainer = bulkRateUpdater.ContainerTypes.AddNew();
			airContainer.RC_Code = "AAA";
			AssertHasError("Invalid ContainerType", airContainer.RC_CodeInfo, "Enter a valid Container Code.");
		}
	}
}
