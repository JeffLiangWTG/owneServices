using CargoWise.Application;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists.US;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Environment.Business.US.Testing
{
	class WhsLocationValidationTest : WhsLocationValidationTestCase
	{
		#region TestCheckWLV_ApprovedKnownLocation

		public void TestCheckWLV_ApprovedKnownLocation()
		{
			var helper = new WhsTestHelperFunctionsEnvUS(Factory);
			var warehouse = helper.CreateWarehouse("TST WHS", "A");
			var location = warehouse.DefaultLocation;
			location.WLV_ApprovedKnownLocation = "";
			AssertHasError(location.WLV_ApprovedKnownLocationInfo, "Please enter a TSA Status.");

			location.WLV_ApprovedKnownLocation = TSAStatus.Codes.Known;
			AssertNoErrors(location.WLV_ApprovedKnownLocationInfo);

			location.WLV_ApprovedKnownLocation = "XXX";
			AssertHasError(location.WLV_ApprovedKnownLocationInfo, "Enter a valid TSA Status.");
			location.WLV_ApprovedKnownLocation = TSAStatus.Codes.Known;

			Factory.Save();

			location.WLV_ApprovedKnownLocation = TSAStatus.Codes.Unknown;
			AssertNoErrors(location.WLV_ApprovedKnownLocationInfo);

			var client = Helper.CreateClient();
			var product = Helper.CreateProduct("BOWLHAT", client);
			var stockHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var inventory = stockHelper.CreateStock(warehouse.PK, client.PK, product.PK, 10m);

			Factory.Save();

			location.WLV_ApprovedKnownLocation = TSAStatus.Codes.Known;
			AssertHasError(location.WLV_ApprovedKnownLocationInfo, "Cannot change TSA status as stock exists in this location.");
		}

		#endregion
	}
}
