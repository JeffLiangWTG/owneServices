using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class LocationWithCapacityTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor_NullArguments()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxWeight = 0;
			location.WLV_MaxCubic = 0;
			location.WLV_MaxQuantity = 0;

			var locationWithCapacity1 = new LocationWithCapacity(location, 1.5, 2.6, 3.7);
			AssertEquals(location, locationWithCapacity1.Location);
			AssertNull(locationWithCapacity1.Weight);
			AssertNull(locationWithCapacity1.Volume);
			AssertNull(locationWithCapacity1.Quantity);
		}

		public void TestConstructor_NonNullArguments()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxWeight = 1;
			location.WLV_MaxCubic = 1;
			location.WLV_MaxQuantity = 1;

			var locationWithCapacity2 = new LocationWithCapacity(location, 1.5, 2.6, 3.7);
			AssertEquals(location, locationWithCapacity2.Location);
			AssertEquals(1.5m, locationWithCapacity2.Weight.Value);
			AssertEquals(2.6m, locationWithCapacity2.Volume.Value);
			AssertEquals(3.7m, locationWithCapacity2.Quantity.Value);
		}

		#endregion
	}
}
