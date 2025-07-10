using System.Linq;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PalletIDLocationValidationCacheManagerTest : WhsTestCaseWithFactory
	{
		public void TestPalletIDLocationValidationCache()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, loc1, "P123");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, loc2, "P123");
			Factory.Save();

			using (PalletIDLocationValidationCacheManager.UseLocationCache(Factory))
			{
				AssertNotNull("Precondition: Cache should exist on factory",
					Factory.GetCachedValue<PalletIDLocationValidationCacheManager>(
						nameof(PalletIDLocationValidationCacheManager), () => null));

				var locationsP123 =
					PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
						"P123");
				AssertNotNull("P123 returned value", locationsP123);
				AssertContainsExactElementsInAnyOrder("P123 record locations are correct", new[] { loc1, loc2 },
					locationsP123);

				var locationsP999 =
					PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
						"P999");
				AssertNotNull("Record P999 found", locationsP999);
				AssertEquals("Record P999 is empty", 0, locationsP999.Count());

				receiveLine1.Delete();
				receiveLine2.Delete();
				Factory.Save();

				locationsP123 =
					PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
						"P123");
				AssertNotNull("P123 returned value still correct", locationsP123);
				AssertContainsExactElementsInAnyOrder("P123 record locations are still there",
					Enumerable.Empty<WhsLocation>(), locationsP123);
			}

			var cache = Factory.GetCachedValue<PalletIDLocationValidationCacheManager>(
				nameof(PalletIDLocationValidationCacheManager), () => null);
			AssertNotNull("Cache object should still exist", cache);
			var locationsP123AfterCache =
				PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
					"P123");
			AssertEquals("Cache should be empty", 0, locationsP123AfterCache.Count());
		}

		public void TestPalletIDLocationValidationCache_DifferentWhsSamePalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc = data.Whs1.FindLocation("A-1");
			var whs2 = Helper.CreateWarehouse("WHS2", "A", 2, 2);
			var whs2loc = whs2.DefaultLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m, loc, "P123");

			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2", Notify);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, whs2loc, "P123");
			Factory.Save();

			using (PalletIDLocationValidationCacheManager.UseLocationCache(Factory))
			{
				AssertNotNull("Precondition: Cache should exist on factory",
					Factory.GetCachedValue<PalletIDLocationValidationCacheManager>(
						nameof(PalletIDLocationValidationCacheManager), () => null));

				var locationsP123 =
					PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
						"P123");
				AssertNotNull("P123 returned value", locationsP123);
				AssertContainsExactElementsInAnyOrder("P123 record locations are correct", new[] { loc },
					locationsP123);

				var locationsWhs2P123 =
					PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, whs2.PK,
						"P123");
				AssertNotNull("P123 returned value", locationsWhs2P123);
				AssertContainsExactElementsInAnyOrder("P123 record locations are correct", new[] { whs2loc },
					locationsWhs2P123);
			}
		}

		public void TestPalletIDLocationValidationCache_ValidatedBeforeCacheCreation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc = data.Whs1.FindLocation("A-1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, loc, "P123");
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 5m, null, "Pallet2");
			WhsValidationHelper.CheckPalletID(receiveLine2.WE_PalletIDInfo, receiveLine2.WE_WL, receive2, receiveLine2,
				receiveLine2.Inventory[0].ValidationPalletIDWarningMessage);

			using (PalletIDLocationValidationCacheManager.UseLocationCache(Factory))
			{
				AssertNotNull("Precondition: Cache should exist on factory",
					Factory.GetCachedValue<PalletIDLocationValidationCacheManager>(
						nameof(PalletIDLocationValidationCacheManager), () => null));

				var locationsP123 =
					PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
						"P123");
				AssertNotNull("P123 returned value", locationsP123);
				AssertContainsExactElementsInAnyOrder("P123 record locations are correct", new[] { loc },
					locationsP123);
			}
		}

		public void TestPalletIDLocationValidationCache_CacheOff()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m, loc1, "P123");
			Factory.Save();

			var locationsP123 =
				PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
					"P123");
			AssertNotNull("P123 returned value", locationsP123);
			AssertContainsExactElementsInAnyOrder("P123 record locations are correct", new[] { loc1 }, locationsP123);

			var cache = Factory.GetCachedValue<PalletIDLocationValidationCacheManager>(
				nameof(PalletIDLocationValidationCacheManager), () => null);
			AssertNotNull("Cache object should still exist", cache);

			receiveLine1.WE_WL = loc2.PK;
			locationsP123 =
				PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
					"P123");
			AssertNotNull("P123 returned value still correct", locationsP123);
			AssertContainsExactElementsInAnyOrder("P123 record locations are correct", new[] { loc2 }, locationsP123);
		}

		public void TestGetFirstLocationWithStockIncludingNotYetFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var loc2 = data.Whs1.FindLocation("A-2");
			var loc3 = data.Whs1.FindLocation("A-3");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 0m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc2, "PLT-2");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc3, "PLT-3");
			Factory.Save(); // to create docket lines

			AssertContainsExactElementsInAnyOrder(
				"When stock is not finalised and it has qty = 0, then system should not find the location as the one with stock.",
				Enumerable.Empty<WhsLocation>(),
				PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
					"PLT-1"));

			AssertContainsExactElementsInAnyOrder(
				"When stock is not finalised and it has qty != 0, then system should find the location as the one with stock.",
				new[] { loc2 },
				PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
					"PLT-2"));

			AssertContainsExactElementsInAnyOrder(
				"When stock is not finalised and it has qty != 0, then system should not find the location as the one with stock.",
				new[] { loc3 },
				PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
					"PLT-3"));

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			AssertContainsExactElementsInAnyOrder(
				"When stock is finalised and it has qty = 0, then system should not find the location as the one with stock.",
				Enumerable.Empty<WhsLocation>(),
				PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
					"PLT-1"));

			AssertContainsExactElementsInAnyOrder(
				"When stock is finalised and it has qty != 0, then system should find the location as the one with stock.",
				new[] { loc2 },
				PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
					"PLT-2"));

			AssertContainsExactElementsInAnyOrder(
				"When stock is finalised and it has qty != 0, then system should find the location as the one with stock.",
				new[] { loc3 },
				PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(Factory, data.Whs1.PK,
					"PLT-3"));
		}
	}
}
