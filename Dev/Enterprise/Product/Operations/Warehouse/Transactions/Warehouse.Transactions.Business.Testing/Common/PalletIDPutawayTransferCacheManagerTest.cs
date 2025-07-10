namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class PalletIDPutawayTransferCacheManagerTest : WhsTestCaseWithFactory
	{
		public void TestPalletIDBoolValidationCache()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m,
				data.Whs1.DefaultInboundDockDoorLocation, "P123");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.CreateWhsTransferLine(putawayTransfer, data.Part1, 5m,
				data.Whs1.DefaultInboundDockDoorLocation.ToLocationString(), "P123",
				data.Whs1.DefaultLocation.ToLocationString(), "P123");
			var pickLine = Helper.CreateWhsPickLine(transferLine, receiveLine.Inventory[0], 5m);

			using (PalletIDPutawayTransferCacheManager.UseHasPutawayTransferCache(Factory))
			{
				AssertNotNull("Precondition: Cache should exist on factory",
					Factory.GetCachedValue<PalletIDPutawayTransferCacheManager>(
						nameof(PalletIDPutawayTransferCacheManager), () => null));

				var hasPTP123 = PalletIDPutawayTransferCacheManager.HasPutawayTransferForPallet(receive, "P123");
				AssertEquals("P123 record result is correct", true, hasPTP123);

				var hasPTP999 = PalletIDPutawayTransferCacheManager.HasPutawayTransferForPallet(receive, "P999");
				AssertEquals("Record P999 is correct", false, hasPTP999);

				pickLine.Delete();
				hasPTP123 = PalletIDPutawayTransferCacheManager.HasPutawayTransferForPallet(receive, "P123");
				AssertEquals("P123 record result is still from cache", true, hasPTP123);
			}

			var cache = Factory.GetCachedValue<PalletIDPutawayTransferCacheManager>(
				nameof(PalletIDPutawayTransferCacheManager), () => null);
			AssertNotNull("Cache object should still exist", cache);
			var hasPTP123NoCache = PalletIDPutawayTransferCacheManager.HasPutawayTransferForPallet(receive, "P123");
			AssertEquals("P123 record result is now changes", false, hasPTP123NoCache);
		}

		public void TestPalletIDBoolValidationCache_DifferentWhsSamePalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "A", 2, 2);

			var receiveWHS1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLineWHS1 = Helper.CreateWhsReceiveLine(receiveWHS1, data.Part1, 5m,
				data.Whs1.DefaultInboundDockDoorLocation, "P123");

			var receiveWHS2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2");
			var receiveLineWHS2 = Helper.CreateWhsReceiveLine(receiveWHS2, data.Part1, 15m,
				whs2.DefaultInboundDockDoorLocation, "P123");
			Factory.Save();

			var putawayTransferWHS1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransferWHS1.WD_IsPutawayTransfer = true;
			var transferLineWHS1 = Helper.CreateWhsTransferLine(putawayTransferWHS1, data.Part1, 5m,
				data.Whs1.DefaultInboundDockDoorLocation.ToLocationString(), "P123",
				data.Whs1.DefaultLocation.ToLocationString(), "P123");
			Helper.CreateWhsPickLine(transferLineWHS1, receiveLineWHS1.Inventory[0], 5m);

			using (PalletIDPutawayTransferCacheManager.UseHasPutawayTransferCache(Factory))
			{
				AssertNotNull("Precondition: Cache should exist on factory",
					Factory.GetCachedValue<PalletIDPutawayTransferCacheManager>(
						nameof(PalletIDPutawayTransferCacheManager), () => null));

				var hasPTWHS1P123 =
					PalletIDPutawayTransferCacheManager.HasPutawayTransferForPallet(receiveWHS1, "P123");
				AssertEquals("hasPTWHS1P123 record is correct", true, hasPTWHS1P123);

				var hasPTWhs2P123 =
					PalletIDPutawayTransferCacheManager.HasPutawayTransferForPallet(receiveWHS2, "P123");
				AssertEquals("hasPTWhs2P123 record is correct", false, hasPTWhs2P123);
			}
		}

		public void TestPalletIDBoolValidationCache_ValidatedBeforeCacheCreation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m,
				data.Whs1.DefaultInboundDockDoorLocation, "P123");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.CreateWhsTransferLine(putawayTransfer, data.Part1, 5m,
				data.Whs1.DefaultInboundDockDoorLocation.ToLocationString(), "P123",
				data.Whs1.DefaultLocation.ToLocationString(), "P123");
			Helper.CreateWhsPickLine(transferLine, receiveLine.Inventory[0], 5m);

			receiveLine.Validation.ValidateWE_PalletID();

			using (PalletIDPutawayTransferCacheManager.UseHasPutawayTransferCache(Factory))
			{
				AssertNotNull("Precondition: Cache should exist on factory",
					Factory.GetCachedValue<PalletIDPutawayTransferCacheManager>(
						nameof(PalletIDPutawayTransferCacheManager), () => null));

				var hasPTP123 = PalletIDPutawayTransferCacheManager.HasPutawayTransferForPallet(receive, "P123");
				AssertEquals("P123 record result is correct", true, hasPTP123);
			}
		}

		public void TestPalletIDBoolValidationCache_CacheOff()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "A", 2, 2);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m,
				data.Whs1.DefaultInboundDockDoorLocation, "P123");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.CreateWhsTransferLine(putawayTransfer, data.Part1, 5m,
				data.Whs1.DefaultInboundDockDoorLocation.ToLocationString(), "P123",
				data.Whs1.DefaultLocation.ToLocationString(), "P123");
			var pickLine = Helper.CreateWhsPickLine(transferLine, receiveLine.Inventory[0], 5m);

			var hasPTP123 = PalletIDPutawayTransferCacheManager.HasPutawayTransferForPallet(receive, "P123");
			AssertEquals("P123 record is correct", true, hasPTP123);

			var cache = Factory.GetCachedValue<PalletIDPutawayTransferCacheManager>(
				nameof(PalletIDPutawayTransferCacheManager), () => null);
			AssertNotNull("Cache object should still exist", cache);

			pickLine.Delete();
			hasPTP123 = PalletIDPutawayTransferCacheManager.HasPutawayTransferForPallet(receive, "P123");
			AssertEquals("P123 should be false", false, hasPTP123);
		}
	}
}
