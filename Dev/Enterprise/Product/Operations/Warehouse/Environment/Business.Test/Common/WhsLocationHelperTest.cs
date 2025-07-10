namespace Enterprise.Warehouse.Environment.Business.Testing
{
	internal class WhsLocationHelperTest : WhsTestCaseWithFactoryEnv
	{
		public void TestGetMaxLocationComponentValueWithFixedWidth()
		{
			AssertEquals(999, WhsLocationHelper.GetMaxLocationComponentValueWithFixedWidth(3));
			AssertEquals(99, WhsLocationHelper.GetMaxLocationComponentValueWithFixedWidth(2));
			AssertEquals(9, WhsLocationHelper.GetMaxLocationComponentValueWithFixedWidth(1));
		}

		public void TestLocationStringCompare()
		{
			var warehouse = Helper.CreateWarehouse("ZZ");
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var location = warehouse.FindLocation("Z-4-3-2");
			AssertNotNull(location);
			AssertEquals(true, WhsLocationHelper.LocationStringCompare(location, "Z-4-3-2"));
			AssertEquals(false, WhsLocationHelper.LocationStringCompare(location, "Z432"));
			AssertEquals(false, WhsLocationHelper.LocationStringCompare(location, "Z00400402"));
			AssertEquals(false, WhsLocationHelper.LocationStringCompare(location, "Z-4-3-1"));
		}

		public void TestLocationStringCompare_FixedWidthLocation()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 3, 3, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var location = warehouse.FindLocation("Z00400302");
			AssertNotNull(location);
			AssertEquals(true, WhsLocationHelper.LocationStringCompare(location, "Z00400302"));
			AssertEquals(true, WhsLocationHelper.LocationStringCompare(location, "Z-004-003-02"));
			AssertEquals(false, WhsLocationHelper.LocationStringCompare(location, "Z-005-004-03"));
		}

		public void TestLocationStringCompare_NullLocation()
		{
			AssertEquals(false, WhsLocationHelper.LocationStringCompare(null, "Any string"));
		}
	}
}
