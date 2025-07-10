using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class LocationStockOnHandInfoCacheManagerTest : WhsTestCaseWithFactory
	{
		public void TestGetLocationStockOnHandCache()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, locationA1, "PLT2");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locationA1);
			Factory.Save();

			var locationStockOnHandCacheA1 =
				LocationStockOnHandInfoCacheManager.GetLocationStockOnHandCache(Factory, locationA1.PK, receive2.PK);
			AssertNotNull("Cache should exist on factory",
				Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(
					GetStockOnHandLocationCacheKey(locationA1.PK, receive2.PK), () => null));
			var part1LocationStockOnHandInfo = locationStockOnHandCacheA1.Single(line => line.ProductCode == "P1");
			AssertEquals(receive1.PK, part1LocationStockOnHandInfo.DocketPK);
			AssertEquals("PLT1", part1LocationStockOnHandInfo.PalletId);
			AssertEquals("111", part1LocationStockOnHandInfo.ClientCode);
			AssertEquals(data.Org1.PK, part1LocationStockOnHandInfo.ClientPK);
			AssertEquals(data.Part1.PK, part1LocationStockOnHandInfo.ProductPK);

			var part2LocationStockOnHandInfo = locationStockOnHandCacheA1.Single(line => line.ProductCode == "P2");
			AssertEquals(receive1.PK, part2LocationStockOnHandInfo.DocketPK);
			AssertEquals("PLT2", part2LocationStockOnHandInfo.PalletId);
			AssertEquals("111", part2LocationStockOnHandInfo.ClientCode);
			AssertEquals(data.Org1.PK, part2LocationStockOnHandInfo.ClientPK);
			AssertEquals(data.Part2.PK, part2LocationStockOnHandInfo.ProductPK);

			var locationStockOnHandCacheA2 =
				LocationStockOnHandInfoCacheManager.GetLocationStockOnHandCache(Factory, locationA2.PK, receive2.PK);
			AssertEquals(0, locationStockOnHandCacheA2.Count());
			AssertNotNull("Cache should exist on factory",
				Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(
					GetStockOnHandLocationCacheKey(locationA2.PK, receive2.PK), () => null));
		}

		public void TestGetLocationStockOnHandCache_SameDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, locationA1, "PLT2");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var locationStockOnHandCache =
				LocationStockOnHandInfoCacheManager.GetLocationStockOnHandCache(Factory, locationA1.PK, receive.PK);
			AssertNotNull("Cache should exist on factory",
				Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(
					GetStockOnHandLocationCacheKey(locationA1.PK, receive.PK), () => null));
			AssertEquals("Should return nothing as its the same docket.", 0, locationStockOnHandCache.Count());
		}

		public void TestGetLocationStockOnHandCache_InventoriesNotYetSavedInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, location);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			AssertEquals(0,
				LocationStockOnHandInfoCacheManager.GetLocationStockOnHandCache(Factory, location.PK, receive2.PK)
					.Count());
			AssertNotNull("Cache should exist on factory",
				Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(
					GetStockOnHandLocationCacheKey(location.PK, receive2.PK), () => null));
		}

		public void TestGetLocationStockOnHandCache_CacheClearedOnFactorySave()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, location);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var locationStockOnHandCache =
				LocationStockOnHandInfoCacheManager.GetLocationStockOnHandCache(Factory, location.PK, receive2.PK);
			AssertEquals(2, locationStockOnHandCache.Count());
			AssertNotNull("Cache should exist on factory",
				Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(
					GetStockOnHandLocationCacheKey(location.PK, receive2.PK), () => null));

			Factory.Save();
			AssertNull("Cache should not exist on factory after factory save",
				Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(
					GetStockOnHandLocationCacheKey(location.PK, receive2.PK), () => null));
		}

		static string GetStockOnHandLocationCacheKey(ZGuid locationPK, ZGuid docketPK)
		{
			return "LocationStockOnHandInfoCache|" + docketPK + "|" + locationPK;
		}
	}
}
