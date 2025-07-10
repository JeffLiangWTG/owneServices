using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PendingOrdersCacheManagerTest : WhsTestCaseWithFactory
	{
		public void TestInventoryPendingOrdersCache()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 5m, data.Whs1.FindLocation("A-1-1"),
				string.Empty);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part2, 5m, data.Whs1.FindLocation("A-1-1"),
				string.Empty);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 3m);
			Factory.Save();

			using (PendingOrdersCacheManager.UseInventoryPendingOrdersCache(Factory))
			{
				AssertNotNull("Precondition: cacheManager should exist on factory",
					Factory.GetCachedValue<PendingOrdersCacheManager>(nameof(PendingOrdersCacheManager), () => null));

				var pendingOrdsR1 =
					PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part1.PK);
				AssertEquals("pendingOrdsR1 record result is correct", "1, 2", pendingOrdsR1);

				var pendingOrdsR2 =
					PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part2.PK);
				AssertEquals("Record pendingOrdsR2 is correct", string.Empty, pendingOrdsR2);

				orderLine1.Delete();
				orderLine2.Delete();
				Factory.Save();

				pendingOrdsR1 =
					PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part1.PK);
				AssertEquals("pendingOrdsR1 record result is still from cache", "1, 2", pendingOrdsR1);
			}

			var cacheManager =
				Factory.GetCachedValue<PendingOrdersCacheManager>(nameof(PendingOrdersCacheManager), () => null);
			AssertNotNull("cacheManager object should still exist", cacheManager);
			var pendingOrdsR1NoCache =
				PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part1.PK);
			AssertEquals("pendingOrdsR1 record result is now changes", string.Empty, pendingOrdsR1NoCache);
		}

		public void TestInventoryPendingOrdersCache_DifferentWhsSameClientProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "A", 2, 2);

			var receiveWHS1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLineWHS1 =
				Helper.CreateWhsReceiveLine(receiveWHS1, data.Part1, 5m, data.Whs1.DefaultLocation, "");

			var receiveWHS2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2");
			var receiveLineWHS2 = Helper.CreateWhsReceiveLine(receiveWHS2, data.Part1, 15m, whs2.DefaultLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "11");
			Helper.CreateWhsOrderLine(order1, data.Part1, 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, whs2, "23");
			Helper.CreateWhsOrderLine(order2, data.Part1, 3m);
			Factory.Save();

			using (PendingOrdersCacheManager.UseInventoryPendingOrdersCache(Factory))
			{
				AssertNotNull("Precondition: cacheManager should exist on factory",
					Factory.GetCachedValue<PendingOrdersCacheManager>(nameof(PendingOrdersCacheManager), () => null));

				var pendingOrdsWhs1 =
					PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part1.PK);
				AssertEquals("pendingOrdsWhs1 record is correct", "11", pendingOrdsWhs1);

				var pendingOrdsWhs2 =
					PendingOrdersCacheManager.GetPendingOrders(Factory, whs2.PK, data.Org1.PK, data.Part1.PK);
				AssertEquals("pendingOrdsWhs2 record is correct", "23", pendingOrdsWhs2);
			}
		}

		public void TestInventoryPendingOrdersCache_DifferentClientSameWhsProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org2 = Helper.CreateClient("org2 22");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			var receiveClient1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R541");
			var receiveLineClient1 =
				Helper.CreateWhsReceiveLine(receiveClient1, data.Part1, 5m, data.Whs1.DefaultLocation, "");

			var receiveClient2 = Helper.CreateWhsReceive(org2, data.Whs1, "Rgf52");
			var receiveLineClient2 =
				Helper.CreateWhsReceiveLine(receiveClient2, data.Part1, 15m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(org2, data.Whs1, "53");
			Helper.CreateWhsOrderLine(order1, data.Part1, 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "GET3");
			Helper.CreateWhsOrderLine(order2, data.Part1, 3m);
			Factory.Save();

			using (PendingOrdersCacheManager.UseInventoryPendingOrdersCache(Factory))
			{
				AssertNotNull("Precondition: cacheManager should exist on factory",
					Factory.GetCachedValue<PendingOrdersCacheManager>(nameof(PendingOrdersCacheManager), () => null));

				var pendingOrdsClient1 =
					PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part1.PK);
				AssertEquals("pendingOrdsClient1 record is correct", "GET3", pendingOrdsClient1);

				var pendingOrdsClient2 =
					PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, org2.PK, data.Part1.PK);
				AssertEquals("pendingOrdsClient2 record is correct", "53", pendingOrdsClient2);
			}
		}

		public void TestInventoryPendingOrdersCache_DifferentProductSameClientWhs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receiveP1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLineP1 = Helper.CreateWhsReceiveLine(receiveP1, data.Part1, 5m, data.Whs1.DefaultLocation, "");

			var receiveP2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLineP2 = Helper.CreateWhsReceiveLine(receiveP2, data.Part2, 15m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "11");
			Helper.CreateWhsOrderLine(order1, data.Part1, 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "23");
			Helper.CreateWhsOrderLine(order2, data.Part2, 3m);
			Factory.Save();

			using (PendingOrdersCacheManager.UseInventoryPendingOrdersCache(Factory))
			{
				AssertNotNull("Precondition: cacheManager should exist on factory",
					Factory.GetCachedValue<PendingOrdersCacheManager>(nameof(PendingOrdersCacheManager), () => null));

				var pendingOrdsP1 =
					PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part1.PK);
				AssertEquals("pendingOrdsP1 record is correct", "11", pendingOrdsP1);

				var pendingOrdsP2 =
					PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part2.PK);
				AssertEquals("pendingOrdsP2 record is correct", "23", pendingOrdsP2);
			}
		}

		public void TestInventoryPendingOrdersCache_ValidatedBeforeCacheCreation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "11");
			Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			Factory.Save();

			receiveLine.Validation.ValidateWE_WL();

			using (PendingOrdersCacheManager.UseInventoryPendingOrdersCache(Factory))
			{
				AssertNotNull("Precondition: cacheManager should exist on factory",
					Factory.GetCachedValue<PendingOrdersCacheManager>(nameof(PendingOrdersCacheManager), () => null));

				var pendingOrds =
					PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part1.PK);
				AssertEquals("pendingOrds record is correct", "11", pendingOrds);
			}
		}

		public void TestInventoryPendingOrdersCache_CacheOff()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "11");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			Factory.Save();

			var pendingOrds =
				PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part1.PK);
			AssertEquals("pendingOrds record is correct", "11", pendingOrds);

			var cacheManager =
				Factory.GetCachedValue<PendingOrdersCacheManager>(nameof(PendingOrdersCacheManager), () => null);
			AssertNotNull("Cache object should still exist", cacheManager);

			orderLine.Delete();
			Factory.Save();

			pendingOrds =
				PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part1.PK);
			AssertEquals("pendingOrds should be empty", string.Empty, pendingOrds);
		}

		public void TestInventoryPendingOrdersCache_WithProductsProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receiveP1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLineP1 = Helper.CreateWhsReceiveLine(receiveP1, data.Part1, 5m, data.Whs1.DefaultLocation, "");

			var receiveP2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLineP2 = Helper.CreateWhsReceiveLine(receiveP2, data.Part2, 15m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "11");
			Helper.CreateWhsOrderLine(order1, data.Part1, 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "23");
			Helper.CreateWhsOrderLine(order2, data.Part2, 3m);
			Factory.Save();

			AssertNotNull("Precondition: cacheManager should exist on factory",
				Factory.GetCachedValue<PendingOrdersCacheManager>(nameof(PendingOrdersCacheManager), () => null));
			using (PendingOrdersCacheManager.UseInventoryPendingOrdersCache(Factory,
					   () => new[] { data.Part1.PK, data.Part2.PK }))
			{
				var pendingOrdsP1 =
					PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part1.PK);
				AssertEquals("pendingOrdsP1 record is correct", "11", pendingOrdsP1);

				order2.Delete();
				Factory.Save();

				var pendingOrdsP2 =
					PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part2.PK);
				AssertEquals("Should not be empty as pending orders for Part2 got cached from 1st call.", "23",
					pendingOrdsP2);
			}

			var pendingOrdsP2AfterCacheClearing =
				PendingOrdersCacheManager.GetPendingOrders(Factory, data.Whs1.PK, data.Org1.PK, data.Part2.PK);
			AssertEquals("No more pending orders", string.Empty, pendingOrdsP2AfterCacheClearing);
		}

		public void TestInventoryPendingOrdersCache_WithProductsProvider_NewFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receiveP1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLineP1 = Helper.CreateWhsReceiveLine(receiveP1, data.Part1, 5m, data.Whs1.DefaultLocation, "");

			var receiveP2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLineP2 = Helper.CreateWhsReceiveLine(receiveP2, data.Part2, 15m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "11");
			Helper.CreateWhsOrderLine(order1, data.Part1, 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "23");
			Helper.CreateWhsOrderLine(order2, data.Part2, 3m);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			using (PendingOrdersCacheManager.UseInventoryPendingOrdersCache(newFactory,
					   () => new[] { data.Part1.PK, data.Part2.PK }))
			{
				AssertNotNull("Precondition: cacheManager should exist on factory",
					newFactory.GetCachedValue<PendingOrdersCacheManager>(nameof(PendingOrdersCacheManager),
						() => null));

				var pendingOrdsP1 =
					PendingOrdersCacheManager.GetPendingOrders(newFactory, data.Whs1.PK, data.Org1.PK, data.Part1.PK);
				AssertEquals("pendingOrdsP1 record is correct", "11", pendingOrdsP1);

				order2.Delete();
				Factory.Save();

				var pendingOrdsP2 =
					PendingOrdersCacheManager.GetPendingOrders(newFactory, data.Whs1.PK, data.Org1.PK, data.Part2.PK);
				AssertEquals("Should not be empty as pending orders for Part2 got cached from 1st call.", "23",
					pendingOrdsP2);
			}

			var pendingOrdsP2AfterCacheClearing =
				PendingOrdersCacheManager.GetPendingOrders(newFactory, data.Whs1.PK, data.Org1.PK, data.Part2.PK);
			AssertEquals("No more pending orders", string.Empty, pendingOrdsP2AfterCacheClearing);
		}
	}
}
