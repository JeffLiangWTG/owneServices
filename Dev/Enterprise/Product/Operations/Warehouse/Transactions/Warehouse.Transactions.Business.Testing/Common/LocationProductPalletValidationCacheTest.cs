using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class LocationProductPalletValidationCacheTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationProductPalletValidationCache(null, Array.Empty<WhsDocketLine>(), ZGuid.Empty));
			AssertExceptionThrown<ArgumentNullException>(() => new LocationProductPalletValidationCache(Factory, null, ZGuid.Empty));
		}

		public void TestPalletCount_DoesNotAccceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationProductPalletValidationCache(Factory, Array.Empty<WhsDocketLine>(), ZGuid.Empty).GetPalletCountExcludingThisJob(null));
		}

		public void TestRequiredPalletCount_DoesNotAccceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationProductPalletValidationCache(Factory, Array.Empty<WhsDocketLine>(), ZGuid.Empty).GetRequiredLocationPalletCount(null));
		}

		public void TestUsesMoreThanOneProduct_DoesNotAccceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationProductPalletValidationCache(Factory, Array.Empty<WhsDocketLine>(), ZGuid.Empty).GetLocationUsesMoreThanOneProduct(null));
		}

		public void TestPalletCount_LocationInMemory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location = data.Whs1.FindLocation("A-1-1");

			location.WLV_PalletFloorSpaces = 2;
			location.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-2");

			receive.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive.FinaliseDocket();

			var cache = new LocationProductPalletValidationCache(Factory, receive.Lines.ToArray(), receive.PK);
			AssertEquals(0, cache.GetPalletCountExcludingThisJob(location));
		}

		public void TestPalletCount_LocationInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location = data.Whs1.FindLocation("A-1-1");

			location.WLV_PalletFloorSpaces = 2;
			location.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-1");

			receive.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive.FinaliseDocket();
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 500m, location, "Pallet-2");

			receive2.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive2.LocationCapacityValidationManager.ClearLocationAvailableCapacityForTest();
			receive2.FinaliseDocket();

			var cache = new LocationProductPalletValidationCache(Factory, receive2.Lines.ToArray(), receive2.PK);
			AssertEquals(1, cache.GetPalletCountExcludingThisJob(location));
		}

		public void TestPalletCount_ExcludedPalletIDs_LocationInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location = data.Whs1.FindLocation("A-1-1");

			location.WLV_PalletFloorSpaces = 3;
			location.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-2");

			receive.FinaliseDocket();
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 500m, location, "Pallet-1");

			receive2.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive2.LocationCapacityValidationManager.ClearLocationAvailableCapacityForTest();
			receive2.FinaliseDocket();

			var cache = new LocationProductPalletValidationCache(Factory, receive2.Lines.ToArray(), receive2.PK);
			AssertEquals(1, cache.GetPalletCountExcludingThisJob(location));
		}

		public void TestPalletCount_BothPendingStockCurrentStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location = data.Whs1.FindLocation("A-1-1");

			location.WLV_PalletFloorSpaces = 3;
			location.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-1");

			receive.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive.FinaliseDocket();
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 500m, location, "Pallet-1");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 500m, location, "Pallet-2");
			receive2.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 500m, location, "Pallet-3");

			receive3.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive3.LocationCapacityValidationManager.ClearLocationAvailableCapacityForTest();
			receive3.FinaliseDocket();

			var cache = new LocationProductPalletValidationCache(Factory, receive3.Lines.ToArray(), receive3.PK);
			AssertEquals(2, cache.GetPalletCountExcludingThisJob(location));
		}

		public void TestRequiredLocationPalletCount()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location = data.Whs1.FindLocation("A-1-1");

			location.WLV_PalletFloorSpaces = 2;
			location.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-2");

			receive.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive.FinaliseDocket();

			var cache = new LocationProductPalletValidationCache(Factory, receive.Lines.ToArray(), receive.PK);
			AssertEquals(2, cache.GetRequiredLocationPalletCount(location));
		}

		public void TestLocationUsesMoreThanOneProduct_InMemory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location = data.Whs1.FindLocation("A-1-1");

			location.WLV_PalletFloorSpaces = 2;
			location.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 500m, location, "Pallet-2");

			receive.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive.FinaliseDocket();

			var cache = new LocationProductPalletValidationCache(Factory, receive.Lines.ToArray(), receive.PK);
			AssertEquals(true, cache.GetLocationUsesMoreThanOneProduct(location));
		}

		public void TestLocationUsesMoreThanOneProduct_InDB()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location = data.Whs1.FindLocation("A-1-1");

			location.WLV_PalletFloorSpaces = 3;
			location.WLV_PalletStackHeight = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 500m, location, "Pallet-1");

			receive.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive.FinaliseDocket();
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 500m, location, "Pallet-2");

			receive2.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			receive2.LocationCapacityValidationManager.ClearLocationAvailableCapacityForTest();
			receive2.FinaliseDocket();

			var cache = new LocationProductPalletValidationCache(Factory, receive2.Lines.ToArray(), receive2.PK);
			AssertEquals(true, cache.GetLocationUsesMoreThanOneProduct(location));
		}
	}
}
